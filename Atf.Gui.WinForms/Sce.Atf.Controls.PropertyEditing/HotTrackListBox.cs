using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing;

public class HotTrackListBox : ListBox
{
	private int m_hoveredIndex = -1;

	public Padding ItemMargin { get; set; } = new Padding(0, 2, 0, 2);

	public Color HighlightTextColor { get; set; } = SystemColors.HighlightText;

	public Color HighlightColor { get; set; } = SystemColors.Highlight;

	public HotTrackListBox()
	{
		DrawMode = DrawMode.OwnerDrawFixed;
	}

	private bool IsHovered(DrawItemEventArgs e)
	{
		return e.Index == m_hoveredIndex;
	}

	private bool IsSelected(DrawItemEventArgs e)
	{
		return (e.State & DrawItemState.Selected) == DrawItemState.Selected;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		ItemHeight = ItemMargin.Top + base.FontHeight + ItemMargin.Bottom;
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		if (e.Index > -1)
		{
			Color color = ForeColor;
			Color color2 = BackColor;
			if (IsHovered(e) || IsSelected(e))
			{
				color = HighlightTextColor;
				color2 = HighlightColor;
			}
			using Brush brush = new SolidBrush(color);
			using Brush brush2 = new SolidBrush(color2);
			e.Graphics.FillRectangle(brush2, e.Bounds);
			e.Graphics.DrawString(base.Items[e.Index].ToString(), e.Font, brush, e.Bounds.Left + ItemMargin.Left, e.Bounds.Top + ItemMargin.Top);
		}
		base.OnDrawItem(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		int num = IndexFromPoint(e.Location);
		if (num != m_hoveredIndex)
		{
			int hoveredIndex = m_hoveredIndex;
			m_hoveredIndex = num;
			if (hoveredIndex > -1)
			{
				Invalidate(GetItemRectangle(hoveredIndex));
			}
			if (m_hoveredIndex > -1)
			{
				Invalidate(GetItemRectangle(m_hoveredIndex));
			}
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (m_hoveredIndex > -1)
		{
			int hoveredIndex = m_hoveredIndex;
			m_hoveredIndex = -1;
			Invalidate(GetItemRectangle(hoveredIndex));
		}
	}
}
