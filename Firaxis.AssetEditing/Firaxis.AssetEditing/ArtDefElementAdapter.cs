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

namespace Firaxis.AssetEditing;

public class ArtDefElementAdapter : DomNodeAdapter, ITransactionContext, INamedAdapter, IFieldContainerAdapter, IPropertyEditingContext, IObservableContext
{
	private bool _isAddingField;

	private readonly IDictionary<string, FieldValueAdapter> m_fieldMap = new Dictionary<string, FieldValueAdapter>();

	private IList<IFieldValueAdapter> m_fields;

	private readonly IDictionary<string, ArtDefCollectionAdapter> m_collectionMap = new Dictionary<string, ArtDefCollectionAdapter>();

	private IList<ArtDefCollectionAdapter> m_collections;

	public virtual string Name
	{
		get
		{
			return GetAttribute<string>(ArtDefSchema.ArtDefElementType.NameAttribute);
		}
		set
		{
			SetAttribute(ArtDefSchema.ArtDefElementType.NameAttribute, value);
		}
	}

	public virtual bool AppendMergedParameterCollections
	{
		get
		{
			return GetAttribute<bool>(ArtDefSchema.ArtDefElementType.AppendMergedParameterCollectionsAttribute);
		}
		set
		{
			SetAttribute(ArtDefSchema.ArtDefElementType.AppendMergedParameterCollectionsAttribute, value);
		}
	}

	public IList<IFieldValueAdapter> Fields => m_fields;

	public IList<ArtDefCollectionAdapter> Collections => m_collections;

	public IArtDef ArtDef { get; set; }

	public IArtDefCollection ArtDefCollection { get; set; }

	public IArtDefElement ArtDefElement { get; set; }

	public IArtDefTemplate ArtDefTmpl { get; set; }

