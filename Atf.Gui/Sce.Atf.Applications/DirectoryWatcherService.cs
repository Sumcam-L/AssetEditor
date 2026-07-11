using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Sce.Atf.Applications;

[Export(typeof(IDirectoryWatcherService))]
[Export(typeof(DirectoryWatcherService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DirectoryWatcherService : IDirectoryWatcherService
{
	private Dictionary<string, List<FileSystemWatcher>> m_watchers = new Dictionary<string, List<FileSystemWatcher>>(StringComparer.InvariantCultureIgnoreCase);

	[Import(AllowDefault = true)]
	private ISynchronizeInvoke m_syncObject;

	private int m_internalBufferSize = 8192;

	private Queue<Pair<object, FileSystemEventArgs>> m_queue = new Queue<Pair<object, FileSystemEventArgs>>();

	public ISynchronizeInvoke SynchronizingObject
	{
		get
		{
			return m_syncObject;
		}
		set
		{
			m_syncObject = value;
		}
	}

	[DefaultValue(8192)]
	public int InternalBufferSize
	{
		get
		{
			return m_internalBufferSize;
		}
		set
		{
			m_internalBufferSize = value;
		}
	}

	public event FileSystemEventHandler FileChanged;

	[ImportingConstructor]
	public DirectoryWatcherService()
	{
	}

	public DirectoryWatcherService(ISynchronizeInvoke synchronizationObject)
	{
		m_syncObject = synchronizationObject;
	}

	public void Register(string directory, IEnumerable<string> extensions, bool includeSubdirectories)
	{
		if (!m_watchers.TryGetValue(directory, out var value))
		{
			value = new List<FileSystemWatcher>();
			m_watchers.Add(directory, value);
		}
		foreach (string extension in extensions)
		{
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(directory, extension);
			fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
			fileSystemWatcher.SynchronizingObject = m_syncObject;
			fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;
			fileSystemWatcher.InternalBufferSize = InternalBufferSize;
			fileSystemWatcher.Changed += watcher_Changed;
			fileSystemWatcher.Renamed += watcher_Changed;
			fileSystemWatcher.Deleted += watcher_Changed;
			value.Add(fileSystemWatcher);
			fileSystemWatcher.EnableRaisingEvents = true;
		}
	}

	public void Unregister(string directory)
	{
		if (!m_watchers.TryGetValue(directory, out var value))
		{
			return;
		}
		foreach (FileSystemWatcher item in value)
		{
			item.EnableRaisingEvents = false;
			item.Dispose();
		}
		m_watchers.Remove(directory);
	}

	protected virtual void OnChanged(object sender, FileSystemEventArgs e)
	{
		this.FileChanged?.Invoke(this, e);
	}

	private void watcher_Changed(object sender, FileSystemEventArgs e)
	{
		lock (m_queue)
		{
			if (m_queue.Any((Pair<object, FileSystemEventArgs> eventInfo) => eventInfo.Second.FullPath == e.FullPath))
			{
				return;
			}
			m_queue.Enqueue(new Pair<object, FileSystemEventArgs>(sender, e));
			if (m_queue.Count <= 1)
			{
				while (m_queue.Count > 0)
				{
					Pair<object, FileSystemEventArgs> pair = m_queue.Peek();
					OnChanged(pair.First, pair.Second);
					m_queue.Dequeue();
				}
			}
		}
	}
}
