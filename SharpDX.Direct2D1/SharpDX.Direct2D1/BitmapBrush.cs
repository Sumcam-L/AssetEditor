using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906aa-12e2-11dc-9fed-001143a055f9")]
public class BitmapBrush : Brush
{
	public ExtendMode ExtendModeX
	{
		get
		{
			return GetExtendModeX();
		}
		set
		{
			SetExtendModeX(value);
		}
	}

	public ExtendMode ExtendModeY
	{
		get
		{
			return GetExtendModeY();
		}
		set
		{
			SetExtendModeY(value);
		}
	}

	public BitmapInterpolationMode InterpolationMode
	{
		get
		{
			return GetInterpolationMode();
		}
		set
		{
			SetInterpolationMode(value);
		}
	}

	public Bitmap Bitmap
	{
		get
		{
			GetBitmap(out var bitmap);
			return bitmap;
		}
		set
		{
			SetBitmap(value);
		}
	}

	public BitmapBrush(RenderTarget renderTarget, Bitmap bitmap)
		: this(renderTarget, bitmap, null, null)
	{
	}

	public BitmapBrush(RenderTarget renderTarget, Bitmap bitmap, BitmapBrushProperties bitmapBrushProperties)
		: this(renderTarget, bitmap, bitmapBrushProperties, null)
	{
	}

	public BitmapBrush(RenderTarget renderTarget, Bitmap bitmap, BrushProperties brushProperties)
		: this(renderTarget, bitmap, null, brushProperties)
	{
	}

	public BitmapBrush(RenderTarget renderTarget, Bitmap bitmap, BitmapBrushProperties? bitmapBrushProperties, BrushProperties? brushProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateBitmapBrush(bitmap, bitmapBrushProperties, brushProperties, this);
	}

	public BitmapBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapBrush(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapBrush(nativePointer);
		}
		return null;
	}

	internal unsafe void SetExtendModeX(ExtendMode extendModeX)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)extendModeX);
	}

	internal unsafe void SetExtendModeY(ExtendMode extendModeY)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (int)extendModeY);
	}

	internal unsafe void SetInterpolationMode(BitmapInterpolationMode interpolationMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)interpolationMode);
	}

	internal unsafe void SetBitmap(Bitmap bitmap)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = bitmap?.NativePointer ?? IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr);
	}

	internal unsafe ExtendMode GetExtendModeX()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ExtendMode GetExtendModeY()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe BitmapInterpolationMode GetInterpolationMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, BitmapInterpolationMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetBitmap(out Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmap = ((zero == IntPtr.Zero) ? null : new Bitmap(zero));
	}
}
