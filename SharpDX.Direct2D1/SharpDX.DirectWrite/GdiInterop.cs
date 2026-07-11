using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("1edd9491-9853-4299-898f-6432983b6f3a")]
public class GdiInterop : ComObject
{
	public unsafe Font FromLogFont(object logFont)
	{
		int num = Marshal.SizeOf(logFont);
		byte* value = stackalloc byte[(int)(uint)num];
		Marshal.StructureToPtr(logFont, new IntPtr(value), fDeleteOld: false);
		CreateFontFromLOGFONT(new IntPtr(value), out var font);
		return font;
	}

	public unsafe bool ToLogFont(Font font, object logFont)
	{
		int num = Marshal.SizeOf(logFont);
		byte* value = stackalloc byte[(int)(uint)num];
		ConvertFontToLOGFONT(font, new IntPtr(value), out var isSystemFont);
		Marshal.PtrToStructure(new IntPtr(value), logFont);
		return isSystemFont;
	}

	public Font FromSystemDrawingFont(System.Drawing.Font font)
	{
		Win32Native.LogFont logFont = new Win32Native.LogFont();
		font.ToLogFont(logFont);
		return FromLogFont(logFont);
	}

	public bool ToSystemDrawingFont(Font d2dFont, out System.Drawing.Font font)
	{
		Win32Native.LogFont logFont = new Win32Native.LogFont();
		bool result = ToLogFont(d2dFont, logFont);
		font = System.Drawing.Font.FromLogFont(logFont);
		return result;
	}

	public GdiInterop(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiInterop(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new GdiInterop(nativePointer);
		}
		return null;
	}

	internal unsafe void CreateFontFromLOGFONT(IntPtr logFont, out Font font)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)logFont, &zero);
		font = ((zero == IntPtr.Zero) ? null : new Font(zero));
		result.CheckError();
	}

	internal unsafe void ConvertFontToLOGFONT(Font font, IntPtr logFont, out Bool isSystemFont)
	{
		isSystemFont = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref isSystemFont))
		{
			void* nativePointer = _nativePointer;
			IntPtr intPtr = font?.NativePointer ?? IntPtr.Zero;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, (void*)logFont, ptr);
		}
		result.CheckError();
	}

	internal unsafe void ConvertFontFaceToLOGFONT(FontFace font, IntPtr logFont)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = font?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, (void*)logFont)).CheckError();
	}

	public unsafe FontFace CreateFontFaceFromHdc(IntPtr hdc)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)hdc, &zero);
		FontFace result2 = ((zero == IntPtr.Zero) ? null : new FontFace(zero));
		result.CheckError();
		return result2;
	}

	public unsafe BitmapRenderTarget CreateBitmapRenderTarget(IntPtr hdc, int width, int height)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)hdc, width, height, &zero);
		BitmapRenderTarget result2 = ((zero == IntPtr.Zero) ? null : new BitmapRenderTarget(zero));
		result.CheckError();
		return result2;
	}
}
