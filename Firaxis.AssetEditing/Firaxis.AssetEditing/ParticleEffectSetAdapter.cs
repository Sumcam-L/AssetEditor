using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DatabaseWrapper;
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

public class ParticleEffectSetAdapter : AssetComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum Command
	{
		NewParticle,
		AddParticle,
		OpenParticle,
		PreviewEntrySource,
		ReimportParticle,
		RemoveParticle
	}

	private struct ParticleEffectCommandTag
	{
		public readonly Command Command;

		private Action<ParticleEffectSetAdapter> m_action;

		public ParticleEffectCommandTag(Command command, Action<ParticleEffectSetAdapter> action)
		{
			Command = command;
			m_action = action;
		}

		public void DoCommand(ParticleEffectSetAdapter adapter)
		{
			m_action(adapter);
		}
	}

	private static ParticleEffectCommandTag AddParticleCommandTag = new ParticleEffectCommandTag(Command.AddParticle, delegate(ParticleEffectSetAdapter adapter)
	{
		AssetContext assetContext = adapter.DomNode.GetRoot().As<AssetContext>();
		if (!(assetContext.CivTechService.PrimaryProject.Config.Classes.FindForInstance(adapter.AssetAdapter.Asset) is IAssetClass assetClass))
		{
			MessageBox.Show("The asset class is no longer valid. Can not add particle to this asset!", "Failed to add particle", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		else
		{
			Dictionary<string, InstanceType> entities = new Dictionary<string, InstanceType>();
			assetContext.AssetBrowserService.OpenEntities(entities, new InstanceType[1] { InstanceType.IT_PARTICLE_EFFECT }, assetClass.AllowedParticleEffectClasses);
			if (entities.Count > 0)
			{
				AddUndoOperation("Add Particle", adapter, delegate
				{
					foreach (KeyValuePair<string, InstanceType> item in entities)
					{
						adapter.AddEffect(item.Key);
					}
				});
			}
		}
	});

	private static ParticleEffectCommandTag NewParticleCommandTag = new ParticleEffectCommandTag(Command.NewParticle, delegate(ParticleEffectSetAdapter adapter)
	{
		AssetContext assetContext = adapter.DomNode.GetRoot().As<AssetContext>();
		if (!(assetContext.CivTechService.PrimaryProject.Config.Classes.FindForInstance(adapter.AssetAdapter.Asset) is IAssetClass assetClass))
		{
			MessageBox.Show("The asset class is no longer valid. Can not add particle to this asset!", "Failed to add particle", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		using IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { assetContext.CivTechService.GetActivePantryPaths() });
		IEnumerable<EntityID> entities = SetAdapterHelper.LaunchMiniImporter(assetContext.CivTechService, assetContext.FileWatchService, entitySet, assetContext.EntityCacheService, assetClass.AllowedParticleEffectClasses, InstanceType.IT_PARTICLE_EFFECT);
		if (entities.Any())
		{
			AddUndoOperation("New Particle", adapter, delegate
			{
				foreach (EntityID item2 in entities.Where((EntityID id) => id.Type == InstanceType.IT_PARTICLE_EFFECT))
				{
					adapter.AddEffect(item2.Name);
				}
			});
		}
	});

	private static ParticleEffectCommandTag OpenParticleCommandTag = new ParticleEffectCommandTag(Command.OpenParticle, delegate(ParticleEffectSetAdapter adapter)
	{
		AssetContext assetContext = adapter.DomNode.GetRoot().As<AssetContext>();
		foreach (object item3 in adapter.Selection)
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = item3.As<ParticleEffectReferenceAdapter>();
			assetContext.AssetDocumentCommands.OpenExistingDocument(InstanceType.IT_PARTICLE_EFFECT, particleEffectReferenceAdapter.Name);
		}
	});

	private static ParticleEffectCommandTag OpenParticleSourceCommandTag = new ParticleEffectCommandTag(Command.PreviewEntrySource, delegate(ParticleEffectSetAdapter adapter)
	{
		AssetContext assetContext = adapter.DomNode.GetRoot().As<AssetContext>();
		_ = assetContext.CivTechService.PrimaryProject.Paths.GamePantry;
		foreach (object item4 in adapter.Selection)
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = item4.As<ParticleEffectReferenceAdapter>();
			using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { assetContext.CivTechService.GetActivePantryPaths() });
			IParticleEffectInstance particleEffectInstance = instanceSet.LoadEntity<IParticleEffectInstance>(assetContext.CivTechService.ProjectMapService.LayeredPantry, particleEffectReferenceAdapter.Name);
			if (particleEffectInstance != null)
			{
				OpenEntitySourceFileForPreview(assetContext.CivTechService, particleEffectInstance);
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Could not load the instance entity with the name {0} and the type {1} for the purpose of getting its children to preview sources with.", particleEffectReferenceAdapter.Name, EnumToStringConverter.GetNameFromType(InstanceType.IT_PARTICLE_EFFECT));
			}
		}
	});

	private static ParticleEffectCommandTag ReimportParticleCommandTag = new ParticleEffectCommandTag(Command.ReimportParticle, delegate(ParticleEffectSetAdapter adapter)
	{
		IEnumerable<string> selectedParticleEffects = GetSelectedParticleEffects(adapter);
		if (selectedParticleEffects.Any())
		{
			IEntityDocument entityDocument = adapter.DomNode.GetRoot().As<IEntityDocument>();
			BaseEntityPropertyContext entityContext = adapter.DomNode.GetRoot().As<BaseEntityPropertyContext>();
			IEnumerable<EntityID> entityIDs = SetAdapterHelper.GetSelectedEntityIDs(selectedParticleEffects, InstanceType.IT_PARTICLE_EFFECT);
			using (entityContext.SuspendRecording())
			{
				entityContext.DoTransaction(delegate
				{
					entityContext.BatchChangelist?.AddGenericEntityChangedEvents(entityIDs);
					SetAdapterHelper.ImportSelectedEntities(entityIDs, entityContext, recurseIntoChildren: false);
				}, "Reimporting particle effects.");
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

	private static ParticleEffectCommandTag RemoveParticleCommandTag = new ParticleEffectCommandTag(Command.RemoveParticle, delegate(ParticleEffectSetAdapter adapter)
	{
		AddUndoOperation("Remove Particle", adapter, delegate
		{
			object[] array = adapter.Selection.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				ParticleEffectReferenceAdapter particleEffectReferenceAdapter = array[i].As<ParticleEffectReferenceAdapter>();
				adapter.RemoveEffect(particleEffectReferenceAdapter.Name);
			}
		});
	});

	private ISelectionContext m_selectionContext = new SelectionContext();

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	public IList<ParticleEffectReferenceAdapter> ParticleEffects { get; private set; }

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

	public IEnumerable<object> Items => ParticleEffects;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (object item in EntitySchema.ParticleEffectReferenceType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

	public bool InTransaction => base.AssetAdapter.As<ITransactionContext>().InTransaction;

	public int PendingOperationCount => base.AssetAdapter.As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	private static void AddUndoOperation(string name, ParticleEffectSetAdapter adapter, Action action)
	{
		adapter.DomNode.GetRoot().As<AssetContext>().DoTransaction(action, name.Localize());
	}

	private static IEnumerable<string> GetSelectedParticleEffects(ParticleEffectSetAdapter adapter)
	{
		List<string> list = new List<string>(adapter.Selection.Count());
		foreach (object item in adapter.Selection)
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = item.As<ParticleEffectReferenceAdapter>();
			if (particleEffectReferenceAdapter != null && !string.IsNullOrEmpty(particleEffectReferenceAdapter.Name))
			{
				list.Add(particleEffectReferenceAdapter.Name);
			}
		}
		return list;
	}

	public ParticleEffectSetAdapter()
	{
		m_commands.Add(new CommandInfo(NewParticleCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add New".Localize("Name of a command"), "Creates new particle effect.".Localize(), Sce.Atf.Input.Keys.None, Firaxis.ATF.Resources.AddNewEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(AddParticleCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add Existing".Localize("Name of a command"), "Adds a particle effect to the particle effect set.".Localize(), Sce.Atf.Input.Keys.None, Firaxis.ATF.Resources.AddExistingEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(RemoveParticleCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Remove".Localize("Name of a command"), "Removes a particle effect from the particle effect set.".Localize(), Sce.Atf.Input.Keys.None, Resources.RemoveItemIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ReimportParticleCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Reimport".Localize("Name of a command"), "Reimports a particle effect that is in the particle effect set.".Localize(), Sce.Atf.Input.Keys.None, Resources.ReimportFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenParticleCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open".Localize("Name of a command"), "Opens the documents of the selected particle effect.".Localize(), Sce.Atf.Input.Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenParticleSourceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open Source File".Localize("Name of a command"), "Opens the selected particle effect's Source File.".Localize(), Sce.Atf.Input.Keys.None, Resources.OpenSourceFileIcon, CommandVisibility.All));
		if (this.SelectionChanging != null && this.SelectionChanged != null && this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RegisterForDomChanges();
		ParticleEffects = new DomNodeListAdapter<ParticleEffectReferenceAdapter>(base.DomNode, EntitySchema.ParticleEffectSetType.ParticleEffectChild);
	}

	private void RegisterForDomChanges()
	{
		UnregisterFromDomChanges();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void UnregisterFromDomChanges()
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	public void AddEffect(string name)
	{
		DomNode domNode = new DomNode(EntitySchema.ParticleEffectReferenceType.Type);
		domNode.InitializeExtensions();
		ParticleEffectReferenceAdapter particleEffectReferenceAdapter = domNode.As<ParticleEffectReferenceAdapter>();
		particleEffectReferenceAdapter.Name = name;
		ParticleEffects.Add(particleEffectReferenceAdapter);
	}

	public void RemoveEffect(string name)
	{
		ParticleEffectReferenceAdapter item = ParticleEffects.FirstOrDefault((ParticleEffectReferenceAdapter pera) => pera.Name == name);
		ParticleEffects.Remove(item);
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.ParticleEffectReferenceType.NameAttribute)
		{
			string particleName = (string)e.OldValue;
			string text = (string)e.NewValue;
			base.AssetAdapter.Asset.RemoveParticleEffect(particleName);
			base.BatchChangelist?.CreateEntityChangedEvent(base.AssetAdapter.Asset.Type, base.AssetAdapter.Asset.Name);
			base.AssetAdapter.Asset.AddParticleEffect(text);
			base.BatchChangelist?.CreateParticleEffectAddedEvent(base.AssetAdapter.Asset, text);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == EntitySchema.ParticleEffectReferenceType.Type)
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = e.Child.As<ParticleEffectReferenceAdapter>();
			base.AssetAdapter.Asset.AddParticleEffect(particleEffectReferenceAdapter.Name);
			base.BatchChangelist?.CreateParticleEffectAddedEvent(base.AssetAdapter.Asset, particleEffectReferenceAdapter.Name);
		}
		OnItemInserted(e.Index, e.Child);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == EntitySchema.ParticleEffectReferenceType.Type)
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = e.Child.As<ParticleEffectReferenceAdapter>();
			base.AssetAdapter.Asset.RemoveParticleEffect(particleEffectReferenceAdapter.Name);
			base.BatchChangelist?.CreateEntityChangedEvent(base.AssetAdapter.Asset.Type, base.AssetAdapter.Asset.Name);
		}
		OnItemRemoved(e.Index, e.Child);
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		IList<ParticleEffectReferenceAdapter> list = new List<ParticleEffectReferenceAdapter>();
		foreach (string partEff in base.AssetAdapter.Asset.GetParticleEffects())
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = ParticleEffects.FirstOrDefault((ParticleEffectReferenceAdapter utl) => utl.Name == partEff);
			if (particleEffectReferenceAdapter == null)
			{
				DomNode domNode = new DomNode(EntitySchema.ParticleEffectReferenceType.Type);
				domNode.InitializeExtensions();
				particleEffectReferenceAdapter = domNode.As<ParticleEffectReferenceAdapter>();
				particleEffectReferenceAdapter.Name = partEff;
				ParticleEffects.Add(particleEffectReferenceAdapter);
			}
			list.Add(particleEffectReferenceAdapter);
		}
		foreach (var entryAdapter in ParticleEffects.Except(list).ToArray())
		{
			ParticleEffects.Remove(entryAdapter);
		}
		RegisterForDomChanges();
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
		if (base.AssetAdapter.Asset == null || string.IsNullOrEmpty(base.AssetAdapter.Asset.ClassName))
		{
			return false;
		}
		ParticleEffectCommandTag particleEffectCommandTag = (ParticleEffectCommandTag)commandTag;
		if (base.AssetAdapter.As<IEntityDocument>().IsReadOnly && particleEffectCommandTag.Command != Command.OpenParticle && particleEffectCommandTag.Command != Command.PreviewEntrySource)
		{
			return false;
		}
		if (particleEffectCommandTag.Command == Command.AddParticle || particleEffectCommandTag.Command == Command.NewParticle)
		{
			if (base.AssetAdapter.Asset.ClassName == "FireFX")
			{
				return !base.AssetAdapter.ParticleEffectsSet.Items.Any();
			}
			return true;
		}
		if (particleEffectCommandTag.Command == Command.RemoveParticle || particleEffectCommandTag.Command == Command.OpenParticle || particleEffectCommandTag.Command == Command.PreviewEntrySource)
		{
			return Selection.Any();
		}
		if (particleEffectCommandTag.Command == Command.ReimportParticle)
		{
			if (Selection.Any())
			{
				return CanReimportSelection();
			}
			return false;
		}
		return false;
	}

	private bool CanReimportSelection()
	{
		ICivTechService civTechService = base.AssetAdapter.CivTechService;
		foreach (object item in Selection)
		{
			ParticleEffectReferenceAdapter particleEffectReferenceAdapter = item.As<ParticleEffectReferenceAdapter>();
			string entityPath = civTechService.GetEntityPath(particleEffectReferenceAdapter.Name, InstanceType.IT_PARTICLE_EFFECT);
			if (string.IsNullOrEmpty(entityPath))
			{
				return false;
			}
			Uri uri = null;
			try
			{
				uri = new Uri(entityPath);
			}
			catch (System.Exception)
			{
				return false;
			}
			if (!civTechService.IsFromActiveProjectOrDependencies(uri))
			{
				return false;
			}
		}
		return true;
	}

	public void DoCommand(object commandTag)
	{
		base.AssetAdapter.As<AssetContext>();
		((ParticleEffectCommandTag)commandTag).DoCommand(this);
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
		base.AssetAdapter.As<ITransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.AssetAdapter.As<ITransactionContext>().Cancel();
	}

	public void End()
	{
		base.AssetAdapter.As<ITransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.AssetAdapter.As<ITransactionContext>().SuspendTransactions();
	}

	private static void OpenEntitySourceFileForPreview(ICivTechService civTechSvc, IImportedEntity entity)
	{
		CivTechHelper.OpenSourceFile(civTechSvc, entity);
	}
}
