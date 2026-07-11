using System;

namespace SharpDX.WIC;

public class PngBitmapDecoder : BitmapDecoder
{
	public PngBitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public PngBitmapDecoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Png)
	{
	}

	public PngBitmapDecoder(ImagingFactory factory, Guid guidVendorRef)
		: base(factory, ContainerFormatGuids.Png, guidVendorRef)
	{
	}
}
