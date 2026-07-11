using System.Drawing;
using System.Drawing.Imaging;
using SharpDX.Direct2D1;
using SharpDX.WIC;

namespace Sce.Atf.Direct2D;

public sealed class D2dWicGraphics : D2dGraphics
{
	private readonly SharpDX.WIC.Bitmap m_wicBitmap;

	internal D2dWicGraphics(WicRenderTarget renderTarget, SharpDX.WIC.Bitmap wicBitmap)
		: base(renderTarget)
	{
		m_wicBitmap = wicBitmap;
	}

	protected override void RecreateRenderTarget()
	{
	}

	public System.Drawing.Bitmap Copy()
	{
		System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(m_wicBitmap.Size.Width, m_wicBitmap.Size.Height);
		BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
		m_wicBitmap.CopyPixels(bitmapData.Stride, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride);
		bitmap.UnlockBits(bitmapData);
		return bitmap;
	}
}
