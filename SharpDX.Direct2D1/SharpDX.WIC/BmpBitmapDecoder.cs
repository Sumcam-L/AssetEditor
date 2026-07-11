using System;

namespace SharpDX.WIC;

public class BmpBitmapDecoder : BitmapDecoder
{
	public BmpBitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public BmpBitmapDecoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Bmp)
	{
	}

	public BmpBitmapDecoder(ImagingFactory factory, Guid guidVendorRef)
		: base(factory, ContainerFormatGuids.Bmp, guidVendorRef)
	{
	}
}
