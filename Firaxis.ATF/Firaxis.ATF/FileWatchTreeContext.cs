using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

public class FileWatchTreeContext : IFileWatchTreeContext, IDisposable
{
	private enum UpdateType
	{
		Add,
		Remove,
		Change
	}

	private class NodeUpdateOperation
	{
		public readonly IFileWatchNode Node;

		public readonly UpdateType OperationType;

		public NodeUpdateOperation(IFileWatchNode node, UpdateType operation)
		{
			Node = node;
			OperationType = operation;
		}
	}

	private bool m_disposedValue;

	private readonly ISet<IFileWatchNode> m_changingSet = new HashSet<IFileWatchNode>();

	private readonly ICivTechService m_civTechService;

	private readonly IFileWatcherService m_fileWatcher;

	private readonly ConcurrentQueue<NodeUpdateOperation> m_pendingOperations = new ConcurrentQueue<NodeUpdateOperation>();

	private readonly Dictionary<string, IFileWatchNode> m_rootNodes = new Dictionary<string, IFileWatchNode>();

	private readonly Thread m_updateThread;

	private readonly AutoResetEvent m_updateThreadSignal = new AutoResetEvent(initialState: false);

	private volatile bool m_running = true;

	public IEnumerable<IFileWatchNode> RootNodes => m_rootNodes.Values;

	public event EventHandler<RootNodeAddedEventArgs> RootNodeAdded;

	public event EventHandler<RootNodeRemovedEventArgs> RootNodeRemoved;

	public FileWatchTreeContext(IFileWatcherService fileWatcher, ICivTechService civTechSvc)
	{
		m_fileWatcher = fileWatcher;
		m_civTechService = civTechSvc;
		m_updateThread = new Thread(UpdateContext);
		m_updateThread.Name = "File Watch Context Update Thread";
		m_updateThread.IsBackground = true;
		m_updateThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
		m_updateThread.Start();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			m_running = false;
			NodeUpdateOperation result;
			while (m_pendingOperations.TryDequeue(out result))
			{
			}
			m_updateThreadSignal.Set();
			m_updateThread.Join();
			m_updateThreadSignal.Dispose();
		}
		m_rootNodes.Clear();
		m_changingSet.Clear();
		m_disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void AddRootNode(Uri nodeUri)
	{
		string localPath = nodeUri.LocalPath;
		if (!m_rootNodes.ContainsKey(localPath))
		{
			IFileWatchNode fileWatchNode = CreateRootNode(nodeUri);
			m_rootNodes[localPath] = fileWatchNode;
			NodeUpdateOperation item = new NodeUpdateOperation(fileWatchNode, UpdateType.Add);
			m_pendingOperations.Enqueue(item);
			m_updateThreadSignal.Set();
		}
	}

	public void RemoveRootNode(Uri nodeUri)
	{
		string localPath = nodeUri.LocalPath;
		if (m_rootNodes.ContainsKey(localPath))
		{
			IFileWatchNode node = m_rootNodes[localPath];
			m_rootNodes.Remove(localPath);
			NodeUpdateOperation item = new NodeUpdateOperation(node, UpdateType.Remove);
			m_pendingOperations.Enqueue(item);
			m_updateThreadSignal.Set();
		}
	}

	public void UpdateChangedNodes(Uri uri)
	{
		IEnumerable<IFileWatchNode> nodesByURI = FileWatchHelper.GetNodesByURI(uri);
		if (!nodesByURI.Any())
		{
			return;
		}
		ISet<string> set = new HashSet<string>();
		foreach (IFileWatchNode item2 in nodesByURI)
		{
			string localPath = item2.FileUri.LocalPath;
			if (!set.Add(localPath) && !item2.Children.Any())
			{
				continue;
			}
			lock (m_changingSet)
			{
				if (m_changingSet.Add(item2))
				{
					NodeUpdateOperation item = new NodeUpdateOperation(item2, UpdateType.Change);
					m_pendingOperations.Enqueue(item);
				}
			}
		}
		m_updateThreadSignal.Set();
	}

	protected virtual void OnRootNodeAdded(IFileWatchNode node)
	{
		if (m_running)
		{
			this.RootNodeAdded?.Invoke(this, new RootNodeAddedEventArgs(node));
		}
	}

	protected virtual void OnRootNodeRemoved(IFileWatchNode node)
	{
		if (m_running)
		{
			this.RootNodeRemoved?.Invoke(this, new RootNodeRemovedEventArgs(node));
		}
	}

