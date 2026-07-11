using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Exceptions;

public static class PythonExceptions
{
	[Serializable]
	[DynamicBaseType]
	[PythonType("BaseException")]
	public class BaseException : ICodeFormattable, IPythonObject, IDynamicMetaObjectProvider, IWeakReferenceable
	{
		private PythonType _type;

		private object _message = string.Empty;

		private PythonTuple _args;

		private PythonDictionary _dict;

		private Exception _clrException;

		private object[] _slots;

		public static string __doc__ = "Common base class for all non-exit exceptions.";

		public object message
		{
			[Python3Warning("BaseException.message has been deprecated as of Python 2.6")]
			get
			{
				return _message;
			}
			[Python3Warning("BaseException.message has been deprecated as of Python 2.6")]
			set
			{
				_message = value;
			}
		}

		public object args
		{
			get
			{
				return _args ?? PythonTuple.EMPTY;
			}
			set
			{
				_args = PythonTuple.Make(value);
			}
		}

		public object this[int index]
		{
			[Python3Warning("__getitem__ not supported for exception classes in 3.x; use args attribute")]
			get
			{
				return ((PythonTuple)args)[index];
			}
		}

		public PythonDictionary __dict__
		{
			get
			{
				EnsureDict();
				return _dict;
			}
			set
			{
				if (_dict == null)
				{
					throw PythonOps.TypeError("__dict__ must be a dictionary");
				}
				_dict = value;
			}
		}

		public Exception clsException
		{
			[PythonHidden]
			get
			{
				return GetClrException();
			}
			internal set
			{
				_clrException = value;
			}
		}

		PythonDictionary IPythonObject.Dict => _dict;

		PythonType IPythonObject.PythonType => _type;

		public BaseException(PythonType type)
		{
			ContractUtils.RequiresNotNull(type, "type");
			_type = type;
		}

		public static object __new__(PythonType cls, params object[] argsø)
		{
			if (cls.UnderlyingSystemType == typeof(BaseException))
			{
				return new BaseException(cls);
			}
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}

		public static object __new__(PythonType cls, [ParamDictionary] IDictionary<object, object> kwArgsø, params object[] argsø)
		{
			if (cls.UnderlyingSystemType == typeof(BaseException))
			{
				return new BaseException(cls);
			}
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}

		public virtual void __init__(params object[] argsø)
		{
			_args = PythonTuple.MakeTuple(argsø ?? new object[1]);
			if (_args.__len__() == 1)
			{
				_message = _args[0];
			}
		}

