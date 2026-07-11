using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public sealed class DomNode : IAdaptable, IDecoratable
{
	private class NodeEventHandlers
	{
		private readonly DomNode m_node;

		private List<DomNode> m_subscribers;

		public event EventHandler<AttributeEventArgs> AttributeChanging;

		public event EventHandler<AttributeEventArgs> AttributeChanged;

		public event EventHandler<ChildEventArgs> ChildInserting;

		public event EventHandler<ChildEventArgs> ChildInserted;

		public event EventHandler<ChildEventArgs> ChildRemoving;

		public event EventHandler<ChildEventArgs> ChildRemoved;

		public NodeEventHandlers(DomNode node)
		{
			m_node = node;
		}

		internal void RaiseAttributeChanging(AttributeEventArgs e)
		{
			this.AttributeChanging.Raise(m_node, e);
			if (m_subscribers == null)
			{
				return;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				subscriber.RaiseAttributeChanging(e);
			}
		}

		internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangingHandlers()
		{
			if (this.AttributeChanging != null)
			{
				Delegate[] invocationList = this.AttributeChanging.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					yield return (EventHandler<AttributeEventArgs>)invocationList[i];
				}
			}
			if (m_subscribers == null)
			{
				yield break;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				foreach (EventHandler<AttributeEventArgs> attributeChangingHandler in subscriber.GetAttributeChangingHandlers())
				{
					yield return attributeChangingHandler;
				}
			}
		}

		internal void RaiseAttributeChanged(AttributeEventArgs e)
		{
			this.AttributeChanged.Raise(m_node, e);
			if (m_subscribers == null)
			{
				return;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				subscriber.RaiseAttributeChanged(e);
			}
		}

		internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangedHandlers()
		{
			if (this.AttributeChanged != null)
			{
				Delegate[] invocationList = this.AttributeChanged.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					yield return (EventHandler<AttributeEventArgs>)invocationList[i];
				}
			}
			if (m_subscribers == null)
			{
				yield break;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				foreach (EventHandler<AttributeEventArgs> attributeChangedHandler in subscriber.GetAttributeChangedHandlers())
				{
					yield return attributeChangedHandler;
				}
			}
		}

		internal void RaiseChildInserting(ChildEventArgs e)
		{
			this.ChildInserting.Raise(m_node, e);
			if (m_subscribers == null)
			{
				return;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				subscriber.RaiseChildInserting(e);
			}
		}

		internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertingHandlers()
		{
			if (this.ChildInserting != null)
			{
				Delegate[] invocationList = this.ChildInserting.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					yield return (EventHandler<ChildEventArgs>)invocationList[i];
				}
			}
			if (m_subscribers == null)
			{
				yield break;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				foreach (EventHandler<ChildEventArgs> childInsertingHandler in subscriber.GetChildInsertingHandlers())
				{
					yield return childInsertingHandler;
				}
			}
		}

		internal void RaiseChildInserted(ChildEventArgs e)
		{
			this.ChildInserted.Raise(m_node, e);
			if (m_subscribers == null)
			{
				return;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				subscriber.RaiseChildInserted(e);
			}
		}

		internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertedHandlers()
		{
			if (this.ChildInserted != null)
			{
				Delegate[] invocationList = this.ChildInserted.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					yield return (EventHandler<ChildEventArgs>)invocationList[i];
				}
			}
			if (m_subscribers == null)
			{
				yield break;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				foreach (EventHandler<ChildEventArgs> childInsertedHandler in subscriber.GetChildInsertedHandlers())
				{
					yield return childInsertedHandler;
				}
			}
		}

		internal void RaiseChildRemoving(ChildEventArgs e)
		{
			this.ChildRemoving.Raise(m_node, e);
			if (m_subscribers == null)
			{
				return;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				subscriber.RaiseChildRemoving(e);
			}
		}

		internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovingHandlers()
		{
			if (this.ChildRemoving != null)
			{
				Delegate[] invocationList = this.ChildRemoving.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					yield return (EventHandler<ChildEventArgs>)invocationList[i];
				}
			}
			if (m_subscribers == null)
			{
				yield break;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				foreach (EventHandler<ChildEventArgs> childRemovingHandler in subscriber.GetChildRemovingHandlers())
				{
					yield return childRemovingHandler;
				}
			}
		}

		internal void RaiseChildRemoved(ChildEventArgs e)
		{
			this.ChildRemoved.Raise(m_node, e);
			if (m_subscribers == null)
			{
				return;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				subscriber.RaiseChildRemoved(e);
			}
		}

		internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovedHandlers()
		{
			if (this.ChildRemoved != null)
			{
				Delegate[] invocationList = this.ChildRemoved.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					yield return (EventHandler<ChildEventArgs>)invocationList[i];
				}
			}
			if (m_subscribers == null)
			{
				yield break;
			}
			foreach (DomNode subscriber in m_subscribers)
			{
				foreach (EventHandler<ChildEventArgs> childRemovedHandler in subscriber.GetChildRemovedHandlers())
				{
					yield return childRemovedHandler;
				}
			}
		}

		internal void Subscribe(DomNode subscriber)
		{
			if (m_subscribers == null)
			{
				m_subscribers = new List<DomNode>();
			}
			m_subscribers.Add(subscriber);
		}

		internal void Unsubscribe(DomNode subscriber)
		{
			m_subscribers.Remove(subscriber);
		}
	}

	private class DomNodeDebugger
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly DomNode m_node;

		public IList<AttributeDebugger> Attributes => m_node.Type.Attributes.Select((AttributeInfo attrInfo) => new AttributeDebugger(attrInfo, m_node.GetAttribute(attrInfo))).ToList();

		public IList<ChildDebugger> Children => m_node.Children.Select((DomNode child) => new ChildDebugger(child.ChildInfo, child)).ToList();

		public IList<object> Extensions
		{
			get
			{
				List<object> list = new List<object>();
				for (int i = m_node.Type.FirstExtensionIndex; i < m_node.Type.FieldCount; i++)
				{
					list.Add(m_node.m_data[i]);
				}
				return list;
			}
		}

		public IList<ListenerDebugger> AttributeChangingListeners => (from listener in m_node.GetAttributeChangingHandlers()
			select new ListenerDebugger(listener)).ToList();

		public IList<ListenerDebugger> AttributeChangedListeners => (from listener in m_node.GetAttributeChangedHandlers()
			select new ListenerDebugger(listener)).ToList();

		public IList<ListenerDebugger> ChildInsertingListeners => (from listener in m_node.GetChildInsertingHandlers()
			select new ListenerDebugger(listener)).ToList();

		public IList<ListenerDebugger> ChildInsertedListeners => (from listener in m_node.GetChildInsertedHandlers()
			select new ListenerDebugger(listener)).ToList();

		public IList<ListenerDebugger> ChildRemovingListeners => (from listener in m_node.GetChildRemovingHandlers()
			select new ListenerDebugger(listener)).ToList();

		public IList<ListenerDebugger> ChildRemovedListeners => (from listener in m_node.GetChildRemovedHandlers()
			select new ListenerDebugger(listener)).ToList();

		public DomNodeDebugger(DomNode node)
		{
			m_node = node;
		}

		public override string ToString()
		{
			return "Additional debug info";
		}
	}

	[DebuggerDisplay("{Value}", Name = "{AttributeInfo}")]
	private class AttributeDebugger
	{
		public readonly AttributeInfo AttributeInfo;

		public readonly object Value;

		public AttributeDebugger(AttributeInfo info, object value)
		{
			AttributeInfo = info;
			Value = value;
		}
	}

	[DebuggerDisplay("{Child}", Name = "{ChildInfo}")]
	private class ChildDebugger
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public readonly ChildInfo ChildInfo;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public readonly DomNode Child;

		public ChildDebugger(ChildInfo info, DomNode child)
		{
			ChildInfo = info;
			Child = child;
		}
	}

	[DebuggerDisplay("{Target}")]
	private class ListenerDebugger
	{
		public readonly MethodInfo Method;

		public readonly object Target;

		public ListenerDebugger(Delegate d)
		{
			Method = d.Method;
			Target = d.Target;
		}
	}

	private class NodeList : IList<DomNode>, ICollection<DomNode>, IEnumerable<DomNode>, IEnumerable
	{
		private class Enumerator : IEnumerator<DomNode>, IDisposable, IEnumerator
		{
			private readonly NodeList m_owner;

			private readonly IEnumerator<DomNode> m_enumerator;

			public DomNode Current
			{
				get
				{
					if (m_enumerator != null)
					{
						return m_enumerator.Current;
					}
					throw new InvalidOperationException("No current element");
				}
			}

			object IEnumerator.Current => Current;

			public Enumerator(NodeList owner)
			{
				m_owner = owner;
				IList<DomNode> list = m_owner.GetList();
				if (list != null)
				{
					m_enumerator = list.GetEnumerator();
				}
			}

			public void Dispose()
			{
				if (m_enumerator != null)
				{
					m_enumerator.Dispose();
				}
			}

			public bool MoveNext()
			{
				if (m_enumerator != null)
				{
					return m_enumerator.MoveNext();
				}
				if (m_owner.GetList() != null)
				{
					throw new InvalidOperationException("Underlying collection was modified");
				}
				return false;
			}

			public void Reset()
			{
				if (m_enumerator != null)
				{
					m_enumerator.Reset();
				}
			}
		}

		public class ChildList : Collection<DomNode>
		{
			private readonly DomNode m_node;

			private readonly ChildInfo m_childInfo;

			public ChildList(DomNode node, ChildInfo childInfo)
			{
				m_node = node;
				m_childInfo = childInfo;
			}

			protected override void InsertItem(int index, DomNode item)
			{
				if (item == null)
				{
					throw new ArgumentNullException("item");
				}
				DomNode node = m_node;
				ChildEventArgs e = new ChildEventArgs(node, m_childInfo, item, index);
				node.RaiseChildInserting(e);
				base.InsertItem(index, item);
				item.SetParent(node, m_childInfo);
				DomNode.DiagnosticChildInserted.Raise(item, e);
				node.RaiseChildInserted(e);
			}

			protected override void RemoveItem(int index)
			{
				DomNode domNode = base[index];
				DomNode node = m_node;
				ChildEventArgs e = new ChildEventArgs(node, m_childInfo, domNode, index);
				node.RaiseChildRemoving(e);
				base.RemoveItem(index);
				domNode.SetParent(null, null);
				DomNode.DiagnosticChildRemoved.Raise(domNode, e);
				node.RaiseChildRemoved(e);
			}

			protected override void SetItem(int index, DomNode item)
			{
				RemoveItem(index);
				InsertItem(index, item);
			}

			protected override void ClearItems()
			{
				while (base.Count > 0)
				{
					RemoveAt(base.Count - 1);
				}
			}
		}

		private readonly DomNode m_node;

		private readonly ChildInfo m_childInfo;

		public DomNode this[int index]
		{
			get
			{
				IList<DomNode> listForIndexing = GetListForIndexing();
				return listForIndexing[index];
			}
			set
			{
				IList<DomNode> listForIndexing = GetListForIndexing();
				listForIndexing[index] = value;
			}
		}

		public int Count => GetList()?.Count ?? 0;

		public bool IsReadOnly => false;

		public NodeList(DomNode node, ChildInfo childInfo)
		{
			m_node = node;
			m_childInfo = childInfo;
		}

		public int IndexOf(DomNode item)
		{
			return GetList()?.IndexOf(item) ?? (-1);
		}

		public void Insert(int index, DomNode item)
		{
			if (item.m_parent != null)
			{
				if (item.m_parent == m_node)
				{
					int num = IndexOf(item);
					if (num < index)
					{
						index--;
					}
				}
				item.RemoveFromParent();
			}
			IList<DomNode> orCreateList = GetOrCreateList();
			orCreateList.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			IList<DomNode> listForIndexing = GetListForIndexing();
			listForIndexing.RemoveAt(index);
			DestroyListIfEmpty();
		}

		public void Add(DomNode item)
		{
			((IList<DomNode>)this).Insert(((ICollection<DomNode>)this).Count, item);
		}

		public void Clear()
		{
			DestroyList();
		}

		public bool Contains(DomNode item)
		{
			return GetList()?.Contains(item) ?? false;
		}

		public void CopyTo(DomNode[] array, int arrayIndex)
		{
			GetList()?.CopyTo(array, arrayIndex);
		}

		public bool Remove(DomNode item)
		{
			IList<DomNode> list = GetList();
			if (list != null)
			{
				bool result = list.Remove(item);
				DestroyListIfEmpty();
				return result;
			}
			return false;
		}

		public IEnumerator<DomNode> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IList<DomNode> GetOrCreateList()
		{
			IList<DomNode> list = GetList();
			if (list == null)
			{
				list = CreateList();
			}
			return list;
		}

		private IList<DomNode> GetListForIndexing()
		{
			IList<DomNode> list = GetList();
			if (list == null)
			{
				throw new IndexOutOfRangeException("List is empty");
			}
			return list;
		}

		private void DestroyListIfEmpty()
		{
			IList<DomNode> list = GetList();
			if (list != null && list.Count == 0)
			{
				DestroyList();
			}
		}

		private DomNode Convert(object value)
		{
			if (!(value is DomNode result))
			{
				throw new ArgumentException("item must be DomNode");
			}
			return result;
		}

		private IList<DomNode> CreateList()
		{
			ChildList childList = new ChildList(m_node, m_childInfo);
			m_node.SetChildListObject(m_childInfo, childList);
			return childList;
		}

		private IList<DomNode> GetList()
		{
			return m_node.GetChildListObject(m_childInfo);
		}

		private void DestroyList()
		{
			ChildList childListObject = m_node.GetChildListObject(m_childInfo);
			if (childListObject != null)
			{
				childListObject.Clear();
				m_node.SetChildListObject(m_childInfo, null);
			}
		}

		public override string ToString()
		{
			return $"Count = {Count}";
		}
	}

	private readonly DomNodeType m_type;

	private DomNode m_parent;

	private ChildInfo m_childInfo;

	private readonly object[] m_data;

	private NodeEventHandlers m_eventHandlers;

	private Dictionary<object, object> m_tags;

	public DomNodeType Type => m_type;

	public DomNode Parent => m_parent;

	public ChildInfo ChildInfo => m_childInfo;

	public IEnumerable<DomNode> Lineage
	{
		get
		{
			for (DomNode node = this; node != null; node = node.m_parent)
			{
				yield return node;
			}
		}
	}

	public IEnumerable<DomNode> Ancestry
	{
		get
		{
			for (DomNode node = m_parent; node != null; node = node.m_parent)
			{
				yield return node;
			}
		}
	}

	public IEnumerable<DomNode> Children
	{
		get
		{
			foreach (ChildInfo childInfo in m_type.Children)
			{
				foreach (DomNode child in GetChildren(childInfo))
				{
					yield return child;
				}
			}
		}
	}

	public IEnumerable<DomNode> Subtree
	{
		get
		{
			Stack<DomNode> nodes = new Stack<DomNode>();
			nodes.Push(this);
			while (nodes.Count > 0)
			{
				DomNode node = nodes.Pop();
				yield return node;
				foreach (ChildInfo childInfo in node.Type.Children)
				{
					int i = childInfo.Index + node.Type.FirstChildIndex;
					if (childInfo.IsList)
					{
						if (node.m_data[i] is NodeList.ChildList children)
						{
							for (int j = children.Count - 1; j >= 0; j--)
							{
								nodes.Push(children[j]);
							}
						}
					}
					else if (node.m_data[i] is DomNode child)
					{
						nodes.Push(child);
					}
				}
			}
		}
	}

	public IEnumerable<DomNode> LevelSubtree
	{
		get
		{
			Queue<DomNode> queue = new Queue<DomNode>();
			queue.Enqueue(this);
			HashSet<DomNode> visited = new HashSet<DomNode> { this };
			while (queue.Count > 0)
			{
				yield return queue.Dequeue();
				foreach (DomNode child in Children)
				{
					if (!visited.Contains(child))
					{
						visited.Add(child);
						queue.Enqueue(child);
					}
				}
			}
		}
	}

	private DomNodeDebugger _DebugInfo => new DomNodeDebugger(this);

	public event EventHandler<AttributeEventArgs> AttributeChanging
	{
		add
		{
			GetEventHandlers().AttributeChanging += value;
		}
		remove
		{
			GetEventHandlers().AttributeChanging -= value;
		}
	}

	public event EventHandler<AttributeEventArgs> AttributeChanged
	{
		add
		{
			GetEventHandlers().AttributeChanged += value;
		}
		remove
		{
			GetEventHandlers().AttributeChanged -= value;
		}
	}

	public event EventHandler<ChildEventArgs> ChildInserting
	{
		add
		{
			GetEventHandlers().ChildInserting += value;
		}
		remove
		{
			GetEventHandlers().ChildInserting -= value;
		}
	}

	public event EventHandler<ChildEventArgs> ChildInserted
	{
		add
		{
			GetEventHandlers().ChildInserted += value;
		}
		remove
		{
			GetEventHandlers().ChildInserted -= value;
		}
	}

	public event EventHandler<ChildEventArgs> ChildRemoving
	{
		add
		{
			GetEventHandlers().ChildRemoving += value;
		}
		remove
		{
			GetEventHandlers().ChildRemoving -= value;
		}
	}

	public event EventHandler<ChildEventArgs> ChildRemoved
	{
		add
		{
			GetEventHandlers().ChildRemoved += value;
		}
		remove
		{
			GetEventHandlers().ChildRemoved -= value;
		}
	}

	internal static event EventHandler<ChildEventArgs> DiagnosticChildInserted;

	internal static event EventHandler<ChildEventArgs> DiagnosticChildRemoved;

	internal static event EventHandler<AttributeEventArgs> DiagnosticAttributeChanged;

	public DomNode(DomNodeType nodeType)
		: this(nodeType, null)
	{
	}

	public DomNode(DomNodeType nodeType, ChildInfo childInfo)
	{
		if (nodeType.IsAbstract)
		{
			throw new InvalidOperationException("Can't instantiate an abstract node type");
		}
		m_type = nodeType;
		m_childInfo = childInfo;
		if (!nodeType.IsFrozen)
		{
			nodeType.Freeze();
		}
		m_data = ((nodeType.FieldCount > 0) ? new object[nodeType.FieldCount] : EmptyArray<object>.Instance);
		int firstExtensionIndex = nodeType.FirstExtensionIndex;
		int num = 0;
		foreach (ExtensionInfo extension in nodeType.Extensions)
		{
			object obj = extension.Create(this);
			m_data[num + firstExtensionIndex] = obj;
			num++;
		}
	}

	public DomNode GetRoot()
	{
		DomNode domNode = this;
		while (domNode.m_parent != null)
		{
			domNode = domNode.m_parent;
		}
		return domNode;
	}

	public IEnumerable<DomNode> GetPath()
	{
		List<DomNode> path = new List<DomNode>(Lineage);
		int i = path.Count;
		while (true)
		{
			int num = i - 1;
			i = num;
			if (num >= 0)
			{
				yield return path[i];
				continue;
			}
			break;
		}
	}

	public bool IsDescendantOf(DomNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		for (DomNode parent = m_parent; parent != null; parent = parent.m_parent)
		{
			if (parent == node)
			{
				return true;
			}
		}
		return false;
	}

	public static IEnumerable<DomNode> GetRoots(IEnumerable<DomNode> nodes)
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		HashSet<DomNode> nodeSet = new HashSet<DomNode>(nodes);
		foreach (DomNode node in nodes)
		{
			DomNode ancestor = node.Parent;
			while (ancestor != null && !nodeSet.Contains(ancestor))
			{
				ancestor = ancestor.Parent;
			}
			if (ancestor == null)
			{
				yield return node;
			}
		}
	}

	public static DomNode GetLowestCommonAncestor(DomNode node1, DomNode node2)
	{
		if (node1 == null)
		{
			return node2;
		}
		if (node2 == null)
		{
			return node1;
		}
		HashSet<DomNode> hashSet = new HashSet<DomNode>(node1.Lineage);
		foreach (DomNode item in node2.Lineage)
		{
			if (hashSet.Contains(item))
			{
				return item;
			}
		}
		return null;
	}

	public static DomNode GetLowestCommonAncestor(IEnumerable<DomNode> nodes)
	{
		if (nodes == null)
		{
			throw new ArgumentNullException("nodes");
		}
		DomNode domNode = null;
		foreach (DomNode node in nodes)
		{
			domNode = GetLowestCommonAncestor(domNode, node);
			if (domNode == null)
			{
				break;
			}
		}
		return domNode;
	}

	public void InitializeExtensions()
	{
		foreach (DomNode item in Subtree)
		{
			foreach (ExtensionInfo extension in item.Type.Extensions)
			{
				if (item.GetExtension(extension) is IAdapter { Adaptee: null } adapter)
				{
					adapter.Adaptee = item;
				}
			}
		}
	}

	public object GetAttribute(AttributeInfo attributeInfo)
	{
		object obj = GetLocalAttribute(attributeInfo);
		if (obj == null)
		{
			obj = attributeInfo.DefaultValue;
		}
		return obj;
	}

	public object GetLocalAttribute(AttributeInfo attributeInfo)
	{
		int dataIndex = m_type.GetDataIndex(attributeInfo);
		return m_data[dataIndex];
	}

	public string GetId()
	{
		string result = null;
		AttributeInfo idAttribute = m_type.IdAttribute;
		if (idAttribute != null)
		{
			object localAttribute = GetLocalAttribute(idAttribute);
			if (localAttribute != null)
			{
				result = localAttribute.ToString();
			}
		}
		return result;
	}

	public bool IsAttributeSet(AttributeInfo attributeInfo)
	{
		return GetLocalAttribute(attributeInfo) != null;
	}

	public bool IsAttributeDefault(AttributeInfo attributeInfo)
	{
		return GetLocalAttribute(attributeInfo)?.Equals(attributeInfo.DefaultValue) ?? true;
	}

	public void SetAttributeIfDefault(AttributeInfo attributeInfo, object value)
	{
		if (IsAttributeDefault(attributeInfo))
		{
			SetAttribute(attributeInfo, value);
		}
	}

	public void SetAttribute(AttributeInfo attributeInfo, object value)
	{
		int dataIndex = m_type.GetDataIndex(attributeInfo);
		object obj = m_data[dataIndex];
		if (obj == null)
		{
			obj = attributeInfo.DefaultValue;
		}
		if (!attributeInfo.Type.AreEqual(obj, value))
		{
			AttributeEventArgs e = new AttributeEventArgs(this, attributeInfo, obj, value);
			RaiseAttributeChanging(e);
			m_data[dataIndex] = value;
			DomNode.DiagnosticAttributeChanged.Raise(this, e);
			try
			{
				RaiseAttributeChanged(e);
				return;
			}
			catch (InvalidTransactionException ex)
			{
				if (ex.ReportError)
				{
					m_data[dataIndex] = obj;
				}
				throw;
			}
		}
		if (m_data[dataIndex] == null)
		{
			m_data[dataIndex] = value;
		}
	}

	public DomNode GetChild(ChildInfo childInfo)
	{
		if (childInfo.IsList)
		{
			throw new InvalidOperationException("field is a list");
		}
		int dataIndex = m_type.GetDataIndex(childInfo);
		return m_data[dataIndex] as DomNode;
	}

	public IList<DomNode> GetChildList(ChildInfo childInfo)
	{
		if (!childInfo.IsList)
		{
			throw new InvalidOperationException("field is a singleton");
		}
		return new NodeList(this, childInfo);
	}

	public IEnumerable<DomNode> GetChildren(ChildInfo childInfo)
	{
		int index = m_type.GetDataIndex(childInfo);
		if (childInfo.IsList)
		{
			if (!(m_data[index] is NodeList.ChildList children))
			{
				yield break;
			}
			foreach (DomNode item in children)
			{
				yield return item;
			}
		}
		else if (m_data[index] is DomNode child)
		{
			yield return child;
		}
	}

	public void SetChild(ChildInfo childInfo, DomNode child)
	{
		if (childInfo.IsList)
		{
			throw new InvalidOperationException("field is a list");
		}
		int dataIndex = m_type.GetDataIndex(childInfo);
		DomNode domNode = m_data[dataIndex] as DomNode;
		ChildEventArgs e = new ChildEventArgs(this, childInfo, domNode, 0);
		ChildEventArgs e2 = new ChildEventArgs(this, childInfo, child, 0);
		if (m_data[dataIndex] != null)
		{
			RaiseChildRemoving(e);
			m_data[dataIndex] = null;
			domNode.SetParent(null, null);
			RaiseChildRemoved(e);
		}
		if (child != null)
		{
			if (child.m_parent != null)
			{
				child.RemoveFromParent();
			}
			RaiseChildInserting(e2);
			m_data[dataIndex] = child;
			child.SetParent(this, childInfo);
			RaiseChildInserted(e2);
		}
	}

	public void RemoveFromParent()
	{
		if (m_parent != null)
		{
			if (m_childInfo.IsList)
			{
				IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
				childList.Remove(this);
			}
			else
			{
				m_parent.SetChild(m_childInfo, null);
			}
			SetParent(null, null);
		}
	}

	public object GetExtension(ExtensionInfo extensionInfo)
	{
		int dataIndex = m_type.GetDataIndex(extensionInfo);
		return m_data[dataIndex];
	}

	public void SubscribeToEvents(DomNode destination)
	{
		GetEventHandlers().Subscribe(destination);
	}

	public void UnsubscribeFromEvents(DomNode destination)
	{
		GetEventHandlers().Unsubscribe(destination);
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(typeof(DomNode)))
		{
			return this;
		}
		return DomNodeType.GetAdapter(this, type);
	}

	public IEnumerable<object> GetDecorators(Type type)
	{
		return DomNodeType.GetAdapters(this, type);
	}

	public static DomNode[] Copy(IEnumerable<DomNode> originals, Dictionary<DomNode, DomNode> originalToCopyMap = null)
	{
		if (originalToCopyMap == null)
		{
			originalToCopyMap = new Dictionary<DomNode, DomNode>();
		}
		else
		{
			originalToCopyMap.Clear();
		}
		List<DomNode> list = new List<DomNode>();
		foreach (DomNode original in originals)
		{
			DomNode item = original.Copy(originalToCopyMap);
			list.Add(item);
		}
		foreach (DomNode item2 in list)
		{
			item2.UpdateReferences(originalToCopyMap);
		}
		return list.ToArray();
	}

	public static DomNode Copy(DomNode original)
	{
		Dictionary<DomNode, DomNode> originalToCopyMap = new Dictionary<DomNode, DomNode>();
		DomNode domNode = original.Copy(originalToCopyMap);
		domNode.UpdateReferences(originalToCopyMap);
		return domNode;
	}

	private DomNode Copy(IDictionary<DomNode, DomNode> originalToCopyMap)
	{
		DomNode domNode = new DomNode(m_type, m_childInfo);
		originalToCopyMap.Add(this, domNode);
		foreach (AttributeInfo attribute in m_type.Attributes)
		{
			object localAttribute = GetLocalAttribute(attribute);
			if (localAttribute != null)
			{
				domNode.SetAttribute(attribute, attribute.Type.Clone(localAttribute));
			}
		}
		foreach (ChildInfo child3 in m_type.Children)
		{
			if (child3.IsList)
			{
				IList<DomNode> childList = domNode.GetChildList(child3);
				foreach (DomNode child4 in GetChildList(child3))
				{
					DomNode item = child4.Copy(originalToCopyMap);
					childList.Add(item);
				}
			}
			else
			{
				DomNode child = GetChild(child3);
				if (child != null)
				{
					DomNode child2 = child.Copy(originalToCopyMap);
					domNode.SetChild(child3, child2);
				}
			}
		}
		return domNode;
	}

	private void UpdateReferences(IDictionary<DomNode, DomNode> originalToCopyMap)
	{
		foreach (AttributeInfo attribute in Type.Attributes)
		{
			if (attribute.Type.Type == AttributeTypes.Reference && GetAttribute(attribute) is DomNode key && originalToCopyMap.TryGetValue(key, out var value))
			{
				SetAttribute(attribute, value);
			}
		}
		foreach (DomNode child in Children)
		{
			child.UpdateReferences(originalToCopyMap);
		}
	}

	private void RaiseAttributeChanging(AttributeEventArgs e)
	{
		foreach (DomNode item in Lineage)
		{
			if (item.m_eventHandlers != null)
			{
				item.m_eventHandlers.RaiseAttributeChanging(e);
			}
		}
	}

	internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangingHandlers()
	{
		foreach (DomNode node in Lineage)
		{
			if (node.m_eventHandlers == null)
			{
				continue;
			}
			foreach (EventHandler<AttributeEventArgs> attributeChangingHandler in node.m_eventHandlers.GetAttributeChangingHandlers())
			{
				yield return attributeChangingHandler;
			}
		}
	}

	private void RaiseAttributeChanged(AttributeEventArgs e)
	{
		foreach (DomNode item in Lineage)
		{
			if (item.m_eventHandlers != null)
			{
				item.m_eventHandlers.RaiseAttributeChanged(e);
			}
		}
	}

	internal IEnumerable<EventHandler<AttributeEventArgs>> GetAttributeChangedHandlers()
	{
		foreach (DomNode node in Lineage)
		{
			if (node.m_eventHandlers == null)
			{
				continue;
			}
			foreach (EventHandler<AttributeEventArgs> attributeChangedHandler in node.m_eventHandlers.GetAttributeChangedHandlers())
			{
				yield return attributeChangedHandler;
			}
		}
	}

	private void RaiseChildInserting(ChildEventArgs e)
	{
		foreach (DomNode item in Lineage)
		{
			if (item.m_eventHandlers != null)
			{
				item.m_eventHandlers.RaiseChildInserting(e);
			}
		}
	}

	internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertingHandlers()
	{
		foreach (DomNode node in Lineage)
		{
			if (node.m_eventHandlers == null)
			{
				continue;
			}
			foreach (EventHandler<ChildEventArgs> childInsertingHandler in node.m_eventHandlers.GetChildInsertingHandlers())
			{
				yield return childInsertingHandler;
			}
		}
	}

	private void RaiseChildInserted(ChildEventArgs e)
	{
		foreach (DomNode item in Lineage)
		{
			if (item.m_eventHandlers != null)
			{
				item.m_eventHandlers.RaiseChildInserted(e);
			}
		}
	}

	internal IEnumerable<EventHandler<ChildEventArgs>> GetChildInsertedHandlers()
	{
		foreach (DomNode node in Lineage)
		{
			if (node.m_eventHandlers == null)
			{
				continue;
			}
			foreach (EventHandler<ChildEventArgs> childInsertedHandler in node.m_eventHandlers.GetChildInsertedHandlers())
			{
				yield return childInsertedHandler;
			}
		}
	}

	private void RaiseChildRemoving(ChildEventArgs e)
	{
		foreach (DomNode item in Lineage)
		{
			if (item.m_eventHandlers != null)
			{
				item.m_eventHandlers.RaiseChildRemoving(e);
			}
		}
	}

	internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovingHandlers()
	{
		foreach (DomNode node in Lineage)
		{
			if (node.m_eventHandlers == null)
			{
				continue;
			}
			foreach (EventHandler<ChildEventArgs> childRemovingHandler in node.m_eventHandlers.GetChildRemovingHandlers())
			{
				yield return childRemovingHandler;
			}
		}
	}

	private void RaiseChildRemoved(ChildEventArgs e)
	{
		foreach (DomNode item in Lineage)
		{
			if (item.m_eventHandlers != null)
			{
				item.m_eventHandlers.RaiseChildRemoved(e);
			}
		}
	}

	internal IEnumerable<EventHandler<ChildEventArgs>> GetChildRemovedHandlers()
	{
		foreach (DomNode node in Lineage)
		{
			if (node.m_eventHandlers == null)
			{
				continue;
			}
			foreach (EventHandler<ChildEventArgs> childRemovedHandler in node.m_eventHandlers.GetChildRemovedHandlers())
			{
				yield return childRemovedHandler;
			}
		}
	}

	private NodeEventHandlers GetEventHandlers()
	{
		if (m_eventHandlers == null)
		{
			m_eventHandlers = new NodeEventHandlers(this);
		}
		return m_eventHandlers;
	}

	public override bool Equals(object obj)
	{
		DomNode domNode = obj as DomNode;
		if (domNode == null)
		{
			IAdapter adapter = obj.As<IAdapter>();
			if (adapter != null)
			{
				domNode = adapter.Adaptee as DomNode;
			}
		}
		return this == domNode;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private NodeList.ChildList GetChildListObject(ChildInfo childInfo)
	{
		int dataIndex = m_type.GetDataIndex(childInfo);
		return m_data[dataIndex] as NodeList.ChildList;
	}

	private void SetChildListObject(ChildInfo childInfo, NodeList.ChildList list)
	{
		int dataIndex = m_type.GetDataIndex(childInfo);
		m_data[dataIndex] = list;
	}

	private void SetParent(DomNode parent, ChildInfo childInfo)
	{
		m_parent = parent;
		m_childInfo = childInfo;
		if (m_parent == null)
		{
			return;
		}
		foreach (ExtensionInfo extension in Type.Extensions)
		{
			if (GetExtension(extension) is DomNodeAdapter domNodeAdapter)
			{
				domNodeAdapter.ParentNodeSetInternal();
			}
		}
	}

	public override string ToString()
	{
		if (m_type != null)
		{
			return $"0x{GetHashCode():x}, {m_type}";
		}
		return base.ToString();
	}

	public void SetTag(object key, object value)
	{
		if (m_tags == null)
		{
			m_tags = new Dictionary<object, object>();
		}
		m_tags[key] = value;
	}

	public object GetTag(object key)
	{
		object value = null;
		if (m_tags != null)
		{
			m_tags.TryGetValue(key, out value);
		}
		return value;
	}
}
