using System;

namespace SharpDX.WIC;

public class TiffBitmapDecoder : BitmapDecoder
{
	public TiffBitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public TiffBitmapDecoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Tiff)
	{
	}

	public TiffBitmapDecoder(ImagingFactory factory, Guid guidVendorRef)
		: base(factory, ContainerFormatGuids.Tiff, guidVendorRef)
	{
	}
}
