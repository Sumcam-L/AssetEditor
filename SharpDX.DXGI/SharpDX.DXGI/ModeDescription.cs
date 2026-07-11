namespace SharpDX.DXGI;

public struct ModeDescription
{
	public int Width;

	public int Height;

	public Rational RefreshRate;

	public Format Format;

	public DisplayModeScanlineOrder ScanlineOrdering;

	public DisplayModeScaling Scaling;

	public ModeDescription(int width, int height, Rational refreshRate, Format format)
	{
		Width = width;
		Height = height;
		RefreshRate = refreshRate;
		Format = format;
		ScanlineOrdering = DisplayModeScanlineOrder.Unspecified;
		Scaling = DisplayModeScaling.Unspecified;
	}
}
