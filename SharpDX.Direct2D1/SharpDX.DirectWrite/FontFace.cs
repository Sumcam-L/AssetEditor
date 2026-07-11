using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

[Guid("5f49804d-7024-4d43-bfa9-d25984f53849")]
public class FontFace : ComObject
{
	public FontFaceType FaceType => GetFaceType();

	public int Index => GetIndex();

	public FontSimulations Simulations => GetSimulations();

	public Bool IsSymbolFont => IsSymbolFont_();

	public FontMetrics Metrics
	{
		get
		{
			GetMetrics(out var fontFaceMetrics);
			return fontFaceMetrics;
		}
	}

	public short GlyphCount => GetGlyphCount();

	public FontFace(Factory factory, FontFaceType fontFaceType, FontFile[] fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags)
	{
		factory.CreateFontFace(fontFaceType, fontFiles.Length, fontFiles, faceIndex, fontFaceSimulationFlags, this);
	}

	public FontFace(Font font)
	{
		font.CreateFontFace(this);
	}

	public GlyphMetrics[] GetDesignGlyphMetrics(short[] glyphIndices, bool isSideways)
	{
		GlyphMetrics[] array = new GlyphMetrics[glyphIndices.Length];
		GetDesignGlyphMetrics(glyphIndices, glyphIndices.Length, array, isSideways);
		return array;
	}

	public GlyphMetrics[] GetGdiCompatibleGlyphMetrics(float fontSize, float pixelsPerDip, Matrix? transform, bool useGdiNatural, short[] glyphIndices, bool isSideways)
	{
		GlyphMetrics[] array = new GlyphMetrics[glyphIndices.Length];
		GetGdiCompatibleGlyphMetrics(fontSize, pixelsPerDip, transform, useGdiNatural, glyphIndices, glyphIndices.Length, array, isSideways);
		return array;
	}

	public short[] GetGlyphIndices(int[] codePoints)
	{
		short[] array = new short[codePoints.Length];
		GetGlyphIndices(codePoints, codePoints.Length, array);
		return array;
	}

	public FontFile[] GetFiles()
	{
		int numberOfFiles = 0;
		GetFiles(ref numberOfFiles, null);
		FontFile[] array = new FontFile[numberOfFiles];
		GetFiles(ref numberOfFiles, array);
		return array;
	}

	public unsafe bool TryGetFontTable(int openTypeTableTag, out DataPointer tableData, out IntPtr tableContext)
	{
		tableData = DataPointer.Zero;
		IntPtr zero = IntPtr.Zero;
		TryGetFontTable(openTypeTableTag, new IntPtr(&zero), out var tableSize, out tableContext, out var exists);
		if (zero != IntPtr.Zero)
		{
			tableData = new DataPointer(zero, tableSize);
		}
		return exists;
	}

	public void GetGlyphRunOutline(float emSize, short[] glyphIndices, float[] glyphAdvances, GlyphOffset[] glyphOffsets, bool isSideways, bool isRightToLeft, SimplifiedGeometrySink geometrySink)
	{
		GetGlyphRunOutline_(geometrySink: (!(geometrySink is GeometrySink)) ? SimplifiedGeometrySinkShadow.ToIntPtr(geometrySink) : GeometrySinkShadow.ToIntPtr((GeometrySink)geometrySink), emSize: emSize, glyphIndices: glyphIndices, glyphAdvances: glyphAdvances, glyphOffsets: glyphOffsets, glyphCount: glyphIndices.Length, isSideways: isSideways, isRightToLeft: isRightToLeft);
	}

	public FontFace(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFace(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new FontFace(nativePointer);
		}
		return null;
	}

