using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("a2296057-ea42-4099-983b-539fb6505426")]
public class Bitmap : Resource
{
	public Size2F DotsPerInch
	{
		get
		{
			GetDpi(out var dpiX, out var dpiY);
			return new Size2F(dpiX, dpiY);
		}
	}

	public Size2F Size => GetSize();

	public Size2 PixelSize => GetPixelSize();

	public PixelFormat PixelFormat => GetPixelFormat();

	public Bitmap(RenderTarget renderTarget, Size2 size)
		: this(renderTarget, size, DataPointer.Zero, 0, new BitmapProperties(new PixelFormat(Format.Unknown, AlphaMode.Unknown)))
	{
	}

	public Bitmap(RenderTarget renderTarget, Size2 size, BitmapProperties bitmapProperties)
		: this(renderTarget, size, DataPointer.Zero, 0, bitmapProperties)
	{
	}

	public Bitmap(RenderTarget renderTarget, Size2 size, DataPointer dataPointer, int pitch)
		: this(renderTarget, size, dataPointer, pitch, new BitmapProperties(new PixelFormat(Format.Unknown, AlphaMode.Unknown)))
	{
	}

	public Bitmap(RenderTarget renderTarget, Size2 size, DataPointer dataPointer, int pitch, BitmapProperties bitmapProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateBitmap(size, (dataPointer == DataPointer.Zero) ? IntPtr.Zero : dataPointer.Pointer, pitch, bitmapProperties, this);
	}

	public Bitmap(RenderTarget renderTarget, Bitmap bitmap)
		: this(renderTarget, bitmap, null)
	{
	}

	public Bitmap(RenderTarget renderTarget, Bitmap bitmap, BitmapProperties? bitmapProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateSharedBitmap(Utilities.GetGuidFromType(typeof(Bitmap)), bitmap.NativePointer, bitmapProperties, this);
	}

	public Bitmap(RenderTarget renderTarget, Surface surface)
		: this(renderTarget, surface, null)
	{
	}

	public Bitmap(RenderTarget renderTarget, Surface surface, BitmapProperties? bitmapProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateSharedBitmap(Utilities.GetGuidFromType(typeof(Surface)), surface.NativePointer, bitmapProperties, this);
	}

	public Bitmap(RenderTarget renderTarget, BitmapLock bitmapLock)
		: this(renderTarget, bitmapLock, null)
	{
	}

