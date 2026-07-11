using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[InheritedExport(typeof(TreeListViewEditor))]
[PartCreationPolicy(CreationPolicy.Any)]
public abstract class TreeListViewEditor : IAdaptable
{
	private readonly TreeListView m_treeListView;

	private readonly TreeListViewAdapter m_treeListViewAdapter;

	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private List<IContextMenuCommandProvider> m_actualContextMenuCommandProviders;

	public ITreeListView View
	{
		get
		{
			return m_treeListViewAdapter.View;
		}
		set
		{
			m_treeListViewAdapter.View = value;
		}
	}

	public TreeListView TreeListView => m_treeListView;

	public TreeListViewAdapter TreeListViewAdapter => m_treeListViewAdapter;

	public object LastHit
	{
		get
		{
			object lastHit = m_treeListViewAdapter.LastHit;
			return (lastHit == TreeListViewAdapter) ? this : lastHit;
		}
	}

	public IEnumerable<object> Selection
	{
		get
		{
			return TreeListViewAdapter.Selection;
		}
		set
		{
			TreeListViewAdapter.Selection = value;
		}
	}

	[Import(AllowDefault = true)]
	public ICommandService CommandService { get; set; }

	[Import(AllowDefault = true)]
	public IStatusService StatusService { get; set; }

	public IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
	{
		get
		{
			return m_actualContextMenuCommandProviders ?? (m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(m_contextMenuCommandProviders.GetValues()));
		}
		set
		{
			m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(value);
		}
	}

	public event EventHandler<KeyEventArgs> KeyDown;

	public event EventHandler<KeyPressEventArgs> KeyPress;

	public event EventHandler<KeyEventArgs> KeyUp;

	public event EventHandler<MouseEventArgs> MouseClick;

	public event EventHandler<MouseEventArgs> MouseDoubleClick;

	public event EventHandler<MouseEventArgs> MouseDown;

	public event EventHandler<MouseEventArgs> MouseUp;

	protected TreeListViewEditor(TreeListView.Style style)
	{
		m_treeListView = new TreeListView(style);
		m_treeListView.Control.KeyDown += ControlKeyDown;
		m_treeListView.Control.KeyPress += ControlKeyPress;
		m_treeListView.Control.KeyUp += ControlKeyUp;
		m_treeListView.Control.MouseClick += ControlMouseClick;
		m_treeListView.Control.MouseDoubleClick += ControlMouseDoubleClick;
		m_treeListView.Control.MouseDown += ControlMouseDown;
		m_treeListView.Control.MouseUp += ControlMouseUp;
		m_treeListView.DragOver += TreeListViewDragOver;
		m_treeListView.DragDrop += TreeListViewDragDrop;
		m_treeListView.NodeDrag += TreeListViewNodeDrag;
		m_treeListViewAdapter = new TreeListViewAdapter(m_treeListView);
	}

	public virtual object GetAdapter(Type type)
	{
		if (type.Equals(typeof(TreeListView)))
		{
			return TreeListView;
		}
		if (type.Equals(typeof(Control)))
		{
			return TreeListView;
		}
		if (type.Equals(typeof(TreeListViewAdapter)))
		{
			return TreeListViewAdapter;
		}
		return type.Equals(typeof(ITreeListView)) ? View : null;
	}

	public object GetItemAt(Point clientPoint)
	{
		return TreeListViewAdapter.GetItemAt(clientPoint);
	}

	public int GetItemColumnIndexAt(Point clientPoint)
	{
		return m_treeListView.GetNodeColumnIndexAt(clientPoint);
	}

	public IEnumerable<T> SelectionAs<T>() where T : class
	{
		return from i in Selection
			where i.Is<T>()
			select i.As<T>();
	}

	protected virtual void OnKeyDown(KeyEventArgs e)
	{
		this.KeyDown.Raise(this, e);
	}

	protected virtual void OnKeyPress(KeyPressEventArgs e)
	{
		this.KeyPress.Raise(this, e);
	}

	protected virtual void OnKeyUp(KeyEventArgs e)
	{
		this.KeyUp.Raise(this, e);
	}

	protected virtual void OnMouseClick(MouseEventArgs e)
	{
		this.MouseClick.Raise(this, e);
	}

	protected virtual void OnMouseDoubleClick(MouseEventArgs e)
	{
		this.MouseDoubleClick.Raise(this, e);
	}

	protected virtual void OnMouseDown(MouseEventArgs e)
	{
		this.MouseDown.Raise(this, e);
	}

	protected virtual void OnMouseUp(MouseEventArgs e)
	{
		this.MouseUp.Raise(this, e);
		if (e.Button == MouseButtons.Right && CommandService != null && ContextMenuCommandProviders != null)
		{
			IEnumerable<object> commands = ContextMenuCommandProviders.GetCommands(View, m_treeListViewAdapter.LastHit);
			Point screenPoint = TreeListView.Control.PointToScreen(new Point(e.X, e.Y));
			CommandService.RunContextMenu(commands, screenPoint);
		}
	}

	protected virtual void OnDragOver(DragEventArgs e)
	{
		bool flag = false;
		try
		{
			flag = ApplicationUtil.CanInsert(View, LastHit, e.Data);
		}
		finally
		{
			e.Effect = (flag ? DragDropEffects.Move : DragDropEffects.None);
		}
	}

	protected virtual void OnDragDrop(DragEventArgs e)
	{
		ApplicationUtil.Insert(View, LastHit, e.Data, "Drag and drop", StatusService);
	}

	protected virtual void OnNodeDrag(TreeListView.NodeDragEventArgs e)
	{
		TreeListView.DoDragDrop(e.Node.Tag, DragDropEffects.All);
	}

	private void ControlKeyDown(object sender, KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	private void ControlKeyPress(object sender, KeyPressEventArgs e)
	{
		OnKeyPress(e);
	}

	private void ControlKeyUp(object sender, KeyEventArgs e)
	{
		OnKeyUp(e);
	}

	private void ControlMouseClick(object sender, MouseEventArgs e)
	{
		OnMouseClick(e);
	}

	private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
	{
		OnMouseDoubleClick(e);
	}

	private void ControlMouseDown(object sender, MouseEventArgs e)
	{
		OnMouseDown(e);
	}

	private void ControlMouseUp(object sender, MouseEventArgs e)
	{
		OnMouseUp(e);
	}

	private void TreeListViewDragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void TreeListViewDragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void TreeListViewNodeDrag(object sender, TreeListView.NodeDragEventArgs e)
	{
		OnNodeDrag(e);
	}
}
