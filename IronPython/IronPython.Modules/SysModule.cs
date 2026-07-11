using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class SysModule
{
	[PythonType("sys.getwindowsversion")]
	[PythonHidden]
	public class windows_version : PythonTuple
	{
		public const int n_fields = 5;

		public const int n_sequence_fields = 5;

		public const int n_unnamed_fields = 0;

		public readonly int major;

		public readonly int minor;

		public readonly int build;

		public readonly int platform;

		public readonly string service_pack;

		internal windows_version(int major, int minor, int build, int platform, string service_pack)
			: base(new object[5] { major, minor, build, platform, service_pack })
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
			this.platform = platform;
			this.service_pack = service_pack;
		}

		public override string __repr__(CodeContext context)
		{
			return $"sys.getwindowsversion(major={major}, minor={minor}, build={build}, platform={platform}, service_pack='{service_pack}')";
		}
	}

	[PythonHidden]
	[PythonType("flags")]
	[DontMapIEnumerableToIter]
	public sealed class SysFlags : IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable
	{
		private const string _className = "sys.flags";

		private const int INDEX_DEBUG = 0;

		private const int INDEX_PY3K_WARNING = 1;

		private const int INDEX_DIVISION_WARNING = 2;

		private const int INDEX_DIVISION_NEW = 3;

		private const int INDEX_INSPECT = 4;

		private const int INDEX_INTERACTIVE = 5;

		private const int INDEX_OPTIMIZE = 6;

		private const int INDEX_DONT_WRITE_BYTECODE = 7;

		private const int INDEX_NO_USER_SITE = 8;

		private const int INDEX_NO_SITE = 9;

		private const int INDEX_IGNORE_ENVIRONMENT = 10;

		private const int INDEX_TABCHECK = 11;

		private const int INDEX_VERBOSE = 12;

		private const int INDEX_UNICODE = 13;

		private const int INDEX_BYTES_WARNING = 14;

		public const int n_fields = 15;

		public const int n_sequence_fields = 15;

		public const int n_unnamed_fields = 0;

		private static readonly string[] _keys = new string[15]
		{
			"debug", "py3k_warning", "division_warning", "division_new", "inspect", "interactive", "optimize", "dont_write_bytecode", "no_user_site", "no_site",
			"ignore_environment", "tabcheck", "verbose", "unicode", "bytes_warning"
		};

		private object[] _values = new object[15]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0
		};

		private PythonTuple __tuple;

		private string __string;

		private bool _modified = true;

		private PythonTuple _tuple
		{
			get
			{
				_Refresh();
				return __tuple;
			}
		}

		private string _string
		{
			get
			{
				_Refresh();
				return __string;
			}
		}

		public int Count
		{
			[PythonHidden]
			get
			{
				return 15;
			}
		}

		bool ICollection<object>.IsReadOnly => true;

		public object this[int i] => _tuple[i];

		public object this[BigInteger i] => this[(int)i];

		public object this[Slice s] => _tuple[s];

		public object this[object o] => this[Converter.ConvertToIndex(o)];

		object IList<object>.this[int index]
		{
			get
			{
				return _tuple[index];
			}
			set
			{
				throw new InvalidOperationException("sys.flags is readonly");
			}
		}

		public int debug
		{
			get
			{
				return _GetVal(0);
			}
			internal set
			{
				_SetVal(0, value);
			}
		}

		public int py3k_warning
		{
			get
			{
				return _GetVal(1);
			}
			internal set
			{
				_SetVal(1, value);
			}
		}

		public int division_warning
		{
			get
			{
				return _GetVal(2);
			}
			internal set
			{
				_SetVal(2, value);
			}
		}

		public int division_new
		{
			get
			{
				return _GetVal(3);
			}
			internal set
			{
				_SetVal(3, value);
			}
		}

		public int inspect
		{
			get
			{
				return _GetVal(4);
			}
			internal set
			{
				_SetVal(4, value);
			}
		}

		public int interactive
		{
			get
			{
				return _GetVal(5);
			}
			internal set
			{
				_SetVal(5, value);
			}
		}

		public int optimize
		{
			get
			{
				return _GetVal(6);
			}
			internal set
			{
				_SetVal(6, value);
			}
		}

		public int dont_write_bytecode
		{
			get
			{
				return _GetVal(7);
			}
			internal set
			{
				_SetVal(7, value);
			}
		}

		public int no_user_site
		{
			get
			{
				return _GetVal(8);
			}
			internal set
			{
				_SetVal(8, value);
			}
		}

		public int no_site
		{
			get
			{
				return _GetVal(9);
			}
			internal set
			{
				_SetVal(9, value);
			}
		}

		public int ignore_environment
		{
			get
			{
				return _GetVal(10);
			}
			internal set
			{
				_SetVal(10, value);
			}
		}

		public int tabcheck
		{
			get
			{
				return _GetVal(11);
			}
			internal set
			{
				_SetVal(11, value);
			}
		}

		public int verbose
		{
			get
			{
				return _GetVal(12);
			}
			internal set
			{
				_SetVal(12, value);
			}
		}

		public int unicode
		{
			get
			{
				return _GetVal(13);
			}
			internal set
			{
				_SetVal(13, value);
			}
		}

		public int bytes_warning
		{
			get
			{
				return _GetVal(14);
			}
			internal set
			{
				_SetVal(14, value);
			}
		}

		internal SysFlags()
		{
		}

		public override string ToString()
		{
			return _string;
		}

		public string __repr__()
		{
			return _string;
		}

		private void _Refresh()
		{
			if (!_modified)
			{
				return;
			}
			__tuple = PythonTuple.MakeTuple(_values);
			StringBuilder stringBuilder = new StringBuilder("sys.flags(");
			for (int i = 0; i < 15; i++)
			{
				if (_keys[i] == null)
				{
					stringBuilder.Append(_values[i]);
				}
				else
				{
					stringBuilder.AppendFormat("{0}={1}", _keys[i], _values[i]);
				}
				if (i < 14)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					stringBuilder.Append(')');
				}
			}
			__string = stringBuilder.ToString();
			_modified = false;
		}

		private int _GetVal(int index)
		{
			return (int)_values[index];
		}

		private void _SetVal(int index, int value)
		{
			if ((int)_values[index] != value)
			{
				_modified = true;
				_values[index] = value;
			}
		}

		void ICollection<object>.Add(object item)
		{
			throw new InvalidOperationException("sys.flags is readonly");
		}

		void ICollection<object>.Clear()
		{
			throw new InvalidOperationException("sys.flags is readonly");
		}

		[PythonHidden]
		public bool Contains(object item)
		{
			return _tuple.Contains(item);
		}

		[PythonHidden]
		public void CopyTo(object[] array, int arrayIndex)
		{
			_tuple.CopyTo(array, arrayIndex);
		}

		bool ICollection<object>.Remove(object item)
		{
			throw new InvalidOperationException("sys.flags is readonly");
		}

		[PythonHidden]
		public IEnumerator GetEnumerator()
		{
			return _tuple.GetEnumerator();
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			return ((IEnumerable<object>)_tuple).GetEnumerator();
		}

		public int __len__()
		{
			return 15;
		}

		public object __getslice__(int start, int end)
		{
			return _tuple.__getslice__(start, end);
		}

		[PythonHidden]
		public int IndexOf(object item)
		{
			return _tuple.IndexOf(item);
		}

		void IList<object>.Insert(int index, object item)
		{
			throw new InvalidOperationException("sys.flags is readonly");
		}

		void IList<object>.RemoveAt(int index)
		{
			throw new InvalidOperationException("sys.flags is readonly");
		}

		public static PythonTuple operator +([NotNull] SysFlags f, [NotNull] PythonTuple t)
		{
			return f._tuple + t;
		}

		public static PythonTuple operator *([NotNull] SysFlags f, int n)
		{
			return f._tuple * n;
		}

		public static PythonTuple operator *(int n, [NotNull] SysFlags f)
		{
			return f._tuple * n;
		}

		public static object operator *([NotNull] SysFlags f, [NotNull] Index n)
		{
			return f._tuple * n;
		}

		public static object operator *([NotNull] Index n, [NotNull] SysFlags f)
		{
			return f._tuple * n;
		}

		public static object operator *([NotNull] SysFlags f, object n)
		{
			return f._tuple * n;
		}

		public static object operator *(object n, [NotNull] SysFlags f)
		{
			return f._tuple * n;
		}

		public static bool operator >(SysFlags f, PythonTuple t)
		{
			return f._tuple > t;
		}

		public static bool operator <(SysFlags f, PythonTuple t)
		{
			return f._tuple < t;
		}

		public static bool operator >=(SysFlags f, PythonTuple t)
		{
			return f._tuple >= t;
		}

		public static bool operator <=(SysFlags f, PythonTuple t)
		{
			return f._tuple <= t;
		}

		public override bool Equals(object obj)
		{
			if (obj is SysFlags)
			{
				return _tuple.Equals(((SysFlags)obj)._tuple);
			}
			return _tuple.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _tuple.GetHashCode();
		}
	}

	[PythonHidden]
	[PythonType("sys.long_info")]
	public class longinfo : PythonTuple
	{
		public const int n_fields = 2;

		public const int n_sequence_fields = 2;

		public const int n_unnamed_fields = 0;

		public readonly int bits_per_digit;

		public readonly int sizeof_digit;

		internal longinfo(int bits_per_digit, int sizeof_digit)
			: base(new object[2] { bits_per_digit, sizeof_digit })
		{
			this.bits_per_digit = bits_per_digit;
			this.sizeof_digit = sizeof_digit;
		}

		public override string __repr__(CodeContext context)
		{
			return $"sys.long_info(bits_per_digit={bits_per_digit}, sizeof_digit={sizeof_digit})";
		}
	}

	[PythonType("sys.float_info")]
	[PythonHidden]
	public class floatinfo : PythonTuple
	{
		public const int n_fields = 11;

		public const int n_sequence_fields = 11;

		public const int n_unnamed_fields = 0;

		public readonly double max;

		public readonly int max_exp;

		public readonly int max_10_exp;

		public readonly double min;

		public readonly int min_exp;

		public readonly int min_10_exp;

		public readonly int dig;

		public readonly int mant_dig;

		public readonly double epsilon;

		public readonly int radix;

		public readonly int rounds;

		internal floatinfo(double max, int max_exp, int max_10_exp, double min, int min_exp, int min_10_exp, int dig, int mant_dig, double epsilon, int radix, int rounds)
			: base(new object[11]
			{
				max, max_exp, max_10_exp, min, min_exp, min_10_exp, dig, mant_dig, epsilon, radix,
				rounds
			})
		{
			this.max = max;
			this.max_exp = max_exp;
			this.max_10_exp = max_10_exp;
			this.min = min;
			this.min_exp = min_exp;
			this.min_10_exp = min_10_exp;
			this.dig = dig;
			this.mant_dig = mant_dig;
			this.epsilon = epsilon;
			this.radix = radix;
			this.rounds = rounds;
		}

		public override string __repr__(CodeContext context)
		{
			return $"sys.float_info(max={max}, max_exp={max_exp}, max_10_exp={max_10_exp},min={min}, min_exp={min_exp}, min_10_exp={min_10_exp},dig={dig}, mant_dig={mant_dig}, epsilon={epsilon}, radix={radix}, rounds={rounds})";
		}
	}

	public const string __doc__ = "Provides access to functions which query or manipulate the Python runtime.";

	public const int api_version = 0;

	public const string copyright = "Copyright (c) IronPython Team";

	public const int dllhandle = 0;

	public const int maxint = int.MaxValue;

	public const int maxsize = int.MaxValue;

	public const int maxunicode = 65535;

	public const string platform = "cli";

	public const string winver = "2.7";

	public static readonly string byteorder = (BitConverter.IsLittleEndian ? "little" : "big");

	public static BuiltinFunction displayhook = BuiltinFunction.MakeFunction("displayhook", ArrayUtils.ConvertAll(typeof(SysModule).GetMember("displayhookImpl"), (MemberInfo x) => (MethodBase)x), typeof(SysModule));

	public static readonly BuiltinFunction __displayhook__ = displayhook;

	public static readonly BuiltinFunction excepthook = BuiltinFunction.MakeFunction("excepthook", ArrayUtils.ConvertAll(typeof(SysModule).GetMember("excepthookImpl"), (MemberInfo x) => (MethodBase)x), typeof(SysModule));

	public static readonly BuiltinFunction __excepthook__ = excepthook;

	public static readonly string prefix = GetPrefix();

	public static PythonTuple subversion = PythonTuple.MakeTuple("IronPython", "", "");

	public static longinfo long_info = new longinfo(32, 4);

	public static floatinfo float_info = new floatinfo(double.MaxValue, 1024, 308, double.MinValue, -1021, -307, 15, 53, double.Epsilon, 2, 1);

	private static string GetPrefix()
	{
		try
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
		catch (SecurityException)
		{
			return string.Empty;
		}
		catch (ArgumentException)
		{
			return string.Empty;
		}
		catch (MethodAccessException)
		{
			return string.Empty;
		}
	}

	public static object callstats()
	{
		return null;
	}

	[Documentation("displayhook(object) -> None\r\n\r\nPrint an object to sys.stdout and also save it in __builtin__._")]
	[PythonHidden]
	public static void displayhookImpl(CodeContext context, object value)
	{
		if (value != null)
		{
			PythonOps.Print(context, PythonOps.Repr(context, value));
			PythonContext.GetContext(context).BuiltinModuleDict["_"] = value;
		}
	}

	[PythonHidden]
	[Documentation("excepthook(exctype, value, traceback) -> None\r\n\r\nHandle an exception by displaying it with a traceback on sys.stderr._")]
	public static void excepthookImpl(CodeContext context, object exctype, object value, object traceback)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonOps.PrintWithDest(context, context2.SystemStandardError, context2.FormatException(PythonExceptions.ToClr(value)));
	}

	public static int getcheckinterval()
	{
		throw PythonOps.NotImplementedError("IronPython does not support sys.getcheckinterval");
	}

	public static void setcheckinterval(int value)
	{
		throw PythonOps.NotImplementedError("IronPython does not support sys.setcheckinterval");
	}

	public static void getrefcount()
	{
		throw PythonOps.NotImplementedError("IronPython does not support sys.getrefcount");
	}

	[Python3Warning("'sys.exc_clear() not supported in 3.x; use except clauses'")]
	public static void exc_clear()
	{
		PythonOps.ClearCurrentException();
	}

	public static PythonTuple exc_info(CodeContext context)
	{
		return PythonOps.GetExceptionInfo(context);
	}

	public static void exit()
	{
		exit(null);
	}

	public static void exit(object code)
	{
		if (code == null)
		{
			throw new PythonExceptions._SystemExit().InitAndGetClrException();
		}
		if (code is PythonTuple pythonTuple && pythonTuple.__len__() == 1)
		{
			code = pythonTuple[0];
		}
		throw new PythonExceptions._SystemExit().InitAndGetClrException(code);
	}

	public static string getdefaultencoding(CodeContext context)
	{
		return PythonContext.GetContext(context).GetDefaultEncodingName();
	}

	public static object getfilesystemencoding()
	{
		return "mbcs";
	}

	[PythonHidden]
	public static TraceBackFrame _getframeImpl(CodeContext context)
	{
		return _getframeImpl(context, 0);
	}

	[PythonHidden]
	public static TraceBackFrame _getframeImpl(CodeContext context, int depth)
	{
		return _getframeImpl(context, depth, PythonOps.GetFunctionStack());
	}

	internal static TraceBackFrame _getframeImpl(CodeContext context, int depth, List<FunctionStack> stack)
	{
		if (depth < stack.Count)
		{
			TraceBackFrame traceBackFrame = null;
			int num = -1;
			for (int i = 0; i < stack.Count - depth; i++)
			{
				FunctionStack functionStack = stack[i];
				if (functionStack.Frame != null)
				{
					traceBackFrame = functionStack.Frame;
				}
				else
				{
					traceBackFrame = new TraceBackFrame(context, Builtin.globals(functionStack.Context), Builtin.locals(functionStack.Context), functionStack.Code, traceBackFrame);
					stack[i] = new FunctionStack(functionStack.Context, functionStack.Code, traceBackFrame);
				}
				num++;
			}
			return traceBackFrame;
		}
		throw PythonOps.ValueError("call stack is not deep enough");
	}

	public static int getsizeof(object o)
	{
		return ObjectOps.__sizeof__(o);
	}

	public static PythonTuple getwindowsversion()
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		return new windows_version(oSVersion.Version.Major, oSVersion.Version.Minor, oSVersion.Version.Build, (int)oSVersion.Platform, oSVersion.ServicePack);
	}

	public static void setdefaultencoding(CodeContext context, object name)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("name cannot be None");
		}
		if (!(name is string text))
		{
			throw PythonOps.TypeError("name must be a string");
		}
		PythonContext context2 = PythonContext.GetContext(context);
		if (!StringOps.TryGetEncoding(text, out var encoding))
		{
			throw PythonOps.LookupError("'{0}' does not match any available encodings", text);
		}
		context2.DefaultEncoding = encoding;
	}

	public static void settrace(CodeContext context, object o)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		context2.EnsureDebugContext();
		if (o == null)
		{
			context2.UnregisterTracebackHandler();
			PythonTracebackListener.SetTrace(null, null);
			return;
		}
		List<FunctionStack> functionStackNoCreate = PythonOps.GetFunctionStackNoCreate();
		if (functionStackNoCreate == null || !PythonTracebackListener.InTraceBack)
		{
			context2.PushTracebackHandler(new PythonTracebackListener(context.LanguageContext));
			context2.RegisterTracebackHandler();
			PythonTracebackListener.SetTrace(o, (TracebackDelegate)Converter.ConvertToDelegate(o, typeof(TracebackDelegate)));
		}
	}

	public static void call_tracing(CodeContext context, object func, PythonTuple args)
	{
		PythonContext languageContext = context.LanguageContext;
		languageContext.EnsureDebugContext();
		languageContext.UnregisterTracebackHandler();
		languageContext.PushTracebackHandler(new PythonTracebackListener(context.LanguageContext));
		languageContext.RegisterTracebackHandler();
		try
		{
			PythonCalls.Call(func, args.ToArray());
		}
		finally
		{
			languageContext.PopTracebackHandler();
			languageContext.UnregisterTracebackHandler();
		}
	}

	public static object gettrace(CodeContext context)
	{
		return PythonTracebackListener.GetTraceObject();
	}

	public static void setrecursionlimit(CodeContext context, int limit)
	{
		PythonContext.GetContext(context).RecursionLimit = limit;
	}

	public static int getrecursionlimit(CodeContext context)
	{
		return PythonContext.GetContext(context).RecursionLimit;
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		dict["stdin"] = dict["__stdin__"];
		dict["stdout"] = dict["__stdout__"];
		dict["stderr"] = dict["__stderr__"];
		dict["warnoptions"] = new List(0);
		PublishBuiltinModuleNames(context, dict);
		context.SetHostVariables(dict);
		dict["meta_path"] = new List(0);
		dict["path_hooks"] = new List(0);
		try
		{
			if (Importer.ImportModule(context.SharedClsContext, context.SharedClsContext.GlobalDict, "zipimport", bottom: false, -1) is PythonModule o)
			{
				object boundAttr = PythonOps.GetBoundAttr(context.SharedClsContext, o, "zipimporter");
				if (dict["path_hooks"] is List list && boundAttr != null)
				{
					list.Add(boundAttr);
				}
			}
		}
		catch
		{
		}
		dict["path_importer_cache"] = new PythonDictionary();
	}

	internal static void PublishBuiltinModuleNames(PythonContext context, PythonDictionary dict)
	{
		object[] array = new object[context.BuiltinModules.Keys.Count];
		int num = 0;
		foreach (string key in context.BuiltinModules.Keys)
		{
			array[num++] = key;
		}
		dict["builtin_module_names"] = PythonTuple.MakeTuple(array);
	}
}
