using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;

namespace SharpDX.Direct2D1;

[Guid("28506e39-ebf6-46a1-bb47-fd85565ab957")]
public class DrawingStateBlock : Resource
{
	public DrawingStateDescription Description
	{
		get
		{
			GetDescription(out var stateDescription);
			return stateDescription;
		}
		set
		{
			SetDescription(ref value);
		}
	}

	public RenderingParams TextRenderingParams
	{
		get
		{
			GetTextRenderingParams(out var textRenderingParams);
			return textRenderingParams;
		}
		set
		{
			SetTextRenderingParams(value);
		}
	}

	public DrawingStateBlock(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DrawingStateBlock(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DrawingStateBlock(nativePointer);
		}
		return null;
	}

	internal unsafe void GetDescription(out DrawingStateDescription stateDescription)
	{
		stateDescription = default(DrawingStateDescription);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<DrawingStateDescription, IntPtr>(ref stateDescription))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe void SetDescription(ref DrawingStateDescription stateDescription)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<DrawingStateDescription, IntPtr>(ref stateDescription))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe void SetTextRenderingParams(RenderingParams textRenderingParams)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = textRenderingParams?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr);
	}

	internal unsafe void GetTextRenderingParams(out RenderingParams textRenderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &zero);
		textRenderingParams = ((zero == IntPtr.Zero) ? null : new RenderingParams(zero));
	}

	public DrawingStateBlock(Factory factory)
		: this(factory, null, null)
	{
	}

	public DrawingStateBlock(Factory factory, DrawingStateDescription drawingStateDescription)
		: this(factory, drawingStateDescription, null)
	{
	}

	public DrawingStateBlock(Factory factory, RenderingParams textRenderingParams)
		: this(factory, null, textRenderingParams)
	{
	}

	public DrawingStateBlock(Factory factory, DrawingStateDescription? drawingStateDescription, RenderingParams textRenderingParams)
		: base(IntPtr.Zero)
	{
		factory.CreateDrawingStateBlock(drawingStateDescription, textRenderingParams, this);
	}
}
