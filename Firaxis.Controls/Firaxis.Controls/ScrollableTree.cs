using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Collections;
using Firaxis.Controls.Scrollables;
using Firaxis.Utility;

namespace Firaxis.Controls;

[Description("Tree that can contain custom display items")]
public class ScrollableTree : ScrollUserControl
{
	public class TreeNode : IDisposable
	{
		private bool disposedValue = false;

		public TreeNode Parent { get; internal set; }

		public IScrollableItem Item { get; private set; }

		public ScrollableTree Owner { get; private set; }

		public TreeNodeCollection Children { get; private set; }

		internal TreeNode(ScrollableTree owner)
		{
			Owner = owner;
			Children = new TreeNodeCollection(owner, isSelectionCollection: false);
		}

		internal TreeNode(ScrollableTree owner, IScrollableItem item)
		{
			Owner = owner;
			Item = item;
			Children = new TreeNodeCollection(owner, isSelectionCollection: false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Item?.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}

	public class TreeNodeCollection : ListEvent<TreeNode>
	{
		private ScrollableTree owner;

		internal bool IsSelectionCollection { get; private set; }

		internal TreeNodeCollection(ScrollableTree owner, bool isSelectionCollection)
		{
			this.owner = owner;
			base.ItemCountChanged += TreeNodeCollection_ElementCountChanged;
			base.RemovedItem += TreeNodeCollection_RemovedElement;
			IsSelectionCollection = isSelectionCollection;
		}

		private void TreeNodeCollection_RemovedElement(object sender, ListEventArgs e)
		{
			if (!owner.IsUpdating)
			{
				if (!IsSelectionCollection)
				{
					owner.RemovedElement(e.Item);
				}
				owner.Invalidate();
			}
		}

		private void TreeNodeCollection_ElementCountChanged(object sender, EventArgs e)
		{
			if (!owner.IsUpdating)
			{
				if (!IsSelectionCollection)
				{
					owner.RebuildDisplayList();
				}
				owner.Invalidate();
			}
		}
	}

	public class DisplayTreeNode
	{
		public int Origin;

		public int Level;

		public TreeNode Node;

		public DisplayTreeNode(int origin, TreeNode node, int level)
		{
			Origin = origin;
			Level = level;
			Node = node;
		}
	}

	public class DisplayTreeNodeCollection : List<DisplayTreeNode>
	{
	}

	public class TreeNodeEventArgs : EventArgs
	{
		public static readonly TreeNodeEventArgs EmptyEvent;

		public TreeNode Node { get; private set; }

		public TreeNodeEventArgs()
		{
		}

		public TreeNodeEventArgs(TreeNode node)
		{
			Node = node;
		}
	}

	public delegate void TreeNodeEventHandler(object sender, TreeNodeEventArgs e);

	public delegate bool DisplayNodeFiterHandler(TreeNode node);

	private DisplayTreeNodeCollection display;

	private ScrollableItemPaintEventArgs paint_args;

	private TreeNode previousSelection;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public DisplayNodeFiterHandler DisplayFilter { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool EnableExpandDoubleClick { get; set; }

	public TreeNodeCollection Root { get; private set; }

	public DisplayTreeNodeCollection DisplayNodes => display;

	public TreeNodeCollection SelectedNodes { get; private set; }

	public bool IsUpdating { get; private set; }

	private TreeNode FirstSelected
	{
		get
		{
			if (SelectedNodes.Count > 0)
			{
				return SelectedNodes[0];
			}
			return null;
		}
	}

	public event TreeNodeEventHandler DisplayListChanged;

	public event TreeNodeEventHandler ContextMenuItem;

	public event TreeNodeEventHandler ExpandedItemChanged;

	public event TreeNodeEventHandler SelectedItemChanged;

	public event TreeNodeEventHandler DoubleClickLeafItem;

	public ScrollableTree()
	{
		paint_args = new ScrollableItemPaintEventArgs();
		display = new DisplayTreeNodeCollection();
		Root = new TreeNodeCollection(this, isSelectionCollection: false);
		SelectedNodes = new TreeNodeCollection(this, isSelectionCollection: true);
		SelectedNodes.ItemCountChanged += SelectedNodes_ElementCountChanged;
		InitializeComponent();
		EnableExpandDoubleClick = true;
		DoubleBuffered = true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Clear();
		}
		base.Dispose(disposing);
	}

	public TreeNode Add(IScrollableItem item)
	{
		return Add(null, Root, item);
	}

	public TreeNode Add(TreeNode parent, IScrollableItem item)
	{
		if (parent == null)
		{
			return Add(item);
		}
		return Add(parent, parent.Children, item);
	}

	public TreeNode Add(string label)
	{
		return Add(null, Root, new ScrollableItemTree(label, Font));
	}

	public TreeNode Add(TreeNode parent, string label)
	{
		if (parent == null)
		{
			return Add(label);
		}
		return Add(parent, parent.Children, new ScrollableItemTree(label, Font));
	}

	private TreeNode Add(TreeNode parent, TreeNodeCollection nodes, IScrollableItem item)
	{
		TreeNode treeNode = new TreeNode(this, item);
		treeNode.Parent = parent;
		nodes.Add(treeNode);
		return treeNode;
	}

	private void SelectedNodes_ElementCountChanged(object sender, EventArgs e)
	{
		TreeNode treeNode = ((SelectedNodes.Count > 0) ? SelectedNodes[0] : null);
		if (treeNode != previousSelection)
		{
			previousSelection = treeNode;
			this.SelectedItemChanged?.Invoke(this, new TreeNodeEventArgs(treeNode));
			RebuildDisplayList();
			Invalidate();
		}
	}

	public void Clear()
	{
		Clear(Root);
	}

	private void Clear(TreeNodeCollection nodes)
	{
		foreach (TreeNode node in nodes)
		{
			Clear(node.Children);
			node?.Dispose();
		}
		nodes.Clear();
	}

	public void BeginUpdate()
	{
		if (IsUpdating)
		{
			throw new Exception("Already updating");
		}
		IsUpdating = true;
	}

	public void EndUpdate()
	{
		if (!IsUpdating)
		{
			throw new Exception("Not Updating");
		}
		IsUpdating = false;
		RebuildDisplayList();
		Invalidate();
	}

	private void RemovedElement(TreeNode node)
	{
		if (SelectedNodes.Contains(node))
		{
			SelectedNodes.Remove(node);
		}
	}

	private void RebuildDisplayList()
	{
		if (IsUpdating)
		{
			return;
		}
		display.Clear();
		int maxValue = 0;
		int level = 0;
		SizeF size = new SizeF(base.ClientSize.Width, base.ClientSize.Height);
		using (Graphics g = Graphics.FromHwnd(base.Handle))
		{
			foreach (TreeNode item in Root)
			{
				RebuildDisplayList(g, item, ref level, ref maxValue, ref size);
			}
		}
		if (previousSelection != null)
		{
			TreeNode treeNode = previousSelection;
			while (treeNode.Parent != null)
			{
				treeNode = treeNode.Parent;
			}
			if (Root.IndexOf(treeNode) == -1)
			{
				SelectedNodes.Clear();
			}
		}
		base.VerticalScroll.MaxValue = maxValue;
		this.DisplayListChanged?.Invoke(this, TreeNodeEventArgs.EmptyEvent);
	}

	public bool IsSelectedVisible()
	{
		foreach (TreeNode node in SelectedNodes)
		{
			if (display.Find((DisplayTreeNode n) => n.Node == node) != null)
			{
				return true;
			}
		}
		return false;
	}

	private void RebuildDisplayList(Graphics g, TreeNode node, ref int level, ref int y, ref SizeF size)
	{
		if (DisplayFilter != null && !DisplayFilter(node))
		{
			return;
		}
		display.Add(new DisplayTreeNode(y, node, level));
		node.Item.CalcLayout(g, size);
		y += node.Item.ItemHeight;
		if (!node.Item.Visible)
		{
			return;
		}
		level++;
		foreach (TreeNode child in node.Children)
		{
			RebuildDisplayList(g, child, ref level, ref y, ref size);
		}
		level--;
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.Name = "ScrollableTree";
		base.Size = new System.Drawing.Size(269, 189);
		base.Paint += new System.Windows.Forms.PaintEventHandler(ScrollableTree_Paint);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(ScrollableTree_MouseMove);
		base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(ScrollableTree_MouseDoubleClick);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(ScrollableTree_MouseDown);
		base.SizeChanged += new System.EventHandler(ScrollableTree_SizeChanged);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(ScrollableTree_KeyDown);
		base.ResumeLayout(false);
	}

	private void ScrollableTree_Paint(object sender, PaintEventArgs e)
	{
		PaintItems(e.Graphics);
	}

	private void PaintItems(Graphics g)
	{
		if (IsUpdating)
		{
			return;
		}
		using SolidBrush brush = new SolidBrush(BackColor);
		int value = base.VerticalScroll.Value;
		int num = value + base.ClientSize.Height;
		g.FillRectangle(brush, base.ClientRectangle);
		Rectangle r = new Rectangle(0, 0, base.ClientSize.Width, 0);
		foreach (DisplayTreeNode item in display)
		{
			if (item.Origin + item.Node.Item.ItemHeight >= value || item.Origin < num)
			{
				r.Y = item.Origin - value;
				r.Height = item.Node.Item.ItemHeight;
				PaintItem(g, item, r);
			}
		}
	}

	public bool HasVisibleChildren(TreeNode node)
	{
		if (node != null)
		{
			foreach (TreeNode child in node.Children)
			{
				if (DisplayFilter == null || DisplayFilter(child))
				{
					return true;
				}
			}
		}
		return false;
	}

	public ScrollableItemStyle GetNodeStyle(TreeNode node)
	{
		if (HasVisibleChildren(node))
		{
			return node.Item.Visible ? ScrollableItemStyle.Expanded : ScrollableItemStyle.Collapsed;
		}
		return ScrollableItemStyle.Leaf;
	}

	public void ExpandSelectNode(TreeNode node)
	{
		IScrollableItem item = node.Parent.Item;
		item.Visible = true;
		if (item.Tag is IExpandable)
		{
			((IExpandable)item.Tag).Expanded = true;
		}
		RebuildDisplayList();
		SelectNode(node);
		Invalidate();
	}

	public void ExpandNode(TreeNode node, bool bRecursive)
	{
		IScrollableItem item = node.Item;
		item.Visible = true;
		if (item.Tag is IExpandable)
		{
			((IExpandable)item.Tag).Expanded = true;
		}
		if (bRecursive)
		{
			SetChildNodeExpand(node, bState: true, bRecursive: true);
		}
		RebuildDisplayList();
		Invalidate();
	}

	public void CollapseNode(TreeNode node, bool bRecursive)
	{
		IScrollableItem item = node.Item;
		item.Visible = false;
		if (item.Tag is IExpandable)
		{
			((IExpandable)item.Tag).Expanded = false;
		}
		if (bRecursive)
		{
			SetChildNodeExpand(node, bState: false, bRecursive: true);
		}
		RebuildDisplayList();
		Invalidate();
	}

	private void SetChildNodeExpand(TreeNode parent, bool bState, bool bRecursive)
	{
		foreach (TreeNode child in parent.Children)
		{
			IScrollableItem item = child.Item;
			item.Visible = bState;
			if (item.Tag is IExpandable)
			{
				((IExpandable)item.Tag).Expanded = bState;
			}
			if (bRecursive)
			{
				SetChildNodeExpand(child, bState, bRecursive: true);
			}
		}
	}

	public void SelectNode(TreeNode node)
	{
		SelectedNodes.Clear();
		if (node != null)
		{
			SelectedNodes.Add(node);
		}
	}

	public Rectangle GetBounds(TreeNode node)
	{
		foreach (DisplayTreeNode item in display)
		{
			int value = base.VerticalScroll.Value;
			if (item.Node == node)
			{
				Rectangle result = new Rectangle(0, 0, base.ClientSize.Width, 0);
				result.Y = item.Origin - value;
				result.Height = item.Node.Item.ItemHeight;
				return result;
			}
		}
		return Rectangle.Empty;
	}

	private DisplayTreeNode FindDisplayNode(int x, int y)
	{
		x += base.HorizontalScroll.Value;
		y += base.VerticalScroll.Value;
		foreach (DisplayTreeNode item in display)
		{
			if (y >= item.Origin && y < item.Origin + item.Node.Item.ItemHeight)
			{
				return item;
			}
		}
		return null;
	}

	public TreeNode FindNode(Predicate<TreeNode> match)
	{
		return FindNode(match, Root);
	}

	public void Iterate(Predicate<TreeNode> action)
	{
		FindNode(action, Root);
	}

	private TreeNode FindNode(Predicate<TreeNode> match, TreeNodeCollection nodes)
	{
		if (nodes != null)
		{
			foreach (TreeNode node in nodes)
			{
				if (match(node))
				{
					return node;
				}
				TreeNode result;
				if ((result = FindNode(match, node.Children)) != null)
				{
					return result;
				}
			}
		}
		return null;
	}

	public TreeNode FindNode(int x, int y)
	{
		return FindDisplayNode(x, y)?.Node;
	}

	public void EnsureVisible(TreeNode node)
	{
		BeginUpdate();
		for (TreeNode treeNode = node; treeNode != null; treeNode = treeNode.Parent)
		{
			treeNode.Item.Visible = true;
		}
		EndUpdate();
		int displayIndex = GetDisplayIndex(node);
		if (displayIndex != -1)
		{
			DisplayTreeNode displayTreeNode = display[displayIndex];
			if (displayTreeNode.Origin <= base.VerticalScroll.Value)
			{
				base.VerticalScroll.Value = displayTreeNode.Origin;
			}
			else if (displayTreeNode.Origin + displayTreeNode.Node.Item.ItemHeight >= base.VerticalScroll.Value + base.ClientSize.Height)
			{
				base.VerticalScroll.Value = displayTreeNode.Origin + displayTreeNode.Node.Item.ItemHeight - base.ClientSize.Height;
			}
		}
		Invalidate();
	}

	private int GetDisplayIndex(TreeNode node)
	{
		int num = 0;
		foreach (DisplayTreeNode item in display)
		{
			if (item.Node == node)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private void PaintItem(Graphics g, DisplayTreeNode node, Rectangle r)
	{
		ScrollableItemState scrollableItemState = (SelectedNodes.Contains(node.Node) ? ScrollableItemState.Selected : ScrollableItemState.Normal);
		paint_args.Graphics = g;
		paint_args.Bounds = r;
		paint_args.State = scrollableItemState;
		paint_args.Level = node.Level;
		paint_args.Style = GetNodeStyle(node.Node);
		node.Node.Item.PaintItem(this, paint_args);
	}

	private void ScrollableTree_SizeChanged(object sender, EventArgs e)
	{
		RebuildDisplayList();
		Invalidate();
	}

	private void ScrollableTree_MouseDown(object sender, MouseEventArgs e)
	{
		Focus();
		if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
		{
			return;
		}
		TreeNode treeNode = FindHitExpand(e.X, e.Y);
		if (treeNode != null)
		{
			IScrollableItem item = treeNode.Item;
			item.Visible = !item.Visible;
			if (item.Tag is IExpandable)
			{
				((IExpandable)item.Tag).Expanded = item.Visible;
				this.ExpandedItemChanged?.Invoke(this, new TreeNodeEventArgs(treeNode));
			}
			RebuildDisplayList();
			if (!IsSelectedVisible())
			{
				SelectedNodes.Clear();
			}
			Invalidate();
			return;
		}
		treeNode = FindNode(e.X, e.Y);
		if (treeNode != null)
		{
			if (treeNode != previousSelection)
			{
				SelectedNodes.Clear();
				SelectedNodes.Add(treeNode);
			}
			if (e.Button == MouseButtons.Right)
			{
				this.ContextMenuItem?.Invoke(this, new TreeNodeEventArgs(treeNode));
			}
		}
		else
		{
			SelectedNodes.Clear();
		}
	}

	private void ScrollableTree_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		Focus();
		if (e.Button != MouseButtons.Left || FindHitExpand(e.X, e.Y) != null)
		{
			return;
		}
		TreeNode treeNode = FindNode(e.X, e.Y);
		if (treeNode == null)
		{
			return;
		}
		if (treeNode.Children.Count > 0)
		{
			if (EnableExpandDoubleClick)
			{
				treeNode.Item.Visible = !treeNode.Item.Visible;
				if (treeNode.Item.Tag is IExpandable)
				{
					((IExpandable)treeNode.Item.Tag).Expanded = treeNode.Item.Visible;
					this.ExpandedItemChanged?.Invoke(this, new TreeNodeEventArgs(treeNode));
				}
				RebuildDisplayList();
			}
			if (!IsSelectedVisible())
			{
				SelectedNodes.Clear();
			}
			Invalidate();
		}
		else
		{
			this.DoubleClickLeafItem?.Invoke(this, new TreeNodeEventArgs(treeNode));
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		switch (keyData)
		{
		case Keys.End:
		case Keys.Home:
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
			return true;
		default:
			return base.IsInputKey(keyData);
		}
	}

	private void ScrollableTree_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Home:
			if (display.Count > 0)
			{
				TreeNode node5 = display[0].Node;
				SelectNode(node5);
				EnsureVisible(node5);
			}
			break;
		case Keys.Left:
		{
			if (display.Count <= 0)
			{
				break;
			}
			int displayIndex = GetDisplayIndex(FirstSelected);
			TreeNode node2;
			if (displayIndex != -1)
			{
				node2 = display[displayIndex].Node;
				if (node2.Item.Visible && node2.Children.Count > 0)
				{
					node2.Item.Visible = false;
					RebuildDisplayList();
					Invalidate();
					break;
				}
			}
			node2 = display[displayIndex switch
			{
				0 => 0, 
				-1 => display.Count - 1, 
				_ => displayIndex - 1, 
			}].Node;
			SelectNode(node2);
			EnsureVisible(node2);
			break;
		}
		case Keys.Right:
		{
			if (display.Count <= 0)
			{
				break;
			}
			int displayIndex3 = GetDisplayIndex(FirstSelected);
			TreeNode node4;
			if (displayIndex3 != -1)
			{
				node4 = display[displayIndex3].Node;
				if (!node4.Item.Visible && node4.Children.Count > 0)
				{
					node4.Item.Visible = true;
					RebuildDisplayList();
					Invalidate();
					break;
				}
			}
			node4 = display[(displayIndex3 != -1) ? ((displayIndex3 == display.Count - 1) ? (display.Count - 1) : (displayIndex3 + 1)) : 0].Node;
			SelectNode(node4);
			EnsureVisible(node4);
			break;
		}
		case Keys.Down:
			if (display.Count > 0)
			{
				int displayIndex4 = GetDisplayIndex(FirstSelected);
				TreeNode node6 = display[(displayIndex4 != -1) ? ((displayIndex4 == display.Count - 1) ? (display.Count - 1) : (displayIndex4 + 1)) : 0].Node;
				SelectNode(node6);
				EnsureVisible(node6);
			}
			break;
		case Keys.Up:
			if (display.Count > 0)
			{
				int displayIndex2 = GetDisplayIndex(FirstSelected);
				TreeNode node3 = display[displayIndex2 switch
				{
					0 => 0, 
					-1 => display.Count - 1, 
					_ => displayIndex2 - 1, 
				}].Node;
				SelectNode(node3);
				EnsureVisible(node3);
			}
			break;
		case Keys.End:
			if (display.Count > 0)
			{
				TreeNode node = display[display.Count - 1].Node;
				SelectNode(node);
				EnsureVisible(node);
			}
			break;
		}
	}

	private TreeNode FindHitExpand(int x, int y)
	{
		DisplayTreeNode displayTreeNode = FindDisplayNode(x, y);
		if (displayTreeNode != null && displayTreeNode.Node.Item is IScrollableItemTree scrollableItemTree && scrollableItemTree.HitExpand(x, displayTreeNode.Origin - base.VerticalScroll.Value))
		{
			return displayTreeNode.Node;
		}
		return null;
	}

	private void ScrollableTree_MouseMove(object sender, MouseEventArgs e)
	{
		if (FindHitExpand(e.X, e.Y) != null)
		{
			Cursor = Cursors.Hand;
		}
		else
		{
			Cursor = Cursors.Default;
		}
	}
}
