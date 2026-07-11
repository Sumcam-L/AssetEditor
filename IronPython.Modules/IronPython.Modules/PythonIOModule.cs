using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonIOModule
{
	[PythonType]
	public class _IOBase : IEnumerator<object>, IDisposable, IEnumerator, IEnumerable<object>, IEnumerable, IWeakReferenceable, IDynamicMetaObjectProvider, IPythonExpandable
	{
		private bool _closed;

		internal CodeContext context;

		private object _current;

		private WeakRefTracker _weakref;

		private IDictionary<string, object> _customAttributes;

		public virtual bool closed => _closed;

		object IEnumerator<object>.Current => _current;

		object IEnumerator.Current => _current;

		IDictionary<string, object> IPythonExpandable.CustomAttributes => _customAttributes;

		CodeContext IPythonExpandable.Context => context;

		public _IOBase(CodeContext context)
		{
			this.context = context;
		}

		public void __del__(CodeContext context)
		{
			close(context);
		}

		public _IOBase __enter__()
		{
			_checkClosed();
			return this;
		}

		public void __exit__(CodeContext context, params object[] excinfo)
		{
			close(context);
		}

		public void _checkClosed()
		{
			_checkClosed(null);
		}

		public void _checkClosed(string msg)
		{
			if (closed)
			{
				throw PythonOps.ValueError(msg ?? "I/O operation on closed file.");
			}
		}

		public void _checkReadable()
		{
			_checkReadable(null);
		}

		public void _checkReadable(string msg)
		{
			if (!readable(context))
			{
				throw PythonOps.ValueError(msg ?? "File or stream is not readable.");
			}
		}

		public void _checkSeekable()
		{
			_checkSeekable(null);
		}

		public void _checkSeekable(string msg)
		{
			if (!seekable(context))
			{
				throw PythonOps.ValueError(msg ?? "File or stream is not seekable.");
			}
		}

		public void _checkWritable()
		{
			_checkWritable(null);
		}

		public void _checkWritable(string msg)
		{
			if (!writable(context))
			{
				throw PythonOps.ValueError(msg ?? "File or stream is not writable.");
			}
		}

		public virtual void close(CodeContext context)
		{
			try
			{
				if (!_closed)
				{
					flush(context);
				}
			}
			finally
			{
				_closed = true;
			}
		}

		public virtual int fileno(CodeContext context)
		{
			throw UnsupportedOperation(context, "fileno");
		}

		public virtual void flush(CodeContext context)
		{
			_checkClosed();
		}

		public virtual bool isatty(CodeContext context)
		{
			_checkClosed();
			return false;
		}

		[PythonHidden]
		public virtual Bytes peek(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			_checkClosed();
			throw AttributeError("peek");
		}

		[PythonHidden]
		public virtual object read(CodeContext context, [DefaultParameterValue(null)] object length)
		{
			throw AttributeError("read");
		}

		[PythonHidden]
		public virtual Bytes read1(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			throw AttributeError("read1");
		}

		public virtual bool readable(CodeContext context)
		{
			return false;
		}

		public virtual object readline(CodeContext context, int limit)
		{
			_checkClosed();
			List<Bytes> list = new List<Bytes>();
			int num = 0;
			while (limit < 0 || list.Count < limit)
			{
				object obj = read(context, 1);
				if (obj == null)
				{
					break;
				}
				Bytes bytes = GetBytes(obj, "read()");
				if (bytes.Count == 0)
				{
					break;
				}
				list.Add(bytes);
				num += bytes.Count;
				if (bytes._bytes[bytes.Count - 1] == 10)
				{
					break;
				}
			}
			return Bytes.Concat(list, num);
		}

		public object readline(CodeContext context, [DefaultParameterValue(null)] object limit)
		{
			return readline(context, GetInt(limit, -1));
		}

		public virtual List readlines()
		{
			return readlines(null);
		}

		public virtual List readlines([DefaultParameterValue(null)] object hint)
		{
			int num = GetInt(hint, -1);
			List list = new List();
			if (num <= 0)
			{
				using (IEnumerator<object> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						object current = enumerator.Current;
						list.AddNoLock(current);
					}
					return list;
				}
			}
			int num2 = 0;
			using (IEnumerator<object> enumerator2 = GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					object current2 = enumerator2.Current;
					if (current2 is Bytes bytes)
					{
						list.AddNoLock(current2);
						num2 += bytes.Count;
						if (num2 >= num)
						{
							break;
						}
						continue;
					}
					if (current2 is string text)
					{
						list.AddNoLock(current2);
						num2 += text.Length;
						if (num2 >= num)
						{
							break;
						}
						continue;
					}
					throw PythonOps.TypeError("next() should return string or bytes");
				}
			}
			return list;
		}

		public virtual BigInteger seek(CodeContext context, BigInteger pos, [DefaultParameterValue(0)] object whence)
		{
			throw UnsupportedOperation(context, "seek");
		}

		public virtual bool seekable(CodeContext context)
		{
			return false;
		}

		public virtual BigInteger tell(CodeContext context)
		{
			return seek(context, 0, 1);
		}

		public virtual BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			throw UnsupportedOperation(context, "truncate");
		}

		public virtual bool writable(CodeContext context)
		{
			return false;
		}

		[PythonHidden]
		public virtual BigInteger write(CodeContext context, object buf)
		{
			throw AttributeError("write");
		}

		public virtual void writelines(CodeContext context, object lines)
		{
			_checkClosed();
			IEnumerator enumerator = PythonOps.GetEnumerator(context, lines);
			while (enumerator.MoveNext())
			{
				write(context, enumerator.Current);
			}
		}

		~_IOBase()
		{
			try
			{
				close(context);
			}
			catch
			{
			}
		}

		bool IEnumerator.MoveNext()
		{
			_current = readline(context, -1);
			if (_current == null)
			{
				return false;
			}
			if (_current is Bytes bytes)
			{
				return bytes.Count > 0;
			}
			if (_current is string text)
			{
				return text.Length > 0;
			}
			return PythonOps.IsTrue(_current);
		}

		void IEnumerator.Reset()
		{
			_current = null;
			seek(context, 0, 0);
		}

		[PythonHidden]
		public IEnumerator<object> GetEnumerator()
		{
			_checkClosed();
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			_checkClosed();
			return this;
		}

		void IDisposable.Dispose()
		{
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

		IDictionary<string, object> IPythonExpandable.EnsureCustomAttributes()
		{
			return _customAttributes = new Dictionary<string, object>();
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<_IOBase>(parameter, this);
		}

		internal Exception UnsupportedOperation(CodeContext context, string attr)
		{
			throw PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState(_unsupportedOperationKey), $"{PythonTypeOps.GetName(this)}.{attr} not supported");
		}

		internal Exception AttributeError(string attrName)
		{
			throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", PythonTypeOps.GetName(this), attrName);
		}

		internal Exception InvalidPosition(BigInteger pos)
		{
			return PythonOps.IOError("Raw stream returned invalid position {0}", pos);
		}
	}

	[PythonType]
	public class _BufferedIOBase : _IOBase, IDynamicMetaObjectProvider
	{
		public _BufferedIOBase(CodeContext context)
			: base(context)
		{
		}

		public virtual object detach(CodeContext context)
		{
			throw UnsupportedOperation(context, "detach");
		}

		public override object read(CodeContext context, [DefaultParameterValue(null)] object length)
		{
			throw UnsupportedOperation(context, "read");
		}

		public virtual BigInteger readinto(CodeContext context, object buf)
		{
			int num = -1;
			if (PythonOps.HasAttr(context, buf, "__len__"))
			{
				num = PythonOps.Length(buf);
			}
			object obj = read(context, num);
			if (obj == null)
			{
				return BigInteger.Zero;
			}
			Bytes bytes = GetBytes(obj, "read()");
			if (buf is IList<byte> list)
			{
				for (int i = 0; i < bytes.Count; i++)
				{
					list[i] = bytes._bytes[i];
				}
				GC.KeepAlive(this);
				return bytes.Count;
			}
			if (PythonOps.TryGetBoundAttr(buf, "__setslice__", out var ret))
			{
				PythonOps.CallWithContext(context, ret, new Slice(bytes.Count), bytes);
				GC.KeepAlive(this);
				return bytes.Count;
			}
			if (PythonOps.TryGetBoundAttr(buf, "__setitem__", out ret))
			{
				for (int j = 0; j < bytes.Count; j++)
				{
					PythonOps.CallWithContext(context, ret, j, bytes[context, j]);
				}
				GC.KeepAlive(this);
				return bytes.Count;
			}
			throw PythonOps.TypeError("must be read-write buffer, not " + PythonTypeOps.GetName(buf));
		}

		public override BigInteger write(CodeContext context, object buf)
		{
			throw UnsupportedOperation(context, "write");
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<_BufferedIOBase>(parameter, this);
		}
	}

	[DontMapIDisposableToContextManager]
	[PythonType]
	public class BytesIO : _BufferedIOBase, IEnumerator, IDisposable, IDynamicMetaObjectProvider
	{
		private static readonly int DEFAULT_BUF_SIZE = 20;

		private byte[] _data;

		private int _pos;

		private int _length;

		private object _current;

		public override bool closed => _data == null;

		object IEnumerator.Current
		{
			get
			{
				_checkClosed();
				return _current;
			}
		}

		internal BytesIO(CodeContext context)
			: base(context)
		{
		}

		public BytesIO(CodeContext context, [DefaultParameterValue(null)] object initial_bytes)
			: base(context)
		{
		}

		public void __init__([DefaultParameterValue(null)] object initial_bytes)
		{
			if (object.ReferenceEquals(_data, null))
			{
				_data = new byte[DEFAULT_BUF_SIZE];
			}
			_pos = (_length = 0);
			if (initial_bytes != null)
			{
				DoWrite(initial_bytes);
				_pos = 0;
			}
		}

		public override void close(CodeContext context)
		{
			_data = null;
		}

		public Bytes getvalue()
		{
			_checkClosed();
			if (_length == 0)
			{
				return Bytes.Empty;
			}
			byte[] array = new byte[_length];
			Array.Copy(_data, array, _length);
			return Bytes.Make(array);
		}

		[Documentation("isatty() -> False\n\nAlways returns False since BytesIO objects are not connected\nto a TTY-like device.")]
		public override bool isatty(CodeContext context)
		{
			_checkClosed();
			return false;
		}

		[Documentation("read([size]) -> read at most size bytes, returned as a bytes object.\n\nIf the size argument is negative, read until EOF is reached.\nReturn an empty string at EOF.")]
		public override object read(CodeContext context, [DefaultParameterValue(null)] object size)
		{
			_checkClosed();
			int num = GetInt(size, -1);
			int num2 = Math.Max(0, _length - _pos);
			if (num >= 0)
			{
				num2 = Math.Min(num2, num);
			}
			if (num2 == 0)
			{
				return Bytes.Empty;
			}
			byte[] array = new byte[num2];
			Array.Copy(_data, _pos, array, 0, num2);
			_pos += num2;
			return Bytes.Make(array);
		}

		[Documentation("read1(size) -> read at most size bytes, returned as a bytes object.\n\nIf the size argument is negative or omitted, read until EOF is reached.\nReturn an empty string at EOF.")]
		public override Bytes read1(CodeContext context, int size)
		{
			return (Bytes)read(context, size);
		}

		public override bool readable(CodeContext context)
		{
			return true;
		}

		[Documentation("readinto(array_or_bytearray) -> int.  Read up to len(b) bytes into b.\n\nReturns number of bytes read (0 for EOF).")]
		public BigInteger readinto([NotNull] ByteArray buffer)
		{
			_checkClosed();
			int num = Math.Min(_length - _pos, buffer.Count);
			for (int i = 0; i < num; i++)
			{
				buffer[i] = _data[_pos++];
			}
			return num;
		}

		public BigInteger readinto([NotNull] ArrayModule.array buffer)
		{
			_checkClosed();
			int num = Math.Min(_length - _pos, buffer.__len__() * buffer.itemsize);
			int num2 = num % buffer.itemsize;
			buffer.FromStream(new MemoryStream(_data, _pos, num - num2, writable: false, publiclyVisible: false), 0);
			_pos += num - num2;
			if (num2 != 0)
			{
				byte[] array = buffer.RawGetItem(num / buffer.itemsize);
				for (int i = 0; i < num2; i++)
				{
					array[i] = _data[_pos++];
				}
				buffer.FromStream(new MemoryStream(array), num / buffer.itemsize);
			}
			return num;
		}

		public override BigInteger readinto(CodeContext context, object buf)
		{
			if (buf is ByteArray buffer)
			{
				return readinto(buffer);
			}
			if (buf is ArrayModule.array buffer2)
			{
				return readinto(buffer2);
			}
			_checkClosed();
			throw PythonOps.TypeError("must be read-write buffer, not {0}", PythonTypeOps.GetName(buf));
		}

		[Documentation("readline([size]) -> next line from the file, as bytes.\n\nRetain newline.  A non-negative size argument limits the maximum\nnumber of bytes to return (an incomplete line may be returned then).\nReturn an empty string at EOF.")]
		public override object readline(CodeContext context, [DefaultParameterValue(-1)] int limit)
		{
			return readline(limit);
		}

		private Bytes readline([DefaultParameterValue(-1)] int size)
		{
			_checkClosed();
			if (_pos >= _length || size == 0)
			{
				return Bytes.Empty;
			}
			int pos = _pos;
			while ((size < 0 || _pos - pos < size) && _pos < _length)
			{
				if (_data[_pos] == 10)
				{
					_pos++;
					break;
				}
				_pos++;
			}
			byte[] array = new byte[_pos - pos];
			Array.Copy(_data, pos, array, 0, _pos - pos);
			return Bytes.Make(array);
		}

		public Bytes readline(object size)
		{
			if (size == null)
			{
				return readline(-1);
			}
			_checkClosed();
			throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(size));
		}

		[Documentation("readlines([size]) -> list of bytes objects, each a line from the file.\n\nCall readline() repeatedly and return a list of the lines so read.\nThe optional size argument, if given, is an approximate bound on the\ntotal number of bytes in the lines returned.")]
		public override List readlines([DefaultParameterValue(null)] object hint)
		{
			_checkClosed();
			int num = GetInt(hint, -1);
			List list = new List();
			Bytes bytes = readline(-1);
			while (bytes.Count > 0)
			{
				list.append(bytes);
				if (num > 0)
				{
					num -= bytes.Count;
					if (num <= 0)
					{
						break;
					}
				}
				bytes = readline(-1);
			}
			return list;
		}

		[Documentation("seek(pos, whence=0) -> int.  Change stream position.\n\nSeek to byte offset pos relative to position indicated by whence:\n     0  Start of stream (the default).  pos should be >= 0;\n     1  Current position - pos may be negative;\n     2  End of stream - pos usually negative.\nReturns the new absolute position.")]
		public BigInteger seek(int pos, [DefaultParameterValue(0)] int whence)
		{
			_checkClosed();
			switch (whence)
			{
			case 0:
				if (pos < 0)
				{
					throw PythonOps.ValueError("negative seek value {0}", pos);
				}
				_pos = pos;
				return _pos;
			case 1:
				_pos = Math.Max(0, _pos + pos);
				return _pos;
			case 2:
				_pos = Math.Max(0, _length + pos);
				return _pos;
			default:
				throw PythonOps.ValueError("invalid whence ({0}, should be 0, 1 or 2)", whence);
			}
		}

		public BigInteger seek(double pos, [DefaultParameterValue(0)] int whence)
		{
			throw PythonOps.TypeError("'float' object cannot be interpreted as an index");
		}

		public override BigInteger seek(CodeContext context, BigInteger pos, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			int pos2 = (int)pos;
			if (whence is double || whence is Extensible<double>)
			{
				if (PythonContext.GetContext(context).PythonOptions.Python30)
				{
					throw PythonOps.TypeError("integer argument expected, got float");
				}
				PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "integer argument expected, got float");
				return seek(pos2, Converter.ConvertToInt32(whence));
			}
			return seek(pos2, GetInt(whence));
		}

		public override bool seekable(CodeContext context)
		{
			return true;
		}

		[Documentation("tell() -> current file position, an integer")]
		public override BigInteger tell(CodeContext context)
		{
			_checkClosed();
			return _pos;
		}

		[Documentation("truncate([size]) -> int.  Truncate the file to at most size bytes.\n\nSize defaults to the current file position, as returned by tell().\nReturns the new size.  Imply an absolute seek to the position size.")]
		public BigInteger truncate()
		{
			return truncate(_pos);
		}

		public BigInteger truncate(int size)
		{
			_checkClosed();
			if (size < 0)
			{
				throw PythonOps.ValueError("negative size value {0}", size);
			}
			_length = Math.Min(_length, size);
			return size;
		}

		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object size)
		{
			if (size == null)
			{
				return truncate();
			}
			if (TryGetInt(size, out var value))
			{
				return truncate(value);
			}
			_checkClosed();
			throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(size));
		}

		public override bool writable(CodeContext context)
		{
			return true;
		}

		[Documentation("write(bytes) -> int.  Write bytes to file.\n\nReturn the number of bytes written.")]
		public override BigInteger write(CodeContext context, object bytes)
		{
			_checkClosed();
			return DoWrite(bytes);
		}

		[Documentation("writelines(sequence_of_strings) -> None.  Write strings to the file.\n\nNote that newlines are not added.  The sequence can be any iterable\nobject producing strings. This is equivalent to calling write() for\neach string.")]
		public void writelines([NotNull] IEnumerable lines)
		{
			_checkClosed();
			IEnumerator enumerator = lines.GetEnumerator();
			while (enumerator.MoveNext())
			{
				DoWrite(enumerator.Current);
			}
		}

		void IDisposable.Dispose()
		{
		}

		bool IEnumerator.MoveNext()
		{
			Bytes bytes = readline(-1);
			if (bytes.Count == 0)
			{
				return false;
			}
			_current = bytes;
			return true;
		}

		void IEnumerator.Reset()
		{
			seek(0, 0);
			_current = null;
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<BytesIO>(parameter, this);
		}

		private int DoWrite(byte[] bytes)
		{
			if (bytes.Length == 0)
			{
				return 0;
			}
			EnsureSizeSetLength(_pos + bytes.Length);
			Array.Copy(bytes, 0, _data, _pos, bytes.Length);
			_pos += bytes.Length;
			return bytes.Length;
		}

		private int DoWrite(ICollection<byte> bytes)
		{
			int count = bytes.Count;
			if (count == 0)
			{
				return 0;
			}
			EnsureSizeSetLength(_pos + count);
			bytes.CopyTo(_data, _pos);
			_pos += count;
			return count;
		}

		private int DoWrite(string bytes)
		{
			int length = bytes.Length;
			if (length == 0)
			{
				return 0;
			}
			byte[] array = new byte[length];
			for (int i = 0; i < length; i++)
			{
				int num = bytes[i];
				if (num < 256)
				{
					array[i] = (byte)num;
					continue;
				}
				throw PythonOps.TypeError("'unicode' does not have the buffer interface");
			}
			return DoWrite(array);
		}

		private int DoWrite(object bytes)
		{
			if (bytes is byte[])
			{
				return DoWrite((byte[])bytes);
			}
			if (bytes is Bytes)
			{
				return DoWrite(((Bytes)bytes)._bytes);
			}
			if (bytes is ArrayModule.array)
			{
				return DoWrite(((ArrayModule.array)bytes).ToByteArray());
			}
			if (bytes is ICollection<byte>)
			{
				return DoWrite((ICollection<byte>)bytes);
			}
			if (bytes is string)
			{
				return DoWrite((string)bytes);
			}
			throw PythonOps.TypeError("expected a readable buffer object");
		}

		private void EnsureSize(int size)
		{
			if (_data.Length < size)
			{
				size = ((size > DEFAULT_BUF_SIZE) ? Math.Max(size, _data.Length * 2) : DEFAULT_BUF_SIZE);
				byte[] data = _data;
				_data = new byte[size];
				Array.Copy(data, _data, _length);
			}
		}

		private void EnsureSizeSetLength(int size)
		{
			if (_data.Length < size)
			{
				EnsureSize(size);
				_length = size;
				return;
			}
			while (_length < _pos)
			{
				_data[_length++] = 0;
			}
			_length = Math.Max(_length, size);
		}
	}

	[PythonType]
	public class _RawIOBase : _IOBase, IDynamicMetaObjectProvider
	{
		public _RawIOBase(CodeContext context)
			: base(context)
		{
		}

		public override object read(CodeContext context, [DefaultParameterValue(null)] object size)
		{
			int num = GetInt(size, -1);
			if (num < 0)
			{
				return readall(context);
			}
			ByteArray byteArray = new ByteArray(new List<byte>(num));
			num = (int)readinto(context, byteArray);
			List<byte> bytes = byteArray._bytes;
			if (num < bytes.Count)
			{
				bytes.RemoveRange(num, bytes.Count - num);
			}
			return new Bytes(bytes);
		}

		public Bytes readall(CodeContext context)
		{
			List<Bytes> list = new List<Bytes>();
			int num = 0;
			while (true)
			{
				object obj = read(context, 8192);
				if (obj == null)
				{
					break;
				}
				Bytes bytes = GetBytes(obj, "read()");
				if (bytes.Count == 0)
				{
					break;
				}
				num += bytes.Count;
				list.Add(bytes);
			}
			return Bytes.Concat(list, num);
		}

		public virtual BigInteger readinto(CodeContext context, object buf)
		{
			throw UnsupportedOperation(context, "readinto");
		}

		public override BigInteger write(CodeContext context, object buf)
		{
			throw UnsupportedOperation(context, "write");
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<_RawIOBase>(parameter, this);
		}
	}

	[PythonType]
	public class _TextIOBase : _IOBase, IDynamicMetaObjectProvider
	{
		public virtual string encoding => null;

		public virtual string errors => null;

		public virtual object newlines => null;

		public _TextIOBase(CodeContext context)
			: base(context)
		{
		}

		public virtual object detach(CodeContext context)
		{
			throw UnsupportedOperation(context, "detach");
		}

		public override object read(CodeContext context, [DefaultParameterValue(-1)] object length)
		{
			throw UnsupportedOperation(context, "read");
		}

		public override object readline(CodeContext context, [DefaultParameterValue(-1)] int limit)
		{
			throw UnsupportedOperation(context, "readline");
		}

		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			throw UnsupportedOperation(context, "truncate");
		}

		public override BigInteger write(CodeContext context, object str)
		{
			throw UnsupportedOperation(context, "write");
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<_TextIOBase>(parameter, this);
		}
	}

	[PythonType]
	public class BufferedReader : _BufferedIOBase, IDynamicMetaObjectProvider
	{
		private _IOBase _rawIO;

		private object _raw;

		private int _bufSize;

		private Bytes _readBuf;

		private int _readBufPos;

		public object raw
		{
			get
			{
				return _raw;
			}
			set
			{
				_rawIO = value as _IOBase;
				_raw = value;
			}
		}

		public override bool closed
		{
			get
			{
				if (_rawIO != null)
				{
					return _rawIO.closed;
				}
				return PythonOps.IsTrue(PythonOps.GetBoundAttr(context, _raw, "closed"));
			}
		}

		public object name => PythonOps.GetBoundAttr(context, _raw, "name");

		public object mode => PythonOps.GetBoundAttr(context, _raw, "mode");

		internal static BufferedReader Create(CodeContext context, object raw, [DefaultParameterValue(8192)] int buffer_size)
		{
			BufferedReader bufferedReader = new BufferedReader(context, raw, buffer_size);
			bufferedReader.__init__(context, raw, buffer_size);
			return bufferedReader;
		}

		public BufferedReader(CodeContext context, object raw, [DefaultParameterValue(8192)] int buffer_size)
			: base(context)
		{
		}

		public void __init__(CodeContext context, object raw, [DefaultParameterValue(8192)] int buffer_size)
		{
			this.raw = raw;
			if (_rawIO != null)
			{
				if (!_rawIO.readable(context))
				{
					throw PythonOps.IOError("\"raw\" argument must be readable.");
				}
			}
			else if (PythonOps.Not(PythonOps.Invoke(context, _raw, "readable")))
			{
				throw PythonOps.IOError("\"raw\" argument must be readable.");
			}
			if (buffer_size <= 0)
			{
				throw PythonOps.ValueError("invalid buffer size (must be positive)");
			}
			_bufSize = buffer_size;
			_readBuf = Bytes.Empty;
		}

		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			if (_rawIO != null)
			{
				return _rawIO.truncate(context, pos);
			}
			return GetBigInt(PythonOps.Invoke(context, _raw, "truncate", pos), "truncate() should return integer");
		}

		public override void close(CodeContext context)
		{
			if (closed)
			{
				return;
			}
			try
			{
				flush(context);
			}
			finally
			{
				if (_rawIO != null)
				{
					_rawIO.close(context);
				}
				else
				{
					PythonOps.Invoke(context, _raw, "close");
				}
			}
		}

		public override object detach(CodeContext context)
		{
			if (_raw == null)
			{
				throw PythonOps.ValueError("raw stream already detached");
			}
			flush(context);
			object result = _raw;
			raw = null;
			return result;
		}

		public override bool seekable(CodeContext context)
		{
			if (_rawIO != null)
			{
				return _rawIO.seekable(context);
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "seekable"));
		}

		public override bool readable(CodeContext context)
		{
			if (_rawIO != null)
			{
				return _rawIO.readable(context);
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "readable"));
		}

		public override bool writable(CodeContext context)
		{
			if (_rawIO != null)
			{
				return _rawIO.writable(context);
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "writable"));
		}

		public override int fileno(CodeContext context)
		{
			if (_rawIO != null)
			{
				return _rawIO.fileno(context);
			}
			return GetInt(PythonOps.Invoke(context, _raw, "fileno"), "fileno() should return integer");
		}

		public override bool isatty(CodeContext context)
		{
			if (_rawIO != null)
			{
				return _rawIO.isatty(context);
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "isatty"));
		}

		public override object read(CodeContext context, [DefaultParameterValue(null)] object length)
		{
			int num = GetInt(length, -1);
			if (num < -1)
			{
				throw PythonOps.ValueError("invalid number of bytes to read");
			}
			lock (this)
			{
				return ReadNoLock(context, num);
			}
		}

		private Bytes ReadNoLock(CodeContext context, int length)
		{
			if (length == 0)
			{
				return Bytes.Empty;
			}
			if (length < 0)
			{
				List<Bytes> list = new List<Bytes>();
				int num = 0;
				if (_readBuf.Count > 0)
				{
					list.Add(ResetReadBuf());
					num += list[0].Count;
				}
				Bytes bytes;
				while (true)
				{
					object o = ((_rawIO == null) ? PythonOps.Invoke(context, _raw, "read", -1) : _rawIO.read(context, -1));
					bytes = GetBytes(o, "read()");
					if (bytes == null || bytes.Count == 0)
					{
						break;
					}
					list.Add(bytes);
					num += bytes.Count;
				}
				if (num == 0)
				{
					return bytes;
				}
				GC.KeepAlive(this);
				return Bytes.Concat(list, num);
			}
			if (length < _readBuf.Count - _readBufPos)
			{
				byte[] array = new byte[length];
				Array.Copy(_readBuf._bytes, _readBufPos, array, 0, length);
				_readBufPos += length;
				if (_readBufPos == _readBuf.Count)
				{
					ResetReadBuf();
				}
				GC.KeepAlive(this);
				return Bytes.Make(array);
			}
			List<Bytes> list2 = new List<Bytes>();
			int num2 = length;
			if (_readBuf.Count > 0)
			{
				list2.Add(ResetReadBuf());
				num2 -= list2[0].Count;
			}
			while (num2 > 0)
			{
				object obj = ((_rawIO == null) ? PythonOps.Invoke(context, _raw, "read", _bufSize) : _rawIO.read(context, _bufSize));
				Bytes readBuf = ((obj != null) ? GetBytes(obj, "read()") : Bytes.Empty);
				_readBuf = readBuf;
				if (_readBuf.Count == 0)
				{
					break;
				}
				if (num2 >= _readBuf.Count - _readBufPos)
				{
					num2 -= _readBuf.Count - _readBufPos;
					list2.Add(ResetReadBuf());
					continue;
				}
				byte[] array2 = new byte[num2];
				Array.Copy(_readBuf._bytes, 0, array2, 0, num2);
				list2.Add(Bytes.Make(array2));
				_readBufPos = num2;
				num2 = 0;
				break;
			}
			GC.KeepAlive(this);
			return Bytes.Concat(list2, length - num2);
		}

		public override Bytes peek(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			_checkClosed();
			if (length <= 0 || length > _bufSize)
			{
				length = _bufSize;
			}
			lock (this)
			{
				return PeekNoLock(context, length);
			}
		}

		private Bytes PeekNoLock(CodeContext context, int length)
		{
			int num = _readBuf.Count - _readBufPos;
			byte[] array = new byte[length];
			if (length <= num)
			{
				Array.Copy(_readBuf._bytes, _readBufPos, array, 0, length);
				return Bytes.Make(array);
			}
			object obj = ((_rawIO == null) ? PythonOps.Invoke(context, _raw, "read", length - _readBuf.Count + _readBufPos) : _rawIO.read(context, length - _readBuf.Count + _readBufPos));
			Bytes bytes = ((obj != null) ? GetBytes(obj, "read()") : Bytes.Empty);
			_readBuf = ResetReadBuf() + bytes;
			return _readBuf;
		}

		public override Bytes read1(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			if (length == 0)
			{
				return Bytes.Empty;
			}
			if (length < 0)
			{
				throw PythonOps.ValueError("number of bytes to read must be positive");
			}
			lock (this)
			{
				PeekNoLock(context, 1);
				return ReadNoLock(context, Math.Min(length, _readBuf.Count - _readBufPos));
			}
		}

		public override BigInteger tell(CodeContext context)
		{
			BigInteger bigInteger = ((_rawIO != null) ? _rawIO.tell(context) : GetBigInt(PythonOps.Invoke(context, _raw, "tell"), "tell() should return integer"));
			if (bigInteger < 0L)
			{
				throw InvalidPosition(bigInteger);
			}
			return bigInteger - _readBuf.Count + _readBufPos;
		}

		public BigInteger seek(double offset, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			throw PythonOps.TypeError("an integer is required");
		}

		public override BigInteger seek(CodeContext context, BigInteger pos, [DefaultParameterValue(0)] object whence)
		{
			int num = GetInt(whence);
			if (num < 0 || num > 2)
			{
				throw PythonOps.ValueError("invalid whence ({0}, should be 0, 1, or 2)", num);
			}
			lock (this)
			{
				if (num == 1)
				{
					pos -= (BigInteger)(_readBuf.Count - _readBufPos);
				}
				object i = ((_rawIO == null) ? PythonOps.Invoke(context, _raw, "seek", num) : ((object)_rawIO.seek(context, pos, num)));
				pos = GetBigInt(i, "seek() should return integer");
				ResetReadBuf();
				if (pos < 0L)
				{
					throw InvalidPosition(pos);
				}
				GC.KeepAlive(this);
				return pos;
			}
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<BufferedReader>(parameter, this);
		}

		private Bytes ResetReadBuf()
		{
			Bytes result;
			if (_readBufPos == 0)
			{
				result = _readBuf;
			}
			else
			{
				byte[] array = new byte[_readBuf.Count - _readBufPos];
				Array.Copy(_readBuf._bytes, _readBufPos, array, 0, array.Length);
				result = Bytes.Make(array);
				_readBufPos = 0;
			}
			_readBuf = Bytes.Empty;
			return result;
		}
	}

	[PythonType]
	public class BufferedWriter : _BufferedIOBase, IDynamicMetaObjectProvider
	{
		private _IOBase _rawIO;

		private object _raw;

		private int _bufSize;

		private List<byte> _writeBuf;

		public object raw
		{
			get
			{
				return _raw;
			}
			set
			{
				_rawIO = value as _IOBase;
				_raw = value;
			}
		}

		public override bool closed
		{
			get
			{
				if (_rawIO != null)
				{
					return _rawIO.closed;
				}
				return PythonOps.IsTrue(PythonOps.GetBoundAttr(context, _raw, "closed"));
			}
		}

		public object name => PythonOps.GetBoundAttr(context, _raw, "name");

		public object mode => PythonOps.GetBoundAttr(context, _raw, "mode");

		internal static BufferedWriter Create(CodeContext context, object raw, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
		{
			BufferedWriter bufferedWriter = new BufferedWriter(context, raw, buffer_size, max_buffer_size);
			bufferedWriter.__init__(context, raw, buffer_size, max_buffer_size);
			return bufferedWriter;
		}

		public BufferedWriter(CodeContext context, object raw, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
			: base(context)
		{
		}

		public void __init__(CodeContext context, object raw, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
		{
			if (max_buffer_size != null)
			{
				PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "max_buffer_size is deprecated");
			}
			this.raw = raw;
			if (_rawIO != null)
			{
				if (!_rawIO.writable(context))
				{
					throw PythonOps.IOError("\"raw\" argument must be writable.");
				}
			}
			else if (PythonOps.Not(PythonOps.Invoke(context, _raw, "writable")))
			{
				throw PythonOps.IOError("\"raw\" argument must be writable.");
			}
			if (buffer_size <= 0)
			{
				throw PythonOps.ValueError("invalid buffer size (must be positive)");
			}
			_bufSize = buffer_size;
			_writeBuf = new List<byte>();
		}

		public override void close(CodeContext context)
		{
			if (closed)
			{
				return;
			}
			try
			{
				flush(context);
			}
			finally
			{
				if (_rawIO != null)
				{
					_rawIO.close(context);
				}
				else
				{
					PythonOps.Invoke(context, _raw, "close");
				}
			}
		}

		public override object detach(CodeContext context)
		{
			if (_raw == null)
			{
				throw PythonOps.ValueError("raw stream already detached");
			}
			flush(context);
			object result = _raw;
			raw = null;
			return result;
		}

		public override bool seekable(CodeContext context)
		{
			if (_rawIO != null)
			{
				bool result = _rawIO.seekable(context);
				GC.KeepAlive(this);
				return result;
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "seekable"));
		}

		public override bool readable(CodeContext context)
		{
			if (_rawIO != null)
			{
				bool result = _rawIO.readable(context);
				GC.KeepAlive(this);
				return result;
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "readable"));
		}

		public override bool writable(CodeContext context)
		{
			if (_rawIO != null)
			{
				bool result = _rawIO.writable(context);
				GC.KeepAlive(this);
				return result;
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "writable"));
		}

		public override int fileno(CodeContext context)
		{
			if (_rawIO != null)
			{
				int result = _rawIO.fileno(context);
				GC.KeepAlive(this);
				return result;
			}
			return GetInt(PythonOps.Invoke(context, _raw, "fileno"), "fileno() should return integer");
		}

		public override bool isatty(CodeContext context)
		{
			if (_rawIO != null)
			{
				bool result = _rawIO.isatty(context);
				GC.KeepAlive(this);
				return result;
			}
			return PythonOps.IsTrue(PythonOps.Invoke(context, _raw, "isatty"));
		}

		public override BigInteger write(CodeContext context, object buf)
		{
			_checkClosed("write to closed file");
			IList<byte> bytes = GetBytes(buf);
			lock (this)
			{
				if (_writeBuf.Count > _bufSize)
				{
					FlushNoLock(context);
				}
				int count = _writeBuf.Count;
				_writeBuf.AddRange(bytes);
				count = _writeBuf.Count - count;
				if (_writeBuf.Count > _bufSize)
				{
					try
					{
						FlushNoLock(context);
					}
					catch (_BlockingIOErrorException)
					{
						if (_writeBuf.Count > _bufSize)
						{
							int num = _writeBuf.Count - _bufSize;
							count -= num;
							_writeBuf.RemoveRange(_bufSize, num);
						}
						throw;
					}
				}
				return count;
			}
		}

		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			lock (this)
			{
				FlushNoLock(context);
				if (pos == null)
				{
					pos = ((_rawIO == null) ? ((object)GetBigInt(PythonOps.Invoke(context, _raw, "tell"), "tell() should return integer")) : ((object)_rawIO.tell(context)));
				}
				if (_rawIO != null)
				{
					return _rawIO.truncate(context, pos);
				}
				BigInteger bigInt = GetBigInt(PythonOps.Invoke(context, _raw, "truncate", pos), "truncate() should return integer");
				GC.KeepAlive(this);
				return bigInt;
			}
		}

		public override void flush(CodeContext context)
		{
			lock (this)
			{
				FlushNoLock(context);
			}
		}

		private void FlushNoLock(CodeContext context)
		{
			_checkClosed("flush of closed file");
			int num = 0;
			try
			{
				while (_writeBuf.Count > 0)
				{
					object i = ((_rawIO == null) ? PythonOps.Invoke(context, _raw, "write", _writeBuf) : ((object)_rawIO.write(context, _writeBuf)));
					int num2 = GetInt(i, "write() should return integer");
					if (num2 > _writeBuf.Count || num2 < 0)
					{
						throw PythonOps.IOError("write() returned incorrect number of bytes");
					}
					_writeBuf.RemoveRange(0, num2);
					num += num2;
				}
			}
			catch (_BlockingIOErrorException o)
			{
				if (!PythonOps.TryGetBoundAttr(o, "characters_written", out var ret) || !TryGetInt(ret, out var value))
				{
					throw;
				}
				_writeBuf.RemoveRange(0, value);
				num += value;
				throw;
			}
		}

		public override BigInteger tell(CodeContext context)
		{
			BigInteger bigInteger = ((_rawIO != null) ? _rawIO.tell(context) : GetBigInt(PythonOps.Invoke(context, _raw, "tell"), "tell() should return integer"));
			if (bigInteger < 0L)
			{
				throw InvalidPosition(bigInteger);
			}
			GC.KeepAlive(this);
			return bigInteger + _writeBuf.Count;
		}

		public BigInteger seek(double offset, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			throw PythonOps.TypeError("an integer is required");
		}

		public override BigInteger seek(CodeContext context, BigInteger pos, [DefaultParameterValue(0)] object whence)
		{
			int num = GetInt(whence);
			if (num < 0 || num > 2)
			{
				throw PythonOps.ValueError("invalid whence ({0}, should be 0, 1, or 2)", num);
			}
			lock (this)
			{
				FlushNoLock(context);
				BigInteger bigInteger = ((_rawIO != null) ? _rawIO.seek(context, pos, num) : (bigInteger = GetBigInt(PythonOps.Invoke(context, _raw, "seek", pos, num), "seek() should return integer")));
				if (bigInteger < 0L)
				{
					throw InvalidPosition(pos);
				}
				GC.KeepAlive(this);
				return bigInteger;
			}
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<BufferedWriter>(parameter, this);
		}
	}

	[PythonType]
	public class BufferedRandom : _BufferedIOBase, IDynamicMetaObjectProvider
	{
		private _IOBase _inner;

		private int _bufSize;

		private Bytes _readBuf;

		private int _readBufPos;

		private List<byte> _writeBuf;

		public _IOBase raw
		{
			get
			{
				return _inner;
			}
			set
			{
				_inner = value;
			}
		}

		public override bool closed => _inner.closed;

		public object name => PythonOps.GetBoundAttr(context, _inner, "name");

		public object mode => PythonOps.GetBoundAttr(context, _inner, "mode");

		internal static BufferedRandom Create(CodeContext context, _IOBase raw, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
		{
			BufferedRandom bufferedRandom = new BufferedRandom(context, raw, buffer_size, max_buffer_size);
			bufferedRandom.__init__(context, raw, buffer_size, max_buffer_size);
			return bufferedRandom;
		}

		public BufferedRandom(CodeContext context, _IOBase raw, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
			: base(context)
		{
		}

		public void __init__(CodeContext context, _IOBase raw, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
		{
			if (max_buffer_size != null)
			{
				PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "max_buffer_size is deprecated");
			}
			raw._checkSeekable();
			if (buffer_size <= 0)
			{
				throw PythonOps.ValueError("invalid buffer size (must be positive)");
			}
			if (!raw.readable(context))
			{
				throw PythonOps.IOError("\"raw\" argument must be readable.");
			}
			if (!raw.writable(context))
			{
				throw PythonOps.IOError("\"raw\" argument must be writable.");
			}
			_bufSize = buffer_size;
			_inner = raw;
			_readBuf = Bytes.Empty;
			_writeBuf = new List<byte>();
		}

		public override void close(CodeContext context)
		{
			if (!closed)
			{
				try
				{
					flush(context);
				}
				finally
				{
					_inner.close(context);
				}
			}
		}

		public override object detach(CodeContext context)
		{
			if (_inner == null)
			{
				throw PythonOps.ValueError("raw stream already detached");
			}
			flush(context);
			_IOBase inner = _inner;
			_inner = null;
			return inner;
		}

		public override bool seekable(CodeContext context)
		{
			bool result = _inner.seekable(context);
			GC.KeepAlive(this);
			return result;
		}

		public override bool readable(CodeContext context)
		{
			bool result = _inner.readable(context);
			GC.KeepAlive(this);
			return result;
		}

		public override bool writable(CodeContext context)
		{
			bool result = _inner.writable(context);
			GC.KeepAlive(this);
			return result;
		}

		public override int fileno(CodeContext context)
		{
			int result = _inner.fileno(context);
			GC.KeepAlive(this);
			return result;
		}

		public override bool isatty(CodeContext context)
		{
			return _inner.isatty(context);
		}

		public override object read(CodeContext context, [DefaultParameterValue(null)] object length)
		{
			flush(context);
			int num = GetInt(length, -1);
			if (num < -1)
			{
				throw PythonOps.ValueError("invalid number of bytes to read");
			}
			lock (this)
			{
				return ReadNoLock(context, num);
			}
		}

		private Bytes ReadNoLock(CodeContext context, int length)
		{
			if (length == 0)
			{
				return Bytes.Empty;
			}
			if (length < 0)
			{
				List<Bytes> list = new List<Bytes>();
				int num = 0;
				if (_readBuf.Count > 0)
				{
					list.Add(ResetReadBuf());
					num += list[0].Count;
				}
				Bytes bytes;
				while (true)
				{
					bytes = (Bytes)_inner.read(context, -1);
					if (bytes == null || bytes.Count == 0)
					{
						break;
					}
					list.Add(bytes);
					num += bytes.Count;
				}
				if (num == 0)
				{
					return bytes;
				}
				GC.KeepAlive(this);
				return Bytes.Concat(list, num);
			}
			if (length < _readBuf.Count - _readBufPos)
			{
				byte[] array = new byte[length];
				Array.Copy(_readBuf._bytes, _readBufPos, array, 0, length);
				_readBufPos += length;
				if (_readBufPos == _readBuf.Count)
				{
					ResetReadBuf();
				}
				GC.KeepAlive(this);
				return Bytes.Make(array);
			}
			List<Bytes> list2 = new List<Bytes>();
			int num2 = length;
			if (_readBuf.Count > 0)
			{
				list2.Add(ResetReadBuf());
				num2 -= list2[0].Count;
			}
			while (num2 > 0)
			{
				_readBuf = ((Bytes)_inner.read(context, _bufSize)) ?? Bytes.Empty;
				if (_readBuf.Count == 0)
				{
					break;
				}
				if (num2 >= _readBuf.Count - _readBufPos)
				{
					num2 -= _readBuf.Count - _readBufPos;
					list2.Add(ResetReadBuf());
					continue;
				}
				byte[] array2 = new byte[num2];
				Array.Copy(_readBuf._bytes, 0, array2, 0, num2);
				list2.Add(Bytes.Make(array2));
				_readBufPos = num2;
				num2 = 0;
				break;
			}
			GC.KeepAlive(this);
			return Bytes.Concat(list2, length - num2);
		}

		public override Bytes peek(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			_checkClosed();
			flush(context);
			if (length <= 0 || length > _bufSize)
			{
				length = _bufSize;
			}
			lock (this)
			{
				return PeekNoLock(context, length);
			}
		}

		private Bytes PeekNoLock(CodeContext context, int length)
		{
			int num = _readBuf.Count - _readBufPos;
			byte[] array = new byte[length];
			if (length <= num)
			{
				Array.Copy(_readBuf._bytes, _readBufPos, array, 0, length);
				return Bytes.Make(array);
			}
			Bytes bytes = ((Bytes)_inner.read(context, length - _readBuf.Count + _readBufPos)) ?? Bytes.Empty;
			_readBuf = ResetReadBuf() + bytes;
			GC.KeepAlive(this);
			return _readBuf;
		}

		public override Bytes read1(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			flush(context);
			if (length == 0)
			{
				return Bytes.Empty;
			}
			if (length < 0)
			{
				throw PythonOps.ValueError("number of bytes to read must be positive");
			}
			lock (this)
			{
				PeekNoLock(context, 1);
				return ReadNoLock(context, Math.Min(length, _readBuf.Count - _readBufPos));
			}
		}

		private Bytes ResetReadBuf()
		{
			Bytes result;
			if (_readBufPos == 0)
			{
				result = _readBuf;
			}
			else
			{
				byte[] array = new byte[_readBuf.Count - _readBufPos];
				Array.Copy(_readBuf._bytes, _readBufPos, array, 0, array.Length);
				result = Bytes.Make(array);
				_readBufPos = 0;
			}
			_readBuf = Bytes.Empty;
			return result;
		}

		public override BigInteger write(CodeContext context, object buf)
		{
			_checkClosed("write to closed file");
			if (_readBuf.Count > 0)
			{
				lock (this)
				{
					_inner.seek(context, _readBufPos - _readBuf.Count, 1);
					ResetReadBuf();
				}
			}
			IList<byte> bytes = GetBytes(buf);
			lock (this)
			{
				if (_writeBuf.Count > _bufSize)
				{
					FlushNoLock(context);
				}
				int count = _writeBuf.Count;
				_writeBuf.AddRange(bytes);
				count = _writeBuf.Count - count;
				if (_writeBuf.Count > _bufSize)
				{
					try
					{
						FlushNoLock(context);
					}
					catch (_BlockingIOErrorException)
					{
						if (_writeBuf.Count > _bufSize)
						{
							int num = _writeBuf.Count - _bufSize;
							count -= num;
							_writeBuf.RemoveRange(_bufSize, num);
						}
						throw;
					}
				}
				return count;
			}
		}

		public override void flush(CodeContext context)
		{
			lock (this)
			{
				FlushNoLock(context);
			}
		}

		private void FlushNoLock(CodeContext context)
		{
			_checkClosed("flush of closed file");
			int num = 0;
			try
			{
				while (_writeBuf.Count > 0)
				{
					int num2 = (int)_inner.write(context, _writeBuf);
					if (num2 > _writeBuf.Count || num2 < 0)
					{
						throw PythonOps.IOError("write() returned incorrect number of bytes");
					}
					_writeBuf.RemoveRange(0, num2);
					num += num2;
				}
			}
			catch (_BlockingIOErrorException o)
			{
				if (!PythonOps.TryGetBoundAttr(o, "characters_written", out var ret) || !TryGetInt(ret, out var value))
				{
					throw;
				}
				_writeBuf.RemoveRange(0, value);
				num += value;
				throw;
			}
		}

		public override BigInteger readinto(CodeContext context, object buf)
		{
			flush(context);
			return base.readinto(context, buf);
		}

		public BigInteger seek(double offset, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			throw PythonOps.TypeError("an integer is required");
		}

		public override BigInteger seek(CodeContext context, BigInteger pos, [DefaultParameterValue(0)] object whence)
		{
			int num = GetInt(whence);
			if (num < 0 || num > 2)
			{
				throw PythonOps.ValueError("invalid whence ({0}, should be 0, 1, or 2)", num);
			}
			lock (this)
			{
				FlushNoLock(context);
				if (_readBuf.Count > 0)
				{
					_inner.seek(context, _readBufPos - _readBuf.Count, 1);
				}
				pos = _inner.seek(context, pos, whence);
				ResetReadBuf();
				if (pos < 0L)
				{
					throw PythonOps.IOError("seek() returned invalid position");
				}
				GC.KeepAlive(this);
				return pos;
			}
		}

		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			lock (this)
			{
				FlushNoLock(context);
				if (pos == null)
				{
					pos = tell(context);
				}
				BigInteger result = _inner.truncate(context, pos);
				GC.KeepAlive(this);
				return result;
			}
		}

		public override BigInteger tell(CodeContext context)
		{
			BigInteger bigInteger = _inner.tell(context);
			if (bigInteger < 0L)
			{
				throw InvalidPosition(bigInteger);
			}
			if (_writeBuf.Count > 0)
			{
				return bigInteger + _writeBuf.Count;
			}
			return bigInteger - _readBuf.Count + _readBufPos;
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<BufferedRandom>(parameter, this);
		}
	}

	[PythonType]
	public class BufferedRWPair : _BufferedIOBase, IDynamicMetaObjectProvider
	{
		private BufferedReader _reader;

		private BufferedWriter _writer;

		public object reader
		{
			get
			{
				return _reader;
			}
			set
			{
				BufferedReader bufferedReader = value as BufferedReader;
				if (bufferedReader == null)
				{
					bufferedReader = BufferedReader.Create(context, value, 8192);
				}
				_reader = bufferedReader;
			}
		}

		public object writer
		{
			get
			{
				return _writer;
			}
			set
			{
				BufferedWriter bufferedWriter = value as BufferedWriter;
				if (bufferedWriter == null)
				{
					bufferedWriter = BufferedWriter.Create(context, value, 8192, null);
				}
				_writer = bufferedWriter;
			}
		}

		public override bool closed => _writer.closed;

		public BufferedRWPair(CodeContext context, object reader, object writer, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
			: base(context)
		{
		}

		public void __init__(CodeContext context, object reader, object writer, [DefaultParameterValue(8192)] int buffer_size, [DefaultParameterValue(null)] object max_buffer_size)
		{
			if (max_buffer_size != null)
			{
				PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "max_buffer_size is deprecated");
			}
			this.reader = reader;
			this.writer = writer;
			if (!_reader.readable(context))
			{
				throw PythonOps.IOError("\"reader\" object must be readable.");
			}
			if (!_writer.writable(context))
			{
				throw PythonOps.IOError("\"writer\" object must be writable.");
			}
		}

		public override object read(CodeContext context, [DefaultParameterValue(null)] object length)
		{
			object result = _reader.read(context, length);
			GC.KeepAlive(this);
			return result;
		}

		public override BigInteger readinto(CodeContext context, object buf)
		{
			BigInteger result = _reader.readinto(context, buf);
			GC.KeepAlive(this);
			return result;
		}

		public override BigInteger write(CodeContext context, object buf)
		{
			BigInteger result = _writer.write(context, buf);
			GC.KeepAlive(this);
			return result;
		}

		public override Bytes peek(CodeContext context, [DefaultParameterValue(0)] int length)
		{
			Bytes result = _reader.peek(context, length);
			GC.KeepAlive(this);
			return result;
		}

		public override Bytes read1(CodeContext context, int length)
		{
			Bytes result = _reader.read1(context, length);
			GC.KeepAlive(this);
			return result;
		}

		public override bool readable(CodeContext context)
		{
			bool result = _reader.readable(context);
			GC.KeepAlive(this);
			return result;
		}

		public override bool writable(CodeContext context)
		{
			bool result = _writer.writable(context);
			GC.KeepAlive(this);
			return result;
		}

		public override void flush(CodeContext context)
		{
			_writer.flush(context);
			GC.KeepAlive(this);
		}

		public override void close(CodeContext context)
		{
			try
			{
				_writer.close(context);
			}
			finally
			{
				_reader.close(context);
			}
			GC.KeepAlive(this);
		}

		public override bool isatty(CodeContext context)
		{
			bool result = _reader.isatty(context) || _writer.isatty(context);
			GC.KeepAlive(this);
			return result;
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<BufferedRWPair>(parameter, this);
		}
	}

	[PythonType]
	public class TextIOWrapper : _TextIOBase, IEnumerator<object>, IDisposable, IEnumerator, IEnumerable<object>, IEnumerable, ICodeFormattable, IDynamicMetaObjectProvider
	{
		public int _CHUNK_SIZE = 128;

		internal _BufferedIOBase _bufferTyped;

		private object _buffer;

		internal string _encoding;

		internal string _errors;

		private bool _seekable;

		private bool _telling;

		private object _encoder;

		private object _decoder;

		private bool _line_buffering;

		private bool _readUniversal;

		private bool _readTranslate;

		internal bool _writeTranslate;

		private string _readNL;

		private string _writeNL;

		private int _decodedCharsUsed;

		private string _decodedChars;

		private Bytes _nextInput;

		private int _decodeFlags;

		private object _current;

		public object buffer => _buffer;

		public override string encoding => _encoding;

		public override string errors => _errors;

		public bool line_buffering => _line_buffering;

		public override object newlines
		{
			get
			{
				if (_readUniversal && _decoder != null)
				{
					if (_decoder is IncrementalNewlineDecoder incrementalNewlineDecoder)
					{
						return incrementalNewlineDecoder.newlines;
					}
					return PythonOps.GetBoundAttr(context, _decoder, "newlines");
				}
				return null;
			}
		}

		public override bool closed
		{
			get
			{
				if (_bufferTyped == null)
				{
					return PythonOps.IsTrue(PythonOps.GetBoundAttr(context, _buffer, "closed"));
				}
				return _bufferTyped.closed;
			}
		}

		public object name => PythonOps.GetBoundAttr(context, _buffer, "name");

		object IEnumerator<object>.Current => _current;

		object IEnumerator.Current => _current;

		internal TextIOWrapper(CodeContext context)
			: base(context)
		{
		}

		internal static TextIOWrapper Create(CodeContext context, object buffer, [DefaultParameterValue(null)] string encoding, [DefaultParameterValue(null)] string errors, [DefaultParameterValue(null)] string newline, [DefaultParameterValue(false)] bool line_buffering)
		{
			TextIOWrapper textIOWrapper = new TextIOWrapper(context, buffer, encoding, errors, newline, line_buffering);
			textIOWrapper.__init__(context, buffer, encoding, errors, newline, line_buffering);
			return textIOWrapper;
		}

		public TextIOWrapper(CodeContext context, object buffer, [DefaultParameterValue(null)] string encoding, [DefaultParameterValue(null)] string errors, [DefaultParameterValue(null)] string newline, [DefaultParameterValue(false)] bool line_buffering)
			: base(context)
		{
		}

		public void __init__(CodeContext context, object buffer, [DefaultParameterValue(null)] string encoding, [DefaultParameterValue(null)] string errors, [DefaultParameterValue(null)] string newline, [DefaultParameterValue(false)] bool line_buffering)
		{
			switch (newline)
			{
			default:
				throw PythonOps.ValueError(string.Format("illegal newline value: " + newline));
			case null:
			case "":
			case "\n":
			case "\r":
			case "\r\n":
				if (encoding == null)
				{
					encoding = PythonLocale.PreferredEncoding;
					if (encoding == "")
					{
						encoding = "ascii";
					}
				}
				if (errors == null)
				{
					errors = "strict";
				}
				_bufferTyped = buffer as _BufferedIOBase;
				_buffer = buffer;
				_encoding = encoding;
				_errors = errors;
				_seekable = (_telling = ((_bufferTyped != null) ? _bufferTyped.seekable(context) : PythonOps.IsTrue(PythonOps.Invoke(context, _buffer, "seekable"))));
				_line_buffering = line_buffering;
				_readUniversal = string.IsNullOrEmpty(newline);
				_readTranslate = newline == null;
				_readNL = newline;
				_writeTranslate = newline != "";
				_writeNL = (string.IsNullOrEmpty(newline) ? Environment.NewLine : newline);
				_decodedChars = "";
				_decodedCharsUsed = 0;
				break;
			}
		}

		public override bool seekable(CodeContext context)
		{
			return _seekable;
		}

		public override bool readable(CodeContext context)
		{
			if (_bufferTyped == null)
			{
				return PythonOps.IsTrue(PythonOps.Invoke(context, _buffer, "readable"));
			}
			return _bufferTyped.readable(context);
		}

		public override bool writable(CodeContext context)
		{
			if (_bufferTyped == null)
			{
				return PythonOps.IsTrue(PythonOps.Invoke(context, _buffer, "writable"));
			}
			return _bufferTyped.writable(context);
		}

		public override void flush(CodeContext context)
		{
			if (_bufferTyped != null)
			{
				_bufferTyped.flush(context);
			}
			else
			{
				PythonOps.Invoke(context, _buffer, "flush");
			}
			_telling = _seekable;
		}

		public override void close(CodeContext context)
		{
			if (closed)
			{
				return;
			}
			try
			{
				flush(context);
			}
			finally
			{
				if (_bufferTyped != null)
				{
					_bufferTyped.close(context);
				}
				else
				{
					PythonOps.Invoke(context, _buffer, "close");
				}
			}
		}

		public override int fileno(CodeContext context)
		{
			if (_bufferTyped == null)
			{
				return GetInt(PythonOps.Invoke(context, _buffer, "fileno"), "fileno() should return an int");
			}
			return _bufferTyped.fileno(context);
		}

		public override bool isatty(CodeContext context)
		{
			if (_bufferTyped == null)
			{
				return PythonOps.IsTrue(PythonOps.Invoke(context, _buffer, "isatty"));
			}
			return _bufferTyped.isatty(context);
		}

		public override BigInteger write(CodeContext context, object s)
		{
			string text = s as string;
			if (text == null)
			{
				if (!(s is Extensible<string> extensible))
				{
					throw PythonOps.TypeError("must be unicode, not {0}", PythonTypeOps.GetName(s));
				}
				text = extensible.Value;
			}
			if (closed)
			{
				throw PythonOps.ValueError("write to closed file");
			}
			int length = text.Length;
			bool flag = (_writeTranslate || _line_buffering) && text.Contains("\n");
			if (flag && _writeTranslate && _writeNL != "\n")
			{
				text = text.Replace("\n", _writeNL);
			}
			text = StringOps.encode(context, text, _encoding, _errors);
			if (_bufferTyped != null)
			{
				_bufferTyped.write(context, text);
			}
			else
			{
				PythonOps.Invoke(context, _buffer, "write", text);
			}
			if (_line_buffering && (flag || text.Contains("\r")))
			{
				flush(context);
			}
			_nextInput = null;
			if (_decoder != null)
			{
				PythonOps.Invoke(context, _decoder, "reset");
			}
			GC.KeepAlive(this);
			return length;
		}

		public override BigInteger tell(CodeContext context)
		{
			if (!_seekable)
			{
				throw PythonOps.IOError("underlying stream is not seekable");
			}
			if (!_telling)
			{
				throw PythonOps.IOError("telling position disabled by next() call");
			}
			flush(context);
			BigInteger bigInteger = ((_bufferTyped != null) ? _bufferTyped.tell(context) : GetBigInt(PythonOps.Invoke(context, _buffer, "tell"), "tell() should return an integer"));
			if (bigInteger < 0L)
			{
				throw InvalidPosition(bigInteger);
			}
			object decoder = _decoder;
			if (decoder == null || _nextInput == null)
			{
				if (!string.IsNullOrEmpty(_decodedChars))
				{
					throw PythonOps.AssertionError("pending decoded text");
				}
				return bigInteger;
			}
			IncrementalNewlineDecoder incrementalNewlineDecoder = decoder as IncrementalNewlineDecoder;
			bigInteger -= (BigInteger)_nextInput.Count;
			int num = _decodedCharsUsed;
			if (num == 0)
			{
				return bigInteger;
			}
			PythonTuple pythonTuple = ((incrementalNewlineDecoder == null) ? ((PythonTuple)PythonOps.Invoke(context, decoder, "getstate")) : incrementalNewlineDecoder.getstate(context));
			try
			{
				if (incrementalNewlineDecoder != null)
				{
					incrementalNewlineDecoder.SetState(context, Bytes.Empty, _decodeFlags);
				}
				else
				{
					PythonOps.Invoke(context, decoder, "setstate", PythonTuple.MakeTuple(Bytes.Empty, _decodeFlags));
				}
				BigInteger result = bigInteger;
				int num2 = 0;
				int num3 = 0;
				byte[] bytes = _nextInput._bytes;
				foreach (byte b in bytes)
				{
					Bytes bytes2 = new Bytes(new byte[1] { b });
					num2++;
					num3 = ((incrementalNewlineDecoder == null) ? (num3 + ((string)PythonOps.Invoke(context, decoder, "decode", bytes2)).Length) : (num3 + incrementalNewlineDecoder.decode(context, bytes2, final: false).Length));
					Bytes buf;
					if (incrementalNewlineDecoder != null)
					{
						incrementalNewlineDecoder.GetState(context, out buf, out _decodeFlags);
					}
					else
					{
						PythonTuple pythonTuple2 = (PythonTuple)PythonOps.Invoke(context, decoder, "getstate");
						buf = GetBytes(pythonTuple2[0], "getstate");
						_decodeFlags = Converter.ConvertToInt32(pythonTuple2[1]);
					}
					if ((buf == null || buf.Count == 0) && num3 <= num)
					{
						result += (BigInteger)num2;
						num -= num3;
						num2 = 0;
						num3 = 0;
					}
					if (num3 >= num)
					{
						num3 = ((incrementalNewlineDecoder == null) ? (num3 + ((string)PythonOps.Invoke(context, decoder, "decode", Bytes.Empty, true)).Length) : (num3 + incrementalNewlineDecoder.decode(context, Bytes.Empty, final: true).Length));
						if (num3 >= num)
						{
							break;
						}
						throw PythonOps.IOError("can't reconstruct logical file position");
					}
				}
				return result;
			}
			finally
			{
				if (incrementalNewlineDecoder != null)
				{
					incrementalNewlineDecoder.setstate(context, pythonTuple);
				}
				else
				{
					PythonOps.Invoke(context, decoder, "setstate", pythonTuple);
				}
			}
		}

		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			flush(context);
			if (pos == null)
			{
				pos = tell(context);
			}
			BigInteger result;
			if (pos is int)
			{
				result = (BigInteger)pos;
			}
			else if (pos is BigInteger)
			{
				result = (BigInteger)pos;
			}
			else if (!Converter.TryConvertToBigInteger(pos, out result))
			{
				throw PythonOps.TypeError("an integer is required");
			}
			seek(context, result, 0);
			if (_bufferTyped == null)
			{
				return GetBigInt(PythonOps.Invoke(context, _buffer, "truncate"), "truncate() should return an integer");
			}
			return _bufferTyped.truncate(context, null);
		}

		public override object detach(CodeContext context)
		{
			if (_buffer == null)
			{
				throw PythonOps.ValueError("buffer is already detached");
			}
			flush(context);
			object result = _bufferTyped ?? _buffer;
			_buffer = (_bufferTyped = null);
			return result;
		}

		public BigInteger seek(double offset, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			throw PythonOps.TypeError("an integer is required");
		}

		public override BigInteger seek(CodeContext context, BigInteger cookie, [DefaultParameterValue(0)] object whence)
		{
			int num = GetInt(whence);
			if (closed)
			{
				throw PythonOps.ValueError("tell on closed file");
			}
			if (!_seekable)
			{
				throw PythonOps.IOError("underlying stream is not seekable");
			}
			switch (num)
			{
			case 1:
				if (cookie != 0L)
				{
					throw PythonOps.IOError("can't do nonzero cur-relative seeks");
				}
				num = 0;
				cookie = tell(context);
				break;
			case 2:
			{
				if (cookie != 0L)
				{
					throw PythonOps.IOError("can't do nonzero end-relative seeks");
				}
				flush(context);
				BigInteger bigInteger = ((_bufferTyped != null) ? _bufferTyped.seek(context, BigInteger.Zero, 2) : GetBigInt(PythonOps.Invoke(context, _buffer, "seek", BigInteger.Zero, 2), "seek() should return an integer"));
				if (bigInteger < 0L)
				{
					throw InvalidPosition(bigInteger);
				}
				SetDecodedChars(string.Empty);
				_nextInput = null;
				if (_decoder != null)
				{
					if (_decoder is IncrementalNewlineDecoder incrementalNewlineDecoder)
					{
						incrementalNewlineDecoder.reset(context);
					}
					else
					{
						PythonOps.Invoke(context, _decoder, "reset");
					}
				}
				GC.KeepAlive(this);
				return bigInteger;
			}
			}
			if (num != 0)
			{
				throw PythonOps.ValueError("invalid whence ({0}, should be 0, 1, or 2)", num);
			}
			if (cookie < 0L)
			{
				throw PythonOps.ValueError("negative seek position {0}", cookie);
			}
			flush(context);
			UnpackCookie(cookie, out var pos, out var decodeFlags, out var bytesFed, out var skip, out var needEOF);
			if (_bufferTyped != null)
			{
				_bufferTyped.seek(context, pos, 0);
			}
			else
			{
				PythonOps.Invoke(context, _buffer, "seek", pos, 0);
			}
			SetDecodedChars(string.Empty);
			_nextInput = null;
			object decoder = _decoder;
			IncrementalNewlineDecoder incrementalNewlineDecoder2 = decoder as IncrementalNewlineDecoder;
			if (cookie == BigInteger.Zero && decoder != null)
			{
				if (incrementalNewlineDecoder2 != null)
				{
					incrementalNewlineDecoder2.reset(context);
				}
				else
				{
					PythonOps.Invoke(context, decoder, "reset");
				}
			}
			else if (decoder != null || decodeFlags != 0 || skip != 0)
			{
				if (_decoder == null)
				{
					decoder = GetDecoder(context);
					incrementalNewlineDecoder2 = decoder as IncrementalNewlineDecoder;
				}
				if (incrementalNewlineDecoder2 != null)
				{
					incrementalNewlineDecoder2.SetState(context, Bytes.Empty, decodeFlags);
				}
				else
				{
					PythonOps.Invoke(context, decoder, "setstate", PythonTuple.MakeTuple(Bytes.Empty, decodeFlags));
				}
				_decodeFlags = decodeFlags;
				_nextInput = Bytes.Empty;
			}
			if (skip > 0)
			{
				object obj = ((_bufferTyped != null) ? _bufferTyped.read(context, bytesFed) : PythonOps.Invoke(context, _buffer, "read", bytesFed));
				Bytes bytes = ((obj != null) ? GetBytes(obj, "read()") : Bytes.Empty);
				if (incrementalNewlineDecoder2 != null)
				{
					SetDecodedChars(incrementalNewlineDecoder2.decode(context, bytes, needEOF));
				}
				else
				{
					SetDecodedChars((string)PythonOps.Invoke(context, decoder, "decode", bytes, needEOF));
				}
				if (_decodedChars.Length < skip)
				{
					throw PythonOps.IOError("can't restore logical file position");
				}
				_decodedCharsUsed = skip;
			}
			try
			{
				object target = _encoder ?? GetEncoder(context);
				if (cookie == 0L)
				{
					PythonOps.Invoke(context, target, "reset");
				}
				else
				{
					PythonOps.Invoke(context, target, "setstate", 0);
				}
			}
			catch (LookupException)
			{
			}
			GC.KeepAlive(this);
			return cookie;
		}

		public override object read(CodeContext context, [DefaultParameterValue(null)] object length)
		{
			_checkClosed();
			if (!readable(context))
			{
				throw PythonOps.IOError("not readable");
			}
			int num = GetInt(length, -1);
			object o = _decoder ?? GetDecoder(context);
			if (num < 0)
			{
				string decodedChars = GetDecodedChars();
				object obj = ((_bufferTyped != null) ? _bufferTyped.read(context, -1) : PythonOps.Invoke(context, _buffer, "read", -1));
				object boundAttr = PythonOps.GetBoundAttr(context, o, "decode");
				string text = (string)PythonOps.CallWithKeywordArgs(context, boundAttr, new object[2] { obj, true }, new string[1] { "final" });
				SetDecodedChars(string.Empty);
				_nextInput = null;
				if (decodedChars == null)
				{
					return text;
				}
				return decodedChars + text;
			}
			StringBuilder stringBuilder = new StringBuilder(GetDecodedChars(num));
			bool flag = true;
			while (stringBuilder.Length < num && flag)
			{
				flag = ReadChunk(context);
				stringBuilder.Append(GetDecodedChars(num - stringBuilder.Length));
			}
			return stringBuilder.ToString();
		}

		public override object readline(CodeContext context, [DefaultParameterValue(-1)] int limit)
		{
			_checkClosed("read from closed file");
			string text = GetDecodedChars();
			int startIndex = 0;
			if (_decoder == null)
			{
				GetDecoder(context);
			}
			int num2;
			while (true)
			{
				if (_readTranslate)
				{
					int num = text.IndexOf('\n', startIndex);
					if (num >= 0)
					{
						num2 = num + 1;
						break;
					}
					startIndex = text.Length;
				}
				else if (_readUniversal)
				{
					int num3 = text.IndexOfAny(new char[2] { '\r', '\n' }, startIndex);
					if (num3 != -1)
					{
						num2 = ((text[num3] != '\n') ? ((text.Length <= num3 + 1 || text[num3 + 1] != '\n') ? (num3 + 1) : (num3 + 2)) : (num3 + 1));
						break;
					}
					startIndex = text.Length;
				}
				else
				{
					int num = text.IndexOf(_readNL);
					if (num >= 0)
					{
						num2 = num + _readNL.Length;
						break;
					}
				}
				if (limit >= 0 && text.Length >= limit)
				{
					num2 = limit;
					break;
				}
				while (ReadChunk(context) && string.IsNullOrEmpty(_decodedChars))
				{
				}
				if (!string.IsNullOrEmpty(_decodedChars))
				{
					text += GetDecodedChars();
					continue;
				}
				SetDecodedChars(string.Empty);
				_nextInput = null;
				return text;
			}
			if (limit >= 0 && num2 > limit)
			{
				num2 = limit;
			}
			RewindDecodedChars(text.Length - num2);
			GC.KeepAlive(this);
			return text.Substring(0, num2);
		}

		bool IEnumerator.MoveNext()
		{
			_telling = false;
			_current = readline(context, -1);
			bool flag = _current != null && (_current is Bytes { Count: >0 } || _current is string { Length: >0 } || PythonOps.IsTrue(_current));
			if (!flag)
			{
				_nextInput = null;
				_telling = _seekable;
			}
			return flag;
		}

		void IEnumerator.Reset()
		{
			_current = null;
			seek(context, 0, 0);
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			_checkClosed();
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			_checkClosed();
			return this;
		}

		public string __repr__(CodeContext context)
		{
			return $"<_io.TextIOWrapper encoding='{_encoding}'>";
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<TextIOWrapper>(parameter, this);
		}

		private void UnpackCookie(BigInteger cookie, out BigInteger pos, out int decodeFlags, out int bytesFed, out int skip, out bool needEOF)
		{
			BigInteger bigInteger = (BigInteger.One << 64) - 1;
			pos = cookie & bigInteger;
			cookie >>= 64;
			decodeFlags = (int)(cookie & bigInteger);
			cookie >>= 64;
			bytesFed = (int)(cookie & bigInteger);
			cookie >>= 64;
			skip = (int)(cookie & bigInteger);
			needEOF = cookie > bigInteger;
		}

		private object GetEncoder(CodeContext context)
		{
			object obj = PythonOps.LookupEncoding(context, _encoding);
			if (obj == null || !PythonOps.TryGetBoundAttr(context, obj, "incrementalencoder", out var ret))
			{
				throw PythonOps.LookupError(_encoding);
			}
			_encoder = PythonOps.CallWithContext(context, ret, _errors);
			return _encoder;
		}

		private object GetDecoder(CodeContext context)
		{
			object obj = PythonOps.LookupEncoding(context, _encoding);
			if (obj == null || !PythonOps.TryGetBoundAttr(context, obj, "incrementaldecoder", out var ret))
			{
				throw PythonOps.LookupError(_encoding);
			}
			_decoder = PythonOps.CallWithContext(context, ret, _errors);
			if (_readUniversal)
			{
				_decoder = new IncrementalNewlineDecoder(_decoder, _readTranslate, "strict");
			}
			return _decoder;
		}

		private void SetDecodedChars(string chars)
		{
			_decodedChars = chars;
			_decodedCharsUsed = 0;
		}

		private string GetDecodedChars()
		{
			string text = _decodedChars.Substring(_decodedCharsUsed);
			_decodedCharsUsed += text.Length;
			return text;
		}

		private string GetDecodedChars(int length)
		{
			length = Math.Min(length, _decodedChars.Length - _decodedCharsUsed);
			string result = _decodedChars.Substring(_decodedCharsUsed, length);
			_decodedCharsUsed += length;
			return result;
		}

		private void RewindDecodedChars(int length)
		{
			if (_decodedCharsUsed < length)
			{
				throw PythonOps.AssertionError("rewind decoded_chars out of bounds");
			}
			_decodedCharsUsed -= length;
		}

		private bool ReadChunk(CodeContext context)
		{
			if (_decoder == null)
			{
				throw PythonOps.ValueError("no decoder");
			}
			IncrementalNewlineDecoder incrementalNewlineDecoder = _decoder as IncrementalNewlineDecoder;
			Bytes buf = null;
			int flags = 0;
			if (_telling)
			{
				if (incrementalNewlineDecoder != null)
				{
					incrementalNewlineDecoder.GetState(context, out buf, out flags);
				}
				else
				{
					PythonTuple pythonTuple = (PythonTuple)PythonOps.Invoke(context, _decoder, "getstate");
					buf = GetBytes(pythonTuple[0], "getstate");
					flags = (int)pythonTuple[1];
				}
			}
			object obj = ((_bufferTyped != null) ? _bufferTyped.read(context, _CHUNK_SIZE) : PythonOps.Invoke(context, _buffer, "read", _CHUNK_SIZE));
			Bytes bytes = ((obj != null) ? GetBytes(obj, "read()") : Bytes.Empty);
			bool flag = obj == null || bytes.Count == 0;
			string decodedChars = ((incrementalNewlineDecoder == null) ? ((string)PythonOps.Invoke(context, _decoder, "decode", bytes, flag)) : incrementalNewlineDecoder.decode(context, bytes, flag));
			SetDecodedChars(decodedChars);
			if (_telling)
			{
				_decodeFlags = flags;
				_nextInput = buf + bytes;
			}
			return !flag;
		}
	}

	[PythonType]
	public class StringIO : TextIOWrapper, IDynamicMetaObjectProvider
	{
		public StringIO(CodeContext context, [DefaultParameterValue("")] string initial_value, [DefaultParameterValue("utf-8")] string encoding, [DefaultParameterValue("string")] string errors, [DefaultParameterValue("\n")] string newline)
			: base(context)
		{
		}

		public void __init__(CodeContext context, [DefaultParameterValue("")] string initial_value, [DefaultParameterValue("utf-8")] string encoding, [DefaultParameterValue("string")] string errors, [DefaultParameterValue("\n")] string newline)
		{
			__init__(context, new BytesIO(context), encoding, errors, newline, line_buffering: false);
			if (newline == null)
			{
				_writeTranslate = false;
			}
			if (!string.IsNullOrEmpty(initial_value))
			{
				write(context, initial_value);
				seek(context, 0, 0);
			}
		}

		public override object detach(CodeContext context)
		{
			throw UnsupportedOperation(context, "detach");
		}

		public string getvalue(CodeContext context)
		{
			flush(context);
			return ((BytesIO)_bufferTyped).getvalue().decode(context, _encoding, _errors);
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<StringIO>(parameter, this);
		}
	}

	[PythonType]
	public class IncrementalNewlineDecoder
	{
		[Flags]
		private enum LineEnding
		{
			None = 0,
			CR = 1,
			LF = 2,
			CRLF = 4,
			All = 7
		}

		private object _decoder;

		private bool _translate;

		private LineEnding _seenNL;

		private bool _pendingCR;

		private string _errors;

		public object newlines => _seenNL switch
		{
			LineEnding.None => null, 
			LineEnding.CR => "\r", 
			LineEnding.LF => "\n", 
			LineEnding.CRLF => "\r\n", 
			LineEnding.CR | LineEnding.LF => PythonTuple.MakeTuple("\r", "\n"), 
			LineEnding.CR | LineEnding.CRLF => PythonTuple.MakeTuple("\r", "\r\n"), 
			LineEnding.LF | LineEnding.CRLF => PythonTuple.MakeTuple("\n", "\r\n"), 
			_ => PythonTuple.MakeTuple("\r", "\n", "\r\n"), 
		};

		public IncrementalNewlineDecoder(object decoder, bool translate, [DefaultParameterValue("strict")] string errors)
		{
			_decoder = decoder;
			_translate = translate;
			_errors = errors;
		}

		public string decode(CodeContext context, [NotNull] IList<byte> input, [DefaultParameterValue(false)] bool final)
		{
			object obj = ((_decoder != null) ? PythonOps.CallWithKeywordArgs(context, PythonOps.GetBoundAttr(context, _decoder, "decode"), new object[2] { input, true }, new string[1] { "final" }) : input.MakeString());
			string text = obj as string;
			if (text == null)
			{
				if (!(obj is Extensible<string>))
				{
					throw PythonOps.TypeError("decoder produced {0}, expected str", PythonTypeOps.GetName(obj));
				}
				text = ((Extensible<string>)obj).Value;
			}
			return DecodeWorker(context, text, final);
		}

		public string decode(CodeContext context, [NotNull] string input, [DefaultParameterValue(false)] bool final)
		{
			if (_decoder == null)
			{
				return DecodeWorker(context, input, final);
			}
			return decode(context, new Bytes(input.MakeByteArray()), final);
		}

		private string DecodeWorker(CodeContext context, string decoded, bool final)
		{
			if (_pendingCR && (final || decoded.Length > 0))
			{
				decoded = "\r" + decoded;
				_pendingCR = false;
			}
			if (decoded.Length == 0)
			{
				return decoded;
			}
			if (!final && decoded.Length > 0 && decoded[decoded.Length - 1] == '\r')
			{
				decoded = decoded.Substring(0, decoded.Length - 1);
				_pendingCR = true;
			}
			if (_translate || _seenNL != LineEnding.All)
			{
				int num = decoded.count("\r\n");
				int num2 = decoded.count("\r") - num;
				if (_seenNL != LineEnding.All)
				{
					int num3 = decoded.count("\n") - num;
					_seenNL |= (LineEnding)((uint)(((num > 0) ? 4 : 0) | ((num3 > 0) ? 2 : 0)) | ((num2 > 0) ? 1u : 0u));
				}
				if (_translate)
				{
					if (num > 0)
					{
						decoded = decoded.Replace("\r\n", "\n");
					}
					if (num2 > 0)
					{
						decoded = decoded.Replace('\r', '\n');
					}
				}
			}
			return decoded;
		}

		public PythonTuple getstate(CodeContext context)
		{
			object obj = Bytes.Empty;
			int num = 0;
			if (_decoder != null)
			{
				PythonTuple pythonTuple = (PythonTuple)PythonOps.Invoke(context, _decoder, "getstate");
				obj = pythonTuple[0];
				num = Converter.ConvertToInt32(pythonTuple[1]) << 1;
			}
			if (_pendingCR)
			{
				num |= 1;
			}
			return PythonTuple.MakeTuple(obj, num);
		}

		internal void GetState(CodeContext context, out Bytes buf, out int flags)
		{
			PythonTuple pythonTuple = (PythonTuple)PythonOps.Invoke(context, _decoder, "getstate");
			buf = GetBytes(pythonTuple[0], "getstate");
			flags = Converter.ConvertToInt32(pythonTuple[1]) << 1;
			if (_pendingCR)
			{
				flags |= 1;
			}
		}

		public void setstate(CodeContext context, [NotNull] PythonTuple state)
		{
			object obj = state[0];
			int num = Converter.ConvertToInt32(state[1]);
			_pendingCR = (num & 1) != 0;
			if (_decoder != null)
			{
				PythonOps.Invoke(context, _decoder, "setstate", PythonTuple.MakeTuple(obj, num >> 1));
			}
		}

		internal void SetState(CodeContext context, Bytes buffer, int flags)
		{
			_pendingCR = (flags & 1) != 0;
			if (_decoder != null)
			{
				PythonOps.Invoke(context, _decoder, "setstate", PythonTuple.MakeTuple(buffer, flags >> 1));
			}
		}

		public void reset(CodeContext context)
		{
			_seenNL = LineEnding.None;
			_pendingCR = false;
			if (_decoder != null)
			{
				PythonOps.Invoke(context, _decoder, "reset");
			}
		}
	}

	[PythonType]
	[DynamicBaseType]
	private class BlockingIOError : PythonExceptions._EnvironmentError
	{
		private int _characters_written;

		public int characters_written
		{
			get
			{
				return _characters_written;
			}
			set
			{
				_characters_written = value;
			}
		}

		public BlockingIOError(PythonType cls)
			: base(cls)
		{
		}

		public override void __init__(params object[] args)
		{
			switch (args.Length)
			{
			case 2:
				base.__init__(args);
				return;
			case 3:
				_characters_written = GetInt(args[2], "an integer is required");
				base.__init__(args[0], args[1]);
				return;
			}
			if (args.Length < 2)
			{
				throw PythonOps.TypeError("BlockingIOError() takes at least 2 arguments ({0} given)", args.Length);
			}
			throw PythonOps.TypeError("BlockingIOError() takes at most 3 arguments ({0} given)", args.Length);
		}
	}

	private class _BlockingIOErrorException : IOException
	{
		public _BlockingIOErrorException(string msg)
			: base(msg)
		{
		}
	}

	[Documentation("file(name: str[, mode: str]) -> file IO object\n\nOpen a file.  The mode can be 'r', 'w' or 'a' for reading (default),\nwriting or appending.   The file will be created if it doesn't exist\nwhen opened for writing or appending; it will be truncated when\nopened for writing.  Add a '+' to the mode to allow simultaneous\nreading and writing.")]
	[PythonType]
	[DontMapIDisposableToContextManager]
	public class FileIO : _RawIOBase, IDisposable, IWeakReferenceable, ICodeFormattable, IDynamicMetaObjectProvider
	{
		private static readonly int DEFAULT_BUF_SIZE = 32;

		private Stream _readStream;

		private Stream _writeStream;

		private bool _closed;

		private bool _closefd;

		private string _mode;

		private WeakRefTracker _tracker;

		private PythonContext _context;

		public object name;

		[Documentation("True if the file is closed")]
		public override bool closed => _closed;

		public bool closefd => _closefd;

		[Documentation("String giving the file mode")]
		public string mode => _mode;

		public FileIO(CodeContext context, int fd, [DefaultParameterValue("r")] string mode, [DefaultParameterValue(true)] bool closefd)
			: base(context)
		{
			if (fd < 0)
			{
				throw PythonOps.ValueError("fd must be >= 0");
			}
			PythonContext pythonContext = PythonContext.GetContext(context);
			FileIO fileIO = (FileIO)pythonContext.FileManager.GetObjectFromId(fd);
			name = fileIO.name ?? ((object)fd);
			_context = pythonContext;
			switch (StandardizeMode(mode))
			{
			case "r":
				_mode = "rb";
				break;
			case "w":
				_mode = "wb";
				break;
			case "a":
				_mode = "w";
				break;
			case "r+":
			case "+r":
				_mode = "rb+";
				break;
			case "w+":
			case "+w":
				_mode = "rb+";
				break;
			case "a+":
			case "+a":
				_mode = "r+";
				break;
			default:
				BadMode(mode);
				break;
			}
			_readStream = fileIO._readStream;
			_writeStream = fileIO._writeStream;
			_closefd = closefd;
		}

		public FileIO(CodeContext context, string name, [DefaultParameterValue("r")] string mode, [DefaultParameterValue(true)] bool closefd)
			: base(context)
		{
			if (!closefd)
			{
				throw PythonOps.ValueError("Cannot use closefd=False with file name");
			}
			_closefd = true;
			this.name = name;
			PlatformAdaptationLayer platform = PythonContext.GetContext(context).DomainManager.Platform;
			switch (StandardizeMode(mode))
			{
			case "r":
				_readStream = (_writeStream = OpenFile(context, platform, name, FileMode.Open, FileAccess.Read, FileShare.None));
				_mode = "rb";
				break;
			case "w":
				_readStream = (_writeStream = OpenFile(context, platform, name, FileMode.Create, FileAccess.Write, FileShare.None));
				_mode = "wb";
				break;
			case "a":
				_readStream = (_writeStream = OpenFile(context, platform, name, FileMode.Append, FileAccess.Write, FileShare.None));
				_readStream.Seek(0L, SeekOrigin.End);
				_mode = "w";
				break;
			case "r+":
			case "+r":
				_readStream = (_writeStream = OpenFile(context, platform, name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
				_mode = "rb+";
				break;
			case "w+":
			case "+w":
				_readStream = (_writeStream = OpenFile(context, platform, name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
				_mode = "rb+";
				break;
			case "a+":
			case "+a":
				_readStream = OpenFile(context, platform, name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				_writeStream = OpenFile(context, platform, name, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				_readStream.Seek(0L, SeekOrigin.End);
				_writeStream.Seek(0L, SeekOrigin.End);
				_mode = "r+";
				break;
			default:
				BadMode(mode);
				break;
			}
			_context = PythonContext.GetContext(context);
		}

		private static string StandardizeMode(string mode)
		{
			int num = mode.IndexOf('b');
			if (num == mode.Length - 1)
			{
				mode = mode.Substring(0, num);
			}
			else if (num >= 0)
			{
				StringBuilder stringBuilder = new StringBuilder(mode.Substring(0, num), mode.Length - 1);
				for (int i = num + 1; i < mode.Length; i++)
				{
					if (mode[i] != 'b')
					{
						stringBuilder.Append(mode[i]);
					}
				}
				mode = stringBuilder.ToString();
			}
			return mode;
		}

		private static void BadMode(string mode)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < mode.Length; i++)
			{
				switch (mode[i])
				{
				case 'a':
				case 'r':
				case 'w':
					if (flag)
					{
						throw PythonOps.ValueError("Must have exactly one of read/write/append mode");
					}
					flag = true;
					break;
				case '+':
					if (flag2)
					{
						throw PythonOps.ValueError("Must have exactly one of read/write/append mode");
					}
					flag2 = true;
					break;
				default:
					throw PythonOps.ValueError("invalid mode: {0}", mode);
				case 'b':
					break;
				}
			}
			throw PythonOps.ValueError("Must have exactly one of read/write/append mode");
		}

		[Documentation("close() -> None.  Close the file.\n\nA closed file cannot be used for further I/O operations.  close() may becalled more than once without error.  Changes the fileno to -1.")]
		public override void close(CodeContext context)
		{
			if (!_closed)
			{
				flush(context);
				_closed = true;
				_readStream.Close();
				_readStream.Dispose();
				if (!object.ReferenceEquals(_readStream, _writeStream))
				{
					_writeStream.Close();
					_writeStream.Dispose();
				}
				_context.RawFileManager?.Remove(this);
			}
		}

		[Documentation("fileno() -> int. \"file descriptor\".\n\nThis is needed for lower-level file interfaces, such as the fcntl module.")]
		public override int fileno(CodeContext context)
		{
			_checkClosed();
			return _context.FileManager.GetIdFromObject(this);
		}

		[Documentation("Flush write buffers, if applicable.\n\nThis is not implemented for read-only and non-blocking streams.\n")]
		public override void flush(CodeContext context)
		{
			_checkClosed();
			_writeStream.Flush();
		}

		[Documentation("isatty() -> bool.  True if the file is connected to a tty device.")]
		public override bool isatty(CodeContext context)
		{
			_checkClosed();
			return false;
		}

		[Documentation("read(size: int) -> bytes.  read at most size bytes, returned as bytes.\n\nOnly makes one system call, so less data may be returned than requested\nIn non-blocking mode, returns None if no data is available.\nOn end-of-file, returns ''.")]
		public override object read(CodeContext context, [DefaultParameterValue(null)] object size)
		{
			int num = GetInt(size, -1);
			if (num < 0)
			{
				return readall();
			}
			EnsureReadable();
			byte[] array = new byte[num];
			int newSize = _readStream.Read(array, 0, num);
			Array.Resize(ref array, newSize);
			return Bytes.Make(array);
		}

		[Documentation("readable() -> bool.  True if file was opened in a read mode.")]
		public override bool readable(CodeContext context)
		{
			_checkClosed();
			return _readStream.CanRead;
		}

		[Documentation("readall() -> bytes.  read all data from the file, returned as bytes.\n\nIn non-blocking mode, returns as much as is immediately available,\nor None if no data is available.  On end-of-file, returns ''.")]
		public Bytes readall()
		{
			EnsureReadable();
			int num = DEFAULT_BUF_SIZE;
			byte[] array = new byte[num];
			int num2 = _readStream.Read(array, 0, num);
			while (num2 == num)
			{
				Array.Resize(ref array, num * 2);
				num2 += _readStream.Read(array, num, num);
				num *= 2;
			}
			Array.Resize(ref array, num2);
			return Bytes.Make(array);
		}

		[Documentation("readinto() -> Same as RawIOBase.readinto().")]
		public BigInteger readinto([NotNull] ArrayModule.array buffer)
		{
			EnsureReadable();
			return (int)buffer.FromStream(_readStream, 0, buffer.__len__() * buffer.itemsize);
		}

		public BigInteger readinto([NotNull] ByteArray buffer)
		{
			EnsureReadable();
			for (int i = 0; i < buffer.Count; i++)
			{
				int num = _readStream.ReadByte();
				if (num == -1)
				{
					return i - 1;
				}
				buffer[i] = (byte)num;
			}
			return buffer.Count;
		}

		public BigInteger readinto([NotNull] PythonBuffer buffer)
		{
			EnsureReadable();
			throw PythonOps.TypeError("buffer is read-only");
		}

		public override BigInteger readinto(CodeContext context, object buf)
		{
			ByteArray byteArray = buf as ByteArray;
			if (byteArray != null)
			{
				return readinto(byteArray);
			}
			if (buf is ArrayModule.array)
			{
				return readinto(byteArray);
			}
			EnsureReadable();
			throw PythonOps.TypeError("argument 1 must be read/write buffer, not {0}", DynamicHelpers.GetPythonType(buf).Name);
		}

		[Documentation("seek(offset: int[, whence: int]) -> None.  Move to new file position.\n\nArgument offset is a byte count.  Optional argument whence defaults to\n0 (offset from start of file, offset should be >= 0); other values are 1\n(move relative to current position, positive or negative), and 2 (move\nrelative to end of file, usually negative, although many platforms allow\nseeking beyond the end of a file).\nNote that not all file objects are seekable.")]
		public override BigInteger seek(CodeContext context, BigInteger offset, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			return _readStream.Seek((long)offset, (SeekOrigin)GetInt(whence));
		}

		public BigInteger seek(double offset, [DefaultParameterValue(0)] object whence)
		{
			_checkClosed();
			throw PythonOps.TypeError("an integer is required");
		}

		[Documentation("seekable() -> bool.  True if file supports random-access.")]
		public override bool seekable(CodeContext context)
		{
			_checkClosed();
			return _readStream.CanSeek;
		}

		[Documentation("tell() -> int.  Current file position")]
		public override BigInteger tell(CodeContext context)
		{
			_checkClosed();
			return _readStream.Position;
		}

		public BigInteger truncate(BigInteger size)
		{
			EnsureWritable();
			long position = _readStream.Position;
			_writeStream.SetLength((long)size);
			_readStream.Seek(position, SeekOrigin.Begin);
			return size;
		}

		public BigInteger truncate(double size)
		{
			EnsureWritable();
			throw PythonOps.TypeError("an integer is required");
		}

		[Documentation("truncate([size: int]) -> None.  Truncate the file to at most size bytes.\n\nSize defaults to the current file position, as returned by tell().The current file position is changed to the value of size.")]
		public override BigInteger truncate(CodeContext context, [DefaultParameterValue(null)] object pos)
		{
			if (pos == null)
			{
				return truncate(tell(context));
			}
			if (TryGetBigInt(pos, out var res))
			{
				return truncate(res);
			}
			EnsureWritable();
			throw PythonOps.TypeError("an integer is required");
		}

		[Documentation("writable() -> bool.  True if file was opened in a write mode.")]
		public override bool writable(CodeContext context)
		{
			_checkClosed();
			return _writeStream.CanWrite;
		}

		private BigInteger write([NotNull] byte[] b)
		{
			EnsureWritable();
			_writeStream.Write(b, 0, b.Length);
			SeekToEnd();
			return b.Length;
		}

		private BigInteger write([NotNull] Bytes b)
		{
			return write(b._bytes);
		}

		private BigInteger write([NotNull] ICollection<byte> b)
		{
			EnsureWritable();
			int count = b.Count;
			byte[] array = new byte[count];
			b.CopyTo(array, 0);
			_writeStream.Write(array, 0, count);
			SeekToEnd();
			return count;
		}

		private BigInteger write([NotNull] string s)
		{
			return write(s.MakeByteArray());
		}

		[Documentation("write(b: bytes) -> int.  Write bytes b to file, return number written.\n\nOnly makes one system call, so not all the data may be written.\nThe number of bytes actually written is returned.")]
		public override BigInteger write(CodeContext context, object b)
		{
			if (b is byte[] b2)
			{
				return write(b2);
			}
			if (b is Bytes b3)
			{
				return write(b3);
			}
			if (b is ArrayModule.array array)
			{
				return write(array.ToByteArray());
			}
			if (b is ICollection<byte> b4)
			{
				return write(b4);
			}
			EnsureWritable();
			throw PythonOps.TypeError("expected a readable buffer object");
		}

		public string __repr__(CodeContext context)
		{
			return $"<_io.FileIO name={PythonOps.Repr(context, name)} mode='{_mode}'>";
		}

		void IDisposable.Dispose()
		{
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _tracker;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			_tracker = value;
			return true;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			((IWeakReferenceable)this).SetWeakRef(value);
		}

		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new MetaExpandable<FileIO>(parameter, this);
		}

		private static Stream OpenFile(CodeContext context, PlatformAdaptationLayer pal, string name, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			try
			{
				return pal.OpenInputFileStream(name, fileMode, fileAccess, fileShare);
			}
			catch (UnauthorizedAccessException e)
			{
				throw PythonFile.ToIoException(context, name, e);
			}
			catch (IOException ioe)
			{
				PythonFile.AddFilename(context, name, ioe);
				throw;
			}
		}

		private void EnsureReadable()
		{
			_checkClosed();
			_checkReadable("File not open for reading");
		}

		private void EnsureWritable()
		{
			_checkClosed();
			_checkWritable("File not open for writing");
		}

		private void SeekToEnd()
		{
			if (!object.ReferenceEquals(_readStream, _writeStream))
			{
				_readStream.Seek(_writeStream.Position, SeekOrigin.Begin);
			}
		}
	}

	public const int DEFAULT_BUFFER_SIZE = 8192;

	private static readonly object _blockingIOErrorKey = new object();

	private static readonly object _unsupportedOperationKey = new object();

	private static HashSet<char> _validModes = MakeSet("abrtwU+");

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException(_blockingIOErrorKey, PythonExceptions.IOError, typeof(BlockingIOError), dict, "BlockingIOError", "__builtin__", (string msg) => new _BlockingIOErrorException(msg));
		context.EnsureModuleException(_unsupportedOperationKey, new PythonType[2]
		{
			PythonExceptions.ValueError,
			PythonExceptions.IOError
		}, typeof(PythonExceptions.BaseException), dict, "UnsupportedOperation", "io");
	}

	public static _IOBase open(CodeContext context, object file, [DefaultParameterValue("r")] string mode, [DefaultParameterValue(-1)] int buffering, [DefaultParameterValue(null)] string encoding, [DefaultParameterValue(null)] string errors, [DefaultParameterValue(null)] string newline, [DefaultParameterValue(true)] bool closefd)
	{
		int fd = -1;
		string text = file as string;
		if (text == null)
		{
			if (file is Extensible<string>)
			{
				text = ((Extensible<string>)file).Value;
			}
			else
			{
				fd = GetInt(file, 0);
			}
		}
		HashSet<char> hashSet = MakeSet(mode);
		if (hashSet.Count < mode.Length || !_validModes.IsSupersetOf(hashSet))
		{
			throw PythonOps.ValueError("invalid mode: {0}", mode);
		}
		bool flag = hashSet.Contains('r');
		bool flag2 = hashSet.Contains('w');
		bool flag3 = hashSet.Contains('a');
		bool flag4 = hashSet.Contains('+');
		bool flag5 = hashSet.Contains('t');
		bool flag6 = hashSet.Contains('b');
		if (hashSet.Contains('U'))
		{
			if (flag2 || flag3)
			{
				throw PythonOps.ValueError("can't use U and writing mode at once");
			}
			flag = true;
		}
		if (flag5 && flag6)
		{
			throw PythonOps.ValueError("can't have text and binary mode at once");
		}
		if ((flag && flag2) || (flag && flag3) || (flag2 && flag3))
		{
			throw PythonOps.ValueError("can't have read/write/append mode at once");
		}
		if (!flag && !flag2 && !flag3)
		{
			throw PythonOps.ValueError("must have exactly one of read/write/append mode");
		}
		if (flag6 && encoding != null)
		{
			throw PythonOps.ValueError("binary mode doesn't take an encoding argument");
		}
		if (flag6 && newline != null)
		{
			throw PythonOps.ValueError("binary mode doesn't take a newline argument");
		}
		mode = (flag ? "r" : "");
		if (flag2)
		{
			mode += 'w';
		}
		if (flag3)
		{
			mode += 'a';
		}
		if (flag4)
		{
			mode += '+';
		}
		FileIO fileIO = ((text == null) ? new FileIO(context, fd, mode, closefd) : new FileIO(context, text, mode, closefd));
		bool line_buffering = false;
		if (buffering == 1 || (buffering < 0 && fileIO.isatty(context)))
		{
			buffering = -1;
			line_buffering = true;
		}
		if (buffering < 0)
		{
			buffering = 8192;
		}
		if (buffering == 0)
		{
			if (flag6)
			{
				return fileIO;
			}
			throw PythonOps.ValueError("can't have unbuffered text I/O");
		}
		_BufferedIOBase bufferedIOBase;
		if (flag4)
		{
			bufferedIOBase = BufferedRandom.Create(context, fileIO, buffering, null);
		}
		else if (flag2 || flag3)
		{
			bufferedIOBase = BufferedWriter.Create(context, fileIO, buffering, null);
		}
		else
		{
			if (!flag)
			{
				throw PythonOps.ValueError("unknown mode: {0}", mode);
			}
			bufferedIOBase = BufferedReader.Create(context, fileIO, buffering);
		}
		if (flag6)
		{
			return bufferedIOBase;
		}
		TextIOWrapper textIOWrapper = TextIOWrapper.Create(context, bufferedIOBase, encoding, errors, newline, line_buffering);
		((IPythonExpandable)textIOWrapper).EnsureCustomAttributes()["mode"] = mode;
		return textIOWrapper;
	}

	private static HashSet<char> MakeSet(string chars)
	{
		HashSet<char> hashSet = new HashSet<char>();
		for (int i = 0; i < chars.Length; i++)
		{
			hashSet.Add(chars[i]);
		}
		return hashSet;
	}

	private static BigInteger GetBigInt(object i, string msg)
	{
		if (TryGetBigInt(i, out var res))
		{
			return res;
		}
		throw PythonOps.TypeError(msg);
	}

	private static bool TryGetBigInt(object i, out BigInteger res)
	{
		if (i is BigInteger)
		{
			res = (BigInteger)i;
			return true;
		}
		if (i is int)
		{
			res = (int)i;
			return true;
		}
		if (i is long)
		{
			res = (long)i;
			return true;
		}
		if (i is Extensible<int> extensible)
		{
			res = extensible.Value;
			return true;
		}
		if (i is Extensible<BigInteger> extensible2)
		{
			res = extensible2.Value;
			return true;
		}
		res = BigInteger.Zero;
		return false;
	}

	private static int GetInt(object i)
	{
		return GetInt(i, null, null);
	}

	private static int GetInt(object i, int defaultValue)
	{
		return GetInt(i, defaultValue, null, null);
	}

	private static int GetInt(object i, string msg, params object[] args)
	{
		if (TryGetInt(i, out var value))
		{
			return value;
		}
		if (msg == null)
		{
			throw PythonOps.TypeError("integer argument expected, got '{0}'", PythonTypeOps.GetName(i));
		}
		throw PythonOps.TypeError(msg, args);
	}

	private static int GetInt(object i, int defaultValue, string msg, params object[] args)
	{
		if (i == null)
		{
			return defaultValue;
		}
		return GetInt(i, msg, args);
	}

	private static bool TryGetInt(object i, out int value)
	{
		if (i == null)
		{
			value = int.MinValue;
			return false;
		}
		if (i is int)
		{
			value = (int)i;
			return true;
		}
		if (i is BigInteger)
		{
			return ((BigInteger)i).AsInt32(out value);
		}
		if (i is Extensible<int> extensible)
		{
			value = extensible.Value;
			return true;
		}
		if (i is Extensible<BigInteger> extensible2)
		{
			return extensible2.Value.AsInt32(out value);
		}
		value = int.MinValue;
		return false;
	}

	private static Bytes GetBytes(object o, string name)
	{
		if (o == null)
		{
			return null;
		}
		if (o is Bytes result)
		{
			return result;
		}
		string text = o as string;
		if (text == null && o is Extensible<string> extensible)
		{
			text = extensible.Value;
		}
		if (text != null)
		{
			return PythonOps.MakeBytes(text.MakeByteArray());
		}
		throw PythonOps.TypeError("'" + name + "' should have returned bytes");
	}

	private static IList<byte> GetBytes(object buf)
	{
		if (buf is IList<byte> result)
		{
			return result;
		}
		string text = buf as string;
		if (text == null && buf is Extensible<string>)
		{
			text = ((Extensible<string>)buf).Value;
		}
		if (text != null)
		{
			return text.MakeByteArray();
		}
		if (buf is ArrayModule.array array)
		{
			return array.ToByteArray();
		}
		throw PythonOps.TypeError("must be bytes or buffer, not {0}", PythonTypeOps.GetName(buf));
	}
}
