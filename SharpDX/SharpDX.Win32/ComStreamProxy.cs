using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

[Guid("0000000c-0000-0000-C000-000000000046")]
internal class ComStreamProxy : CallbackBase, IStream, IStreamBase, ICallbackable, IDisposable
{
	private Stream sourceStream;

	private byte[] tempBuffer = new byte[4096];

	public ComStreamProxy(Stream sourceStream)
	{
		this.sourceStream = sourceStream;
	}

	public unsafe int Read(IntPtr buffer, int numberOfBytesToRead)
	{
		int num = 0;
		while (numberOfBytesToRead > 0)
		{
			int count = Math.Min(numberOfBytesToRead, tempBuffer.Length);
			int num2 = sourceStream.Read(tempBuffer, 0, count);
			if (num2 == 0)
			{
				return num;
			}
			Utilities.Write(new IntPtr(num + (byte*)(void*)buffer), tempBuffer, 0, num2);
			numberOfBytesToRead -= num2;
			num += num2;
		}
		return num;
	}

	public unsafe int Write(IntPtr buffer, int numberOfBytesToWrite)
	{
		int num = 0;
		while (numberOfBytesToWrite > 0)
		{
			int num2 = Math.Min(numberOfBytesToWrite, tempBuffer.Length);
			Utilities.Read(new IntPtr(num + (byte*)(void*)buffer), tempBuffer, 0, num2);
			sourceStream.Write(tempBuffer, 0, num2);
			numberOfBytesToWrite -= num2;
			num += num2;
		}
		return num;
	}

	public long Seek(long offset, SeekOrigin origin)
	{
		return sourceStream.Seek(offset, origin);
	}

	public void SetSize(long newSize)
	{
	}

	public unsafe long CopyTo(IStream streamDest, long numberOfBytesToCopy, out long bytesWritten)
	{
		bytesWritten = 0L;
		fixed (IntPtr* ptr = tempBuffer)
		{
			while (numberOfBytesToCopy > 0)
			{
				int count = (int)Math.Min(numberOfBytesToCopy, tempBuffer.Length);
				int num = sourceStream.Read(tempBuffer, 0, count);
				if (num == 0)
				{
					break;
				}
				streamDest.Write((IntPtr)ptr, num);
				numberOfBytesToCopy -= num;
				bytesWritten += num;
			}
		}
		return bytesWritten;
	}

	public void Commit(CommitFlags commitFlags)
	{
		sourceStream.Flush();
	}

	public void Revert()
	{
		throw new NotImplementedException();
	}

	public void LockRegion(long offset, long numberOfBytesToLock, LockType dwLockType)
	{
		throw new NotImplementedException();
	}

	public void UnlockRegion(long offset, long numberOfBytesToLock, LockType dwLockType)
	{
		throw new NotImplementedException();
	}

	public StorageStatistics GetStatistics(StorageStatisticsFlags storageStatisticsFlags)
	{
		long num = sourceStream.Length;
		if (num == 0)
		{
			num = 2147483647L;
		}
		return new StorageStatistics
		{
			Type = 2,
			CbSize = num,
			GrfLocksSupported = 2,
			GrfMode = 2
		};
	}

	public IStream Clone()
	{
		return new ComStreamProxy(sourceStream);
	}

	protected override void Dispose(bool disposing)
	{
		sourceStream = null;
		base.Dispose(disposing);
	}
}
