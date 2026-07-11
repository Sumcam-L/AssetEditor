using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

public class BehaviorSetAdapter : BehaviorComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum Command
	{
		AddBehavior,
		RemoveBehavior,
		OpenBehavior,
		MoveBehaviorsUp,
		MoveBehaviorsDown,
		ExportBehavior,
		FlattenBehaviors,
		ClearBehaviors
	}

	private struct BehaviorCommandTag
	{
		public readonly Command Command;

		public BehaviorCommandTag(Command command)
		{
			Command = command;
		}
	}

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	private ISelectionContext m_selectionContext = new SelectionContext();

	private static BehaviorCommandTag m_addBehaviorCommandTag = new BehaviorCommandTag(Command.AddBehavior);

	private static BehaviorCommandTag m_clearBehaviorsCommandTag = new BehaviorCommandTag(Command.ClearBehaviors);

	private static BehaviorCommandTag m_exportBehaviorCommandTag = new BehaviorCommandTag(Command.ExportBehavior);

	private static BehaviorCommandTag m_flattenBehaviorsCommandTag = new BehaviorCommandTag(Command.FlattenBehaviors);

	private static BehaviorCommandTag m_moveBehaviorsUpCommandTag = new BehaviorCommandTag(Command.MoveBehaviorsUp);

	private static BehaviorCommandTag m_moveBehaviorsDownCommandTag = new BehaviorCommandTag(Command.MoveBehaviorsDown);

	private static BehaviorCommandTag m_openBehaviorsCommandTag = new BehaviorCommandTag(Command.OpenBehavior);

	private static BehaviorCommandTag m_removeBehaviorCommandTag = new BehaviorCommandTag(Command.RemoveBehavior);

	public IList<BehaviorReferenceAdapter> Behaviors { get; private set; }

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

	public IEnumerable<object> Items => Behaviors;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (object item in EntitySchema.BehaviorReferenceType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

	public bool InTransaction => base.DomNode.Parent.As<TransactionContext>().InTransaction;

	public int PendingOperationCount => base.DomNode.GetRoot().As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public BehaviorSetAdapter()
	{
		m_commands.Add(new CommandInfo(m_addBehaviorCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add Existing".Localize("Name of a command"), "Adds the selected behaviors to the entity.".Localize(), Keys.None, Firaxis.ATF.Resources.AddExistingEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_removeBehaviorCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Remove".Localize("Name of a command"), "Removes the selected behaviors from the entity.".Localize(), Keys.None, Resources.RemoveItemIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_openBehaviorsCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Open".Localize("Name of a command"), "Opens the Selected Behavior Instances.".Localize(), Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_moveBehaviorsUpCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Move Up".Localize("Name of a command"), "Moves the selected behaviors up in the behavior set, increasing their priority for conflict resolution.".Localize(), Keys.None, Resources.ArrowUpIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_moveBehaviorsDownCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Move Down".Localize("Name of a command"), "Moves the selected behaviors down in the behavior set, decreasing their priority for conflict resolution.".Localize(), Keys.None, Resources.ArrowDownIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_exportBehaviorCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Export to New Behavior".Localize("Name of a command"), "Exports the DSG, animation bindings, timeline bindings, timelines, and attachment points to a new Behavior Instance.".Localize(), Keys.None, Resources.CopyIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_flattenBehaviorsCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Flatten to New Behavior".Localize("Name of a command"), "Generates the resolved behavior data for this entity and set, and creates a new Behavior Instance from that.".Localize(), Keys.None, Sce.Atf.Resources.GroupImage, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_clearBehaviorsCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Clear Local Behavior Overrides".Localize("Name of a command"), "Clears all locally assigned animation bindings, attachments, timeline bindings, and triggers.".Localize(), Keys.None, Resources.DeleteIcon, CommandVisibility.All));
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
		BehaviorCommandTag behaviorCommandTag = (BehaviorCommandTag)commandTag;
		if (base.EntityAdapter.As<IEntityDocument>().IsReadOnly && behaviorCommandTag.Command != Command.OpenBehavior)
		{
			return false;
		}
		if (behaviorCommandTag.Command == Command.AddBehavior)
		{
			return !string.IsNullOrEmpty(base.BehaviorProvider.DSG);
		}
		if (behaviorCommandTag.Command == Command.RemoveBehavior || behaviorCommandTag.Command == Command.OpenBehavior || behaviorCommandTag.Command == Command.MoveBehaviorsUp || behaviorCommandTag.Command == Command.MoveBehaviorsDown)
		{
			return Selection.Any();
		}
		if (behaviorCommandTag.Command != Command.ExportBehavior && behaviorCommandTag.Command != Command.FlattenBehaviors)
		{
			return behaviorCommandTag.Command == Command.ClearBehaviors;
		}
		return true;
	}

	public void DoCommand(object commandTag)
	{
		switch (((BehaviorCommandTag)commandTag).Command)
		{
		case Command.AddBehavior:
			AddBehavior();
			break;
		case Command.RemoveBehavior:
			RemoveBehavior();
			break;
		case Command.OpenBehavior:
			OpenBehaviors();
			break;
		case Command.MoveBehaviorsUp:
			MoveSelectedBehaviorsUp();
			break;
		case Command.MoveBehaviorsDown:
			MoveSelectedBehaviorsDown();
			break;
		case Command.ExportBehavior:
			ExportBehavior();
			break;
		case Command.FlattenBehaviors:
			FlattenBehaviors();
			break;
		case Command.ClearBehaviors:
			ClearBehaviorOverrides();
			break;
		}
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
		base.DomNode.Parent.As<TransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.Parent.As<TransactionContext>().Cancel();
	}

	public void End()
	{
		base.DomNode.Parent.As<TransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.Parent.As<TransactionContext>().SuspendTransactions();
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		List<string> list = new List<string>();
		List<object> list2 = new List<object>();
		foreach (BehaviorReferenceAdapter item2 in Selection)
		{
			list.Add(item2.Name);
		}
		m_selectionContext.Selection = Enumerable.Empty<object>();
		Behaviors.Clear();
		foreach (string referencedBehavior in base.BehaviorProvider.BehaviorData.ReferencedBehaviors)
		{
			BehaviorReferenceAdapter item = CreateBehaviorReference(referencedBehavior);
			Behaviors.Add(item);
			if (list.Contains(referencedBehavior))
			{
				list2.Add(item);
			}
		}
		m_selectionContext.SetRange(list2);
		RegisterForDomChanges();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		Behaviors = new DomNodeListAdapter<BehaviorReferenceAdapter>(base.DomNode, EntitySchema.BehaviorSetType.BehaviorChild);
		m_selectionContext.SelectionChanging += selection_Changing;
		m_selectionContext.SelectionChanged += selection_Changed;
	}

	protected override void OnParentNodeSet()
	{
		RegisterForDomChanges();
		base.OnParentNodeSet();
	}

	protected virtual void RegisterForDomChanges()
	{
		UnregisterFromDomChanges();
		base.DomNode.ChildInserted += HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved += HandleDomNodeChildRemoved;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == EntitySchema.BehaviorReferenceType.Type)
		{
			if (SelectionContains(e.Child))
			{
				m_selectionContext.Remove(e.Child);
			}
			BehaviorReferenceAdapter behaviorReferenceAdapter = e.Child.As<BehaviorReferenceAdapter>();
			base.BehaviorProvider.BehaviorData.RemoveBehavior(behaviorReferenceAdapter.Name);
			base.BatchChangelist?.CreateBehaviorRemovedEvent(base.BehaviorProvider.InstanceEntity, behaviorReferenceAdapter.Name);
			OnItemRemoved(e.Index, e.Child);
		}
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Child.Type != EntitySchema.BehaviorReferenceType.Type)
		{
			return;
		}
		BehaviorReferenceAdapter behaviorReferenceAdapter = e.Child.As<BehaviorReferenceAdapter>();
		using (IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() }))
		{
			IBehaviorInstance behaviorInstance = instanceSet.LoadEntityIfUnique<IBehaviorInstance>(behaviorReferenceAdapter.Name);
			if (behaviorInstance == null)
			{
				throw new InvalidTransactionException("Could not load Behavior " + behaviorReferenceAdapter.Name + ".");
			}
			if (!(behaviorInstance.DSGName == base.BehaviorProvider.DSG))
			{
				throw new InvalidTransactionException("Could not add Behavior " + behaviorInstance.Name + " to Entity " + base.BehaviorProvider.Name + " because they have different DSGs.");
			}
			base.BehaviorProvider.BehaviorData.AddBehavior(behaviorInstance);
		}
		base.BatchChangelist?.CreateBehaviorAddedEvent(base.BehaviorProvider.InstanceEntity, behaviorReferenceAdapter.Name);
		OnItemInserted(e.Index, e.Child);
	}

	private void AddBehavior()
	{
		BaseEntityPropertyContext parentAs = GetParentAs<BaseEntityPropertyContext>();
		IAssetBrowserServiceProvider parentAs2 = GetParentAs<IAssetBrowserServiceProvider>();
		Dictionary<string, InstanceType> entities = new Dictionary<string, InstanceType>();
		parentAs2.AssetBrowserService.OpenEntities(entities, new InstanceType[1] { InstanceType.IT_BEHAVIOR }, base.BehaviorProvider.AllowedBehaviorClasses);
		if (entities.Count <= 0)
		{
			return;
		}
		parentAs.DoTransaction(delegate
		{
			foreach (KeyValuePair<string, InstanceType> item2 in entities)
			{
				BehaviorReferenceAdapter item = CreateBehaviorReference(item2.Key);
				Behaviors.Add(item);
			}
		}, "Adding behaviors from the Asset Cloud.");
	}

	private BehaviorReferenceAdapter CreateBehaviorReference(string name)
	{
		DomNode domNode = new DomNode(EntitySchema.BehaviorReferenceType.Type);
		domNode.InitializeExtensions();
		BehaviorReferenceAdapter behaviorReferenceAdapter = domNode.As<BehaviorReferenceAdapter>();
		behaviorReferenceAdapter.Name = name;
		behaviorReferenceAdapter.AllowedBehaviorClasses = base.BehaviorProvider.AllowedBehaviorClasses;
		return behaviorReferenceAdapter;
	}

	private void RemoveBehavior()
	{
		GetParentAs<TransactionContext>().DoTransaction(delegate
		{
			object[] array = Selection.ToArray();
			foreach (object reference in array)
			{
				BehaviorReferenceAdapter adp = reference.As<BehaviorReferenceAdapter>();
				BehaviorReferenceAdapter item = Behaviors.FirstOrDefault((BehaviorReferenceAdapter bhv) => bhv.Name == adp.Name);
				Behaviors.Remove(item);
			}
		}, "Removing behaviors.");
	}

	private void OpenBehaviors()
	{
		BaseEntityPropertyContext parentAs = GetParentAs<BaseEntityPropertyContext>();
		foreach (object item in Selection)
		{
			BehaviorReferenceAdapter behaviorReferenceAdapter = item.As<BehaviorReferenceAdapter>();
			parentAs.AssetDocumentCommands.OpenExistingDocument(InstanceType.IT_BEHAVIOR, behaviorReferenceAdapter.Name);
		}
	}

	private void MoveSelectedBehaviorsUp()
	{
		GetParentAs<TransactionContext>().DoTransaction(delegate
		{
			foreach (object item in Selection)
			{
				BehaviorReferenceAdapter behaviorReferenceAdapter = item.As<BehaviorReferenceAdapter>();
				int num = Behaviors.IndexOf(behaviorReferenceAdapter);
				if (num > 0)
				{
					base.BehaviorProvider.BehaviorData.MoveChildBehaviors(behaviorReferenceAdapter.Name, num - 1);
				}
			}
		}, "Moving behaviors up.");
	}

	private void MoveSelectedBehaviorsDown()
	{
		GetParentAs<TransactionContext>().DoTransaction(delegate
		{
			foreach (object item in Selection)
			{
				BehaviorReferenceAdapter behaviorReferenceAdapter = item.As<BehaviorReferenceAdapter>();
				int num = Behaviors.IndexOf(behaviorReferenceAdapter);
				if (num >= 0 && num < Behaviors.Count)
				{
					base.BehaviorProvider.BehaviorData.MoveChildBehaviors(behaviorReferenceAdapter.Name, num + 1);
				}
			}
		}, "Moving behaviors down.");
	}

	private void ExportBehavior()
	{
		BaseEntityPropertyContext parentAs = GetParentAs<BaseEntityPropertyContext>();
		MessageBoxResult messageBoxResult = MessageBoxes.Show("Do you want to strip model names when exporting the behavior?", "Strip Model Names", MessageBoxButton.YesNo, MessageBoxImage.None);
		bool stripModelNames = messageBoxResult == MessageBoxResult.Yes;
		IEntityDocument entityDocument = parentAs.AssetDocumentCommands.OpenNewDocument(InstanceType.IT_BEHAVIOR);
		TransactionContext context = entityDocument.As<TransactionContext>();
		IBehaviorInstance entity = entityDocument.InstanceEntity as IBehaviorInstance;
		context.DoTransaction(delegate
		{
			entity.ClassName = base.BehaviorProvider.AllowedBehaviorClasses.FirstOrDefault();
			base.BehaviorProvider.BehaviorData.Export(entity);
			foreach (string referenceGeometryName in base.BehaviorProvider.ReferenceGeometryNames)
			{
				entity.AddReferenceGeometry(referenceGeometryName);
			}
			if (stripModelNames)
			{
				foreach (IAttachmentPoint item in entity.AttachmentPointSet.Items)
				{
					item.ModelInstanceName = string.Empty;
				}
			}
		}, "Behavior created through export.");
	}

	private void FlattenBehaviors()
	{
		BaseEntityPropertyContext context = GetParentAs<BaseEntityPropertyContext>();
		bool stripModelNames = MessageBoxes.Show("Do you want to strip model names when exporting the behavior?", "Strip Model Names", MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.Yes;
		IInstanceSet tempSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { context.CivTechService.GetActivePantryPaths() });
		try
		{
			tempSet.LoadDependentBehaviors(base.BehaviorProvider.InstanceEntity);
			IEntityDocument entityDocument = context.AssetDocumentCommands.OpenNewDocument(InstanceType.IT_BEHAVIOR);
			TransactionContext context2 = entityDocument.As<TransactionContext>();
			IBehaviorInstance entity = entityDocument.InstanceEntity as IBehaviorInstance;
			context2.DoTransaction(delegate
			{
				entity.ClassName = base.BehaviorProvider.AllowedBehaviorClasses.FirstOrDefault();
				base.BehaviorProvider.BehaviorData.Flatten(tempSet, context.CivTechService.PrimaryProject.Config.Classes, entity);
				foreach (string referenceGeometryName in base.BehaviorProvider.ReferenceGeometryNames)
				{
					entity.AddReferenceGeometry(referenceGeometryName);
				}
				if (stripModelNames)
				{
					foreach (IAttachmentPoint item in entity.AttachmentPointSet.Items)
					{
						item.ModelInstanceName = string.Empty;
					}
				}
			}, "Behavior created through flattening.");
		}
		finally
		{
			if (tempSet != null)
			{
				tempSet.Dispose();
			}
		}
	}

	private void ClearBehaviorOverrides()
	{
		GetParentAs<TransactionContext>().DoTransaction(delegate
		{
			base.BehaviorProvider.BehaviorData.ClearBehaviorOverrides();
		}, "Clearing behavior overrides.");
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		this.SelectionChanged?.Invoke(this, e);
	}

	private void selection_Changing(object sender, EventArgs e)
	{
		this.SelectionChanging?.Invoke(this, e);
	}
}
