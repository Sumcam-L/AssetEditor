using System.Collections;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class DomNodeListAdapter<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : class
{
	private readonly IList<DomNode> m_nodes;

	public T this[int index]
	{
		get
		{
			return m_nodes[index].As<T>();
		}
		set
		{
			m_nodes[index] = GetNode(value);
		}
	}

	public int Count => m_nodes.Count;

	public bool IsReadOnly => false;

	public DomNodeListAdapter(DomNode node, ChildInfo childInfo)
	{
		m_nodes = node.GetChildList(childInfo);
	}

	public int IndexOf(T item)
	{
		return m_nodes.IndexOf(GetNode(item));
	}

	public void Insert(int index, T item)
	{
		m_nodes.Insert(index, GetNode(item));
	}

	public void RemoveAt(int index)
	{
		m_nodes.RemoveAt(index);
	}

	public void Add(T item)
	{
		m_nodes.Add(GetNode(item));
	}

	public void Clear()
	{
		m_nodes.Clear();
	}

	public bool Contains(T item)
	{
		return m_nodes.Contains(GetNode(item));
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		for (int i = arrayIndex; i < m_nodes.Count; i++)
		{
			array[i] = m_nodes[i].As<T>();
		}
	}

	public bool Remove(T item)
	{
		if (GetNode(item) != null)
		{
			return m_nodes.Remove(GetNode(item));
		}
		return false;
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (DomNode node in m_nodes)
		{
			yield return node.As<T>();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private DomNode GetNode(T item)
	{
		if (item == null)
		{
			return null;
		}
		DomNode domNode = item.As<DomNode>();
		if (domNode == null)
		{
			return null;
		}
		return domNode;
	}
}
