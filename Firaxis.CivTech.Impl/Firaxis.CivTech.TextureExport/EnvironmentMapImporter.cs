using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Firaxis.CivTech.AssetObjects;
using Platform;
using Primitives;
using TextureExport;

namespace Firaxis.CivTech.TextureExport;

public class EnvironmentMapImporter : IEnvironmentMapImporter
{
	public virtual IEnumerable<string> SupportedFileTypes
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(".dds");
			return list;
		}
	}

	private void _007EEnvironmentMapImporter()
	{
	}

	public unsafe virtual IEnvironmentSource OpenSourceFile(string path)
	{
		//Discarded unreachable code: IL_00c9
		if (!File.Exists(path))
		{
			throw new ArgumentException("The path passed in does not exist on disk.");
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
		IEnvironmentSource result;
		try
		{
			global::_003CModule_003E.Platform_002EMemoryBuffer_002EInit(&memoryBuffer, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002Eg_TextureAllocator), 0uL);
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(path).ToPointer();
			IO_RESULT num = global::_003CModule_003E.Platform_002EReadCompleteFile(ptr, &memoryBuffer);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			if (num != 0)
			{
				throw new FileLoadException("Could not open file");
			}
			if (!global::_003CModule_003E.TextureLib_002EInitTextureDataFromDDS(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer)))
			{
				throw new ArgumentException("Not a DDS file");
			}
			if (System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData, 4)) == 2)
			{
				result = new CubeMap(&memoryBuffer, &textureData);
				goto IL_009c;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		IEnvironmentSource result2;
		try
		{
			result2 = new EnvironmentMapSource(&memoryBuffer, &textureData);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return result2;
		IL_009c:
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return result;
	}

	public unsafe virtual ICubeMap CreateCube(IEnvironmentSource source, EnvironmentMapParameterization eSourceParametrization, uint SampleCount, IEnvironmentMapImportOptions opts, [MarshalAs(UnmanagedType.U1)] bool useIdentityBasis, uint width)
	{
		//Discarded unreachable code: IL_01ef
		TextureData* textureData = ((EnvironmentMapSource)source).GetTextureData();
		uint num = width;
		global::TextureExport.EnvironmentMapParameterization environmentMapParameterization = eSourceParametrization switch
		{
			EnvironmentMapParameterization.ENVMAP_CUBE => (global::TextureExport.EnvironmentMapParameterization)1, 
			EnvironmentMapParameterization.ENVMAP_LATLONG => (global::TextureExport.EnvironmentMapParameterization)0, 
			_ => throw new Exception("Bad enum"), 
		};
		if (opts.RequirePow2 && global::_003CModule_003E.Math_002EIsPow2(width) == 0)
		{
			num = global::_003CModule_003E.Math_002ERoundDownPow2(width);
		}
		if (num < opts.MinWidth)
		{
			num = opts.MinWidth;
		}
		if (num > opts.MaxWidth)
		{
			num = opts.MaxWidth;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out EnvironmentImportSettings environmentImportSettings);
		global::_003CModule_003E.TextureExport_002EEnvironmentImportSettings_002E_007Bctor_007D(&environmentImportSettings);
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, sbyte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 8)) = 0;
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, sbyte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 9)) = 0;
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, global::TextureExport.EnvironmentMapParameterization>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 4)) = environmentMapParameterization;
		*(global::TextureExport.PixelFormat*)(&environmentImportSettings) = ExportSettingsParams.ConvertPixelFormat(PixelFormat.R32G32B32A32_FLOAT);
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 28)) = 0f;
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 24)) = 0f;
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 12)) = num;
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 16)) = 0;
		System.Runtime.CompilerServices.Unsafe.As<EnvironmentImportSettings, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 20)) = SampleCount;
		if (useIdentityBasis)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			FGXVector3* ptr = global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, 1f, 0f, 0f);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 32), ptr, 12);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector2);
			FGXVector3* ptr2 = global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector2, 0f, 1f, 0f);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 44), ptr2, 12);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector3);
			FGXVector3* ptr3 = global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector3, 0f, 0f, 1f);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 56), ptr3, 12);
		}
		else
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector4);
			FGXVector3* ptr4 = global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector4, 1f, 0f, 0f);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 32), ptr4, 12);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector5);
			FGXVector3* ptr5 = global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector5, 0f, 0f, 1f);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 44), ptr5, 12);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector6);
			FGXVector3* ptr6 = global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector6, 0f, 1f, 0f);
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref environmentImportSettings, 56), ptr6, 12);
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002Eg_TextureAllocator), 0uL);
		ICubeMap result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData2);
			if (!global::_003CModule_003E.TextureExport_002ECreateCubeMap(&textureData2, &memoryBuffer, textureData, &environmentImportSettings, (Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002Eg_TextureAllocator)))
			{
				throw new Exception("CreateCubeMap failed\n");
			}
			result = new CubeMap(&memoryBuffer, &textureData2);
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

	public unsafe virtual ICubeMap OpenCubeMap(string dds)
	{
		//Discarded unreachable code: IL_00b7
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C0_002C0_003E heapAllocator_003C0_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C0_002C0_003E));
		*(long*)(&heapAllocator_003C0_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240A_0040_00240A_0040_0040Platform_0040_00406B_0040);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C9_002C0_003E heapAllocator_003C9_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C9_002C0_003E));
		*(long*)(&heapAllocator_003C9_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_002408_00240A_0040_0040Platform_0040_00406B_0040);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
		ICubeMap result;
		try
		{
			global::_003CModule_003E.Platform_002EMemoryBuffer_002EInit(&memoryBuffer, (Allocator*)(&heapAllocator_003C0_002C0_003E), 0uL);
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(dds).ToPointer();
			IO_RESULT num = global::_003CModule_003E.Platform_002EReadCompleteFile(ptr, &memoryBuffer);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			if (num != 0)
			{
				throw new Exception("Could not open file");
			}
			System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
			if (!global::_003CModule_003E.TextureLib_002EInitTextureDataFromDDS(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer)))
			{
				throw new Exception("Not a DDS file");
			}
			if (System.Runtime.CompilerServices.Unsafe.As<TextureData, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref textureData, 4)) != 2)
			{
				throw new Exception("Not a cube map");
			}
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

	public unsafe virtual float[] DirectionToThumbnailUV(float x, float y, float z)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, x, y, z);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector2);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector2, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EINVERSE_ENVIRONMENT_BASIS));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector3);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector3, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EINVERSE_ENVIRONMENT_BASIS, 12)));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector4);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector4, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EINVERSE_ENVIRONMENT_BASIS, 24)));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector5);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector6);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector7);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector8);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector9);
		global::_003CModule_003E.FGXVector3_002E_002B(global::_003CModule_003E.FGXVector3_002E_002B(global::_003CModule_003E.FGXVector3_002E_002A(&fGXVector2, &fGXVector5, *(float*)(&fGXVector)), &fGXVector6, *global::_003CModule_003E.FGXVector3_002E_002A(&fGXVector3, &fGXVector7, System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 4)))), &fGXVector8, *global::_003CModule_003E.FGXVector3_002E_002A(&fGXVector4, &fGXVector9, System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 8))));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector2 fGXVector10);
		global::_003CModule_003E.TextureLib_002EDirectionToLatLongUV(&fGXVector10, *(float*)(&fGXVector8), System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector8, 4)), System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector8, 8)));
		return new float[2]
		{
			*(float*)(&fGXVector10),
			System.Runtime.CompilerServices.Unsafe.As<FGXVector2, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector10, 4))
		};
	}

	public unsafe virtual float[] ThumbnailUVToDirection(float u, float v)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
		global::_003CModule_003E.TextureLib_002ELatLongUVToDirection(&fGXVector, u, v);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector2);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector2, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EENVIRONMENT_BASIS));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector3);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector3, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EENVIRONMENT_BASIS, 12)));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector4);
		global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector4, (float*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref global::_003CModule_003E.Firaxis_002ECivTech_002ETextureExport_002E_003FA0x4a1fc913_002EENVIRONMENT_BASIS, 24)));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector5);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector6);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector7);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector8);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector9);
		global::_003CModule_003E.FGXVector3_002E_002B(global::_003CModule_003E.FGXVector3_002E_002B(global::_003CModule_003E.FGXVector3_002E_002A(&fGXVector2, &fGXVector5, *(float*)(&fGXVector)), &fGXVector6, *global::_003CModule_003E.FGXVector3_002E_002A(&fGXVector3, &fGXVector7, System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 4)))), &fGXVector8, *global::_003CModule_003E.FGXVector3_002E_002A(&fGXVector4, &fGXVector9, System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 8))));
		return new float[3]
		{
			*(float*)(&fGXVector8),
			System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector8, 4)),
			System.Runtime.CompilerServices.Unsafe.As<FGXVector3, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector8, 8))
		};
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (!A_0)
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
