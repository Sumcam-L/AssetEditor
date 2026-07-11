using System.Drawing;
using System.Windows.Forms;

namespace SharpDX.Windows;

public class RenderControl : UserControl
{
	private Font fontForDesignMode;

	public RenderControl()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, value: true);
		UpdateStyles();
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		if (base.DesignMode)
		{
			base.OnPaintBackground(e);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (base.DesignMode)
		{
			if (fontForDesignMode == null)
			{
				fontForDesignMode = new Font("Calibri", 24f, FontStyle.Regular);
			}
			e.Graphics.Clear(System.Drawing.Color.WhiteSmoke);
			string s = "SharpDX RenderControl";
			SizeF sizeF = e.Graphics.MeasureString(s, fontForDesignMode);
			e.Graphics.DrawString(s, fontForDesignMode, new SolidBrush(System.Drawing.Color.Black), ((float)base.Width - sizeF.Width) / 2f, ((float)base.Height - sizeF.Height) / 2f);
		}
	}
}
