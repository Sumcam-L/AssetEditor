using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

[Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48")]
public class Factory : ComObject
{
	private readonly List<FontCollectionLoader> _fontCollectionLoaderCallbacks = new List<FontCollectionLoader>();

	private readonly List<FontFileLoader> _fontFileLoaderCallbacks = new List<FontFileLoader>();

	public GdiInterop GdiInterop
	{
		get
		{
			GetGdiInterop(out var gdiInterop);
			return gdiInterop;
		}
	}

	internal FontCollectionLoader FindRegisteredFontCollectionLoaderCallback(FontCollectionLoader loader)
	{
		foreach (FontCollectionLoader fontCollectionLoaderCallback in _fontCollectionLoaderCallbacks)
		{
			if (fontCollectionLoaderCallback == loader)
			{
				return fontCollectionLoaderCallback;
			}
		}
		return null;
	}

	internal FontFileLoader FindRegisteredFontFileLoaderCallback(FontFileLoader loader)
	{
		foreach (FontFileLoader fontFileLoaderCallback in _fontFileLoaderCallbacks)
		{
			if (fontFileLoaderCallback == loader)
			{
				return fontFileLoaderCallback;
			}
		}
		return null;
	}

	public Factory()
		: this(FactoryType.Shared)
	{
	}

	public Factory(FactoryType factoryType)
		: base(IntPtr.Zero)
	{
		DWrite.CreateFactory(factoryType, Utilities.GetGuidFromType(typeof(Factory)), this);
	}

	public void RegisterFontCollectionLoader(FontCollectionLoader fontCollectionLoader)
	{
		RegisterFontCollectionLoader_(FontCollectionLoaderShadow.ToIntPtr(this, fontCollectionLoader));
		_fontCollectionLoaderCallbacks.Add(fontCollectionLoader);
	}

	public void UnregisterFontCollectionLoader(FontCollectionLoader fontCollectionLoader)
	{
		if (!_fontCollectionLoaderCallbacks.Contains(fontCollectionLoader))
		{
			throw new ArgumentException("This font collection loader is not registered", "fontCollectionLoader");
		}
		UnregisterFontCollectionLoader_(FontCollectionLoaderShadow.ToIntPtr(fontCollectionLoader));
		_fontCollectionLoaderCallbacks.Remove(fontCollectionLoader);
	}

	public void RegisterFontFileLoader(FontFileLoader fontFileLoader)
	{
		RegisterFontFileLoader_(FontFileLoaderShadow.ToIntPtr(fontFileLoader));
		_fontFileLoaderCallbacks.Add(fontFileLoader);
	}

	public void UnregisterFontFileLoader(FontFileLoader fontFileLoader)
	{
		if (!_fontFileLoaderCallbacks.Contains(fontFileLoader))
		{
			throw new ArgumentException("This font file loader is not registered", "fontFileLoader");
		}
		UnregisterFontFileLoader_(FontFileLoaderShadow.ToIntPtr(fontFileLoader));
		_fontFileLoaderCallbacks.Remove(fontFileLoader);
	}

	public Factory(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Factory(nativePointer);
		}
		return null;
	}

	public unsafe FontCollection GetSystemFontCollection(Bool checkForUpdates)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, Bool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero, checkForUpdates);
		FontCollection result2 = ((zero == IntPtr.Zero) ? null : new FontCollection(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void CreateCustomFontCollection_(IntPtr collectionLoader, IntPtr collectionKey, int collectionKeySize, FontCollection fontCollection)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)collectionLoader, (void*)collectionKey, collectionKeySize, &zero);
		fontCollection.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void RegisterFontCollectionLoader_(IntPtr fontCollectionLoader)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)fontCollectionLoader)).CheckError();
	}

	internal unsafe void UnregisterFontCollectionLoader_(IntPtr fontCollectionLoader)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)fontCollectionLoader)).CheckError();
	}

	internal unsafe void CreateFontFileReference(string filePath, long? lastWriteTime, FontFile fontFile)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(filePath);
		long value = default(long);
		if (lastWriteTime.HasValue)
		{
			value = lastWriteTime.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr2 = (void*)intPtr;
		void* intPtr3 = (lastWriteTime.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr2, intPtr3, &zero);
		Marshal.FreeHGlobal(intPtr);
		fontFile.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateCustomFontFileReference_(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, IntPtr fontFileLoader, FontFile fontFile)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, (void*)fontFileLoader, &zero);
		fontFile.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateFontFace(FontFaceType fontFaceType, int numberOfFiles, FontFile[] fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags, FontFace fontFace)
	{
		IntPtr* ptr = null;
		if (fontFiles != null)
		{
			IntPtr* ptr2 = stackalloc IntPtr[fontFiles.Length];
			ptr = ptr2;
			for (int i = 0; i < fontFiles.Length; i++)
			{
				ptr[i] = ((fontFiles[i] == null) ? IntPtr.Zero : fontFiles[i].NativePointer);
			}
		}
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (int)fontFaceType, numberOfFiles, ptr, faceIndex, (int)fontFaceSimulationFlags, &zero);
		fontFace.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateFontFace(FontFaceType fontFaceType, int numberOfFiles, ComArray<FontFile> fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags, FontFace fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = fontFiles?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (int)fontFaceType, numberOfFiles, (void*)intPtr, faceIndex, (int)fontFaceSimulationFlags, &zero);
		fontFace.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRenderingParams(RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		renderingParams.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateMonitorRenderingParams(IntPtr monitor, RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)monitor, &zero);
		renderingParams.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateCustomRenderingParams(float gamma, float enhancedContrast, float clearTypeLevel, PixelGeometry pixelGeometry, RenderingMode renderingMode, RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, gamma, enhancedContrast, clearTypeLevel, (int)pixelGeometry, (int)renderingMode, &zero);
		renderingParams.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void RegisterFontFileLoader_(IntPtr fontFileLoader)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileLoader)).CheckError();
	}

	internal unsafe void UnregisterFontFileLoader_(IntPtr fontFileLoader)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileLoader)).CheckError();
	}

	internal unsafe void CreateTextFormat(string fontFamilyName, FontCollection fontCollection, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize, string localeName, TextFormat textFormat)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(fontFamilyName);
		IntPtr intPtr2 = Utilities.StringToHGlobalUni(localeName);
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr3 = (void*)intPtr;
		IntPtr intPtr4 = fontCollection?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, int, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, intPtr3, (void*)intPtr4, (int)fontWeight, (int)fontStyle, (int)fontStretch, fontSize, (void*)intPtr2, &zero);
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		textFormat.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTypography(Typography typography)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &zero);
		typography.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void GetGdiInterop(out GdiInterop gdiInterop)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &zero);
		gdiInterop = ((zero == IntPtr.Zero) ? null : new GdiInterop(zero));
		result.CheckError();
	}

	internal unsafe void CreateTextLayout(string text, int stringLength, TextFormat textFormat, float maxWidth, float maxHeight, TextLayout textLayout)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(text);
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr2 = (void*)intPtr;
		IntPtr intPtr3 = textFormat?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(nativePointer, intPtr2, stringLength, (void*)intPtr3, maxWidth, maxHeight, &zero);
		Marshal.FreeHGlobal(intPtr);
		textLayout.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGdiCompatibleTextLayout(string text, int stringLength, TextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, Matrix3x2? transform, Bool useGdiNatural, TextLayout textLayout)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(text);
		Matrix3x2 value = default(Matrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		void* intPtr2 = (void*)intPtr;
		void* intPtr3 = (void*)(textFormat?.NativePointer ?? IntPtr.Zero);
		void* intPtr4 = (transform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, float, float, float, void*, Bool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(nativePointer, intPtr2, stringLength, intPtr3, layoutWidth, layoutHeight, pixelsPerDip, intPtr4, useGdiNatural, &zero);
		Marshal.FreeHGlobal(intPtr);
		textLayout.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateEllipsisTrimmingSign(TextFormat textFormat, InlineObject trimmingSign)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = textFormat?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, &zero);
		((InlineObjectNative)trimmingSign).NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTextAnalyzer(TextAnalyzer textAnalyzer)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, &zero);
		textAnalyzer.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateNumberSubstitution(NumberSubstitutionMethod substitutionMethod, string localeName, Bool ignoreUserOverride, NumberSubstitution numberSubstitution)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(localeName);
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, Bool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (int)substitutionMethod, (void*)intPtr, ignoreUserOverride, &zero);
		Marshal.FreeHGlobal(intPtr);
		numberSubstitution.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGlyphRunAnalysis(GlyphRun glyphRun, float pixelsPerDip, Matrix3x2? transform, RenderingMode renderingMode, MeasuringMode measuringMode, float baselineOriginX, float baselineOriginY, GlyphRunAnalysis glyphRunAnalysis)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		Matrix3x2 value = default(Matrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		GlyphRun.__Native* num = &@ref;
		void* intPtr = (transform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int, int, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(nativePointer, num, pixelsPerDip, intPtr, (int)renderingMode, (int)measuringMode, baselineOriginX, baselineOriginY, &zero);
		glyphRun.__MarshalFree(ref @ref);
		glyphRunAnalysis.NativePointer = zero;
		result.CheckError();
	}
}
