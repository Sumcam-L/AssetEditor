using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ColorEditing;

public class ColorBox : Control
{
	public enum eDrawStyle
	{
		Hue,
		Saturation,
		Brightness,
		Red,
		Green,
		Blue
	}

	private int m_iMarker_X = 0;

	private int m_iMarker_Y = 0;

	private bool m_bDragging = false;

	private eDrawStyle m_eDrawStyle = eDrawStyle.Hue;

	private AdobeColors.HSL m_hsl;

	private Color m_rgb;

	private readonly Container components = null;

	public eDrawStyle DrawStyle
	{
		get
		{
			return m_eDrawStyle;
		}
		set
		{
			m_eDrawStyle = value;
			Reset_Marker();
			Redraw_Control();
		}
	}

	public AdobeColors.HSL HSL
	{
		get
		{
			return m_hsl;
		}
		set
		{
			m_hsl = value;
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			Reset_Marker();
			Redraw_Control();
		}
	}

	public Color RGB
	{
		get
		{
			return m_rgb;
		}
		set
		{
			m_rgb = value;
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			Reset_Marker();
			Redraw_Control();
		}
	}

	public event EventHandler ColorChanged;

	public ColorBox()
	{
		InitializeComponent();
		m_hsl = new AdobeColors.HSL();
		m_hsl.H = 1.0;
		m_hsl.S = 1.0;
		m_hsl.L = 1.0;
		m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
		m_eDrawStyle = eDrawStyle.Hue;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.ColorEditing.ColorBox));
		base.SuspendLayout();
		base.Name = "ColorBox";
		resources.ApplyResources(this, "$this");
		base.VisibleChanged += new System.EventHandler(ctrl2DColorBox_Load);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(ctrl2DColorBox_MouseDown);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(ctrl2DColorBox_MouseMove);
		base.Resize += new System.EventHandler(ctrl2DColorBox_Resize);
		base.Paint += new System.Windows.Forms.PaintEventHandler(ctrl2DColorBox_Paint);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(ctrl2DColorBox_MouseUp);
		base.ResumeLayout(false);
		this.DoubleBuffered = true;
	}

	private void ctrl2DColorBox_Load(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			Redraw_Control();
		}
	}

	private void ctrl2DColorBox_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		m_bDragging = true;
		int num = e.X - 2;
		int num2 = e.Y - 2;
		if (num < 0)
		{
			num = 0;
		}
		if (num > base.Width - 4)
		{
			num = base.Width - 4;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > base.Height - 4)
		{
			num2 = base.Height - 4;
		}
		if (num != m_iMarker_X || num2 != m_iMarker_Y)
		{
			m_iMarker_X = num;
			m_iMarker_Y = num2;
			Redraw_Control();
			ResetHSLRGB();
			if (this.ColorChanged != null)
			{
				this.ColorChanged(this, e);
			}
		}
	}

	private void ctrl2DColorBox_MouseMove(object sender, MouseEventArgs e)
	{
		if (!m_bDragging)
		{
			return;
		}
		int num = e.X - 2;
		int num2 = e.Y - 2;
		if (num < 0)
		{
			num = 0;
		}
		if (num > base.Width - 4)
		{
			num = base.Width - 4;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > base.Height - 4)
		{
			num2 = base.Height - 4;
		}
		if (num != m_iMarker_X || num2 != m_iMarker_Y)
		{
			m_iMarker_X = num;
			m_iMarker_Y = num2;
			Redraw_Control();
			ResetHSLRGB();
			if (this.ColorChanged != null)
			{
				this.ColorChanged(this, e);
			}
		}
	}

	private void ctrl2DColorBox_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left || !m_bDragging)
		{
			return;
		}
		m_bDragging = false;
		int num = e.X - 2;
		int num2 = e.Y - 2;
		if (num < 0)
		{
			num = 0;
		}
		if (num > base.Width - 4)
		{
			num = base.Width - 4;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > base.Height - 4)
		{
			num2 = base.Height - 4;
		}
		if (num != m_iMarker_X || num2 != m_iMarker_Y)
		{
			m_iMarker_X = num;
			m_iMarker_Y = num2;
			Redraw_Control();
			ResetHSLRGB();
			if (this.ColorChanged != null)
			{
				this.ColorChanged(this, e);
			}
		}
	}

	private void ctrl2DColorBox_Resize(object sender, EventArgs e)
	{
		Redraw_Control();
	}

	private void ctrl2DColorBox_Paint(object sender, PaintEventArgs e)
	{
		DrawMarker(m_iMarker_X, m_iMarker_Y, e.Graphics);
		DrawBorder(e.Graphics);
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		Graphics graphics = pevent.Graphics;
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			Draw_Style_Hue(graphics);
			break;
		case eDrawStyle.Saturation:
			Draw_Style_Saturation(graphics);
			break;
		case eDrawStyle.Brightness:
			Draw_Style_Luminance(graphics);
			break;
		case eDrawStyle.Red:
			Draw_Style_Red(graphics);
			break;
		case eDrawStyle.Green:
			Draw_Style_Green(graphics);
			break;
		case eDrawStyle.Blue:
			Draw_Style_Blue(graphics);
			break;
		}
	}

	private void DrawMarker(int x, int y, Graphics g)
	{
		AdobeColors.HSL color = GetColor(x, y);
		Pen pen = ((color.L < 40.0 / 51.0) ? new Pen(Color.White) : ((!(color.H < 13.0 / 180.0) && !(color.H > 5.0 / 9.0)) ? new Pen(Color.Black) : ((!(color.S > 14.0 / 51.0)) ? new Pen(Color.Black) : new Pen(Color.White))));
		using (pen)
		{
			g.DrawEllipse(pen, x - 3, y - 3, 10, 10);
		}
	}

	private void DrawBorder(Graphics g)
	{
		Pen pen;
		using (pen = new Pen(Color.FromArgb(172, 168, 153)))
		{
			g.DrawLine(pen, base.Width - 2, 0, 0, 0);
			g.DrawLine(pen, 0, 0, 0, base.Height - 2);
		}
		using (pen = new Pen(Color.White))
		{
			g.DrawLine(pen, base.Width - 1, 0, base.Width - 1, base.Height - 1);
			g.DrawLine(pen, base.Width - 1, base.Height - 1, 0, base.Height - 1);
		}
		using (pen = new Pen(Color.Black))
		{
			g.DrawRectangle(pen, 1, 1, base.Width - 3, base.Height - 3);
		}
	}

	private void Draw_Style_Hue(Graphics g)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		AdobeColors.HSL hSL2 = new AdobeColors.HSL();
		hSL.H = m_hsl.H;
		hSL2.H = m_hsl.H;
		hSL.S = 0.0;
		hSL2.S = 1.0;
		for (int i = 0; i < base.Height - 4; i++)
		{
			hSL.L = 1.0 - (double)i / (double)(base.Height - 4);
			hSL2.L = hSL.L;
			LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(2, 2, base.Width - 4, 1), AdobeColors.HSL_to_RGB(hSL), AdobeColors.HSL_to_RGB(hSL2), 0f, isAngleScaleable: false);
			g.FillRectangle(brush, new Rectangle(2, i + 2, base.Width - 4, 1));
		}
	}

	private void Draw_Style_Saturation(Graphics g)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		AdobeColors.HSL hSL2 = new AdobeColors.HSL();
		hSL.S = m_hsl.S;
		hSL2.S = m_hsl.S;
		hSL.L = 1.0;
		hSL2.L = 0.0;
		for (int i = 0; i < base.Width - 4; i++)
		{
			hSL.H = (double)i / (double)(base.Width - 4);
			hSL2.H = hSL.H;
			LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(2, 2, 1, base.Height - 4), AdobeColors.HSL_to_RGB(hSL), AdobeColors.HSL_to_RGB(hSL2), 90f, isAngleScaleable: false);
			g.FillRectangle(brush, new Rectangle(i + 2, 2, 1, base.Height - 4));
		}
	}

	private void Draw_Style_Luminance(Graphics g)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		AdobeColors.HSL hSL2 = new AdobeColors.HSL();
		hSL.L = m_hsl.L;
		hSL2.L = m_hsl.L;
		hSL.S = 1.0;
		hSL2.S = 0.0;
		for (int i = 0; i < base.Width - 4; i++)
		{
			hSL.H = (double)i / (double)(base.Width - 4);
			hSL2.H = hSL.H;
			LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(2, 2, 1, base.Height - 4), AdobeColors.HSL_to_RGB(hSL), AdobeColors.HSL_to_RGB(hSL2), 90f, isAngleScaleable: false);
			g.FillRectangle(brush, new Rectangle(i + 2, 2, 1, base.Height - 4));
		}
	}

	private void Draw_Style_Red(Graphics g)
	{
		int r = m_rgb.R;
		for (int i = 0; i < base.Height - 4; i++)
		{
			int green = Round(255.0 - 255.0 * (double)i / (double)(base.Height - 4));
			LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(2, 2, base.Width - 4, 1), Color.FromArgb(r, green, 0), Color.FromArgb(r, green, 255), 0f, isAngleScaleable: false);
			g.FillRectangle(brush, new Rectangle(2, i + 2, base.Width - 4, 1));
		}
	}

	private void Draw_Style_Green(Graphics g)
	{
		int g2 = m_rgb.G;
		for (int i = 0; i < base.Height - 4; i++)
		{
			int red = Round(255.0 - 255.0 * (double)i / (double)(base.Height - 4));
			LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(2, 2, base.Width - 4, 1), Color.FromArgb(red, g2, 0), Color.FromArgb(red, g2, 255), 0f, isAngleScaleable: false);
			g.FillRectangle(brush, new Rectangle(2, i + 2, base.Width - 4, 1));
		}
	}

	private void Draw_Style_Blue(Graphics g)
	{
		int b = m_rgb.B;
		for (int i = 0; i < base.Height - 4; i++)
		{
			int green = Round(255.0 - 255.0 * (double)i / (double)(base.Height - 4));
			LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(2, 2, base.Width - 4, 1), Color.FromArgb(0, green, b), Color.FromArgb(255, green, b), 0f, isAngleScaleable: false);
			g.FillRectangle(brush, new Rectangle(2, i + 2, base.Width - 4, 1));
		}
	}

	private void Redraw_Control()
	{
		Refresh();
	}

	private void Reset_Marker()
	{
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			m_iMarker_X = Round((double)(base.Width - 4) * m_hsl.S);
			m_iMarker_Y = Round((double)(base.Height - 4) * (1.0 - m_hsl.L));
			break;
		case eDrawStyle.Saturation:
			m_iMarker_X = Round((double)(base.Width - 4) * m_hsl.H);
			m_iMarker_Y = Round((double)(base.Height - 4) * (1.0 - m_hsl.L));
			break;
		case eDrawStyle.Brightness:
			m_iMarker_X = Round((double)(base.Width - 4) * m_hsl.H);
			m_iMarker_Y = Round((double)(base.Height - 4) * (1.0 - m_hsl.S));
			break;
		case eDrawStyle.Red:
			m_iMarker_X = Round((double)(base.Width - 4) * (double)(int)m_rgb.B / 255.0);
			m_iMarker_Y = Round((double)(base.Height - 4) * (1.0 - (double)(int)m_rgb.G / 255.0));
			break;
		case eDrawStyle.Green:
			m_iMarker_X = Round((double)(base.Width - 4) * (double)(int)m_rgb.B / 255.0);
			m_iMarker_Y = Round((double)(base.Height - 4) * (1.0 - (double)(int)m_rgb.R / 255.0));
			break;
		case eDrawStyle.Blue:
			m_iMarker_X = Round((double)(base.Width - 4) * (double)(int)m_rgb.R / 255.0);
			m_iMarker_Y = Round((double)(base.Height - 4) * (1.0 - (double)(int)m_rgb.G / 255.0));
			break;
		}
	}

	private void ResetHSLRGB()
	{
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			m_hsl.S = (double)m_iMarker_X / (double)(base.Width - 4);
			m_hsl.L = 1.0 - (double)m_iMarker_Y / (double)(base.Height - 4);
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			break;
		case eDrawStyle.Saturation:
			m_hsl.H = (double)m_iMarker_X / (double)(base.Width - 4);
			m_hsl.L = 1.0 - (double)m_iMarker_Y / (double)(base.Height - 4);
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			break;
		case eDrawStyle.Brightness:
			m_hsl.H = (double)m_iMarker_X / (double)(base.Width - 4);
			m_hsl.S = 1.0 - (double)m_iMarker_Y / (double)(base.Height - 4);
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			break;
		case eDrawStyle.Red:
		{
			int blue = Round(255.0 * (double)m_iMarker_X / (double)(base.Width - 4));
			int green = Round(255.0 * (1.0 - (double)m_iMarker_Y / (double)(base.Height - 4)));
			m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, green, blue);
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			break;
		}
		case eDrawStyle.Green:
		{
			int blue = Round(255.0 * (double)m_iMarker_X / (double)(base.Width - 4));
			int red = Round(255.0 * (1.0 - (double)m_iMarker_Y / (double)(base.Height - 4)));
			m_rgb = Color.FromArgb(m_rgb.A, red, m_rgb.G, blue);
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			break;
		}
		case eDrawStyle.Blue:
		{
			int red = Round(255.0 * (double)m_iMarker_X / (double)(base.Width - 4));
			int green = Round(255.0 * (1.0 - (double)m_iMarker_Y / (double)(base.Height - 4)));
			m_rgb = Color.FromArgb(m_rgb.A, red, green, m_rgb.B);
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			break;
		}
		}
	}

	private int Round(double val)
	{
		int num = (int)val;
		int num2 = (int)(val * 100.0);
		if (num2 % 100 >= 50)
		{
			num++;
		}
		return num;
	}

	private AdobeColors.HSL GetColor(int x, int y)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			hSL.H = m_hsl.H;
			hSL.S = (double)x / (double)(base.Width - 4);
			hSL.L = 1.0 - (double)y / (double)(base.Height - 4);
			break;
		case eDrawStyle.Saturation:
			hSL.S = m_hsl.S;
			hSL.H = (double)x / (double)(base.Width - 4);
			hSL.L = 1.0 - (double)y / (double)(base.Height - 4);
			break;
		case eDrawStyle.Brightness:
			hSL.L = m_hsl.L;
			hSL.H = (double)x / (double)(base.Width - 4);
			hSL.S = 1.0 - (double)y / (double)(base.Height - 4);
			break;
		case eDrawStyle.Red:
			hSL = AdobeColors.RGB_to_HSL(Color.FromArgb(m_rgb.R, Round(255.0 * (1.0 - (double)y / (double)(base.Height - 4))), Round(255.0 * (double)x / (double)(base.Width - 4))));
			break;
		case eDrawStyle.Green:
			hSL = AdobeColors.RGB_to_HSL(Color.FromArgb(Round(255.0 * (1.0 - (double)y / (double)(base.Height - 4))), m_rgb.G, Round(255.0 * (double)x / (double)(base.Width - 4))));
			break;
		case eDrawStyle.Blue:
			hSL = AdobeColors.RGB_to_HSL(Color.FromArgb(Round(255.0 * (double)x / (double)(base.Width - 4)), Round(255.0 * (1.0 - (double)y / (double)(base.Height - 4))), m_rgb.B));
			break;
		}
		return hSL;
	}
}
