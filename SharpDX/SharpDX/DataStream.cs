using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX;

public class DataStream : Stream
{
	private unsafe byte* _buffer;

	private readonly bool _canRead;

	private readonly bool _canWrite;

	private GCHandle _gCHandle;

	private Blob _blob;

	private readonly bool _ownsBuffer;

	private long _position;

	private readonly long _size;

	public override bool CanRead => _canRead;

	public override bool CanSeek => true;

	public override bool CanWrite => _canWrite;

	public unsafe IntPtr DataPointer => new IntPtr(_buffer);

	public override long Length => _size;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public unsafe IntPtr PositionPointer => (IntPtr)(_buffer + _position);

	public long RemainingLength => _size - _position;

	public unsafe DataStream(Blob buffer)
	{
		_buffer = (byte*)(void*)buffer.GetBufferPointer();
		_size = buffer.GetBufferSize();
		_canRead = true;
		_canWrite = true;
		_blob = buffer;
	}

	public unsafe static DataStream Create<T>(T[] userBuffer, bool canRead, bool canWrite, int index = 0, bool pinBuffer = true) where T : struct
	{
		if (userBuffer == null)
		{
			throw new ArgumentNullException("userBuffer");
		}
		if (index < 0 || index > userBuffer.Length)
		{
			throw new ArgumentException("Index is out of range [0, userBuffer.Length-1]", "index");
		}
		int num = Utilities.SizeOf(userBuffer);
		if (pinBuffer)
		{
			GCHandle handle = GCHandle.Alloc(userBuffer, GCHandleType.Pinned);
			int num2 = index * Utilities.SizeOf<T>();
			return new DataStream(num2 + (byte*)(void*)handle.AddrOfPinnedObject(), num - num2, canRead, canWrite, handle);
		}
		fixed (T* buffer = &userBuffer[0])
		{
			return new DataStream(buffer, num, canRead, canWrite, makeCopy: true);
		}
	}

	public unsafe DataStream(int sizeInBytes, bool canRead, bool canWrite)
	{
		_buffer = (byte*)(void*)Utilities.AllocateMemory(sizeInBytes);
		_size = sizeInBytes;
		_ownsBuffer = true;
		_canRead = canRead;
		_canWrite = canWrite;
	}

	public DataStream(DataPointer dataPointer)
		: this(dataPointer.Pointer, dataPointer.Size, canRead: true, canWrite: true)
	{
	}

	public unsafe DataStream(IntPtr userBuffer, long sizeInBytes, bool canRead, bool canWrite)
	{
		_buffer = (byte*)userBuffer.ToPointer();
		_size = sizeInBytes;
		_canRead = canRead;
		_canWrite = canWrite;
	}

	internal unsafe DataStream(void* dataPointer, int sizeInBytes, bool canRead, bool canWrite, GCHandle handle)
	{
		_gCHandle = handle;
		_buffer = (byte*)dataPointer;
		_size = sizeInBytes;
		_canRead = canRead;
		_canWrite = canWrite;
		_ownsBuffer = false;
	}

	internal unsafe DataStream(void* buffer, int sizeInBytes, bool canRead, bool canWrite, bool makeCopy)
	{
		if (makeCopy)
		{
			_buffer = (byte*)(void*)Utilities.AllocateMemory(sizeInBytes);
			Utilities.CopyMemory((IntPtr)_buffer, (IntPtr)buffer, sizeInBytes);
		}
		else
		{
			_buffer = (byte*)buffer;
		}
		_size = sizeInBytes;
		_canRead = canRead;
		_canWrite = canWrite;
		_ownsBuffer = makeCopy;
	}

	protected unsafe override void Dispose(bool disposing)
	{
		if (disposing && _blob != null)
		{
			_blob.Dispose();
			_blob = null;
		}
		if (_gCHandle.IsAllocated)
		{
			_gCHandle.Free();
		}
		if (_ownsBuffer && _buffer != null)
		{
			Utilities.FreeMemory((IntPtr)_buffer);
			_buffer = null;
		}
	}

	public override void Flush()
	{
		throw new NotSupportedException("DataStream objects cannot be flushed.");
	}

