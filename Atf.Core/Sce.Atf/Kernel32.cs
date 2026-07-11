using System.Runtime.InteropServices;
using System.Text;

namespace Sce.Atf;

public static class Kernel32
{
	[StructLayout(LayoutKind.Sequential)]
	private class MEMORYSTATUSEX
	{
		public uint dwLength = 64u;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}

	[DllImport("kernel32.dll")]
	public static extern uint QueryDosDeviceW([In][MarshalAs(UnmanagedType.LPWStr)] string lpDeviceName, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpTargetPath, uint ucchMax);

	[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);

	public static int GetPhysicalMemoryMB()
	{
		MEMORYSTATUSEX mEMORYSTATUSEX = new MEMORYSTATUSEX();
		GlobalMemoryStatusEx(mEMORYSTATUSEX);
		return (int)(mEMORYSTATUSEX.ullTotalPhys / 1048576);
	}
}
