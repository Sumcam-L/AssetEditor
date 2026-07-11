using System;
using System.Threading;

namespace Firaxis.Threading;

public class ScopedReaderLock : IDisposable
{
	private readonly ReaderWriterLockSlim ReaderWriterLock;

	private IDisposable LockTimer;

	public ScopedReaderLock(ReaderWriterLockSlim rwLock)
	{
		ReaderWriterLock = rwLock;
		using (ReaderWriterStatistics.TimeEnteringReadLock(1))
		{
			ReaderWriterLock.EnterReadLock();
		}
		LockTimer = ReaderWriterStatistics.TimeReadLock(1);
	}

	public void Dispose()
	{
		ReaderWriterLock.ExitReadLock();
		LockTimer?.Dispose();
	}
}
