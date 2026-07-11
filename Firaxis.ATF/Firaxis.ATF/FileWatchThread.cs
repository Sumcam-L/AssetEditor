using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Firaxis.ATF;

public class FileWatchThread : IDisposable
{
	private struct FileChangedOperation
	{
		public readonly WatcherChangeTypes ChangeType;

		public readonly string FilePath;

		public readonly string OldPath;

		public FileChangedOperation(string filePath, string oldPath, WatcherChangeTypes changeType)
		{
			FilePath = filePath.ToLower();
			OldPath = ((oldPath != null) ? oldPath.ToLower() : oldPath);
			ChangeType = changeType;
		}
	}

	private IQuietTimeAction m_quietTimeWaiter;

	private bool m_disposedValue;

	private volatile bool m_isThreadRunning = true;

	private string[] m_paramHolder = new string[1] { string.Empty };

	private readonly ICollection<FileSystemWatcher> m_fileWatchers = new List<FileSystemWatcher>();

	private readonly Thread m_fileWatchThread;

	private readonly AutoResetEvent m_fileWatchThreadSignal = new AutoResetEvent(initialState: false);

	private readonly ConcurrentQueue<FileChangedOperation> m_pendingOperations = new ConcurrentQueue<FileChangedOperation>();

	public event EventHandler<FileChangedEventArgs> FileChanged;

	public event EventHandler<FileChangedEventArgs> FileCreated;

	public event EventHandler<FileChangedEventArgs> FileDeleted;

	public event EventHandler<FileRenamedEventArgs> FileRenamed;

	public FileWatchThread()
		: this("File Watch Thread")
	{
	}

	public FileWatchThread(string name)
	{
		m_fileWatchThread = new Thread(ServiceFileWatchEvents);
		m_fileWatchThread.IsBackground = true;
		m_fileWatchThread.Name = name;
		m_quietTimeWaiter = new QuietTimeAction(QuietTimeWaitBehavior.ExponentialBackoff, delegate
		{
			KickEventHandlers();
		});
	}

	private void ScheduleKickEventHandlers()
	{
		m_quietTimeWaiter?.UpdateLastChangeTime();
	}

	private void KickEventHandlers()
	{
		_ = m_quietTimeWaiter?.UpdatesSinceLastAction;
		_ = m_pendingOperations.Count;
		m_fileWatchThreadSignal.Set();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			m_quietTimeWaiter?.Dispose();
			m_quietTimeWaiter = null;
			m_isThreadRunning = false;
			foreach (FileSystemWatcher fileWatcher in m_fileWatchers)
			{
				fileWatcher.Changed -= HandleFileChanged;
				fileWatcher.Created -= HandleFileChanged;
				fileWatcher.Deleted -= HandleFileChanged;
				fileWatcher.Renamed -= HandleFileRenamed;
				fileWatcher.EnableRaisingEvents = false;
				fileWatcher.Dispose();
			}
			m_fileWatchers.Clear();
			FileChangedOperation result;
			while (m_pendingOperations.TryDequeue(out result))
			{
			}
			m_fileWatchThreadSignal.Set();
			if (m_fileWatchThread.IsAlive)
			{
				m_fileWatchThread.Join();
			}
			m_fileWatchThreadSignal.Dispose();
		}
		m_disposedValue = true;
	}

	public void AddWatchPath(string directory, string filter)
	{
		m_paramHolder[0] = directory;
		AddWatchPaths(m_paramHolder, filter);
	}

	public void AddWatchPaths(IEnumerable<string> directories, string filter)
	{
		foreach (string directory in directories)
		{
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(directory, filter);
			fileSystemWatcher.IncludeSubdirectories = true;
			fileSystemWatcher.Changed += HandleFileChanged;
			fileSystemWatcher.Created += HandleFileChanged;
			fileSystemWatcher.Deleted += HandleFileChanged;
			fileSystemWatcher.Renamed += HandleFileRenamed;
			m_fileWatchers.Add(fileSystemWatcher);
		}
	}

	public void Start()
	{
		foreach (FileSystemWatcher fileWatcher in m_fileWatchers)
		{
			fileWatcher.EnableRaisingEvents = true;
		}
		m_fileWatchThread.Start();
	}

	protected virtual void OnFileChanged(string path)
	{
		this.FileChanged?.Invoke(this, new FileChangedEventArgs(path));
	}

	protected virtual void OnFileCreated(string path)
	{
		if (File.Exists(path))
		{
			this.FileCreated?.Invoke(this, new FileChangedEventArgs(path));
		}
	}

	protected virtual void OnFileDeleted(string path)
	{
		this.FileDeleted?.Invoke(this, new FileChangedEventArgs(path));
	}

	protected virtual void OnFileRenamed(string path, string oldPath)
	{
		this.FileRenamed?.Invoke(this, new FileRenamedEventArgs(path, oldPath));
	}

	private void HandleFileChanged(object sender, FileSystemEventArgs e)
	{
		FileChangedOperation item = new FileChangedOperation(e.FullPath, null, e.ChangeType);
		m_pendingOperations.Enqueue(item);
		ScheduleKickEventHandlers();
	}

	private void HandleFileRenamed(object sender, RenamedEventArgs e)
	{
		FileChangedOperation item = new FileChangedOperation(e.FullPath, e.OldFullPath, e.ChangeType);
		m_pendingOperations.Enqueue(item);
		ScheduleKickEventHandlers();
	}

	private void ProcessQueue()
	{
		FileChangedOperation result;
		while (m_pendingOperations.TryDequeue(out result))
		{
			switch (result.ChangeType)
			{
			case WatcherChangeTypes.Created:
				OnFileCreated(result.FilePath);
				break;
			case WatcherChangeTypes.Deleted:
				OnFileDeleted(result.FilePath);
				break;
			case WatcherChangeTypes.Changed:
				OnFileChanged(result.FilePath);
				break;
			case WatcherChangeTypes.Renamed:
				OnFileRenamed(result.FilePath, result.OldPath);
				break;
			}
		}
	}

	private void ServiceFileWatchEvents(object context)
	{
		while (m_isThreadRunning && m_fileWatchThreadSignal.WaitOne())
		{
			ProcessQueue();
		}
	}
}
