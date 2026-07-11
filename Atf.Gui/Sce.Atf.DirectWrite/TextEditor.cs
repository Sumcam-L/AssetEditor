using System;
using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.DirectWrite;

public class TextEditor
{
	public enum SelectionMode
	{
		Left,
		Right,
		Up,
		Down,
		LeftChar,
		RightChar,
		LeftWord,
		RightWord,
		SingleWord,
		Home,
		End,
		First,
		Last,
		AbsoluteLeading,
		AbsoluteTrailing,
		All
	}

	private int m_caretAnchor;

	private int m_caretPosition;

	private int m_caretPositionOffset;

	public D2dTextLayout TextLayout { get; set; }

	public D2dTextFormat TextFormat { get; set; }

	public int SelectionStart { get; set; }

	public int SelectionLength { get; set; }

	public int CaretPosition => m_caretPosition;

	public int CaretAnchorPosition => m_caretAnchor;

	public int CaretAbsolutePosition => m_caretPosition + m_caretPositionOffset;

	public int TopLine { get; set; }

	public bool VerticalScrollBarVisibe { get; set; }

	public void SetSelectionFromPoint(float x, float y, bool extendSelection)
	{
		HitTestMetrics hitTestMetrics = TextLayout.HitTestPoint(x, y);
		SetSelection(hitTestMetrics.IsTrailingHit ? SelectionMode.AbsoluteTrailing : SelectionMode.AbsoluteLeading, hitTestMetrics.TextPosition, extendSelection, updateCaretFormat: true);
		UpdateSelectionRange();
	}

	private void AlignCaretToNearestCluster(bool isTrailingHit, bool skipZeroWidth)
	{
		HitTestMetrics hitTestMetrics = TextLayout.HitTestTextPosition(m_caretPosition, isTrailingHit: false);
		m_caretPosition = hitTestMetrics.TextPosition;
		m_caretPositionOffset = (isTrailingHit ? hitTestMetrics.Length : 0);
		if (skipZeroWidth && hitTestMetrics.Width == 0f)
		{
			m_caretPosition += m_caretPositionOffset;
			m_caretPositionOffset = 0;
		}
	}

