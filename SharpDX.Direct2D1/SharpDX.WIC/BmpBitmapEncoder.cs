using System;
using System.IO;

namespace SharpDX.WIC;

public class BmpBitmapEncoder : BitmapEncoder
{
	public BmpBitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public BmpBitmapEncoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Bmp)
	{
	}

	public BmpBitmapEncoder(ImagingFactory factory, Stream stream = null)
		: base(factory, ContainerFormatGuids.Bmp, stream)
	{
	}

	public BmpBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, Stream stream = null)
		: base(factory, ContainerFormatGuids.Bmp, guidVendorRef, stream)
	{
	}

	public BmpBitmapEncoder(ImagingFactory factory, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Bmp, stream)
	{
	}

	public BmpBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Bmp, guidVendorRef, stream)
	{
	}
}
