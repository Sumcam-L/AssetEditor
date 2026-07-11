using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[DontMapIEnumerableToContains]
[PythonType("file")]
public class PythonFile : ICodeFormattable, IEnumerator<string>, IDisposable, IEnumerator, IWeakReferenceable
{
	private enum PythonFileMode
	{
		Binary,
		TextCrLf,
		TextCr,
		TextLf,
		UniversalNewline
	}

	private ConsoleStreamType _consoleStreamType;

	private SharedIO _io;

	internal Stream _stream;

	private string _mode;

	private string _name;

	private string _encoding;

	private PythonFileMode _fileMode;

	private PythonStreamReader _reader;

	private PythonStreamWriter _writer;

	protected bool _isOpen;

	private long? _reseekPosition;

	private WeakRefTracker _weakref;

	private string _enumValue;

	internal readonly PythonContext _context;

	private bool _softspace;

	internal bool IsConsole => _stream == null;

	internal Encoding Encoding
	{
		get
		{
			if (_reader == null)
			{
				if (_writer == null)
				{
					return null;
				}
				return _writer.Encoding;
			}
			return _reader.Encoding;
		}
	}

	internal PythonContext Context => _context;

	[Documentation("True if the file is closed, False if the file is still open")]
	public bool closed => !_isOpen;

	[Documentation("gets the mode of the file")]
	public string mode => _mode;

	[Documentation("gets the name of the file")]
	public string name => _name;

	[Documentation("gets the encoding used when reading/writing text")]
	public string encoding => _encoding;

	public bool softspace
	{
		get
		{
			return _softspace;
		}
		set
		{
			_softspace = value;
		}
	}

	public object newlines
	{
		get
		{
			if (_reader == null || !(_reader is PythonUniversalReader))
			{
				return null;
			}
			PythonUniversalReader.TerminatorStyles terminators = ((PythonUniversalReader)_reader).Terminators;
			switch (terminators)
			{
			case PythonUniversalReader.TerminatorStyles.None:
				return null;
			case PythonUniversalReader.TerminatorStyles.CrLf:
				return "\r\n";
			case PythonUniversalReader.TerminatorStyles.Cr:
				return "\r";
			case PythonUniversalReader.TerminatorStyles.Lf:
				return "\n";
			default:
			{
				List<string> list = new List<string>();
				if ((terminators & PythonUniversalReader.TerminatorStyles.CrLf) != PythonUniversalReader.TerminatorStyles.None)
				{
					list.Add("\r\n");
				}
				if ((terminators & PythonUniversalReader.TerminatorStyles.Cr) != PythonUniversalReader.TerminatorStyles.None)
				{
					list.Add("\r");
				}
				if ((terminators & PythonUniversalReader.TerminatorStyles.Lf) != PythonUniversalReader.TerminatorStyles.None)
				{
					list.Add("\n");
				}
				return new PythonTuple(list.ToArray());
			}
			}
		}
	}

	string IEnumerator<string>.Current => _enumValue;

	object IEnumerator.Current => _enumValue;

	internal PythonFile(PythonContext context)
	{
		_context = context;
	}

	public PythonFile(CodeContext context)
		: this(PythonContext.GetContext(context))
	{
	}

	internal static PythonFile Create(CodeContext context, Stream stream, string name, string mode)
	{
		return Create(context, stream, PythonContext.GetContext(context).DefaultEncoding, name, mode);
	}

	internal static PythonFile Create(CodeContext context, Stream stream, Encoding encoding, string name, string mode)
	{
		PythonFile pythonFile = new PythonFile(PythonContext.GetContext(context));
		pythonFile.__init__(stream, encoding, name, mode);
		return pythonFile;
	}

	internal static PythonFile CreateConsole(PythonContext context, SharedIO io, ConsoleStreamType type, string name)
	{
		PythonFile pythonFile = new PythonFile(context);
		pythonFile.InitializeConsole(io, type, name);
		return pythonFile;
	}

	~PythonFile()
	{
		try
		{
			Dispose(disposing: false);
		}
		catch (ObjectDisposedException)
		{
		}
		catch (EncoderFallbackException)
		{
		}
	}

