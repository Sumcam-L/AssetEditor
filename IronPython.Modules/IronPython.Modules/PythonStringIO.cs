using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonStringIO
{
	[PythonType]
	[PythonHidden]
	public class StringI : IEnumerator<string>, IDisposable, IEnumerator
	{
		private StringStream _sr;

		private string _enumValue;

		public bool closed => _sr == null;

		object IEnumerator.Current => _enumValue;

		string IEnumerator<string>.Current => _enumValue;

		internal StringI(string data)
		{
			_sr = new StringStream(data);
		}

		public void close()
		{
			_sr = null;
		}

		public void flush()
		{
			ThrowIfClosed();
		}

		public string getvalue()
		{
			ThrowIfClosed();
			return _sr.Data;
		}

		public string getvalue(bool usePos)
		{
			return _sr.Prefix;
		}

		public bool isatty()
		{
			ThrowIfClosed();
			return false;
		}

		public object __iter__()
		{
			return this;
		}

		public string next()
		{
			ThrowIfClosed();
			if (_sr.EOF)
			{
				throw PythonOps.StopIteration();
			}
			return readline();
		}

		public string read()
		{
			ThrowIfClosed();
			return _sr.ReadToEnd();
		}

		public string read(int s)
		{
			ThrowIfClosed();
			if (s >= 0)
			{
				return _sr.Read(s);
			}
			return _sr.ReadToEnd();
		}

		public string readline()
		{
			ThrowIfClosed();
			return _sr.ReadLine(-1);
		}

		public string readline(int size)
		{
			ThrowIfClosed();
			return _sr.ReadLine(size);
		}

		public List readlines()
		{
			ThrowIfClosed();
			List list = PythonOps.MakeList();
			while (!_sr.EOF)
			{
				list.AddNoLock(readline());
			}
			return list;
		}

		public List readlines(int size)
		{
			ThrowIfClosed();
			List list = PythonOps.MakeList();
			while (!_sr.EOF)
			{
				string text = readline();
				list.AddNoLock(text);
				if (text.Length >= size)
				{
					break;
				}
				size -= text.Length;
			}
			return list;
		}

		public void reset()
		{
			ThrowIfClosed();
			_sr.Reset();
		}

		public void seek(int position)
		{
			seek(position, 0);
		}

		public void seek(int position, int mode)
		{
			ThrowIfClosed();
			_sr.Seek(position, mode switch
			{
				1 => SeekOrigin.Current, 
				2 => SeekOrigin.End, 
				_ => SeekOrigin.Begin, 
			});
		}

		public int tell()
		{
			ThrowIfClosed();
			return _sr.Position;
		}

		public void truncate()
		{
			ThrowIfClosed();
			_sr.Truncate();
		}

		public void truncate(int size)
		{
			ThrowIfClosed();
			_sr.Truncate(size);
		}

		private void ThrowIfClosed()
		{
			if (closed)
			{
				throw PythonOps.ValueError("I/O operation on closed file");
			}
		}

		bool IEnumerator.MoveNext()
		{
			if (!_sr.EOF)
			{
				_enumValue = readline();
				return true;
			}
			_enumValue = null;
			return false;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		void IDisposable.Dispose()
		{
		}
	}

	[PythonType]
	[PythonHidden]
	[DontMapIEnumerableToContains]
	public class StringO : IEnumerator<string>, IDisposable, IEnumerator
	{
		private StringStream _sr = new StringStream("");

		private int _softspace;

		private string _enumValue;

		public bool closed => _sr == null;

		public int softspace
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

		object IEnumerator.Current => _enumValue;

		string IEnumerator<string>.Current => _enumValue;

		internal StringO()
		{
		}

		public object __iter__()
		{
			return this;
		}

		public void close()
		{
			if (_sr != null)
			{
				_sr = null;
			}
		}

		public void flush()
		{
		}

		public string getvalue()
		{
			ThrowIfClosed();
			return _sr.Data;
		}

		public string getvalue(bool usePos)
		{
			ThrowIfClosed();
			return _sr.Prefix;
		}

		public bool isatty()
		{
			ThrowIfClosed();
			return false;
		}

		public string next()
		{
			ThrowIfClosed();
			if (_sr.EOF)
			{
				throw PythonOps.StopIteration();
			}
			return readline();
		}

		public string read()
		{
			ThrowIfClosed();
			return _sr.ReadToEnd();
		}

		public string read(int i)
		{
			ThrowIfClosed();
			if (i >= 0)
			{
				return _sr.Read(i);
			}
			return _sr.ReadToEnd();
		}

		public string readline()
		{
			ThrowIfClosed();
			return _sr.ReadLine(-1);
		}

		public string readline(int size)
		{
			ThrowIfClosed();
			return _sr.ReadLine(size);
		}

		public List readlines()
		{
			ThrowIfClosed();
			List list = PythonOps.MakeList();
			while (!_sr.EOF)
			{
				list.AddNoLock(readline());
			}
			return list;
		}

		public List readlines(int size)
		{
			ThrowIfClosed();
			List list = PythonOps.MakeList();
			while (!_sr.EOF)
			{
				string text = readline();
				list.AddNoLock(text);
				if (text.Length >= size)
				{
					break;
				}
				size -= text.Length;
			}
			return list;
		}

		public void reset()
		{
			ThrowIfClosed();
			_sr.Reset();
		}

		public void seek(int position)
		{
			seek(position, 0);
		}

		public void seek(int offset, int origin)
		{
			ThrowIfClosed();
			_sr.Seek(offset, origin switch
			{
				1 => SeekOrigin.Current, 
				2 => SeekOrigin.End, 
				_ => SeekOrigin.Begin, 
			});
		}

		public int tell()
		{
			ThrowIfClosed();
			return _sr.Position;
		}

		public void truncate()
		{
			ThrowIfClosed();
			_sr.Truncate();
		}

		public void truncate(int size)
		{
			ThrowIfClosed();
			_sr.Truncate(size);
		}

		public void write(string s)
		{
			if (s == null)
			{
				throw PythonOps.TypeError("write argument must be a string or read-only character buffer, not None");
			}
			ThrowIfClosed();
			_sr.Write(s);
		}

		public void write([NotNull] PythonBuffer buffer)
		{
			_sr.Write(buffer.ToString());
		}

		public void writelines(object o)
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(o);
			while (enumerator.MoveNext())
			{
				if (!(enumerator.Current is string s))
				{
					throw PythonOps.TypeError("string expected");
				}
				write(s);
			}
		}

		private void ThrowIfClosed()
		{
			if (closed)
			{
				throw PythonOps.ValueError("I/O operation on closed file");
			}
		}

		bool IEnumerator.MoveNext()
		{
			if (!_sr.EOF)
			{
				_enumValue = readline();
				return true;
			}
			_enumValue = null;
			return false;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		void IDisposable.Dispose()
		{
		}
	}

	public const string __doc__ = "Provides file like objects for reading and writing to strings.";

	public static PythonType InputType = DynamicHelpers.GetPythonTypeFromType(typeof(StringI));

	public static PythonType OutputType = DynamicHelpers.GetPythonTypeFromType(typeof(StringO));

	public static object StringIO()
	{
		return new StringO();
	}

	public static object StringIO(string data)
	{
		return new StringI(data);
	}
}
