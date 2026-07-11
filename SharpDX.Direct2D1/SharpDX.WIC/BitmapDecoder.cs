using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.IO;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("9EDDE9E7-8DEE-47ea-99DF-E6FAF2ED44BF")]
public class BitmapDecoder : ComObject
{
	private WICStream internalWICStream;

	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public BitmapDecoderInfo DecoderInfo
	{
		get
		{
			GetDecoderInfo(out var decoderInfoOut);
			return decoderInfoOut;
		}
	}

	public MetadataQueryReader MetadataQueryReader
	{
		get
		{
			GetMetadataQueryReader(out var metadataQueryReaderOut);
			return metadataQueryReaderOut;
		}
	}

	public BitmapSource Preview
	{
		get
		{
			GetPreview(out var bitmapSourceOut);
			return bitmapSourceOut;
		}
	}

	public BitmapSource Thumbnail
	{
		get
		{
			GetThumbnail(out var thumbnailOut);
			return thumbnailOut;
		}
	}

	public int FrameCount
	{
		get
		{
			GetFrameCount(out var countRef);
			return countRef;
		}
	}

	[Obsolete("Use TryGetColorContexts instead")]
	public ColorContext[] ColorContexts => new ColorContext[0];

	public BitmapDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapDecoder(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapDecoder(nativePointer);
		}
		return null;
	}

	internal unsafe BitmapDecoderCapabilities QueryCapability_(IntPtr streamRef)
	{
		BitmapDecoderCapabilities result = default(BitmapDecoderCapabilities);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)streamRef, &result)).CheckError();
		return result;
	}

	internal unsafe void Initialize_(IntPtr streamRef, DecodeOptions cacheOptions)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)streamRef, (int)cacheOptions)).CheckError();
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref guidContainerFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetDecoderInfo(out BitmapDecoderInfo decoderInfoOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &zero);
		decoderInfoOut = ((zero == IntPtr.Zero) ? null : new BitmapDecoderInfo(zero));
		result.CheckError();
	}

	public unsafe void CopyPalette(Palette paletteRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = paletteRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void GetMetadataQueryReader(out MetadataQueryReader metadataQueryReaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		metadataQueryReaderOut = ((zero == IntPtr.Zero) ? null : new MetadataQueryReader(zero));
		result.CheckError();
	}

	internal unsafe void GetPreview(out BitmapSource bitmapSourceOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &zero);
		bitmapSourceOut = ((zero == IntPtr.Zero) ? null : new BitmapSource(zero));
		result.CheckError();
	}

	internal unsafe Result GetColorContexts(int count, ColorContext[] colorContextsOut, out int actualCountRef)
	{
		IntPtr* ptr = null;
		if (colorContextsOut != null)
		{
			IntPtr* ptr2 = stackalloc IntPtr[colorContextsOut.Length];
			ptr = ptr2;
			for (int i = 0; i < colorContextsOut.Length; i++)
			{
				ptr[i] = ((colorContextsOut[i] == null) ? IntPtr.Zero : colorContextsOut[i].NativePointer);
			}
		}
		Result result;
		fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualCountRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, count, ptr, ptr3);
		}
		return result;
	}

	internal unsafe Result GetColorContexts(int count, ComArray<ColorContext> colorContextsOut, out int actualCountRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualCountRef))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = colorContextsOut?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr, ptr);
		}
		return result;
	}

	internal unsafe void GetThumbnail(out BitmapSource thumbnailOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &zero);
		thumbnailOut = ((zero == IntPtr.Zero) ? null : new BitmapSource(zero));
		result.CheckError();
	}

	internal unsafe void GetFrameCount(out int countRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref countRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe BitmapFrameDecode GetFrame(int index)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		BitmapFrameDecode result2 = ((zero == IntPtr.Zero) ? null : new BitmapFrameDecode(zero));
		result.CheckError();
		return result2;
	}

	public BitmapDecoder(BitmapDecoderInfo bitmapDecoderInfo)
	{
		bitmapDecoderInfo.CreateInstance(this);
	}

	public BitmapDecoder(ImagingFactory factory, Guid containerFormatGuid)
	{
		factory.CreateDecoder(containerFormatGuid, null, this);
	}

	public BitmapDecoder(ImagingFactory factory, Guid containerFormatGuid, Guid guidVendorRef)
	{
		factory.CreateDecoder(containerFormatGuid, guidVendorRef, this);
	}

	public BitmapDecoder(ImagingFactory factory, IStream streamRef, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromStream_(ComStream.ToIntPtr(streamRef), null, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, Stream streamRef, DecodeOptions metadataOptions)
	{
		internalWICStream = new WICStream(factory, streamRef);
		factory.CreateDecoderFromStream_(ComStream.ToIntPtr(internalWICStream), null, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, IStream streamRef, Guid guidVendorRef, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromStream_(ComStream.ToIntPtr(streamRef), guidVendorRef, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, Stream streamRef, Guid guidVendorRef, DecodeOptions metadataOptions)
	{
		internalWICStream = new WICStream(factory, streamRef);
		factory.CreateDecoderFromStream_(ComStream.ToIntPtr(internalWICStream), guidVendorRef, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, string filename, DecodeOptions metadataOptions)
		: this(factory, filename, null, NativeFileAccess.Read, metadataOptions)
	{
	}

	public BitmapDecoder(ImagingFactory factory, string filename, NativeFileAccess desiredAccess, DecodeOptions metadataOptions)
		: this(factory, filename, null, desiredAccess, metadataOptions)
	{
	}

	public BitmapDecoder(ImagingFactory factory, string filename, Guid? guidVendorRef, NativeFileAccess desiredAccess, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromFilename(filename, guidVendorRef, (int)desiredAccess, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, FileStream fileStream, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromFileHandle(fileStream.SafeFileHandle.DangerousGetHandle(), null, metadataOptions, this);
	}

	public BitmapDecoder(ImagingFactory factory, FileStream fileStream, Guid guidVendorRef, DecodeOptions metadataOptions)
	{
		factory.CreateDecoderFromFileHandle(fileStream.SafeFileHandle.DangerousGetHandle(), guidVendorRef, metadataOptions, this);
	}

	public BitmapDecoderCapabilities QueryCapability(IStream stream)
	{
		return QueryCapability_(ComStream.ToIntPtr(stream));
	}

	public void Initialize(IStream stream, DecodeOptions cacheOptions)
	{
		if (internalWICStream != null)
		{
			throw new InvalidOperationException("This instance is already initialized with an existing stream");
		}
		Initialize_(ComStream.ToIntPtr(stream), cacheOptions);
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

	public Result TryGetColorContexts(ImagingFactory imagingFactory, out ColorContext[] colorContexts)
	{
		return ColorContextsHelper.TryGetColorContexts(GetColorContexts, imagingFactory, out colorContexts);
	}

	public ColorContext[] TryGetColorContexts(ImagingFactory imagingFactory)
	{
		return ColorContextsHelper.TryGetColorContexts(GetColorContexts, imagingFactory);
	}
}
