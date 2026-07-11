using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class HoverLabel : HoverBase
{
	private string m_label;

	private const int TextMargin = 2;

	public string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			m_label = value;
			SetBounds();
		}
	}

	public HoverLabel(string label)
	{
		Label = label;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, new Rectangle(0, 0, base.Width - 1, base.Height - 1));
		e.Graphics.DrawString(m_label, Font, Brushes.Black, 2f, 2f);
	}

	private void SetBounds()
	{
		using Graphics graphics = CreateGraphics();
		SizeF sizeF = graphics.MeasureString(m_label, Font);
		base.Size = new Size((int)Math.Ceiling(sizeF.Width) + 4, (int)Math.Ceiling(sizeF.Height) + 4);
	}
}
