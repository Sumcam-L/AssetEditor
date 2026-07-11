using System;

namespace SharpDX.WIC;

public class GifBitmapDecoder : BitmapDecoder
{
	public GifBitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public GifBitmapDecoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Gif)
	{
	}

	public GifBitmapDecoder(ImagingFactory factory, Guid guidVendorRef)
		: base(factory, ContainerFormatGuids.Gif, guidVendorRef)
	{
	}
}
