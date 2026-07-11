using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sce.Atf;

[Serializable]
public class Tree<T>
{
	private class ChildCollection : Collection<Tree<T>>
	{
		private readonly Tree<T> m_parent;

		public ChildCollection(Tree<T> parent)
		{
			m_parent = parent;
		}

		protected override void InsertItem(int index, Tree<T> item)
		{
			item.m_parent = m_parent;
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			base.Items[index].m_parent = null;
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, Tree<T> item)
		{
			base.Items[index].m_parent = null;
			item.m_parent = m_parent;
			base.SetItem(index, item);
		}

		protected override void ClearItems()
		{
			foreach (Tree<T> item in base.Items)
			{
				item.m_parent = null;
			}
			base.ClearItems();
		}
	}

	private T m_value;

	private Tree<T> m_parent;

	private readonly ChildCollection m_children;

	public Tree<T> Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			if (m_parent != value)
			{
				if (m_parent != null)
				{
					m_parent.Children.Remove(this);
				}
				m_parent = value;
				if (m_parent != null)
				{
					m_parent.Children.Add(this);
				}
			}
		}
	}

	public IList<Tree<T>> Children => m_children;

	public T Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public bool IsLeaf => m_children.Count == 0;

	public int Level
	{
		get
		{
			int num = 0;
			for (Tree<T> parent = m_parent; parent != null; parent = parent.Parent)
			{
				num++;
			}
			return num;
		}
	}

	public int DescendantCount
	{
		get
		{
			int num = 0;
			foreach (Tree<T> item in PreOrder)
			{
				num++;
			}
			return num;
		}
	}

	public IEnumerable<Tree<T>> PreOrder
	{
		get
		{
			Stack<Tree<T>> nodes = new Stack<Tree<T>>();
			nodes.Push(this);
			while (nodes.Count > 0)
			{
				Tree<T> node = nodes.Pop();
				yield return node;
				for (int i = node.m_children.Count - 1; i >= 0; i--)
				{
					nodes.Push(node.m_children[i]);
				}
			}
		}
	}

	public IEnumerable<Tree<T>> PostOrder
	{
		get
		{
			Stack<Tree<T>> nodes = new Stack<Tree<T>>();
			nodes.Push(this);
			if (!IsLeaf)
			{
				nodes.Push(this);
			}
			while (nodes.Count > 1)
			{
				Tree<T> node = nodes.Pop();
				if (node != nodes.Peek())
				{
					yield return node;
					continue;
				}
				for (int i = node.m_children.Count - 1; i >= 0; i--)
				{
					Tree<T> child = node.m_children[i];
					nodes.Push(child);
					if (!child.IsLeaf)
					{
						nodes.Push(child);
					}
				}
			}
			yield return nodes.Pop();
		}
	}

	public IEnumerable<Tree<T>> LevelOrder
	{
		get
		{
			Queue<Tree<T>> nodes = new Queue<Tree<T>>();
			nodes.Enqueue(this);
			while (nodes.Count > 0)
			{
				Tree<T> node = nodes.Dequeue();
				yield return node;
				foreach (Tree<T> child in node.m_children)
				{
					nodes.Enqueue(child);
				}
			}
		}
	}

	public Tree()
		: this(default(T))
	{
	}

	public Tree(T value)
	{
		m_value = value;
		m_children = new ChildCollection(this);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Tree<T> tree))
		{
			return false;
		}
		ref T value = ref m_value;
		object obj2 = tree.m_value;
		if (!value.Equals(obj2))
		{
			return false;
		}
		if (m_children.Count != tree.m_children.Count)
		{
			return false;
		}
		for (int i = 0; i < m_children.Count; i++)
		{
			if (!m_children[i].Equals(tree.m_children[i]))
			{
				return false;
			}
		}
		return true;
	}

	public bool Similar(Tree<T> other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (m_children.Count != other.m_children.Count)
		{
			return false;
		}
		for (int i = 0; i < m_children.Count; i++)
		{
			if (!m_children[i].Similar(other.m_children[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = 0;
		foreach (Tree<T> item in PreOrder)
		{
			num ^= item.Value.GetHashCode();
		}
		return num;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('(');
		Stringify(stringBuilder);
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	private void Stringify(StringBuilder builder)
	{
		builder.Append(m_value.ToString());
		if (IsLeaf)
		{
			return;
		}
		builder.Append('(');
		bool flag = true;
		foreach (Tree<T> child in m_children)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				builder.Append(',');
			}
			child.Stringify(builder);
		}
		builder.Append(')');
	}

	public bool IsDescendantOf(Tree<T> ancestor)
	{
		for (Tree<T> tree = this; tree != null; tree = tree.Parent)
		{
			if (ancestor == tree)
			{
				return true;
			}
		}
		return false;
	}
}
