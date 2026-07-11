using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("53737037-6d14-410b-9bfe-0b182bb70961")]
public class TextLayout : TextFormat
{
	public float MaxWidth
	{
		get
		{
			return GetMaxWidth();
		}
		set
		{
			SetMaxWidth(value);
		}
	}

	public float MaxHeight
	{
		get
		{
			return GetMaxHeight();
		}
		set
		{
			SetMaxHeight(value);
		}
	}

	public TextMetrics Metrics
	{
		get
		{
			GetMetrics(out var textMetrics);
			return textMetrics;
		}
	}

	public OverhangMetrics OverhangMetrics
	{
		get
		{
			GetOverhangMetrics(out var overhangs);
			return overhangs;
		}
	}

	public TextLayout(Factory factory, string text, TextFormat textFormat, float maxWidth, float maxHeight)
		: base(IntPtr.Zero)
	{
		factory.CreateTextLayout(text, text.Length, textFormat, maxWidth, maxHeight, this);
	}

	public TextLayout(Factory factory, string text, TextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, bool useGdiNatural)
		: this(factory, text, textFormat, layoutWidth, layoutHeight, pixelsPerDip, null, useGdiNatural)
	{
	}

	public TextLayout(Factory factory, string text, TextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, Matrix3x2? transform, bool useGdiNatural)
		: base(IntPtr.Zero)
	{
		factory.CreateGdiCompatibleTextLayout(text, text.Length, textFormat, layoutWidth, layoutHeight, pixelsPerDip, transform, useGdiNatural, this);
	}

	public void Draw(TextRenderer renderer, float originX, float originY)
	{
		Draw(null, renderer, originX, originY);
	}

	public void Draw(object clientDrawingContext, TextRenderer renderer, float originX, float originY)
	{
		GCHandle value = GCHandle.Alloc(clientDrawingContext);
		try
		{
			Draw_(GCHandle.ToIntPtr(value), TextRendererShadow.ToIntPtr(renderer), originX, originY);
		}
		finally
		{
			if (value.IsAllocated)
			{
				value.Free();
			}
		}
	}

	public ClusterMetrics[] GetClusterMetrics()
	{
		ClusterMetrics[] array = new ClusterMetrics[0];
		int maxClusterCount = 0;
		int actualClusterCount = 0;
		GetClusterMetrics(array, maxClusterCount, out actualClusterCount);
		if (actualClusterCount > 0)
		{
			array = new ClusterMetrics[actualClusterCount];
			GetClusterMetrics(array, actualClusterCount, out actualClusterCount);
		}
		return array;
	}

	public void SetDrawingEffect(ComObject drawingEffect, TextRange textRange)
	{
		IntPtr iUnknownForObject = Utilities.GetIUnknownForObject(drawingEffect);
		SetDrawingEffect_(iUnknownForObject, textRange);
		if (iUnknownForObject != IntPtr.Zero)
		{
			Marshal.Release(iUnknownForObject);
		}
	}

	public ComObject GetDrawingEffect(int currentPosition)
	{
		TextRange textRange;
		return GetDrawingEffect(currentPosition, out textRange);
	}

	public ComObject GetDrawingEffect(int currentPosition, out TextRange textRange)
	{
		return (ComObject)Utilities.GetObjectForIUnknown(GetDrawingEffect_(currentPosition, out textRange));
	}

	public FontCollection GetFontCollection(int currentPosition)
	{
		TextRange textRange;
		return GetFontCollection(currentPosition, out textRange);
	}

	public string GetFontFamilyName(int currentPosition)
	{
		TextRange textRange;
		return GetFontFamilyName(currentPosition, out textRange);
	}

	public unsafe string GetFontFamilyName(int currentPosition, out TextRange textRange)
	{
		GetFontFamilyNameLength(currentPosition, out var nameLength, out textRange);
		char* value = stackalloc char[nameLength + 1];
		GetFontFamilyName(currentPosition, new IntPtr(value), nameLength + 1, out textRange);
		return new string(value, 0, nameLength);
	}

	public float GetFontSize(int currentPosition)
	{
		TextRange textRange;
		return GetFontSize(currentPosition, out textRange);
	}

	public FontStretch GetFontStretch(int currentPosition)
	{
		TextRange textRange;
		return GetFontStretch(currentPosition, out textRange);
	}

	public FontStyle GetFontStyle(int currentPosition)
	{
		TextRange textRange;
		return GetFontStyle(currentPosition, out textRange);
	}

	public FontWeight GetFontWeight(int currentPosition)
	{
		TextRange textRange;
		return GetFontWeight(currentPosition, out textRange);
	}

	public InlineObject GetInlineObject(int currentPosition)
	{
		TextRange textRange;
		return GetInlineObject(currentPosition, out textRange);
	}

