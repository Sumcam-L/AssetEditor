using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;
using UtilityTools.Helpers;

namespace Firaxis.AssetEditing;

public class CookParameterSetAdapter : EntityComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, IFieldContainerAdapter, ICommandClient, ISubSelectionContext, ISelectionContext, IValidationContext
{
	private enum Command
	{
		AddEntriesFromSource,
		ReimportEntries,
		OpenEntries,
		ClearEntries
	}

	private struct CookParameterSetCommandTag
	{
		public readonly Command Command;

		public readonly Action CommandAction;

		public readonly Func<bool> CanDoCommand;

		public CookParameterSetCommandTag(Command command, Action action, Func<bool> canDoCommand)
		{
			Command = command;
			CommandAction = action;
			CanDoCommand = canDoCommand;
		}
	}

	private readonly CookParameterSetCommandTag AddEntriesFromSourceCommandTag;

	private readonly CookParameterSetCommandTag ReimportEntriesCommandTag;

	private readonly CookParameterSetCommandTag OpenEntriesCommandTag;

	private readonly CookParameterSetCommandTag ClearEntriesCommandTag;

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	private ISelectionContext m_selectionContext = new SelectionContext();

	private IParameterSet ParameterSet { get; set; }

	public IValueSet ValueSet { get; set; }

	public IList<IFieldValueAdapter> CookParameters { get; private set; }

	public IList<IFieldValueAdapter> Fields => CookParameters;

	public IEntityFilteringContext EntityFilteringContext { get; private set; }

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

	public ISelectionContext SubSelectionContext => m_selectionContext.LastSelected.As<ISelectionContext>();

