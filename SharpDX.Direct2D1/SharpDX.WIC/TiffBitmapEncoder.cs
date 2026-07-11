using System;
using System.IO;

namespace SharpDX.WIC;

public class TiffBitmapEncoder : BitmapEncoder
{
	public TiffBitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public TiffBitmapEncoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Tiff)
	{
	}

	public TiffBitmapEncoder(ImagingFactory factory, Stream stream = null)
		: base(factory, ContainerFormatGuids.Tiff, stream)
	{
	}

	public TiffBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, Stream stream = null)
		: base(factory, ContainerFormatGuids.Tiff, guidVendorRef, stream)
	{
	}

	public TiffBitmapEncoder(ImagingFactory factory, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Tiff, stream)
	{
	}

	public TiffBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Tiff, guidVendorRef, stream)
	{
	}
}
