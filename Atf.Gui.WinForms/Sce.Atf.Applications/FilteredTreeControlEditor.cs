using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(FilteredTreeControlEditor))]
[PartCreationPolicy(CreationPolicy.Any)]
public class FilteredTreeControlEditor : TreeControlEditor
{
	private UserControl m_control;

	private StringSearchInputUI m_searchInput;

	private bool m_searching = false;

	private bool m_autoExpanding = false;

	private TreeControl.Node m_nodeToExpand;

	private List<object> m_expandedItems = new List<object>();

	private IEnumerable<object> m_selectedItems;

	private bool m_rememberExpansion;

	public StringSearchInputUI SearchInputUI => m_searchInput;

	public Control Control => m_control;

	public bool RestoreSubExpansion
	{
		get
		{
			return m_rememberExpansion;
		}
		set
		{
			m_rememberExpansion = value;
		}
	}

	[ImportingConstructor]
	public FilteredTreeControlEditor(ICommandService commandService)
		: base(commandService)
	{
	}

	public FilteredTreeControlEditor(ICommandService commandService, IContextMenuCommandProvider menuProvider)
		: base(commandService, menuProvider)
	{
	}

	protected override void Configure(out TreeControl treeControl, out TreeControlAdapter treeControlAdapter)
	{
		treeControl = new TreeControl();
		treeControl.ImageList = ResourceUtil.GetImageList16();
		treeControl.StateImageList = ResourceUtil.GetImageList16();
		treeControlAdapter = new TreeControlAdapter(treeControl);
		treeControl.PreviewKeyDown += treeControl_PreviewKeyDown;
		treeControl.NodeExpandedChanging += treeControl_NodeExpandedChanging;
		treeControl.NodeExpandedChanged += treeControl_NodeExpandedChanged;
		m_searchInput = new StringSearchInputUI();
		m_searchInput.Updated += UpdateFiltering;
		m_control = new UserControl();
		m_control.Dock = DockStyle.Fill;
		m_control.SuspendLayout();
		m_control.Name = "Tree View".Localize();
		m_control.Text = "Tree View".Localize();
		m_control.Controls.Add(m_searchInput);
		m_control.Controls.Add(base.TreeControl);
		m_control.Layout += controls_Layout;
		m_control.ResumeLayout();
	}

	public bool DefaultFilter(object item)
	{
		IItemView itemView = base.TreeView.As<IItemView>();
		if (itemView != null)
		{
			ItemInfo itemInfo = new WinFormsItemInfo();
			itemView.GetInfo(item, itemInfo);
			return SearchInputUI.IsNullOrEmpty() || SearchInputUI.Matches(itemInfo.Label);
		}
		return true;
	}

	protected void UpdateFiltering(object sender, EventArgs e)
	{
		if (base.TreeView == null || base.TreeControl == null || base.TreeView.Root == null)
		{
			return;
		}
		base.TreeControl.SuspendLayout();
		bool flag = !m_searchInput.IsNullOrEmpty();
		if (!m_searching && flag)
		{
			RememberExpansion();
			m_selectedItems = RememberSelection();
		}
		TreeItemRenderer itemRenderer = base.TreeControl.ItemRenderer;
		itemRenderer.FilteringPattern = m_searchInput.SearchPattern;
		if (base.TreeView is FilteredTreeView filteredTreeView)
		{
			if (flag)
			{
				filteredTreeView.BuildTreeCache();
				filteredTreeView.BuildVisibility();
				filteredTreeView.IsFiltering = true;
				if (itemRenderer.FilteringStatus == null)
				{
					itemRenderer.FilteringStatus = filteredTreeView.NodeCurrentFilteringStatus;
				}
			}
			else
			{
				filteredTreeView.IsFiltering = false;
			}
		}
		m_autoExpanding = true;
		if (flag)
		{
			base.TreeControlAdapter.TreeView = base.TreeView;
			ExpandAllMatches();
		}
		else if (m_searching)
		{
			base.TreeControlAdapter.TreeView = base.TreeView;
			IEnumerable<object> enumerable = RememberSelection();
			if (enumerable != null && enumerable.Any())
			{
				m_selectedItems = enumerable;
			}
			RestoreExpansion();
			RestoreSelection();
		}
		m_searching = flag;
		m_autoExpanding = false;
		base.TreeControl.ResumeLayout();
	}

	private void RememberExpansion()
	{
		m_expandedItems.Clear();
		foreach (object item in GetSubtree(base.TreeView.Root))
		{
			if (base.TreeControlAdapter.IsExpanded(item))
			{
				m_expandedItems.Add(item);
			}
		}
	}

	private void RestoreExpansion()
	{
		foreach (object expandedItem in m_expandedItems)
		{
			base.TreeControlAdapter.Expand(expandedItem);
		}
	}

