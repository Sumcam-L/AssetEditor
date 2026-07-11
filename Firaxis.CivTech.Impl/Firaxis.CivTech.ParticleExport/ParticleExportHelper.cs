using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.ParticleExport;

public class ParticleExportHelper : IParticleExportHelper
{
	private void _007EParticleExportHelper()
	{
	}

	public unsafe virtual IEnumerable<KeyValuePair<ParticleAssetType, string>> GetParticleEffectAssets(string pmFilename)
	{
		//IL_002f: Expected I, but got I8
		//IL_0021: Expected I4, but got I8
		//IL_005c: Expected I, but got I8
		//IL_004e: Expected I4, but got I8
		//IL_00fb: Expected I, but got I8
		//IL_00b9: Expected I8, but got I
		//IL_00c3: Expected I, but got I8
		//IL_00cd: Expected I, but got I8
		//IL_0156: Expected I, but got I8
		//IL_01aa: Expected I, but got I8
		//IL_01d9: Expected I, but got I8
		if (string.IsNullOrEmpty(pmFilename))
		{
			return Enumerable.Empty<KeyValuePair<ParticleAssetType, string>>();
		}
		ForkAssetEnumerator* ptr = (ForkAssetEnumerator*)global::_003CModule_003E.@new(64uL);
		ForkAssetEnumerator* ptr2;
		try
		{
			if (ptr != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr, 0, 64);
				global::_003CModule_003E.AssetObjects_002EContainer_003Cchar_0020const_0020_002A_003E_002E_007Bctor_007D((Container_003Cchar_0020const_0020_002A_003E*)ptr);
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
			global::_003CModule_003E.delete(ptr, 64uL);
			throw;
		}
		ForkAssetEnumerator* ptr3 = (ForkAssetEnumerator*)global::_003CModule_003E.@new(64uL);
		ForkAssetEnumerator* ptr4;
		try
		{
			if (ptr3 != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr3, 0, 64);
				global::_003CModule_003E.AssetObjects_002EContainer_003Cchar_0020const_0020_002A_003E_002E_007Bctor_007D((Container_003Cchar_0020const_0020_002A_003E*)ptr3);
				ptr4 = ptr3;
			}
			else
			{
				ptr4 = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr3, 64uL);
			throw;
		}
		char* ptr5 = (char*)Marshal.StringToHGlobalUni(pmFilename).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
		*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)(&heapAllocator_003C23_002C0_003E), 0uL);
		List<KeyValuePair<ParticleAssetType, string>> list;
		try
		{
			if (global::_003CModule_003E.Platform_002EReadCompleteFile(ptr5, &memoryBuffer) == (IO_RESULT)0)
			{
				void* ptr6 = global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer);
				if (global::_003CModule_003E.Particle_002ESDK_002EPrepareForkPSB(ptr6))
				{
					long num = (nint)global::_003CModule_003E.__unep_0040_003FDo_0040ForkAssetEnumerator_0040AssetObjects_0040_0040_0024_0024FSAXPEAXPEBD_0040Z;
					global::_003CModule_003E.Particle_002ESDK_002EEnumerateForkPSBModels(ptr6, ptr2, (delegate* unmanaged[Cdecl, Cdecl]<void*, sbyte*, void>)num);
					global::_003CModule_003E.Particle_002ESDK_002EEnumerateForkPSBTextures(ptr6, ptr4, (delegate* unmanaged[Cdecl, Cdecl]<void*, sbyte*, void>)num);
				}
			}
			list = new List<KeyValuePair<ParticleAssetType, string>>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EForkAssetEnumerator_002Ebegin(ptr2, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E.const_iterator const_iterator2);
			global::_003CModule_003E.AssetObjects_002EForkAssetEnumerator_002Eend(ptr2, &const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2))
			{
				do
				{
					sbyte* value = (sbyte*)(*(ulong*)global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator));
					IntPtr ptr7 = new IntPtr(value);
					KeyValuePair<ParticleAssetType, string> item = new KeyValuePair<ParticleAssetType, string>(ParticleAssetType.Model, Marshal.PtrToStringAnsi(ptr7));
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2));
			}
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E.const_iterator const_iterator3);
			global::_003CModule_003E.AssetObjects_002EForkAssetEnumerator_002Ebegin(ptr4, &const_iterator3);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E.const_iterator const_iterator4);
			global::_003CModule_003E.AssetObjects_002EForkAssetEnumerator_002Eend(ptr4, &const_iterator4);
			if (global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator3, &const_iterator4))
			{
				do
				{
					sbyte* value2 = (sbyte*)(*(ulong*)global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator3));
					IntPtr ptr8 = new IntPtr(value2);
					KeyValuePair<ParticleAssetType, string> item2 = new KeyValuePair<ParticleAssetType, string>(ParticleAssetType.Texture, Marshal.PtrToStringAnsi(ptr8));
					list.Add(item2);
					global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator3);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator3, &const_iterator4));
			}
			IntPtr hglobal = new IntPtr(ptr5);
			Marshal.FreeHGlobal(hglobal);
			ForkAssetEnumerator* pThis = ptr2;
			if (ptr2 != null)
			{
				try
				{
					global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002E_007Bdtor_007D((ChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E*)((ulong)(nint)ptr2 + 32uL));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E_002E_007Bdtor_007D), pThis);
					throw;
				}
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E*)ptr2);
				global::_003CModule_003E.delete(ptr2, 64uL);
			}
			ForkAssetEnumerator* pThis2 = ptr4;
			if (ptr4 != null)
			{
				try
				{
					global::_003CModule_003E.Types_002EChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E_002E_007Bdtor_007D((ChunkedVector_003Cchar_0020const_0020_002A_002C4096_003E*)((ulong)(nint)ptr4 + 32uL));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E_002E_007Bdtor_007D), pThis2);
					throw;
				}
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E*)ptr4);
				global::_003CModule_003E.delete(ptr4, 64uL);
			}
			list.Sort(global::_003CModule_003E.Firaxis_002ECivTech_002EParticleExport_002E_003FA0x99a2b882_002EAssetComparison);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return list;
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
