using System;
using System.IO;

namespace SharpDX.WIC;

public class JpegBitmapEncoder : BitmapEncoder
{
	public JpegBitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public JpegBitmapEncoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Jpeg)
	{
	}

	public JpegBitmapEncoder(ImagingFactory factory, Stream stream = null)
		: base(factory, ContainerFormatGuids.Jpeg, stream)
	{
	}

	public JpegBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, Stream stream = null)
		: base(factory, ContainerFormatGuids.Jpeg, guidVendorRef, stream)
	{
	}

	public JpegBitmapEncoder(ImagingFactory factory, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Jpeg, stream)
	{
	}

	public JpegBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Jpeg, guidVendorRef, stream)
	{
	}
}
