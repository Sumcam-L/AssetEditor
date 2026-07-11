using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX.IO;

public class NativeFileStream : Stream
{
	private bool canRead;

	private bool canWrite;

	private bool canSeek;

	private IntPtr handle;

	private long position;

	public override bool CanRead => canRead;

	public override bool CanSeek => canSeek;

	public override bool CanWrite => canWrite;

	public override long Length
	{
		get
		{
			if (!NativeFile.GetFileSizeEx(handle, out var fileSize))
			{
				throw new IOException("Unable to get file length", MarshalGetLastWin32Error());
			}
			return fileSize;
		}
	}

	public override long Position
	{
		get
		{
			return position;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
			position = value;
		}
	}

	public NativeFileStream(string fileName, NativeFileMode fileMode, NativeFileAccess access, NativeFileShare share = NativeFileShare.Read)
	{
		handle = NativeFile.Create(fileName, access, share, IntPtr.Zero, fileMode, NativeFileOptions.None, IntPtr.Zero);
		if (handle == new IntPtr(-1))
		{
			int num = MarshalGetLastWin32Error();
			if (num == 2)
			{
				throw new FileNotFoundException("Unable to find file", fileName);
			}
			Result resultFromWin32Error = Result.GetResultFromWin32Error(num);
			throw new IOException(string.Format(CultureInfo.InvariantCulture, "Unable to open file {0}", new object[1] { fileName }), resultFromWin32Error.Code);
		}
		canRead = 0 != ((uint)access & 0x80000000u);
		canWrite = 0 != (access & NativeFileAccess.Write);
		canSeek = true;
	}

	private static int MarshalGetLastWin32Error()
	{
		return Marshal.GetLastWin32Error();
	}

	public override void Flush()
	{
		if (!NativeFile.FlushFileBuffers(handle))
		{
			throw new IOException("Unable to flush stream", MarshalGetLastWin32Error());
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (!NativeFile.SetFilePointerEx(handle, offset, out var distanceToMoveHigh, origin))
		{
			throw new IOException("Unable to seek to this position", MarshalGetLastWin32Error());
		}
		position = distanceToMoveHigh;
		return position;
	}

	public override void SetLength(long value)
	{
		if (!NativeFile.SetFilePointerEx(handle, value, out var _, SeekOrigin.Begin))
		{
			throw new IOException("Unable to seek to this position", MarshalGetLastWin32Error());
		}
		if (!NativeFile.SetEndOfFile(handle))
		{
			throw new IOException("Unable to set the new length", MarshalGetLastWin32Error());
		}
		if (position < value)
		{
			Seek(position, SeekOrigin.Begin);
		}
		else
		{
			Seek(0L, SeekOrigin.End);
		}
	}

	public unsafe override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		fixed (IntPtr* ptr = buffer)
		{
			return Read((IntPtr)ptr, offset, count);
		}
	}

	public unsafe int Read(IntPtr buffer, int offset, int count)
	{
		if (buffer == IntPtr.Zero)
		{
			throw new ArgumentNullException("buffer");
		}
		void* ptr = (byte*)(void*)buffer + offset;
		if (!NativeFile.ReadFile(handle, (IntPtr)ptr, count, out var numberOfBytesRead, IntPtr.Zero))
		{
			throw new IOException("Unable to read from file", MarshalGetLastWin32Error());
		}
		position += numberOfBytesRead;
		return numberOfBytesRead;
	}

	public unsafe override void Write(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		fixed (IntPtr* ptr = buffer)
		{
			Write((IntPtr)ptr, offset, count);
		}
	}

	public unsafe void Write(IntPtr buffer, int offset, int count)
	{
		if (buffer == IntPtr.Zero)
		{
			throw new ArgumentNullException("buffer");
		}
		void* ptr = (byte*)(void*)buffer + offset;
		if (!NativeFile.WriteFile(handle, (IntPtr)ptr, count, out var numberOfBytesRead, IntPtr.Zero))
		{
			throw new IOException("Unable to write to file", MarshalGetLastWin32Error());
		}
		position += numberOfBytesRead;
	}

	protected override void Dispose(bool disposing)
	{
		Utilities.CloseHandle(handle);
		handle = IntPtr.Zero;
		base.Dispose(disposing);
	}
}
