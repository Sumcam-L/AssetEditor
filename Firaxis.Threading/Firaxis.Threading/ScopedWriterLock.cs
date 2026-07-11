using System;
using System.Threading;

namespace Firaxis.Threading;

public class ScopedWriterLock : IDisposable
{
	private IDisposable LockTimer;

	private readonly IWriterLockProvider ReaderWriterLock;

	public ScopedWriterLock(ScopedUpgrableReaderLock upgrableReadLock)
	{
		ReaderWriterLock = upgrableReadLock;
		using (ReaderWriterStatistics.TimeEnteringWriteLock(1))
		{
			ReaderWriterLock.EnterWriteLock();
		}
		LockTimer = ReaderWriterStatistics.TimeWriteLock(1);
	}

	public ScopedWriterLock(ReaderWriterLockSlim rwLock)
	{
		ReaderWriterLock = new WriteLockProxy(rwLock);
		using (ReaderWriterStatistics.TimeEnteringWriteLock(1))
		{
			ReaderWriterLock.EnterWriteLock();
		}
		LockTimer = ReaderWriterStatistics.TimeWriteLock(1);
	}

	public void Dispose()
	{
		ReaderWriterLock.ExitWriteLock();
		LockTimer?.Dispose();
	}
}
