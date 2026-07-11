using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Firaxis.ATF;

public static class DebugHelp
{
	[Flags]
	public enum Option : uint
	{
		Normal = 0u,
		WithDataSegs = 1u,
		WithFullMemory = 2u,
		WithHandleData = 4u,
		FilterMemory = 8u,
		ScanMemory = 0x10u,
		WithUnloadedModules = 0x20u,
		WithIndirectlyReferencedMemory = 0x40u,
		FilterModulePaths = 0x80u,
		WithProcessThreadData = 0x100u,
		WithPrivateReadWriteMemory = 0x200u,
		WithoutOptionalData = 0x400u,
		WithFullMemoryInfo = 0x800u,
		WithThreadInfo = 0x1000u,
		WithCodeSegs = 0x2000u,
		WithoutAuxiliaryState = 0x4000u,
		WithFullAuxiliaryState = 0x8000u,
		WithPrivateWriteCopyMemory = 0x10000u,
		IgnoreInaccessibleMemory = 0x20000u,
		ValidTypeFlags = 0x3FFFFu
	}

	public enum ExceptionInfo
	{
		None,
		Present
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct MiniDumpExceptionInformation
	{
		public uint ThreadId;

		public IntPtr ExceptionPointers;

		[MarshalAs(UnmanagedType.Bool)]
		public bool ClientPointers;
	}

	[DllImport("dbghelp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

	[DllImport("dbghelp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

	[DllImport("kernel32.dll", ExactSpelling = true)]
	private static extern uint GetCurrentThreadId();

	public static bool WriteMiniDump(SafeHandle fileHandle, Option options, ExceptionInfo exceptionInfo)
	{
		Process currentProcess = Process.GetCurrentProcess();
		IntPtr handle = currentProcess.Handle;
		uint id = (uint)currentProcess.Id;
		MiniDumpExceptionInformation expParam = new MiniDumpExceptionInformation
		{
			ThreadId = GetCurrentThreadId(),
			ClientPointers = false,
			ExceptionPointers = IntPtr.Zero
		};
		if (exceptionInfo == ExceptionInfo.Present)
		{
			expParam.ExceptionPointers = Marshal.GetExceptionPointers();
		}
		if (expParam.ExceptionPointers == IntPtr.Zero)
		{
			return MiniDumpWriteDump(handle, id, fileHandle, (uint)options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}
		return MiniDumpWriteDump(handle, id, fileHandle, (uint)options, ref expParam, IntPtr.Zero, IntPtr.Zero);
	}

	public static bool WriteMiniDump(SafeHandle fileHandle, Option dumpType)
	{
		return WriteMiniDump(fileHandle, dumpType, ExceptionInfo.None);
	}

	public static bool WriteMiniDump(FileStream fileStream, Option dumpType)
	{
		return WriteMiniDump(fileStream.SafeFileHandle, dumpType, ExceptionInfo.Present);
	}
}
