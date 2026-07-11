using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd90695-12e2-11dc-9fed-001143a055f9")]
public class BitmapRenderTarget : RenderTarget
{
	public Bitmap Bitmap
	{
		get
		{
			GetBitmap(out var bitmap);
			return bitmap;
		}
	}

	public BitmapRenderTarget(RenderTarget renderTarget, CompatibleRenderTargetOptions options)
		: this(renderTarget, options, null, null, null)
	{
	}

	public BitmapRenderTarget(RenderTarget renderTarget, CompatibleRenderTargetOptions options, Size2F desiredSize)
		: this(renderTarget, options, desiredSize, null, null)
	{
	}

	public BitmapRenderTarget(RenderTarget renderTarget, CompatibleRenderTargetOptions options, PixelFormat? desiredFormat)
		: this(renderTarget, options, null, null, desiredFormat)
	{
	}

	public BitmapRenderTarget(RenderTarget renderTarget, CompatibleRenderTargetOptions options, Size2F? desiredSize, Size2? desiredPixelSize, PixelFormat? desiredFormat)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateCompatibleRenderTarget(desiredSize, desiredPixelSize, desiredFormat, options, this);
	}

	public BitmapRenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapRenderTarget(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapRenderTarget(nativePointer);
		}
		return null;
	}

	internal unsafe void GetBitmap(out Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmap = ((zero == IntPtr.Zero) ? null : new Bitmap(zero));
		result.CheckError();
	}
}
