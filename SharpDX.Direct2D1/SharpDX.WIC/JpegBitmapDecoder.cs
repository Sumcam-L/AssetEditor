using System;

namespace SharpDX.WIC;

public class JpegBitmapDecoder : BitmapDecoder
{
	public JpegBitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public JpegBitmapDecoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Jpeg)
	{
	}

	public JpegBitmapDecoder(ImagingFactory factory, Guid guidVendorRef)
		: base(factory, ContainerFormatGuids.Jpeg, guidVendorRef)
	{
	}
}