	public unsafe T Read<T>() where T : struct
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		byte* ptr = _buffer + _position;
		T data = default(T);
		_position = (byte*)(void*)Utilities.ReadAndPosition((IntPtr)ptr, ref data) - _buffer;
		return data;
	}

	public unsafe float ReadFloat()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		float result = *(float*)(_buffer + _position);
		_position += 4L;
		return result;
	}

	public unsafe int ReadInt()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		int result = *(int*)(_buffer + _position);
		_position += 4L;
		return result;
	}

	public unsafe short ReadShort()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		short result = *(short*)(_buffer + _position);
		_position += 2L;
		return result;
	}

	public unsafe bool ReadBoolean()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		bool result = *(int*)(_buffer + _position) != 0;
		_position += 4L;
		return result;
	}

	public unsafe Vector2 ReadVector2()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Vector2 result = *(Vector2*)(_buffer + _position);
		_position += 8L;
		return result;
	}

	public unsafe Vector3 ReadVector3()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Vector3 result = *(Vector3*)(_buffer + _position);
		_position += 12L;
		return result;
	}

	public unsafe Vector4 ReadVector4()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Vector4 result = *(Vector4*)(_buffer + _position);
		_position += 16L;
		return result;
	}

	public unsafe Color3 ReadColor3()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Color3 result = *(Color3*)(_buffer + _position);
		_position += 12L;
		return result;
	}

	public unsafe Color4 ReadColor4()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Color4 result = *(Color4*)(_buffer + _position);
		_position += 16L;
		return result;
	}

	public unsafe Half ReadHalf()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Half result = *(Half*)(_buffer + _position);
		_position += 2L;
		return result;
	}

	public unsafe Half2 ReadHalf2()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Half2 result = *(Half2*)(_buffer + _position);
		_position += 4L;
		return result;
	}

	public unsafe Half3 ReadHalf3()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Half3 result = *(Half3*)(_buffer + _position);
		_position += 6L;
		return result;
	}

	public unsafe Half4 ReadHalf4()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Half4 result = *(Half4*)(_buffer + _position);
		_position += 8L;
		return result;
	}

	public unsafe Matrix ReadMatrix()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Matrix result = *(Matrix*)(_buffer + _position);
		_position += 64L;
		return result;
	}

	public unsafe Quaternion ReadQuaternion()
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		Quaternion result = *(Quaternion*)(_buffer + _position);
		_position += 16L;
		return result;
	}

	public unsafe override int ReadByte()
	{
		if (_position >= Length)
		{
			return -1;
		}
		return _buffer[_position++];
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int count2 = (int)Math.Min(RemainingLength, count);
		return ReadRange(buffer, offset, count2);
	}

	public unsafe void Read(IntPtr buffer, int offset, int count)
	{
		Utilities.CopyMemory(new IntPtr((byte*)(void*)buffer + offset), (IntPtr)(_buffer + _position), count);
		_position += count;
	}

	public unsafe T[] ReadRange<T>(int count) where T : struct
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		byte* ptr = _buffer + _position;
		T[] array = new T[count];
		_position = (byte*)(void*)Utilities.Read((IntPtr)ptr, array, 0, count) - _buffer;
		return array;
	}

	public unsafe int ReadRange<T>(T[] buffer, int offset, int count) where T : struct
	{
		if (!_canRead)
		{
			throw new NotSupportedException();
		}
		long position = _position;
		_position = (byte*)(void*)Utilities.Read((IntPtr)(_buffer + _position), buffer, offset, count) - _buffer;
		return (int)(_position - position);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long num = 0L;
		switch (origin)
		{
		case SeekOrigin.Begin:
			num = offset;
			break;
		case SeekOrigin.Current:
			num = _position + offset;
			break;
		case SeekOrigin.End:
			num = _size - offset;
			break;
		}
		if (num < 0)
		{
			throw new InvalidOperationException("Cannot seek beyond the beginning of the stream.");
		}
		if (num > _size)
		{
			throw new InvalidOperationException("Cannot seek beyond the end of the stream.");
		}
		_position = num;
		return _position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException("DataStream objects cannot be resized.");
	}

	public unsafe void Write<T>(T value) where T : struct
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		_position = (byte*)(void*)Utilities.WriteAndPosition((IntPtr)(_buffer + _position), ref value) - _buffer;
	}

	public unsafe void Write(float value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(float*)(_buffer + _position) = value;
		_position += 4L;
	}

	public unsafe void Write(int value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(int*)(_buffer + _position) = value;
		_position += 4L;
	}

	public unsafe void Write(short value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(short*)(_buffer + _position) = value;
		_position += 2L;
	}

	public unsafe void Write(bool value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(int*)(_buffer + _position) = (value ? 1 : 0);
		_position += 4L;
	}

	public unsafe void Write(Vector2 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Vector2*)(_buffer + _position) = value;
		_position += 8L;
	}

	public unsafe void Write(Vector3 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Vector3*)(_buffer + _position) = value;
		_position += 12L;
	}

	public unsafe void Write(Vector4 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Vector4*)(_buffer + _position) = value;
		_position += 16L;
	}

	public unsafe void Write(Color3 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Color3*)(_buffer + _position) = value;
		_position += 12L;
	}

	public unsafe void Write(Color4 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Color4*)(_buffer + _position) = value;
		_position += 16L;
	}

	public unsafe void Write(Half value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Half*)(_buffer + _position) = value;
		_position += 2L;
	}

	public unsafe void Write(Half2 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Half2*)(_buffer + _position) = value;
		_position += 4L;
	}

	public unsafe void Write(Half3 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Half3*)(_buffer + _position) = value;
		_position += 6L;
	}

	public unsafe void Write(Half4 value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Half4*)(_buffer + _position) = value;
		_position += 8L;
	}

	public unsafe void Write(Matrix value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Matrix*)(_buffer + _position) = value;
		_position += 64L;
	}

	public unsafe void Write(Quaternion value)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		*(Quaternion*)(_buffer + _position) = value;
		_position += 16L;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		WriteRange(buffer, offset, count);
	}

	public unsafe void Write(IntPtr buffer, int offset, int count)
	{
		Utilities.CopyMemory((IntPtr)(_buffer + _position), new IntPtr((byte*)(void*)buffer + offset), count);
		_position += count;
	}

	public void WriteRange<T>(T[] data) where T : struct
	{
		WriteRange(data, 0, data.Length);
	}

	public unsafe void WriteRange(IntPtr source, long count)
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		Utilities.CopyMemory((IntPtr)(_buffer + _position), source, (int)count);
		_position += count;
	}

	public unsafe void WriteRange<T>(T[] data, int offset, int count) where T : struct
	{
		if (!_canWrite)
		{
			throw new NotSupportedException();
		}
		_position = (byte*)(void*)Utilities.Write((IntPtr)(_buffer + _position), data, offset, count) - _buffer;
	}

	public static implicit operator DataPointer(DataStream from)
	{
		return new DataPointer(from.PositionPointer, (int)from.RemainingLength);
	}
}
