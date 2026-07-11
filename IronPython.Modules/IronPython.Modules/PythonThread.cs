using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonThread
{
	[PythonHidden]
	[PythonType]
	public class @lock
	{
		private AutoResetEvent blockEvent;

		private Thread curHolder;

		public object __enter__()
		{
			acquire();
			return this;
		}

		public void __exit__(CodeContext context, params object[] args)
		{
			release(context);
		}

		public object acquire()
		{
			return acquire(ScriptingRuntimeHelpers.True);
		}

		public object acquire(object waitflag)
		{
			bool flag = PythonOps.IsTrue(waitflag);
			while (true)
			{
				if (Interlocked.CompareExchange(ref curHolder, Thread.CurrentThread, null) == null)
				{
					return ScriptingRuntimeHelpers.True;
				}
				if (!flag)
				{
					break;
				}
				if (blockEvent == null)
				{
					CreateBlockEvent();
					continue;
				}
				blockEvent.WaitOne();
				GC.KeepAlive(this);
			}
			return ScriptingRuntimeHelpers.False;
		}

		public void release(CodeContext context, params object[] param)
		{
			release(context);
		}

		public void release(CodeContext context)
		{
			if (Interlocked.Exchange(ref curHolder, null) == null)
			{
				throw PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState("threaderror"), "lock isn't held", null);
			}
			if (blockEvent != null)
			{
				blockEvent.Set();
				GC.KeepAlive(this);
			}
		}

		public bool locked()
		{
			return curHolder != null;
		}

		private void CreateBlockEvent()
		{
			AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
			if (Interlocked.CompareExchange(ref blockEvent, autoResetEvent, null) != null)
			{
				autoResetEvent.Close();
			}
		}
	}

	private class ThreadObj
	{
		private readonly object _func;

		private readonly object _kwargs;

		private readonly PythonTuple _args;

		private readonly CodeContext _context;

		public ThreadObj(CodeContext context, object function, PythonTuple args, object kwargs)
		{
			_func = function;
			_kwargs = kwargs;
			_args = args;
			_context = context;
		}

		public void Start()
		{
			lock (_threadCountKey)
			{
				int num = (int)_context.LanguageContext.GetOrCreateModuleState(_threadCountKey, (Func<object>)(() => 0));
				_context.LanguageContext.SetModuleState(_threadCountKey, num + 1);
			}
			try
			{
				if (_kwargs != null)
				{
					PythonOps.CallWithArgsTupleAndKeywordDictAndContext(_context, _func, ArrayUtils.EmptyObjects, ArrayUtils.EmptyStrings, _args, _kwargs);
				}
				else
				{
					PythonOps.CallWithArgsTuple(_func, ArrayUtils.EmptyObjects, _args);
				}
			}
			catch (SystemExitException)
			{
			}
			catch (Exception exception)
			{
				PythonOps.PrintWithDest(_context, PythonContext.GetContext(_context).SystemStandardError, "Unhandled exception on thread");
				string o = _context.LanguageContext.FormatException(exception);
				PythonOps.PrintWithDest(_context, PythonContext.GetContext(_context).SystemStandardError, o);
			}
			finally
			{
				lock (_threadCountKey)
				{
					int num2 = (int)_context.LanguageContext.GetModuleState(_threadCountKey);
					_context.LanguageContext.SetModuleState(_threadCountKey, num2 - 1);
				}
			}
		}
	}

	[PythonType]
	public class _local
	{
		private class ThreadLocalDictionaryStorage : DictionaryStorage
		{
			private readonly Microsoft.Scripting.Utils.ThreadLocal<CommonDictionaryStorage> _storage = new Microsoft.Scripting.Utils.ThreadLocal<CommonDictionaryStorage>();

			public override int Count => GetStorage().Count;

			public override void Add(ref DictionaryStorage storage, object key, object value)
			{
				GetStorage().Add(key, value);
			}

			public override bool Contains(object key)
			{
				return GetStorage().Contains(key);
			}

			public override bool Remove(ref DictionaryStorage storage, object key)
			{
				return GetStorage().Remove(ref storage, key);
			}

			public override bool TryGetValue(object key, out object value)
			{
				return GetStorage().TryGetValue(key, out value);
			}

			public override void Clear(ref DictionaryStorage storage)
			{
				GetStorage().Clear(ref storage);
			}

			public override List<KeyValuePair<object, object>> GetItems()
			{
				return GetStorage().GetItems();
			}

			private CommonDictionaryStorage GetStorage()
			{
				return _storage.GetOrCreate(() => new CommonDictionaryStorage());
			}
		}

		private readonly PythonDictionary _dict = new PythonDictionary(new ThreadLocalDictionaryStorage());

		public PythonDictionary __dict__ => _dict;

		[SpecialName]
		public object GetCustomMember(string name)
		{
			return _dict.get(name, OperationFailed.Value);
		}

		[SpecialName]
		public void SetMemberAfter(string name, object value)
		{
			_dict[name] = value;
		}

		[SpecialName]
		public void DeleteMember(string name)
		{
			_dict.__delitem__(name);
		}
	}

	public const string __doc__ = "Provides low level primitives for threading.";

	private static readonly object _stackSizeKey = new object();

	private static object _threadCountKey = new object();

	public static readonly PythonType LockType = DynamicHelpers.GetPythonTypeFromType(typeof(@lock));

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.SetModuleState(_stackSizeKey, 0);
		context.EnsureModuleException("threaderror", dict, "error", "thread");
	}

	[Documentation("start_new_thread(function, [args, [kwDict]]) -> thread id\nCreates a new thread running the given function")]
	public static object start_new_thread(CodeContext context, object function, object args, object kwDict)
	{
		if (!(args is PythonTuple args2))
		{
			throw PythonOps.TypeError("2nd arg must be a tuple");
		}
		Thread thread = CreateThread(context, new ThreadObj(context, function, args2, kwDict).Start);
		thread.Start();
		return thread.ManagedThreadId;
	}

	[Documentation("start_new_thread(function, args, [kwDict]) -> thread id\nCreates a new thread running the given function")]
	public static object start_new_thread(CodeContext context, object function, object args)
	{
		if (!(args is PythonTuple args2))
		{
			throw PythonOps.TypeError("2nd arg must be a tuple");
		}
		Thread thread = CreateThread(context, new ThreadObj(context, function, args2, null).Start);
		thread.IsBackground = true;
		thread.Start();
		return thread.ManagedThreadId;
	}

	public static void interrupt_main(CodeContext context)
	{
		Thread mainThread = context.LanguageContext.MainThread;
		if (mainThread != null)
		{
			mainThread.Abort(new KeyboardInterruptException(""));
			return;
		}
		throw PythonOps.SystemError("no main thread has been registered");
	}

	public static void exit()
	{
		PythonOps.SystemExit();
	}

	[Documentation("allocate_lock() -> lock object\nAllocates a new lock object that can be used for synchronization")]
	public static object allocate_lock()
	{
		return new @lock();
	}

	public static object get_ident()
	{
		return Thread.CurrentThread.ManagedThreadId;
	}

	public static int stack_size(CodeContext context)
	{
		return GetStackSize(context);
	}

	public static int stack_size(CodeContext context, int size)
	{
		if (size < 262144 && size != 0)
		{
			throw PythonOps.ValueError("size too small: {0}", size);
		}
		int stackSize = GetStackSize(context);
		SetStackSize(context, size);
		return stackSize;
	}

	[Documentation("start_new(function, [args, [kwDict]]) -> thread id\nCreates a new thread running the given function")]
	public static object start_new(CodeContext context, object function, object args)
	{
		return start_new_thread(context, function, args);
	}

	public static void exit_thread()
	{
		exit();
	}

	public static object allocate()
	{
		return allocate_lock();
	}

	public static int _count(CodeContext context)
	{
		return (int)context.LanguageContext.GetOrCreateModuleState(_threadCountKey, (Func<object>)(() => 0));
	}

	private static Thread CreateThread(CodeContext context, ThreadStart start)
	{
		int stackSize = GetStackSize(context);
		if (stackSize == 0)
		{
			return new Thread(start);
		}
		return new Thread(start, stackSize);
	}

	private static int GetStackSize(CodeContext context)
	{
		return (int)PythonContext.GetContext(context).GetModuleState(_stackSizeKey);
	}

	private static void SetStackSize(CodeContext context, int stackSize)
	{
		PythonContext.GetContext(context).SetModuleState(_stackSizeKey, stackSize);
	}
}
