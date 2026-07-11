using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

[Shadow(typeof(ComStreamShadow))]
[Guid("0000000c-0000-0000-C000-000000000046")]
public interface IStream : IStreamBase, ICallbackable, IDisposable
{
	long Seek(long offset, SeekOrigin origin);

	void SetSize(long newSize);

	long CopyTo(IStream streamDest, long numberOfBytesToCopy, out long bytesWritten);

	void Commit(CommitFlags commitFlags);

	void Revert();

	void LockRegion(long offset, long numberOfBytesToLock, LockType dwLockType);

	void UnlockRegion(long offset, long numberOfBytesToLock, LockType dwLockType);

	StorageStatistics GetStatistics(StorageStatisticsFlags storageStatisticsFlags);

	IStream Clone();
}
