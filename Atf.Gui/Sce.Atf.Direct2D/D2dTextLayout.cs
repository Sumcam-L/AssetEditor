using System.Drawing;
using Sce.Atf.DirectWrite;
using SharpDX;
using SharpDX.DirectWrite;

namespace Sce.Atf.Direct2D;

public class D2dTextLayout : D2dTextFormat
{
	private string m_text;

	private TextLayout m_nativeTextLayout;

	public string Text => m_text;

	public float LayoutWidth
	{
		get
		{
			return m_nativeTextLayout.MaxWidth;
		}
		set
		{
			m_nativeTextLayout.MaxWidth = value;
		}
	}

	public float LayoutHeight
	{
		get
		{
			return m_nativeTextLayout.MaxHeight;
		}
		set
		{
			m_nativeTextLayout.MaxHeight = value;
		}
	}

	public float Width => m_nativeTextLayout.Metrics.Width;

	public float Height => m_nativeTextLayout.Metrics.Height;

	public int LineCount => m_nativeTextLayout.Metrics.LineCount;

	internal TextLayout NativeTextLayout => m_nativeTextLayout;

	public void SetStrikethrough(bool hasStrikethrough, int startPosition, int length)
	{
		m_nativeTextLayout.SetStrikethrough(hasStrikethrough, new TextRange(startPosition, length));
	}

	public void SetUnderline(bool hasUnderline, int startPosition, int length)
	{
		m_nativeTextLayout.SetUnderline(hasUnderline, new TextRange(startPosition, length));
	}

	public Sce.Atf.DirectWrite.HitTestMetrics HitTestPoint(float x, float y)
	{
		Bool isTrailingHit;
		Bool isInside;
		SharpDX.DirectWrite.HitTestMetrics hitTestMetrics = NativeTextLayout.HitTestPoint(x, y, out isTrailingHit, out isInside);
		return new Sce.Atf.DirectWrite.HitTestMetrics
		{
			IsInside = isInside,
			IsTrailingHit = isTrailingHit,
			TextPosition = hitTestMetrics.TextPosition,
			Length = hitTestMetrics.Length
		};
	}

	public Sce.Atf.DirectWrite.HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit)
	{
		float ointXRef;
		float ointYRef;
		SharpDX.DirectWrite.HitTestMetrics hitTestMetrics = NativeTextLayout.HitTestTextPosition(textPosition, isTrailingHit, out ointXRef, out ointYRef);
		return new Sce.Atf.DirectWrite.HitTestMetrics
		{
			TextPosition = hitTestMetrics.TextPosition,
			Length = hitTestMetrics.Length,
			Point = new PointF(ointXRef, ointYRef),
			Width = hitTestMetrics.Width,
			Height = hitTestMetrics.Height
		};
	}

	public Sce.Atf.DirectWrite.HitTestMetrics[] HitTestTextRange(int textPosition, int textLength, float originX, float originY)
	{
		SharpDX.DirectWrite.HitTestMetrics[] array = NativeTextLayout.HitTestTextRange(textPosition, textLength, originX, originY);
		Sce.Atf.DirectWrite.HitTestMetrics[] array2 = new Sce.Atf.DirectWrite.HitTestMetrics[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i].TextPosition = array[i].TextPosition;
			array2[i].Length = array[i].Length;
			array2[i].Point = new PointF(array[i].Left, array[i].Top);
			array2[i].Height = array[i].Height;
			array2[i].Width = array[i].Width;
			array2[i].Top = array[i].Top;
		}
		return array2;
	}

	public Sce.Atf.DirectWrite.LineMetrics[] GetLineMetrics()
	{
		SharpDX.DirectWrite.LineMetrics[] lineMetrics = NativeTextLayout.GetLineMetrics();
		Sce.Atf.DirectWrite.LineMetrics[] array = new Sce.Atf.DirectWrite.LineMetrics[lineMetrics.Length];
		for (int i = 0; i < lineMetrics.Length; i++)
		{
			array[i].Baseline = lineMetrics[i].Baseline;
			array[i].Height = lineMetrics[i].Height;
			array[i].IsTrimmed = lineMetrics[i].IsTrimmed;
			array[i].Length = lineMetrics[i].Length;
			array[i].TrailingWhitespaceLength = lineMetrics[i].TrailingWhitespaceLength;
			array[i].NewlineLength = lineMetrics[i].NewlineLength;
		}
		return array;
	}

	public Sce.Atf.DirectWrite.ClusterMetrics[] GetClusterMetrics()
	{
		SharpDX.DirectWrite.ClusterMetrics[] clusterMetrics = NativeTextLayout.GetClusterMetrics();
		Sce.Atf.DirectWrite.ClusterMetrics[] array = new Sce.Atf.DirectWrite.ClusterMetrics[clusterMetrics.Length];
		for (int i = 0; i < clusterMetrics.Length; i++)
		{
			array[i].Length = clusterMetrics[i].Length;
			array[i].Width = clusterMetrics[i].Width;
			array[i].CanWrapLineAfter = clusterMetrics[i].CanWrapLineAfter;
			array[i].IsNewline = clusterMetrics[i].IsNewline;
			array[i].IsRightToLeft = clusterMetrics[i].IsRightToLeft;
			array[i].IsSoftHyphen = clusterMetrics[i].IsSoftHyphen;
			array[i].IsWhitespace = clusterMetrics[i].IsWhitespace;
		}
		return array;
	}

	internal D2dTextLayout(string text, TextLayout textLayout)
		: base(textLayout)
	{
		m_nativeTextLayout = textLayout;
		m_text = text;
	}
}
