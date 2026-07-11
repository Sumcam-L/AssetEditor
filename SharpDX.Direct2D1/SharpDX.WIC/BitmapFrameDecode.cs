using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("3B16811B-6A43-4ec9-A813-3D930C13B940")]
public class BitmapFrameDecode : BitmapSource
{
	public MetadataQueryReader MetadataQueryReader
	{
		get
		{
			GetMetadataQueryReader(out var metadataQueryReaderOut);
			return metadataQueryReaderOut;
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

	[Obsolete("Use TryGetColorContexts instead")]
	public ColorContext[] ColorContexts => new ColorContext[0];

	public BitmapFrameDecode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapFrameDecode(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new BitmapFrameDecode(nativePointer);
		}
		return null;
	}

	internal unsafe void GetMetadataQueryReader(out MetadataQueryReader metadataQueryReaderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		metadataQueryReaderOut = ((zero == IntPtr.Zero) ? null : new MetadataQueryReader(zero));
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
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, count, ptr, ptr3);
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
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, count, (void*)intPtr, ptr);
		}
		return result;
	}

	internal unsafe void GetThumbnail(out BitmapSource thumbnailOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		thumbnailOut = ((zero == IntPtr.Zero) ? null : new BitmapSource(zero));
		result.CheckError();
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
