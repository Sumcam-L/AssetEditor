namespace Firaxis.Threading;

internal interface IWriterLockProvider
{
	void EnterWriteLock();

	void ExitWriteLock();
}
