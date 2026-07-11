using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("b7e6163e-7f46-43b4-84b3-e4e6249c365d")]
public class TextAnalyzer : ComObject
{
	public TextAnalyzer(Factory factory)
	{
		factory.CreateTextAnalyzer(this);
	}

	public void AnalyzeScript(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		AnalyzeScript__(TextAnalysisSourceShadow.ToIntPtr(analysisSource), textPosition, textLength, TextAnalysisSinkShadow.ToIntPtr(analysisSink));
	}

	public void AnalyzeBidi(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		AnalyzeBidi__(TextAnalysisSourceShadow.ToIntPtr(analysisSource), textPosition, textLength, TextAnalysisSinkShadow.ToIntPtr(analysisSink));
	}

	public void AnalyzeNumberSubstitution(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		AnalyzeNumberSubstitution__(TextAnalysisSourceShadow.ToIntPtr(analysisSource), textPosition, textLength, TextAnalysisSinkShadow.ToIntPtr(analysisSink));
	}

	public void AnalyzeLineBreakpoints(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		AnalyzeLineBreakpoints__(TextAnalysisSourceShadow.ToIntPtr(analysisSource), textPosition, textLength, TextAnalysisSinkShadow.ToIntPtr(analysisSink));
	}

	public void GetGlyphs(string textString, int textLength, FontFace fontFace, bool isSideways, bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, NumberSubstitution numberSubstitution, FontFeature[][] features, int[] featureRangeLengths, int maxGlyphCount, short[] clusterMap, ShapingTextProperties[] textProps, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, out int actualGlyphCount)
	{
		IntPtr intPtr = AllocateFeatures(features);
		try
		{
			GetGlyphs(textString, textLength, fontFace, isSideways, isRightToLeft, scriptAnalysis, localeName, numberSubstitution, intPtr, featureRangeLengths, (featureRangeLengths != null) ? featureRangeLengths.Length : 0, maxGlyphCount, clusterMap, textProps, glyphIndices, glyphProps, out actualGlyphCount);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	public void GetGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, bool isSideways, bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, FontFeature[][] features, int[] featureRangeLengths, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr intPtr = AllocateFeatures(features);
		try
		{
			GetGlyphPlacements(textString, clusterMap, textProps, textLength, glyphIndices, glyphProps, glyphCount, fontFace, fontEmSize, isSideways, isRightToLeft, scriptAnalysis, localeName, intPtr, featureRangeLengths, (featureRangeLengths != null) ? featureRangeLengths.Length : 0, glyphAdvances, glyphOffsets);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	public void GetGdiCompatibleGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, float pixelsPerDip, Matrix3x2? transform, bool useGdiNatural, bool isSideways, bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, FontFeature[][] features, int[] featureRangeLengths, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr intPtr = AllocateFeatures(features);
		try
		{
			GetGdiCompatibleGlyphPlacements(textString, clusterMap, textProps, textLength, glyphIndices, glyphProps, glyphCount, fontFace, fontEmSize, pixelsPerDip, transform, useGdiNatural, isSideways, isRightToLeft, scriptAnalysis, localeName, intPtr, featureRangeLengths, (featureRangeLengths != null) ? featureRangeLengths.Length : 0, glyphAdvances, glyphOffsets);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	private unsafe static IntPtr AllocateFeatures(FontFeature[][] features)
	{
		byte* ptr = null;
		if (features != null && features.Length > 0)
		{
			int num = sizeof(IntPtr) * features.Length;
			int num2 = num + sizeof(TypographicFeatures) * features.Length;
			foreach (FontFeature[] array in features)
			{
				if (array == null)
				{
					throw new ArgumentNullException("features", "FontFeature[] inside features array cannot be null.");
				}
				num2 += sizeof(FontFeature) * array.Length;
			}
			ptr = (byte*)(void*)Marshal.AllocHGlobal(num2);
			TypographicFeatures* ptr2 = (TypographicFeatures*)(ptr + num);
			FontFeature* ptr3 = (FontFeature*)(ptr2 + features.Length);
			for (int j = 0; j < features.Length; j++)
			{
				*(TypographicFeatures**)(ptr + (nint)j * (nint)sizeof(void*)) = ptr2;
				FontFeature[] array2 = features[j];
				ptr2->Features = (IntPtr)ptr3;
				ptr2->FeatureCount = array2.Length;
				ptr2++;
				for (int k = 0; k < array2.Length; k++)
				{
					*ptr3 = array2[k];
					ptr3++;
				}
			}
		}
		return (IntPtr)ptr;
	}

	public TextAnalyzer(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextAnalyzer(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new TextAnalyzer(nativePointer);
		}
		return null;
	}

	internal unsafe void AnalyzeScript__(IntPtr analysisSource, int textPosition, int textLength, IntPtr analysisSink)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)analysisSource, textPosition, textLength, (void*)analysisSink)).CheckError();
	}

