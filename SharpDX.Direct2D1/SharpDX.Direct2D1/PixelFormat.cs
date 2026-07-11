using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

public struct PixelFormat
{
	public Format Format;

	public AlphaMode AlphaMode;

	public PixelFormat(Format format, AlphaMode alphaMode)
	{
		Format = format;
		AlphaMode = alphaMode;
	}
}
