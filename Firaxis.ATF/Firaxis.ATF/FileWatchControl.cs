using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Controls;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class FileWatchControl : UserControl
{
	private enum OperationType
	{
		Add,
		Remove
	}

	private struct FileWatchTreeOperation
	{
		public readonly IFileWatchNode ItemNode;

		public readonly OperationType Operation;

		public FileWatchTreeOperation(IFileWatchNode node, OperationType operation)
		{
			ItemNode = node;
			Operation = operation;
		}
	}

	private IFileWatchTreeContext m_context;

	private bool m_disposedValue;

	private readonly ICivTechService m_civTechService;

	private readonly ISkinService m_skinService;

	private readonly ConcurrentQueue<FileWatchTreeOperation> m_pendingOperations = new ConcurrentQueue<FileWatchTreeOperation>();

	private readonly IDictionary<IFileWatchNode, TreeNode> m_nodeMap = new ConcurrentDictionary<IFileWatchNode, TreeNode>();

	private readonly AutoResetEvent m_queryNextUIEvent = new AutoResetEvent(initialState: false);

	private volatile bool m_threadAlive = true;

	private readonly TaskScheduler m_uiTaskScheduler;

	private readonly Thread m_uiUpdateThread;

	private IContainer components;

	private ImageList imageList1;

	private TriStateCheckboxTreeView _fileWatchTree;

	public FileWatchControl(ICivTechService civTechSvc, ISkinService skinSvc)
	{
		InitializeComponent();
		m_civTechService = civTechSvc;
		m_skinService = skinSvc;
		_fileWatchTree.LockParentChildState = true;
		_fileWatchTree.AfterCheck += HandleUICheckedChanged;
		m_uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
		m_uiUpdateThread = new Thread(ServiceRequests);
		m_uiUpdateThread.IsBackground = true;
		m_uiUpdateThread.Name = "File Watch update thread.";
		m_uiUpdateThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
		m_uiUpdateThread.Start();
		m_skinService.SkinChangedOrApplied += SkinService_SkinChangedOrApplied;
	}

	private void SkinService_SkinChangedOrApplied(object sender, EventArgs e)
	{
		SkinService.ApplyActiveSkin(this);
		ApplySkinToNodes(_fileWatchTree.Nodes);
	}

	private void ApplySkinToNodes(TreeNodeCollection nodes)
	{
		if (nodes == null)
		{
			return;
		}
		foreach (TreeNode node in nodes)
		{
			node.BackColor = BackColor;
			node.ForeColor = ForeColor;
			ApplySkinToNodes(node.Nodes);
		}
	}

	public void Bind(IFileWatchTreeContext context)
	{
		Bind(context, disposing: false);
	}

	protected override void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			_fileWatchTree.AfterCheck -= HandleUICheckedChanged;
			m_threadAlive = false;
			FileWatchTreeOperation result;
			while (m_pendingOperations.TryDequeue(out result))
			{
			}
			m_queryNextUIEvent.Set();
			m_uiUpdateThread.Join(TimeSpan.FromMilliseconds(1.0));
			m_queryNextUIEvent.Dispose();
			Parallel.ForEach(m_nodeMap.Keys, delegate(IFileWatchNode node)
			{
				RemoveEvents(node);
			});
			m_nodeMap.Clear();
			Bind(null, disposing: true);
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
		m_disposedValue = true;
	}

	private void Bind(IFileWatchTreeContext context, bool disposing)
	{
		if (m_context != null)
		{
			m_context.RootNodeAdded -= Context_RootNodeAdded;
			m_context.RootNodeRemoved -= Context_RootNodeRemoved;
			if (!disposing)
			{
				_fileWatchTree.Nodes.Clear();
			}
		}
		m_context = context;
		if (context != null)
		{
			context.RootNodeAdded += Context_RootNodeAdded;
			context.RootNodeRemoved += Context_RootNodeRemoved;
			ReconcileFileWatchTree();
		}
	}

	private TreeNode BuildFileWatchTreeNode(TreeNodeCollection parent, IFileWatchNode fileWatchNode)
	{
		TreeNode treeNode = ConstructNode(parent, fileWatchNode);
		OnNodeCreated(treeNode, fileWatchNode);
		IEnumerable<IFileWatchNode> enumerable;
		lock (fileWatchNode.Children)
		{
			enumerable = fileWatchNode.Children.ToArray();
		}
		foreach (IFileWatchNode item in enumerable)
		{
			BuildFileWatchTreeNode(treeNode.Nodes, item);
		}
		return treeNode;
	}

	private Action BuildWrappedUIAction(Action uiTask)
	{
		return delegate
		{
			if (_fileWatchTree.IsHandleCreated)
			{
				_fileWatchTree.BeginUpdate();
				uiTask();
				_fileWatchTree.EndUpdate();
			}
		};
	}

	private TreeNode ConstructNode(TreeNodeCollection owner, IFileWatchNode fileWatchNode)
	{
		TreeNode treeNode = null;
		string localPath = fileWatchNode.FileUri.LocalPath;
		string text = localPath;
		Color foreColor = ForeColor;
		if (fileWatchNode.Location != FileLocation.OnDisk)
		{
			foreColor = Color.Red;
			text += "  [Unavailable on disk]".Localize();
		}
		if (owner != null)
		{
			treeNode = owner.Add(localPath, text);
		}
		else
		{
			treeNode = new TreeNode(text);
			treeNode.Name = localPath;
		}
		treeNode.ForeColor = foreColor;
		SetChecked(fileWatchNode, treeNode);
		return treeNode;
	}

	private void Context_RootNodeAdded(object sender, RootNodeAddedEventArgs e)
	{
		FileWatchTreeOperation item = new FileWatchTreeOperation(e.Node, OperationType.Add);
		m_pendingOperations.Enqueue(item);
		m_queryNextUIEvent.Set();
	}

	private void Context_RootNodeRemoved(object sender, RootNodeRemovedEventArgs e)
	{
		FileWatchTreeOperation item = new FileWatchTreeOperation(e.Node, OperationType.Remove);
		m_pendingOperations.Enqueue(item);
		m_queryNextUIEvent.Set();
	}

	private void HandleAddRequest(IFileWatchNode node)
	{
		TreeNode treeRoot = BuildFileWatchTreeNode(null, node);
		TreeNodeCollection parentCollection;
		TreeNode value;
		if (node.Parent == null)
		{
			parentCollection = _fileWatchTree.Nodes;
		}
		else if (m_nodeMap.TryGetValue(node.Parent, out value))
		{
			parentCollection = value.Nodes;
		}
		else
		{
			BugSubmitter.SilentReport("Unable to find corresponding parent node in the file watch tree. @assign bwhitman");
			parentCollection = _fileWatchTree.Nodes;
		}
		Action task = BuildWrappedUIAction(delegate
		{
			parentCollection.Add(treeRoot);
		});
		PerformUITask(task);
	}

	private void HandleCheckedChanged(object sender, EventArgs e)
	{
		IFileWatchNode node = (IFileWatchNode)sender;
		if (!m_nodeMap.TryGetValue(node, out var treeNode))
		{
			return;
		}
		Action task = delegate
		{
			if (_fileWatchTree.GetChecked(treeNode) == TriStateCheckboxTreeView.CheckState.Checked != node.IsChecked)
			{
				SetChecked(node, treeNode);
			}
		};
		PerformUITask(task);
	}

	private void HandleChildNodeAdded(object sender, ChildNodeAddedEventArgs e)
	{
		FileWatchTreeOperation item = new FileWatchTreeOperation(e.Node, OperationType.Add);
		m_pendingOperations.Enqueue(item);
		m_queryNextUIEvent.Set();
	}

	private void HandleChildNodeRemoved(object sender, ChildNodeRemovedEventArgs e)
	{
		FileWatchTreeOperation item = new FileWatchTreeOperation(e.Node, OperationType.Remove);
		m_pendingOperations.Enqueue(item);
		m_queryNextUIEvent.Set();
	}

	private void HandleRemoveRequest(IFileWatchNode node)
	{
		RemoveEventsRecursive(node);
		Action task = BuildWrappedUIAction(delegate
		{
			RemoveNodeFromTree(node);
		});
		PerformUITask(task);
	}

	private void HandleUICheckedChanged(object sender, TreeViewEventArgs e)
	{
		if (e.Action != TreeViewAction.Unknown)
		{
			TreeNode node = e.Node;
			if (node.Tag != null)
			{
				IFileWatchNode obj = node.Tag as IFileWatchNode;
				BugSubmitter.SilentAssert(obj != null, "Attempted to add non IFileWatchNode to FileWatchControl. @assign bwhitman");
				FileWatchHelper.UpdateCheckedState(newCheckedState: _fileWatchTree.GetChecked(node) == TriStateCheckboxTreeView.CheckState.Checked, nodeUri: obj.FileUri);
			}
		}
	}

	private void OnNodeCreated(TreeNode treeNode, IFileWatchNode dataRepresentation)
	{
		BugSubmitter.SilentAssert(!m_nodeMap.ContainsKey(dataRepresentation), "Duplicate node added to FileWatchControl for " + dataRepresentation.FileUri.LocalPath + ". @assign bwhitman @summary Duplicate node added to FileWatchControl");
		m_nodeMap[dataRepresentation] = treeNode;
		treeNode.Tag = dataRepresentation;
		dataRepresentation.ChildNodeAdded += HandleChildNodeAdded;
		dataRepresentation.ChildNodeRemoved += HandleChildNodeRemoved;
		dataRepresentation.CheckedChanged += HandleCheckedChanged;
	}

	private void PerformUITask(Action task)
	{
		Task task2 = Task.Factory.StartNew(task, default(CancellationToken), TaskCreationOptions.PreferFairness, m_uiTaskScheduler);
		try
		{
			task2.Wait();
		}
		catch (AggregateException ex)
		{
			BugSubmitter.SilentException(ex.Flatten());
		}
	}

	private void ProcessQueue()
	{
		FileWatchTreeOperation result;
		while (m_pendingOperations.TryDequeue(out result))
		{
			switch (result.Operation)
			{
			case OperationType.Add:
				HandleAddRequest(result.ItemNode);
				break;
			case OperationType.Remove:
				HandleRemoveRequest(result.ItemNode);
				break;
			}
		}
	}

	private void ReconcileFileWatchTree()
	{
		foreach (IFileWatchNode rootNode in m_context.RootNodes)
		{
			FileWatchTreeOperation item = new FileWatchTreeOperation(rootNode, OperationType.Add);
			m_pendingOperations.Enqueue(item);
		}
		m_queryNextUIEvent.Set();
	}

	private void RemoveEvents(IFileWatchNode node)
	{
		node.ChildNodeAdded -= HandleChildNodeAdded;
		node.ChildNodeRemoved -= HandleChildNodeRemoved;
		node.CheckedChanged -= HandleCheckedChanged;
	}

	private void RemoveEventsRecursive(IFileWatchNode node)
	{
		RemoveEvents(node);
		IEnumerable<IFileWatchNode> source;
		lock (node.Children)
		{
			source = node.Children.ToArray();
		}
		Parallel.ForEach(source, delegate(IFileWatchNode childNode)
		{
			RemoveEventsRecursive(childNode);
		});
	}

	private void RemoveNodeFromTree(IFileWatchNode node)
	{
		if (m_nodeMap.TryGetValue(node, out var value))
		{
			TreeNode treeNode = value.Parent;
			((treeNode != null) ? treeNode.Nodes : _fileWatchTree.Nodes).Remove(value);
		}
	}

	private void ServiceRequests()
	{
		while (m_threadAlive && m_queryNextUIEvent.WaitOne())
		{
			ProcessQueue();
		}
	}

	private void SetChecked(IFileWatchNode dataRepresentation, TreeNode treeNode)
	{
		TriStateCheckboxTreeView.CheckState checkState = (dataRepresentation.IsChecked ? TriStateCheckboxTreeView.CheckState.Checked : TriStateCheckboxTreeView.CheckState.Unchecked);
		_fileWatchTree.SetChecked(treeNode, checkState);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.ATF.FileWatchControl));
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		this._fileWatchTree = new Firaxis.Controls.TriStateCheckboxTreeView();
		base.SuspendLayout();
		this.imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
		this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
		this.imageList1.Images.SetKeyName(0, "unchecked.png");
		this.imageList1.Images.SetKeyName(1, "checked.png");
		this.imageList1.Images.SetKeyName(2, "partial-checked.png");
		this._fileWatchTree.Dock = System.Windows.Forms.DockStyle.Fill;
		this._fileWatchTree.ImageIndex = 0;
		this._fileWatchTree.Location = new System.Drawing.Point(0, 0);
		this._fileWatchTree.Name = "fileWatchTree";
		this._fileWatchTree.SelectedImageIndex = 0;
		this._fileWatchTree.Size = new System.Drawing.Size(541, 295);
		this._fileWatchTree.StateImageList = this.imageList1;
		this._fileWatchTree.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this._fileWatchTree);
		base.Name = "FileWatchControl";
		base.Size = new System.Drawing.Size(541, 295);
		base.ResumeLayout(false);
	}
}
