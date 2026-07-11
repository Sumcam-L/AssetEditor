using System;

namespace SharpDX.Direct2D1;

public struct HwndRenderTargetProperties
{
	public IntPtr Hwnd;

	public Size2 PixelSize;

	public PresentOptions PresentOptions;
}
