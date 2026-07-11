using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Platform;
using Primitives;

namespace Firaxis.CivTech.Texture;

public class DDSDataExtractor : IDDSDataExtractor
{
	private readonly TextureDataWrapper m_textureData;

	public DDSDataExtractor()
	{
		TextureDataWrapper textureData = new TextureDataWrapper();
		try
		{
			m_textureData = textureData;
			base._002Ector();
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)m_textureData).Dispose();
			throw;
		}
	}

	private void _007EDDSDataExtractor()
	{
	}

	private void _0021DDSDataExtractor()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool LoadDDSFile(string filePath)
	{
		//IL_00b0: Expected I, but got I8
		//IL_00e6: Expected I, but got I8
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(filePath);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		try
		{
			iOStringWrapper = iOStringWrapper2;
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
			try
			{
				global::_003CModule_003E.Platform_002EMemoryBuffer_002EInit(&memoryBuffer, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETexture_002E_003FA0x59fdfe15_002Eg_DDSAllocator), 0uL);
				if (global::_003CModule_003E.Platform_002EReadCompleteFile(iOStringWrapper.Value, &memoryBuffer) != 0)
				{
					goto end_IL_0015;
				}
				goto end_IL_000a;
				end_IL_0015:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
			goto IL_0054;
			end_IL_000a:;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
		try
		{
			try
			{
				if (!global::_003CModule_003E.TextureLib_002EInitTextureDataFromDDS(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer)))
				{
					goto end_IL_005d;
				}
				goto end_IL_005d_2;
				end_IL_005d:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
			goto IL_008f;
			end_IL_005d_2:;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		try
		{
			try
			{
				MemoryBuffer* ptr = (MemoryBuffer*)global::_003CModule_003E.@new(32uL);
				MemoryBuffer* ptr2;
				try
				{
					ptr2 = ((ptr == null) ? null : global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.delete(ptr, 32uL);
					throw;
				}
				global::_003CModule_003E.Platform_002EMemoryBuffer_002EMove(&memoryBuffer, ptr2);
				TextureData* ptr3 = (TextureData*)global::_003CModule_003E.@new(1088uL);
				TextureData* textureData2;
				if (ptr3 != null)
				{
					// IL cpblk instruction
					System.Runtime.CompilerServices.Unsafe.CopyBlock(ptr3, ref textureData, 1088);
					textureData2 = ptr3;
				}
				else
				{
					textureData2 = null;
				}
				m_textureData.Reset(ptr2, textureData2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return true;
		IL_0054:
		((IDisposable)iOStringWrapper).Dispose();
		return false;
		IL_008f:
		((IDisposable)iOStringWrapper).Dispose();
		return false;
	}

	public unsafe virtual uint GetTextureWidth()
	{
		if (!m_textureData.op_Implicit())
		{
			return uint.MaxValue;
		}
		return *(uint*)((ulong)(nint)m_textureData.GetTextureData() + 16uL);
	}

	public unsafe virtual uint GetTextureHeight()
	{
		if (!m_textureData.op_Implicit())
		{
			return uint.MaxValue;
		}
		return *(uint*)((ulong)(nint)m_textureData.GetTextureData() + 20uL);
	}

	public unsafe virtual uint GetTextureDepth()
	{
		if (!m_textureData.op_Implicit())
		{
			return uint.MaxValue;
		}
		return *(uint*)((ulong)(nint)m_textureData.GetTextureData() + 24uL);
	}

	public unsafe virtual uint GetTextureMipMapCount()
	{
		if (!m_textureData.op_Implicit())
		{
			return uint.MaxValue;
		}
		return (uint)(*(int*)((ulong)(nint)m_textureData.GetTextureData() + 8uL) - 1);
	}

	public unsafe virtual Bitmap CreateThumbnailImage(int width, int height)
	{
		Bitmap bitmap = null;
		if (!m_textureData.op_Implicit())
		{
			return null;
		}
		TextureData* textureData = m_textureData.GetTextureData();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData2);
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock(ref textureData2, textureData, 1088);
		System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 4)) = 0;
		System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 24)) = 1;
		System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 8)) = 1;
		int num = System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 16));
		int num2 = System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 20));
		Bitmap bitmap2 = new Bitmap(System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 16)), System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData2, 20)), PixelFormat.Format32bppRgb);
		Bitmap result;
		try
		{
			bitmap = bitmap2;
			result = null;
			Rectangle rect = new Rectangle(0, 0, num, num2);
			BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
			byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData3);
			global::_003CModule_003E.TextureLib_002EInitTextureData(&textureData3, ptr, (TextureType)0, (FGX_FORMAT)1001u, (uint)num, (uint)num2, 1u, 1u);
			bool flag = global::_003CModule_003E.TextureLib_002EConvertFormat(&textureData3, &textureData2);
			bitmap.UnlockBits(bitmapData);
			if (flag)
			{
				Image.GetThumbnailImageAbort callback = global::_003CModule_003E.Firaxis_002ECivTech_002ETexture_002E_003FA0x59fdfe15_002EThumbnailImageAbortCallback;
				result = (Bitmap)bitmap.GetThumbnailImage(width, height, callback, IntPtr.Zero);
			}
		}
		catch
		{
			//try-fault
			((IDisposable)bitmap).Dispose();
			throw;
		}
		((IDisposable)bitmap).Dispose();
		return result;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				return;
			}
			finally
			{
				((IDisposable)m_textureData).Dispose();
			}
		}
		try
		{
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

	~DDSDataExtractor()
	{
		Dispose(A_0: false);
	}
}
