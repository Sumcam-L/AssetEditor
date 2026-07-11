using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("buffer")]
[DontMapGetMemberNamesToDir]
public sealed class PythonBuffer : ICodeFormattable, IDynamicMetaObjectProvider, IList<byte>, ICollection<byte>, IEnumerable<byte>, IEnumerable
{
	private class BufferMeta : DynamicMetaObject, IComConvertible
	{
		public BufferMeta(Expression expr, BindingRestrictions restrictions, object value)
			: base(expr, restrictions, value)
		{
		}

		DynamicMetaObject IComConvertible.GetComMetaObject()
		{
			return new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("ConvertBufferToByteArray"), Utils.Convert(base.Expression, typeof(PythonBuffer))), BindingRestrictions.Empty);
		}
	}

	internal object _object;

	private int _offset;

	private int _size;

	private readonly CodeContext _context;

	private byte[] _objectByteCache;

	public object this[object s]
	{
		get
		{
			return PythonOps.GetIndex(_context, GetSelectedRange(), s);
		}
		set
		{
			throw ReadOnlyError();
		}
	}

	internal int Size => _size;

	internal byte[] byteCache => _objectByteCache ?? (_objectByteCache = PythonOps.ConvertBufferToByteArray(this));

	byte IList<byte>.this[int index]
	{
		[PythonHidden]
		get
		{
			return byteCache[index];
		}
		[PythonHidden]
		set
		{
			throw ReadOnlyError();
		}
	}

	int ICollection<byte>.Count
	{
		[PythonHidden]
		get
		{
			return byteCache.Length;
		}
	}

	bool ICollection<byte>.IsReadOnly
	{
		[PythonHidden]
		get
		{
			return true;
		}
	}

	public PythonBuffer(CodeContext context, object @object)
		: this(context, @object, 0)
	{
	}

	public PythonBuffer(CodeContext context, object @object, int offset)
		: this(context, @object, offset, -1)
	{
	}

	public PythonBuffer(CodeContext context, object @object, int offset, int size)
	{
		if (!InitBufferObject(@object, offset, size))
		{
			throw PythonOps.TypeError("expected buffer object");
		}
		_context = context;
	}

	private bool InitBufferObject(object o, int offset, int size)
	{
		if (offset < 0)
		{
			throw PythonOps.ValueError("offset must be zero or positive");
		}
		if (size < -1)
		{
			throw PythonOps.ValueError("size must be zero or positive");
		}
		int num;
		if (o is PythonBuffer)
		{
			PythonBuffer pythonBuffer = (PythonBuffer)o;
			o = pythonBuffer._object;
			num = pythonBuffer._size;
		}
		else if (o is string)
		{
			string text = (string)o;
			num = text.Length;
		}
		else if (o is Bytes)
		{
			num = ((Bytes)o).Count;
		}
		else if (o is ByteArray)
		{
			num = ((ByteArray)o).Count;
		}
		else if (o is Array || o is IPythonArray)
		{
			if (o is Array array)
			{
				Type elementType = array.GetType().GetElementType();
				if (!elementType.IsPrimitive && elementType != typeof(string))
				{
					return false;
				}
				num = array.Length;
			}
			else
			{
				IPythonArray pythonArray = (IPythonArray)o;
				num = pythonArray.Count;
			}
		}
		else
		{
			if (!(o is IPythonBufferable))
			{
				return false;
			}
			num = ((IPythonBufferable)o).Size;
			_object = o;
		}
		if (size >= num - offset || size == -1)
		{
			_size = num - offset;
		}
		else
		{
			_size = size;
		}
		_object = o;
		_offset = offset;
		return true;
	}

	public override string ToString()
	{
		object selectedRange = GetSelectedRange();
		if (selectedRange is Bytes)
		{
			return ((Bytes)selectedRange).MakeString();
		}
		if (selectedRange is ByteArray)
		{
			return ((ByteArray)selectedRange).MakeString();
		}
		if (selectedRange is IPythonBufferable)
		{
			return ((IList<byte>)GetSelectedRange()).MakeString();
		}
		if (selectedRange is byte[])
		{
			return ((byte[])GetSelectedRange()).MakeString();
		}
		return selectedRange.ToString();
	}

	public int __cmp__([NotNull] PythonBuffer other)
	{
		if (object.ReferenceEquals(this, other))
		{
			return 0;
		}
		return PythonOps.Compare(ToString(), other.ToString());
	}

	[PythonHidden]
	public override bool Equals(object obj)
	{
		if (!(obj is PythonBuffer other))
		{
			return false;
		}
		return __cmp__(other) == 0;
	}

	public override int GetHashCode()
	{
		return _object.GetHashCode() ^ _offset ^ ((_size << 16) | (_size >> 16));
	}

	private Slice GetSlice()
	{
		object stop = null;
		if (_size >= 0)
		{
			stop = _offset + _size;
		}
		return new Slice(_offset, stop);
	}

	public object __getslice__(object start, object stop)
	{
		return this[new Slice(start, stop)];
	}

	private static Exception ReadOnlyError()
	{
		return PythonOps.TypeError("buffer is read-only");
	}

	public object __setslice__(object start, object stop, object value)
	{
		throw ReadOnlyError();
	}

	public void __delitem__(int index)
	{
		throw ReadOnlyError();
	}

	public void __delslice__(object start, object stop)
	{
		throw ReadOnlyError();
	}

	private object GetSelectedRange()
	{
		if (_object is IPythonArray pythonArray)
		{
			return pythonArray.tostring();
		}
		if (_object is ByteArray byteArray)
		{
			return new Bytes((IList<byte>)byteArray[GetSlice()]);
		}
		if (_object is IPythonBufferable pythonBufferable)
		{
			return new Bytes(pythonBufferable.GetBytes(_offset, _size));
		}
		return PythonOps.GetIndex(_context, _object, GetSlice());
	}

	public static object operator +(PythonBuffer a, PythonBuffer b)
	{
		PythonContext context = PythonContext.GetContext(a._context);
		return context.Operation(PythonOperationKind.Add, PythonOps.GetIndex(a._context, a._object, a.GetSlice()), PythonOps.GetIndex(a._context, b._object, b.GetSlice()));
	}

	public static object operator +(PythonBuffer a, string b)
	{
		return a.ToString() + b;
	}

	public static object operator *(PythonBuffer b, int n)
	{
		PythonContext context = PythonContext.GetContext(b._context);
		return context.Operation(PythonOperationKind.Multiply, PythonOps.GetIndex(b._context, b._object, b.GetSlice()), n);
	}

	public static object operator *(int n, PythonBuffer b)
	{
		PythonContext context = PythonContext.GetContext(b._context);
		return context.Operation(PythonOperationKind.Multiply, PythonOps.GetIndex(b._context, b._object, b.GetSlice()), n);
	}

	public int __len__()
	{
		return Math.Max(_size, 0);
	}

	public string __repr__(CodeContext context)
	{
		return $"<read-only buffer for 0x{PythonOps.Id(_object):X16}, size {_size}, offset {_offset} at 0x{PythonOps.Id(this):X16}>";
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new BufferMeta(parameter, BindingRestrictions.Empty, this);
	}

	[PythonHidden]
	int IList<byte>.IndexOf(byte item)
	{
		for (int i = 0; i < byteCache.Length; i++)
		{
			if (byteCache[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	[PythonHidden]
	void IList<byte>.Insert(int index, byte item)
	{
		throw ReadOnlyError();
	}

	[PythonHidden]
	void IList<byte>.RemoveAt(int index)
	{
		throw ReadOnlyError();
	}

	[PythonHidden]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return byteCache.GetEnumerator();
	}

	[PythonHidden]
	IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
	{
		return ((IEnumerable<byte>)byteCache).GetEnumerator();
	}

	[PythonHidden]
	void ICollection<byte>.Add(byte item)
	{
		throw ReadOnlyError();
	}

	[PythonHidden]
	void ICollection<byte>.Clear()
	{
		throw ReadOnlyError();
	}

	[PythonHidden]
	bool ICollection<byte>.Contains(byte item)
	{
		return ((IList<byte>)this).IndexOf(item) != -1;
	}

	[PythonHidden]
	void ICollection<byte>.CopyTo(byte[] array, int arrayIndex)
	{
		byteCache.CopyTo(array, arrayIndex);
	}

	[PythonHidden]
	bool ICollection<byte>.Remove(byte item)
	{
		throw ReadOnlyError();
	}
}
