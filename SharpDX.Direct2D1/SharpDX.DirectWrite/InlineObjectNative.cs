using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("8339FDE3-106F-47ab-8373-1C6295EB10B3")]
public class InlineObjectNative : ComObjectCallback, InlineObject, ICallbackable, IDisposable
{
	public InlineObjectMetrics Metrics
	{
		get
		{
			GetMetrics_(out var metrics);
			return metrics;
		}
	}

	public OverhangMetrics OverhangMetrics
	{
		get
		{
			GetOverhangMetrics_(out var overhangs);
			return overhangs;
		}
	}

	public void Draw(object clientDrawingContext, TextRenderer renderer, float originX, float originY, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
	{
		GCHandle value = GCHandle.Alloc(clientDrawingContext);
		IntPtr iUnknownForObject = Utilities.GetIUnknownForObject(clientDrawingEffect);
		try
		{
			Draw__(GCHandle.ToIntPtr(value), TextRendererShadow.ToIntPtr(renderer), originX, originY, isSideways, isRightToLeft, iUnknownForObject);
		}
		finally
		{
			if (value.IsAllocated)
			{
				value.Free();
			}
			if (iUnknownForObject != IntPtr.Zero)
			{
				Marshal.Release(iUnknownForObject);
			}
		}
	}

	public void GetBreakConditions(out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter)
	{
		GetBreakConditions_(out breakConditionBefore, out breakConditionAfter);
	}

	public InlineObjectNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InlineObjectNative(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new InlineObjectNative(nativePointer);
		}
		return null;
	}

	internal unsafe void Draw__(IntPtr clientDrawingContext, IntPtr renderer, float originX, float originY, Bool isSideways, Bool isRightToLeft, IntPtr clientDrawingEffect)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, float, Bool, Bool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)clientDrawingContext, (void*)renderer, originX, originY, isSideways, isRightToLeft, (void*)clientDrawingEffect)).CheckError();
	}

	internal unsafe void GetMetrics_(out InlineObjectMetrics metrics)
	{
		metrics = default(InlineObjectMetrics);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<InlineObjectMetrics, IntPtr>(ref metrics))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetOverhangMetrics_(out OverhangMetrics overhangs)
	{
		overhangs = default(OverhangMetrics);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<OverhangMetrics, IntPtr>(ref overhangs))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetBreakConditions_(out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<BreakCondition, IntPtr>(ref breakConditionBefore))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<BreakCondition, IntPtr>(ref breakConditionAfter))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
		result.CheckError();
	}
}
