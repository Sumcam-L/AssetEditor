using System;
using System.Drawing;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dHwndGraphics : D2dGraphics
{
	public IntPtr Hwnd
	{
		get
		{
			WindowRenderTarget windowRenderTarget = (WindowRenderTarget)base.D2dRenderTarget;
			return windowRenderTarget.Hwnd;
		}
	}

	public void Resize(Size pixelSize)
	{
		WindowRenderTarget windowRenderTarget = (WindowRenderTarget)base.D2dRenderTarget;
		windowRenderTarget.Resize(new Size2(pixelSize.Width, pixelSize.Height));
	}

	public D2dWindowState CheckWindowState()
	{
		WindowRenderTarget windowRenderTarget = (WindowRenderTarget)base.D2dRenderTarget;
		return (D2dWindowState)windowRenderTarget.CheckWindowState();
	}

	protected override void RecreateRenderTarget()
	{
		WindowRenderTarget windowRenderTarget = (WindowRenderTarget)base.D2dRenderTarget;
		Matrix3x2F transform = base.Transform;
		RenderTarget renderTarget = new WindowRenderTarget(hwndProperties: new HwndRenderTargetProperties
		{
			Hwnd = Hwnd,
			PixelSize = windowRenderTarget.PixelSize,
			PresentOptions = PresentOptions.Immediately
		}, factory: D2dFactory.NativeFactory, renderTargetProperties: D2dFactory.RenderTargetProperties);
		SetRenderTarget(renderTarget);
		base.Transform = transform;
	}

	internal D2dHwndGraphics(WindowRenderTarget renderTarget)
		: base(renderTarget)
	{
	}
}