	private void AddChildToNode(IFileWatchNode node, Uri childUri, ISet<string> seenChildren)
	{
		if (m_running)
		{
			IFileWatchNode node2 = node.AddChild(childUri);
			OnNodeCreated(node2);
			CreateNodeChildren(node2, seenChildren);
		}
	}

	private void CreateNodeChildren(IFileWatchNode node, ISet<string> seenChildren)
	{
		if (!m_running)
		{
			return;
		}
		Uri fileUri = node.FileUri;
		string localPath = fileUri.LocalPath;
		if (!seenChildren.Add(localPath))
		{
			return;
		}
		IWorkspaceDependencyRegistry workspaceDependencyRegistry = m_civTechService.GetWorkspaceDependencyRegistry(fileUri);
		if (workspaceDependencyRegistry == null)
		{
			BugSubmitter.SilentReport("Unable to create node children since the Dependency Registry has not been set yet!  @assign bwhitman");
			return;
		}
		foreach (Uri dependency in workspaceDependencyRegistry.GetDependencies(fileUri))
		{
			AddChildToNode(node, dependency, seenChildren);
		}
	}

	private IFileWatchNode CreateRootNode(Uri nodeUri)
	{
		_ = m_fileWatcher;
		IFileWatchNode parent = null;
		return new FileWatchNode(m_fileWatcher, parent, nodeUri);
	}

	private void DoAddOperation(IFileWatchNode rootNode)
	{
		OnNodeCreated(rootNode);
		ISet<string> seenChildren = new HashSet<string>();
		CreateNodeChildren(rootNode, seenChildren);
		OnRootNodeAdded(rootNode);
	}

	private void DoChangeOperation(IFileWatchNode node)
	{
		Uri fileUri = node.FileUri;
		if (!m_running)
		{
			return;
		}
		IEnumerable<Uri> dependencies = m_civTechService.GetWorkspaceDependencyRegistry(fileUri).GetDependencies(fileUri);
		IEnumerable<Uri> enumerable = node.Children.Select((IFileWatchNode child) => child.FileUri).ToArray();
		IEnumerable<Uri> enumerable2 = dependencies.Except(enumerable).ToArray();
		foreach (Uri item in (IEnumerable<Uri>)enumerable.Except(dependencies).ToArray())
		{
			if (m_running)
			{
				node.RemoveChild(item);
			}
		}
		ISet<string> seenChildren = new HashSet<string>();
		foreach (Uri item2 in enumerable2)
		{
			if (m_running)
			{
				AddChildToNode(node, item2, seenChildren);
			}
		}
		lock (m_changingSet)
		{
			if (m_running)
			{
				m_changingSet.Remove(node);
			}
		}
		RemoveRootNode(node.FileUri);
		AddRootNode(node.FileUri);
	}

	private void DoOperation(NodeUpdateOperation operation)
	{
		switch (operation.OperationType)
		{
		case UpdateType.Add:
			DoAddOperation(operation.Node);
			break;
		case UpdateType.Remove:
			DoRemoveOperation(operation.Node);
			break;
		case UpdateType.Change:
			DoChangeOperation(operation.Node);
			break;
		default:
			BugSubmitter.SilentReport("Invalid operation on FileWatchTree. @assign bwhitman");
			break;
		}
	}

	private void DoRemoveOperation(IFileWatchNode rootNode)
	{
		UnregisterNodeAndChildren(rootNode);
		OnRootNodeRemoved(rootNode);
	}

	private void OnNodeCreated(IFileWatchNode node)
	{
		FileWatchHelper.RegisterNode(node);
		node.IsChecked = FileWatchHelper.GetStartingCheckedState(node);
	}

	private void UnregisterNodeAndChildren(IFileWatchNode node)
	{
		if (!m_running)
		{
			return;
		}
		FileWatchHelper.UnregisterNode(node);
		foreach (IFileWatchNode item in (IEnumerable<IFileWatchNode>)node.Children.ToArray())
		{
			UnregisterNodeAndChildren(item);
		}
	}

	private void UpdateContext()
	{
		while (m_updateThreadSignal.WaitOne() && m_running)
		{
			NodeUpdateOperation result;
			while (m_pendingOperations.TryDequeue(out result))
			{
				DoOperation(result);
			}
		}
	}
}
