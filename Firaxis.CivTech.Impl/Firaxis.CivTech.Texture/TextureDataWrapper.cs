using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Platform;
using Primitives;

namespace Firaxis.CivTech.Texture;

internal class TextureDataWrapper : IDisposable
{
	private unsafe MemoryBuffer* m_memoryBuffer = null;

	private unsafe TextureData* m_textureData = null;

	private unsafe void _007ETextureDataWrapper()
	{
		//IL_000a: Expected I, but got I8
		//IL_000a: Expected I, but got I8
		Reset(null, null);
	}

	private unsafe void _0021TextureDataWrapper()
	{
		//IL_000a: Expected I, but got I8
		//IL_000a: Expected I, but got I8
		Reset(null, null);
	}

	public unsafe void Reset(MemoryBuffer* memoryBuffer, TextureData* textureData)
	{
		TextureData* textureData2 = m_textureData;
		if (textureData2 != null)
		{
			global::_003CModule_003E.delete(textureData2, 1088uL);
		}
		m_textureData = textureData;
		MemoryBuffer* memoryBuffer2 = m_memoryBuffer;
		if (memoryBuffer2 != null)
		{
			MemoryBuffer* ptr = memoryBuffer2;
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(ptr);
			global::_003CModule_003E.delete(ptr, 32uL);
		}
		m_memoryBuffer = memoryBuffer;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe implicit operator bool()
	{
		return (byte)(((long)(nint)m_textureData != 0) ? 1u : 0u) != 0;
	}

	public unsafe TextureData* GetTextureData()
	{
		return m_textureData;
	}

	[HandleProcessCorruptedStateExceptions]
	protected unsafe virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		//IL_000d: Expected I, but got I8
		//IL_000d: Expected I, but got I8
		//IL_001a: Expected I, but got I8
		//IL_001a: Expected I, but got I8
		if (A_0)
		{
			Reset(null, null);
			return;
		}
		try
		{
			Reset(null, null);
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~TextureDataWrapper()
	{
		Dispose(A_0: false);
	}
}