	public LineMetrics[] GetLineMetrics()
	{
		LineMetrics[] array = new LineMetrics[0];
		int maxLineCount = 0;
		int actualLineCount = 0;
		GetLineMetrics(array, maxLineCount, out actualLineCount);
		if (actualLineCount > 0)
		{
			array = new LineMetrics[actualLineCount];
			GetLineMetrics(array, actualLineCount, out actualLineCount);
		}
		return array;
	}

	public string GetLocaleName(int currentPosition)
	{
		TextRange textRange;
		return GetLocaleName(currentPosition, out textRange);
	}

	public unsafe string GetLocaleName(int currentPosition, out TextRange textRange)
	{
		GetLocaleNameLength(currentPosition, out var nameLength, out textRange);
		char* value = stackalloc char[nameLength + 1];
		GetLocaleName(currentPosition, new IntPtr(value), nameLength + 1, out textRange);
		return new string(value, 0, nameLength);
	}

	public bool HasStrikethrough(int currentPosition)
	{
		TextRange textRange;
		return HasStrikethrough(currentPosition, out textRange);
	}

	public Typography GetTypography(int currentPosition)
	{
		TextRange textRange;
		return GetTypography(currentPosition, out textRange);
	}

	public bool HasUnderline(int currentPosition)
	{
		TextRange textRange;
		return HasUnderline(currentPosition, out textRange);
	}

	public HitTestMetrics[] HitTestTextRange(int textPosition, int textLength, float originX, float originY)
	{
		HitTestMetrics[] array = new HitTestMetrics[0];
		int actualHitTestMetricsCount = 0;
		HitTestTextRange(textPosition, textLength, originX, originY, array, 0, out actualHitTestMetricsCount);
		if (actualHitTestMetricsCount > 0)
		{
			array = new HitTestMetrics[actualHitTestMetricsCount];
			HitTestTextRange(textPosition, textLength, originX, originY, array, actualHitTestMetricsCount, out actualHitTestMetricsCount);
		}
		return array;
	}

	public void SetInlineObject(InlineObject inlineObject, TextRange textRange)
	{
		SetInlineObject_(InlineObjectShadow.ToIntPtr(inlineObject), textRange);
	}

	public TextLayout(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextLayout(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new TextLayout(nativePointer);
		}
		return null;
	}

