using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("9c906818-31d7-4fd3-a151-7c5e225db55a")]
public class TextFormat : ComObject
{
	public unsafe string FontFamilyName
	{
		get
		{
			int fontFamilyNameLength = GetFontFamilyNameLength();
			char* value = stackalloc char[fontFamilyNameLength + 1];
			GetFontFamilyName(new IntPtr(value), fontFamilyNameLength + 1);
			return new string(value, 0, fontFamilyNameLength);
		}
	}

	public unsafe string LocaleName
	{
		get
		{
			int localeNameLength = GetLocaleNameLength();
			char* value = stackalloc char[localeNameLength + 1];
			GetLocaleName(new IntPtr(value), localeNameLength + 1);
			return new string(value, 0, localeNameLength);
		}
	}

	public TextAlignment TextAlignment
	{
		get
		{
			return GetTextAlignment();
		}
		set
		{
			SetTextAlignment(value);
		}
	}

	public ParagraphAlignment ParagraphAlignment
	{
		get
		{
			return GetParagraphAlignment();
		}
		set
		{
			SetParagraphAlignment(value);
		}
	}

	public WordWrapping WordWrapping
	{
		get
		{
			return GetWordWrapping();
		}
		set
		{
			SetWordWrapping(value);
		}
	}

	public ReadingDirection ReadingDirection
	{
		get
		{
			return GetReadingDirection();
		}
		set
		{
			SetReadingDirection(value);
		}
	}

	public FlowDirection FlowDirection
	{
		get
		{
			return GetFlowDirection();
		}
		set
		{
			SetFlowDirection(value);
		}
	}

	public float IncrementalTabStop
	{
		get
		{
			return GetIncrementalTabStop();
		}
		set
		{
			SetIncrementalTabStop(value);
		}
	}

	public FontCollection FontCollection
	{
		get
		{
			GetFontCollection(out var fontCollection);
			return fontCollection;
		}
	}

	public FontWeight FontWeight => GetFontWeight();

	public FontStyle FontStyle => GetFontStyle();

	public FontStretch FontStretch => GetFontStretch();

	public float FontSize => GetFontSize();

	public TextFormat(Factory factory, string fontFamilyName, float fontSize)
		: this(factory, fontFamilyName, null, FontWeight.Normal, FontStyle.Normal, FontStretch.Normal, fontSize, "")
	{
	}

	public TextFormat(Factory factory, string fontFamilyName, FontWeight fontWeight, FontStyle fontStyle, float fontSize)
		: this(factory, fontFamilyName, null, fontWeight, fontStyle, FontStretch.Normal, fontSize, "")
	{
	}

	public TextFormat(Factory factory, string fontFamilyName, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize)
		: this(factory, fontFamilyName, null, fontWeight, fontStyle, fontStretch, fontSize, "")
	{
	}

	public TextFormat(Factory factory, string fontFamilyName, FontCollection fontCollection, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize)
		: this(factory, fontFamilyName, fontCollection, fontWeight, fontStyle, fontStretch, fontSize, "")
	{
	}

	public TextFormat(Factory factory, string fontFamilyName, FontCollection fontCollection, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize, string localeName)
		: base(IntPtr.Zero)
	{
		factory.CreateTextFormat(fontFamilyName, fontCollection, fontWeight, fontStyle, fontStretch, fontSize, localeName, this);
	}

	public void SetTrimming(Trimming trimmingOptions, InlineObject trimmingSign)
	{
		SetTrimming_(trimmingOptions, InlineObjectShadow.ToIntPtr(trimmingSign));
	}

	public TextFormat(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextFormat(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new TextFormat(nativePointer);
		}
		return null;
	}

	internal unsafe void SetTextAlignment(TextAlignment textAlignment)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)textAlignment)).CheckError();
	}

	internal unsafe void SetParagraphAlignment(ParagraphAlignment paragraphAlignment)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)paragraphAlignment)).CheckError();
	}

	internal unsafe void SetWordWrapping(WordWrapping wordWrapping)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (int)wordWrapping)).CheckError();
	}

	internal unsafe void SetReadingDirection(ReadingDirection readingDirection)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (int)readingDirection)).CheckError();
	}

	internal unsafe void SetFlowDirection(FlowDirection flowDirection)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (int)flowDirection)).CheckError();
	}

	internal unsafe void SetIncrementalTabStop(float incrementalTabStop)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, incrementalTabStop)).CheckError();
	}

	internal unsafe void SetTrimming_(Trimming trimmingOptions, IntPtr trimmingSign)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &trimmingOptions, (void*)trimmingSign)).CheckError();
	}

	public unsafe void SetLineSpacing(LineSpacingMethod lineSpacingMethod, float lineSpacing, float baseline)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, float, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)lineSpacingMethod, lineSpacing, baseline)).CheckError();
	}

	internal unsafe TextAlignment GetTextAlignment()
	{
		return ((delegate* unmanaged[Stdcall]<void*, TextAlignment>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ParagraphAlignment GetParagraphAlignment()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ParagraphAlignment>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe WordWrapping GetWordWrapping()
	{
		return ((delegate* unmanaged[Stdcall]<void*, WordWrapping>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ReadingDirection GetReadingDirection()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ReadingDirection>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FlowDirection GetFlowDirection()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FlowDirection>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetIncrementalTabStop()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetTrimming(out Trimming trimmingOptions, out InlineObject trimmingSign)
	{
		trimmingOptions = default(Trimming);
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Trimming, IntPtr>(ref trimmingOptions))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		trimmingSign = ((zero == IntPtr.Zero) ? null : new InlineObjectNative(zero));
		result.CheckError();
	}

	public unsafe void GetLineSpacing(out LineSpacingMethod lineSpacingMethod, out float lineSpacing, out float baseline)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<LineSpacingMethod, IntPtr>(ref lineSpacingMethod))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref lineSpacing))
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<float, IntPtr>(ref baseline))
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2, ptr3);
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void GetFontCollection(out FontCollection fontCollection)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, &zero);
		fontCollection = ((zero == IntPtr.Zero) ? null : new FontCollection(zero));
		result.CheckError();
	}

	internal unsafe int GetFontFamilyNameLength()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFontFamilyName(IntPtr fontFamilyName, int nameSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFamilyName, nameSize)).CheckError();
	}

	internal unsafe FontWeight GetFontWeight()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontWeight>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontStyle GetFontStyle()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontStretch GetFontStretch()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontStretch>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetFontSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe int GetLocaleNameLength()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetLocaleName(IntPtr localeName, int nameSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, (void*)localeName, nameSize)).CheckError();
	}
}
