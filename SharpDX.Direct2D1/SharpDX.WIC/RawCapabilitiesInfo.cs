namespace SharpDX.WIC;

public struct RawCapabilitiesInfo
{
	public int CbSize;

	public int CodecMajorVersion;

	public int CodecMinorVersion;

	public RawCapabilities ExposureCompensationSupport;

	public RawCapabilities ContrastSupport;

	public RawCapabilities RGBWhitePointSupport;

	public RawCapabilities NamedWhitePointSupport;

	public int NamedWhitePointSupportMask;

	public RawCapabilities KelvinWhitePointSupport;

	public RawCapabilities GammaSupport;

	public RawCapabilities TintSupport;

	public RawCapabilities SaturationSupport;

	public RawCapabilities SharpnessSupport;

	public RawCapabilities NoiseReductionSupport;

	public RawCapabilities DestinationColorProfileSupport;

	public RawCapabilities ToneCurveSupport;

	public RawRotationCapabilities RotationSupport;

	public RawCapabilities RenderModeSupport;
}
