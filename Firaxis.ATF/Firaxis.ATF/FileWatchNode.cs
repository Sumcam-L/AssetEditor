using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

public class FileWatchNode : IFileWatchNode
{
	private bool m_isChecked;

	private readonly Dictionary<string, IFileWatchNode> m_children = new Dictionary<string, IFileWatchNode>();

	private readonly string m_filePath;

	private readonly Uri m_fileUri;

	private readonly IFileWatcherService m_fileWatcher;

	private readonly FileLocation m_location;

	private readonly IFileWatchNode m_parent;

	public IEnumerable<IFileWatchNode> Children => m_children.Values;

	public Uri FileUri => m_fileUri;

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			if (m_isChecked == value)
			{
				return;
			}
			m_isChecked = value;
			if (Location == FileLocation.OnDisk)
			{
				if (m_isChecked)
				{
					m_fileWatcher.Register(m_filePath);
				}
				else
				{
					m_fileWatcher.Unregister(m_filePath);
				}
			}
			OnCheckedChanged();
		}
	}

	public FileLocation Location => m_location;

	public IFileWatchNode Parent => m_parent;

	public event EventHandler CheckedChanged;

	public event EventHandler<ChildNodeAddedEventArgs> ChildNodeAdded;

	public event EventHandler<ChildNodeRemovedEventArgs> ChildNodeRemoved;

	public FileWatchNode(IFileWatcherService fileWatch, IFileWatchNode parent, Uri fileUri)
	{
		m_fileWatcher = fileWatch;
		m_parent = parent;
		m_fileUri = fileUri;
		m_filePath = m_fileUri.LocalPath;
		m_location = GetFileLocation(fileUri);
	}

	public IFileWatchNode AddChild(Uri childUri)
	{
		IFileWatchNode fileWatchNode = CreateChild(childUri);
		m_children[childUri.LocalPath] = fileWatchNode;
		OnChildNodeAdded(fileWatchNode);
		return fileWatchNode;
	}

	public void RemoveChild(Uri childUri)
	{
		string localPath = childUri.LocalPath;
		if (m_children.ContainsKey(localPath))
		{
			IFileWatchNode node = m_children[localPath];
			m_children.Remove(localPath);
			OnChildNodeRemoved(node);
		}
	}

	protected virtual void OnCheckedChanged()
	{
		this.CheckedChanged?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnChildNodeAdded(IFileWatchNode node)
	{
		this.ChildNodeAdded?.Invoke(this, new ChildNodeAddedEventArgs(node));
	}

	protected virtual void OnChildNodeRemoved(IFileWatchNode node)
	{
		this.ChildNodeRemoved?.Invoke(this, new ChildNodeRemovedEventArgs(node));
	}

	private IFileWatchNode CreateChild(Uri childUri)
	{
		return new FileWatchNode(m_fileWatcher, this, childUri);
	}

	private FileLocation GetFileLocation(Uri fileUri)
	{
		string localPath = fileUri.LocalPath;
		if (new FileInfo(localPath).Exists)
		{
			if (PathHelper.IsPathOnNetwork(localPath))
			{
				return FileLocation.OnNetwork;
			}
			return FileLocation.OnDisk;
		}
		return FileLocation.NotFound;
	}
}
