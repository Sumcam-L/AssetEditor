using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Modules;

[PythonType("_subprocess")]
public static class PythonSubprocess
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct STARTUPINFO
	{
		public int cb;

		public string lpReserved;

		public string lpDesktop;

		public string lpTitle;

		public int dwX;

		public int dwY;

		public int dwXSize;

		public int dwYSize;

		public int dwXCountChars;

		public int dwYCountChars;

		public int dwFillAttribute;

		public int dwFlags;

		public short wShowWindow;

		public short cbReserved2;

		public IntPtr lpReserved2;

		public IntPtr hStdInput;

		public IntPtr hStdOutput;

		public IntPtr hStdError;
	}

	internal struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;

		public IntPtr hThread;

		public int dwProcessId;

		public int dwThreadId;
	}

	internal struct SECURITY_ATTRIBUTES
	{
		public int nLength;

		public IntPtr lpSecurityDescriptor;

		public int bInheritHandle;
	}

	[Flags]
	internal enum DuplicateOptions : uint
	{
		DUPLICATE_CLOSE_SOURCE = 1u,
		DUPLICATE_SAME_ACCESS = 2u
	}

	public const string __doc__ = "_subprocess Module";

	public const int CREATE_NEW_CONSOLE = 16;

	public const int CREATE_NEW_PROCESS_GROUP = 512;

	public const int DUPLICATE_SAME_ACCESS = 2;

	public const int INFINITE = -1;

	public const int STARTF_USESHOWWINDOW = 1;

	public const int STARTF_USESTDHANDLES = 256;

	public const int STD_ERROR_HANDLE = -12;

	public const int STD_INPUT_HANDLE = -10;

	public const int STD_OUTPUT_HANDLE = -11;

	public const int SW_HIDE = 0;

	public const int WAIT_OBJECT_0 = 0;

	public const int PIPE = -1;

	public const int STDOUT = -2;

	public static PythonTuple CreatePipe(CodeContext context, object pSec, int bufferSize)
	{
		SECURITY_ATTRIBUTES lpPipeAttributes = default(SECURITY_ATTRIBUTES);
		lpPipeAttributes.nLength = Marshal.SizeOf((object)lpPipeAttributes);
		CreatePipePI(out var hReadPipe, out var hWritePipe, ref lpPipeAttributes, (uint)bufferSize);
		return PythonTuple.MakeTuple(new PythonSubprocessHandle(hReadPipe), new PythonSubprocessHandle(hWritePipe));
	}

	public static PythonTuple CreateProcess(CodeContext context, string applicationName, string commandLineArgs, object pSec, object tSec, int? bInheritHandles, uint? dwCreationFlags, PythonDictionary lpEnvironment, string lpCurrentDirectory, object lpStartupInfo)
	{
		object boundAttr = PythonOps.GetBoundAttr(context, lpStartupInfo, "dwFlags");
		object boundAttr2 = PythonOps.GetBoundAttr(context, lpStartupInfo, "hStdInput");
		object boundAttr3 = PythonOps.GetBoundAttr(context, lpStartupInfo, "hStdOutput");
		object boundAttr4 = PythonOps.GetBoundAttr(context, lpStartupInfo, "hStdError");
		object boundAttr5 = PythonOps.GetBoundAttr(context, lpStartupInfo, "wShowWindow");
		int dwFlags = ((boundAttr != null) ? Converter.ConvertToInt32(boundAttr) : 0);
		IntPtr hStdInput = ((boundAttr2 != null) ? new IntPtr(Converter.ConvertToInt32(boundAttr2)) : IntPtr.Zero);
		IntPtr hStdOutput = ((boundAttr3 != null) ? new IntPtr(Converter.ConvertToInt32(boundAttr3)) : IntPtr.Zero);
		IntPtr hStdError = ((boundAttr4 != null) ? new IntPtr(Converter.ConvertToInt32(boundAttr4)) : IntPtr.Zero);
		short wShowWindow = (short)((boundAttr5 != null) ? Converter.ConvertToInt16(boundAttr5) : 0);
		STARTUPINFO lpStartupInfo2 = new STARTUPINFO
		{
			dwFlags = dwFlags,
			hStdInput = hStdInput,
			hStdOutput = hStdOutput,
			hStdError = hStdError,
			wShowWindow = wShowWindow
		};
		SECURITY_ATTRIBUTES lpProcessAttributes = default(SECURITY_ATTRIBUTES);
		lpProcessAttributes.nLength = Marshal.SizeOf((object)lpProcessAttributes);
		SECURITY_ATTRIBUTES lpThreadAttributes = default(SECURITY_ATTRIBUTES);
		lpThreadAttributes.nLength = Marshal.SizeOf((object)lpThreadAttributes);
		string lpEnvironment2 = EnvironmentToNative(lpEnvironment);
		PROCESS_INFORMATION lpProcessInformation = default(PROCESS_INFORMATION);
		if (!CreateProcessPI(string.IsNullOrEmpty(applicationName) ? null : applicationName, string.IsNullOrEmpty(commandLineArgs) ? null : commandLineArgs, ref lpProcessAttributes, ref lpThreadAttributes, (bInheritHandles.HasValue && bInheritHandles.Value > 0) ? true : false, dwCreationFlags.HasValue ? dwCreationFlags.Value : 0u, lpEnvironment2, lpCurrentDirectory, ref lpStartupInfo2, out lpProcessInformation))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, lastWin32Error, CTypes.FormatError(lastWin32Error));
		}
		IntPtr hProcess = lpProcessInformation.hProcess;
		IntPtr hThread = lpProcessInformation.hThread;
		int dwProcessId = lpProcessInformation.dwProcessId;
		int dwThreadId = lpProcessInformation.dwThreadId;
		return PythonTuple.MakeTuple(new PythonSubprocessHandle(hProcess, isProcess: true), new PythonSubprocessHandle(hThread), dwProcessId, dwThreadId);
	}

	private static string EnvironmentToNative(PythonDictionary lpEnvironment)
	{
		if (lpEnvironment == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<object, object> item in lpEnvironment)
		{
			stringBuilder.Append(item.Key);
			stringBuilder.Append('=');
			stringBuilder.Append(item.Value);
			stringBuilder.Append('\0');
		}
		return stringBuilder.ToString();
	}

	public static PythonSubprocessHandle DuplicateHandle(CodeContext context, BigInteger sourceProcess, PythonSubprocessHandle handle, BigInteger targetProcess, int desiredAccess, bool inherit_handle, object DUPLICATE_SAME_ACCESS)
	{
		if (handle._duplicated)
		{
			return DuplicateHandle(context, sourceProcess, (BigInteger)handle, targetProcess, desiredAccess, inherit_handle, DUPLICATE_SAME_ACCESS);
		}
		PythonSubprocessHandle pythonSubprocessHandle = DuplicateHandle(context, sourceProcess, (BigInteger)handle, targetProcess, desiredAccess, inherit_handle, DUPLICATE_SAME_ACCESS);
		pythonSubprocessHandle._duplicated = true;
		handle.Close();
		return pythonSubprocessHandle;
	}

	public static PythonSubprocessHandle DuplicateHandle(CodeContext context, BigInteger sourceProcess, BigInteger handle, BigInteger targetProcess, int desiredAccess, bool inherit_handle, object DUPLICATE_SAME_ACCESS)
	{
		IntPtr hSourceProcessHandle = new IntPtr((long)sourceProcess);
		IntPtr hSourceHandle = new IntPtr((long)handle);
		IntPtr hTargetProcessHandle = new IntPtr((long)targetProcess);
		bool flag = DUPLICATE_SAME_ACCESS != null && Converter.ConvertToBoolean(DUPLICATE_SAME_ACCESS);
		DuplicateHandlePI(hSourceProcessHandle, hSourceHandle, hTargetProcessHandle, out var lpTargetHandle, Converter.ConvertToUInt32(desiredAccess), inherit_handle, (!flag) ? 1u : 2u);
		return new PythonSubprocessHandle(lpTargetHandle);
	}

	public static PythonSubprocessHandle GetCurrentProcess()
	{
		IntPtr currentProcessPI = GetCurrentProcessPI();
		return new PythonSubprocessHandle(currentProcessPI);
	}

	public static int GetExitCodeProcess(PythonSubprocessHandle hProcess)
	{
		if (hProcess._isProcess && hProcess._closed)
		{
			return hProcess._exitCode;
		}
		IntPtr hProcess2 = new IntPtr(Converter.ConvertToInt32(hProcess));
		int lpExitCode = int.MinValue;
		GetExitCodeProcessPI(hProcess2, out lpExitCode);
		return lpExitCode;
	}

	public static string GetModuleFileName(object ignored)
	{
		return Process.GetCurrentProcess().MainModule.FileName;
	}

	public static object GetStdHandle(int STD_OUTPUT_HANDLE)
	{
		return GetStdHandlePI(STD_OUTPUT_HANDLE).ToPython();
	}

	public static int GetVersion()
	{
		return GetVersionPI();
	}

	public static bool TerminateProcess(PythonSubprocessHandle handle, object uExitCode)
	{
		IntPtr hProcess = new IntPtr(Converter.ConvertToInt32(handle));
		uint uExitCode2 = Converter.ConvertToUInt32(uExitCode);
		return TerminateProcessPI(hProcess, uExitCode2);
	}

	public static int WaitForSingleObject(PythonSubprocessHandle handle, int dwMilliseconds)
	{
		return WaitForSingleObjectPI(handle, dwMilliseconds);
	}

	[DllImport("kernel32.dll", EntryPoint = "CreateProcess", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CreateProcessPI(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles, uint dwCreationFlags, string lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

	[DllImport("kernel32.dll", EntryPoint = "CreatePipe")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CreatePipePI(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

	[DllImport("kernel32.dll", EntryPoint = "DuplicateHandle", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DuplicateHandlePI(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

	[DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
	private static extern IntPtr GetCurrentProcessPI();

	[DllImport("kernel32.dll", EntryPoint = "GetExitCodeProcess", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetExitCodeProcessPI(IntPtr hProcess, out int lpExitCode);

	[DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true)]
	private static extern IntPtr GetStdHandlePI(int nStdHandle);

	[DllImport("kernel32.dll", EntryPoint = "GetVersion")]
	private static extern int GetVersionPI();

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll", EntryPoint = "TerminateProcess", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool TerminateProcessPI(IntPtr hProcess, uint uExitCode);

	[DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "WaitForSingleObject", ExactSpelling = true, SetLastError = true)]
	internal static extern int WaitForSingleObjectPI(IntPtr hHandle, int dwMilliseconds);
}
