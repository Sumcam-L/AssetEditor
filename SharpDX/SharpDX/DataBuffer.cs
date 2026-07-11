using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Direct3D;

namespace SharpDX;

public class DataBuffer : Component
{
	private unsafe sbyte* _buffer;

	private GCHandle _gCHandle;

	private Blob _blob;

	private readonly bool _ownsBuffer;

	private readonly int _size;

	public unsafe IntPtr DataPointer => new IntPtr(_buffer);

	public int Size => _size;

	public unsafe static DataBuffer Create<T>(T[] userBuffer, int index = 0, bool pinBuffer = true) where T : struct
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
			return new DataBuffer(num2 + (byte*)(void*)handle.AddrOfPinnedObject(), num - num2, handle);
		}
		fixed (T* buffer = &userBuffer[0])
		{
			return new DataBuffer(buffer, num, makeCopy: true);
		}
	}

	public unsafe DataBuffer(int sizeInBytes)
	{
		_buffer = (sbyte*)(void*)Utilities.AllocateMemory(sizeInBytes);
		_size = sizeInBytes;
		_ownsBuffer = true;
	}

	public DataBuffer(DataPointer dataPointer)
		: this(dataPointer.Pointer, dataPointer.Size)
	{
	}

	public unsafe DataBuffer(IntPtr userBuffer, int sizeInBytes)
		: this((void*)userBuffer, sizeInBytes, makeCopy: false)
	{
	}

	internal unsafe DataBuffer(void* buffer, int sizeInBytes, GCHandle handle)
	{
		_buffer = (sbyte*)buffer;
		_size = sizeInBytes;
		_gCHandle = handle;
		_ownsBuffer = false;
	}

	internal unsafe DataBuffer(void* buffer, int sizeInBytes, bool makeCopy)
	{
		if (makeCopy)
		{
			_buffer = (sbyte*)(void*)Utilities.AllocateMemory(sizeInBytes);
			Utilities.CopyMemory((IntPtr)_buffer, (IntPtr)buffer, sizeInBytes);
		}
		else
		{
			_buffer = (sbyte*)buffer;
		}
		_size = sizeInBytes;
		_ownsBuffer = makeCopy;
	}

	internal unsafe DataBuffer(Blob buffer)
	{
		_buffer = (sbyte*)(void*)buffer.GetBufferPointer();
		_size = buffer.GetBufferSize();
		_blob = buffer;
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
		base.Dispose(disposing);
	}

	public unsafe void Clear(byte value = 0)
	{
		Utilities.ClearMemory((IntPtr)_buffer, value, Size);
	}

	public unsafe T Get<T>(int positionInBytes) where T : struct
	{
		T data = default(T);
		Utilities.Read((IntPtr)(_buffer + positionInBytes), ref data);
		return data;
	}

	public unsafe void Get<T>(int positionInBytes, out T value) where T : struct
	{
		Utilities.ReadOut<T>((IntPtr)(_buffer + positionInBytes), out value);
	}

	public unsafe float GetFloat(int positionInBytes)
	{
		return *(float*)(_buffer + positionInBytes);
	}

	public unsafe int GetInt(int positionInBytes)
	{
		return *(int*)(_buffer + positionInBytes);
	}

	public unsafe short GetShort(int positionInBytes)
	{
		return *(short*)(_buffer + positionInBytes);
	}

	public unsafe bool GetBoolean(int positionInBytes)
	{
		return *(int*)(_buffer + positionInBytes) != 0;
	}

	public unsafe Vector2 GetVector2(int positionInBytes)
	{
		return *(Vector2*)(_buffer + positionInBytes);
	}

	public unsafe Vector3 GetVector3(int positionInBytes)
	{
		return *(Vector3*)(_buffer + positionInBytes);
	}

	public unsafe Vector4 GetVector4(int positionInBytes)
	{
		return *(Vector4*)(_buffer + positionInBytes);
	}

	public unsafe Color3 GetColor3(int positionInBytes)
	{
		return *(Color3*)(_buffer + positionInBytes);
	}

	public unsafe Color4 GetColor4(int positionInBytes)
	{
		return *(Color4*)(_buffer + positionInBytes);
	}

	public unsafe Half GetHalf(int positionInBytes)
	{
		return *(Half*)(_buffer + positionInBytes);
	}

	public unsafe Half2 GetHalf2(int positionInBytes)
	{
		return *(Half2*)(_buffer + positionInBytes);
	}

	public unsafe Half3 GetHalf3(int positionInBytes)
	{
		return *(Half3*)(_buffer + positionInBytes);
	}

	public unsafe Half4 GetHalf4(int positionInBytes)
	{
		return *(Half4*)(_buffer + positionInBytes);
	}

	public unsafe Matrix GetMatrix(int positionInBytes)
	{
		return *(Matrix*)(_buffer + positionInBytes);
	}

	public unsafe Quaternion GetQuaternion(int positionInBytes)
	{
		return *(Quaternion*)(_buffer + positionInBytes);
	}

	public unsafe T[] GetRange<T>(int positionInBytes, int count) where T : struct
	{
		T[] array = new T[count];
		Utilities.Read((IntPtr)(_buffer + positionInBytes), array, 0, count);
		return array;
	}

	public unsafe void GetRange<T>(int positionInBytes, T[] buffer, int offset, int count) where T : struct
	{
		Utilities.Read((IntPtr)(_buffer + positionInBytes), buffer, offset, count);
	}

	public unsafe void Set<T>(int positionInBytes, ref T value) where T : struct
	{
		System.Runtime.CompilerServices.Unsafe.Write(_buffer + positionInBytes, value);
	}

	public unsafe void Set<T>(int positionInBytes, T value) where T : struct
	{
		System.Runtime.CompilerServices.Unsafe.Write(_buffer + positionInBytes, value);
	}

	public unsafe void Set(int positionInBytes, float value)
	{
		*(float*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, int value)
	{
		*(int*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, short value)
	{
		*(short*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, bool value)
	{
		*(int*)(_buffer + positionInBytes) = (value ? 1 : 0);
	}

	public unsafe void Set(int positionInBytes, Vector2 value)
	{
		*(Vector2*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Vector3 value)
	{
		*(Vector3*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Vector4 value)
	{
		*(Vector4*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Color3 value)
	{
		*(Color3*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Color4 value)
	{
		*(Color4*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Half value)
	{
		*(Half*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Half2 value)
	{
		*(Half2*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Half3 value)
	{
		*(Half3*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Half4 value)
	{
		*(Half4*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Matrix value)
	{
		*(Matrix*)(_buffer + positionInBytes) = value;
	}

	public unsafe void Set(int positionInBytes, Quaternion value)
	{
		*(Quaternion*)(_buffer + positionInBytes) = value;
	}

	public void Set<T>(int positionInBytes, T[] data) where T : struct
	{
		Set(positionInBytes, data, 0, data.Length);
	}

	public unsafe void Set(int positionInBytes, IntPtr source, long count)
	{
		Utilities.CopyMemory((IntPtr)(_buffer + positionInBytes), source, (int)count);
	}

	public unsafe void Set<T>(int positionInBytes, T[] data, int offset, int count) where T : struct
	{
		Utilities.Write((IntPtr)(_buffer + positionInBytes), data, offset, count);
	}

	public static implicit operator DataPointer(DataBuffer from)
	{
		return new DataPointer(from.DataPointer, from.Size);
	}
}
