using System;

namespace SharpDX.WIC;

public class WmpBitmapDecoder : BitmapDecoder
{
	public WmpBitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public WmpBitmapDecoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Wmp)
	{
	}

	public WmpBitmapDecoder(ImagingFactory factory, Guid guidVendorRef)
		: base(factory, ContainerFormatGuids.Wmp, guidVendorRef)
	{
	}
}
