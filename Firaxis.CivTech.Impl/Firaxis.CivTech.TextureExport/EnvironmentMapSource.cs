using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform;
using Primitives;
using TextureExport;

namespace Firaxis.CivTech.TextureExport;

internal class EnvironmentMapSource : IEnvironmentSource, IDisposable
{
	protected unsafe BufferInfo* m_pBuffer;

	public unsafe virtual bool IsCubeMap
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return *(int*)((ulong)(nint)m_pBuffer + 4uL) == 2;
		}
	}

	public unsafe virtual PixelFormat PixelFormat => ExportSettingsParams.ConvertPixelFormat(*(global::TextureExport.PixelFormat*)m_pBuffer);

	public unsafe virtual uint Height => *(uint*)((ulong)(nint)m_pBuffer + 20uL);

	public unsafe virtual uint Width => *(uint*)((ulong)(nint)m_pBuffer + 16uL);

	public unsafe EnvironmentMapSource(MemoryBuffer* mb, TextureData* td)
	{
		//IL_027a: Expected I, but got I8
		//IL_0265: Expected I4, but got I8
		//IL_0272: Expected I, but got I8
		//IL_02a6: Expected I, but got I8
		switch ((uint)(*(int*)td))
		{
		case 71u:
		case 74u:
		case 77u:
		case 80u:
		case 83u:
		{
			uint num = *(uint*)((ulong)(nint)td + 16uL);
			uint num2 = *(uint*)((ulong)(nint)td + 20uL);
			uint num3 = *(uint*)((ulong)(nint)td + 8uL);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002Eg_TextureAllocator), 0uL);
			try
			{
				uint num4 = global::_003CModule_003E.TextureLib_002EGetMipChainSize((FGX_FORMAT)1001u, num, num2, 1u, num3);
				global::_003CModule_003E.Platform_002EMemoryBuffer_002EGrowToFit(&memoryBuffer, num4, 16uL);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
				global::_003CModule_003E.TextureLib_002EInitTextureData(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer), (TextureType)0, (FGX_FORMAT)1001u, num, num2, 1u, num3);
				global::_003CModule_003E.TextureLib_002EConvertFormat(&textureData, td);
				// IL cpblk instruction
				System.Runtime.CompilerServices.Unsafe.CopyBlock(td, ref textureData, 1088);
				global::_003CModule_003E.Platform_002EMemoryBuffer_002EMove(&memoryBuffer, mb);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
			break;
		}
		default:
			throw new Exception("Cannot import this pixel format!");
		case 2u:
		case 6u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 16u:
		case 24u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 34u:
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 41u:
		case 49u:
		case 50u:
		case 51u:
		case 52u:
		case 54u:
		case 56u:
		case 57u:
		case 58u:
		case 59u:
		case 61u:
		case 62u:
		case 63u:
		case 64u:
		case 65u:
		case 85u:
		case 1001u:
		case 1007u:
		case 1009u:
		case 1010u:
			break;
		}
		int num5 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		BufferInfo* ptr = (BufferInfo*)global::_003CModule_003E.@new(1120uL, num5, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HAGNIMK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 134, 23, 0);
		BufferInfo* ptr2;
		try
		{
			if (ptr != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr, 0, 1120);
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D((MemoryBuffer*)((ulong)(nint)ptr + 1088uL));
				ptr2 = ptr;
			}
			else
			{
				ptr2 = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num5, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HAGNIMK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 134, 23, 0);
			throw;
		}
		m_pBuffer = ptr2;
		global::_003CModule_003E.Platform_002EMemoryBuffer_002EMove(mb, (MemoryBuffer*)((ulong)(nint)ptr2 + 1088uL));
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock(m_pBuffer, td, 1088);
	}

	private unsafe void _007EEnvironmentMapSource()
	{
		//IL_0017: Expected I, but got I8
		BufferInfo* pBuffer = m_pBuffer;
		if (pBuffer != null)
		{
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D((MemoryBuffer*)((ulong)(nint)pBuffer + 1088uL));
			global::_003CModule_003E.delete(pBuffer, 1120uL);
		}
	}

	public unsafe TextureData* GetTextureData()
	{
		return (TextureData*)m_pBuffer;
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EEnvironmentMapSource();
		}
		else
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}
