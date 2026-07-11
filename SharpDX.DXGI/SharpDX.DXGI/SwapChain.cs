using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DXGI;

[Guid("310d36a0-d2e7-4c0a-aa04-6a9d23b8886a")]
public class SwapChain : DeviceChild
{
	public SwapChainDescription Description
	{
		get
		{
			GetDescription(out var descRef);
			return descRef;
		}
	}

	public Output ContainingOutput
	{
		get
		{
			GetContainingOutput(out var outputOut);
			return outputOut;
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

	public int LastPresentCount
	{
		get
		{
			GetLastPresentCount(out var lastPresentCountRef);
			return lastPresentCountRef;
		}
	}

	public bool IsFullScreen
	{
		get
		{
			GetFullscreenState(out var fullscreenRef, out var targetOut);
			targetOut?.Dispose();
			return fullscreenRef;
		}
		set
		{
			SetFullscreenState(value, null);
		}
	}

	public SwapChain(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SwapChain(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new SwapChain(nativePointer);
		}
		return null;
	}

	public unsafe void Present(int syncInterval, PresentFlags flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, syncInterval, (int)flags)).CheckError();
	}

	internal unsafe void GetBuffer(int buffer, Guid riid, out IntPtr surfaceOut)
	{
		Result result;
		fixed (IntPtr* ptr = &surfaceOut)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, buffer, &riid, ptr);
		}
		result.CheckError();
	}

	public unsafe void SetFullscreenState(Bool fullscreen, Output targetRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = targetRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, Bool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, fullscreen, (void*)intPtr)).CheckError();
	}

	public unsafe void GetFullscreenState(out Bool fullscreenRef, out Output targetOut)
	{
		fullscreenRef = default(Bool);
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fullscreenRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		targetOut = ((zero == IntPtr.Zero) ? null : new Output(zero));
		result.CheckError();
	}

	internal unsafe void GetDescription(out SwapChainDescription descRef)
	{
		descRef = default(SwapChainDescription);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<SwapChainDescription, IntPtr>(ref descRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void ResizeBuffers(int bufferCount, int width, int height, Format newFormat, SwapChainFlags swapChainFlags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, bufferCount, width, height, (int)newFormat, (int)swapChainFlags)).CheckError();
	}

	public unsafe void ResizeTarget(ref ModeDescription newTargetParametersRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<ModeDescription, IntPtr>(ref newTargetParametersRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetContainingOutput(out Output outputOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &zero);
		outputOut = ((zero == IntPtr.Zero) ? null : new Output(zero));
		result.CheckError();
	}

	internal unsafe void GetFrameStatistics(out FrameStatistics statsRef)
	{
		statsRef = default(FrameStatistics);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<FrameStatistics, IntPtr>(ref statsRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetLastPresentCount(out int lastPresentCountRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref lastPresentCountRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public SwapChain(Factory factory, ComObject device, SwapChainDescription description)
		: base(IntPtr.Zero)
	{
		factory.CreateSwapChain(device, ref description, this);
	}

	public T GetBackBuffer<T>(int index) where T : ComObject
	{
		GetBuffer(index, Utilities.GetGuidFromType(typeof(T)), out var surfaceOut);
		return CppObject.FromPointer<T>(surfaceOut);
	}
}
