using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpDX.IO;

public static class NativeFile
{
	internal struct FILETIME
	{
		public uint DateTimeLow;

		public uint DateTimeHigh;

		public DateTime ToDateTime()
		{
			return DateTime.FromFileTimeUtc((long)(((ulong)DateTimeHigh << 32) | DateTimeLow));
		}
	}

	internal struct WIN32_FILE_ATTRIBUTE_DATA
	{
		public uint FileAttributes;

		public FILETIME CreationTime;

		public FILETIME LastAccessTime;

		public FILETIME LastWriteTime;

		public uint FileSizeHigh;

		public uint FileSizeLow;
	}

	private const string KERNEL_FILE = "kernel32.dll";

	public static bool Exists(string filePath)
	{
		try
		{
			string fullPath = Path.GetFullPath(filePath);
			if (GetFileAttributesEx(fullPath, 0, out var _))
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public static byte[] ReadAllBytes(string path)
	{
		using NativeFileStream nativeFileStream = new NativeFileStream(path, NativeFileMode.Open, NativeFileAccess.Read);
		int num = 0;
		long length = nativeFileStream.Length;
		if (length > int.MaxValue)
		{
			throw new IOException("File too long");
		}
		int num2 = (int)length;
		byte[] array = new byte[num2];
		while (num2 > 0)
		{
			int num3 = nativeFileStream.Read(array, num, num2);
			if (num3 == 0)
			{
				throw new EndOfStreamException();
			}
			num += num3;
			num2 -= num3;
		}
		return array;
	}

	public static string ReadAllText(string path)
	{
		return ReadAllText(path, Encoding.UTF8);
	}

	public static string ReadAllText(string path, Encoding encoding, NativeFileShare sharing = NativeFileShare.Read)
	{
		using NativeFileStream stream = new NativeFileStream(path, NativeFileMode.Open, NativeFileAccess.Read, sharing);
		using StreamReader streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, 1024);
		return streamReader.ReadToEnd();
	}

	public static DateTime GetLastWriteTime(string path)
	{
		if (GetFileAttributesEx(path, 0, out var lpFileInformation))
		{
			return lpFileInformation.LastWriteTime.ToDateTime().ToLocalTime();
		}
		return new DateTime(0L);
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool ReadFile(IntPtr fileHandle, IntPtr buffer, int numberOfBytesToRead, out int numberOfBytesRead, IntPtr overlapped);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool FlushFileBuffers(IntPtr hFile);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool WriteFile(IntPtr fileHandle, IntPtr buffer, int numberOfBytesToRead, out int numberOfBytesRead, IntPtr overlapped);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool SetFilePointerEx(IntPtr handle, long distanceToMove, out long distanceToMoveHigh, SeekOrigin seekOrigin);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool SetEndOfFile(IntPtr handle);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetFileAttributesExW", SetLastError = true)]
	internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateFile", SetLastError = true)]
	internal static extern IntPtr Create(string fileName, NativeFileAccess desiredAccess, NativeFileShare shareMode, IntPtr securityAttributes, NativeFileMode mode, NativeFileOptions flagsAndOptions, IntPtr templateFile);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	internal static extern bool GetFileSizeEx(IntPtr handle, out long fileSize);
}