	public IArtDefElementTemplate ArtDefElementTmpl { get; set; }

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
			foreach (System.ComponentModel.PropertyDescriptor defaultProperty in base.DomNode.GetDefaultProperties())
			{
				yield return defaultProperty;
			}
			foreach (System.ComponentModel.PropertyDescriptor fieldPropertyDescriptor in GetFieldPropertyDescriptors())
			{
				yield return fieldPropertyDescriptor;
			}
		}
	}

	public bool InTransaction => base.DomNode.GetRoot().As<ITransactionContext>()?.InTransaction ?? false;

	public int PendingOperationCount => base.DomNode.GetRoot().As<ITransactionContext>()?.PendingOperationCount ?? 0;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public override object GetAdapter(Type type)
	{
		if (ArtDefElementTmpl != null)
		{
			object customEditorAdapter = ArtDefCustomEditors.GetCustomEditorAdapter(ArtDefElementTmpl.CustomEditor, type, this);
			if (customEditorAdapter != null)
			{
				return customEditorAdapter;
			}
		}
		return base.GetAdapter(type);
	}

	protected override void OnNodeSet()
	{
		m_fields = new DomNodeListAdapter<IFieldValueAdapter>(base.DomNode, ArtDefSchema.ArtDefElementType.FieldsChild);
		m_collections = new DomNodeListAdapter<ArtDefCollectionAdapter>(base.DomNode, ArtDefSchema.ArtDefElementType.CollectionsChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		if (ArtDefElement != null)
		{
			InitializeFields(ArtDef, ArtDefElement);
		}
	}

	public IFieldValueAdapter AddField(IParameter param)
	{
		if (m_fieldMap.ContainsKey(param.Name))
		{
			throw new System.Exception("Attemping to add duplicate parameter with name " + param.Name + " in element " + ArtDefElement.Name);
		}
		FieldValueAdapter fieldValueAdapter = FieldValueHelper.CreateField(param);
		m_fieldMap[param.Name] = fieldValueAdapter;
		_isAddingField = true;
		Fields.Add(fieldValueAdapter);
		_isAddingField = false;
		fieldValueAdapter.AssignDefaultValue();
		return fieldValueAdapter;
	}

	private void RegisterForDomChanges(bool includeAttr = true)
	{
		UnregisterFromDomChanges(includeAttr);
		if (includeAttr)
		{
			base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		}
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterFromDomChanges(bool includeAttr = true)
	{
		if (includeAttr)
		{
			base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		}
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	private ITransactionContext GetActiveContext()
	{
		for (DomNode domNode = base.DomNode; domNode != null; domNode = domNode.Parent)
		{
			ITransactionContext transactionContext = domNode.As<ITransactionContext>();
			if (transactionContext != null && transactionContext.InTransaction)
			{
				return transactionContext;
			}
		}
		return null;
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Parent == base.DomNode)
		{
			if (e.ChildInfo == ArtDefSchema.ArtDefElementType.FieldsChild)
			{
				IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
				ArtDefElement.Fields.Remove(fieldValueAdapter.Value);
			}
			else if (e.ChildInfo == ArtDefSchema.ArtDefElementType.CollectionsChild)
			{
				ArtDefCollectionAdapter artDefCollectionAdapter = e.Child.As<ArtDefCollectionAdapter>();
				ArtDefElement.RemoveCollection(artDefCollectionAdapter.Name);
			}
		}
		OnItemRemoved(e.Index, e.Child);
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Parent == base.DomNode)
		{
			if (e.ChildInfo == ArtDefSchema.ArtDefElementType.FieldsChild)
			{
				IFieldValueAdapter fieldValueAdapter = e.Child.As<IFieldValueAdapter>();
				if (ArtDefElement.Fields.FindValue(fieldValueAdapter.Parameter.Name) != null)
				{
					throw new System.Exception("Duplicate parameter with name " + fieldValueAdapter.Parameter.Name + " in element " + ArtDefElement.Name);
				}
				fieldValueAdapter.AddNativeField(ArtDefElement.Fields, fieldValueAdapter.Parameter);
				if (_isAddingField)
				{
					using (GetActiveContext()?.SuspendTransactions())
					{
						fieldValueAdapter.UpdateDomFromNative(fieldValueAdapter.Value);
					}
				}
				else
				{
					fieldValueAdapter.UpdateNativeFromDom();
				}
			}
			else if (e.ChildInfo == ArtDefSchema.ArtDefElementType.CollectionsChild)
			{
				ArtDefCollectionAdapter artDefCollectionAdapter = e.Child.As<ArtDefCollectionAdapter>();
				IArtDefCollection artDefCollection = ArtDefElement.AddCollection(artDefCollectionAdapter.Name, artDefCollectionAdapter.ArtDefElementsTemplate);
				artDefCollectionAdapter.ArtDefCollection = artDefCollection;
			}
		}
		OnItemInserted(e.Index, e.Child);
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.DomNode == base.DomNode)
		{
			if (e.AttributeInfo == ArtDefSchema.ArtDefElementType.NameAttribute)
			{
				if (ArtDefElement != null)
				{
					string newName = (string)e.NewValue;
					string text = (string)e.OldValue;
					if (newName != ArtDefElement.Name)
					{
						if (ArtDefCollection.Elements.Any((IArtDefElement elem) => elem.Name == newName))
						{
							ArtDefElement.Name = text;
							throw new InvalidTransactionException("Element named \"" + Name + "\" already exists in collection \"" + ArtDefCollection.CollectionName + "\", Reverting to " + text + "!");
						}
						ArtDefElement.Name = newName;
					}
				}
			}
			else if (e.AttributeInfo == ArtDefSchema.ArtDefElementType.AppendMergedParameterCollectionsAttribute)
			{
				ArtDefElement.AppendMergedParameterCollections = (bool)e.NewValue;
			}
			OnItemChanged(e.DomNode);
		}
	}

	private IEnumerable<Pair<string, IArtDefElementTemplate>> BuildAddedCollectionList(IArtDefElementTemplate artDefElemTmpl)
	{
		foreach (IArtDefElementTemplate colTmpl in artDefElemTmpl.Children)
		{
			if (!Collections.Any((ArtDefCollectionAdapter col) => col.Name == colTmpl.Name))
			{
				yield return new Pair<string, IArtDefElementTemplate>(colTmpl.Name, colTmpl);
			}
		}
	}

	private IEnumerable<string> BuildRemovedCollectionList(IArtDefElementTemplate artDefElemTmpl)
	{
		foreach (ArtDefCollectionAdapter col in Collections)
		{
			if (!artDefElemTmpl.Children.Any((IArtDefElementTemplate colTmpl) => colTmpl.Name == col.Name))
			{
				yield return col.Name;
			}
		}
	}

	private void DomUpdateCollectionsFromTemplate(IArtDefElementTemplate artDefElemTmpl)
	{
		foreach (string colName in BuildRemovedCollectionList(artDefElemTmpl).ToArray())
		{
			RemoveCollection(colName);
		}
		foreach (var colInfo in BuildAddedCollectionList(artDefElemTmpl).ToArray())
		{
			AddCollection(colInfo.First, colInfo.Second);
		}
	}

	public void DomApplyTemplateChanges(IArtDefTemplate artDefTmpl, IArtDefElementTemplate artDefElemTmpl, string parentName)
	{
		ArtDefTmpl = artDefTmpl;
		ArtDefElementTmpl = artDefElemTmpl;
		Func<IFieldValueAdapter, bool> predicate = delegate(IFieldValueAdapter value)
		{
			IParameter parameter = artDefElemTmpl.Parameters.FindByName(value.Name);
			return parameter == null || parameter.ParameterValueType != value.Value.ParameterType;
		};
		foreach (IFieldValueAdapter item in (IEnumerable<IFieldValueAdapter>)Fields.Where(predicate).ToArray())
		{
			Fields.Remove(item);
		}
		DomAddElementFieldsFromTemplate(artDefElemTmpl);
		DomUpdateCollectionsFromTemplate(artDefElemTmpl);
		foreach (ArtDefCollectionAdapter artDefColAdapter in Collections)
		{
			artDefElemTmpl.Children.ForEach(delegate(IArtDefElementTemplate adet)
			{
				if (adet.Name == artDefColAdapter.Name)
				{
					artDefColAdapter.DomApplyTemplateChanges(artDefTmpl, adet);
				}
			});
		}
	}

	public void NativeApplyTemplateChanges(IArtDefTemplate artDefTmpl, string parentName, IArtDefElementTemplate artDefElemTmpl)
	{
		UnregisterFromDomChanges();
		ArtDefTmpl = artDefTmpl;
		ArtDefElementTmpl = artDefElemTmpl;
		Func<IValue, bool> predicate = delegate(IValue value)
		{
			IParameter parameter = artDefElemTmpl.Parameters.FindByName(value.ParameterName);
			return parameter == null || parameter.ParameterValueType != value.ParameterType;
		};
		foreach (IValue item in (IEnumerable<IValue>)ArtDefElement.Fields.Items.Where(predicate).ToArray())
		{
			ArtDefElement.Fields.Remove(item);
		}
		DomAddElementFieldsFromTemplate(artDefElemTmpl);
		ArtDefElement.UpdateCollectionsFromTemplate(artDefElemTmpl);
		Update();
		foreach (ArtDefCollectionAdapter artDefColAdapter in m_collections)
		{
			artDefElemTmpl.Children.ForEach(delegate(IArtDefElementTemplate adet)
			{
				if (adet.Name == artDefColAdapter.Name)
				{
					artDefColAdapter.NativeApplyTemplateChanges(artDefTmpl, adet);
				}
			});
		}
		RegisterForDomChanges();
	}

	public void InitializeFields(IArtDef artDef, IArtDefElement artDefElement)
	{
		UnregisterFromDomChanges(includeAttr: false);
		ArtDef = artDef;
		ArtDefElement = artDefElement;
		m_fieldMap.Clear();
		if (ArtDefElementTmpl != null)
		{
			IValue[] array = artDefElement.Fields.Items.ToArray();
			foreach (IValue val in array)
			{
				IParameter parameter = ArtDefElementTmpl.Parameters.Items.FirstOrDefault((IParameter paramItem) => paramItem.Name == val.ParameterName);
				if (parameter == null)
				{
					Outputs.WriteLine(OutputMessageType.Error, "Unable to create an adapter for Value {0}.  There is no matching parameter in the parameter set.", val.ParameterName);
					continue;
				}
				if (val.ParameterType != parameter.ParameterValueType)
				{
					Outputs.WriteLine(OutputMessageType.Error, "Unable to create an adapter for Value {0}.  The Value's Type is {1}, but the expected type is {2}.", val.ParameterName, val.ParameterType, parameter.ParameterValueType);
					continue;
				}
				FieldValueAdapter fieldValueAdapter = CreateFieldValueAdapter(parameter);
				if (fieldValueAdapter != null)
				{
					fieldValueAdapter.ItemChanged += FieldAdapter_ItemChanged;
					m_fieldMap[val.ParameterName] = fieldValueAdapter;
					m_fields.Add(fieldValueAdapter);
					fieldValueAdapter.UpdateDomFromNative(val);
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, "Tried to create a parameter that's not supported by this parameter set. Parameter Name: {0}, Type: {1}", parameter.Name, parameter.ParameterType);
					artDefElement.Fields.Remove(val);
				}
			}
		}
		RegisterForDomChanges(includeAttr: false);
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		if (Name != ArtDefElement.Name)
		{
			Name = ArtDefElement.Name;
		}
		UpdateFields();
		UpdateCollections();
		RegisterForDomChanges();
	}

	public void UpdateFields()
	{
		IList<IFieldValueAdapter> list = new List<IFieldValueAdapter>();
		if (ArtDefElementTmpl != null)
		{
			Dictionary<string, IParameter> paramMap = new Dictionary<string, IParameter>();
			foreach (IParameter p in ArtDefElementTmpl.Parameters.Items)
			{
				paramMap[p.Name] = p;
			}
			IValue[] array = ArtDefElement.Fields.Items.ToArray();
			foreach (IValue val in array)
			{
				IParameter parameter = null;
				paramMap.TryGetValue(val.ParameterName, out parameter);
				if (parameter != null)
				{
					if (!m_fieldMap.TryGetValue(val.ParameterName, out var value))
					{
						if (val.ParameterType != parameter.ParameterValueType)
						{
							Outputs.WriteLine(OutputMessageType.Error, "Unable to create an adapter for Value {0}.  The Value's Type is {1}, but the expected type is {2}.", val.ParameterName, val.ParameterType, parameter.ParameterValueType);
							continue;
						}
						value = CreateFieldValueAdapter(parameter);
						if (value == null)
						{
							Outputs.WriteLine(OutputMessageType.Error, "Tried to create a parameter that's not supported by this parameter set. Parameter Name: {0}, Type: {1}", parameter.Name, parameter.ParameterType);
							continue;
						}
						value.ItemChanged += FieldAdapter_ItemChanged;
						m_fieldMap[val.ParameterName] = value;
						m_fields.Add(value);
					}
					value.UpdateDomFromNative(val);
					list.Add(value);
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Error, "Unable to create an adapter for Value {0}.  There is no matching parameter in the parameter set.", val.ParameterName);
					ArtDefElement.Fields.Remove(val);
				}
			}
		}
		HashSet<IFieldValueAdapter> keepSet = new HashSet<IFieldValueAdapter>(list);
		for (int i = m_fields.Count - 1; i >= 0; i--)
		{
			IFieldValueAdapter fldAdapter = m_fields[i];
			if (!keepSet.Contains(fldAdapter))
			{
				m_fieldMap.Remove(fldAdapter.Name);
				m_fields.RemoveAt(i);
			}
		}
	}

	public ArtDefCollectionAdapter AddCollection(string name, IArtDefElementTemplate artDefElemTmpl)
	{
		DomNode domNode = new DomNode(ArtDefSchema.ArtDefCollectionType.Type);
		domNode.InitializeExtensions();
		ArtDefCollectionAdapter artDefCollectionAdapter = domNode.As<ArtDefCollectionAdapter>();
		artDefCollectionAdapter.Name = name;
		artDefCollectionAdapter.ArtDefElementsTemplate = artDefElemTmpl;
		m_collectionMap[name] = artDefCollectionAdapter;
		m_collections.Add(artDefCollectionAdapter);
		artDefCollectionAdapter.ArtDef = ArtDef;
		artDefCollectionAdapter.ArtDefTemplate = ArtDefTmpl;
		return artDefCollectionAdapter;
	}

	public void RemoveCollection(string name)
	{
		ArtDefCollectionAdapter value = null;
		m_collectionMap.TryGetValue(name, out value);
		m_collectionMap.Remove(name);
		m_collections.Remove(value);
	}

	public void UpdateCollections()
	{
		List<ArtDefCollectionAdapter> list = new List<ArtDefCollectionAdapter>();
		string[] templateCollectionNames = ArtDefElementTmpl.Children.Select((IArtDefElementTemplate x) => x.Name).ToArray();
		foreach (IArtDefCollection artDefCol in ArtDefElement.Children.OrderBy((IArtDefCollection c) => Array.IndexOf(templateCollectionNames, c.CollectionName)))
		{
			IArtDefElementTemplate artDefElementTemplate = ArtDefElementTmpl.Children.FirstOrDefault((IArtDefElementTemplate adet) => adet.Name == artDefCol.CollectionName);
			if (artDefElementTemplate != null)
			{
				if (!m_collectionMap.TryGetValue(artDefCol.CollectionName, out var value))
				{
					value = AddCollection(artDefCol.CollectionName, artDefElementTemplate);
					value.ArtDefCollection = artDefCol;
				}
				else
				{
					value.ArtDef = ArtDef;
					value.ArtDefCollection = artDefCol;
					value.Name = artDefCol.CollectionName;
					value.ArtDefTemplate = ArtDefTmpl;
					value.ArtDefElementsTemplate = artDefElementTemplate;
				}
				value.Update();
				list.Add(value);
			}
		}
		HashSet<ArtDefCollectionAdapter> keepSet = new HashSet<ArtDefCollectionAdapter>(list);
		for (int i = m_collections.Count - 1; i >= 0; i--)
		{
			ArtDefCollectionAdapter artDefCollectionAdapter = m_collections[i];
			if (!keepSet.Contains(artDefCollectionAdapter))
			{
				m_collectionMap.Remove(artDefCollectionAdapter.Name);
				m_collections.RemoveAt(i);
			}
		}
	}

	private void FieldAdapter_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		OnItemChanged(e.Item);
	}

	private FieldValueAdapter CreateFieldValueAdapter(IParameter parameter)
	{
		return FieldValueHelper.CreateField(parameter);
	}

	public IFieldValueAdapter FindField(string name)
	{
		if (!m_fieldMap.ContainsKey(name))
		{
			return null;
		}
		return m_fieldMap[name];
	}

	public IFieldValueAdapter GetAdapterByName(string fieldName)
	{
		return Fields.FirstOrDefault((IFieldValueAdapter fva) => fva.Name == fieldName);
	}

	public ArtDefCollectionAdapter FindCollection(string name)
	{
		if (!m_collectionMap.ContainsKey(name))
		{
			return null;
		}
		return m_collectionMap[name];
	}

	public void DomAddElementFieldsFromTemplate(IArtDefElementTemplate elemTmpl)
	{
		AppendMergedParameterCollections = elemTmpl.AppendMergedParameterCollections;
		foreach (IParameter param in elemTmpl.Parameters.Items)
		{
			if (!m_fieldMap.ContainsKey(param.Name))
			{
				AddField(param);
			}
		}
	}

	public void DomAddElementCollectionsFromTemplate(IArtDefElementTemplate elemTmpl)
	{
		foreach (IArtDefElementTemplate child in elemTmpl.Children)
		{
			AddCollection(child.Name, child);
		}
	}

	private IEnumerable<System.ComponentModel.PropertyDescriptor> GetFieldPropertyDescriptors()
	{
		foreach (IFieldValueAdapter item in Fields.Where((IFieldValueAdapter f) => f.PropertyDescriptors != null))
		{
			foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in item.PropertyDescriptors)
			{
				yield return propertyDescriptor;
			}
		}
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
		base.DomNode.GetRoot().As<ITransactionContext>()?.Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.GetRoot().As<ITransactionContext>()?.Cancel();
	}

	public void End()
	{
		base.DomNode.GetRoot().As<ITransactionContext>()?.End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.GetRoot().As<ITransactionContext>()?.SuspendTransactions();
	}
}
