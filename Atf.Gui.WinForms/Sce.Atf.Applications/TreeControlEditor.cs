using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(TreeControlEditor))]
[PartCreationPolicy(CreationPolicy.Any)]
public class TreeControlEditor
{
	[Import(AllowDefault = true)]
	private IStatusService m_statusService;

	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private readonly ICommandService m_commandService;

	private readonly TreeControl m_treeControl;

	private readonly TreeControlAdapter m_treeControlAdapter;

	public ITreeView TreeView
	{
		get
		{
			return m_treeControlAdapter.TreeView;
		}
		set
		{
			m_treeControlAdapter.TreeView = value;
		}
	}

	public TreeControl TreeControl => m_treeControl;

	public TreeControlAdapter TreeControlAdapter => m_treeControlAdapter;

	public object LastHit => m_treeControlAdapter.LastHit;

	protected ICommandService CommandService => m_commandService;

	protected IStatusService StatusService => m_statusService;

	protected IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders => (m_contextMenuCommandProviders == null) ? EmptyEnumerable<IContextMenuCommandProvider>.Instance : m_contextMenuCommandProviders.GetValues();

	public event EventHandler LastHitChanged;

	[ImportingConstructor]
	public TreeControlEditor(ICommandService commandService)
	{
		m_commandService = commandService;
		Configure(out m_treeControl, out m_treeControlAdapter);
		m_treeControl.MouseMove += TreeControlMouseMove;
		m_treeControl.MouseUp += TreeControlMouseUp;
		m_treeControl.DragOver += TreeControlDragOver;
		m_treeControl.DragDrop += TreeControlDragDrop;
		m_treeControl.NodeLabelEdited += TreeControlNodeLabelEdited;
		m_treeControlAdapter.LastHitChanged += TreeControlAdapterLastHitChanged;
	}

	public TreeControlEditor(ICommandService commandService, IContextMenuCommandProvider menuProvider)
	{
		m_commandService = commandService;
		Configure(out m_treeControl, out m_treeControlAdapter);
		m_treeControl.MouseMove += TreeControlMouseMove;
		m_treeControl.MouseUp += TreeControlMouseUp;
		m_treeControl.DragOver += TreeControlDragOver;
		m_treeControl.DragDrop += TreeControlDragDrop;
		m_treeControl.NodeLabelEdited += TreeControlNodeLabelEdited;
		m_treeControlAdapter.LastHitChanged += TreeControlAdapterLastHitChanged;
		m_contextMenuCommandProviders = new List<Lazy<IContextMenuCommandProvider>>
		{
			new Lazy<IContextMenuCommandProvider>(() => menuProvider)
		};
	}

	protected virtual void Configure(out TreeControl treeControl, out TreeControlAdapter treeControlAdapter)
	{
		treeControl = new TreeControl
		{
			ImageList = ResourceUtil.GetImageList16(),
			StateImageList = ResourceUtil.GetImageList16()
		};
		treeControlAdapter = new TreeControlAdapter(treeControl);
	}

	protected virtual void OnLastHitChanged(EventArgs e)
	{
		this.LastHitChanged.Raise(this, e);
	}

	protected virtual void OnStartDrag(IEnumerable<object> items)
	{
		m_treeControl.DoDragDrop(items, DragDropEffects.All);
	}

	protected virtual void OnMouseUp(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			IEnumerable<object> commands = ContextMenuCommandProviders.GetCommands(TreeView, m_treeControlAdapter.LastHit);
			Point screenPoint = m_treeControl.PointToScreen(new Point(e.X, e.Y));
			m_commandService.RunContextMenu(commands, screenPoint);
		}
	}

	protected virtual void OnDragOver(DragEventArgs e)
	{
		bool showDragBetweenCue = TreeControl.ShowDragBetweenCue;
		TreeControl.ShowDragBetweenCue = false;
		bool flag = ApplicationUtil.CanInsert(m_treeControlAdapter.TreeView, m_treeControlAdapter.LastHit, e.Data);
		e.Effect = (flag ? DragDropEffects.Move : DragDropEffects.None);
		if (showDragBetweenCue != TreeControl.ShowDragBetweenCue)
		{
			TreeControl.Refresh();
		}
	}

	protected virtual void OnDragDrop(DragEventArgs e)
	{
		ApplicationUtil.Insert(m_treeControlAdapter.TreeView, m_treeControlAdapter.LastHit, e.Data, "Drag and Drop", m_statusService);
		if (TreeControl.ShowDragBetweenCue)
		{
			TreeControl.ShowDragBetweenCue = false;
			TreeControl.Invalidate();
		}
	}

	protected virtual void OnNodeLabelEdited(TreeControl.NodeEventArgs e)
	{
		ITreeView treeView = m_treeControlAdapter.TreeView;
		foreach (INamingContext namingContext in treeView.AsAll<INamingContext>())
		{
			if (namingContext.CanSetName(e.Node.Tag))
			{
				ITransactionContext context = treeView.As<ITransactionContext>();
				context.DoTransaction(delegate
				{
					namingContext.SetName(e.Node.Tag, e.Node.Label);
				}, "Edit Label".Localize());
				break;
			}
		}
	}

	private void TreeControlAdapterLastHitChanged(object sender, EventArgs e)
	{
		OnLastHitChanged(EventArgs.Empty);
	}

	private void TreeControlMouseMove(object sender, MouseEventArgs e)
	{
		if (!m_treeControl.IsDragging)
		{
			return;
		}
		object[] array = null;
		if ((Control.ModifierKeys & Keys.Alt) != Keys.None)
		{
			object itemAt = m_treeControlAdapter.GetItemAt(new Point(e.X, e.Y));
			if (itemAt != null)
			{
				array = new object[1] { itemAt };
			}
		}
		else
		{
			array = m_treeControlAdapter.GetSelectedItems();
		}
		if (array != null)
		{
			OnStartDrag(array);
		}
	}

	private void TreeControlMouseUp(object sender, MouseEventArgs e)
	{
		OnMouseUp(e);
	}

	private void TreeControlDragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void TreeControlDragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void TreeControlNodeLabelEdited(object sender, TreeControl.NodeEventArgs e)
	{
		OnNodeLabelEdited(e);
	}
}
