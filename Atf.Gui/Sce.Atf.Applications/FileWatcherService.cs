using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IFileWatcherService))]
[Export(typeof(FileWatcherService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FileWatcherService : IFileWatcherService, IDisposable, IInitializable
{
	private ISettingsService m_settingsService;

	private bool m_useSinglePantryMode = false;

	[Import(AllowDefault = true)]
	private ISynchronizeInvoke m_syncObject;

	private readonly ConcurrentDictionary<string, FileSystemWatcher> m_watchers = new ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.InvariantCultureIgnoreCase);

	private readonly ConcurrentQueue<Pair<object, FileSystemEventArgs>> m_queue = new ConcurrentQueue<Pair<object, FileSystemEventArgs>>();

	private readonly IDictionary<string, int> m_suspensions = new ConcurrentDictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

	private bool disposedValue = false;

	public bool UseSinglePantryMode
	{
		get
		{
			return m_useSinglePantryMode;
		}
		set
		{
			m_useSinglePantryMode = value;
		}
	}

	protected ISynchronizeInvoke SynchronizingObject
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

	public event FileSystemEventHandler FileChanged;

	[ImportingConstructor]
	public FileWatcherService(ISettingsService settingSvc)
	{
		m_settingsService = settingSvc;
	}

	public FileWatcherService(ISynchronizeInvoke synchronizationObject)
	{
		m_syncObject = synchronizationObject;
	}

	public void Register(string filePath)
	{
		if (!m_watchers.ContainsKey(filePath))
		{
			string directoryName = Path.GetDirectoryName(filePath);
			string fileName = Path.GetFileName(filePath);
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(directoryName, fileName);
			if (m_watchers.TryAdd(filePath, fileSystemWatcher))
			{
				fileSystemWatcher.SynchronizingObject = m_syncObject;
				fileSystemWatcher.Changed += watcher_Changed;
				fileSystemWatcher.Renamed += OnRenamed;
				fileSystemWatcher.EnableRaisingEvents = true;
			}
			else
			{
				fileSystemWatcher.Dispose();
			}
		}
	}

	public void Unregister(string filePath)
	{
		if (m_watchers.TryRemove(filePath, out var value))
		{
			value.EnableRaisingEvents = false;
			value.Changed -= watcher_Changed;
			value.Renamed -= OnRenamed;
			value.Dispose();
		}
	}

	protected virtual void OnChanged(object sender, FileSystemEventArgs e)
	{
		if (!disposedValue)
		{
			this.FileChanged?.Invoke(this, e);
		}
	}

	protected virtual void OnRenamed(object sender, RenamedEventArgs e)
	{
		if (m_watchers.TryGetValue(e.FullPath, out var value))
		{
			watcher_Changed(value, new FileSystemEventArgs(WatcherChangeTypes.Changed, value.Path, value.Filter));
		}
	}

	private void watcher_Changed(object sender, FileSystemEventArgs e)
	{
		if (m_queue.Any((Pair<object, FileSystemEventArgs> pair) => pair.Second.FullPath == e.FullPath))
		{
			return;
		}
		m_queue.Enqueue(new Pair<object, FileSystemEventArgs>(sender, e));
		if (m_queue.Count > 1)
		{
			return;
		}
		while (m_queue.Count > 0)
		{
			if (m_queue.TryDequeue(out var result))
			{
				OnChanged(result.First, result.Second);
			}
		}
	}

	public void Suspend(string filePath)
	{
		if (!m_suspensions.ContainsKey(filePath))
		{
			m_suspensions[filePath] = 0;
		}
		m_suspensions[filePath]++;
		if (m_watchers.ContainsKey(filePath))
		{
			m_watchers[filePath].EnableRaisingEvents = false;
		}
	}

	public void Unsuspend(string filePath)
	{
		if (m_suspensions.ContainsKey(filePath) && --m_suspensions[filePath] == 0)
		{
			m_suspensions.Remove(filePath);
			if (m_watchers.ContainsKey(filePath))
			{
				m_watchers[filePath].EnableRaisingEvents = true;
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			foreach (FileSystemWatcher value in m_watchers.Values)
			{
				value.EnableRaisingEvents = false;
				value.Changed -= watcher_Changed;
				value.Renamed -= OnRenamed;
				value.Dispose();
			}
			m_watchers.Clear();
			m_suspensions.Clear();
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void Initialize()
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterUserSettings("Application".Localize(), new BoundPropertyDescriptor(this, () => UseSinglePantryMode, "Enable Single Pantry Mode".Localize(), "File Watching".Localize(), "If true, Use a single pantry for all assets except artdefs and xlps.".Localize()));
		}
	}
}
