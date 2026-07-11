using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonNT
{
	[DontMapIEnumerableToIter]
	[PythonType]
	public class stat_result : IList, ICollection, IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable
	{
		public const int n_fields = 13;

		public const int n_sequence_fields = 10;

		public const int n_unnamed_fields = 3;

		private readonly object _mode;

		private readonly object _size;

		private readonly object _atime;

		private readonly object _mtime;

		private readonly object _ctime;

		private readonly object _st_atime;

		private readonly object _st_mtime;

		private readonly object _st_ctime;

		private readonly object _ino;

		private readonly object _dev;

		private readonly object _nlink;

		private readonly object _uid;

		private readonly object _gid;

		public object st_atime => _st_atime;

		public object st_ctime => _st_ctime;

		public object st_mtime => _st_mtime;

		public object st_dev => TryShrinkToInt(_dev);

		public object st_gid => _gid;

		public object st_ino => _ino;

		public object st_mode => TryShrinkToInt(_mode);

		public object st_nlink => TryShrinkToInt(_nlink);

		public object st_size => _size;

		public object st_uid => _uid;

		public object this[int index] => MakeTuple()[index];

		public object this[Slice slice] => MakeTuple()[slice];

		object IList<object>.this[int index]
		{
			get
			{
				return MakeTuple()[index];
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		int ICollection<object>.Count => __len__();

		bool ICollection<object>.IsReadOnly => true;

		bool IList.IsFixedSize => true;

		bool IList.IsReadOnly => true;

		object IList.this[int index]
		{
			get
			{
				return MakeTuple()[index];
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		int ICollection.Count => __len__();

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		internal stat_result(int mode)
			: this(mode, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero)
		{
			_mode = mode;
		}

		internal stat_result(int mode, BigInteger size, BigInteger st_atime, BigInteger st_mtime, BigInteger st_ctime)
		{
			_mode = mode;
			_size = size;
			_st_atime = (_atime = TryShrinkToInt(st_atime));
			_st_mtime = (_mtime = TryShrinkToInt(st_mtime));
			_st_ctime = (_ctime = TryShrinkToInt(st_ctime));
			_ino = (_dev = (_nlink = (_uid = (_gid = ScriptingRuntimeHelpers.Int32ToObject(0)))));
		}

		public stat_result(CodeContext context, IList statResult, [DefaultParameterValue(null)] PythonDictionary dict)
		{
			if (statResult.Count < 10)
			{
				throw PythonOps.TypeError("stat_result() takes an at least 10-sequence ({0}-sequence given)", statResult.Count);
			}
			_mode = statResult[0];
			_ino = statResult[1];
			_dev = statResult[2];
			_nlink = statResult[3];
			_uid = statResult[4];
			_gid = statResult[5];
			_size = statResult[6];
			_atime = statResult[7];
			_mtime = statResult[8];
			_ctime = statResult[9];
			object dictTime;
			if (statResult.Count >= 11)
			{
				_st_atime = TryShrinkToInt(statResult[10]);
			}
			else if (TryGetDictValue(dict, "st_atime", out dictTime))
			{
				_st_atime = dictTime;
			}
			else
			{
				_st_atime = TryShrinkToInt(_atime);
			}
			if (statResult.Count >= 12)
			{
				_st_mtime = TryShrinkToInt(statResult[11]);
			}
			else if (TryGetDictValue(dict, "st_mtime", out dictTime))
			{
				_st_mtime = dictTime;
			}
			else
			{
				_st_mtime = TryShrinkToInt(_mtime);
			}
			if (statResult.Count >= 13)
			{
				_st_ctime = TryShrinkToInt(statResult[12]);
			}
			else if (TryGetDictValue(dict, "st_ctime", out dictTime))
			{
				_st_ctime = dictTime;
			}
			else
			{
				_st_ctime = TryShrinkToInt(_ctime);
			}
		}

		private static bool TryGetDictValue(PythonDictionary dict, string name, out object dictTime)
		{
			if (dict != null && dict.TryGetValue(name, out dictTime))
			{
				dictTime = TryShrinkToInt(dictTime);
				return true;
			}
			dictTime = null;
			return false;
		}

		private static object TryShrinkToInt(object value)
		{
			if (!(value is BigInteger))
			{
				return value;
			}
			return BigIntegerOps.__int__((BigInteger)value);
		}

		public static PythonTuple operator +(stat_result stat, object tuple)
		{
			if (!(tuple is PythonTuple pythonTuple))
			{
				throw PythonOps.TypeError("can only concatenate tuple (not \"{0}\") to tuple", PythonTypeOps.GetName(tuple));
			}
			return stat.MakeTuple() + pythonTuple;
		}

		public static bool operator >(stat_result stat, [NotNull] stat_result o)
		{
			return stat.MakeTuple() > PythonTuple.Make(o);
		}

		public static bool operator <(stat_result stat, [NotNull] stat_result o)
		{
			return stat.MakeTuple() < PythonTuple.Make(o);
		}

		public static bool operator >=(stat_result stat, [NotNull] stat_result o)
		{
			return stat.MakeTuple() >= PythonTuple.Make(o);
		}

		public static bool operator <=(stat_result stat, [NotNull] stat_result o)
		{
			return stat.MakeTuple() <= PythonTuple.Make(o);
		}

		public static bool operator >(stat_result stat, object o)
		{
			return true;
		}

		public static bool operator <(stat_result stat, object o)
		{
			return false;
		}

		public static bool operator >=(stat_result stat, object o)
		{
			return true;
		}

		public static bool operator <=(stat_result stat, object o)
		{
			return false;
		}

		public static PythonTuple operator *(stat_result stat, int size)
		{
			return stat.MakeTuple() * size;
		}

		public static PythonTuple operator *(int size, stat_result stat)
		{
			return stat.MakeTuple() * size;
		}

		public override string ToString()
		{
			return MakeTuple().ToString();
		}

		public string __repr__()
		{
			return ToString();
		}

		public PythonTuple __reduce__()
		{
			PythonDictionary pythonDictionary = new PythonDictionary(3);
			pythonDictionary["st_atime"] = st_atime;
			pythonDictionary["st_ctime"] = st_ctime;
			pythonDictionary["st_mtime"] = st_mtime;
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(typeof(stat_result)), PythonTuple.MakeTuple(MakeTuple(), pythonDictionary));
		}

		public object __getslice__(int start, int stop)
		{
			return MakeTuple().__getslice__(start, stop);
		}

		public int __len__()
		{
			return MakeTuple().__len__();
		}

		public bool __contains__(object item)
		{
			return ((ICollection<object>)MakeTuple()).Contains(item);
		}

		private PythonTuple MakeTuple()
		{
			return PythonTuple.MakeTuple(st_mode, st_ino, st_dev, st_nlink, st_uid, st_gid, st_size, _atime, _mtime, _ctime);
		}

		public override bool Equals(object obj)
		{
			if (obj is stat_result)
			{
				return MakeTuple().Equals(((stat_result)obj).MakeTuple());
			}
			return MakeTuple().Equals(obj);
		}

		public override int GetHashCode()
		{
			return MakeTuple().GetHashCode();
		}

		int IList<object>.IndexOf(object item)
		{
			return MakeTuple().IndexOf(item);
		}

		void IList<object>.Insert(int index, object item)
		{
			throw new InvalidOperationException();
		}

		void IList<object>.RemoveAt(int index)
		{
			throw new InvalidOperationException();
		}

		void ICollection<object>.Add(object item)
		{
			throw new InvalidOperationException();
		}

		void ICollection<object>.Clear()
		{
			throw new InvalidOperationException();
		}

		bool ICollection<object>.Contains(object item)
		{
			return __contains__(item);
		}

		void ICollection<object>.CopyTo(object[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<object>.Remove(object item)
		{
			throw new InvalidOperationException();
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			foreach (object item in MakeTuple())
			{
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (object item in MakeTuple())
			{
				yield return item;
			}
		}

		int IList.Add(object value)
		{
			throw new InvalidOperationException();
		}

		void IList.Clear()
		{
			throw new InvalidOperationException();
		}

		bool IList.Contains(object value)
		{
			return __contains__(value);
		}

		int IList.IndexOf(object value)
		{
			return MakeTuple().IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			throw new InvalidOperationException();
		}

		void IList.Remove(object value)
		{
			throw new InvalidOperationException();
		}

		void IList.RemoveAt(int index)
		{
			throw new InvalidOperationException();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}
	}

	[PythonType]
	private class POpenFile : PythonFile
	{
		private Process _process;

		internal POpenFile(CodeContext context, string command, Process process, Stream stream, string mode)
			: base(PythonContext.GetContext(context))
		{
			__init__(stream, PythonContext.GetContext(context).DefaultEncoding, command, mode);
			_process = process;
		}

		public override object close()
		{
			base.close();
			if (_process.HasExited && _process.ExitCode != 0)
			{
				return _process.ExitCode;
			}
			return null;
		}
	}

	public const string __doc__ = "Provides low-level operationg system access for files, the environment, etc...";

	private const int DefaultBufferSize = 4096;

	public const int O_APPEND = 8;

	public const int O_CREAT = 256;

	public const int O_TRUNC = 512;

	public const int O_EXCL = 1024;

	public const int O_NOINHERIT = 128;

	public const int O_RANDOM = 16;

	public const int O_SEQUENTIAL = 32;

	public const int O_SHORT_LIVED = 4096;

	public const int O_TEMPORARY = 64;

	public const int O_WRONLY = 1;

	public const int O_RDONLY = 0;

	public const int O_RDWR = 2;

	public const int O_BINARY = 32768;

	public const int O_TEXT = 16384;

	public const int P_WAIT = 0;

	public const int P_NOWAIT = 1;

	public const int P_NOWAITO = 3;

	public const int P_OVERLAY = 2;

	public const int P_DETACH = 4;

	public const int TMP_MAX = 32767;

	private const int S_IWRITE = 146;

	private const int S_IREAD = 292;

	private const int S_IEXEC = 73;

	public const int F_OK = 0;

	public const int X_OK = 1;

	public const int W_OK = 2;

	public const int R_OK = 4;

	private static Dictionary<int, Process> _processToIdMapping = new Dictionary<int, Process>();

	private static List<int> _freeProcessIds = new List<int>();

	private static int _processCount;

	public static readonly object environ = new PythonDictionary(new EnvironmentDictionaryStorage());

	public static readonly PythonType error = Builtin.OSError;

	private static readonly object _umaskKey = new object();

	private static PythonType WindowsError => PythonExceptions.WindowsError;

	public static void abort()
	{
		Environment.FailFast("IronPython os.abort");
	}

	public static bool access(CodeContext context, string path, int mode)
	{
		if (path == null)
		{
			throw PythonOps.TypeError("expected string, got None");
		}
		if (mode == 0)
		{
			if (!context.LanguageContext.DomainManager.Platform.FileExists(path))
			{
				return context.LanguageContext.DomainManager.Platform.DirectoryExists(path);
			}
			return true;
		}
		FileAttributes attributes = File.GetAttributes(path);
		if ((attributes & FileAttributes.Directory) != 0)
		{
			return true;
		}
		if ((attributes & FileAttributes.ReadOnly) != 0 && (mode & 2) != 0)
		{
			return false;
		}
		return true;
	}

	public static void chdir([NotNull] string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw PythonExceptions.CreateThrowable(WindowsError, 123, "Path cannot be an empty string");
		}
		try
		{
			Directory.SetCurrentDirectory(path);
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static void chmod(string path, int mode)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if ((mode & 0x92) != 0)
			{
				fileInfo.Attributes &= ~FileAttributes.ReadOnly;
			}
			else
			{
				fileInfo.Attributes |= FileAttributes.ReadOnly;
			}
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static void close(CodeContext context, int fd)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (context2.FileManager.TryGetFileFromId(context2, fd, out var pf))
		{
			pf.close();
			return;
		}
		if (!(context2.FileManager.GetObjectFromId(fd) is Stream stream))
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, 9, "Bad file descriptor");
		}
		stream.Close();
	}

	public static void _exit(CodeContext context, int code)
	{
		PythonContext.GetContext(context).DomainManager.Platform.TerminateScriptExecution(code);
	}

	public static object fdopen(CodeContext context, int fd)
	{
		return fdopen(context, fd, "r");
	}

	public static object fdopen(CodeContext context, int fd, string mode)
	{
		return fdopen(context, fd, mode, 0);
	}

	public static object fdopen(CodeContext context, int fd, string mode, int bufsize)
	{
		PythonFile.ValidateMode(mode);
		PythonContext context2 = PythonContext.GetContext(context);
		if (context2.FileManager.TryGetFileFromId(context2, fd, out var pf))
		{
			return pf;
		}
		if (!(context2.FileManager.GetObjectFromId(fd) is Stream stream))
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, 9, "Bad file descriptor");
		}
		return PythonFile.Create(context, stream, stream.ToString(), mode);
	}

	[LightThrowing]
	public static object fstat(CodeContext context, int fd)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonFile fileFromId = context2.FileManager.GetFileFromId(context2, fd);
		if (fileFromId.IsConsole)
		{
			return new stat_result(8192);
		}
		return lstat(fileFromId.name);
	}

	public static string getcwd(CodeContext context)
	{
		return context.LanguageContext.DomainManager.Platform.CurrentDirectory;
	}

	public static string getcwdu(CodeContext context)
	{
		return context.LanguageContext.DomainManager.Platform.CurrentDirectory;
	}

	public static string _getfullpathname(CodeContext context, [NotNull] string dir)
	{
		PlatformAdaptationLayer platform = context.LanguageContext.DomainManager.Platform;
		try
		{
			return platform.GetFullPath(dir);
		}
		catch (ArgumentException)
		{
			string text = dir;
			if (IsWindows())
			{
				if (text.Length >= 2 && text[1] == ':' && (text[0] < 'a' || text[0] > 'z') && (text[0] < 'A' || text[0] > 'Z'))
				{
					if (text.Length == 2)
					{
						return text + Path.DirectorySeparatorChar;
					}
					if (text[2] == Path.DirectorySeparatorChar)
					{
						return text;
					}
					return text.Substring(0, 2) + Path.DirectorySeparatorChar + text.Substring(2);
				}
				if (text.Length > 2 && text.IndexOf(':', 2) != -1)
				{
					text = text.Substring(0, 2) + text.Substring(2).Replace(':', '\uffff');
				}
				if (text.Length > 0 && text[0] == ':')
				{
					text = '\uffff' + text.Substring(1);
				}
			}
			char[] invalidPathChars = Path.GetInvalidPathChars();
			foreach (char oldChar in invalidPathChars)
			{
				text = text.Replace(oldChar, '\uffff');
			}
			string text2 = platform.GetFullPath(text);
			int num = dir.Length;
			for (int num2 = text2.Length - 1; num2 >= 0; num2--)
			{
				if (text2[num2] == '\uffff')
				{
					for (num--; num >= 0; num--)
					{
						if (text[num] == '\uffff')
						{
							text2 = text2.Substring(0, num2) + dir[num] + text2.Substring(num2 + 1);
							break;
						}
					}
				}
			}
			return text2;
		}
	}

	private static bool IsWindows()
	{
		if (Environment.OSVersion.Platform != PlatformID.Win32NT && Environment.OSVersion.Platform != PlatformID.Win32S)
		{
			return Environment.OSVersion.Platform == PlatformID.Win32Windows;
		}
		return true;
	}

	public static int getpid()
	{
		return Process.GetCurrentProcess().Id;
	}

	public static List listdir(CodeContext context, [NotNull] string path)
	{
		if (path == string.Empty)
		{
			path = ".";
		}
		List list = PythonOps.MakeList();
		try
		{
			addBase(context.LanguageContext.DomainManager.Platform.GetFileSystemEntries(path, "*"), list);
			return list;
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static void lseek(CodeContext context, int filedes, long offset, int whence)
	{
		PythonFile fileFromId = context.LanguageContext.FileManager.GetFileFromId(context.LanguageContext, filedes);
		fileFromId.seek(offset, whence);
	}

	[LightThrowing]
	public static object lstat(string path)
	{
		return stat(path);
	}

	public static void mkdir(string path)
	{
		if (Directory.Exists(path))
		{
			throw DirectoryExists();
		}
		try
		{
			Directory.CreateDirectory(path);
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static void mkdir(string path, int mode)
	{
		if (Directory.Exists(path))
		{
			throw DirectoryExists();
		}
		try
		{
			Directory.CreateDirectory(path);
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static object open(CodeContext context, string filename, int flag)
	{
		return open(context, filename, flag, 777);
	}

	public static object open(CodeContext context, string filename, int flag, int mode)
	{
		try
		{
			FileMode fileMode = FileModeFromFlags(flag);
			FileAccess fileAccess = FileAccessFromFlags(flag);
			FileOptions options = FileOptionsFromFlags(flag);
			FileStream fileStream;
			if (fileAccess != FileAccess.Read || (fileMode != FileMode.CreateNew && fileMode != FileMode.Create && fileMode != FileMode.Append))
			{
				fileStream = ((fileAccess != FileAccess.ReadWrite || fileMode != FileMode.Append) ? new FileStream(filename, fileMode, fileAccess, FileShare.ReadWrite, 4096, options) : new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, options));
			}
			else
			{
				fileStream = new FileStream(filename, fileMode, FileAccess.Write, FileShare.None);
				fileStream.Close();
				fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, options);
			}
			string text = ((fileStream.CanRead && fileStream.CanWrite) ? "w+" : ((!fileStream.CanWrite) ? "r" : "w"));
			if ((flag & 0x8000) != 0)
			{
				text += "b";
			}
			return PythonContext.GetContext(context).FileManager.AddToStrongMapping(PythonFile.Create(context, fileStream, filename, text));
		}
		catch (Exception e)
		{
			throw ToPythonException(e, filename);
		}
	}

	private static FileOptions FileOptionsFromFlags(int flag)
	{
		FileOptions fileOptions = FileOptions.None;
		if ((flag & 0x40) != 0)
		{
			fileOptions |= FileOptions.DeleteOnClose;
		}
		if ((flag & 0x10) != 0)
		{
			fileOptions |= FileOptions.RandomAccess;
		}
		if ((flag & 0x20) != 0)
		{
			fileOptions |= FileOptions.SequentialScan;
		}
		return fileOptions;
	}

	public static PythonTuple pipe(CodeContext context)
	{
		PythonSubprocess.SECURITY_ATTRIBUTES lpPipeAttributes = default(PythonSubprocess.SECURITY_ATTRIBUTES);
		lpPipeAttributes.nLength = Marshal.SizeOf((object)lpPipeAttributes);
		PythonSubprocess.CreatePipePI(out var hReadPipe, out var hWritePipe, ref lpPipeAttributes, 0u);
		return PythonTuple.MakeTuple(PythonMsvcrt.open_osfhandle(context, new BigInteger(hReadPipe.ToInt64()), 0), PythonMsvcrt.open_osfhandle(context, new BigInteger(hWritePipe.ToInt64()), 0));
	}

	public static PythonFile popen(CodeContext context, string command)
	{
		return popen(context, command, "r");
	}

	public static PythonFile popen(CodeContext context, string command, string mode)
	{
		return popen(context, command, mode, 4096);
	}

	public static PythonFile popen(CodeContext context, string command, string mode, int bufsize)
	{
		if (string.IsNullOrEmpty(mode))
		{
			mode = "r";
		}
		ProcessStartInfo processInfo = GetProcessInfo(command);
		processInfo.CreateNoWindow = true;
		try
		{
			switch (mode)
			{
			case "r":
			{
				processInfo.RedirectStandardOutput = true;
				Process process = Process.Start(processInfo);
				return new POpenFile(context, command, process, process.StandardOutput.BaseStream, "r");
			}
			case "w":
			{
				processInfo.RedirectStandardInput = true;
				Process process = Process.Start(processInfo);
				return new POpenFile(context, command, process, process.StandardInput.BaseStream, "w");
			}
			default:
				throw PythonOps.ValueError("expected 'r' or 'w' for mode, got {0}", mode);
			}
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static PythonTuple popen2(CodeContext context, string command)
	{
		return popen2(context, command, "t");
	}

	public static PythonTuple popen2(CodeContext context, string command, string mode)
	{
		return popen2(context, command, "t", 4096);
	}

	public static PythonTuple popen2(CodeContext context, string command, string mode, int bufsize)
	{
		if (string.IsNullOrEmpty(mode))
		{
			mode = "t";
		}
		if (mode != "t" && mode != "b")
		{
			throw PythonOps.ValueError("mode must be 't' or 'b' (default is t)");
		}
		if (mode == "t")
		{
			mode = string.Empty;
		}
		try
		{
			ProcessStartInfo processInfo = GetProcessInfo(command);
			processInfo.RedirectStandardInput = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.CreateNoWindow = true;
			Process process = Process.Start(processInfo);
			return PythonTuple.MakeTuple(new POpenFile(context, command, process, process.StandardInput.BaseStream, "w" + mode), new POpenFile(context, command, process, process.StandardOutput.BaseStream, "r" + mode));
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static PythonTuple popen3(CodeContext context, string command)
	{
		return popen3(context, command, "t");
	}

	public static PythonTuple popen3(CodeContext context, string command, string mode)
	{
		return popen3(context, command, "t", 4096);
	}

	public static PythonTuple popen3(CodeContext context, string command, string mode, int bufsize)
	{
		if (string.IsNullOrEmpty(mode))
		{
			mode = "t";
		}
		if (mode != "t" && mode != "b")
		{
			throw PythonOps.ValueError("mode must be 't' or 'b' (default is t)");
		}
		if (mode == "t")
		{
			mode = string.Empty;
		}
		try
		{
			ProcessStartInfo processInfo = GetProcessInfo(command);
			processInfo.RedirectStandardInput = true;
			processInfo.RedirectStandardOutput = true;
			processInfo.RedirectStandardError = true;
			processInfo.CreateNoWindow = true;
			Process process = Process.Start(processInfo);
			return PythonTuple.MakeTuple(new POpenFile(context, command, process, process.StandardInput.BaseStream, "w" + mode), new POpenFile(context, command, process, process.StandardOutput.BaseStream, "r" + mode), new POpenFile(context, command, process, process.StandardError.BaseStream, "r+" + mode));
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static void putenv(string varname, string value)
	{
		try
		{
			Environment.SetEnvironmentVariable(varname, value);
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static string read(CodeContext context, int fd, int buffersize)
	{
		if (buffersize < 0)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, 22, "Invalid argument");
		}
		try
		{
			PythonContext context2 = PythonContext.GetContext(context);
			PythonFile fileFromId = context2.FileManager.GetFileFromId(context2, fd);
			return fileFromId.read();
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static void rename(string src, string dst)
	{
		try
		{
			Directory.Move(src, dst);
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static void rmdir(string path)
	{
		try
		{
			Directory.Delete(path);
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static object spawnl(CodeContext context, int mode, string path, params object[] args)
	{
		return SpawnProcessImpl(context, MakeProcess(), mode, path, args);
	}

	public static object spawnle(CodeContext context, int mode, string path, params object[] args)
	{
		if (args.Length < 1)
		{
			throw PythonOps.TypeError("spawnle() takes at least three arguments ({0} given)", 2 + args.Length);
		}
		object newEnvironment = args[args.Length - 1];
		object[] args2 = ArrayUtils.RemoveFirst(args);
		Process process = MakeProcess();
		SetEnvironment(process.StartInfo.EnvironmentVariables, newEnvironment);
		return SpawnProcessImpl(context, process, mode, path, args2);
	}

	public static object spawnv(CodeContext context, int mode, string path, object args)
	{
		return SpawnProcessImpl(context, MakeProcess(), mode, path, args);
	}

	public static object spawnve(CodeContext context, int mode, string path, object args, object env)
	{
		Process process = MakeProcess();
		SetEnvironment(process.StartInfo.EnvironmentVariables, env);
		return SpawnProcessImpl(context, process, mode, path, args);
	}

	private static Process MakeProcess()
	{
		try
		{
			return new Process();
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	private static object SpawnProcessImpl(CodeContext context, Process process, int mode, string path, object args)
	{
		try
		{
			process.StartInfo.Arguments = ArgumentsToString(context, args);
			process.StartInfo.FileName = path;
			process.StartInfo.UseShellExecute = false;
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
		if (!process.Start())
		{
			throw PythonOps.OSError("Cannot start process: {0}", path);
		}
		if (mode == 0)
		{
			process.WaitForExit();
			int exitCode = process.ExitCode;
			process.Close();
			return exitCode;
		}
		lock (_processToIdMapping)
		{
			int num;
			if (_freeProcessIds.Count > 0)
			{
				num = _freeProcessIds[_freeProcessIds.Count - 1];
				_freeProcessIds.RemoveAt(_freeProcessIds.Count - 1);
			}
			else
			{
				_processCount += 4;
				num = _processCount;
			}
			_processToIdMapping[num] = process;
			return ScriptingRuntimeHelpers.Int32ToObject(num);
		}
	}

	private static void SetEnvironment(StringDictionary currentEnvironment, object newEnvironment)
	{
		if (!(newEnvironment is PythonDictionary pythonDictionary))
		{
			throw PythonOps.TypeError("env argument must be a dict");
		}
		currentEnvironment.Clear();
		foreach (object item in pythonDictionary.keys())
		{
			if (!Converter.TryConvertToString(item, out var result))
			{
				throw PythonOps.TypeError("env dict contains a non-string key");
			}
			if (!Converter.TryConvertToString(pythonDictionary[item], out var result2))
			{
				throw PythonOps.TypeError("env dict contains a non-string value");
			}
			currentEnvironment[result] = result2;
		}
	}

	private static string ArgumentsToString(CodeContext context, object args)
	{
		StringBuilder stringBuilder = null;
		if (!PythonOps.TryGetEnumerator(context, args, out var enumerator))
		{
			throw PythonOps.TypeError("args parameter must be sequence, not {0}", DynamicHelpers.GetPythonType(args));
		}
		bool flag = false;
		try
		{
			enumerator.MoveNext();
			while (enumerator.MoveNext())
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				string text = PythonOps.ToString(enumerator.Current);
				if (flag)
				{
					stringBuilder.Append(' ');
				}
				if (text.IndexOf(' ') != -1)
				{
					stringBuilder.Append('"');
					stringBuilder.Append(text.Replace("\"", "\"\""));
					stringBuilder.Append('"');
				}
				else
				{
					stringBuilder.Append(text);
				}
				flag = true;
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		if (stringBuilder == null)
		{
			return "";
		}
		return stringBuilder.ToString();
	}

	public static void startfile(string filename, [DefaultParameterValue("open")] string operation)
	{
		Process process = new Process();
		process.StartInfo.FileName = filename;
		process.StartInfo.UseShellExecute = true;
		process.StartInfo.Verb = operation;
		try
		{
			process.Start();
		}
		catch (Exception e)
		{
			throw ToPythonException(e, filename);
		}
	}

	private static bool HasExecutableExtension(string path)
	{
		string text = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
		switch (text)
		{
		default:
			return text == ".bat";
		case ".exe":
		case ".dll":
		case ".com":
			return true;
		}
	}

	[Documentation("stat(path) -> stat result\nGathers statistics about the specified file or directory")]
	[LightThrowing]
	public static object stat(string path)
	{
		if (path == null)
		{
			return LightExceptions.Throw(PythonOps.TypeError("expected string, got NoneType"));
		}
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			int num = 0;
			long num2;
			if (Directory.Exists(path))
			{
				num2 = 0L;
				num = 16457;
			}
			else
			{
				if (!File.Exists(path))
				{
					return LightExceptions.Throw(PythonExceptions.CreateThrowable(WindowsError, 3, "file does not exist: " + path));
				}
				num2 = fileInfo.Length;
				num = 32768;
				if (HasExecutableExtension(path))
				{
					num |= 0x49;
				}
			}
			long num3 = (long)PythonTime.TicksToTimestamp(fileInfo.LastAccessTime.ToUniversalTime().Ticks);
			long num4 = (long)PythonTime.TicksToTimestamp(fileInfo.CreationTime.ToUniversalTime().Ticks);
			long num5 = (long)PythonTime.TicksToTimestamp(fileInfo.LastWriteTime.ToUniversalTime().Ticks);
			num |= 0x124;
			if ((fileInfo.Attributes & FileAttributes.ReadOnly) == 0)
			{
				num |= 0x92;
			}
			return new stat_result(num, num2, num3, num5, num4);
		}
		catch (ArgumentException)
		{
			return LightExceptions.Throw(PythonExceptions.CreateThrowable(WindowsError, 123, "The path is invalid: " + path));
		}
		catch (Exception e)
		{
			return LightExceptions.Throw(ToPythonException(e, path));
		}
	}

	public static string strerror(int code)
	{
		return code switch
		{
			0 => "No error", 
			7 => "Arg list too long", 
			13 => "Permission denied", 
			11 => "Resource temporarily unavailable", 
			9 => "Bad file descriptor", 
			16 => "Resource device", 
			10 => "No child processes", 
			36 => "Resource deadlock avoided", 
			33 => "Domain error", 
			10069 => "Unknown error", 
			17 => "File exists", 
			14 => "Bad address", 
			27 => "File too large", 
			42 => "Illegal byte sequence", 
			4 => "Interrupted function call", 
			22 => "Invalid argument", 
			5 => "Input/output error", 
			10056 => "Unknown error", 
			21 => "Is a directory", 
			24 => "Too many open files", 
			31 => "Too many links", 
			38 => "Filename too long", 
			23 => "Too many open files in system", 
			19 => "No such device", 
			2 => "No such file or directory", 
			8 => "Exec format error", 
			39 => "No locks available", 
			12 => "Not enough space", 
			28 => "No space left on device", 
			40 => "Function not implemented", 
			20 => "Not a directory", 
			41 => "Directory not empty", 
			10038 => "Unknown error", 
			25 => "Inappropriate I/O control operation", 
			6 => "No such device or address", 
			1 => "Operation not permitted", 
			32 => "Broken pipe", 
			34 => "Result too large", 
			30 => "Read-only file system", 
			29 => "Invalid seek", 
			3 => "No such process", 
			18 => "Improper link", 
			_ => "Unknown error " + code, 
		};
	}

	[Documentation("system(command) -> int\nExecute the command (a string) in a subshell.")]
	public static int system(string command)
	{
		ProcessStartInfo processInfo = GetProcessInfo(command);
		processInfo.CreateNoWindow = false;
		try
		{
			Process process = Process.Start(processInfo);
			if (process == null)
			{
				return -1;
			}
			process.WaitForExit();
			return process.ExitCode;
		}
		catch (Win32Exception)
		{
			return 1;
		}
	}

	public static string tempnam(CodeContext context)
	{
		return tempnam(context, null);
	}

	public static string tempnam(CodeContext context, string dir)
	{
		return tempnam(context, null, null);
	}

	public static string tempnam(CodeContext context, string dir, string prefix)
	{
		PythonOps.Warn(context, PythonExceptions.RuntimeWarning, "tempnam is a potential security risk to your program");
		try
		{
			dir = Path.GetTempPath();
			return Path.GetFullPath(Path.Combine(dir, prefix ?? string.Empty) + Path.GetRandomFileName());
		}
		catch (Exception e)
		{
			throw ToPythonException(e, dir);
		}
	}

	public static object times()
	{
		Process currentProcess = Process.GetCurrentProcess();
		return PythonTuple.MakeTuple(currentProcess.UserProcessorTime.TotalSeconds, currentProcess.PrivilegedProcessorTime.TotalSeconds, 0, 0, DateTime.Now.Subtract(currentProcess.StartTime).TotalSeconds);
	}

	public static PythonFile tmpfile(CodeContext context)
	{
		try
		{
			FileStream fileStream = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
			return PythonFile.Create(context, fileStream, fileStream.Name, "w+b");
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	public static string tmpnam(CodeContext context)
	{
		PythonOps.Warn(context, PythonExceptions.RuntimeWarning, "tmpnam is a potential security risk to your program");
		return Path.GetFullPath(Path.GetTempPath() + Path.GetRandomFileName());
	}

	public static void remove(string path)
	{
		UnlinkWorker(path);
	}

	public static void unlink(string path)
	{
		UnlinkWorker(path);
	}

	private static void UnlinkWorker(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1 || Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw PythonExceptions.CreateThrowable(WindowsError, 123, "The file could not be found for deletion: " + path);
		}
		if (!File.Exists(path))
		{
			throw PythonExceptions.CreateThrowable(WindowsError, 2, "The file could not be found for deletion: " + path);
		}
		try
		{
			File.Delete(path);
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static void unsetenv(string varname)
	{
		Environment.SetEnvironmentVariable(varname, null);
	}

	public static object urandom(int n)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] data = new byte[n];
		rNGCryptoServiceProvider.GetBytes(data);
		return PythonBinaryReader.PackDataIntoString(data, n);
	}

	public static object urandom(BigInteger n)
	{
		return urandom((int)n);
	}

	public static object urandom(double n)
	{
		throw PythonOps.TypeError("integer argument expected, got float");
	}

	public static int umask(CodeContext context, int mask)
	{
		mask &= 0x180;
		object setModuleState = PythonContext.GetContext(context).GetSetModuleState(_umaskKey, mask);
		if (setModuleState == null)
		{
			return 0;
		}
		return (int)setModuleState;
	}

	public static void utime(string path, PythonTuple times)
	{
		try
		{
			FileSystemInfo fileSystemInfo = (Directory.Exists(path) ? ((FileSystemInfo)new DirectoryInfo(path)) : ((FileSystemInfo)new FileInfo(path)));
			if (times == null)
			{
				fileSystemInfo.LastAccessTime = DateTime.Now;
				fileSystemInfo.LastWriteTime = DateTime.Now;
				return;
			}
			if (times.__len__() == 2)
			{
				DateTime lastAccessTime = new DateTime(PythonTime.TimestampToTicks(Converter.ConvertToDouble(times[0])), DateTimeKind.Utc);
				DateTime lastWriteTime = new DateTime(PythonTime.TimestampToTicks(Converter.ConvertToDouble(times[1])), DateTimeKind.Utc);
				fileSystemInfo.LastAccessTime = lastAccessTime;
				fileSystemInfo.LastWriteTime = lastWriteTime;
				return;
			}
			throw PythonOps.TypeError("times value must be a 2-value tuple (atime, mtime)");
		}
		catch (Exception e)
		{
			throw ToPythonException(e, path);
		}
	}

	public static PythonTuple waitpid(int pid, object options)
	{
		Process value;
		lock (_processToIdMapping)
		{
			if (!_processToIdMapping.TryGetValue(pid, out value))
			{
				throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, 10, "No child processes");
			}
		}
		value.WaitForExit();
		PythonTuple result = PythonTuple.MakeTuple(pid, value.ExitCode);
		lock (_processToIdMapping)
		{
			_processToIdMapping.Remove(pid & -4);
			_freeProcessIds.Add(pid & -4);
			return result;
		}
	}

	public static int write(CodeContext context, int fd, string text)
	{
		try
		{
			PythonContext context2 = PythonContext.GetContext(context);
			PythonFile fileFromId = context2.FileManager.GetFileFromId(context2, fd);
			fileFromId.write(text);
			return text.Length;
		}
		catch (Exception e)
		{
			throw ToPythonException(e);
		}
	}

	[Documentation("Send signal sig to the process pid. Constants for the specific signals available on the host platform \r\nare defined in the signal module.")]
	public static void kill(CodeContext context, int pid, int sig)
	{
		Process processById = Process.GetProcessById(pid);
		processById.Kill();
	}

	private static Exception ToPythonException(Exception e)
	{
		return ToPythonException(e, null);
	}

	private static Exception ToPythonException(Exception e, string filename)
	{
		if (e is IPythonAwareException)
		{
			return e;
		}
		if (e is ArgumentException || e is ArgumentNullException || e is ArgumentTypeException)
		{
			return ExceptionHelpers.UpdateForRethrow(e);
		}
		int lastWin32Error = Marshal.GetLastWin32Error();
		string text = e.Message;
		int num = 0;
		bool flag = false;
		if (e is Win32Exception ex)
		{
			num = ex.NativeErrorCode;
			text = GetFormattedException(e, num);
			flag = true;
		}
		else
		{
			if (e is UnauthorizedAccessException)
			{
				flag = true;
				num = 5;
				text = ((filename == null) ? "Access is denied" : $"Access is denied: '{filename}'");
			}
			if (e is IOException)
			{
				switch (lastWin32Error)
				{
				case 145:
					throw PythonExceptions.CreateThrowable(WindowsError, lastWin32Error, "The directory is not empty");
				case 5:
					throw PythonExceptions.CreateThrowable(WindowsError, lastWin32Error, "Access is denied");
				case 32:
					throw PythonExceptions.CreateThrowable(WindowsError, lastWin32Error, "The process cannot access the file because it is being used by another process");
				}
			}
			num = Marshal.GetHRForException(e);
			if ((num & -4096) == -2147024896)
			{
				num &= 0xFFF;
				text = GetFormattedException(e, num);
				flag = true;
			}
		}
		if (flag)
		{
			return PythonExceptions.CreateThrowable(WindowsError, num, text);
		}
		return PythonExceptions.CreateThrowable(PythonExceptions.OSError, num, text);
	}

	private static string GetFormattedException(Exception e, int hr)
	{
		return "[Errno " + hr + "] " + e.Message;
	}

	private static void addBase(string[] files, List ret)
	{
		foreach (string path in files)
		{
			ret.AddNoLock(Path.GetFileName(path));
		}
	}

	private static FileMode FileModeFromFlags(int flags)
	{
		if ((flags & 8) != 0)
		{
			return FileMode.Append;
		}
		if ((flags & 0x400) != 0)
		{
			if ((flags & 0x100) != 0)
			{
				return FileMode.CreateNew;
			}
			return FileMode.Open;
		}
		if ((flags & 0x100) != 0)
		{
			return FileMode.Create;
		}
		if ((flags & 0x200) != 0)
		{
			return FileMode.Truncate;
		}
		return FileMode.Open;
	}

	private static FileAccess FileAccessFromFlags(int flags)
	{
		if ((flags & 2) != 0)
		{
			return FileAccess.ReadWrite;
		}
		if ((flags & 1) != 0)
		{
			return FileAccess.Write;
		}
		return FileAccess.Read;
	}

	private static ProcessStartInfo GetProcessInfo(string command)
	{
		command = command.Trim();
		if (!TryGetExecutableCommand(command, out var baseCommand, out var args) && !TryGetShellCommand(command, out baseCommand, out args))
		{
			throw PythonOps.WindowsError("The system can not find command '{0}'", command);
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo(baseCommand, args);
		processStartInfo.UseShellExecute = false;
		return processStartInfo;
	}

	private static bool TryGetExecutableCommand(string command, out string baseCommand, out string args)
	{
		baseCommand = command;
		args = string.Empty;
		if (command[0] == '"')
		{
			int i;
			for (i = 1; i < command.Length; i++)
			{
				if (command[i] == '"')
				{
					baseCommand = command.Substring(1, i - 1).Trim();
					if (i + 1 < command.Length)
					{
						args = command.Substring(i + 1);
					}
					break;
				}
			}
			if (i == command.Length)
			{
				baseCommand = command.Substring(1).Trim();
				command += "\"";
			}
		}
		else
		{
			int i = command.IndexOf(' ');
			if (i != -1)
			{
				baseCommand = command.Substring(0, i);
				args = command.Substring(i + 1);
			}
		}
		string fullPath = Path.GetFullPath(baseCommand);
		if (File.Exists(fullPath))
		{
			baseCommand = fullPath;
			return true;
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
		string[] array = new string[5]
		{
			string.Empty,
			".com",
			".exe",
			"cmd",
			".bat"
		};
		foreach (string text in array)
		{
			fullPath = Path.Combine(folderPath, baseCommand + text);
			if (File.Exists(fullPath))
			{
				baseCommand = fullPath;
				return true;
			}
		}
		return false;
	}

	private static bool TryGetShellCommand(string command, out string baseCommand, out string args)
	{
		baseCommand = Environment.GetEnvironmentVariable("COMSPEC");
		args = string.Empty;
		if (baseCommand == null)
		{
			baseCommand = Environment.GetEnvironmentVariable("SHELL");
			if (baseCommand == null)
			{
				return false;
			}
			args = $"-c \"{command}\"";
		}
		else
		{
			args = $"/c {command}";
		}
		return true;
	}

	private static Exception DirectoryExists()
	{
		return PythonExceptions.CreateThrowable(WindowsError, 183, "directory already exists");
	}
}
