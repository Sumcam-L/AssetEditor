using System;

namespace Sce.Atf;

public class FileWatchSuspension : IDisposable
{
	private IFileWatcherService m_fileWatcherService;

	private Uri m_fileToSuspend;

	public FileWatchSuspension(IFileWatcherService watcherService, Uri fileToSuspend)
	{
		m_fileWatcherService = watcherService;
		m_fileToSuspend = fileToSuspend;
		m_fileWatcherService.Suspend(m_fileToSuspend.LocalPath);
	}

	public void Dispose()
	{
		m_fileWatcherService.Unsuspend(m_fileToSuspend.LocalPath);
	}
}
