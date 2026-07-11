using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a9-12e2-11dc-9fed-001143a055f9")]
public class SolidColorBrush : Brush
{
	public Color4 Color
	{
		get
		{
			return GetColor();
		}
		set
		{
			SetColor(value);
		}
	}

	public SolidColorBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SolidColorBrush(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new SolidColorBrush(nativePointer);
		}
		return null;
	}

	internal unsafe void SetColor(Color4 color)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &color);
	}

	internal unsafe Color4 GetColor()
	{
		Color4 result = default(Color4);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	public SolidColorBrush(RenderTarget renderTarget, Color4 color)
		: this(renderTarget, color, null)
	{
	}

	public SolidColorBrush(RenderTarget renderTarget, Color4 color, BrushProperties? brushProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateSolidColorBrush(color, brushProperties, this);
	}
}