	public bool SetSelection(SelectionMode moveMode, int advance, bool extendSelection, bool updateCaretFormat)
	{
		int line = int.MaxValue;
		int num = m_caretPosition + m_caretPositionOffset;
		int num2 = num;
		int caretAnchor = m_caretAnchor;
		switch (moveMode)
		{
		case SelectionMode.Left:
			m_caretPosition += m_caretPositionOffset;
			if (m_caretPosition > 0)
			{
				m_caretPosition--;
				AlignCaretToNearestCluster(isTrailingHit: false, skipZeroWidth: true);
				num = m_caretPosition + m_caretPositionOffset;
				if (num >= 1 && num < TextLayout.Text.Length && TextLayout.Text[num - 1] == '\r' && TextLayout.Text[num] == '\n')
				{
					m_caretPosition = num - 1;
					AlignCaretToNearestCluster(isTrailingHit: false, skipZeroWidth: true);
				}
			}
			break;
		case SelectionMode.Right:
			m_caretPosition = num;
			AlignCaretToNearestCluster(isTrailingHit: true, skipZeroWidth: true);
			num = m_caretPosition + m_caretPositionOffset;
			if (num >= 1 && num < TextLayout.Text.Length && TextLayout.Text[num - 1] == '\r' && TextLayout.Text[num] == '\n')
			{
				m_caretPosition = num + 1;
				AlignCaretToNearestCluster(isTrailingHit: false, skipZeroWidth: true);
			}
			break;
		case SelectionMode.LeftChar:
			m_caretPosition = num;
			m_caretPosition -= Math.Min(advance, num);
			m_caretPositionOffset = 0;
			break;
		case SelectionMode.RightChar:
		{
			m_caretPosition = num + advance;
			m_caretPositionOffset = 0;
			HitTestMetrics hitTestMetrics2 = TextLayout.HitTestTextPosition(m_caretPosition, isTrailingHit: false);
			m_caretPosition = Math.Min(m_caretPosition, hitTestMetrics2.TextPosition + hitTestMetrics2.Length);
			break;
		}
		case SelectionMode.Up:
		case SelectionMode.Down:
		{
			LineMetrics[] lineMetrics2 = TextLayout.GetLineMetrics();
			GetLineFromPosition(lineMetrics2, m_caretPosition, out line, out var linePosition2);
			if (moveMode == SelectionMode.Up)
			{
				if (line <= 0)
				{
					break;
				}
				line--;
				linePosition2 -= lineMetrics2[line].Length;
				if (line <= TopLine)
				{
					TopLine = ((TopLine - 1 >= 0) ? (TopLine - 1) : 0);
				}
			}
			else
			{
				linePosition2 += lineMetrics2[line].Length;
				line++;
				if (line >= lineMetrics2.Length)
				{
					break;
				}
				TopLine++;
			}
			float x = TextLayout.HitTestTextPosition(m_caretPosition, m_caretPositionOffset > 0).Point.X;
			float y = TextLayout.HitTestTextPosition(linePosition2, isTrailingHit: false).Point.Y;
			HitTestMetrics hitTestMetrics = TextLayout.HitTestPoint(x, y);
			m_caretPosition = hitTestMetrics.TextPosition;
			m_caretPositionOffset = ((hitTestMetrics.IsTrailingHit && hitTestMetrics.Length > 0) ? 1 : 0);
			break;
		}
		case SelectionMode.LeftWord:
		case SelectionMode.RightWord:
		{
			ClusterMetrics[] clusterMetrics = TextLayout.GetClusterMetrics();
			if (clusterMetrics.Length == 0)
			{
				break;
			}
			m_caretPosition = num;
			int num4 = 0;
			int caretPosition = m_caretPosition;
			if (moveMode == SelectionMode.LeftWord)
			{
				m_caretPosition = 0;
				m_caretPositionOffset = 0;
				for (int i = 0; i < clusterMetrics.Length; i++)
				{
					num4 += clusterMetrics[i].Length;
					if (clusterMetrics[i].CanWrapLineAfter)
					{
						if (num4 >= caretPosition)
						{
							break;
						}
						m_caretPosition = num4;
					}
				}
				break;
			}
			for (int j = 0; j < clusterMetrics.Length; j++)
			{
				int length = clusterMetrics[j].Length;
				m_caretPosition = num4;
				m_caretPositionOffset = length;
				if (num4 >= caretPosition && clusterMetrics[j].CanWrapLineAfter)
				{
					break;
				}
				num4 += length;
			}
			break;
		}
		case SelectionMode.SingleWord:
		{
			ClusterMetrics[] clusterMetrics2 = TextLayout.GetClusterMetrics();
			if (clusterMetrics2.Length == 0)
			{
				break;
			}
			m_caretPosition = num;
			int num5 = 0;
			int caretPosition2 = m_caretPosition;
			m_caretPosition = 0;
			m_caretPositionOffset = 0;
			for (int k = 0; k < clusterMetrics2.Length; k++)
			{
				num5 += clusterMetrics2[k].Length;
				if (clusterMetrics2[k].CanWrapLineAfter)
				{
					if (num5 >= caretPosition2)
					{
						break;
					}
					m_caretPosition = num5;
				}
			}
			int caretPosition3 = m_caretPosition;
			for (int l = 0; l < clusterMetrics2[l].Length; l++)
			{
				int length2 = clusterMetrics2[l].Length;
				m_caretPosition = num5;
				m_caretPositionOffset = length2;
				if (num5 >= caretPosition2 && clusterMetrics2[l].CanWrapLineAfter)
				{
					break;
				}
				num5 += length2;
			}
			int caretPosition4 = m_caretPosition - 1;
			m_caretPositionOffset = 0;
			m_caretAnchor = caretPosition3;
			m_caretPosition = caretPosition4;
			break;
		}
		case SelectionMode.Home:
		case SelectionMode.End:
		{
			LineMetrics[] lineMetrics = TextLayout.GetLineMetrics();
			GetLineFromPosition(lineMetrics, m_caretPosition, out line, out var linePosition);
			m_caretPosition = linePosition;
			m_caretPositionOffset = 0;
			if (moveMode == SelectionMode.End)
			{
				int num3 = lineMetrics[line].Length - lineMetrics[line].NewlineLength;
				m_caretPositionOffset = Math.Min(num3, 1);
				m_caretPosition += num3 - m_caretPositionOffset;
				AlignCaretToNearestCluster(isTrailingHit: true, skipZeroWidth: false);
			}
			break;
		}
		case SelectionMode.First:
			m_caretPosition = 0;
			m_caretPositionOffset = 0;
			break;
		case SelectionMode.All:
			m_caretAnchor = 0;
			extendSelection = true;
			goto case SelectionMode.Last;
		case SelectionMode.Last:
			m_caretPosition = int.MaxValue;
			m_caretPositionOffset = 0;
			AlignCaretToNearestCluster(isTrailingHit: true, skipZeroWidth: false);
			break;
		case SelectionMode.AbsoluteLeading:
			m_caretPosition = advance;
			m_caretPositionOffset = 0;
			break;
		case SelectionMode.AbsoluteTrailing:
			m_caretPosition = advance;
			AlignCaretToNearestCluster(isTrailingHit: true, skipZeroWidth: false);
			break;
		}
		num = m_caretPosition + m_caretPositionOffset;
		if (!extendSelection)
		{
			m_caretAnchor = num;
		}
		bool flag = num != num2 || m_caretAnchor != caretAnchor;
		if (flag)
		{
			LineMetrics[] lineMetrics3 = TextLayout.GetLineMetrics();
			GetLineFromPosition(lineMetrics3, m_caretPosition, out line, out var _);
			if (line <= TopLine)
			{
				TopLine = ((TopLine - 1 >= 0) ? (TopLine - 1) : 0);
			}
			float num6 = TextLayout.Height / lineMetrics3[0].Height;
			if ((float)line > (float)TopLine + num6)
			{
				TopLine++;
			}
			GetCaretRect();
		}
		Validate();
		return flag;
	}

