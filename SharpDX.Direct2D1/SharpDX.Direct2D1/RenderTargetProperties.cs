namespace SharpDX.Direct2D1;

public struct RenderTargetProperties
{
	public RenderTargetType Type;

	public PixelFormat PixelFormat;

	public float DpiX;

	public float DpiY;

	public RenderTargetUsage Usage;

	public FeatureLevel MinLevel;

	public RenderTargetProperties(PixelFormat pixelFormat)
		: this(RenderTargetType.Default, pixelFormat, 96f, 96f, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT)
	{
	}

	public RenderTargetProperties(RenderTargetType type, PixelFormat pixelFormat, float dpiX, float dpiY, RenderTargetUsage usage, FeatureLevel minLevel)
	{
		Type = type;
		PixelFormat = pixelFormat;
		DpiX = dpiX;
		DpiY = dpiY;
		Usage = usage;
		MinLevel = minLevel;
	}
}