	public void __init__(CodeContext context, string name, [DefaultParameterValue("r")] string mode, [DefaultParameterValue(-1)] int buffering)
	{
		FileShare share = FileShare.ReadWrite;
		if (name == null)
		{
			throw PythonOps.TypeError("file name must be string, found NoneType");
		}
		if (mode == null)
		{
			throw PythonOps.TypeError("mode must be string, not None");
		}
		if (string.IsNullOrEmpty(mode))
		{
			throw PythonOps.ValueError("empty mode string");
		}
		TranslateAndValidateMode(mode, out var fmode, out var faccess, out var seekEnd);
		try
		{
			Stream stream;
			try
			{
				stream = ((Environment.OSVersion.Platform == PlatformID.Win32NT && name == "nul") ? Stream.Null : ((buffering > 0) ? PythonContext.GetContext(context).DomainManager.Platform.OpenInputFileStream(name, fmode, faccess, share, buffering) : PythonContext.GetContext(context).DomainManager.Platform.OpenInputFileStream(name, fmode, faccess, share)));
			}
			catch (IOException ioe)
			{
				AddFilename(context, name, ioe);
				throw;
			}
			GC.SuppressFinalize(stream);
			if (seekEnd)
			{
				stream.Seek(0L, SeekOrigin.End);
			}
			__init__(stream, PythonContext.GetContext(context).DefaultEncoding, name, mode);
			_isOpen = true;
		}
		catch (UnauthorizedAccessException e)
		{
			throw ToIoException(context, name, e);
		}
	}

	internal static Exception ToIoException(CodeContext context, string name, UnauthorizedAccessException e)
	{
		Exception ex = new IOException(e.Message, e);
		AddFilename(context, name, ex);
		return ex;
	}

	internal static void AddFilename(CodeContext context, string name, Exception ioe)
	{
		object o = PythonExceptions.ToPython(ioe);
		PythonOps.SetAttr(context, o, "filename", name);
	}

	internal static void ValidateMode(string mode)
	{
		TranslateAndValidateMode(mode, out var _, out var _, out var _);
	}