	public RectangleF GetCaretRect()
	{
		if (TextLayout == null)
		{
			return default(RectangleF);
		}
		HitTestMetrics hitTestMetrics = TextLayout.HitTestTextPosition(m_caretPosition, m_caretPositionOffset > 0);
		float x = hitTestMetrics.Point.X;
		float y = hitTestMetrics.Point.Y;
		UpdateSelectionRange();
		if (SelectionLength > 0)
		{
			HitTestMetrics[] array = TextLayout.HitTestTextRange(m_caretPosition, 0, 0f, 0f);
			y = array[0].Top;
		}
		return new RectangleF(x - 0.5f, y, 1f, hitTestMetrics.Height);
	}

	public void UpdateSelectionRange()
	{
		int num = m_caretAnchor;
		int num2 = m_caretPosition + m_caretPositionOffset;
		if (num > num2)
		{
			int num3 = num2;
			num2 = num;
			num = num3;
		}
		num = Math.Min(num, TextLayout.Text.Length);
		num2 = Math.Min(num2, TextLayout.Text.Length);
		SelectionStart = num;
		SelectionLength = num2 - num;
	}

	public string InsertTextAt(string originalText, string textToInsert)
	{
		int val = m_caretPosition + m_caretPositionOffset;
		int startIndex = Math.Min(val, originalText.Length);
		string text = originalText.Insert(startIndex, textToInsert);
		RecreateLayout(text);
		return text;
	}

	public string RemoveTextAt(string originalText, int startPosition, int length)
	{
		startPosition = Math.Min(startPosition, originalText.Length);
		length = Math.Min(originalText.Length - startPosition, length);
		string text = originalText.Remove(startPosition, length);
		RecreateLayout(text);
		return text;
	}

	public void ResetText(string newText)
	{
		RecreateLayout(newText);
		m_caretPosition = Math.Min(m_caretPosition, TextLayout.Text.Length);
		m_caretPositionOffset = 0;
		m_caretAnchor = m_caretPosition;
		UpdateSelectionRange();
	}

	private void RecreateLayout(string text)
	{
		D2dTextLayout d2dTextLayout = D2dFactory.CreateTextLayout(text, TextFormat, TextLayout.LayoutWidth, TextLayout.LayoutHeight);
		if (TextFormat.Underlined)
		{
			d2dTextLayout.SetUnderline(hasUnderline: true, 0, text.Length);
		}
		if (TextFormat.Strikeout)
		{
			d2dTextLayout.SetStrikethrough(hasStrikethrough: true, 0, text.Length);
		}
		TextLayout.Dispose();
		TextLayout = d2dTextLayout;
	}

	private void GetLineFromPosition(LineMetrics[] lineMetrics, int textPosition, out int line, out int linePosition)
	{
		line = 0;
		linePosition = 0;
		int num = 0;
		while (line < lineMetrics.Length)
		{
			linePosition = num;
			num = linePosition + lineMetrics[line].Length;
			if (num > textPosition)
			{
				break;
			}
			line++;
		}
		if (line >= lineMetrics.Length)
		{
			line = lineMetrics.Length - 1;
		}
	}

	public int GetVisibleLines()
	{
		LineMetrics[] lineMetrics = TextLayout.GetLineMetrics();
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < lineMetrics.Length; i++)
		{
			num2 += lineMetrics[i].Height;
			if (num2 > TextLayout.LayoutHeight)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public float GetLineYOffset(int line)
	{
		if (line > TextLayout.LineCount - 1)
		{
			return 0f;
		}
		float num = 0f;
		LineMetrics[] lineMetrics = TextLayout.GetLineMetrics();
		for (int i = 0; i < line - 1; i++)
		{
			num += lineMetrics[i].Height;
		}
		return num;
	}

	public void Validate()
	{
		int visibleLines = GetVisibleLines();
		LineMetrics[] lineMetrics = TextLayout.GetLineMetrics();
		GetLineFromPosition(lineMetrics, m_caretPosition, out var line, out var _);
		if (line > TopLine + visibleLines - 1)
		{
			int num = line - TopLine - visibleLines + 1;
			TopLine += num;
		}
		int num2 = TextLayout.LineCount - visibleLines;
		if (TopLine > num2)
		{
			TopLine = num2;
		}
	}
}
