using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class GeometryReferenceSetAdapter : BehaviorComponentAdapterBase, IPropertyEditingListContext, IPropertyEditingContext, IObservableContext, ITransactionContext, ICommandContext, ICommandClient, ISelectionContext
{
	private enum Command
	{
		AddGeometryReference,
		RemoveGeometryReference,
		OpenGeometry
	}

	private struct GeometryReferenceCommandTag
	{
		public readonly Command Command;

		public GeometryReferenceCommandTag(Command command)
		{
			Command = command;
		}
	}

	private List<CommandInfo> m_commands = new List<CommandInfo>();

	private bool m_containsDependencyChange;

	private ISelectionContext m_selectionContext = new SelectionContext();

	private static GeometryReferenceCommandTag m_addGeometryReferenceCommandTag = new GeometryReferenceCommandTag(Command.AddGeometryReference);

	private static GeometryReferenceCommandTag m_openGeometriesCommandTag = new GeometryReferenceCommandTag(Command.OpenGeometry);

	private static GeometryReferenceCommandTag m_removeGeometryReferenceCommandTag = new GeometryReferenceCommandTag(Command.RemoveGeometryReference);

	public IList<GeometryReferenceAdapter> GeometryReferences { get; private set; }

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

	public IEnumerable<object> Items => GeometryReferences;

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (object item in EntitySchema.GeometryReferenceType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

	public bool InTransaction => base.DomNode.Parent.As<ITransactionContext>().InTransaction;

	public int PendingOperationCount => base.DomNode.Parent.As<ITransactionContext>().PendingOperationCount;

	public event EventHandler SelectionChanging;

	public event EventHandler SelectionChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public GeometryReferenceSetAdapter()
	{
		m_commands.Add(new CommandInfo(m_addGeometryReferenceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Add Geometry Reference".Localize("Name of a command"), "Adds the selected geometry to the behavior as a reference.".Localize(), Keys.None, Firaxis.ATF.Resources.AddExistingEntityIcon, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_removeGeometryReferenceCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Remove Geometry Reference".Localize("Name of a command"), "Removes the selected geometry references from the behavior.".Localize(), Keys.None, Sce.Atf.Resources.RemoveImage, CommandVisibility.All));
		m_commands.Add(new CommandInfo(m_openGeometriesCommandTag, StandardMenu.Edit, AssetComponentCommandGroup.SetEdits, "Open Geometry".Localize("Name of a command"), "Opens the Selected Geometry Instances.".Localize(), Keys.None, Firaxis.ATF.Resources.GotoFileIcon, CommandVisibility.All));
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
		GeometryReferenceCommandTag geometryReferenceCommandTag = (GeometryReferenceCommandTag)commandTag;
		if (base.EntityAdapter.As<IEntityDocument>().IsReadOnly && geometryReferenceCommandTag.Command != Command.OpenGeometry)
		{
			return false;
		}
		if (geometryReferenceCommandTag.Command == Command.AddGeometryReference)
		{
			if (!string.IsNullOrEmpty(base.BehaviorProvider.InstanceEntity.ClassName))
			{
				return base.BehaviorProvider.InstanceEntity is IBehaviorInstance;
			}
			return false;
		}
		if (geometryReferenceCommandTag.Command == Command.RemoveGeometryReference || geometryReferenceCommandTag.Command == Command.OpenGeometry)
		{
			return Selection.Any();
		}
		return false;
	}

	private void AddReferenceGeometry()
	{
		BehaviorAdapter parentAs = GetParentAs<BehaviorAdapter>();
		BaseEntityPropertyContext parentAs2 = GetParentAs<BaseEntityPropertyContext>();
		IAssetBrowserServiceProvider parentAs3 = GetParentAs<IAssetBrowserServiceProvider>();
		Dictionary<string, InstanceType> entities = new Dictionary<string, InstanceType>();
		parentAs3.AssetBrowserService.OpenEntities(entities, new InstanceType[1] { InstanceType.IT_GEOMETRY }, parentAs.BehaviorClass.AllowedGeometryClasses);
		if (entities.Count <= 0)
		{
			return;
		}
		parentAs2.DoTransaction(delegate
		{
			foreach (KeyValuePair<string, InstanceType> item in entities)
			{
				AddGeometryReference(item.Key);
			}
			m_containsDependencyChange = true;
		}, "Adding geometries from the Asset Cloud.");
	}

	private void RemoveReferenceGeometry()
	{
		TransactionContext parentAs = GetParentAs<TransactionContext>();
		if (!(base.BehaviorProvider.InstanceEntity is IBehaviorInstance))
		{
			return;
		}
		parentAs.DoTransaction(delegate
		{
			object[] array = Selection.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				GeometryReferenceAdapter geometryReferenceAdapter = array[i].As<GeometryReferenceAdapter>();
				RemoveGeometryReference(geometryReferenceAdapter.Name);
			}
			m_containsDependencyChange = true;
		}, "Removing reference geometries.");
	}

	private void OpenReferenceGeometries()
	{
		BaseEntityPropertyContext parentAs = GetParentAs<BaseEntityPropertyContext>();
		foreach (object item in Selection)
		{
			GeometryReferenceAdapter geometryReferenceAdapter = item.As<GeometryReferenceAdapter>();
			parentAs.AssetDocumentCommands.OpenExistingDocument(InstanceType.IT_GEOMETRY, geometryReferenceAdapter.Name);
		}
	}

	public void DoCommand(object commandTag)
	{
		switch (((GeometryReferenceCommandTag)commandTag).Command)
		{
		case Command.AddGeometryReference:
			AddReferenceGeometry();
			break;
		case Command.RemoveGeometryReference:
			RemoveReferenceGeometry();
			break;
		case Command.OpenGeometry:
			OpenReferenceGeometries();
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
		base.DomNode.Parent.As<ITransactionContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.Parent.As<ITransactionContext>().Cancel();
	}

	public void End()
	{
		base.DomNode.Parent.As<ITransactionContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.Parent.As<ITransactionContext>().SuspendTransactions();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		GeometryReferences = new DomNodeListAdapter<GeometryReferenceAdapter>(base.DomNode, EntitySchema.GeometryReferenceSetType.GeometryReferencesChild);
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
		if (e.Child.Type == EntitySchema.GeometryReferenceType.Type)
		{
			IBehaviorInstance obj = base.BehaviorProvider.InstanceEntity as IBehaviorInstance;
			GeometryReferenceAdapter geometryReferenceAdapter = e.Child.As<GeometryReferenceAdapter>();
			obj.RemoveReferenceGeometry(geometryReferenceAdapter.Name);
			base.BatchChangelist?.CreateEntityChangedEvent(base.BehaviorProvider.InstanceType, base.BehaviorProvider.Name);
			OnItemRemoved(e.Index, e.Child);
		}
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Child.Type == EntitySchema.GeometryReferenceType.Type)
		{
			IBehaviorInstance obj = base.BehaviorProvider.InstanceEntity as IBehaviorInstance;
			GeometryReferenceAdapter geometryReferenceAdapter = e.Child.As<GeometryReferenceAdapter>();
			obj.AddReferenceGeometry(geometryReferenceAdapter.Name);
			base.BatchChangelist?.CreateEntityChangedEvent(base.BehaviorProvider.InstanceType, base.BehaviorProvider.Name);
			OnItemInserted(e.Index, e.Child);
		}
	}

	public void AddGeometryReference(string geoName)
	{
		DomNode domNode = new DomNode(EntitySchema.GeometryReferenceType.Type);
		domNode.InitializeExtensions();
		GeometryReferenceAdapter geometryReferenceAdapter = domNode.As<GeometryReferenceAdapter>();
		geometryReferenceAdapter.Name = geoName;
		GeometryReferences.Add(geometryReferenceAdapter);
	}

	public bool ContainsDependencyChange()
	{
		bool containsDependencyChange = m_containsDependencyChange;
		m_containsDependencyChange = false;
		return containsDependencyChange;
	}

	public void RemoveGeometryReference(string geoName)
	{
		GeometryReferenceAdapter item = GeometryReferences.FirstOrDefault((GeometryReferenceAdapter gr) => gr.Name == geoName);
		GeometryReferences.Remove(item);
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		GeometryReferences.Clear();
		foreach (string referenceGeometryName in base.BehaviorProvider.ReferenceGeometryNames)
		{
			AddGeometryReference(referenceGeometryName);
		}
		RegisterForDomChanges();
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
