using System;

namespace SharpDX;

public struct DataPointer : IEquatable<DataPointer>
{
	public static readonly DataPointer Zero;

	public IntPtr Pointer;

	public int Size;

	public bool IsEmpty => Equals(Zero);

	public DataPointer(IntPtr pointer, int size)
	{
		Pointer = pointer;
		Size = size;
	}

	public unsafe DataPointer(void* pointer, int size)
	{
		Pointer = (IntPtr)pointer;
		Size = size;
	}

	public bool Equals(DataPointer other)
	{
		if (Pointer.Equals(other.Pointer))
		{
			return Size == other.Size;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is DataPointer)
		{
			return Equals((DataPointer)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Pointer.GetHashCode() * 397) ^ Size;
	}

	public DataStream ToDataStream()
	{
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		return new DataStream(this);
	}

	public DataBuffer ToDataBuffer()
	{
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		return new DataBuffer(this);
	}

	public byte[] ToArray()
	{
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		if (Size < 0)
		{
			throw new InvalidOperationException("Size cannot be < 0");
		}
		byte[] array = new byte[Size];
		Utilities.Read(Pointer, array, 0, Size);
		return array;
	}

	public T[] ToArray<T>() where T : struct
	{
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		T[] array = new T[Size / Utilities.SizeOf<T>()];
		CopyTo(array, 0, array.Length);
		return array;
	}

	public void CopyTo<T>(T[] buffer, int offset, int count) where T : struct
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
		}
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Must be > 0");
		}
		if (count * Utilities.SizeOf<T>() > Size)
		{
			throw new ArgumentOutOfRangeException("buffer", "Total buffer size cannot be larger than size of this data pointer");
		}
		Utilities.Read(Pointer, buffer, offset, count);
	}

	public void CopyFrom<T>(T[] buffer) where T : struct
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		CopyFrom(buffer, 0, buffer.Length);
	}

	public void CopyFrom<T>(T[] buffer, int offset, int count) where T : struct
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (Pointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("DataPointer is Zero");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Must be >= 0");
		}
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Must be > 0");
		}
		if (count * Utilities.SizeOf<T>() > Size)
		{
			throw new ArgumentOutOfRangeException("buffer", "Total buffer size cannot be larger than size of this data pointer");
		}
		Utilities.Write(Pointer, buffer, offset, count);
	}

	public static bool operator ==(DataPointer left, DataPointer right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(DataPointer left, DataPointer right)
	{
		return !left.Equals(right);
	}

	static DataPointer()
	{
		Zero = new DataPointer(IntPtr.Zero, 0);
	}
}