	internal unsafe void SetMaxWidth(float maxWidth)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, maxWidth)).CheckError();
	}

	internal unsafe void SetMaxHeight(float maxHeight)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, maxHeight)).CheckError();
	}

	public unsafe void SetFontCollection(FontCollection fontCollection, TextRange textRange)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = fontCollection?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, textRange)).CheckError();
	}

	public unsafe void SetFontFamilyName(string fontFamilyName, TextRange textRange)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(fontFamilyName);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, textRange);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	public unsafe void SetFontWeight(FontWeight fontWeight, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (int)fontWeight, textRange)).CheckError();
	}

	public unsafe void SetFontStyle(FontStyle fontStyle, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (int)fontStyle, textRange)).CheckError();
	}

	public unsafe void SetFontStretch(FontStretch fontStretch, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, (int)fontStretch, textRange)).CheckError();
	}

	public unsafe void SetFontSize(float fontSize, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, fontSize, textRange)).CheckError();
	}

	public unsafe void SetUnderline(Bool hasUnderline, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Bool, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, hasUnderline, textRange)).CheckError();
	}

	public unsafe void SetStrikethrough(Bool hasStrikethrough, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, Bool, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, hasStrikethrough, textRange)).CheckError();
	}

	internal unsafe void SetDrawingEffect_(IntPtr drawingEffect, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, (void*)drawingEffect, textRange)).CheckError();
	}

	internal unsafe void SetInlineObject_(IntPtr inlineObject, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, (void*)inlineObject, textRange)).CheckError();
	}

	public unsafe void SetTypography(Typography typography, TextRange textRange)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = typography?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, textRange)).CheckError();
	}

	public unsafe void SetLocaleName(string localeName, TextRange textRange)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(localeName);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, textRange);
		Marshal.FreeHGlobal(intPtr);
		result.CheckError();
	}

	internal unsafe float GetMaxWidth()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetMaxHeight()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe FontCollection GetFontCollection(int currentPosition, out TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &zero, ptr);
		}
		FontCollection result2 = ((zero == IntPtr.Zero) ? null : new FontCollection(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void GetFontFamilyNameLength(int currentPosition, out int nameLength, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref nameLength))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, currentPosition, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetFontFamilyName(int currentPosition, IntPtr fontFamilyName, int nameSize, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer, currentPosition, (void*)fontFamilyName, nameSize, ptr);
		}
		result.CheckError();
	}

	public unsafe FontWeight GetFontWeight(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		FontWeight result2 = default(FontWeight);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr);
		}
		result.CheckError();
		return result2;
	}

	public unsafe FontStyle GetFontStyle(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		FontStyle result2 = default(FontStyle);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr);
		}
		result.CheckError();
		return result2;
	}

	public unsafe FontStretch GetFontStretch(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		FontStretch result2 = default(FontStretch);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr);
		}
		result.CheckError();
		return result2;
	}

	public unsafe float GetFontSize(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		float result2 = default(float);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr);
		}
		result.CheckError();
		return result2;
	}

	public unsafe Bool HasUnderline(int currentPosition, out TextRange textRange)
	{
		Bool result = default(Bool);
		textRange = default(TextRange);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result2 = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result, ptr);
		}
		result2.CheckError();
		return result;
	}

	public unsafe Bool HasStrikethrough(int currentPosition, out TextRange textRange)
	{
		Bool result = default(Bool);
		textRange = default(TextRange);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result2 = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result, ptr);
		}
		result2.CheckError();
		return result;
	}

	internal unsafe IntPtr GetDrawingEffect_(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		IntPtr result2 = default(IntPtr);
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr);
		}
		result.CheckError();
		return result2;
	}

	public unsafe InlineObject GetInlineObject(int currentPosition, out TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &zero, ptr);
		}
		InlineObject result2 = ((zero == IntPtr.Zero) ? null : new InlineObjectNative(zero));
		result.CheckError();
		return result2;
	}

	public unsafe Typography GetTypography(int currentPosition, out TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &zero, ptr);
		}
		Typography result2 = ((zero == IntPtr.Zero) ? null : new Typography(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void GetLocaleNameLength(int currentPosition, out int nameLength, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref nameLength))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(_nativePointer, currentPosition, ptr, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetLocaleName(int currentPosition, IntPtr localeName, int nameSize, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextRange, IntPtr>(ref textRange))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, currentPosition, (void*)localeName, nameSize, ptr);
		}
		result.CheckError();
	}

	internal unsafe void Draw_(IntPtr clientDrawingContext, IntPtr renderer, float originX, float originY)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(_nativePointer, (void*)clientDrawingContext, (void*)renderer, originX, originY)).CheckError();
	}

	internal unsafe Result GetLineMetrics(LineMetrics[] lineMetrics, int maxLineCount, out int actualLineCount)
	{
		Result result;
		fixed (IntPtr* ptr = lineMetrics)
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualLineCount))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)59 * (nint)sizeof(void*))))(_nativePointer, ptr, maxLineCount, ptr2);
			}
		}
		return result;
	}

	internal unsafe void GetMetrics(out TextMetrics textMetrics)
	{
		textMetrics = default(TextMetrics);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<TextMetrics, IntPtr>(ref textMetrics))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)60 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetOverhangMetrics(out OverhangMetrics overhangs)
	{
		overhangs = default(OverhangMetrics);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<OverhangMetrics, IntPtr>(ref overhangs))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)61 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe Result GetClusterMetrics(ClusterMetrics[] clusterMetrics, int maxClusterCount, out int actualClusterCount)
	{
		Result result;
		fixed (IntPtr* ptr = clusterMetrics)
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualClusterCount))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)62 * (nint)sizeof(void*))))(_nativePointer, ptr, maxClusterCount, ptr2);
			}
		}
		return result;
	}

	public unsafe float DetermineMinWidth()
	{
		float result = default(float);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)63 * (nint)sizeof(void*))))(_nativePointer, &result)).CheckError();
		return result;
	}

	public unsafe HitTestMetrics HitTestPoint(float pointX, float pointY, out Bool isTrailingHit, out Bool isInside)
	{
		isTrailingHit = default(Bool);
		isInside = default(Bool);
		HitTestMetrics result = default(HitTestMetrics);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref isTrailingHit))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<Bool, IntPtr>(ref isInside))
			{
				result2 = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(_nativePointer, pointX, pointY, ptr, ptr2, &result);
			}
		}
		result2.CheckError();
		return result;
	}

	public unsafe HitTestMetrics HitTestTextPosition(int textPosition, Bool isTrailingHit, out float ointXRef, out float ointYRef)
	{
		HitTestMetrics result = default(HitTestMetrics);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref ointXRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref ointYRef))
			{
				result2 = ((delegate* unmanaged[Stdcall]<void*, int, Bool, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)65 * (nint)sizeof(void*))))(_nativePointer, textPosition, isTrailingHit, ptr, ptr2, &result);
			}
		}
		result2.CheckError();
		return result;
	}

	internal unsafe Result HitTestTextRange(int textPosition, int textLength, float originX, float originY, HitTestMetrics[] hitTestMetrics, int maxHitTestMetricsCount, out int actualHitTestMetricsCount)
	{
		Result result;
		fixed (IntPtr* ptr = hitTestMetrics)
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref actualHitTestMetricsCount))
			{
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, float, float, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)66 * (nint)sizeof(void*))))(_nativePointer, textPosition, textLength, originX, originY, ptr, maxHitTestMetricsCount, ptr2);
			}
		}
		return result;
	}
}
