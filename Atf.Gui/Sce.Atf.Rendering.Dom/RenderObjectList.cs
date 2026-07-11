using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf.Rendering.Dom;

internal class RenderObjectList : ICollection<IRenderObject>, IEnumerable<IRenderObject>, IEnumerable
{
	private readonly LinkedList<IRenderObject> m_list = new LinkedList<IRenderObject>();

	public bool IsReadOnly => false;

	public int Count => m_list.Count;

	internal LinkedList<IRenderObject> InternalList => m_list;

	public IEnumerator<IRenderObject> GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_list).GetEnumerator();
	}

	public void Add(IRenderObject toAdd)
	{
		Type[] dependencies = toAdd.GetDependencies();
		if (dependencies.Length == 0)
		{
			m_list.AddFirst(toAdd);
			return;
		}
		LinkedListNode<IRenderObject> linkedListNode;
		for (linkedListNode = m_list.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (DependsOn(toAdd, dependencies, linkedListNode.Value))
			{
				m_list.AddAfter(linkedListNode, toAdd);
				break;
			}
		}
		if (linkedListNode == null)
		{
			m_list.AddFirst(toAdd);
			return;
		}
		LinkedListNode<IRenderObject> linkedListNode3;
		LinkedListNode<IRenderObject> linkedListNode2 = (linkedListNode3 = linkedListNode.Next);
		LinkedListNode<IRenderObject> linkedListNode4 = m_list.First;
		while (linkedListNode4 != linkedListNode2)
		{
			LinkedListNode<IRenderObject> next = linkedListNode4.Next;
			LinkedListNode<IRenderObject> linkedListNode5 = linkedListNode2;
			while (true)
			{
				if (DependsOn(linkedListNode4.Value, linkedListNode4.Value.GetDependencies(), linkedListNode5.Value))
				{
					LinkedListNode<IRenderObject> linkedListNode6 = linkedListNode4;
					m_list.Remove(linkedListNode6);
					m_list.AddAfter(linkedListNode3, linkedListNode6);
					linkedListNode3 = linkedListNode6;
					break;
				}
				if (linkedListNode5 == linkedListNode3)
				{
					break;
				}
				linkedListNode5 = linkedListNode5.Next;
			}
			linkedListNode4 = next;
		}
	}

	public void Clear()
	{
		m_list.Clear();
	}

	public bool Contains(IRenderObject obj)
	{
		return m_list.Contains(obj);
	}

	public void CopyTo(IRenderObject[] array, int arrayIndex)
	{
		m_list.CopyTo(array, arrayIndex);
	}

	public bool Remove(IRenderObject obj)
	{
		return m_list.Remove(obj);
	}

	private bool DependsOn(IRenderObject a, Type[] dependencies, IRenderObject b)
	{
		for (int i = 0; i < dependencies.Length; i++)
		{
			if (dependencies[i].IsAssignableFrom(b.GetType()))
			{
				return true;
			}
		}
		return false;
	}
}
