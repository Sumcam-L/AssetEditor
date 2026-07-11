using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class CompactSpinner : Control
{
	private int m_currentMouseX;

	public event EventHandler<SpinDirectionEventArgs> Changing = delegate
	{
	};

	public event EventHandler<SpinDirectionEventArgs> Changed = delegate
	{
	};

	public CompactSpinner()
	{
		DoubleBuffered = true;
		base.TabStop = false;
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		Cursor = Cursors.SizeWE;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			base.Capture = true;
			m_currentMouseX = e.X;
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			base.Capture = false;
			int value = e.X - m_currentMouseX;
			int direction = Math.Sign(value);
			this.Changed(this, new SpinDirectionEventArgs(direction, value));
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			int value = e.X - m_currentMouseX;
			int num = Math.Sign(value);
			m_currentMouseX = e.X;
			if (num != 0)
			{
				this.Changing(this, new SpinDirectionEventArgs(num, value));
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		int num = base.Height / 2;
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using Pen pen = new Pen(Color.LightBlue);
		pen.Width = 4f;
		pen.EndCap = LineCap.ArrowAnchor;
		pen.StartCap = LineCap.ArrowAnchor;
		e.Graphics.DrawLine(pen, 0, num, base.Width, num);
	}
}
