using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("00000103-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapEncoder : ComObject
{
	private ImagingFactory factory;

	private WICStream internalWICStream;

	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public BitmapEncoderInfo EncoderInfo
	{
		get
		{
			GetEncoderInfo(out var encoderInfoOut);
			return encoderInfoOut;
		}
	}

	public Palette Palette
	{
		set
		{
			SetPalette(value);
		}
	}

	public BitmapSource Thumbnail
	{
		set
		{
			SetThumbnail(value);
		}
	}

	public BitmapSource Preview
	{
		set
		{
			SetPreview(value);
		}
	}

	public MetadataQueryWriter MetadataQueryWriter
	{
		get
		{
			GetMetadataQueryWriter(out var metadataQueryWriterOut);
			return metadataQueryWriterOut;
		}
	}

	public BitmapEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapEncoder(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapEncoder(nativePointer);
		}
		return null;
	}

	internal unsafe void Initialize_(IntPtr streamRef, BitmapEncoderCacheOption cacheOption)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)streamRef, (int)cacheOption)).CheckError();
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref guidContainerFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetEncoderInfo(out BitmapEncoderInfo encoderInfoOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &zero);
		encoderInfoOut = ((zero == IntPtr.Zero) ? null : new BitmapEncoderInfo(zero));
		result.CheckError();
	}

	internal unsafe void SetColorContexts(int count, ColorContext[] colorContextOut)
	{
		IntPtr* ptr = null;
		if (colorContextOut != null)
		{
			IntPtr* ptr2 = stackalloc IntPtr[colorContextOut.Length];
			ptr = ptr2;
			for (int i = 0; i < colorContextOut.Length; i++)
			{
				ptr[i] = ((colorContextOut[i] == null) ? IntPtr.Zero : colorContextOut[i].NativePointer);
			}
		}
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, count, ptr)).CheckError();
	}

	internal unsafe void SetColorContexts(int count, ComArray<ColorContext> colorContextOut)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = colorContextOut?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr)).CheckError();
	}

	internal unsafe void SetPalette(Palette paletteRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = paletteRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void SetThumbnail(BitmapSource thumbnailRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = thumbnailRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void SetPreview(BitmapSource previewRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = previewRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void CreateNewFrame(BitmapFrameEncode frameEncodeOut, PropertyBag encoderOptionsOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero, &zero2);
		frameEncodeOut.NativePointer = zero;
		encoderOptionsOut.NativePointer = zero2;
		result.CheckError();
	}

	public unsafe void Commit()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void GetMetadataQueryWriter(out MetadataQueryWriter metadataQueryWriterOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		metadataQueryWriterOut = ((zero == IntPtr.Zero) ? null : new MetadataQueryWriter(zero));
		result.CheckError();
	}

	public BitmapEncoder(ImagingFactory factory, Guid containerFormatGuid)
	{
		this.factory = factory;
		factory.CreateEncoder(containerFormatGuid, null, this);
	}

	public BitmapEncoder(ImagingFactory factory, Guid containerFormatGuid, Guid guidVendorRef)
	{
		this.factory = factory;
		factory.CreateEncoder(containerFormatGuid, guidVendorRef, this);
	}

	public BitmapEncoder(ImagingFactory factory, Guid containerFormatGuid, WICStream stream = null)
	{
		this.factory = factory;
		factory.CreateEncoder(containerFormatGuid, null, this);
		if (stream != null)
		{
			Initialize(stream);
		}
	}

	public BitmapEncoder(ImagingFactory factory, Guid containerFormatGuid, Stream stream = null)
	{
		this.factory = factory;
		factory.CreateEncoder(containerFormatGuid, null, this);
		if (stream != null)
		{
			Initialize(stream);
		}
	}

	public BitmapEncoder(ImagingFactory factory, Guid containerFormatGuid, Guid guidVendorRef, WICStream stream = null)
	{
		this.factory = factory;
		factory.CreateEncoder(containerFormatGuid, guidVendorRef, this);
		if (stream != null)
		{
			Initialize(stream);
		}
	}

	public BitmapEncoder(ImagingFactory factory, Guid containerFormatGuid, Guid guidVendorRef, Stream stream = null)
	{
		this.factory = factory;
		factory.CreateEncoder(containerFormatGuid, guidVendorRef, this);
		if (stream != null)
		{
			Initialize(stream);
		}
	}

	public void Initialize(IStream stream)
	{
		if (internalWICStream != null)
		{
			throw new InvalidOperationException("This instance is already initialized with an existing stream");
		}
		Initialize_(ComStream.ToIntPtr(stream), BitmapEncoderCacheOption.NoCache);
	}

	public void Initialize(Stream stream)
	{
		if (internalWICStream != null)
		{
			throw new InvalidOperationException("This instance is already initialized with an existing stream");
		}
		internalWICStream = new WICStream(factory, stream);
		Initialize_(internalWICStream.NativePointer, BitmapEncoderCacheOption.NoCache);
	}

	public void SetColorContexts(ColorContext[] colorContextOut)
	{
		SetColorContexts((colorContextOut != null) ? colorContextOut.Length : 0, colorContextOut);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			if (internalWICStream != null)
			{
				internalWICStream.Dispose();
			}
			internalWICStream = null;
		}
	}
}
