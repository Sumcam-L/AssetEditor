using System;
using System.Collections.Generic;
using System.Numerics;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("memoryview")]
public sealed class MemoryView : ICodeFormattable
{
	public const object __hash__ = null;

	private readonly IBufferProtocol _buffer;

	private readonly int _start;

	private readonly int? _end;

	public string format => _buffer.Format;

	public BigInteger itemsize => _buffer.ItemSize;

	public BigInteger ndim => _buffer.NumberDimensions;

	public bool @readonly => _buffer.ReadOnly;

	public PythonTuple shape
	{
		get
		{
			IList<BigInteger> list = _buffer.GetShape(_start, _end);
			if (list == null)
			{
				return null;
			}
			return new PythonTuple(list);
		}
	}

	public PythonTuple strides => _buffer.Strides;

	public object suboffsets => _buffer.SubOffsets;

	public object this[int index]
	{
		get
		{
			index = ValidateIndex(index);
			return _buffer.GetItem(index + _start);
		}
		set
		{
			if (_buffer.ReadOnly)
			{
				throw PythonOps.TypeError("cannot modify read-only memory");
			}
			ValidateIndex(index);
			_buffer.SetItem(index + _start, value);
		}
	}

	public object this[[NotNull] Slice slice]
	{
		get
		{
			if (slice.step != null)
			{
				throw PythonOps.NotImplementedError("");
			}
			return new MemoryView(_buffer, (slice.start == null) ? _start : (Converter.ConvertToInt32(slice.start) + _start), (slice.stop == null) ? _end : new int?(Converter.ConvertToInt32(slice.stop) + _start));
		}
		set
		{
			if (_start != 0 || _end.HasValue)
			{
				slice = new Slice((slice.start == null) ? _start : (Converter.ConvertToInt32(slice.start) + _start), (slice.stop == null) ? _end : new int?(Converter.ConvertToInt32(slice.stop) + _start));
			}
			int num = PythonOps.Length(value);
			slice.indices(PythonOps.Length(_buffer), out var ostart, out var ostop, out var _);
			if (ostop - ostart != num)
			{
				throw PythonOps.ValueError("cannot resize memory view");
			}
			_buffer.SetSlice(slice, value);
		}
	}

	public MemoryView(IBufferProtocol @object)
	{
		_buffer = @object;
	}

	private MemoryView(IBufferProtocol @object, int start, int? end)
	{
		_buffer = @object;
		_start = start;
		_end = end;
	}

	public int __len__()
	{
		if (_end.HasValue)
		{
			return _end.Value - _start;
		}
		return _buffer.ItemCount;
	}

	public Bytes tobytes()
	{
		return _buffer.ToBytes(_start, _end);
	}

	public List tolist()
	{
		return _buffer.ToList(_start, _end);
	}

	private int ValidateIndex(int index)
	{
		if (_end.HasValue && index + _start >= _end)
		{
			throw PythonOps.IndexError("index out of range ", index);
		}
		if (index < 0)
		{
			int num = __len__();
			if (index * -1 > num)
			{
				throw PythonOps.IndexError("index out of range ", index);
			}
			index = num + index;
		}
		return index;
	}

	public void __delitem__(int index)
	{
		throw new NotImplementedException();
	}

	public void __delitem__(Slice slice)
	{
		throw new NotImplementedException();
	}

	public static bool operator >(MemoryView self, IBufferProtocol other)
	{
		return self > new MemoryView(other);
	}

	public static bool operator >(IBufferProtocol self, MemoryView other)
	{
		return new MemoryView(self) > other;
	}

	public static bool operator >(MemoryView self, MemoryView other)
	{
		if ((object)self == null)
		{
			return (object)other != null;
		}
		if ((object)other == null)
		{
			return true;
		}
		return self.tobytes() > other.tobytes();
	}

	public static bool operator <(MemoryView self, MemoryView other)
	{
		if ((object)self == null)
		{
			return (object)other == null;
		}
		if ((object)other == null)
		{
			return false;
		}
		return self.tobytes() < other.tobytes();
	}

	public static bool operator <(MemoryView self, IBufferProtocol other)
	{
		return self < new MemoryView(other);
	}

	public static bool operator <(IBufferProtocol self, MemoryView other)
	{
		return new MemoryView(self) < other;
	}

	public static bool operator >=(MemoryView self, MemoryView other)
	{
		if ((object)self == null)
		{
			return (object)other == null;
		}
		if ((object)other == null)
		{
			return false;
		}
		return self.tobytes() >= other.tobytes();
	}

	public static bool operator >=(MemoryView self, IBufferProtocol other)
	{
		return self >= new MemoryView(other);
	}

	public static bool operator >=(IBufferProtocol self, MemoryView other)
	{
		return new MemoryView(self) >= other;
	}

	public static bool operator <=(MemoryView self, MemoryView other)
	{
		if ((object)self == null)
		{
			return (object)other != null;
		}
		if ((object)other == null)
		{
			return true;
		}
		return self.tobytes() <= other.tobytes();
	}

	public static bool operator <=(MemoryView self, IBufferProtocol other)
	{
		return self <= new MemoryView(other);
	}

	public static bool operator <=(IBufferProtocol self, MemoryView other)
	{
		return new MemoryView(self) <= other;
	}

	public static bool operator ==(MemoryView self, MemoryView other)
	{
		if ((object)self == null)
		{
			return (object)other == null;
		}
		if ((object)other == null)
		{
			return false;
		}
		return self.tobytes().Equals(other.tobytes());
	}

	public static bool operator ==(MemoryView self, IBufferProtocol other)
	{
		return self == new MemoryView(other);
	}

	public static bool operator ==(IBufferProtocol self, MemoryView other)
	{
		return new MemoryView(self) == other;
	}

	public static bool operator !=(MemoryView self, MemoryView other)
	{
		if ((object)self == null)
		{
			return (object)other != null;
		}
		if ((object)other == null)
		{
			return true;
		}
		return !self.tobytes().Equals(other.tobytes());
	}

	public static bool operator !=(MemoryView self, IBufferProtocol other)
	{
		return self != new MemoryView(other);
	}

	public static bool operator !=(IBufferProtocol self, MemoryView other)
	{
		return new MemoryView(self) != other;
	}

	public override bool Equals(object obj)
	{
		if (obj is MemoryView memoryView)
		{
			return this == memoryView;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public string __repr__(CodeContext context)
	{
		return $"<memory at {PythonOps.Id(this)}>";
	}
}
