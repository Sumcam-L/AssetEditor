using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Docking;

internal class DockIcon : Control
{
	public static DependencyProperty HighlightProperty = DependencyProperty.Register("Highlight", typeof(bool), typeof(DockIcon));

	private Size m_size;

	private Point m_offset;

	public Point Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
			Canvas.SetLeft(this, m_offset.X);
			Canvas.SetTop(this, m_offset.Y);
		}
	}

	public bool Highlight
	{
		get
		{
			return (bool)GetValue(HighlightProperty);
		}
		set
		{
			SetValue(HighlightProperty, value);
		}
	}

	public DockIcon(Style style, Size size)
	{
		Highlight = false;
		base.Style = style;
		base.Width = size.Width;
		base.Height = size.Height;
		m_size = size;
	}

	public bool HitTest(Point p)
	{
		return new Rect(m_offset, m_size).Contains(p);
	}
}