	private void ExpandAllMatches()
	{
		Tree<object> tree = new Tree<object>(base.TreeView.Root);
		BuildTree(tree);
		foreach (Tree<object> item in tree.PreOrder)
		{
			if (!item.IsLeaf)
			{
				base.TreeControlAdapter.Expand(item.Value);
			}
		}
	}

	private void BuildTree(Tree<object> rootNode)
	{
		foreach (object child in base.TreeView.GetChildren(rootNode.Value))
		{
			Tree<object> tree = new Tree<object>(child);
			rootNode.Children.Add(tree);
			BuildTree(tree);
		}
	}

	private IEnumerable<object> GetSubtree(object root)
	{
		yield return root;
		foreach (object child in TreeView.GetChildren(root))
		{
			foreach (object item in GetSubtree(child))
			{
				yield return item;
			}
		}
	}

	private IEnumerable<object> RememberSelection()
	{
		return base.TreeView.As<ISelectionContext>()?.Selection;
	}

	private void RestoreSelection()
	{
		if (m_selectedItems == null)
		{
			return;
		}
		foreach (object selectedItem in m_selectedItems)
		{
			Path<object> path = selectedItem.As<Path<object>>();
			if (path != null)
			{
				base.TreeControlAdapter.Show(path, select: true);
			}
		}
	}

	private void controls_Layout(object sender, LayoutEventArgs e)
	{
		Rectangle clientRectangle = m_control.ClientRectangle;
		int num = 0;
		if (m_searchInput.Visible)
		{
			num = m_searchInput.Height + base.TreeControl.Margin.Top;
		}
		base.TreeControl.Bounds = new Rectangle(0, num, clientRectangle.Width, clientRectangle.Height - num);
	}

	private void treeControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Escape && m_searching)
		{
			SearchInputUI.ClearSearch();
		}
	}

	private void treeControl_NodeExpandedChanging(object sender, TreeControl.CancelNodeEventArgs e)
	{
		if (!(base.TreeView is FilteredTreeView filteredTreeView) || m_autoExpanding || !m_searching)
		{
			return;
		}
		if (filteredTreeView.IsNodeOpaque(e.Node) && filteredTreeView.IsNodeCurrentlyOpaque(e.Node))
		{
			IEnumerable<object> children = filteredTreeView.GetChildren(e.Node.Tag);
			if (!children.Any())
			{
				if (e.Node.Expanded)
				{
					return;
				}
				{
					foreach (object unfilteredChild in filteredTreeView.GetUnfilteredChildren(e.Node.Tag))
					{
						filteredTreeView.AddCurrentVisibleNode(unfilteredChild);
					}
					return;
				}
			}
			if (!e.Node.Expanded)
			{
				return;
			}
			bool flag = false;
			foreach (object unfilteredChild2 in filteredTreeView.GetUnfilteredChildren(e.Node.Tag))
			{
				if (filteredTreeView.AddCurrentVisibleNode(unfilteredChild2))
				{
					flag = true;
				}
			}
			filteredTreeView.RemoveOpaqueNode(e.Node);
			filteredTreeView.RememberExpansion(e.Node);
			if (flag)
			{
				m_nodeToExpand = e.Node;
			}
			return;
		}
		if (filteredTreeView.IsNodeOpaque(e.Node) && !filteredTreeView.IsNodeCurrentlyOpaque(e.Node) && e.Node.Expanded)
		{
			filteredTreeView.AddOpaqueNode(e.Node);
			filteredTreeView.RememberExpansion(e.Node);
			{
				foreach (TreeControl.Node child in e.Node.Children)
				{
					if (!filteredTreeView.IsNodeMatched(child))
					{
						filteredTreeView.RemoveVisibleNode(child.Tag);
					}
				}
				return;
			}
		}
		if (e.Node.Expanded)
		{
			filteredTreeView.RememberExpansion(e.Node);
		}
		else
		{
			if (filteredTreeView.IsNodeOpaque(e.Node))
			{
				return;
			}
			foreach (object unfilteredChild3 in filteredTreeView.GetUnfilteredChildren(e.Node.Tag))
			{
				filteredTreeView.AddCurrentVisibleNode(unfilteredChild3);
			}
		}
	}

	private void treeControl_NodeExpandedChanged(object sender, TreeControl.NodeEventArgs e)
	{
		if (m_searching && base.TreeView is FilteredTreeView filteredTreeView)
		{
			if (m_nodeToExpand != null)
			{
				TreeControl.Node nodeToExpand = m_nodeToExpand;
				m_autoExpanding = true;
				base.TreeControlAdapter.Expand(m_nodeToExpand.Tag);
				filteredTreeView.RestoreExpansion(base.TreeControlAdapter, nodeToExpand);
				m_nodeToExpand = null;
				m_autoExpanding = false;
			}
			else if (e.Node.Expanded && RestoreSubExpansion)
			{
				filteredTreeView.RestoreExpansion(base.TreeControlAdapter, e.Node);
			}
		}
	}
}