	internal unsafe FontFaceType GetFaceType()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontFaceType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFiles(ref int numberOfFiles, FontFile[] fontFiles)
	{
		IntPtr* ptr = stackalloc IntPtr[(fontFiles != null) ? fontFiles.Length : 0];
		Result result;
		fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref numberOfFiles))
		{
			void* nativePointer = _nativePointer;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, ptr2, (fontFiles == null) ? null : ptr);
		}
		if (fontFiles != null)
		{
			for (int i = 0; i < fontFiles.Length; i++)
			{
				fontFiles[i] = ((ptr[i] == IntPtr.Zero) ? null : new FontFile(ptr[i]));
			}
		}
		result.CheckError();
	}

	internal unsafe int GetIndex()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontSimulations GetSimulations()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontSimulations>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe Bool IsSymbolFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Bool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetMetrics(out FontMetrics fontFaceMetrics)
	{
		fontFaceMetrics = default(FontMetrics);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<FontMetrics, IntPtr>(ref fontFaceMetrics))
		{
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
	}

	internal unsafe short GetGlyphCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, short>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetDesignGlyphMetrics(short[] glyphIndices, int glyphCount, GlyphMetrics[] glyphMetrics, Bool isSideways)
	{
		Result result;
		fixed (IntPtr* ptr = glyphIndices)
		{
			fixed (IntPtr* ptr2 = glyphMetrics)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, Bool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr, glyphCount, ptr2, isSideways);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetGlyphIndices(int[] codePoints, int codePointCount, short[] glyphIndices)
	{
		Result result;
		fixed (IntPtr* ptr = codePoints)
		{
			fixed (IntPtr* ptr2 = glyphIndices)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr, codePointCount, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void TryGetFontTable(int openTypeTableTag, IntPtr tableData, out int tableSize, out IntPtr tableContext, out Bool exists)
	{
		exists = default(Bool);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref tableSize))
		{
			fixed (IntPtr* ptr2 = &tableContext)
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref exists))
				{
					result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, openTypeTableTag, (void*)tableData, ptr, ptr2, ptr3);
				}
			}
		}
		result.CheckError();
	}

	public unsafe void ReleaseFontTable(IntPtr tableContext)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)tableContext);
	}

	internal unsafe void GetGlyphRunOutline_(float emSize, short[] glyphIndices, float[] glyphAdvances, GlyphOffset[] glyphOffsets, int glyphCount, Bool isSideways, Bool isRightToLeft, IntPtr geometrySink)
	{
		Result result;
		fixed (IntPtr* ptr = glyphIndices)
		{
			fixed (IntPtr* ptr2 = glyphAdvances)
			{
				fixed (IntPtr* ptr3 = glyphOffsets)
				{
					result = ((delegate* unmanaged[Stdcall]<void*, float, void*, void*, void*, int, Bool, Bool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, emSize, ptr, ptr2, ptr3, glyphCount, isSideways, isRightToLeft, (void*)geometrySink);
				}
			}
		}
		result.CheckError();
	}

	public unsafe RenderingMode GetRecommendedRenderingMode(float emSize, float pixelsPerDip, MeasuringMode measuringMode, RenderingParams renderingParams)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = renderingParams?.NativePointer ?? IntPtr.Zero;
		RenderingMode result = default(RenderingMode);
		((Result)((delegate* unmanaged[Stdcall]<void*, float, float, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, (int)measuringMode, (void*)intPtr, &result)).CheckError();
		return result;
	}

	public unsafe FontMetrics GetGdiCompatibleMetrics(float emSize, float pixelsPerDip, Matrix3x2? transform)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		FontMetrics result = default(FontMetrics);
		void* nativePointer = _nativePointer;
		void* intPtr = (transform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, float, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, intPtr, &result)).CheckError();
		return result;
	}

	internal unsafe void GetGdiCompatibleGlyphMetrics(float emSize, float pixelsPerDip, Matrix3x2? transform, Bool useGdiNatural, short[] glyphIndices, int glyphCount, GlyphMetrics[] glyphMetrics, Bool isSideways)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (IntPtr* ptr = glyphIndices)
		{
			fixed (IntPtr* ptr2 = glyphMetrics)
			{
				void* nativePointer = _nativePointer;
				void* intPtr = (transform.HasValue ? (&value) : ((void*)IntPtr.Zero));
				result = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, Bool, void*, int, void*, Bool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, intPtr, useGdiNatural, ptr, glyphCount, ptr2, isSideways);
			}
		}
		result.CheckError();
	}
}