	internal unsafe void AnalyzeBidi__(IntPtr analysisSource, int textPosition, int textLength, IntPtr analysisSink)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)analysisSource, textPosition, textLength, (void*)analysisSink)).CheckError();
	}

	internal unsafe void AnalyzeNumberSubstitution__(IntPtr analysisSource, int textPosition, int textLength, IntPtr analysisSink)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)analysisSource, textPosition, textLength, (void*)analysisSink)).CheckError();
	}

	internal unsafe void AnalyzeLineBreakpoints__(IntPtr analysisSource, int textPosition, int textLength, IntPtr analysisSink)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)analysisSource, textPosition, textLength, (void*)analysisSink)).CheckError();
	}

	internal unsafe void GetGlyphs(string textString, int textLength, FontFace fontFace, Bool isSideways, Bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, NumberSubstitution numberSubstitution, IntPtr features, int[] featureRangeLengths, int featureRanges, int maxGlyphCount, short[] clusterMap, ShapingTextProperties[] textProps, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, out int actualGlyphCount)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(textString);
		IntPtr intPtr2 = Utilities.StringToHGlobalUni(localeName);
		Result result;
		fixed (IntPtr* ptr = featureRangeLengths)
		{
			fixed (IntPtr* ptr2 = clusterMap)
			{
				fixed (IntPtr* ptr3 = textProps)
				{
					fixed (IntPtr* ptr4 = glyphIndices)
					{
						fixed (IntPtr* ptr5 = glyphProps)
						{
							fixed (IntPtr* ptr6 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualGlyphCount))
							{
								void* nativePointer = _nativePointer;
								void* intPtr3 = (void*)intPtr;
								void* intPtr4 = (void*)(fontFace?.NativePointer ?? IntPtr.Zero);
								ScriptAnalysis* num = &scriptAnalysis;
								void* intPtr5 = (void*)intPtr2;
								IntPtr intPtr6 = numberSubstitution?.NativePointer ?? IntPtr.Zero;
								result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, Bool, Bool, void*, void*, void*, void*, void*, int, int, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr3, textLength, intPtr4, isSideways, isRightToLeft, num, intPtr5, (void*)intPtr6, (void*)features, ptr, featureRanges, maxGlyphCount, ptr2, ptr3, ptr4, ptr5, ptr6);
							}
						}
					}
				}
			}
		}
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		result.CheckError();
	}

	internal unsafe void GetGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, Bool isSideways, Bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, IntPtr features, int[] featureRangeLengths, int featureRanges, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(textString);
		IntPtr intPtr2 = Utilities.StringToHGlobalUni(localeName);
		Result result;
		fixed (IntPtr* ptr = clusterMap)
		{
			fixed (IntPtr* ptr2 = textProps)
			{
				fixed (IntPtr* ptr3 = glyphIndices)
				{
					fixed (IntPtr* ptr4 = glyphProps)
					{
						fixed (IntPtr* ptr5 = featureRangeLengths)
						{
							fixed (IntPtr* ptr6 = glyphAdvances)
							{
								fixed (IntPtr* ptr7 = glyphOffsets)
								{
									void* nativePointer = _nativePointer;
									void* intPtr3 = (void*)intPtr;
									IntPtr intPtr4 = fontFace?.NativePointer ?? IntPtr.Zero;
									result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, void*, int, void*, float, Bool, Bool, void*, void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr3, ptr, ptr2, textLength, ptr3, ptr4, glyphCount, (void*)intPtr4, fontEmSize, isSideways, isRightToLeft, &scriptAnalysis, (void*)intPtr2, (void*)features, ptr5, featureRanges, ptr6, ptr7);
								}
							}
						}
					}
				}
			}
		}
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		result.CheckError();
	}

	internal unsafe void GetGdiCompatibleGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, float pixelsPerDip, Matrix3x2? transform, Bool useGdiNatural, Bool isSideways, Bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, IntPtr features, int[] featureRangeLengths, int featureRanges, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(textString);
		Matrix3x2 value = default(Matrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		IntPtr intPtr2 = Utilities.StringToHGlobalUni(localeName);
		Result result;
		fixed (IntPtr* ptr = clusterMap)
		{
			fixed (IntPtr* ptr2 = textProps)
			{
				fixed (IntPtr* ptr3 = glyphIndices)
				{
					fixed (IntPtr* ptr4 = glyphProps)
					{
						fixed (IntPtr* ptr5 = featureRangeLengths)
						{
							fixed (IntPtr* ptr6 = glyphAdvances)
							{
								fixed (IntPtr* ptr7 = glyphOffsets)
								{
									void* nativePointer = _nativePointer;
									void* intPtr3 = (void*)intPtr;
									void* intPtr4 = (void*)(fontFace?.NativePointer ?? IntPtr.Zero);
									void* intPtr5 = (transform.HasValue ? (&value) : ((void*)IntPtr.Zero));
									result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, void*, int, void*, float, float, void*, Bool, Bool, Bool, void*, void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, intPtr3, ptr, ptr2, textLength, ptr3, ptr4, glyphCount, intPtr4, fontEmSize, pixelsPerDip, intPtr5, useGdiNatural, isSideways, isRightToLeft, &scriptAnalysis, (void*)intPtr2, (void*)features, ptr5, featureRanges, ptr6, ptr7);
								}
							}
						}
					}
				}
			}
		}
		Marshal.FreeHGlobal(intPtr);
		Marshal.FreeHGlobal(intPtr2);
		result.CheckError();
	}
}
