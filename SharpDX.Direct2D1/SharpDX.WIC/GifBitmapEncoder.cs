using System;
using System.IO;

namespace SharpDX.WIC;

public class GifBitmapEncoder : BitmapEncoder
{
	public GifBitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public GifBitmapEncoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Gif)
	{
	}

	public GifBitmapEncoder(ImagingFactory factory, Stream stream = null)
		: base(factory, ContainerFormatGuids.Gif, stream)
	{
	}

	public GifBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, Stream stream = null)
		: base(factory, ContainerFormatGuids.Gif, guidVendorRef, stream)
	{
	}

	public GifBitmapEncoder(ImagingFactory factory, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Gif, stream)
	{
	}

	public GifBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Gif, guidVendorRef, stream)
	{
	}
}
