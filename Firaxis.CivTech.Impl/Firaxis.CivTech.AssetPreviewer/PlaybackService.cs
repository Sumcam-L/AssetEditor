using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform;

namespace Firaxis.CivTech.AssetPreviewer;

public class PlaybackService : IPlaybackService
{
	private void _007EPlaybackService()
	{
	}

	public virtual ITracePlayer OpenTracePlayer(ICivTechService civ, string TracePath)
	{
		return new TracePlayer(civ, TracePath);
	}

	public unsafe virtual string DisassembleTraceFile(string TracePath)
	{
		//Discarded unreachable code: IL_0082
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
		*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer, (Allocator*)(&heapAllocator_003C23_002C0_003E), 0uL);
		string result;
		try
		{
			char* ptr = (char*)Marshal.StringToHGlobalUni(TracePath).ToPointer();
			bool flag = global::_003CModule_003E.AssetPreviewer_002EDisassembleTraceToMemory(&memoryBuffer, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			if (!flag)
			{
				throw new FileNotFoundException("Unable to open trace file!");
			}
			IntPtr ptr2 = new IntPtr(global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer));
			result = Marshal.PtrToStringAnsi(ptr2);
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
