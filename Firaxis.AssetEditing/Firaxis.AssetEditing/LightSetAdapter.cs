using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class LightSetAdapter : LightRigComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum Command
	{
		NewLight,
		AddLight,
		OpenLight,
		ReimportLight,
		RemoveLight
	}

	private struct LightCommandTag
	{
		public readonly Command Command;

		private Action<LightSetAdapter> m_action;

		public LightCommandTag(Command command, Action<LightSetAdapter> action)
		{
			Command = command;
			m_action = action;
		}

		public void DoCommand(LightSetAdapter adapter)
		{
			m_action(adapter);
		}
	}

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	private ISelectionContext m_selectionContext = new SelectionContext();

	private static LightCommandTag m_addLightCommandTag = new LightCommandTag(Command.AddLight, delegate(LightSetAdapter adapter)
	{
		LightRigContext lightRigContext = adapter.DomNode.GetRoot().As<LightRigContext>();
		if (!(lightRigContext.CivTechService.PrimaryProject.Config.Classes.FindForInstance(adapter.LightRigAdapter.LightRig) is ILightRigClass lightRigClass))
		{
			MessageBox.Show("The light rig class is no longer valid. Can not add a light to this light rig!", "Failed to add light rig", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		else
		{
			Dictionary<string, InstanceType> entities = new Dictionary<string, InstanceType>();
			lightRigContext.AssetBrowserService.OpenEntities(entities, new InstanceType[1] { adapter.LightType }, lightRigClass.AllowedLightClasses);
			if (entities.Count > 0)
			{
				AddUndoOperation("Add Light", adapter, delegate
				{
					foreach (KeyValuePair<string, InstanceType> item in entities)
					{
						bool flag = false;
						IInstanceEntity instanceEntity = adapter.EntityAdapter.Instances.LoadEntityByName(item.Key, item.Value);
						if (instanceEntity != null)
						{
							flag = adapter.LightRigAdapter.LightRig.AddEntity(instanceEntity, adapter.LightType);
						}
						if (!flag)
						{
							Outputs.Write(OutputMessageType.Error, "Failed to load light entity " + item.Key + " when attempting to add to light rig " + adapter.EntityAdapter.Name);
						}
					}
				});
			}
		}
	});

	private static LightCommandTag m_newLightCommandTag = new LightCommandTag(Command.NewLight, delegate(LightSetAdapter adapter)
	{
		LightRigContext context = adapter.DomNode.GetRoot().As<LightRigContext>();
		_ = context.CivTechService.PrimaryProject.Paths.GamePantry;
		ILightRigInstance lightRig = adapter.LightRigAdapter.LightRig;
		ILightRigClass lightRigClass = context.CivTechService.PrimaryProject.Config.Classes.FindForInstance(lightRig) as ILightRigClass;
		IInstanceSet instSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { context.CivTechService.GetActivePantryPaths() });
		try
		{
			IEnumerable<EntityID> exportedEntities = SetAdapterHelper.LaunchMiniImporter(context.CivTechService, context.FileWatchService, instSet, context.EntityCacheService, lightRigClass.AllowedLightClasses, adapter.LightType);
			if (exportedEntities.Any())
			{
				AddUndoOperation("New Light", adapter, delegate
				{
					foreach (EntityID item2 in exportedEntities)
					{
						IInstanceEntity instanceEntity = instSet.LoadEntityIfUnique(item2.Name, item2.Type);
						if (instanceEntity != null)
						{
							adapter.LightRigAdapter.LightRig.AddEntity(instanceEntity, item2.Type);
						}
					}
				});
			}
		}
		finally
		{
			if (instSet != null)
			{
				instSet.Dispose();
			}
		}
	});

	private static LightCommandTag m_openLightCommandTag = new LightCommandTag(Command.OpenLight, delegate(LightSetAdapter adapter)
	{
		LightRigContext lightRigContext = adapter.DomNode.GetRoot().As<LightRigContext>();
		foreach (object item3 in adapter.Selection)
		{
			LightReferenceAdapter lightReferenceAdapter = item3.As<LightReferenceAdapter>();
			lightRigContext.AssetDocumentCommands.OpenExistingDocument(adapter.LightType, lightReferenceAdapter.Name);
		}
	});

	private static LightCommandTag m_reimportLightCommandTag = new LightCommandTag(Command.ReimportLight, delegate(LightSetAdapter adapter)
	{
		IEnumerable<string> selectedLights = GetSelectedLights(adapter);
		if (selectedLights.Any())
		{
			IEntityDocument entityDocument = adapter.DomNode.GetRoot().As<IEntityDocument>();
			BaseEntityPropertyContext entityContext = adapter.DomNode.GetRoot().As<BaseEntityPropertyContext>();
			IEnumerable<EntityID> entityIDs = SetAdapterHelper.GetSelectedEntityIDs(selectedLights, adapter.LightType);
			using (entityContext.SuspendRecording())
			{
				entityContext.DoTransaction(delegate
				{
					entityContext.BatchChangelist?.AddGenericEntityChangedEvents(entityIDs);
					SetAdapterHelper.ImportSelectedEntities(entityIDs, entityContext, recurseIntoChildren: false);
				}, "Reimporting lights.");
			}
			entityContext.AssetDocumentCommands.OpenExistingDocument(entityDocument.InstanceEntity.Type, entityDocument.InstanceEntity.Name);
			if (entityContext.HotLoadOnReimport)
			{
				List<Uri> list = new List<Uri> { entityDocument.Uri };
				list.AddRange(StaticMethods.GetEntityURIs(entityContext.CivTechService, entityIDs));
				entityContext.TunerQueueService?.AddFilesToQueue(list);
			}
		}
	});

	private static LightCommandTag m_removeLightCommandTag = new LightCommandTag(Command.RemoveLight, delegate(LightSetAdapter adapter)
	{
		AddUndoOperation("Remove Light", adapter, delegate
		{
			BaseEntityPropertyContext baseEntityPropertyContext = adapter.DomNode.GetRoot().As<BaseEntityPropertyContext>();
			using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { baseEntityPropertyContext.CivTechService.GetActivePantryPaths() });
			foreach (object item4 in adapter.Selection)
			{
				LightReferenceAdapter lightReferenceAdapter = item4.As<LightReferenceAdapter>();
				IInstanceEntity entity = instanceSet.LoadEntityByName(lightReferenceAdapter.Name, adapter.LightType);
				if (instanceSet != null)
				{
					adapter.LightRigAdapter.LightRig.RemoveEntity(entity, adapter.LightType);
				}
			}
		});
	});

	public IList<LightReferenceAdapter> Lights { get; private set; }

	public InstanceType LightType { get; set; }

	public ICommandClient CommandClient => this;

	public IEnumerable<CommandInfo> Commands => m_commands;

	public string DefaultSortPropertyName
	{
		get
		{
			System.ComponentModel.PropertyDescriptor propertyDescriptor = PropertyDescriptors.FirstOrDefault();
			if (propertyDescriptor == null)
			{
				return string.Empty;
			}
			return propertyDescriptor.Name;
		}
	}

	public ListSortDirection DefaultListSortDirection => ListSortDirection.Ascending;

	public IEnumerable<object> Selection
	{
		get
		{
			return m_selectionContext.Selection;
		}
		set
		{
			m_selectionContext.Selection = value;
		}
	}

	public object LastSelected => m_selectionContext.LastSelected;

	public int SelectionCount => m_selectionContext.SelectionCount;

	public IEnumerable<object> Items => Lights;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (object item in EntitySchema.LightReferenceType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

	public bool InTransaction => base.EntityAdapter.As<LightRigContext>().InTransaction;

	public int PendingOperationCount => base.EntityAdapter.As<LightRigContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public LightSetAdapter()
	{
		m_commands.Add(new CommandInfo(m_newLightCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add New".Localize("Name of a command"), "Creates new particle effect.".Localize(), Sce.Atf.Input.Keys.None, Resources.AddNewIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_addLightCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add Existing".Localize("Name of a command"), "Adds a light to the light set.".Localize(), Sce.Atf.Input.Keys.None, Resources.AddExistingIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_removeLightCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Remove".Localize("Name of a command"), "Removes a light from the light set.".Localize(), Sce.Atf.Input.Keys.None, Sce.Atf.Resources.RemoveImage, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_reimportLightCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Reimport".Localize("Name of a command"), "Reimports a light that is in the light set.".Localize(), Sce.Atf.Input.Keys.None, "file_refresh", CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_openLightCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open".Localize("Name of a command"), "Open the light as a new document in the editor.".Localize(), Sce.Atf.Input.Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		if (this.SelectionChanging != null && this.SelectionChanged != null && this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	public IEnumerable<T> GetSelection<T>() where T : class
	{
		return m_selectionContext.GetSelection<T>();
	}

	public T GetLastSelected<T>() where T : class
	{
		return m_selectionContext.GetLastSelected<T>();
	}

	public bool SelectionContains(object item)
	{
		return m_selectionContext.SelectionContains(item);
	}

	public bool CanDoCommand(object commandTag)
	{
		if (base.EntityAdapter.InstanceEntity == null || string.IsNullOrEmpty(base.EntityAdapter.InstanceEntity.ClassName))
		{
			return false;
		}
		LightCommandTag lightCommandTag = (LightCommandTag)commandTag;
		if (base.EntityAdapter.As<IEntityDocument>().IsReadOnly && lightCommandTag.Command != Command.OpenLight)
		{
			return false;
		}
		if (lightCommandTag.Command == Command.AddLight || lightCommandTag.Command == Command.NewLight)
		{
			return true;
		}
		if (lightCommandTag.Command == Command.RemoveLight || lightCommandTag.Command == Command.ReimportLight || lightCommandTag.Command == Command.OpenLight)
		{
			return Selection.Any();
		}
		return false;
	}

	public void DoCommand(object commandTag)
	{
		base.EntityAdapter.As<LightRigContext>();
		((LightCommandTag)commandTag).DoCommand(this);
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(index, item));
	}

	protected virtual void OnItemChanged(object item)
	{
		this.ItemChanged?.Invoke(this, new ItemChangedEventArgs<object>(item));
	}

	protected virtual void OnReloaded()
	{
		this.Reloaded?.Invoke(this, EventArgs.Empty);
	}

	public void Begin(string transactionName)
	{
		base.EntityAdapter.As<LightRigContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.EntityAdapter.As<LightRigContext>().Cancel();
	}

	public void End()
	{
		base.EntityAdapter.As<LightRigContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.EntityAdapter.As<LightRigContext>().SuspendTransactions();
	}

	public void Update(IEnumerable<ILightReference> lightReferences)
	{
		IList<LightReferenceAdapter> list = new List<LightReferenceAdapter>();
		foreach (ILightReference lightRef in lightReferences)
		{
			LightReferenceAdapter lightReferenceAdapter = Lights.FirstOrDefault((LightReferenceAdapter utl) => utl.Name == lightRef.LightName);
			if (lightReferenceAdapter == null)
			{
				DomNode domNode = new DomNode(EntitySchema.LightReferenceType.Type);
				domNode.InitializeExtensions();
				lightReferenceAdapter = domNode.As<LightReferenceAdapter>();
				lightReferenceAdapter.Name = lightRef.LightName;
				Lights.Add(lightReferenceAdapter);
			}
			list.Add(lightReferenceAdapter);
		}
		foreach (var entryAdapter in Lights.Except(list).ToArray())
		{
			Lights.Remove(entryAdapter);
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		Lights = new DomNodeListAdapter<LightReferenceAdapter>(base.DomNode, EntitySchema.LightSetType.LightChild);
	}

	private static void AddUndoOperation(string name, LightSetAdapter adapter, Action action)
	{
		adapter.DomNode.GetRoot().As<LightRigContext>().DoTransaction(action, name.Localize());
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		OnItemInserted(e.Index, e.Child);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		OnItemRemoved(e.Index, e.Child);
	}

	private static IEnumerable<string> GetSelectedLights(LightSetAdapter lightSetAdapter)
	{
		List<string> list = new List<string>(lightSetAdapter.Selection.Count());
		foreach (object item in lightSetAdapter.Selection)
		{
			LightReferenceAdapter lightReferenceAdapter = item.As<LightReferenceAdapter>();
			if (item != null && !string.IsNullOrEmpty(lightReferenceAdapter.Name))
			{
				list.Add(lightReferenceAdapter.Name);
			}
		}
		return list;
	}
}