	private static void TranslateAndValidateMode(string mode, out FileMode fmode, out FileAccess faccess, out bool seekEnd)
	{
		if (mode.Length == 0)
		{
			throw PythonOps.ValueError("empty mode string");
		}
		string text = mode;
		if (mode.IndexOf('U') != -1)
		{
			mode = mode.Replace("U", string.Empty);
			if (mode.Length == 0)
			{
				mode = "r";
			}
			else if (mode == "+")
			{
				mode = "r+";
			}
			else
			{
				if (mode[0] == 'w' || mode[0] == 'a')
				{
					throw PythonOps.ValueError("universal newline mode can only be used with modes starting with 'r'");
				}
				mode = "r" + mode;
			}
		}
		seekEnd = false;
		switch (mode[0])
		{
		case 'r':
			fmode = FileMode.Open;
			break;
		case 'w':
			fmode = FileMode.Create;
			break;
		case 'a':
			fmode = FileMode.Append;
			break;
		default:
			throw PythonOps.ValueError("mode string must begin with one of 'r', 'w', 'a' or 'U', not '{0}'", text);
		}
		if (mode.IndexOf('+') != -1)
		{
			faccess = FileAccess.ReadWrite;
			if (fmode == FileMode.Append)
			{
				fmode = FileMode.OpenOrCreate;
				seekEnd = true;
			}
			return;
		}
		switch (fmode)
		{
		case FileMode.Create:
			faccess = FileAccess.Write;
			break;
		case FileMode.Open:
			faccess = FileAccess.Read;
			break;
		case FileMode.Append:
			faccess = FileAccess.Write;
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	public void __init__(CodeContext context, [NotNull] Stream stream)
	{
		ContractUtils.RequiresNotNull(stream, "stream");
		__init__(mode: (stream.CanRead && stream.CanWrite) ? "w+" : ((!stream.CanWrite) ? "r" : "w"), stream: stream, encoding: PythonContext.GetContext(context).DefaultEncoding);
	}

	public void __init__(CodeContext context, [NotNull] Stream stream, string mode)
	{
		__init__(stream, PythonContext.GetContext(context).DefaultEncoding, mode);
	}

	public void __init__([NotNull] Stream stream, Encoding encoding, string mode)
	{
		InternalInitialize(stream, encoding, mode);
	}

	public void __init__([NotNull] Stream stream, [NotNull] Encoding encoding, string name, string mode)
	{
		ContractUtils.RequiresNotNull(stream, "stream");
		ContractUtils.RequiresNotNull(encoding, "encoding");
		InternalInitialize(stream, encoding, name, mode);
	}

	private PythonTextReader CreateTextReader(TextReader reader, Encoding encoding, long initPosition)
	{
		return _fileMode switch
		{
			PythonFileMode.TextCrLf => new PythonTextCRLFReader(reader, encoding, initPosition), 
			PythonFileMode.TextCr => new PythonTextCRReader(reader, encoding, initPosition), 
			PythonFileMode.TextLf => new PythonTextLFReader(reader, encoding, initPosition), 
			PythonFileMode.UniversalNewline => new PythonUniversalReader(reader, encoding, initPosition), 
			_ => throw Assert.Unreachable, 
		};
	}

	private PythonTextReader CreateConsoleReader()
	{
		Encoding encoding;
		return CreateTextReader(_io.GetReader(out encoding), encoding, 0L);
	}

	private PythonTextWriter CreateTextWriter(TextWriter writer)
	{
		PythonFileMode pythonFileMode = _fileMode;
		if (_fileMode == PythonFileMode.UniversalNewline)
		{
			pythonFileMode = ((Environment.OSVersion.Platform != PlatformID.Unix) ? PythonFileMode.TextCrLf : PythonFileMode.TextLf);
		}
		return pythonFileMode switch
		{
			PythonFileMode.TextCrLf => new PythonTextWriter(writer, "\r\n"), 
			PythonFileMode.TextCr => new PythonTextWriter(writer, "\r"), 
			PythonFileMode.TextLf => new PythonTextWriter(writer, null), 
			_ => throw Assert.Unreachable, 
		};
	}

	internal bool SetMode(CodeContext context, bool text)
	{
		lock (this)
		{
			PythonFileMode pythonFileMode = MapFileMode(_mode);
			if (text)
			{
				if (pythonFileMode == PythonFileMode.Binary)
				{
					_fileMode = PythonFileMode.UniversalNewline;
				}
				else
				{
					_fileMode = pythonFileMode;
				}
			}
			else
			{
				_fileMode = PythonFileMode.Binary;
			}
			if (_stream != null)
			{
				if (!StringOps.TryGetEncoding(_encoding, out var defaultEncoding))
				{
					defaultEncoding = context.LanguageContext.DefaultEncoding;
				}
				InitializeReaderAndWriter(_stream, defaultEncoding);
			}
			else if (text)
			{
				InitializeConsole(_io, _consoleStreamType, _name);
			}
			else
			{
				InitializeReaderAndWriter(_stream = _io.GetStream(_consoleStreamType), _io.GetEncoding(_consoleStreamType));
			}
			if (_fileMode == PythonFileMode.Binary)
			{
				return false;
			}
			return true;
		}
	}

	internal void InternalInitialize(Stream stream, Encoding encoding, string mode)
	{
		_stream = stream;
		_mode = mode;
		_isOpen = true;
		_io = null;
		_fileMode = MapFileMode(mode);
		_encoding = StringOps.GetEncodingName(encoding);
		InitializeReaderAndWriter(stream, encoding);
		if (stream is FileStream fileStream)
		{
			_name = fileStream.Name;
		}
		else
		{
			_name = "nul";
		}
	}

	private void InitializeReaderAndWriter(Stream stream, Encoding encoding)
	{
		if (stream.CanRead)
		{
			if (_fileMode == PythonFileMode.Binary)
			{
				_reader = new PythonBinaryReader(stream);
			}
			else
			{
				long initPosition = (stream.CanSeek ? stream.Position : 0);
				_reader = CreateTextReader(new StreamReader(stream, encoding), encoding, initPosition);
			}
		}
		if (stream.CanWrite)
		{
			if (_fileMode == PythonFileMode.Binary)
			{
				_writer = new PythonBinaryWriter(stream);
			}
			else
			{
				_writer = CreateTextWriter(new StreamWriter(stream, encoding));
			}
		}
	}

	internal void InitializeConsole(SharedIO io, ConsoleStreamType type, string name)
	{
		_consoleStreamType = type;
		_io = io;
		_mode = ((type == ConsoleStreamType.Input) ? "r" : "w");
		_isOpen = true;
		_fileMode = MapFileMode(_mode);
		_name = name;
		_encoding = StringOps.GetEncodingName(io.OutputEncoding);
		if (type == ConsoleStreamType.Input)
		{
			_reader = CreateConsoleReader();
		}
		else
		{
			_writer = CreateTextWriter(_io.GetWriter(type));
		}
	}

	internal void InternalInitialize(Stream stream, Encoding encoding, string name, string mode)
	{
		InternalInitialize(stream, encoding, mode);
		_name = name;
	}

	private static PythonFileMode MapFileMode(string mode)
	{
		if (mode.Contains("b"))
		{
			return PythonFileMode.Binary;
		}
		if (mode.Contains("U"))
		{
			return PythonFileMode.UniversalNewline;
		}
		return Environment.NewLine switch
		{
			"\r\n" => PythonFileMode.TextCrLf, 
			"\r" => PythonFileMode.TextCr, 
			"\n" => PythonFileMode.TextLf, 
			_ => throw new NotImplementedException("Unsupported Environment.NewLine value"), 
		};
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[PythonHidden]
	protected virtual void Dispose(bool disposing)
	{
		lock (this)
		{
			if (_isOpen)
			{
				FlushNoLock();
				_isOpen = false;
				if (!IsConsole)
				{
					_stream.Close();
				}
				PythonFileManager rawFileManager = _context.RawFileManager;
				if (rawFileManager != null)
				{
					rawFileManager.Remove(this);
					rawFileManager.Remove(_stream);
				}
			}
		}
	}

	public virtual object close()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
		return null;
	}

	[PythonHidden]
	protected void ThrowIfClosed()
	{
		if (!_isOpen)
		{
			throw PythonOps.ValueError("I/O operation on closed file");
		}
	}

	public virtual void flush()
	{
		lock (this)
		{
			FlushNoLock();
		}
	}

	private void FlushNoLock()
	{
		ThrowIfClosed();
		if (_writer != null)
		{
			_writer.Flush();
			if (!IsConsole)
			{
				_stream.Flush();
			}
		}
	}

	public int fileno()
	{
		ThrowIfClosed();
		return _context.FileManager.GetIdFromFile(this);
	}

	public string read()
	{
		return read(-1);
	}

	public string read(int size)
	{
		PythonStreamReader reader = GetReader();
		if (size < 0)
		{
			return reader.ReadToEnd();
		}
		return reader.Read(size);
	}

	public string readline()
	{
		return GetReader().ReadLine();
	}

	public string readline(int size)
	{
		return GetReader().ReadLine(size);
	}

	public List readlines()
	{
		List list = new List();
		while (true)
		{
			string text = readline();
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			list.AddNoLock(text);
		}
		return list;
	}

	public List readlines(int sizehint)
	{
		List list = new List();
		while (true)
		{
			string text = readline();
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			list.AddNoLock(text);
			if (text.Length >= sizehint)
			{
				break;
			}
			sizehint -= text.Length;
		}
		return list;
	}

	public void seek(long offset)
	{
		seek(offset, 0);
	}

	public void seek(long offset, int whence)
	{
		if (_mode == "a")
		{
			return;
		}
		ThrowIfClosed();
		if (IsConsole || !_stream.CanSeek)
		{
			throw PythonOps.IOError("Can not seek on file " + _name);
		}
		lock (this)
		{
			FlushNoLock();
			SavePositionPreSeek();
			long position = _stream.Seek(offset, (SeekOrigin)whence);
			if (_reader != null)
			{
				_reader.DiscardBufferedData();
				_reader.Position = position;
			}
		}
	}

	public object tell()
	{
		long currentPosition = GetCurrentPosition();
		if (currentPosition <= int.MaxValue)
		{
			return (int)currentPosition;
		}
		return (BigInteger)currentPosition;
	}

	private long GetCurrentPosition()
	{
		if (_reader != null)
		{
			return _reader.Position;
		}
		if (_stream != null)
		{
			return _stream.Position;
		}
		throw PythonExceptions.CreateThrowable(PythonExceptions.IOError, 9, "Bad file descriptor");
	}

	public void truncate()
	{
		lock (this)
		{
			FlushNoLock();
			TruncateNoLock(GetCurrentPosition());
		}
	}

	public void truncate(long size)
	{
		lock (this)
		{
			FlushNoLock();
			TruncateNoLock(size);
		}
	}

	private void TruncateNoLock(long size)
	{
		if (size < 0)
		{
			throw PythonExceptions.CreateThrowable(PythonExceptions.IOError, 22, "Invalid argument");
		}
		lock (this)
		{
			if (_stream is FileStream fileStream)
			{
				if (fileStream.CanWrite)
				{
					fileStream.SetLength(size);
					return;
				}
				throw PythonExceptions.CreateThrowable(PythonExceptions.IOError, 13, "Permission denied");
			}
			throw PythonExceptions.CreateThrowable(PythonExceptions.IOError, 9, "Bad file descriptor");
		}
	}

	public void write(string s)
	{
		if (s == null)
		{
			throw PythonOps.TypeError("must be string or read-only character buffer, not None");
		}
		lock (this)
		{
			WriteNoLock(s);
		}
	}

	public void write([NotNull] IList<byte> bytes)
	{
		lock (this)
		{
			WriteNoLock(bytes);
		}
	}

	private void WriteNoLock(string s)
	{
		PythonStreamWriter writer = GetWriter();
		int num = writer.Write(s);
		if (!IsConsole && _reader != null && _stream.CanSeek)
		{
			_reader.Position += num;
		}
		if (IsConsole)
		{
			FlushNoLock();
		}
	}

	private void WriteNoLock([NotNull] IList<byte> b)
	{
		PythonStreamWriter writer = GetWriter();
		int num = writer.WriteBytes(b);
		if (!IsConsole && _reader != null && _stream.CanSeek)
		{
			_reader.Position += num;
		}
		if (IsConsole)
		{
			FlushNoLock();
		}
	}

	public void write([NotNull] PythonBuffer buf)
	{
		write((IList<byte>)buf);
	}

	public void write([NotNull] object arr)
	{
		WriteWorker(arr, locking: true);
	}

	private void WriteWorker(object arr, bool locking)
	{
		if (!(arr is IPythonArray pythonArray))
		{
			throw PythonOps.TypeError("file.write() argument must be string or read-only character buffer, not {0}", DynamicHelpers.GetPythonType(arr).Name);
		}
		if (_fileMode != PythonFileMode.Binary)
		{
			throw PythonOps.TypeError("file.write() argument must be string or buffer, not {0}", DynamicHelpers.GetPythonType(arr).Name);
		}
		if (locking)
		{
			write(pythonArray.tostring());
		}
		else
		{
			WriteNoLock(pythonArray.tostring());
		}
	}

	public void writelines(object o)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(o);
		if (!enumerator.MoveNext())
		{
			return;
		}
		lock (this)
		{
			do
			{
				if (!(enumerator.Current is string s))
				{
					if (enumerator.Current is Bytes arr)
					{
						WriteWorker(arr, locking: false);
						continue;
					}
					if (enumerator.Current is PythonBuffer b)
					{
						WriteNoLock(b);
						continue;
					}
					if (!(enumerator.Current is IPythonArray arr2))
					{
						throw PythonOps.TypeError("writelines() argument must be a sequence of strings");
					}
					WriteWorker(arr2, locking: false);
				}
				else
				{
					WriteNoLock(s);
				}
			}
			while (enumerator.MoveNext());
		}
	}

	[Python3Warning("f.xreadlines() not supported in 3.x, try 'for line in f' instead")]
	public PythonFile xreadlines()
	{
		return this;
	}

	private void SavePositionPreSeek()
	{
		if (_mode == "a+")
		{
			_reseekPosition = _stream.Position;
		}
	}

	private PythonStreamReader GetReader()
	{
		ThrowIfClosed();
		if (_reader == null)
		{
			throw PythonOps.IOError("Can not read from " + _name);
		}
		if (IsConsole)
		{
			lock (this)
			{
				if (!object.ReferenceEquals(_io.InputReader, _reader.TextReader))
				{
					_reader = CreateConsoleReader();
				}
			}
		}
		return _reader;
	}

	private PythonStreamWriter GetWriter()
	{
		ThrowIfClosed();
		if (_writer == null)
		{
			throw PythonOps.IOError("Can not write to " + _name);
		}
		lock (this)
		{
			if (IsConsole)
			{
				if (_stream != _io.GetStream(_consoleStreamType))
				{
					TextWriter writer = _io.GetWriter(_consoleStreamType);
					if (!object.ReferenceEquals(writer, _writer.TextWriter))
					{
						try
						{
							_writer.Flush();
						}
						catch (ObjectDisposedException)
						{
						}
						_writer = CreateTextWriter(writer);
					}
				}
			}
			else if (_reseekPosition.HasValue)
			{
				_stream.Seek(_reseekPosition.Value, SeekOrigin.Begin);
				_reader.Position = _reseekPosition.Value;
				_reseekPosition = null;
			}
		}
		return _writer;
	}

	public object next()
	{
		string text = readline();
		if (string.IsNullOrEmpty(text))
		{
			throw PythonOps.StopIteration();
		}
		return text;
	}

	public object __iter__()
	{
		ThrowIfClosed();
		return this;
	}

	public bool isatty()
	{
		return IsConsole;
	}

	public object __enter__()
	{
		ThrowIfClosed();
		return this;
	}

	public void __exit__(params object[] excinfo)
	{
		close();
	}

	public virtual string __repr__(CodeContext context)
	{
		return string.Format("<{0} file '{1}', mode '{2}' at 0x{3:X8}>", _isOpen ? "open" : "closed", _name ?? "<uninitialized file>", _mode ?? "<uninitialized file>", GetHashCode());
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		return _weakref;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		_weakref = value;
		return true;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		((IWeakReferenceable)this).SetWeakRef(value);
	}

	bool IEnumerator.MoveNext()
	{
		_enumValue = readline();
		if (string.IsNullOrEmpty(_enumValue))
		{
			return false;
		}
		return true;
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}
}