	public Bitmap(RenderTarget renderTarget, BitmapLock bitmapLock, BitmapProperties? bitmapProperties)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateSharedBitmap(Utilities.GetGuidFromType(typeof(BitmapLock)), bitmapLock.NativePointer, bitmapProperties, this);
	}

	public unsafe static Bitmap New<T>(RenderTarget renderTarget, Size2 size, T[] pixelDatas, BitmapProperties bitmapProperties) where T : struct
	{
		int num = pixelDatas.Length * Utilities.SizeOf<T>();
		int num2 = size.Width * size.Height * (int)FormatHelper.SizeOfInBytes(bitmapProperties.PixelFormat.Format);
		if (num != num2)
		{
			throw new ArgumentException("Invalid size of pixelDatas. Must be equal to sizeof(T) == sizeof(PixelFormat.Format) and  Width * Height elements");
		}
		Size2 size2 = size;
		fixed (T* ptr = &pixelDatas[0])
		{
			return new Bitmap(renderTarget, size2, new DataPointer((IntPtr)ptr, num), size.Width * (int)FormatHelper.SizeOfInBytes(bitmapProperties.PixelFormat.Format), bitmapProperties);
		}
	}

	public static Bitmap FromWicBitmap(RenderTarget renderTarget, BitmapSource wicBitmapSource)
	{
		renderTarget.CreateBitmapFromWicBitmap(wicBitmapSource, null, out var bitmap);
		return bitmap;
	}

	public static Bitmap FromWicBitmap(RenderTarget renderTarget, BitmapSource wicBitmap, BitmapProperties bitmapProperties)
	{
		renderTarget.CreateBitmapFromWicBitmap(wicBitmap, bitmapProperties, out var bitmap);
		return bitmap;
	}

	public void CopyFromBitmap(Bitmap sourceBitmap)
	{
		CopyFromBitmap(null, sourceBitmap, null);
	}

	public void CopyFromBitmap(Bitmap sourceBitmap, Point destinationPoint)
	{
		CopyFromBitmap(destinationPoint, sourceBitmap, null);
	}

	public void CopyFromBitmap(Bitmap sourceBitmap, Point destinationPoint, Rectangle sourceArea)
	{
		CopyFromBitmap(destinationPoint, sourceBitmap, sourceArea);
	}

	public void CopyFromMemory(IntPtr pointer, int pitch)
	{
		CopyFromMemory(null, pointer, pitch);
	}

	public unsafe void CopyFromMemory(byte[] memory, int pitch)
	{
		fixed (IntPtr* value = &System.Runtime.CompilerServices.Unsafe.As<byte, IntPtr>(ref memory[0]))
		{
			CopyFromMemory(null, new IntPtr(value), pitch);
		}
	}

	public unsafe void CopyFromMemory<T>(T[] memory, int pitch = 0) where T : struct
	{
		if (pitch == 0)
		{
			pitch = (int)(Size.Width * (float)Utilities.SizeOf<T>() / DotsPerInch.Width);
		}
		fixed (T* ptr = &memory[0])
		{
			CopyFromMemory(null, (IntPtr)ptr, pitch);
		}
	}

	public void CopyFromMemory(IntPtr pointer, int pitch, Rectangle destinationArea)
	{
		CopyFromMemory(destinationArea, pointer, pitch);
	}

	public unsafe void CopyFromMemory(byte[] memory, int pitch, Rectangle destinationArea)
	{
		fixed (IntPtr* value = &System.Runtime.CompilerServices.Unsafe.As<byte, IntPtr>(ref memory[0]))
		{
			CopyFromMemory(destinationArea, new IntPtr(value), pitch);
		}
	}

	public unsafe void CopyFromMemory<T>(T[] memory, int pitch, Rectangle destinationArea) where T : struct
	{
		Rectangle? dstRect = destinationArea;
		fixed (T* ptr = &memory[0])
		{
			CopyFromMemory(dstRect, (IntPtr)ptr, pitch);
		}
	}

	public void CopyFromRenderTarget(RenderTarget renderTarget)
	{
		CopyFromRenderTarget(null, renderTarget, null);
	}

	public void CopyFromRenderTarget(RenderTarget renderTarget, Point destinationPoint)
	{
		CopyFromRenderTarget(destinationPoint, renderTarget, null);
	}

	public void CopyFromRenderTarget(RenderTarget renderTarget, Point destinationPoint, Rectangle sourceArea)
	{
		CopyFromRenderTarget(destinationPoint, renderTarget, sourceArea);
	}

	public void CopyFromStream(Stream stream, int pitch, int length)
	{
		CopyFromMemory(Utilities.ReadStream(stream, ref length), pitch);
	}

	public void CopyFromStream(Stream stream, int pitch, int length, Rectangle destinationArea)
	{
		CopyFromMemory(Utilities.ReadStream(stream, ref length), pitch, destinationArea);
	}

	public Bitmap(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Bitmap(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Bitmap(nativePointer);
		}
		return null;
	}

	internal unsafe Size2F GetSize()
	{
		Size2F result = default(Size2F);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe Size2 GetPixelSize()
	{
		Size2 result = default(Size2);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe PixelFormat GetPixelFormat()
	{
		PixelFormat result = default(PixelFormat);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	internal unsafe void GetDpi(out float dpiX, out float dpiY)
	{
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref dpiX))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref dpiY))
			{
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2);
			}
		}
	}

	internal unsafe void CopyFromBitmap(Point? destPoint, Bitmap bitmap, Rectangle? srcRect)
	{
		Point value = default(Point);
		if (destPoint.HasValue)
		{
			value = destPoint.Value;
		}
		Rectangle value2 = default(Rectangle);
		if (srcRect.HasValue)
		{
			value2 = srcRect.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (destPoint.HasValue ? (&value) : ((void*)IntPtr.Zero));
		void* intPtr2 = (void*)(bitmap?.NativePointer ?? IntPtr.Zero);
		void* intPtr3 = (srcRect.HasValue ? (&value2) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3)).CheckError();
	}

	internal unsafe void CopyFromRenderTarget(Point? destPoint, RenderTarget renderTarget, Rectangle? srcRect)
	{
		Point value = default(Point);
		if (destPoint.HasValue)
		{
			value = destPoint.Value;
		}
		Rectangle value2 = default(Rectangle);
		if (srcRect.HasValue)
		{
			value2 = srcRect.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (destPoint.HasValue ? (&value) : ((void*)IntPtr.Zero));
		void* intPtr2 = (void*)(renderTarget?.NativePointer ?? IntPtr.Zero);
		void* intPtr3 = (srcRect.HasValue ? (&value2) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3)).CheckError();
	}

	internal unsafe void CopyFromMemory(Rectangle? dstRect, IntPtr srcData, int pitch)
	{
		Rectangle value = default(Rectangle);
		if (dstRect.HasValue)
		{
			value = dstRect.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (dstRect.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)srcData, pitch)).CheckError();
	}
}
