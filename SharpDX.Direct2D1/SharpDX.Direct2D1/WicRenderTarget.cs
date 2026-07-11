using System;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

public class WicRenderTarget : RenderTarget
{
	public WicRenderTarget(Factory factory, SharpDX.WIC.Bitmap wicBitmap, RenderTargetProperties renderTargetProperties)
		: base(IntPtr.Zero)
	{
		factory.CreateWicBitmapRenderTarget(wicBitmap, ref renderTargetProperties, this);
	}
}
