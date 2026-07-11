using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dBitmapGraphics : D2dGraphics
{
	private readonly D2dGraphics m_owner;

	public D2dBitmap GetBitmap()
	{
		BitmapRenderTarget bitmapRenderTarget = (BitmapRenderTarget)base.D2dRenderTarget;
		return new D2dBitmap(m_owner, bitmapRenderTarget.Bitmap);
	}

	protected override void RecreateRenderTarget()
	{
	}

	internal D2dBitmapGraphics(D2dGraphics owner, BitmapRenderTarget renderTarget)
		: base(renderTarget)
	{
		m_owner = owner;
	}
}
