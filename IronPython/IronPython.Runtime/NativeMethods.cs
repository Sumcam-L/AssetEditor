using System;
using System.Runtime.InteropServices;

namespace IronPython.Runtime;

internal class NativeMethods
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	[BestFitMapping(false)]
	internal struct WIN32_FIND_DATA
	{
		internal int dwFileAttributes;

		internal uint ftCreationTime_dwLowDateTime;

		internal uint ftCreationTime_dwHighDateTime;

		internal uint ftLastAccessTime_dwLowDateTime;

		internal uint ftLastAccessTime_dwHighDateTime;

		internal uint ftLastWriteTime_dwLowDateTime;

		internal uint ftLastWriteTime_dwHighDateTime;

		internal int nFileSizeHigh;

		internal int nFileSizeLow;

		internal int dwReserved0;

		internal int dwReserved1;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string cFileName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		internal string cAlternateFileName;
	}

	public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

	[DllImport("kernel32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
	internal static extern IntPtr FindFirstFile(string fileName, out WIN32_FIND_DATA data);

	[DllImport("kernel32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool FindNextFile(IntPtr hndFindFile, out WIN32_FIND_DATA lpFindFileData);

	[DllImport("kernel32")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool FindClose(IntPtr handle);
}