	public IEnumerable<object> Items
	{
		get
		{
			yield return base.DomNode;
		}
	}

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (IFieldValueAdapter cookParameter in CookParameters)
			{
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in cookParameter.PropertyDescriptors)
				{
					yield return propertyDescriptor;
				}
			}
		}
	}

	public bool InTransaction => base.EntityAdapter.As<ITransactionContext>().InTransaction;

	public int PendingOperationCount => base.EntityAdapter.As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public event EventHandler Beginning;

	public event EventHandler Cancelled;

	public event EventHandler Ending;

	public event EventHandler Ended;

	public CookParameterSetAdapter()
	{
		AddEntriesFromSourceCommandTag = new CookParameterSetCommandTag(Command.AddEntriesFromSource, DoCreateCommand, CanDoCreateCommand);
		ReimportEntriesCommandTag = new CookParameterSetCommandTag(Command.ReimportEntries, DoReimportCommand, CanDoReimportCommand);
		OpenEntriesCommandTag = new CookParameterSetCommandTag(Command.OpenEntries, DoOpenCommand, CanDoOpenCommand);
		ClearEntriesCommandTag = new CookParameterSetCommandTag(Command.ClearEntries, DoClearCommand, CanDoClearCommand);
		m_commands.Add(new CommandInfo(AddEntriesFromSourceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add New".Localize("Name of a command"), "Creates new entities from the source file and adds them to the cook parameter set.".Localize(), Keys.None, Firaxis.ATF.Resources.AddNewEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ReimportEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Reimport".Localize("Name of a command"), "Reimports the object parameters within the cook parameter set.".Localize(), Keys.None, Firaxis.ATF.Resources.ReimportFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(OpenEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Open".Localize("Name of a command"), "Opens the documents of the assigned object cook parameters.".Localize(), Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(ClearEntriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.ComponentEdits, "Clear Bound Objects".Localize("Name of a command"), "Clears all objects bound in the Cook Parameter Set.".Localize(), Keys.None, Resources.ClearMaterialsIcon, CommandVisibility.All));
		if (this.SelectionChanging != null && this.SelectionChanged != null && this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		CookParameters = new DomNodeListAdapter<IFieldValueAdapter>(base.DomNode, EntitySchema.CookParametersSetType.CookParameterChild);
		EntityFilteringContext = new EntityFilteringContext();
	}

	private void CreateParamChangedEvent(DomNode param)
	{
		while (param.Parent != base.DomNode && param.Parent != null)
		{
			param = param.Parent;
		}
		if (param.Parent == base.DomNode)
		{
			IFieldValueAdapter fieldValueAdapter = param.As<IFieldValueAdapter>();
			if (fieldValueAdapter != null)
			{
				IInstanceEntity instanceEntity = base.EntityAdapter.InstanceEntity;
				base.BatchChangelist?.CreateEntityCookParameterChangedEvent(instanceEntity.Type, instanceEntity.Name, fieldValueAdapter.Name, fieldValueAdapter.Value);
			}
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		OnItemChanged(e.DomNode);
		if (base.DomNode.Parent.Is<InstanceEntityAdapter>())
		{
			CreateParamChangedEvent(e.DomNode);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Parent.Equals(base.DomNode))
		{
			IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
			if (fieldValueAdapter != null)
			{
				fieldValueAdapter.AddNativeField(ValueSet, fieldValueAdapter.Parameter);
				IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter != null)
				{
					fieldContainerAdapter.ItemInserted += FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter.ItemRemoved += FieldContainerAdapter_ItemRemoved;
				}
			}
			OnItemInserted(e.Index, e.Child);
		}
		CreateParamChangedEvent(e.Child);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Parent.Equals(base.DomNode))
		{
			IInstanceEntity instanceEntity = base.EntityAdapter.InstanceEntity;
			base.BatchChangelist?.CreateEntityChangedEvent(instanceEntity.Type, instanceEntity.Name);
		}
		else
		{
			CreateParamChangedEvent(e.Parent);
		}
		if (!e.Parent.Equals(base.DomNode))
		{
			return;
		}
		IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
		if (fieldValueAdapter != null)
		{
			IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
			if (fieldContainerAdapter != null)
			{
				fieldContainerAdapter.ItemInserted -= FieldContainerAdapter_ItemInserted;
				fieldContainerAdapter.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
			}
			ValueSet.Remove(fieldValueAdapter.Value);
		}
		OnItemRemoved(e.Index, e.Child);
	}

	private void FieldContainerAdapter_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		OnItemInserted(e.Index, e.Item);
	}

	private void FieldContainerAdapter_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		OnItemRemoved(e.Index, e.Item);
	}

	public void AddDefaultValuesAsNecessary(IParameterSet paramSet)
	{
		foreach (IParameter fldDef in paramSet.Items)
		{
			IFieldValueAdapter fieldValueAdapter = Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == fldDef.Name);
			if (fieldValueAdapter == null)
			{
				fieldValueAdapter = FieldValueHelper.CreateField(fldDef);
				Fields.Add(fieldValueAdapter);
			}
			fieldValueAdapter.AssignDefaultValue();
		}
	}

	public FieldValueAdapter AddParameter(IParameter param)
	{
		return AddParameter(param, null, null);
	}

	public FieldValueAdapter AddParameter(IParameter param, EventHandler<AttributeEventArgs> onChangedAction)
	{
		return AddParameter(param, onChangedAction, null);
	}

	public FieldValueAdapter AddParameter(IParameter param, EventHandler<AttributeEventArgs> onChangedAction, EventHandler<AttributeEventArgs> onChangingAction)
	{
		FieldValueAdapter fieldValueAdapter = FieldValueHelper.CreateField(param);
		PlatformAssert.If(fieldValueAdapter == null, "Tried to create a parameter that's not supported by this parameter set.");
		CookParameters.Add(fieldValueAdapter);
		fieldValueAdapter.AssignDefaultValue();
		fieldValueAdapter.DomNode.AttributeChanging += onChangingAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanging);
		fieldValueAdapter.DomNode.AttributeChanged += onChangedAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanged);
		return fieldValueAdapter;
	}

	public void Update(IValueSet values, IParameterSet paramSet, bool updateUI)
	{
		Update(values, paramSet, null, null, updateUI);
	}

	public void Update(IValueSet values, IParameterSet paramSet, EventHandler<AttributeEventArgs> onChangedAction, bool updateUI)
	{
		Update(values, paramSet, onChangedAction, null, updateUI);
	}

	public void Update(IValueSet values, IParameterSet paramSet, EventHandler<AttributeEventArgs> onChangedAction, EventHandler<AttributeEventArgs> onChangingAction, bool updateUI)
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		IList<IFieldValueAdapter> list = new List<IFieldValueAdapter>();
		values.RemoveUnusedValues(paramSet);
		values.AddDefaultValuesAsNecessary(paramSet);
		ParameterSet = paramSet;
		ValueSet = values;
		foreach (IValue value in values.Items)
		{
			FieldValueAdapter fieldValueAdapter = CookParameters.FirstOrDefault((IFieldValueAdapter ufv) => ufv.Parameter.Name == value.ParameterName && ufv.Parameter.ParameterValueType == value.ParameterType) as FieldValueAdapter;
			bool flag = false;
			if (fieldValueAdapter == null)
			{
				fieldValueAdapter = FieldValueHelper.CreateField(value.ParameterName, paramSet);
				PlatformAssert.If(fieldValueAdapter == null, "Tried to create a parameter that's not supported by this parameter set.");
				CookParameters.Add(fieldValueAdapter);
				UpdateAdapter(fieldValueAdapter, value, paramSet);
				fieldValueAdapter.DomNode.AttributeChanging += onChangingAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanging);
				fieldValueAdapter.DomNode.AttributeChanged += onChangedAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanged);
				IFieldContainerAdapter fieldContainerAdapter = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter != null)
				{
					fieldContainerAdapter.ItemInserted += FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter.ItemRemoved += FieldContainerAdapter_ItemRemoved;
				}
			}
			else
			{
				IFieldContainerAdapter fieldContainerAdapter2 = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter2 != null)
				{
					fieldContainerAdapter2.ItemInserted -= FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter2.ItemRemoved -= FieldContainerAdapter_ItemRemoved;
				}
				fieldValueAdapter.DomNode.AttributeChanging -= onChangingAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanging);
				fieldValueAdapter.DomNode.AttributeChanged -= onChangedAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanged);
				flag = fieldValueAdapter.RequiresUpdate(value);
				UpdateAdapter(fieldValueAdapter, value, paramSet);
				fieldValueAdapter.DomNode.AttributeChanging += onChangingAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanging);
				fieldValueAdapter.DomNode.AttributeChanged += onChangedAction ?? new EventHandler<AttributeEventArgs>(CookParameterChanged);
				fieldContainerAdapter2 = fieldValueAdapter.As<IFieldContainerAdapter>();
				if (fieldContainerAdapter2 != null)
				{
					fieldContainerAdapter2.ItemInserted += FieldContainerAdapter_ItemInserted;
					fieldContainerAdapter2.ItemRemoved += FieldContainerAdapter_ItemRemoved;
				}
			}
			list.Add(fieldValueAdapter);
			if (flag)
			{
				IInstanceEntity instanceEntity = base.EntityAdapter.InstanceEntity;
				base.BatchChangelist?.CreateEntityCookParameterChangedEvent(instanceEntity.Type, instanceEntity.Name, fieldValueAdapter.Name, fieldValueAdapter.Value);
			}
			if (updateUI)
			{
				OnReloaded();
			}
		}
		foreach (var entryAdapter in CookParameters.Except(list).ToArray())
		{
			DomNode domNode = entryAdapter.DomNode;
			EventHandler<AttributeEventArgs> value2;
			if ((value2 = onChangingAction) == null)
			{
				value2 = CookParameterChanging;
			}
			domNode.AttributeChanging -= value2;
			DomNode domNode2 = entryAdapter.DomNode;
			EventHandler<AttributeEventArgs> value3;
			if ((value3 = onChangedAction) == null)
			{
				value3 = CookParameterChanged;
			}
			domNode2.AttributeChanged -= value3;
			CookParameters.Remove(entryAdapter);
		}
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void UpdateAdapter(FieldValueAdapter adapter, IValue value, IParameterSet parameterSet)
	{
		adapter.UpdateDomFromNative(value);
		FieldValueHelper.UpdateObjectValues(adapter, parameterSet);
	}

	protected virtual void CookParameterChanging(object sender, AttributeEventArgs e)
	{
	}

	protected virtual void CookParameterChanged(object sender, AttributeEventArgs e)
	{
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

	private void DoCreateCommand()
	{
		IEnumerable<InstanceType> objectParameterTypes = ParameterSet.GetObjectParameterTypes();
		InstanceType newEntityType = objectParameterTypes.First();
		objectParameterTypes.Count();
		_ = 1;
		BaseEntityPropertyContext context = base.EntityAdapter.As<BaseEntityPropertyContext>();
		List<string> list = DialogHelper.SelectSourceFiles(context.CivTechService, multiSelect: true, newEntityType).ToList();
		List<SourceFileModel> sources = new List<SourceFileModel>();
		foreach (string item in list)
		{
			sources.Add(new SourceFileModel(item, newEntityType));
		}
		if (list.Count == 0)
		{
			return;
		}
		context.DoTransaction(delegate
		{
			using IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { base.EntityAdapter.CivTechService.GetActivePantryPaths() });
			SetAdapterHelper.LaunchSourceClassAssociationView(context.CivTechService, entitySet, sources, ParameterSet, ValueSet, newEntityType);
		}, "Populate Parameter Set");
	}

	private bool CanDoCreateCommand()
	{
		if (!base.EntityAdapter.As<IEntityDocument>().IsReadOnly && ParameterSet != null && ValueSet != null)
		{
			return ParameterSet.GetObjectParameterTypes().Count() > 0;
		}
		return false;
	}

	private void DoOpenCommand()
	{
		BaseEntityPropertyContext baseEntityPropertyContext = base.EntityAdapter.As<BaseEntityPropertyContext>();
		foreach (IObjectValue item in ValueSet.ItemsOfType<IObjectValue>())
		{
			if (!string.IsNullOrWhiteSpace(item.GetBoundObjectName()))
			{
				baseEntityPropertyContext.AssetDocumentCommands.OpenExistingDocument(item.GetBoundObjectType(), item.GetBoundObjectName());
			}
		}
	}

	private bool CanDoOpenCommand()
	{
		if (ParameterSet != null && ValueSet != null)
		{
			return ParameterSet.GetObjectParameterTypes().Count() > 0;
		}
		return false;
	}

	private void DoReimportCommand()
	{
		IObjectValue[] array = (from val in ValueSet.ItemsOfType<IObjectValue>()
			where !string.IsNullOrEmpty(val.GetBoundObjectName())
			select val).ToArray();
		if (!array.Any())
		{
			return;
		}
		BaseEntityPropertyContext entityContext = base.EntityAdapter.As<BaseEntityPropertyContext>();
		IEntityDocument entityDocument = base.EntityAdapter.As<IEntityDocument>();
		List<EntityID> entityIDs = new List<EntityID>();
		IObjectValue[] array2 = array;
		foreach (IObjectValue objectValue in array2)
		{
			entityIDs.Add(new EntityID(objectValue));
		}
		using (entityContext.SuspendRecording())
		{
			entityContext.DoTransaction(delegate
			{
				entityContext.BatchChangelist?.AddGenericEntityChangedEvents(entityIDs);
				SetAdapterHelper.ImportSelectedEntities(entityIDs, entityContext, recurseIntoChildren: false);
			}, "Reimporting Entries.");
		}
		entityContext.AssetDocumentCommands.OpenExistingDocument(entityDocument.InstanceEntity.Type, entityDocument.InstanceEntity.Name);
		if (base.HotLoadOnReimport)
		{
			List<Uri> list = new List<Uri> { entityDocument.Uri };
			list.AddRange(StaticMethods.GetEntityURIs(entityContext.CivTechService, entityIDs));
			base.TunerQueueService?.AddFilesToQueue(list);
		}
	}

	private bool CanDoReimportCommand()
	{
		if (base.EntityAdapter.As<IEntityDocument>().IsReadOnly)
		{
			return false;
		}
		if (ParameterSet == null || ValueSet == null)
		{
			return false;
		}
		IEnumerable<IObjectValue> enumerable = ValueSet.ItemsOfType<IObjectValue>();
		if (!enumerable.Any())
		{
			return false;
		}
		ICivTechService civTechService = base.EntityAdapter.CivTechService;
		foreach (IObjectValue item in enumerable)
		{
			string boundObjectName = item.GetBoundObjectName();
			if (!string.IsNullOrEmpty(boundObjectName))
			{
				InstanceType boundObjectType = item.GetBoundObjectType();
				string entityPath = civTechService.GetEntityPath(boundObjectName, boundObjectType);
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
		}
		return true;
	}

	private void DoClearCommand()
	{
		BaseEntityPropertyContext context = base.EntityAdapter.As<BaseEntityPropertyContext>();
		IEnumerable<IObjectValue> objValues = ValueSet.ItemsOfType<IObjectValue>();
		context.DoTransaction(delegate
		{
			foreach (IObjectValue item in objValues)
			{
				item.BindObject(string.Empty, InstanceType.IT_INVALID);
			}
		}, "Clear Bound Objects");
	}

	private bool CanDoClearCommand()
	{
		return !base.EntityAdapter.As<IEntityDocument>().IsReadOnly;
	}

	public bool CanDoCommand(object commandTag)
	{
		return ((CookParameterSetCommandTag)commandTag).CanDoCommand();
	}

	public void DoCommand(object commandTag)
	{
		((CookParameterSetCommandTag)commandTag).CommandAction();
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
		base.EntityAdapter.As<ITransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.EntityAdapter.As<ITransactionContext>().Cancel();
	}

	public void End()
	{
		base.EntityAdapter.As<ITransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.EntityAdapter.As<ITransactionContext>().SuspendTransactions();
	}
}
