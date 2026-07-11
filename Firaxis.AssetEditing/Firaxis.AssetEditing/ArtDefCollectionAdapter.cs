using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefCollectionAdapter : DomNodeAdapter, ITransactionContext, IDescriptionProvider, INamedAdapter, IATFEditorTarget, IObservableContext
{
	private int m_domNotificationPauseCount;

	private IList<ArtDefElementAdapter> m_elements;

	private IArtDefElementTemplate m_artDefElementsTemplate;

	private bool DomNotificationSuspended => m_domNotificationPauseCount > 0;

	public virtual string Name
	{
		get
		{
			return GetAttribute<string>(ArtDefSchema.ArtDefCollectionType.NameAttribute);
		}
		set
		{
			SetAttribute(ArtDefSchema.ArtDefCollectionType.NameAttribute, value);
		}
	}

	public string Description
	{
		get
		{
			return GetAttribute<string>(ArtDefSchema.ArtDefCollectionType.DescriptionAttribute);
		}
		set
		{
			SetAttribute(ArtDefSchema.ArtDefCollectionType.DescriptionAttribute, value);
		}
	}

	public bool ReplaceMergedCollectionElements
	{
		get
		{
			return GetAttribute<bool>(ArtDefSchema.ArtDefCollectionType.ReplaceMergedCollectionElementsAttribute);
		}
		set
		{
			SetAttribute(ArtDefSchema.ArtDefCollectionType.ReplaceMergedCollectionElementsAttribute, value);
		}
	}

	public IList<ArtDefElementAdapter> Elements => m_elements;

	private IDictionary<string, ArtDefElementAdapter> ElementMap { get; set; }

	public bool UseCustomEditor
	{
		get
		{
			if (ArtDefElementsTemplate != null)
			{
				return ArtDefElementsTemplate.HasCustomEditor;
			}
			return false;
		}
	}

	public string CustomEditor
	{
		get
		{
			if (ArtDefElementsTemplate == null)
			{
				return string.Empty;
			}
			return ArtDefElementsTemplate.CustomEditor;
		}
	}

	public IArtDef ArtDef { get; set; }

	public IArtDefCollection ArtDefCollection { get; set; }

	public IArtDefTemplate ArtDefTemplate { get; set; }

	public IArtDefElementTemplate ArtDefElementsTemplate
	{
		get
		{
			return m_artDefElementsTemplate;
		}
		set
		{
			if (m_artDefElementsTemplate != value)
			{
				m_artDefElementsTemplate = value;
				if (m_artDefElementsTemplate != null)
				{
					Description = m_artDefElementsTemplate.Description;
					ReplaceMergedCollectionElements = m_artDefElementsTemplate.ReplaceMergedCollectionElements;
				}
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
		if (ArtDefElementsTemplate != null)
		{
			object customEditorAdapter = ArtDefCustomEditors.GetCustomEditorAdapter(ArtDefElementsTemplate.CustomEditor, type, this);
			if (customEditorAdapter != null)
			{
				return customEditorAdapter;
			}
		}
		return base.GetAdapter(type);
	}

	private void SuspendDomNotifications()
	{
		m_domNotificationPauseCount++;
	}

	private void ResumeDomNotifications()
	{
		m_domNotificationPauseCount--;
		BugSubmitter.SilentAssert(m_domNotificationPauseCount >= 0, "ArtDefCollectionAdapter DomNode notification suspension has been resumed too many times @assign bwhitman");
		if (m_domNotificationPauseCount < 0)
		{
			m_domNotificationPauseCount = 0;
		}
	}

	public void DomApplyTemplateChanges(IArtDefTemplate artDefTmpl, IArtDefElementTemplate artDefElemTmpl)
	{
		ArtDefTemplate = artDefTmpl;
		ArtDefElementsTemplate = artDefElemTmpl;
		ReplaceMergedCollectionElements = artDefElemTmpl.ReplaceMergedCollectionElements;
		string collectionName = ArtDefCollection.CollectionName;
		foreach (ArtDefElementAdapter element in m_elements)
		{
			element.DomApplyTemplateChanges(artDefTmpl, artDefElemTmpl, collectionName);
		}
	}

	public void NativeApplyTemplateChanges(IArtDefTemplate artDefTmpl, IArtDefElementTemplate artDefElemTmpl)
	{
		SuspendDomNotifications();
		ArtDefCollection.ReplaceMergedCollectionElements = artDefElemTmpl.ReplaceMergedCollectionElements;
		ArtDefTemplate = artDefTmpl;
		ArtDefElementsTemplate = artDefElemTmpl;
		foreach (ArtDefElementAdapter element in m_elements)
		{
			element.NativeApplyTemplateChanges(artDefTmpl, ArtDefCollection.CollectionName, artDefElemTmpl);
		}
		Update();
		ResumeDomNotifications();
	}

	public void Update()
	{
		SuspendDomNotifications();
		if (Name != ArtDefCollection.CollectionName)
		{
			Name = ArtDefCollection.CollectionName;
		}
		ICollection<ArtDefElementAdapter> collection = new List<ArtDefElementAdapter>();
		foreach (IArtDefElement item in (IEnumerable<IArtDefElement>)ArtDefCollection.Elements.OrderBy((IArtDefElement x) => x.Name).ToArray())
		{
			ArtDefElementAdapter value = null;
			if (!ElementMap.TryGetValue(item.Name, out value))
			{
				DomNode domNode = new DomNode(ArtDefSchema.ArtDefElementType.Type);
				domNode.InitializeExtensions();
				value = domNode.As<ArtDefElementAdapter>();
				UpdateAdapterData(value, item);
				ElementMap[item.Name] = value;
				m_elements.Add(value);
			}
			else
			{
				UpdateAdapterData(value, item);
			}
			value.Update();
			collection.Add(value);
		}
		foreach (ArtDefElementAdapter item2 in m_elements.Except(collection).ToArray())
		{
			ElementMap.Remove(item2.Name);
			m_elements.Remove(item2);
		}
		ResumeDomNotifications();
	}

	private void UpdateAdapterData(ArtDefElementAdapter adapter, IArtDefElement element)
	{
		adapter.ArtDefTmpl = ArtDefTemplate;
		adapter.ArtDefElementTmpl = ArtDefElementsTemplate;
		adapter.ArtDef = ArtDef;
		adapter.ArtDefCollection = ArtDefCollection;
		adapter.ArtDefElement = element;
	}

	protected override void OnNodeSet()
	{
		m_elements = new DomNodeListAdapter<ArtDefElementAdapter>(base.DomNode, ArtDefSchema.ArtDefCollectionType.ElementsChild);
		ElementMap = new Dictionary<string, ArtDefElementAdapter>();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.OnNodeSet();
	}

	public ArtDefElementAdapter AddElement(string name, int index)
	{
		ArtDefElementAdapter artDefElementAdapter = CreateElementAdapter(name);
		AddToElementList(artDefElementAdapter, index);
		artDefElementAdapter.DomAddElementFieldsFromTemplate(ArtDefElementsTemplate);
		artDefElementAdapter.DomAddElementCollectionsFromTemplate(ArtDefElementsTemplate);
		OnReloaded();
		return artDefElementAdapter;
	}

	private ArtDefElementAdapter CreateElementAdapter(string name)
	{
		DomNode domNode = new DomNode(ArtDefSchema.ArtDefElementType.Type);
		domNode.InitializeExtensions();
		ArtDefElementAdapter artDefElementAdapter = domNode.As<ArtDefElementAdapter>();
		artDefElementAdapter.Name = name;
		artDefElementAdapter.ArtDefElementTmpl = ArtDefElementsTemplate;
		return artDefElementAdapter;
	}

	private int AddToElementList(ArtDefElementAdapter adapter, int index)
	{
		int result = index;
		ElementMap[adapter.Name] = adapter;
		if (index < 0)
		{
			result = m_elements.Count;
			m_elements.Add(adapter);
		}
		else
		{
			m_elements.Insert(index, adapter);
		}
		if (adapter.ArtDefElement == null)
		{
			throw new System.Exception("Native ArtDefElement should be assigned by this point!");
		}
		return result;
	}

	public void RemoveElement(string name)
	{
		if (ElementMap.TryGetValue(name, out var value))
		{
			RemoveFieldsFromElement(value);
			RemoveChildCollectionsFromElement(value);
			RemoveElementImpl(value);
		}
	}

	private void RemoveFieldsFromElement(ArtDefElementAdapter adapter)
	{
		while (adapter.Fields.Count > 0)
		{
			adapter.Fields.RemoveAt(0);
		}
	}

	private void RemoveChildCollectionsFromElement(ArtDefElementAdapter adapter)
	{
		while (adapter.Collections.Count > 0)
		{
			ArtDefCollectionAdapter artDefCollectionAdapter = adapter.Collections.ElementAt(0);
			string[] array = artDefCollectionAdapter.Elements.Select((ArtDefElementAdapter elem) => elem.Name).ToArray();
			foreach (string name in array)
			{
				artDefCollectionAdapter.RemoveElement(name);
			}
			adapter.Collections.RemoveAt(0);
		}
	}

	private void RemoveElementImpl(ArtDefElementAdapter adapter)
	{
		m_elements.Remove(adapter);
		ElementMap.Remove(adapter.Name);
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (!DomNotificationSuspended && e.Parent == base.DomNode && e.ChildInfo.Type == ArtDefSchema.ArtDefElementType.Type)
		{
			ArtDefElementAdapter artDefElementAdapter = e.Child.As<ArtDefElementAdapter>();
			artDefElementAdapter.ArtDef = ArtDef;
			artDefElementAdapter.ArtDefTmpl = ArtDefTemplate;
			artDefElementAdapter.ArtDefElement = ArtDefCollection.AddElement(artDefElementAdapter.Name);
			artDefElementAdapter.ArtDefCollection = ArtDefCollection;
			artDefElementAdapter.ArtDefElementTmpl = ArtDefElementsTemplate;
			ElementMap[artDefElementAdapter.Name] = artDefElementAdapter;
			OnItemInserted(e.Index, e.Child);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (!DomNotificationSuspended && e.Parent == base.DomNode && e.ChildInfo.Type == ArtDefSchema.ArtDefElementType.Type)
		{
			ArtDefElementAdapter artDefElementAdapter = e.Child.As<ArtDefElementAdapter>();
			artDefElementAdapter.ArtDefElement = null;
			ArtDefCollection.RemoveElement(artDefElementAdapter.Name);
			if (ElementMap.ContainsKey(artDefElementAdapter.Name))
			{
				ElementMap.Remove(artDefElementAdapter.Name);
			}
			OnItemRemoved(e.Index, e.Child);
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (!DomNotificationSuspended && e.AttributeInfo != ArtDefSchema.ArtDefCollectionType.NameAttribute)
		{
			if (e.AttributeInfo == ArtDefSchema.ArtDefCollectionType.ReplaceMergedCollectionElementsAttribute)
			{
				ArtDefCollection.ReplaceMergedCollectionElements = m_artDefElementsTemplate.ReplaceMergedCollectionElements;
			}
			else if (e.AttributeInfo == ArtDefSchema.ArtDefElementType.NameAttribute && e.DomNode.Parent == base.DomNode && !ElementMap.ContainsKey((string)e.NewValue))
			{
				ElementMap[(string)e.NewValue] = ElementMap[(string)e.OldValue];
				ElementMap.Remove((string)e.OldValue);
			}
		}
	}

	public ArtDefElementAdapter FindElement(string name)
	{
		if (!ElementMap.ContainsKey(name))
		{
			return null;
		}
		return ElementMap[name];
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
