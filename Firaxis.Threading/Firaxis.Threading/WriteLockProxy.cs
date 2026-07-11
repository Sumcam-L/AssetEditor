using System.Threading;

namespace Firaxis.Threading;

internal class WriteLockProxy : IWriterLockProvider
{
	private readonly ReaderWriterLockSlim ReaderWriterLock;

	public WriteLockProxy(ReaderWriterLockSlim rwLock)
	{
		ReaderWriterLock = rwLock;
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
