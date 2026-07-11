using System;
using System.IO;

namespace SharpDX.WIC;

public class PngBitmapEncoder : BitmapEncoder
{
	public PngBitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public PngBitmapEncoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Png)
	{
	}

	public PngBitmapEncoder(ImagingFactory factory, Stream stream = null)
		: base(factory, ContainerFormatGuids.Png, stream)
	{
	}

	public PngBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, Stream stream = null)
		: base(factory, ContainerFormatGuids.Png, guidVendorRef, stream)
	{
	}

	public PngBitmapEncoder(ImagingFactory factory, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Png, stream)
	{
	}

	public PngBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Png, guidVendorRef, stream)
	{
	}
}
