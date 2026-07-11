using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

public class FilteredTreeView : ITreeView, IAdaptable, IDecoratable
{
	private readonly ITreeView m_treeView;

	private Tree<object> m_fullTree;

	private readonly Predicate<object> m_filterFunc;

	private HashSet<object> m_visibleNodes = new HashSet<object>();

	private HashSet<object> m_currentVisibleNodes = new HashSet<object>();

	private HashSet<object> m_opaqueNodes = new HashSet<object>();

	private HashSet<object> m_currentOpaqueNodes;

	private Dictionary<object, List<object>> m_expandedItems = new Dictionary<object, List<object>>();

	public object Root => m_treeView.Root;

	public bool IsFiltering { get; set; }

	public FilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
	{
		m_treeView = treeView;
		m_filterFunc = filterFunc;
	}

	public static bool Equals(ITreeView first, ITreeView second)
	{
		FilteredTreeView filteredTreeView = first.As<FilteredTreeView>();
		if (filteredTreeView != null)
		{
			first = filteredTreeView.m_treeView;
		}
		FilteredTreeView filteredTreeView2 = second.As<FilteredTreeView>();
		if (filteredTreeView2 != null)
		{
			second = filteredTreeView2.m_treeView;
		}
		return first == second;
	}

	public IEnumerable<object> GetChildren(object parent)
	{
		IEnumerable<object> children = m_treeView.GetChildren(parent);
		if (IsFiltering)
		{
			return children.Intersect(m_currentVisibleNodes);
		}
		return children;
	}

	public object GetAdapter(Type type)
	{
		if (typeof(FilteredTreeView).IsAssignableFrom(type))
		{
			return this;
		}
		object obj = m_treeView.As(type);
		if (obj != null)
		{
			return obj;
		}
		return m_treeView;
	}

	public IEnumerable<object> GetDecorators(Type type)
	{
		object adapter = GetAdapter(type);
		if (adapter != null)
		{
			yield return adapter;
		}
	}

	internal void BuildTreeCache()
	{
		m_fullTree = null;
		if (m_treeView.Root != null)
		{
			m_fullTree = new Tree<object>(m_treeView.Root);
			BuildTree(m_fullTree);
		}
	}

	private void BuildTree(Tree<object> rootNode)
	{
		foreach (object child in m_treeView.GetChildren(rootNode.Value))
		{
			Tree<object> tree = new Tree<object>(child);
			tree.Parent = rootNode;
			BuildTree(tree);
		}
	}

	internal void BuildVisibility()
	{
		m_visibleNodes.Clear();
		m_opaqueNodes.Clear();
		if (m_fullTree == null)
		{
			return;
		}
		foreach (Tree<object> item in m_fullTree.PreOrder)
		{
			if (m_filterFunc(item.Value))
			{
				Tree<object> tree = item;
				while (tree != null && !m_visibleNodes.Contains(tree.Value))
				{
					m_visibleNodes.Add(tree.Value);
					tree = tree.Parent;
				}
			}
		}
		m_currentVisibleNodes = new HashSet<object>(m_visibleNodes);
		BuildOpacity(m_fullTree);
		m_currentOpaqueNodes = new HashSet<object>(m_opaqueNodes);
	}

	internal bool IsNodeCurrentlyOpaque(TreeControl.Node node)
	{
		if (node.Tag == null)
		{
			return false;
		}
		return m_currentOpaqueNodes.Contains(node.Tag);
	}

	internal bool IsNodeOpaque(TreeControl.Node node)
	{
		if (node.Tag == null)
		{
			return false;
		}
		return m_opaqueNodes.Contains(node.Tag);
	}

	internal bool IsNodeMatched(TreeControl.Node node)
	{
		if (node.Tag == null)
		{
			return false;
		}
		return m_visibleNodes.Contains(node.Tag);
	}

	internal TreeItemRenderer.NodeFilteringStatus NodeCurrentFilteringStatus(TreeControl.Node node)
	{
		TreeItemRenderer.NodeFilteringStatus nodeFilteringStatus = TreeItemRenderer.NodeFilteringStatus.Normal;
		if (node.Tag != null)
		{
			if (m_currentOpaqueNodes.Contains(node.Tag))
			{
				nodeFilteringStatus |= TreeItemRenderer.NodeFilteringStatus.PartiallyExpanded;
			}
			if (m_visibleNodes.Contains(node.Tag))
			{
				nodeFilteringStatus |= TreeItemRenderer.NodeFilteringStatus.Visible;
			}
			if (GetUnfilteredChildren(node.Tag).Any((object i) => m_visibleNodes.Contains(i)))
			{
				nodeFilteringStatus |= TreeItemRenderer.NodeFilteringStatus.ChildVisible;
			}
		}
		return nodeFilteringStatus;
	}

	internal void RemoveOpaqueNode(TreeControl.Node node)
	{
		if (m_opaqueNodes.Contains(node.Tag))
		{
			m_currentOpaqueNodes.Remove(node.Tag);
		}
	}

	internal void AddOpaqueNode(TreeControl.Node node)
	{
		if (m_opaqueNodes.Contains(node.Tag))
		{
			m_currentOpaqueNodes.Add(node.Tag);
		}
	}

	private void BuildOpacity(Tree<object> parent)
	{
		int num = 0;
		foreach (Tree<object> child in parent.Children)
		{
			if (!m_visibleNodes.Contains(child.Value))
			{
				num++;
			}
		}
		if (num > 0 && num < parent.Children.Count && !m_opaqueNodes.Contains(parent.Value))
		{
			m_opaqueNodes.Add(parent.Value);
		}
		foreach (Tree<object> child2 in parent.Children)
		{
			BuildOpacity(child2);
		}
	}

	internal bool AddCurrentVisibleNode(object item)
	{
		if (item != null && !m_currentVisibleNodes.Contains(item))
		{
			return m_currentVisibleNodes.Add(item);
		}
		return false;
	}

	internal void RemoveVisibleNode(object item)
	{
		if (item != null && m_currentVisibleNodes.Contains(item))
		{
			m_currentVisibleNodes.Remove(item);
		}
	}

	public void RememberExpansion(TreeControl.Node parent)
	{
		if (parent.Tag == null)
		{
			return;
		}
		List<object> list = new List<object>();
		foreach (TreeControl.Node item in GetSubtree(parent))
		{
			if (item.Expanded)
			{
				list.Add(item.Tag);
			}
		}
		m_expandedItems[parent.Tag] = list;
	}

	public void RestoreExpansion(TreeControlAdapter treeControlAdapter, TreeControl.Node parent)
	{
		if (parent.Tag == null || !m_expandedItems.ContainsKey(parent.Tag))
		{
			return;
		}
		foreach (object item in m_expandedItems[parent.Tag])
		{
			treeControlAdapter.Expand(item);
		}
	}

	private IEnumerable<TreeControl.Node> GetSubtree(TreeControl.Node parent)
	{
		yield return parent;
		foreach (TreeControl.Node child in parent.Children)
		{
			foreach (TreeControl.Node item in GetSubtree(child))
			{
				yield return item;
			}
		}
	}

	internal IEnumerable<object> GetUnfilteredChildren(object parent)
	{
		return m_treeView.GetChildren(parent);
	}
}
