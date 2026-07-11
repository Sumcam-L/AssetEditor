using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonSignal
{
	internal class PythonSignalState
	{
		public PythonContext SignalPythonContext;

		public Dictionary<int, object> PySignalToPyHandler;

		public PythonSignalState(PythonContext pc)
		{
			SignalPythonContext = pc;
			PySignalToPyHandler = new Dictionary<int, object>
			{
				{ 22, 0 },
				{ 21, 0 },
				{ 8, 0 },
				{ 4, 0 },
				{ 2, default_int_handler },
				{ 11, 0 },
				{ 15, 0 }
			};
		}
	}

	internal class NtSignalState : PythonSignalState
	{
		public NativeSignal.WinSignalsHandler WinAllSignalsHandlerDelegate;

		public NtSignalState(PythonContext pc)
			: base(pc)
		{
			WinAllSignalsHandlerDelegate = WindowsEventHandler;
			NativeSignal.SetConsoleCtrlHandler(WinAllSignalsHandlerDelegate, Add: true);
		}

		private bool WindowsEventHandler(uint winSignal)
		{
			int num = winSignal switch
			{
				0u => 2, 
				1u => 21, 
				2u => 21, 
				5u => 21, 
				6u => 21, 
				_ => throw new Exception("unreachable"), 
			};
			bool result;
			lock (PySignalToPyHandler)
			{
				if (PySignalToPyHandler[num].GetType() == typeof(int))
				{
					result = (int)PySignalToPyHandler[num] switch
					{
						0 => false, 
						1 => true, 
						_ => throw new Exception("unreachable"), 
					};
				}
				else if (PySignalToPyHandler[num] == default_int_handler)
				{
					if (num != 2)
					{
						result = true;
						default_int_handlerImpl(num, null);
					}
					else
					{
						result = false;
					}
				}
				else
				{
					result = true;
					PySignalHandler pySignalHandler = (PySignalHandler)Converter.ConvertToDelegate(PySignalToPyHandler[num], typeof(PySignalHandler));
					try
					{
						if (SignalPythonContext.PythonOptions.Frames)
						{
							pySignalHandler(num, SysModule._getframeImpl(null, 0, SignalPythonContext._mainThreadFunctionStack));
						}
						else
						{
							pySignalHandler(num, null);
						}
					}
					catch (Exception exception)
					{
						Console.WriteLine(SignalPythonContext.FormatException(exception));
					}
				}
			}
			return result;
		}
	}

	internal static class NativeSignal
	{
		internal delegate bool WinSignalsHandler(uint winSignal);

		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetConsoleCtrlHandler(WinSignalsHandler Handler, [MarshalAs(UnmanagedType.Bool)] bool Add);

		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
	}

	internal class SimpleSignalState : PythonSignalState
	{
		public SimpleSignalState(PythonContext pc)
			: base(pc)
		{
			Console.CancelKeyPress += Console_CancelKeyPress;
		}

		private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			int num = e.SpecialKey switch
			{
				ConsoleSpecialKey.ControlC => 2, 
				ConsoleSpecialKey.ControlBreak => 21, 
				_ => throw new InvalidOperationException("unreachable"), 
			};
			lock (PySignalToPyHandler)
			{
				if (PySignalToPyHandler[num].GetType() == typeof(int))
				{
					switch ((int)PySignalToPyHandler[num])
					{
					case 0:
						break;
					case 1:
						e.Cancel = false;
						break;
					default:
						throw new Exception("unreachable");
					}
					return;
				}
				if (PySignalToPyHandler[num] == default_int_handler)
				{
					if (num != 2)
					{
						e.Cancel = true;
						default_int_handlerImpl(num, null);
					}
					return;
				}
				PySignalHandler pySignalHandler = (PySignalHandler)Converter.ConvertToDelegate(PySignalToPyHandler[num], typeof(PySignalHandler));
				try
				{
					if (SignalPythonContext.PythonOptions.Frames)
					{
						pySignalHandler(num, SysModule._getframeImpl(null, 0, SignalPythonContext._mainThreadFunctionStack));
					}
					else
					{
						pySignalHandler(num, null);
					}
				}
				catch (Exception exception)
				{
					Console.WriteLine(SignalPythonContext.FormatException(exception));
				}
				e.Cancel = true;
			}
		}
	}

	private delegate object PySignalHandler(int signalnum, TraceBackFrame frame);

	public const string __doc__ = "This module provides mechanisms to use signal handlers in Python.\r\n\r\nFunctions:\r\n\r\nsignal() -- set the action for a given signal\r\ngetsignal() -- get the signal action for a given signal\r\ndefault_int_handler() -- default SIGINT handler\r\n\r\nsignal constants:\r\nSIG_DFL -- used to refer to the system default handler\r\nSIG_IGN -- used to ignore the signal\r\nNSIG -- number of defined signals\r\nSIGINT, SIGTERM, etc. -- signal numbers\r\n\r\n*** IMPORTANT NOTICE ***\r\nA signal handler function is called with two arguments:\r\nthe first is the signal number, the second is the interrupted stack frame.";

	public const int NSIG = 23;

	public const int SIGABRT = 22;

	public const int SIGBREAK = 21;

	public const int SIGFPE = 8;

	public const int SIGILL = 4;

	public const int SIGINT = 2;

	public const int SIGSEGV = 11;

	public const int SIGTERM = 15;

	public const int SIG_DFL = 0;

	public const int SIG_IGN = 1;

	public const int CTRL_C_EVENT = 0;

	public const int CTRL_BREAK_EVENT = 1;

	public const int CTRL_CLOSE_EVENT = 2;

	public const int CTRL_LOGOFF_EVENT = 5;

	public const int CTRL_SHUTDOWN_EVENT = 6;

	public static BuiltinFunction default_int_handler = BuiltinFunction.MakeFunction("default_int_handler", ArrayUtils.ConvertAll(typeof(PythonSignal).GetMember("default_int_handlerImpl"), (MemberInfo x) => (MethodBase)x), typeof(PythonSignal));

	private static readonly object _PythonSignalStateKey = new object();

	private static readonly int[] _PySupportedSignals = new int[8] { 22, 21, 8, 4, 2, 11, 15, 6 };

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.SetModuleState(_PythonSignalStateKey, MakeSignalState(context));
	}

	private static PythonSignalState MakeSignalState(PythonContext context)
	{
		if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
		{
			return MakePosixSignalState(context);
		}
		return MakeNtSignalState(context);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static PythonSignalState MakeNtSignalState(PythonContext context)
	{
		return new NtSignalState(context);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static PythonSignalState MakePosixSignalState(PythonContext context)
	{
		return new SimpleSignalState(context);
	}

	[PythonHidden]
	[Documentation("default_int_handler(...)\r\n\r\nThe default handler for SIGINT installed by Python.\r\nIt raises KeyboardInterrupt.")]
	public static object default_int_handlerImpl(int signalnum, TraceBackFrame frame)
	{
		throw new KeyboardInterruptException("");
	}

	[Documentation("getsignal(sig) -> action\r\n\r\nReturn the current action for the given signal.  The return value can be:\r\nSIG_IGN -- if the signal is being ignored\r\nSIG_DFL -- if the default action for the signal is in effect\r\nNone -- if an unknown handler is in effect\r\nanything else -- the callable Python object used as a handler")]
	public static object getsignal(CodeContext context, int signalnum)
	{
		lock (GetPythonSignalState(context).PySignalToPyHandler)
		{
			if (signalnum < 1 || signalnum > 22)
			{
				throw PythonOps.ValueError("signal number out of range");
			}
			if (!GetPythonSignalState(context).PySignalToPyHandler.ContainsKey(signalnum))
			{
				return null;
			}
			return GetPythonSignalState(context).PySignalToPyHandler[signalnum];
		}
	}

	[Documentation("signal(sig, action) -> action\r\n\r\nSet the action for the given signal.  The action can be SIG_DFL,\r\nSIG_IGN, or a callable Python object.  The previous action is\r\nreturned.  See getsignal() for possible return values.\r\n\r\n*** IMPORTANT NOTICE ***\r\nA signal handler function is called with two arguments:\r\nthe first is the signal number, the second is the interrupted stack frame.")]
	public static object signal(CodeContext context, int sig, object action)
	{
		if (sig < 1 || sig >= 23)
		{
			throw PythonOps.ValueError("signal number out of range");
		}
		if (Array.IndexOf(_PySupportedSignals, sig) == -1)
		{
			throw new RuntimeException("no IronPython support for given signal");
		}
		if (action == null)
		{
			throw PythonOps.TypeError("signal handler must be signal.SIG_IGN, signal.SIG_DFL, or a callable object");
		}
		if (action.GetType() == typeof(int))
		{
			int num = (int)action;
			if (num != 0 && num != 1)
			{
				throw PythonOps.TypeError("signal handler must be signal.SIG_IGN, signal.SIG_DFL, or a callable object");
			}
		}
		else if (action != default_int_handler)
		{
			PythonFunction pythonFunction = action as PythonFunction;
			if (pythonFunction == null && !PythonOps.IsCallable(context, action))
			{
				throw PythonOps.TypeError("signal handler must be signal.SIG_IGN, signal.SIG_DFL, or a callable object");
			}
		}
		object obj = null;
		lock (GetPythonSignalState(context).PySignalToPyHandler)
		{
			obj = getsignal(context, sig);
			GetPythonSignalState(context).PySignalToPyHandler[sig] = action;
			return obj;
		}
	}

	[Documentation("NOT YET IMPLEMENTED\r\n\r\nset_wakeup_fd(fd) -> fd\r\n\r\nSets the fd to be written to (with '\\0') when a signal\r\ncomes in.  A library can use this to wakeup select or poll.\r\nThe previous fd is returned.\r\n\r\nThe fd must be non-blocking.")]
	public static void set_wakeup_fd(CodeContext context, uint fd)
	{
		throw new NotImplementedException();
	}

	private static PythonSignalState GetPythonSignalState(CodeContext context)
	{
		return (PythonSignalState)PythonContext.GetContext(context).GetModuleState(_PythonSignalStateKey);
	}

	private static void SetPythonSignalState(CodeContext context, PythonSignalState pss)
	{
		PythonContext.GetContext(context).SetModuleState(_PythonSignalStateKey, pss);
	}
}
