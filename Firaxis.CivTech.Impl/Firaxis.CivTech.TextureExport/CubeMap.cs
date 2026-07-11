using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using Firaxis.CivTech.AssetObjects;
using Platform;
using Primitives;
using TextureLib;

namespace Firaxis.CivTech.TextureExport;

internal class CubeMap : EnvironmentMapSource, ICubeMap
{
	public unsafe CubeMap(MemoryBuffer* mb, TextureData* td)
		: base(mb, td)
	{
		//IL_00e3: Expected I, but got I8
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
			if (*(int*)m_pBuffer != 2)
			{
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002Eg_TextureAllocator), 0uL);
				try
				{
					BufferInfo* pBuffer = m_pBuffer;
					uint num = *(uint*)((ulong)(nint)pBuffer + 8uL);
					uint num2 = *(uint*)((ulong)(nint)pBuffer + 24uL);
					uint num3 = *(uint*)((ulong)(nint)pBuffer + 20uL);
					uint num4 = *(uint*)((ulong)(nint)pBuffer + 16uL);
					int num5 = ((*(int*)((ulong)(nint)pBuffer + 4uL) != 2) ? 1 : 6);
					uint num6 = global::_003CModule_003E.TextureLib_002EGetMipChainSize((FGX_FORMAT)2u, num4, num3, num2, num) * (uint)num5;
					global::_003CModule_003E.Platform_002EMemoryBuffer_002EGrowToFit(&memoryBuffer, num6);
					BufferInfo* pBuffer2 = m_pBuffer;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
					global::_003CModule_003E.TextureLib_002EInitTextureData(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer), *(Primitives.TextureType*)((ulong)(nint)pBuffer2 + 4uL), (FGX_FORMAT)2u, *(uint*)((ulong)(nint)pBuffer2 + 16uL), *(uint*)((ulong)(nint)pBuffer2 + 20uL), *(uint*)((ulong)(nint)pBuffer2 + 24uL), *(uint*)((ulong)(nint)pBuffer2 + 8uL));
					if (!global::_003CModule_003E.TextureLib_002EConvertFormat(&textureData, (TextureData*)m_pBuffer))
					{
						throw new Exception("Cannot import this pixel format!");
					}
					// IL cpblk instruction
					System.Runtime.CompilerServices.Unsafe.CopyBlock(m_pBuffer, ref textureData, 1088);
					global::_003CModule_003E.Platform_002EMemoryBuffer_002EMove(&memoryBuffer, (MemoryBuffer*)((ulong)(nint)m_pBuffer + 1088uL));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
					throw;
				}
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
			}
			try
			{
				return;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
		}
		catch
		{
			//try-fault
			base.Dispose(A_0: true);
			throw;
		}
	}

	public unsafe virtual Bitmap CreateThumbnail()
	{
		//IL_0043: Expected I8, but got I
		//IL_0059: Expected I, but got I8
		//IL_00b6: Expected I8, but got I
		//IL_00cc: Expected I, but got I8
		//IL_018a: Expected I, but got I8
		//IL_01b0: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
		*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		uint num = *(uint*)((ulong)(nint)m_pBuffer + 16uL);
		uint num2 = global::_003CModule_003E.TextureLib_002EGetMipChainSize((FGX_FORMAT)6u, num, num, 1u, 1u) * 6;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E);
		System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)) = (nint)global::_003CModule_003E.TextureLib_002EDefaultTextureAllocator_002ELocalAlloc((DefaultTextureAllocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)), num2);
		global::_003CModule_003E.TextureLib_002EInitTextureData((TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E), (void*)System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)), (Primitives.TextureType)2, (FGX_FORMAT)6u, num, num, 1u, 1u);
		Bitmap bitmap;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlock(ref textureData, m_pBuffer, 1088);
			System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData, 8)) = 1;
			global::_003CModule_003E.TextureLib_002EConvertFormat((TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E), &textureData);
			uint num3 = (uint)(double)global::_003CModule_003E.sqrt((float)(num * num * 6) * 0.5f);
			uint num4 = num3 * 2;
			uint num5 = global::_003CModule_003E.TextureLib_002EGetMipChainSize((FGX_FORMAT)6u, num4, num3, 1u, 1u);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2);
			System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2, 1088)) = (nint)global::_003CModule_003E.TextureLib_002EDefaultTextureAllocator_002ELocalAlloc((DefaultTextureAllocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2, 1088)), num5);
			global::_003CModule_003E.TextureLib_002EInitTextureData((TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2), (void*)System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2, 1088)), (Primitives.TextureType)0, (FGX_FORMAT)6u, num4, num3, 1u, 1u);
			try
			{
				global::_003CModule_003E.TextureLib_002EResampleEnvironmentToLatLong((TextureLevel*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2, 16)), (TextureLevel*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 16)), 32u, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EINVERSE_ENVIRONMENT_BASIS), (delegate* unmanaged[Cdecl, Cdecl]<TextureLevel*, float*, uint, void>)global::_003CModule_003E.__unep_0040_003FFetchCubemapTexels_0040TextureLib_0040_0040_0024_0024FYAXPEBUTextureLevel_0040Primitives_0040_0040PEAMI_0040Z, (Allocator*)(&heapAllocator_003C23_002C0_003E));
				global::_003CModule_003E.TextureLib_002EClampFloat((TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2), 0f, 1f);
				global::_003CModule_003E.TextureLib_002EConvertToGamma((TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2), (GAMMA_TYPE)0, 2.2f, true);
				bitmap = new Bitmap((int)num4, (int)num3);
				Rectangle rect = new Rectangle(0, 0, (int)num4, (int)num3);
				BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
				byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
				System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData2);
				global::_003CModule_003E.TextureLib_002EInitTextureData(&textureData2, ptr, (Primitives.TextureType)0, (FGX_FORMAT)1001u, num4, num3, 1u, 1u);
				global::_003CModule_003E.TextureLib_002EConvertFormat(&textureData2, (TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2));
				bitmap.UnlockBits(bitmapData);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E*, void>)(&global::_003CModule_003E.TextureLib_002EScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E_002E_007Bdtor_007D), &scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2);
				throw;
			}
			global::_003CModule_003E.TextureLib_002EDefaultTextureAllocator_002ELocalFree((DefaultTextureAllocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2, 1088)), (void*)System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E2, 1088)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E*, void>)(&global::_003CModule_003E.TextureLib_002EScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E_002E_007Bdtor_007D), &scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E);
			throw;
		}
		global::_003CModule_003E.TextureLib_002EDefaultTextureAllocator_002ELocalFree((DefaultTextureAllocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)), (void*)System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)));
		return bitmap;
	}

	public unsafe virtual void SaveDDS(string path, PixelFormat TargetFormat)
	{
		//IL_0079: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
		*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)(&heapAllocator_003C23_002C0_003E), 0uL);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E);
		try
		{
			FGX_FORMAT fGX_FORMAT = (FGX_FORMAT)ExportSettingsParams.ConvertPixelFormat(TargetFormat);
			BufferInfo* pBuffer = m_pBuffer;
			if (*(int*)pBuffer != (int)fGX_FORMAT)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out DefaultTextureAllocator kAlloc);
				global::_003CModule_003E.TextureLib_002EScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E_002E_007Bctor_007D(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, (TextureData*)pBuffer, fGX_FORMAT, kAlloc);
				try
				{
					if (!global::_003CModule_003E.TextureLib_002ECreateDDSInMemory((TextureData*)(&scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E), &memoryBuffer))
					{
						throw new Exception("CreateDDSInMemory failed\n");
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E*, void>)(&global::_003CModule_003E.TextureLib_002EScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E_002E_007Bdtor_007D), &scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E);
					throw;
				}
				global::_003CModule_003E.TextureLib_002EDefaultTextureAllocator_002ELocalFree((DefaultTextureAllocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)), (void*)System.Runtime.CompilerServices.Unsafe.As<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E, 1088)));
			}
			else if (!global::_003CModule_003E.TextureLib_002ECreateDDSInMemory((TextureData*)pBuffer, &memoryBuffer))
			{
				throw new Exception("CreateDDSInMemory failed\n");
			}
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(path).ToPointer();
			IO_RESULT num = global::_003CModule_003E.Platform_002EWriteCompleteFile(ptr, &memoryBuffer);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			if (num != 0)
			{
				throw new Exception("File I/O failed");
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		try
		{
			try
			{
				return;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E*, void>)(&global::_003CModule_003E.TextureLib_002EScopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E_002E_007Bdtor_007D), &scopedTextureData_003CTextureLib_003A_003ADefaultTextureAllocator_003E);
				throw;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
	}

	public unsafe virtual ICubeMap Clone()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002Eg_TextureAllocator), 0uL);
		ICubeMap result;
		try
		{
			TextureData* pBuffer = (TextureData*)m_pBuffer;
			ulong num = global::_003CModule_003E.TextureLib_002EGetTextureSize(pBuffer);
			global::_003CModule_003E.Platform_002EMemoryBuffer_002EGrowToFit(&memoryBuffer, num);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
			global::_003CModule_003E.TextureLib_002EInitTextureData(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer), *(Primitives.TextureType*)((ulong)(nint)pBuffer + 4uL), *(FGX_FORMAT*)pBuffer, *(uint*)((ulong)(nint)pBuffer + 16uL), *(uint*)((ulong)(nint)pBuffer + 20uL), 1u, *(uint*)((ulong)(nint)pBuffer + 8uL));
			global::_003CModule_003E.TextureLib_002ECopy(&textureData, pBuffer);
			result = new CubeMap(&memoryBuffer, &textureData);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return result;
	}

	public unsafe virtual void ReExposeFrom(ICubeMap source, float Multiplier)
	{
		//IL_0052: Expected I, but got I8
		//IL_0066: Expected I, but got I8
		//IL_00b4: Expected I, but got I8
		TextureData* pBuffer = (TextureData*)((EnvironmentMapSource)source).m_pBuffer;
		TextureData* pBuffer2 = (TextureData*)m_pBuffer;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY03M _0024ArrayType_0024_0024_0024BY03M2);
		*(float*)(&_0024ArrayType_0024_0024_0024BY03M2) = Multiplier;
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY03M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY03M2, 4)) = Multiplier;
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY03M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY03M2, 8)) = Multiplier;
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY03M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY03M2, 12)) = 1f;
		if (!global::_003CModule_003E.TextureLib_002ECopyAndScaleFloat(pBuffer2, pBuffer, (float*)(&_0024ArrayType_0024_0024_0024BY03M2)))
		{
			ulong num = global::_003CModule_003E.TextureLib_002EGetTextureSize(pBuffer);
			global::_003CModule_003E.Platform_002EMemoryBuffer_002EGrowToFit((MemoryBuffer*)((ulong)(nint)m_pBuffer + 1088uL), num);
			global::_003CModule_003E.TextureLib_002EInitTextureData(pBuffer2, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes((MemoryBuffer*)((ulong)(nint)m_pBuffer + 1088uL)), *(Primitives.TextureType*)((ulong)(nint)pBuffer + 4uL), *(FGX_FORMAT*)pBuffer, *(uint*)((ulong)(nint)pBuffer + 16uL), *(uint*)((ulong)(nint)pBuffer + 20uL), 1u, *(uint*)((ulong)(nint)pBuffer + 8uL));
			bool flag = global::_003CModule_003E.TextureLib_002ECopyAndScaleFloat(pBuffer2, pBuffer, (float*)(&_0024ArrayType_0024_0024_0024BY03M2));
			if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F5_003F_003FReExposeFrom_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICubeMap_0040345_0040M_0040Z_00404_NA && !flag && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_02MFDMBIJM_0040ok_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HAGNIMK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 344u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F5_003F_003FReExposeFrom_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICubeMap_0040345_0040M_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual void CopyFrom(ICubeMap source)
	{
		//IL_0035: Expected I, but got I8
		//IL_0049: Expected I, but got I8
		//IL_0095: Expected I, but got I8
		TextureData* pBuffer = (TextureData*)((EnvironmentMapSource)source).m_pBuffer;
		TextureData* pBuffer2 = (TextureData*)m_pBuffer;
		if (!global::_003CModule_003E.TextureLib_002ECopy(pBuffer2, pBuffer))
		{
			ulong num = global::_003CModule_003E.TextureLib_002EGetTextureSize(pBuffer);
			global::_003CModule_003E.Platform_002EMemoryBuffer_002EGrowToFit((MemoryBuffer*)((ulong)(nint)m_pBuffer + 1088uL), num);
			global::_003CModule_003E.TextureLib_002EInitTextureData(pBuffer2, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes((MemoryBuffer*)((ulong)(nint)m_pBuffer + 1088uL)), *(Primitives.TextureType*)((ulong)(nint)pBuffer + 4uL), *(FGX_FORMAT*)pBuffer, *(uint*)((ulong)(nint)pBuffer + 16uL), *(uint*)((ulong)(nint)pBuffer + 20uL), 1u, *(uint*)((ulong)(nint)pBuffer + 8uL));
			bool flag = global::_003CModule_003E.TextureLib_002ECopy(pBuffer2, pBuffer);
			if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F5_003F_003FCopyFrom_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICubeMap_0040345_0040_0040Z_00404_NA && !flag && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_02MFDMBIJM_0040ok_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HAGNIMK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 369u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F5_003F_003FCopyFrom_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUICubeMap_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
	}

	public unsafe virtual IFloatVector3 GetLightIntensity(float x, float y, float z)
	{
		//IL_006e: Expected I, but got I8
		//IL_002f: Expected I, but got I8
		TextureData* pBuffer = (TextureData*)m_pBuffer;
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003FGetLightIntensity_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIFloatVector3_0040AssetObjects_004045_0040MMM_0040Z_00404_NA && *(int*)pBuffer != 2 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DG_0040FNBGMHCL_0040rTD_003F4m_Format_003F5_003F_0024DN_003F_0024DN_003F5Primitives_003F3_003F3FGXF_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HAGNIMK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 376u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003FGetLightIntensity_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIFloatVector3_0040AssetObjects_004045_0040MMM_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		float num = global::_003CModule_003E.Platform_002Esqrt(x * x + y * y + z * z);
		float num2 = 1f / num;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY02M _0024ArrayType_0024_0024_0024BY02M2);
		*(float*)(&_0024ArrayType_0024_0024_0024BY02M2) = num2 * x;
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY02M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY02M2, 4)) = num2 * y;
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY02M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY02M2, 8)) = num2 * z;
		global::_003CModule_003E.TextureLib_002EFetchCubemapTexelsRGBA((TextureLevel*)((ulong)(nint)pBuffer + 16uL), (float*)(&_0024ArrayType_0024_0024_0024BY02M2), 1u);
		return new FloatVector3(*(float*)(&_0024ArrayType_0024_0024_0024BY02M2), System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY02M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY02M2, 4)), System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY02M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY02M2, 8)));
	}

	public unsafe virtual IFloatVector3 FindSun()
	{
		//IL_005f: Expected I, but got I8
		//IL_0064: Expected I, but got I8
		//IL_0031: Expected I, but got I8
		//IL_02d1: Expected I, but got I8
		//IL_00a7: Expected I, but got I8
		//IL_0259: Expected I, but got I8
		//IL_0108: Expected I, but got I8
		//IL_016f: Expected I, but got I8
		//IL_01d6: Expected I, but got I8
		TextureData* pBuffer = (TextureData*)m_pBuffer;
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003FFindSun_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIFloatVector3_0040AssetObjects_004045_0040XZ_00404_NA && *(int*)pBuffer != 2 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DG_0040FNBGMHCL_0040rTD_003F4m_Format_003F5_003F_0024DN_003F_0024DN_003F5Primitives_003F3_003F3FGXF_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040HAGNIMK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 387u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003FFindSun_0040CubeMap_0040TextureExport_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIFloatVector3_0040AssetObjects_004045_0040XZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		float num = 0f;
		uint nFace = 0u;
		float num2 = 0f;
		float num3 = 0f;
		uint num4 = *(uint*)((ulong)(nint)pBuffer + 16uL);
		uint num5 = 0u;
		TextureData* ptr = (TextureData*)((ulong)(nint)pBuffer + 32uL);
		do
		{
			FGXVector4* ptr2 = (FGXVector4*)(*(ulong*)ptr);
			uint num6 = 0u;
			if (0 < num4)
			{
				do
				{
					uint num7 = 0u;
					if (num4 >= 4)
					{
						uint num8 = num4 - 3;
						uint num9 = num6 * num4;
						uint num10 = num9 + 1;
						uint num11 = 0 - num9;
						uint num12 = 1 - num9;
						uint num13 = 2 - num9;
						do
						{
							FGXVector4* ptr3 = (FGXVector4*)((long)(num10 - 1) * 16L + (nint)ptr2);
							float num14 = *(float*)((ulong)(nint)ptr3 + 4uL);
							float num15 = *(float*)ptr3;
							float num16 = *(float*)((ulong)(nint)ptr3 + 8uL);
							float num17 = num15;
							float num18 = num17 * num17;
							float num19 = num14;
							float num20 = num18 + num19 * num19;
							float num21 = num16;
							float num22 = num20 + num21 * num21;
							if (num22 > num)
							{
								nFace = num5;
								float num23 = 1f / (float)num4;
								num2 = ((float)num7 + 0.5f) * num23;
								num3 = ((float)num6 + 0.5f) * num23;
								num = num22;
							}
							FGXVector4* ptr4 = (FGXVector4*)((long)num10 * 16L + (nint)ptr2);
							num14 = *(float*)((ulong)(nint)ptr4 + 4uL);
							num15 = *(float*)ptr4;
							num16 = *(float*)((ulong)(nint)ptr4 + 8uL);
							float num24 = num15;
							float num25 = num24 * num24;
							float num26 = num14;
							float num27 = num25 + num26 * num26;
							float num28 = num16;
							num22 = num27 + num28 * num28;
							if (num22 > num)
							{
								nFace = num5;
								float num23 = 1f / (float)num4;
								num2 = ((float)(num11 + num10) + 0.5f) * num23;
								num3 = ((float)num6 + 0.5f) * num23;
								num = num22;
							}
							FGXVector4* ptr5 = (FGXVector4*)((long)(num10 + 1) * 16L + (nint)ptr2);
							num14 = *(float*)((ulong)(nint)ptr5 + 4uL);
							num15 = *(float*)ptr5;
							num16 = *(float*)((ulong)(nint)ptr5 + 8uL);
							float num29 = num15;
							float num30 = num29 * num29;
							float num31 = num14;
							float num32 = num30 + num31 * num31;
							float num33 = num16;
							num22 = num32 + num33 * num33;
							if (num22 > num)
							{
								nFace = num5;
								float num23 = 1f / (float)num4;
								num2 = ((float)(num12 + num10) + 0.5f) * num23;
								num3 = ((float)num6 + 0.5f) * num23;
								num = num22;
							}
							FGXVector4* ptr6 = (FGXVector4*)((long)(num10 + 2) * 16L + (nint)ptr2);
							num14 = *(float*)((ulong)(nint)ptr6 + 4uL);
							num15 = *(float*)ptr6;
							num16 = *(float*)((ulong)(nint)ptr6 + 8uL);
							float num34 = num15;
							float num35 = num34 * num34;
							float num36 = num14;
							float num37 = num35 + num36 * num36;
							float num38 = num16;
							num22 = num37 + num38 * num38;
							if (num22 > num)
							{
								nFace = num5;
								float num23 = 1f / (float)num4;
								num2 = ((float)(num13 + num10) + 0.5f) * num23;
								num3 = ((float)num6 + 0.5f) * num23;
								num = num22;
							}
							num7 += 4;
							num10 += 4;
						}
						while (num7 < num8);
					}
					if (num7 < num4)
					{
						uint num39 = num6 * num4;
						do
						{
							FGXVector4* ptr7 = (FGXVector4*)((long)(num39 + num7) * 16L + (nint)ptr2);
							float num40 = *(float*)((ulong)(nint)ptr7 + 4uL);
							float num15 = *(float*)ptr7;
							float num41 = *(float*)((ulong)(nint)ptr7 + 8uL);
							float num42 = num40 * num40;
							float num43 = num15;
							float num22 = num42 + num43 * num43 + num41 * num41;
							if (num22 > num)
							{
								nFace = num5;
								float num23 = 1f / (float)num4;
								num2 = ((float)num7 + 0.5f) * num23;
								num3 = ((float)num6 + 0.5f) * num23;
								num = num22;
							}
							num7++;
						}
						while (num7 < num4);
					}
					num6++;
				}
				while (num6 < num4);
			}
			num5++;
			ptr = (TextureData*)((ulong)(nint)ptr + 8uL);
		}
		while (num5 < 6);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
		global::_003CModule_003E.TextureLib_002EGetCubemapTexelDir(&fGXVector, nFace, num2 * 2f - 1f, 1f - num3 * 2f);
		return new FloatVector3(*(float*)(&fGXVector), System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 4)), System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 8)));
	}

	public new unsafe TextureData* GetTextureData()
	{
		return (TextureData*)m_pBuffer;
	}
}
