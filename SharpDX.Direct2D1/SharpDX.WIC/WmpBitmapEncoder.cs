using System;
using System.IO;

namespace SharpDX.WIC;

public class WmpBitmapEncoder : BitmapEncoder
{
	public WmpBitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public WmpBitmapEncoder(ImagingFactory factory)
		: base(factory, ContainerFormatGuids.Wmp)
	{
	}

	public WmpBitmapEncoder(ImagingFactory factory, Stream stream = null)
		: base(factory, ContainerFormatGuids.Wmp, stream)
	{
	}

	public WmpBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, Stream stream = null)
		: base(factory, ContainerFormatGuids.Wmp, guidVendorRef, stream)
	{
	}

	public WmpBitmapEncoder(ImagingFactory factory, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Wmp, stream)
	{
	}

	public WmpBitmapEncoder(ImagingFactory factory, Guid guidVendorRef, WICStream stream = null)
		: base(factory, ContainerFormatGuids.Wmp, guidVendorRef, stream)
	{
	}
}
