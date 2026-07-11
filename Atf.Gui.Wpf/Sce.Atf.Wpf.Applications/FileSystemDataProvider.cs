using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Applications;

public class FileSystemDataProvider : DataSourceProvider
{
	private readonly FileSystemWatcher m_watcher;

	private readonly ObservableFileInfoCollection m_files;

	public string Path
	{
		get
		{
			return m_watcher.Path;
		}
		set
		{
			if (!(m_watcher.Path == value) && Directory.Exists(value))
			{
				m_watcher.Path = value;
				if (!m_watcher.EnableRaisingEvents)
				{
					m_watcher.EnableRaisingEvents = true;
				}
				OnPropertyChanged(new PropertyChangedEventArgs("Path"));
			}
		}
	}

	public FileSystemDataProvider()
	{
		m_files = new ObservableFileInfoCollection();
		m_watcher = new FileSystemWatcher();
		m_watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
		m_watcher.Changed += OnFileChanged;
		m_watcher.Created += OnFileCreated;
		m_watcher.Deleted += OnFileDeleted;
		m_watcher.Renamed += OnFileRenamed;
	}

	protected override void BeginQuery()
	{
		string path = m_watcher.Path;
		if (Directory.Exists(path))
		{
			if (0 < m_files.Count)
			{
				m_files.Clear();
			}
			string[] files = Directory.GetFiles(path);
			string[] array = files;
			foreach (string file in array)
			{
				m_files.Add(new ObservableFileInfo(file));
			}
		}
		OnQueryFinished(m_files);
	}

	private void OnFileChanged(object sender, FileSystemEventArgs e)
	{
		base.Dispatcher.InvokeIfRequired(delegate
		{
			m_files.GetFile(e.FullPath)?.Refresh();
		});
	}

	private void OnFileCreated(object sender, FileSystemEventArgs e)
	{
		base.Dispatcher.InvokeIfRequired(delegate
		{
			if (File.Exists(e.FullPath))
			{
				m_files.Add(new ObservableFileInfo(e.FullPath));
			}
		});
	}

	private void OnFileDeleted(object sender, FileSystemEventArgs e)
	{
		base.Dispatcher.InvokeIfRequired(delegate
		{
			ObservableFileInfo file = m_files.GetFile(e.FullPath);
			if (file != null)
			{
				m_files.Remove(file);
			}
		});
	}

	private void OnFileRenamed(object sender, RenamedEventArgs e)
	{
		base.Dispatcher.InvokeIfRequired(delegate
		{
			m_files.GetFile(e.OldFullPath)?.ChangeName(e.FullPath);
		});
	}
}
