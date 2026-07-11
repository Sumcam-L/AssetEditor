using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("ae02eedb-c735-4690-8d52-5a8dc20213aa")]
public class Output : DXGIObject
{
	public OutputDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public GammaControlCapabilities GammaControlCapabilities
	{
		get
		{
			GetGammaControlCapabilities(out var gammaCapsRef);
			return gammaCapsRef;
		}
	}

	public GammaControl GammaControl
	{
		get
		{
			GetGammaControl(out var arrayRef);
			return arrayRef;
		}
		set
		{
			SetGammaControl(ref value);
		}
	}

	public FrameStatistics FrameStatistics
	{
		get
		{
			GetFrameStatistics(out var statsRef);
			return statsRef;
		}
	}

	public Output(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Output(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Output(nativePointer);
		}
		return null;
	}

	internal unsafe void GetDescription(out OutputDescription descRef)
	{
		OutputDescription.__Native @ref = default(OutputDescription.__Native);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		descRef = default(OutputDescription);
		descRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetDisplayModeList(Format enumFormat, int flags, ref int numModesRef, ModeDescription[] descRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref numModesRef))
		{
			fixed (IntPtr* ptr2 = descRef)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)enumFormat, flags, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void FindClosestMatchingMode(ref ModeDescription modeToMatchRef, out ModeDescription closestMatchRef, ComObject concernedDeviceRef)
	{
		closestMatchRef = default(ModeDescription);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<ModeDescription, IntPtr>(ref modeToMatchRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<ModeDescription, IntPtr>(ref closestMatchRef))
			{
				void* nativePointer = _nativePointer;
				IntPtr intPtr = concernedDeviceRef?.NativePointer ?? IntPtr.Zero;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, ptr, ptr2, (void*)intPtr);
			}
		}
		result.CheckError();
	}

	public unsafe void WaitForVerticalBlank()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void TakeOwnership(ComObject deviceRef, Bool exclusive)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = deviceRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, Bool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, exclusive)).CheckError();
	}

	public unsafe void ReleaseOwnership()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetGammaControlCapabilities(out GammaControlCapabilities gammaCapsRef)
	{
		GammaControlCapabilities.__Native @ref = default(GammaControlCapabilities.__Native);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		gammaCapsRef = default(GammaControlCapabilities);
		gammaCapsRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe void SetGammaControl(ref GammaControl arrayRef)
	{
		GammaControl.__Native @ref = default(GammaControl.__Native);
		arrayRef.__MarshalTo(ref @ref);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		arrayRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetGammaControl(out GammaControl arrayRef)
	{
		GammaControl.__Native @ref = default(GammaControl.__Native);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &@ref);
		arrayRef = default(GammaControl);
		arrayRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void SetDisplaySurface(Surface scanoutSurfaceRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = scanoutSurfaceRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	public unsafe void CopyDisplaySurfaceTo(Surface destinationRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = destinationRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void GetFrameStatistics(out FrameStatistics statsRef)
	{
		statsRef = default(FrameStatistics);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<FrameStatistics, IntPtr>(ref statsRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public void GetClosestMatchingMode(ComObject device, ModeDescription modeToMatch, out ModeDescription closestMatch)
	{
		FindClosestMatchingMode(ref modeToMatch, out closestMatch, device);
	}

	public ModeDescription[] GetDisplayModeList(Format format, DisplayModeEnumerationFlags flags)
	{
		int numModesRef = 0;
		GetDisplayModeList(format, (int)flags, ref numModesRef, null);
		ModeDescription[] array = new ModeDescription[numModesRef];
		if (numModesRef > 0)
		{
			GetDisplayModeList(format, (int)flags, ref numModesRef, array);
		}
		return array;
	}
}
