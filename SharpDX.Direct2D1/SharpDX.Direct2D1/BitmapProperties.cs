namespace SharpDX.Direct2D1;

public struct BitmapProperties
{
	public PixelFormat PixelFormat;

	public float DpiX;

	public float DpiY;

	public BitmapProperties(PixelFormat pixelFormat)
	{
		DpiX = 96f;
		DpiY = 96f;
		PixelFormat = pixelFormat;
	}

	public BitmapProperties(PixelFormat pixelFormat, float dpiX, float dpiY)
	{
		PixelFormat = pixelFormat;
		DpiX = dpiX;
		DpiY = dpiY;
	}
}
