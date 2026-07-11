using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronPython.Runtime;

namespace IronPython.Modules;

internal sealed class MemoryHolder : CriticalFinalizerObject
{
	private readonly IntPtr _data;

	private readonly bool _ownsData;

	private readonly int _size;

	private PythonDictionary _objects;

	private readonly MemoryHolder _parent;

	public IntPtr UnsafeAddress => _data;

	public int Size => _size;

	public PythonDictionary Objects
	{
		get
		{
			return _objects;
		}
		set
		{
			_objects = value;
		}
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public MemoryHolder(int size)
	{
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
		}
		finally
		{
			_size = size;
			_data = NativeFunctions.Calloc(new IntPtr(size));
			if (_data == IntPtr.Zero)
			{
				GC.SuppressFinalize(this);
				throw new OutOfMemoryException();
			}
			_ownsData = true;
		}
	}

	public MemoryHolder(IntPtr data, int size)
	{
		GC.SuppressFinalize(this);
		_data = data;
		_size = size;
	}

	public MemoryHolder(IntPtr data, int size, MemoryHolder parent)
	{
		GC.SuppressFinalize(this);
		_data = data;
		_parent = parent;
		_objects = parent._objects;
		_size = size;
	}

	internal PythonDictionary EnsureObjects()
	{
		if (_objects == null)
		{
			Interlocked.CompareExchange(ref _objects, new PythonDictionary(), null);
		}
		return _objects;
	}

	internal void AddObject(object key, object value)
	{
		EnsureObjects()[key] = value;
	}

	public byte ReadByte(int offset)
	{
		byte result = Marshal.ReadByte(_data, offset);
		GC.KeepAlive(this);
		return result;
	}

	public short ReadInt16(int offset)
	{
		short result = Marshal.ReadInt16(_data, offset);
		GC.KeepAlive(this);
		return result;
	}

	public int ReadInt32(int offset)
	{
		int result = Marshal.ReadInt32(_data, offset);
		GC.KeepAlive(this);
		return result;
	}

	public long ReadInt64(int offset)
	{
		long result = Marshal.ReadInt64(_data, offset);
		GC.KeepAlive(this);
		return result;
	}

	public IntPtr ReadIntPtr(int offset)
	{
		IntPtr result = Marshal.ReadIntPtr(_data, offset);
		GC.KeepAlive(this);
		return result;
	}

	public MemoryHolder ReadMemoryHolder(int offset)
	{
		IntPtr data = Marshal.ReadIntPtr(_data, offset);
		return new MemoryHolder(data, IntPtr.Size, this);
	}

	internal string ReadAnsiString(int offset)
	{
		try
		{
			return Marshal.PtrToStringAnsi(_data.Add(offset));
		}
		finally
		{
			GC.KeepAlive(this);
		}
	}

	internal string ReadUnicodeString(int offset)
	{
		try
		{
			return Marshal.PtrToStringUni(_data.Add(offset));
		}
		finally
		{
			GC.KeepAlive(this);
		}
	}

	internal string ReadAnsiString(int offset, int length)
	{
		try
		{
			return ReadAnsiString(_data, offset, length);
		}
		finally
		{
			GC.KeepAlive(this);
		}
	}

	internal static string ReadAnsiString(IntPtr addr, int offset, int length)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (checked(offset + length) < int.MaxValue)
		{
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append((char)Marshal.ReadByte(addr, offset + i));
			}
		}
		return stringBuilder.ToString();
	}

	internal static string ReadAnsiString(IntPtr addr, int offset)
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte value;
		while ((value = Marshal.ReadByte(addr, offset++)) != 0)
		{
			stringBuilder.Append((char)value);
		}
		return stringBuilder.ToString();
	}

	internal string ReadUnicodeString(int offset, int length)
	{
		try
		{
			return Marshal.PtrToStringUni(_data.Add(offset), length);
		}
		finally
		{
			GC.KeepAlive(this);
		}
	}

	public void WriteByte(int offset, byte value)
	{
		Marshal.WriteByte(_data, offset, value);
		GC.KeepAlive(this);
	}

	public void WriteInt16(int offset, short value)
	{
		Marshal.WriteInt16(_data, offset, value);
		GC.KeepAlive(this);
	}

	public void WriteInt32(int offset, int value)
	{
		Marshal.WriteInt32(_data, offset, value);
		GC.KeepAlive(this);
	}

	public void WriteInt64(int offset, long value)
	{
		Marshal.WriteInt64(_data, offset, value);
		GC.KeepAlive(this);
	}

	public void WriteIntPtr(int offset, IntPtr value)
	{
		Marshal.WriteIntPtr(_data, offset, value);
		GC.KeepAlive(this);
	}

	public void WriteIntPtr(int offset, MemoryHolder address)
	{
		Marshal.WriteIntPtr(_data, offset, address.UnsafeAddress);
		GC.KeepAlive(this);
		GC.KeepAlive(address);
	}

	public void CopyFrom(IntPtr source, IntPtr size)
	{
		NativeFunctions.CopyMemory(_data, source, size);
		GC.KeepAlive(this);
	}

	internal void WriteUnicodeString(int offset, string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			WriteInt16(checked(offset + i * 2), (short)value[i]);
		}
	}

	internal void WriteAnsiString(int offset, string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			WriteByte(checked(offset + i), (byte)value[i]);
		}
	}

	public MemoryHolder GetSubBlock(int offset)
	{
		return new MemoryHolder(_data.Add(offset), _size - offset, this);
	}

	public void CopyTo(MemoryHolder destAddress, int writeOffset, int size)
	{
		NativeFunctions.CopyMemory(destAddress._data.Add(writeOffset), _data, new IntPtr(size));
		GC.KeepAlive(destAddress);
		GC.KeepAlive(this);
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	~MemoryHolder()
	{
		if (_ownsData)
		{
			Marshal.FreeHGlobal(_data);
		}
	}
}
