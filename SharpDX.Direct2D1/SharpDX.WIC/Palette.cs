using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("00000040-a8f2-4877-ba0a-fd2b6645fb94")]
public class Palette : ComObject
{
	public BitmapPaletteType TypeInfo
	{
		get
		{
			GetTypeInfo(out var ePaletteTypeRef);
			return ePaletteTypeRef;
		}
	}

	public int ColorCount
	{
		get
		{
			GetColorCount(out var countRef);
			return countRef;
		}
	}

	public Bool IsBlackWhite
	{
		get
		{
			IsBlackWhite_(out var fIsBlackWhiteRef);
			return fIsBlackWhiteRef;
		}
	}

	public Bool IsGrayscale
	{
		get
		{
			IsGrayscale_(out var fIsGrayscaleRef);
			return fIsGrayscaleRef;
		}
	}

	public unsafe Color[] Colors
	{
		get
		{
			int colorCount = ColorCount;
			Color[] array = new Color[colorCount];
			int actualColorsRef;
			fixed (IntPtr* ptr = array)
			{
				GetColors(colorCount, (IntPtr)ptr, out actualColorsRef);
			}
			if (actualColorsRef == 0)
			{
				return new Color[0];
			}
			if (colorCount != actualColorsRef)
			{
				array = new Color[actualColorsRef];
				fixed (IntPtr* ptr2 = array)
				{
					GetColors(actualColorsRef, (IntPtr)ptr2, out actualColorsRef);
				}
			}
			return array;
		}
	}

	public Palette(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Palette(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Palette(nativePointer);
		}
		return null;
	}

	public unsafe void Initialize(BitmapPaletteType ePaletteType, Bool fAddTransparentColor)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, Bool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)ePaletteType, fAddTransparentColor)).CheckError();
	}

	internal unsafe void Initialize(IntPtr colorsRef, int count)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)colorsRef, count)).CheckError();
	}

	public unsafe void Initialize(BitmapSource surfaceRef, int count, Bool fAddTransparentColor)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = surfaceRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, Bool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, count, fAddTransparentColor)).CheckError();
	}

	public unsafe void Initialize(Palette paletteRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = paletteRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	internal unsafe void GetTypeInfo(out BitmapPaletteType ePaletteTypeRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<BitmapPaletteType, IntPtr>(ref ePaletteTypeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetColorCount(out int countRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref countRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetColors(int count, IntPtr colorsRef, out int actualColorsRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualColorsRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, count, (void*)colorsRef, ptr);
		}
		result.CheckError();
	}

	internal unsafe void IsBlackWhite_(out Bool fIsBlackWhiteRef)
	{
		fIsBlackWhiteRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fIsBlackWhiteRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void IsGrayscale_(out Bool fIsGrayscaleRef)
	{
		fIsGrayscaleRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fIsGrayscaleRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void HasAlpha(out Bool fHasAlphaRef)
	{
		fHasAlphaRef = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref fHasAlphaRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public Palette(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreatePalette(this);
	}

	public unsafe void Initialize(Color[] colors)
	{
		fixed (IntPtr* ptr = colors)
		{
			Initialize((IntPtr)ptr, colors.Length);
		}
	}

	public void Initialize(Color4[] colors)
	{
		Color[] array = new Color[colors.Length];
		for (int i = 0; i < array.Length; i++)
		{
			ref Color reference = ref array[i];
			reference = (Color)colors[i];
		}
		Initialize(array);
	}
}