		public virtual object __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), args);
		}

		public virtual object __reduce_ex__(int protocol)
		{
			return __reduce__();
		}

		[Python3Warning("__getslice__ not supported for exception classes in 3.x; use args attribute")]
		public PythonTuple __getslice__(int start, int stop)
		{
			PythonTuple pythonTuple = (PythonTuple)args;
			Slice.FixSliceArguments(pythonTuple._data.Length, ref start, ref stop);
			return PythonTuple.MakeTuple(ArrayOps.GetSlice(pythonTuple._data, start, stop));
		}

		public void __setstate__(PythonDictionary state)
		{
			foreach (KeyValuePair<object, object> item in state)
			{
				__dict__[item.Key] = item.Value;
			}
		}

		public override string ToString()
		{
			if (_args == null)
			{
				return string.Empty;
			}
			if (_args.__len__() == 0)
			{
				return string.Empty;
			}
			if (_args.__len__() == 1)
			{
				if (_args[0] is string result)
				{
					return result;
				}
				if (_args[0] is Extensible<string> extensible)
				{
					return extensible.Value;
				}
				return PythonOps.ToString(_args[0]);
			}
			return _args.ToString();
		}

		public string __unicode__()
		{
			return ToString();
		}

		[SpecialName]
		public object GetBoundMember(string name)
		{
			if (_dict != null && _dict.TryGetValue(name, out var value))
			{
				return value;
			}
			return OperationFailed.Value;
		}

		[SpecialName]
		public void SetMemberAfter(string name, object value)
		{
			EnsureDict();
			_dict[name] = value;
		}

		[SpecialName]
		public bool DeleteCustomMember(string name)
		{
			if (name == "message")
			{
				_message = null;
				return true;
			}
			if (_dict == null)
			{
				return false;
			}
			return _dict.Remove(name);
		}

		private void EnsureDict()
		{
			if (_dict == null)
			{
				Interlocked.CompareExchange(ref _dict, PythonDictionary.MakeSymbolDictionary(), null);
			}
		}

		public virtual string __repr__(CodeContext context)
		{
			return _type.Name + ((ICodeFormattable)args).__repr__(context);
		}

		PythonDictionary IPythonObject.SetDict(PythonDictionary dict)
		{
			Interlocked.CompareExchange(ref _dict, dict, null);
			return _dict;
		}

		bool IPythonObject.ReplaceDict(PythonDictionary dict)
		{
			return Interlocked.CompareExchange(ref _dict, dict, null) == null;
		}

		void IPythonObject.SetPythonType(PythonType newType)
		{
			if (_type.IsSystemType || newType.IsSystemType)
			{
				throw PythonOps.TypeError("__class__ assignment can only be performed on user defined types");
			}
			_type = newType;
		}

		object[] IPythonObject.GetSlots()
		{
			return _slots;
		}

		object[] IPythonObject.GetSlotsCreate()
		{
			if (_slots == null)
			{
				Interlocked.CompareExchange(ref _slots, new object[1], null);
			}
			return _slots;
		}

		[PythonHidden]
		protected internal virtual void InitializeFromClr(Exception exception)
		{
			if (exception.Message != null)
			{
				__init__(exception.Message);
			}
			else
			{
				__init__();
			}
		}

		internal Exception GetClrException()
		{
			if (_clrException != null)
			{
				return _clrException;
			}
			string text = _message as string;
			if (string.IsNullOrEmpty(text))
			{
				text = _type.Name;
			}
			Exception ex = _type._makeException(text);
			ex.SetPythonException(this);
			Interlocked.CompareExchange(ref _clrException, ex, null);
			return _clrException;
		}

		internal Exception InitAndGetClrException(params object[] args)
		{
			__init__(args);
			return GetClrException();
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaUserObject(parameter, BindingRestrictions.Empty, null, this);
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return UserTypeOps.GetWeakRefHelper(this);
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			return UserTypeOps.SetWeakRefHelper(this, value);
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			UserTypeOps.SetFinalizerHelper(this, value);
		}
	}

	[Serializable]
	[PythonType("SyntaxError")]
	[PythonHidden]
	[DynamicBaseType]
	public class _SyntaxError : BaseException
	{
		private object _text;

		private object _print_file_and_line;

		private object _filename;

		private object _lineno;

		private object _offset;

		private object _msg;

		public object text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
			}
		}

		public object print_file_and_line
		{
			get
			{
				return _print_file_and_line;
			}
			set
			{
				_print_file_and_line = value;
			}
		}

		public object filename
		{
			get
			{
				return _filename;
			}
			set
			{
				_filename = value;
			}
		}

		public object lineno
		{
			get
			{
				return _lineno;
			}
			set
			{
				_lineno = value;
			}
		}

		public object offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		public object msg
		{
			get
			{
				return _msg;
			}
			set
			{
				_msg = value;
			}
		}

		public override string ToString()
		{
			PythonTuple pythonTuple = (PythonTuple)base.args;
			if (pythonTuple != null)
			{
				switch (pythonTuple.__len__())
				{
				case 0:
					return PythonOps.ToString(null);
				case 1:
					return PythonOps.ToString(pythonTuple[0]);
				case 2:
					if (pythonTuple[0] is string result)
					{
						return result;
					}
					break;
				}
				return PythonOps.ToString(pythonTuple);
			}
			return string.Empty;
		}

		public override void __init__(params object[] args)
		{
			base.__init__(args);
			if (args == null || args.Length == 0)
			{
				return;
			}
			msg = args[0];
			if (args.Length >= 2 && args[1] is PythonTuple pythonTuple)
			{
				if (pythonTuple.__len__() != 4)
				{
					throw PythonOps.IndexError("SyntaxError expected tuple with 4 arguments, got {0}", pythonTuple.__len__());
				}
				filename = pythonTuple[0];
				lineno = pythonTuple[1];
				offset = pythonTuple[2];
				text = pythonTuple[3];
			}
		}

		public _SyntaxError()
			: base(SyntaxError)
		{
		}

		public _SyntaxError(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}
	}

	[Serializable]
	[PythonHidden]
	[PythonType("EnvironmentError")]
	[DynamicBaseType]
	public class _EnvironmentError : BaseException
	{
		private const int EACCES = 13;

		private const int ENOENT = 2;

		private object _errno;

		private object _strerror;

		private object _filename;

		public object errno
		{
			get
			{
				return _errno;
			}
			set
			{
				_errno = value;
			}
		}

		public object strerror
		{
			get
			{
				return _strerror;
			}
			set
			{
				_strerror = value;
			}
		}

		public object filename
		{
			get
			{
				return _filename;
			}
			set
			{
				_filename = value;
			}
		}

		public override object __reduce__()
		{
			if (_filename != null)
			{
				return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), PythonTuple.MakeTuple(ArrayUtils.Append(((PythonTuple)base.args)._data, _filename)));
			}
			return base.__reduce__();
		}

		public override void __init__(params object[] args)
		{
			if (args != null)
			{
				switch (args.Length)
				{
				case 2:
					_errno = args[0];
					_strerror = args[1];
					break;
				case 3:
					_errno = args[0];
					_strerror = args[1];
					_filename = args[2];
					args = ArrayUtils.RemoveLast(args);
					break;
				}
			}
			base.__init__(args);
		}

		public override string ToString()
		{
			if (_errno != null && _strerror != null)
			{
				if (_filename != null)
				{
					return $"[Errno {_errno}] {_strerror}: {_filename}";
				}
				return $"[Errno {_errno}] {_strerror}";
			}
			if (base.args is PythonTuple && ((PythonTuple)base.args).Count > 0)
			{
				return PythonOps.ToString(((PythonTuple)base.args)[0]);
			}
			return string.Empty;
		}

		[PythonHidden]
		protected internal override void InitializeFromClr(Exception exception)
		{
			if (exception is FileNotFoundException || exception is DirectoryNotFoundException || exception is PathTooLongException || exception is DriveNotFoundException)
			{
				__init__(2, exception.Message);
				return;
			}
			if (exception is UnauthorizedAccessException)
			{
				__init__(13, exception.Message);
				return;
			}
			if (exception is IOException)
			{
				try
				{
					uint hRForException = (uint)GetHRForException(exception);
					if ((hRForException & 0xFFFF0000u) == 2147942400u)
					{
						__init__(hRForException & 0xFFFF, exception.Message);
						return;
					}
				}
				catch (MethodAccessException)
				{
				}
				catch (SecurityException)
				{
				}
			}
			InitAndGetClrException(exception);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int GetHRForException(Exception exception)
		{
			return Marshal.GetHRForException(exception);
		}

		public _EnvironmentError()
			: base(EnvironmentError)
		{
		}

		public _EnvironmentError(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}
	}

	[Serializable]
	[PythonHidden]
	[PythonType("UnicodeTranslateError")]
	[DynamicBaseType]
	public class _UnicodeTranslateError : BaseException
	{
		private object _start;

		private object _reason;

		private object _object;

		private object _end;

		private object _encoding;

		public object start
		{
			get
			{
				return _start;
			}
			set
			{
				_start = value;
			}
		}

		public object reason
		{
			get
			{
				return _reason;
			}
			set
			{
				_reason = value;
			}
		}

		public object @object
		{
			get
			{
				return _object;
			}
			set
			{
				_object = value;
			}
		}

		public object end
		{
			get
			{
				return _end;
			}
			set
			{
				_end = value;
			}
		}

		public object encoding
		{
			get
			{
				return _encoding;
			}
			set
			{
				_encoding = value;
			}
		}

		public override void __init__(params object[] args)
		{
			if (args.Length != 4)
			{
				throw PythonOps.TypeError("function takes exactly 4 arguments ({0} given)", args.Length);
			}
			if (args[0] is string || args[0] is Extensible<string>)
			{
				@object = args[0];
				start = args[1];
				end = args[2];
				if (args[3] is string || args[3] is Extensible<string>)
				{
					reason = args[3];
					base.__init__(args);
					return;
				}
				throw PythonOps.TypeError("argument 4 must be str, not {0}", DynamicHelpers.GetPythonType(args[3]).Name);
			}
			throw PythonOps.TypeError("argument 4 must be unicode, not {0}", DynamicHelpers.GetPythonType(args[0]).Name);
		}

		public _UnicodeTranslateError()
			: base(UnicodeTranslateError)
		{
		}

		public _UnicodeTranslateError(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}
	}

	[Serializable]
	[PythonType("WindowsError")]
	[PythonHidden]
	[DynamicBaseType]
	public class _WindowsError : _EnvironmentError
	{
		internal const int ERROR_FILE_NOT_FOUND = 2;

		internal const int ERROR_PATH_NOT_FOUND = 3;

		internal const int ERROR_TOO_MANY_OPEN_FILES = 4;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_INVALID_HANDLE = 6;

		internal const int ERROR_ARENA_TRASHED = 7;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_INVALID_BLOCK = 9;

		internal const int ERROR_BAD_ENVIRONMENT = 10;

		internal const int ERROR_BAD_FORMAT = 11;

		internal const int ERROR_INVALID_DRIVE = 15;

		internal const int ERROR_CURRENT_DIRECTORY = 16;

		internal const int ERROR_NOT_SAME_DEVICE = 17;

		internal const int ERROR_NO_MORE_FILES = 18;

		internal const int ERROR_WRITE_PROTECT = 19;

		internal const int ERROR_BAD_UNIT = 20;

		internal const int ERROR_NOT_READY = 21;

		internal const int ERROR_BAD_COMMAND = 22;

		internal const int ERROR_CRC = 23;

		internal const int ERROR_BAD_LENGTH = 24;

		internal const int ERROR_SEEK = 25;

		internal const int ERROR_NOT_DOS_DISK = 26;

		internal const int ERROR_SECTOR_NOT_FOUND = 27;

		internal const int ERROR_OUT_OF_PAPER = 28;

		internal const int ERROR_WRITE_FAULT = 29;

		internal const int ERROR_READ_FAULT = 30;

		internal const int ERROR_GEN_FAILURE = 31;

		internal const int ERROR_SHARING_VIOLATION = 32;

		internal const int ERROR_LOCK_VIOLATION = 33;

		internal const int ERROR_WRONG_DISK = 34;

		internal const int ERROR_SHARING_BUFFER_EXCEEDED = 36;

		internal const int ERROR_BAD_NETPATH = 53;

		internal const int ERROR_NETWORK_ACCESS_DENIED = 65;

		internal const int ERROR_BAD_NET_NAME = 67;

		internal const int ERROR_FILE_EXISTS = 80;

		internal const int ERROR_CANNOT_MAKE = 82;

		internal const int ERROR_FAIL_I24 = 83;

		internal const int ERROR_NO_PROC_SLOTS = 89;

		internal const int ERROR_DRIVE_LOCKED = 108;

		internal const int ERROR_BROKEN_PIPE = 109;

		internal const int ERROR_DISK_FULL = 112;

		internal const int ERROR_INVALID_TARGET_HANDLE = 114;

		internal const int ERROR_WAIT_NO_CHILDREN = 128;

		internal const int ERROR_CHILD_NOT_COMPLETE = 129;

		internal const int ERROR_DIRECT_ACCESS_HANDLE = 130;

		internal const int ERROR_SEEK_ON_DEVICE = 132;

		internal const int ERROR_DIR_NOT_EMPTY = 145;

		internal const int ERROR_NOT_LOCKED = 158;

		internal const int ERROR_BAD_PATHNAME = 161;

		internal const int ERROR_MAX_THRDS_REACHED = 164;

		internal const int ERROR_LOCK_FAILED = 167;

		internal const int ERROR_ALREADY_EXISTS = 183;

		internal const int ERROR_INVALID_STARTING_CODESEG = 188;

		internal const int ERROR_INVALID_STACKSEG = 189;

		internal const int ERROR_INVALID_MODULETYPE = 190;

		internal const int ERROR_INVALID_EXE_SIGNATURE = 191;

		internal const int ERROR_EXE_MARKED_INVALID = 192;

		internal const int ERROR_BAD_EXE_FORMAT = 193;

		internal const int ERROR_ITERATED_DATA_EXCEEDS_64k = 194;

		internal const int ERROR_INVALID_MINALLOCSIZE = 195;

		internal const int ERROR_DYNLINK_FROM_INVALID_RING = 196;

		internal const int ERROR_IOPL_NOT_ENABLED = 197;

		internal const int ERROR_INVALID_SEGDPL = 198;

		internal const int ERROR_AUTODATASEG_EXCEEDS_64k = 199;

		internal const int ERROR_RING2SEG_MUST_BE_MOVABLE = 200;

		internal const int ERROR_RELOC_CHAIN_XEEDS_SEGLIM = 201;

		internal const int ERROR_INFLOOP_IN_RELOC_CHAIN = 202;

		internal const int ERROR_FILENAME_EXCED_RANGE = 206;

		internal const int ERROR_NESTING_NOT_ALLOWED = 215;

		internal const int ERROR_NOT_ENOUGH_QUOTA = 1816;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_INVALID_NAME = 123;

		internal const int ERROR_FILE_INVALID = 1006;

		internal const int ERROR_MAPPED_ALIGNMENT = 1132;

		private object _winerror;

		public object winerror
		{
			get
			{
				return _winerror;
			}
			set
			{
				_winerror = value;
			}
		}

		public override void __init__(params object[] args)
		{
			if ((args.Length == 2 || args.Length == 3) && !(args[0] is int))
			{
				throw PythonOps.TypeError("an integer is required for the 1st argument of WindowsError");
			}
			base.__init__(args);
			if (args != null && (args.Length == 2 || args.Length == 3))
			{
				winerror = args[0];
			}
			if (base.errno is int num)
			{
				switch (num)
				{
				case 109:
					base.errno = 32;
					break;
				case 2:
				case 3:
				case 15:
				case 18:
				case 53:
				case 67:
				case 161:
				case 206:
					base.errno = 2;
					break;
				case 10:
					base.errno = 7;
					break;
				case 11:
				case 188:
				case 189:
				case 190:
				case 191:
				case 192:
				case 193:
				case 194:
				case 195:
				case 196:
				case 197:
				case 198:
				case 199:
				case 200:
				case 201:
				case 202:
					base.errno = 8;
					break;
				case 6:
				case 114:
				case 130:
					base.errno = 9;
					break;
				case 128:
				case 129:
					base.errno = 10;
					break;
				case 89:
				case 164:
				case 215:
					base.errno = 11;
					break;
				case 7:
				case 8:
				case 9:
				case 1816:
					base.errno = 12;
					break;
				case 5:
				case 16:
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
				case 31:
				case 32:
				case 33:
				case 34:
				case 36:
				case 65:
				case 82:
				case 83:
				case 108:
				case 132:
				case 158:
				case 167:
					base.errno = 13;
					break;
				case 80:
				case 183:
					base.errno = 17;
					break;
				case 17:
					base.errno = 18;
					break;
				case 145:
					base.errno = 41;
					break;
				case 4:
					base.errno = 24;
					break;
				case 112:
					base.errno = 28;
					break;
				default:
					base.errno = 22;
					break;
				}
			}
		}

		public _WindowsError()
			: base(WindowsError)
		{
		}

		public _WindowsError(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}
	}

	[Serializable]
	[PythonType("UnicodeDecodeError")]
	[DynamicBaseType]
	[PythonHidden]
	public class _UnicodeDecodeError : BaseException
	{
		private object _start;

		private object _reason;

		private object _object;

		private object _end;

		private object _encoding;

		public object start
		{
			get
			{
				return _start;
			}
			set
			{
				_start = value;
			}
		}

		public object reason
		{
			get
			{
				return _reason;
			}
			set
			{
				_reason = value;
			}
		}

		public object @object
		{
			get
			{
				return _object;
			}
			set
			{
				_object = value;
			}
		}

		public object end
		{
			get
			{
				return _end;
			}
			set
			{
				_end = value;
			}
		}

		public object encoding
		{
			get
			{
				return _encoding;
			}
			set
			{
				_encoding = value;
			}
		}

		[PythonHidden]
		protected internal override void InitializeFromClr(Exception exception)
		{
			if (exception is DecoderFallbackException ex)
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (ex.BytesUnknown != null)
				{
					for (int i = 0; i < ex.BytesUnknown.Length; i++)
					{
						stringBuilder.Append((char)ex.BytesUnknown[i]);
					}
				}
				__init__("unknown", stringBuilder.ToString(), ex.Index, ex.Index + 1, "");
			}
			else
			{
				base.InitializeFromClr(exception);
			}
		}

		public _UnicodeDecodeError()
			: base(UnicodeDecodeError)
		{
		}

		public _UnicodeDecodeError(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}

		public void __init__(object encoding, object @object, object start, object end, object reason)
		{
			_encoding = encoding;
			_object = @object;
			_start = start;
			_end = end;
			_reason = reason;
			base.args = PythonTuple.MakeTuple(encoding, @object, start, end, reason);
		}

		public override void __init__(params object[] args)
		{
			if (args == null || args.Length != 5)
			{
				throw PythonOps.TypeError("__init__ takes exactly 5 arguments ({0} given)", args.Length);
			}
			__init__(encoding, @object, start, end, reason);
		}
	}

	[Serializable]
	[DynamicBaseType]
	[PythonType("UnicodeEncodeError")]
	[PythonHidden]
	public class _UnicodeEncodeError : BaseException
	{
		private object _start;

		private object _reason;

		private object _object;

		private object _end;

		private object _encoding;

		public object start
		{
			get
			{
				return _start;
			}
			set
			{
				_start = value;
			}
		}

		public object reason
		{
			get
			{
				return _reason;
			}
			set
			{
				_reason = value;
			}
		}

		public object @object
		{
			get
			{
				return _object;
			}
			set
			{
				_object = value;
			}
		}

		public object end
		{
			get
			{
				return _end;
			}
			set
			{
				_end = value;
			}
		}

		public object encoding
		{
			get
			{
				return _encoding;
			}
			set
			{
				_encoding = value;
			}
		}

		[PythonHidden]
		protected internal override void InitializeFromClr(Exception exception)
		{
			if (exception is EncoderFallbackException ex)
			{
				__init__("unknown", new string(ex.CharUnknown, 1), ex.Index, ex.Index + 1, "");
			}
			else
			{
				base.InitializeFromClr(exception);
			}
		}

		public _UnicodeEncodeError()
			: base(UnicodeEncodeError)
		{
		}

		public _UnicodeEncodeError(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}

		public void __init__(object encoding, object @object, object start, object end, object reason)
		{
			_encoding = encoding;
			_object = @object;
			_start = start;
			_end = end;
			_reason = reason;
			base.args = PythonTuple.MakeTuple(encoding, @object, start, end, reason);
		}

		public override void __init__(params object[] args)
		{
			if (args == null || args.Length != 5)
			{
				throw PythonOps.TypeError("__init__ takes exactly 5 arguments ({0} given)", args.Length);
			}
			__init__(encoding, @object, start, end, reason);
		}
	}

	[Serializable]
	[PythonHidden]
	[PythonType("SystemExit")]
	[DynamicBaseType]
	public class _SystemExit : BaseException
	{
		private object _code;

		public object code
		{
			get
			{
				return _code;
			}
			set
			{
				_code = value;
			}
		}

		public override void __init__(params object[] args)
		{
			base.__init__(args);
			if (args != null && args.Length != 0)
			{
				code = base.message;
			}
		}

		public _SystemExit()
			: base(SystemExit)
		{
		}

		public _SystemExit(PythonType type)
			: base(type)
		{
		}

		public new static object __new__(PythonType cls, params object[] args)
		{
			return Activator.CreateInstance(cls.UnderlyingSystemType, cls);
		}
	}

	[Serializable]
	private class ExceptionDataWrapper : MarshalByRefObject
	{
		private readonly object _value;

		public object Value => _value;

		public ExceptionDataWrapper(object value)
		{
			_value = value;
		}
	}

	internal const string DefaultExceptionModule = "exceptions";

	public const string __doc__ = "Provides the most commonly used exceptions for Python programs";

	private static object _pythonExceptionKey = typeof(BaseException);

	private static object _pythonExceptionsLock = new object();

	private static PythonType GeneratorExitStorage;

	private static PythonType SystemExitStorage;

	private static PythonType KeyboardInterruptStorage;

	private static PythonType ExceptionStorage;

	private static PythonType StopIterationStorage;

	private static PythonType StandardErrorStorage;

	private static PythonType BufferErrorStorage;

	private static PythonType ArithmeticErrorStorage;

	private static PythonType FloatingPointErrorStorage;

	private static PythonType OverflowErrorStorage;

	private static PythonType ZeroDivisionErrorStorage;

	private static PythonType AssertionErrorStorage;

	private static PythonType AttributeErrorStorage;

	private static PythonType EnvironmentErrorStorage;

	private static PythonType IOErrorStorage;

	private static PythonType OSErrorStorage;

	private static PythonType WindowsErrorStorage;

	private static PythonType EOFErrorStorage;

	private static PythonType ImportErrorStorage;

	private static PythonType LookupErrorStorage;

	private static PythonType IndexErrorStorage;

	private static PythonType KeyErrorStorage;

	private static PythonType MemoryErrorStorage;

	private static PythonType NameErrorStorage;

	private static PythonType UnboundLocalErrorStorage;

	private static PythonType ReferenceErrorStorage;

	private static PythonType RuntimeErrorStorage;

	private static PythonType NotImplementedErrorStorage;

	private static PythonType SyntaxErrorStorage;

	private static PythonType IndentationErrorStorage;

	private static PythonType TabErrorStorage;

	private static PythonType SystemErrorStorage;

	private static PythonType TypeErrorStorage;

	private static PythonType ValueErrorStorage;

	private static PythonType UnicodeErrorStorage;

	private static PythonType UnicodeDecodeErrorStorage;

	private static PythonType UnicodeEncodeErrorStorage;

	private static PythonType UnicodeTranslateErrorStorage;

	private static PythonType WarningStorage;

	private static PythonType DeprecationWarningStorage;

	private static PythonType PendingDeprecationWarningStorage;

	private static PythonType RuntimeWarningStorage;

	private static PythonType SyntaxWarningStorage;

	private static PythonType UserWarningStorage;

	private static PythonType FutureWarningStorage;

	private static PythonType ImportWarningStorage;

	private static PythonType UnicodeWarningStorage;

	private static PythonType BytesWarningStorage;

	public static PythonType GeneratorExit
	{
		get
		{
			if (GeneratorExitStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					GeneratorExitStorage = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), "GeneratorExit", (string msg) => new GeneratorExitException(msg));
				}
			}
			return GeneratorExitStorage;
		}
	}

	public static PythonType SystemExit
	{
		get
		{
			if (SystemExitStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					SystemExitStorage = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), typeof(_SystemExit), (string msg) => new SystemExitException(msg));
				}
			}
			return SystemExitStorage;
		}
	}

	public static PythonType KeyboardInterrupt
	{
		get
		{
			if (KeyboardInterruptStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					KeyboardInterruptStorage = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), "KeyboardInterrupt", (string msg) => new KeyboardInterruptException(msg));
				}
			}
			return KeyboardInterruptStorage;
		}
	}

	public static PythonType Exception
	{
		get
		{
			if (ExceptionStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ExceptionStorage = CreateSubType(DynamicHelpers.GetPythonTypeFromType(typeof(BaseException)), "Exception", (string msg) => new PythonException(msg));
				}
			}
			return ExceptionStorage;
		}
	}

	public static PythonType StopIteration
	{
		get
		{
			if (StopIterationStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					StopIterationStorage = CreateSubType(Exception, "StopIteration", (string msg) => new StopIterationException(msg));
				}
			}
			return StopIterationStorage;
		}
	}

	public static PythonType StandardError
	{
		get
		{
			if (StandardErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					StandardErrorStorage = CreateSubType(Exception, "StandardError", (string msg) => new ApplicationException(msg));
				}
			}
			return StandardErrorStorage;
		}
	}

	public static PythonType BufferError
	{
		get
		{
			if (BufferErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					BufferErrorStorage = CreateSubType(StandardError, "BufferError", (string msg) => new BufferException(msg));
				}
			}
			return BufferErrorStorage;
		}
	}

	public static PythonType ArithmeticError
	{
		get
		{
			if (ArithmeticErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ArithmeticErrorStorage = CreateSubType(StandardError, "ArithmeticError", (string msg) => new ArithmeticException(msg));
				}
			}
			return ArithmeticErrorStorage;
		}
	}

	public static PythonType FloatingPointError
	{
		get
		{
			if (FloatingPointErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					FloatingPointErrorStorage = CreateSubType(ArithmeticError, "FloatingPointError", (string msg) => new FloatingPointException(msg));
				}
			}
			return FloatingPointErrorStorage;
		}
	}

	public static PythonType OverflowError
	{
		get
		{
			if (OverflowErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					OverflowErrorStorage = CreateSubType(ArithmeticError, "OverflowError", (string msg) => new OverflowException(msg));
				}
			}
			return OverflowErrorStorage;
		}
	}

	public static PythonType ZeroDivisionError
	{
		get
		{
			if (ZeroDivisionErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ZeroDivisionErrorStorage = CreateSubType(ArithmeticError, "ZeroDivisionError", (string msg) => new DivideByZeroException(msg));
				}
			}
			return ZeroDivisionErrorStorage;
		}
	}

	public static PythonType AssertionError
	{
		get
		{
			if (AssertionErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					AssertionErrorStorage = CreateSubType(StandardError, "AssertionError", (string msg) => new AssertionException(msg));
				}
			}
			return AssertionErrorStorage;
		}
	}

	public static PythonType AttributeError
	{
		get
		{
			if (AttributeErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					AttributeErrorStorage = CreateSubType(StandardError, "AttributeError", (string msg) => new AttributeErrorException(msg));
				}
			}
			return AttributeErrorStorage;
		}
	}

	public static PythonType EnvironmentError
	{
		get
		{
			if (EnvironmentErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					EnvironmentErrorStorage = CreateSubType(StandardError, typeof(_EnvironmentError), (string msg) => new ExternalException(msg));
				}
			}
			return EnvironmentErrorStorage;
		}
	}

	public static PythonType IOError
	{
		get
		{
			if (IOErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					IOErrorStorage = CreateSubType(EnvironmentError, "IOError", (string msg) => new IOException(msg));
				}
			}
			return IOErrorStorage;
		}
	}

	public static PythonType OSError
	{
		get
		{
			if (OSErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					OSErrorStorage = CreateSubType(EnvironmentError, "OSError", (string msg) => new OSException(msg));
				}
			}
			return OSErrorStorage;
		}
	}

	public static PythonType WindowsError
	{
		get
		{
			if (WindowsErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					WindowsErrorStorage = CreateSubType(OSError, typeof(_WindowsError), (string msg) => new Win32Exception(msg));
				}
			}
			return WindowsErrorStorage;
		}
	}

	public static PythonType EOFError
	{
		get
		{
			if (EOFErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					EOFErrorStorage = CreateSubType(StandardError, "EOFError", (string msg) => new EndOfStreamException(msg));
				}
			}
			return EOFErrorStorage;
		}
	}

	public static PythonType ImportError
	{
		get
		{
			if (ImportErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ImportErrorStorage = CreateSubType(StandardError, "ImportError", (string msg) => new ImportException(msg));
				}
			}
			return ImportErrorStorage;
		}
	}

	public static PythonType LookupError
	{
		get
		{
			if (LookupErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					LookupErrorStorage = CreateSubType(StandardError, "LookupError", (string msg) => new LookupException(msg));
				}
			}
			return LookupErrorStorage;
		}
	}

	public static PythonType IndexError
	{
		get
		{
			if (IndexErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					IndexErrorStorage = CreateSubType(LookupError, "IndexError", (string msg) => new IndexOutOfRangeException(msg));
				}
			}
			return IndexErrorStorage;
		}
	}

	public static PythonType KeyError
	{
		get
		{
			if (KeyErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					KeyErrorStorage = CreateSubType(LookupError, "KeyError", (string msg) => new KeyNotFoundException(msg));
				}
			}
			return KeyErrorStorage;
		}
	}

	public static PythonType MemoryError
	{
		get
		{
			if (MemoryErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					MemoryErrorStorage = CreateSubType(StandardError, "MemoryError", (string msg) => new OutOfMemoryException(msg));
				}
			}
			return MemoryErrorStorage;
		}
	}

	public static PythonType NameError
	{
		get
		{
			if (NameErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					NameErrorStorage = CreateSubType(StandardError, "NameError", (string msg) => new UnboundNameException(msg));
				}
			}
			return NameErrorStorage;
		}
	}

	public static PythonType UnboundLocalError
	{
		get
		{
			if (UnboundLocalErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UnboundLocalErrorStorage = CreateSubType(NameError, "UnboundLocalError", (string msg) => new UnboundLocalException(msg));
				}
			}
			return UnboundLocalErrorStorage;
		}
	}

	public static PythonType ReferenceError
	{
		get
		{
			if (ReferenceErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ReferenceErrorStorage = CreateSubType(StandardError, "ReferenceError", (string msg) => new ReferenceException(msg));
				}
			}
			return ReferenceErrorStorage;
		}
	}

	public static PythonType RuntimeError
	{
		get
		{
			if (RuntimeErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					RuntimeErrorStorage = CreateSubType(StandardError, "RuntimeError", (string msg) => new RuntimeException(msg));
				}
			}
			return RuntimeErrorStorage;
		}
	}

	public static PythonType NotImplementedError
	{
		get
		{
			if (NotImplementedErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					NotImplementedErrorStorage = CreateSubType(RuntimeError, "NotImplementedError", (string msg) => new NotImplementedException(msg));
				}
			}
			return NotImplementedErrorStorage;
		}
	}

	public static PythonType SyntaxError
	{
		get
		{
			if (SyntaxErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					SyntaxErrorStorage = CreateSubType(StandardError, typeof(_SyntaxError), (string msg) => new SyntaxErrorException(msg));
				}
			}
			return SyntaxErrorStorage;
		}
	}

	public static PythonType IndentationError
	{
		get
		{
			if (IndentationErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					IndentationErrorStorage = CreateSubType(SyntaxError, "IndentationError", (string msg) => new IndentationException(msg));
				}
			}
			return IndentationErrorStorage;
		}
	}

	public static PythonType TabError
	{
		get
		{
			if (TabErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					TabErrorStorage = CreateSubType(IndentationError, "TabError", (string msg) => new TabException(msg));
				}
			}
			return TabErrorStorage;
		}
	}

	public static PythonType SystemError
	{
		get
		{
			if (SystemErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					SystemErrorStorage = CreateSubType(StandardError, "SystemError", (string msg) => new SystemException(msg));
				}
			}
			return SystemErrorStorage;
		}
	}

	public static PythonType TypeError
	{
		get
		{
			if (TypeErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					TypeErrorStorage = CreateSubType(StandardError, "TypeError", (string msg) => new TypeErrorException(msg));
				}
			}
			return TypeErrorStorage;
		}
	}

	public static PythonType ValueError
	{
		get
		{
			if (ValueErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ValueErrorStorage = CreateSubType(StandardError, "ValueError", (string msg) => new ValueErrorException(msg));
				}
			}
			return ValueErrorStorage;
		}
	}

	public static PythonType UnicodeError
	{
		get
		{
			if (UnicodeErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UnicodeErrorStorage = CreateSubType(ValueError, "UnicodeError", (string msg) => new UnicodeException(msg));
				}
			}
			return UnicodeErrorStorage;
		}
	}

	public static PythonType UnicodeDecodeError
	{
		get
		{
			if (UnicodeDecodeErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UnicodeDecodeErrorStorage = CreateSubType(UnicodeError, typeof(_UnicodeDecodeError), (string msg) => new DecoderFallbackException(msg));
				}
			}
			return UnicodeDecodeErrorStorage;
		}
	}

	public static PythonType UnicodeEncodeError
	{
		get
		{
			if (UnicodeEncodeErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UnicodeEncodeErrorStorage = CreateSubType(UnicodeError, typeof(_UnicodeEncodeError), (string msg) => new EncoderFallbackException(msg));
				}
			}
			return UnicodeEncodeErrorStorage;
		}
	}

	public static PythonType UnicodeTranslateError
	{
		get
		{
			if (UnicodeTranslateErrorStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UnicodeTranslateErrorStorage = CreateSubType(UnicodeError, typeof(_UnicodeTranslateError), (string msg) => new UnicodeTranslateException(msg));
				}
			}
			return UnicodeTranslateErrorStorage;
		}
	}

	public static PythonType Warning
	{
		get
		{
			if (WarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					WarningStorage = CreateSubType(Exception, "Warning", (string msg) => new WarningException(msg));
				}
			}
			return WarningStorage;
		}
	}

	public static PythonType DeprecationWarning
	{
		get
		{
			if (DeprecationWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					DeprecationWarningStorage = CreateSubType(Warning, "DeprecationWarning", (string msg) => new DeprecationWarningException(msg));
				}
			}
			return DeprecationWarningStorage;
		}
	}

	public static PythonType PendingDeprecationWarning
	{
		get
		{
			if (PendingDeprecationWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					PendingDeprecationWarningStorage = CreateSubType(Warning, "PendingDeprecationWarning", (string msg) => new PendingDeprecationWarningException(msg));
				}
			}
			return PendingDeprecationWarningStorage;
		}
	}

	public static PythonType RuntimeWarning
	{
		get
		{
			if (RuntimeWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					RuntimeWarningStorage = CreateSubType(Warning, "RuntimeWarning", (string msg) => new RuntimeWarningException(msg));
				}
			}
			return RuntimeWarningStorage;
		}
	}

	public static PythonType SyntaxWarning
	{
		get
		{
			if (SyntaxWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					SyntaxWarningStorage = CreateSubType(Warning, "SyntaxWarning", (string msg) => new SyntaxWarningException(msg));
				}
			}
			return SyntaxWarningStorage;
		}
	}

	public static PythonType UserWarning
	{
		get
		{
			if (UserWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UserWarningStorage = CreateSubType(Warning, "UserWarning", (string msg) => new UserWarningException(msg));
				}
			}
			return UserWarningStorage;
		}
	}

	public static PythonType FutureWarning
	{
		get
		{
			if (FutureWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					FutureWarningStorage = CreateSubType(Warning, "FutureWarning", (string msg) => new FutureWarningException(msg));
				}
			}
			return FutureWarningStorage;
		}
	}

	public static PythonType ImportWarning
	{
		get
		{
			if (ImportWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					ImportWarningStorage = CreateSubType(Warning, "ImportWarning", (string msg) => new ImportWarningException(msg));
				}
			}
			return ImportWarningStorage;
		}
	}

	public static PythonType UnicodeWarning
	{
		get
		{
			if (UnicodeWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					UnicodeWarningStorage = CreateSubType(Warning, "UnicodeWarning", (string msg) => new UnicodeWarningException(msg));
				}
			}
			return UnicodeWarningStorage;
		}
	}

	public static PythonType BytesWarning
	{
		get
		{
			if (BytesWarningStorage == null)
			{
				lock (_pythonExceptionsLock)
				{
					BytesWarningStorage = CreateSubType(Warning, "BytesWarning", (string msg) => new BytesWarningException(msg));
				}
			}
			return BytesWarningStorage;
		}
	}

	internal static Exception CreateThrowable(PythonType type, params object[] args)
	{
		BaseException ex = CreatePythonThrowable(type, args);
		return ex.GetClrException();
	}

	internal static BaseException CreatePythonThrowable(PythonType type, params object[] args)
	{
		BaseException ex = ((!(type.UnderlyingSystemType == typeof(BaseException))) ? ((BaseException)Activator.CreateInstance(type.UnderlyingSystemType, type)) : new BaseException(type));
		ex.__init__(args);
		return ex;
	}

	internal static Exception CreateThrowableForRaise(CodeContext context, PythonType type, object value)
	{
		object obj = (PythonOps.IsInstance(value, type) ? value : ((value is PythonTuple) ? PythonOps.CallWithArgsTuple(type, ArrayUtils.EmptyObjects, value) : ((value == null) ? PythonCalls.Call(context, type) : PythonCalls.Call(context, type, value))));
		if (PythonOps.IsInstance(obj, type))
		{
			return ((BaseException)obj).GetClrException();
		}
		return new ObjectException(type, obj);
	}

	internal static Exception CreateThrowableForRaise(CodeContext context, OldClass type, object value)
	{
		object obj = (PythonOps.IsInstance(context, value, type) ? value : ((!(value is PythonTuple)) ? PythonCalls.Call(context, type, value) : PythonOps.CallWithArgsTuple(type, ArrayUtils.EmptyObjects, value)));
		return new OldInstanceException((OldInstance)obj);
	}

	internal static Exception ToClr(object pythonException)
	{
		if (pythonException is BaseException ex)
		{
			return ex.GetClrException();
		}
		Exception ex2 = ((!(pythonException is OldInstance instance)) ? new Exception(PythonOps.ToString(pythonException)) : new OldInstanceException(instance));
		ex2.SetPythonException(pythonException);
		return ex2;
	}

	internal static object ToPython(Exception clrException)
	{
		if (clrException is IPythonException ex)
		{
			return ex.ToPythonException();
		}
		object obj = clrException.GetPythonException();
		if (obj == null)
		{
			if (clrException is SyntaxErrorException e)
			{
				return SyntaxErrorToPython(e);
			}
			if (clrException is ThreadAbortException ex2)
			{
				if (ex2.ExceptionState is KeyboardInterruptException ex3)
				{
					ex2.Data[typeof(KeyboardInterruptException)] = ex3;
					return ToPython(ex3);
				}
				if (ex2.Data[typeof(KeyboardInterruptException)] is KeyboardInterruptException clrException2)
				{
					return ToPython(clrException2);
				}
			}
			if (obj == null)
			{
				obj = ToPythonNewStyle(clrException);
			}
			clrException.SetPythonException(obj);
		}
		return obj;
	}

	private static BaseException ToPythonNewStyle(Exception clrException)
	{
		BaseException ex;
		if (clrException is InvalidCastException || clrException is ArgumentNullException)
		{
			ex = new BaseException(TypeError);
		}
		else
		{
			if (clrException is Win32Exception)
			{
				Win32Exception ex2 = (Win32Exception)clrException;
				ex = new _WindowsError();
				if ((ex2.ErrorCode & 0x80070000u) == 2147942400u)
				{
					ex.__init__(ex2.ErrorCode & 0xFFFF, ex2.Message);
				}
				else
				{
					ex.__init__(ex2.ErrorCode, ex2.Message);
				}
				return ex;
			}
			ex = ToPythonHelper(clrException);
		}
		ex.InitializeFromClr(clrException);
		return ex;
	}

	private static void SetPythonException(this Exception e, object exception)
	{
		if (e is IPythonAwareException ex)
		{
			ex.PythonException = exception;
		}
		else
		{
			e.Data[_pythonExceptionKey] = new ExceptionDataWrapper(exception);
		}
		if (exception is BaseException ex2)
		{
			ex2.clsException = e;
		}
	}

	private static object GetPythonException(this Exception e)
	{
		if (e is IPythonAwareException ex)
		{
			return ex.PythonException;
		}
		if (e.Data.Contains(_pythonExceptionKey))
		{
			return ((ExceptionDataWrapper)e.Data[_pythonExceptionKey]).Value;
		}
		return null;
	}

	internal static List<DynamicStackFrame> GetFrameList(this Exception e)
	{
		if (e is IPythonAwareException ex)
		{
			return ex.Frames;
		}
		return e.Data[typeof(DynamicStackFrame)] as List<DynamicStackFrame>;
	}

	internal static void SetFrameList(this Exception e, List<DynamicStackFrame> frames)
	{
		if (e is IPythonAwareException ex)
		{
			ex.Frames = frames;
		}
		else
		{
			e.Data[typeof(DynamicStackFrame)] = frames;
		}
	}

	internal static void RemoveFrameList(this Exception e)
	{
		if (e is IPythonAwareException ex)
		{
			ex.Frames = null;
		}
		else
		{
			e.Data.Remove(typeof(DynamicStackFrame));
		}
	}

	internal static TraceBack GetTraceBack(this Exception e)
	{
		if (e is IPythonAwareException ex)
		{
			return ex.TraceBack;
		}
		return e.Data[typeof(TraceBack)] as TraceBack;
	}

	internal static void SetTraceBack(this Exception e, TraceBack traceback)
	{
		if (e is IPythonAwareException ex)
		{
			ex.TraceBack = traceback;
		}
		else
		{
			e.Data[typeof(TraceBack)] = traceback;
		}
	}

	internal static void RemoveTraceBack(this Exception e)
	{
		if (e is IPythonAwareException ex)
		{
			ex.TraceBack = null;
		}
		else
		{
			e.Data.Remove(typeof(TraceBack));
		}
	}

	private static BaseException SyntaxErrorToPython(SyntaxErrorException e)
	{
		_SyntaxError syntaxError = ((e.GetType() == typeof(IndentationException)) ? new _SyntaxError(IndentationError) : ((!(e.GetType() == typeof(TabException))) ? new _SyntaxError() : new _SyntaxError(TabError)));
		string sourceLine = PythonContext.GetSourceLine(e);
		string symbolDocumentName = e.GetSymbolDocumentName();
		object obj = ((e.Column == 0 || e.Data.Contains(PythonContext._syntaxErrorNoCaret)) ? null : ((object)e.Column));
		syntaxError.args = PythonTuple.MakeTuple(e.Message, PythonTuple.MakeTuple(symbolDocumentName, e.Line, obj, sourceLine));
		syntaxError.filename = symbolDocumentName;
		syntaxError.lineno = e.Line;
		syntaxError.offset = obj;
		syntaxError.text = sourceLine;
		syntaxError.msg = e.Message;
		e.SetPythonException(syntaxError);
		return syntaxError;
	}

	[PythonHidden]
	public static PythonType CreateSubType(PythonContext context, PythonType baseType, string name, string module, string documentation, Func<string, Exception> exceptionMaker)
	{
		PythonType pythonType = new PythonType(context, baseType, name, module, documentation, exceptionMaker);
		pythonType.SetCustomMember(context.SharedContext, "__weakref__", new PythonTypeWeakRefSlot(pythonType));
		pythonType.IsWeakReferencable = true;
		return pythonType;
	}

	[PythonHidden]
	public static PythonType CreateSubType(PythonContext context, PythonType baseType, Type underlyingType, string name, string module, string documentation, Func<string, Exception> exceptionMaker)
	{
		PythonType pythonType = new PythonType(context, new PythonType[1] { baseType }, underlyingType, name, module, documentation, exceptionMaker);
		pythonType.SetCustomMember(context.SharedContext, "__weakref__", new PythonTypeWeakRefSlot(pythonType));
		pythonType.IsWeakReferencable = true;
		return pythonType;
	}

	[PythonHidden]
	public static PythonType CreateSubType(PythonContext context, PythonType[] baseTypes, Type underlyingType, string name, string module, string documentation, Func<string, Exception> exceptionMaker)
	{
		PythonType pythonType = new PythonType(context, baseTypes, underlyingType, name, module, documentation, exceptionMaker);
		pythonType.SetCustomMember(context.SharedContext, "__weakref__", new PythonTypeWeakRefSlot(pythonType));
		pythonType.IsWeakReferencable = true;
		return pythonType;
	}

	private static PythonType CreateSubType(PythonType baseType, string name, Func<string, Exception> exceptionMaker)
	{
		return new PythonType(baseType, name, exceptionMaker);
	}

	private static PythonType CreateSubType(PythonType baseType, Type concreteType, Func<string, Exception> exceptionMaker)
	{
		PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(concreteType);
		pythonTypeFromType.ResolutionOrder = Mro.Calculate(pythonTypeFromType, new PythonType[1] { baseType });
		pythonTypeFromType.BaseTypes = new PythonType[1] { baseType };
		pythonTypeFromType.HasDictionary = true;
		pythonTypeFromType._makeException = exceptionMaker;
		return pythonTypeFromType;
	}

	internal static DynamicStackFrame[] GetDynamicStackFrames(Exception e)
	{
		List<DynamicStackFrame> frameList = e.GetFrameList();
		if (frameList == null)
		{
			return new DynamicStackFrame[0];
		}
		frameList = new List<DynamicStackFrame>(frameList);
		List<DynamicStackFrame> list = new List<DynamicStackFrame>();
		try
		{
			StackTrace stackTrace = new StackTrace(e);
			IList<StackTrace> list2 = ExceptionHelpers.GetExceptionStackTraces(e) ?? new List<StackTrace>();
			List<StackFrame> list3 = new List<StackFrame>();
			foreach (StackTrace item in list2)
			{
				list3.AddRange(item.GetFrames() ?? new StackFrame[0]);
			}
			list3.AddRange(stackTrace.GetFrames() ?? new StackFrame[0]);
			int num = 0;
			foreach (StackFrame item2 in InterpretedFrame.GroupStackFrames(list3))
			{
				MethodBase method = item2.GetMethod();
				for (int i = num; i < frameList.Count; i++)
				{
					MethodBase method2 = frameList[i].GetMethod();
					if (MethodsMatch(method, method2))
					{
						list.Add(frameList[i]);
						frameList.RemoveAt(i);
						num = i;
						break;
					}
				}
			}
		}
		catch (MemberAccessException)
		{
		}
		list.AddRange(frameList);
		return list.ToArray();
	}

	private static bool MethodsMatch(MethodBase method, MethodBase other)
	{
		if (method.Module == other.Module && method.DeclaringType == other.DeclaringType)
		{
			return method.Name == other.Name;
		}
		return false;
	}

	private static BaseException ToPythonHelper(Exception clrException)
	{
		if (clrException is BytesWarningException)
		{
			return new BaseException(BytesWarning);
		}
		if (clrException is DecoderFallbackException)
		{
			return new _UnicodeDecodeError();
		}
		if (clrException is DeprecationWarningException)
		{
			return new BaseException(DeprecationWarning);
		}
		if (clrException is DivideByZeroException)
		{
			return new BaseException(ZeroDivisionError);
		}
		if (clrException is EncoderFallbackException)
		{
			return new _UnicodeEncodeError();
		}
		if (clrException is EndOfStreamException)
		{
			return new BaseException(EOFError);
		}
		if (clrException is FutureWarningException)
		{
			return new BaseException(FutureWarning);
		}
		if (clrException is ImportWarningException)
		{
			return new BaseException(ImportWarning);
		}
		if (clrException is MissingMemberException)
		{
			return new BaseException(AttributeError);
		}
		if (clrException is OverflowException)
		{
			return new BaseException(OverflowError);
		}
		if (clrException is PendingDeprecationWarningException)
		{
			return new BaseException(PendingDeprecationWarning);
		}
		if (clrException is RuntimeWarningException)
		{
			return new BaseException(RuntimeWarning);
		}
		if (clrException is SyntaxWarningException)
		{
			return new BaseException(SyntaxWarning);
		}
		if (clrException is TabException)
		{
			return new _SyntaxError(TabError);
		}
		if (clrException is UnicodeWarningException)
		{
			return new BaseException(UnicodeWarning);
		}
		if (clrException is UserWarningException)
		{
			return new BaseException(UserWarning);
		}
		if (clrException is Win32Exception)
		{
			return new _WindowsError();
		}
		if (clrException is ArgumentException)
		{
			return new BaseException(ValueError);
		}
		if (clrException is ArithmeticException)
		{
			return new BaseException(ArithmeticError);
		}
		if (clrException is ExternalException)
		{
			return new _EnvironmentError();
		}
		if (clrException is IOException)
		{
			return new _EnvironmentError(IOError);
		}
		if (clrException is IndentationException)
		{
			return new _SyntaxError(IndentationError);
		}
		if (clrException is IndexOutOfRangeException)
		{
			return new BaseException(IndexError);
		}
		if (clrException is KeyNotFoundException)
		{
			return new BaseException(KeyError);
		}
		if (clrException is NotImplementedException)
		{
			return new BaseException(NotImplementedError);
		}
		if (clrException is OSException)
		{
			return new _EnvironmentError(OSError);
		}
		if (clrException is OutOfMemoryException)
		{
			return new BaseException(MemoryError);
		}
		if (clrException is UnboundLocalException)
		{
			return new BaseException(UnboundLocalError);
		}
		if (clrException is UnicodeTranslateException)
		{
			return new _UnicodeTranslateError();
		}
		if (clrException is WarningException)
		{
			return new BaseException(Warning);
		}
		if (clrException is ApplicationException)
		{
			return new BaseException(StandardError);
		}
		if (clrException is ArgumentTypeException)
		{
			return new BaseException(TypeError);
		}
		if (clrException is AssertionException)
		{
			return new BaseException(AssertionError);
		}
		if (clrException is BufferException)
		{
			return new BaseException(BufferError);
		}
		if (clrException is FloatingPointException)
		{
			return new BaseException(FloatingPointError);
		}
		if (clrException is GeneratorExitException)
		{
			return new BaseException(GeneratorExit);
		}
		if (clrException is ImportException)
		{
			return new BaseException(ImportError);
		}
		if (clrException is KeyboardInterruptException)
		{
			return new BaseException(KeyboardInterrupt);
		}
		if (clrException is LookupException)
		{
			return new BaseException(LookupError);
		}
		if (clrException is PythonException)
		{
			return new BaseException(Exception);
		}
		if (clrException is ReferenceException)
		{
			return new BaseException(ReferenceError);
		}
		if (clrException is RuntimeException)
		{
			return new BaseException(RuntimeError);
		}
		if (clrException is StopIterationException)
		{
			return new BaseException(StopIteration);
		}
		if (clrException is SyntaxErrorException)
		{
			return new _SyntaxError();
		}
		if (clrException is SystemException)
		{
			return new BaseException(SystemError);
		}
		if (clrException is SystemExitException)
		{
			return new _SystemExit();
		}
		if (clrException is UnboundNameException)
		{
			return new BaseException(NameError);
		}
		return new BaseException(Exception);
	}
}
