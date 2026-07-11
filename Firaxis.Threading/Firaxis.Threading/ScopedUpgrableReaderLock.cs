using System;
using System.Threading;

namespace Firaxis.Threading;

public class ScopedUpgrableReaderLock : IWriterLockProvider, IDisposable
{
	private IDisposable LockTimer;

	private readonly ReaderWriterLockSlim ReaderWriterLock;

	public ScopedUpgrableReaderLock(ReaderWriterLockSlim rwLock)
	{
		ReaderWriterLock = rwLock;
		using (ReaderWriterStatistics.TimeEnteringReadLock(1))
		{
			ReaderWriterLock.EnterUpgradeableReadLock();
		}
		LockTimer = ReaderWriterStatistics.TimeReadLock(1);
	}

	public void Dispose()
	{
		ReaderWriterLock.ExitUpgradeableReadLock();
		LockTimer?.Dispose();
	}

	public void EnterWriteLock()
	{
		ReaderWriterLock.EnterWriteLock();
	}

	public void ExitWriteLock()
	{
		ReaderWriterLock.ExitWriteLock();
	}
}
