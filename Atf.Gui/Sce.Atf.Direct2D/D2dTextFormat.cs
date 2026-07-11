using System;
using SharpDX.DirectWrite;

namespace Sce.Atf.Direct2D;

public class D2dTextFormat : D2dResource
{
	private TextFormat m_nativeTextFormat;

	private EllipsisTrimming m_ellipsisTrimming;

	private D2dDrawTextOptions m_drawTextOptions = D2dDrawTextOptions.None;

	private readonly float m_fontHeight;

	public string FontFamilyName => m_nativeTextFormat.FontFamilyName;

	public float FontSize => m_nativeTextFormat.FontSize;

	public float FontHeight => m_fontHeight;

	public D2dParagraphAlignment ParagraphAlignment
	{
		get
		{
			return (D2dParagraphAlignment)m_nativeTextFormat.ParagraphAlignment;
		}
		set
		{
			m_nativeTextFormat.ParagraphAlignment = (ParagraphAlignment)value;
		}
	}

	public D2dReadingDirection ReadingDirection
	{
		get
		{
			return (D2dReadingDirection)m_nativeTextFormat.ReadingDirection;
		}
		set
		{
			m_nativeTextFormat.ReadingDirection = (ReadingDirection)value;
		}
	}

	public D2dTextAlignment TextAlignment
	{
		get
		{
			return (D2dTextAlignment)m_nativeTextFormat.TextAlignment;
		}
		set
		{
			m_nativeTextFormat.TextAlignment = (TextAlignment)value;
		}
	}

	public D2dWordWrapping WordWrapping
	{
		get
		{
			return (D2dWordWrapping)m_nativeTextFormat.WordWrapping;
		}
		set
		{
			m_nativeTextFormat.WordWrapping = (WordWrapping)value;
		}
	}

	public D2dTrimming Trimming
	{
		get
		{
			m_nativeTextFormat.GetTrimming(out var trimmingOptions, out var trimmingSign);
			D2dTrimming result = default(D2dTrimming);
			result.Delimiter = trimmingOptions.Delimiter;
			result.DelimiterCount = trimmingOptions.DelimiterCount;
			result.Granularity = (D2dTrimmingGranularity)trimmingOptions.Granularity;
			trimmingSign.Dispose();
			return result;
		}
		set
		{
			Trimming trimmingOptions = new Trimming
			{
				Delimiter = value.Delimiter,
				DelimiterCount = value.DelimiterCount,
				Granularity = (TrimmingGranularity)value.Granularity
			};
			EllipsisTrimming trimmingSign = ((value.Granularity != D2dTrimmingGranularity.None) ? m_ellipsisTrimming : null);
			m_nativeTextFormat.SetTrimming(trimmingOptions, trimmingSign);
		}
	}

	public D2dDrawTextOptions DrawTextOptions
	{
		get
		{
			return m_drawTextOptions;
		}
		set
		{
			m_drawTextOptions = value;
		}
	}

	public bool Underlined { get; set; }

	public bool Strikeout { get; set; }

	internal TextFormat NativeTextFormat => m_nativeTextFormat;

	public D2dResult GetLineSpacing(out D2dLineSpacingMethod lineSpacingMethod, out float lineSpacing, out float baseline)
	{
		m_nativeTextFormat.GetLineSpacing(out var lineSpacingMethod2, out lineSpacing, out baseline);
		lineSpacingMethod = (D2dLineSpacingMethod)lineSpacingMethod2;
		return D2dResult.Ok;
	}

	public D2dResult SetLineSpacing(D2dLineSpacingMethod lineSpacingMethod, float lineSpacing, float baseline)
	{
		m_nativeTextFormat.SetLineSpacing((LineSpacingMethod)lineSpacingMethod, lineSpacing, baseline);
		return D2dResult.Ok;
	}

	protected override void Dispose(bool disposing)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (disposing)
		{
			if (m_ellipsisTrimming != null)
			{
				m_ellipsisTrimming.Dispose();
				m_ellipsisTrimming = null;
			}
			if (m_nativeTextFormat != null)
			{
				m_nativeTextFormat.Dispose();
				m_nativeTextFormat = null;
			}
		}
		base.Dispose(disposing);
	}

	internal D2dTextFormat(TextFormat textFormat)
	{
		if (textFormat == null)
		{
			throw new ArgumentNullException("textFormat");
		}
		m_nativeTextFormat = textFormat;
		m_ellipsisTrimming = new EllipsisTrimming(D2dFactory.NativeDwFactory, m_nativeTextFormat);
		m_fontHeight = D2dFactory.FontSizeToPixel(m_nativeTextFormat.FontSize);
	}
}
