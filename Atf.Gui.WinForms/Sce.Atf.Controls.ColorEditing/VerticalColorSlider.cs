using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.ColorEditing;

internal class VerticalColorSlider : UserControl
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

	private int m_iMarker_Start_Y = 0;

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
			Reset_Slider();
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
			Reset_Slider();
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
			Reset_Slider();
			Redraw_Control();
		}
	}

	public event EventHandler ColorChanged;

	public VerticalColorSlider()
	{
		InitializeComponent();
		m_hsl = new AdobeColors.HSL();
		m_hsl.A = 1.0;
		m_hsl.H = 1.0;
		m_hsl.S = 1.0;
		m_hsl.L = 1.0;
		m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
		m_eDrawStyle = eDrawStyle.Hue;
		DoubleBuffered = true;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.ColorEditing.VerticalColorSlider));
		base.SuspendLayout();
		base.Name = "VerticalColorSlider";
		resources.ApplyResources(this, "$this");
		base.Load += new System.EventHandler(ctrl1DColorBar_Load);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(ctrl1DColorBar_MouseDown);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(ctrl1DColorBar_MouseMove);
		base.Resize += new System.EventHandler(ctrl1DColorBar_Resize);
		base.Paint += new System.Windows.Forms.PaintEventHandler(ctrl1DColorBar_Paint);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(ctrl1DColorBar_MouseUp);
		base.ResumeLayout(false);
	}

	private void ctrl1DColorBar_Load(object sender, EventArgs e)
	{
		Redraw_Control();
	}

	private void ctrl1DColorBar_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		m_bDragging = true;
		int num = e.Y;
		num -= 4;
		if (num < 0)
		{
			num = 0;
		}
		if (num > base.Height - 9)
		{
			num = base.Height - 9;
		}
		if (num != m_iMarker_Start_Y)
		{
			m_iMarker_Start_Y = num;
			Redraw_Control();
			ResetHSLRGB();
			if (this.ColorChanged != null)
			{
				this.ColorChanged(this, e);
			}
		}
	}

	private void ctrl1DColorBar_MouseMove(object sender, MouseEventArgs e)
	{
		if (!m_bDragging)
		{
			return;
		}
		int num = e.Y;
		num -= 4;
		if (num < 0)
		{
			num = 0;
		}
		if (num > base.Height - 9)
		{
			num = base.Height - 9;
		}
		if (num != m_iMarker_Start_Y)
		{
			m_iMarker_Start_Y = num;
			Redraw_Control();
			ResetHSLRGB();
			if (this.ColorChanged != null)
			{
				this.ColorChanged(this, e);
			}
		}
	}

	private void ctrl1DColorBar_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		m_bDragging = false;
		int num = e.Y;
		num -= 4;
		if (num < 0)
		{
			num = 0;
		}
		if (num > base.Height - 9)
		{
			num = base.Height - 9;
		}
		if (num != m_iMarker_Start_Y)
		{
			m_iMarker_Start_Y = num;
			Redraw_Control();
			ResetHSLRGB();
			if (this.ColorChanged != null)
			{
				this.ColorChanged(this, e);
			}
		}
	}

	private void ctrl1DColorBar_Paint(object sender, PaintEventArgs e)
	{
		DrawSlider(m_iMarker_Start_Y, e.Graphics);
		DrawBorder(e.Graphics);
		DrawContent(e.Graphics);
	}

	private void ctrl1DColorBar_Resize(object sender, EventArgs e)
	{
		Redraw_Control();
	}

	private void DrawSlider(int position, Graphics g)
	{
		using Pen pen = new Pen(Color.FromArgb(116, 114, 106));
		Brush white = Brushes.White;
		Point[] array = new Point[7]
		{
			new Point(1, position),
			new Point(3, position),
			new Point(7, position + 4),
			new Point(3, position + 8),
			new Point(1, position + 8),
			new Point(0, position + 7),
			new Point(0, position + 1)
		};
		g.FillPolygon(white, array);
		g.DrawPolygon(pen, array);
		array[0] = new Point(base.Width - 2, position);
		array[1] = new Point(base.Width - 4, position);
		array[2] = new Point(base.Width - 8, position + 4);
		array[3] = new Point(base.Width - 4, position + 8);
		array[4] = new Point(base.Width - 2, position + 8);
		array[5] = new Point(base.Width - 1, position + 7);
		array[6] = new Point(base.Width - 1, position + 1);
		g.FillPolygon(white, array);
		g.DrawPolygon(pen, array);
	}

	private void DrawBorder(Graphics g)
	{
		using (Pen pen = new Pen(Color.FromArgb(172, 168, 153)))
		{
			g.DrawLine(pen, base.Width - 10, 2, 9, 2);
			g.DrawLine(pen, 9, 2, 9, base.Height - 4);
		}
		using (Pen pen2 = new Pen(Color.White))
		{
			g.DrawLine(pen2, base.Width - 9, 2, base.Width - 9, base.Height - 3);
			g.DrawLine(pen2, base.Width - 9, base.Height - 3, 9, base.Height - 3);
		}
		using Pen pen3 = new Pen(Color.Black);
		g.DrawRectangle(pen3, 10, 3, base.Width - 20, base.Height - 7);
	}

	private void DrawContent(Graphics g)
	{
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			Draw_Style_Hue(g);
			break;
		case eDrawStyle.Saturation:
			Draw_Style_Saturation(g);
			break;
		case eDrawStyle.Brightness:
			Draw_Style_Luminance(g);
			break;
		case eDrawStyle.Red:
			Draw_Style_Red(g);
			break;
		case eDrawStyle.Green:
			Draw_Style_Green(g);
			break;
		case eDrawStyle.Blue:
			Draw_Style_Blue(g);
			break;
		}
	}

	private void Draw_Style_Hue(Graphics g)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		hSL.S = 1.0;
		hSL.L = 1.0;
		for (int i = 0; i < base.Height - 8; i++)
		{
			hSL.H = 1.0 - (double)i / (double)(base.Height - 8);
			using Pen pen = new Pen(AdobeColors.HSL_to_RGB(hSL));
			g.DrawLine(pen, 11, i + 4, base.Width - 11, i + 4);
		}
	}

	private void Draw_Style_Saturation(Graphics g)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		hSL.H = m_hsl.H;
		hSL.L = m_hsl.L;
		for (int i = 0; i < base.Height - 8; i++)
		{
			hSL.S = 1.0 - (double)i / (double)(base.Height - 8);
			using Pen pen = new Pen(AdobeColors.HSL_to_RGB(hSL));
			g.DrawLine(pen, 11, i + 4, base.Width - 11, i + 4);
		}
	}

	private void Draw_Style_Luminance(Graphics g)
	{
		AdobeColors.HSL hSL = new AdobeColors.HSL();
		hSL.H = m_hsl.H;
		hSL.S = m_hsl.S;
		for (int i = 0; i < base.Height - 8; i++)
		{
			hSL.L = 1.0 - (double)i / (double)(base.Height - 8);
			using Pen pen = new Pen(AdobeColors.HSL_to_RGB(hSL));
			g.DrawLine(pen, 11, i + 4, base.Width - 11, i + 4);
		}
	}

	private void Draw_Style_Red(Graphics g)
	{
		for (int i = 0; i < base.Height - 8; i++)
		{
			int red = 255 - Round(255.0 * (double)i / (double)(base.Height - 8));
			using Pen pen = new Pen(Color.FromArgb(red, m_rgb.G, m_rgb.B));
			g.DrawLine(pen, 11, i + 4, base.Width - 11, i + 4);
		}
	}

	private void Draw_Style_Green(Graphics g)
	{
		for (int i = 0; i < base.Height - 8; i++)
		{
			int green = 255 - Round(255.0 * (double)i / (double)(base.Height - 8));
			using Pen pen = new Pen(Color.FromArgb(m_rgb.R, green, m_rgb.B));
			g.DrawLine(pen, 11, i + 4, base.Width - 11, i + 4);
		}
	}

	private void Draw_Style_Blue(Graphics g)
	{
		for (int i = 0; i < base.Height - 8; i++)
		{
			int blue = 255 - Round(255.0 * (double)i / (double)(base.Height - 8));
			using Pen pen = new Pen(Color.FromArgb(m_rgb.R, m_rgb.G, blue));
			g.DrawLine(pen, 11, i + 4, base.Width - 11, i + 4);
		}
	}

	private void Redraw_Control()
	{
		Refresh();
	}

	private void Reset_Slider()
	{
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			m_iMarker_Start_Y = base.Height - 8 - Round((double)(base.Height - 8) * m_hsl.H);
			break;
		case eDrawStyle.Saturation:
			m_iMarker_Start_Y = base.Height - 8 - Round((double)(base.Height - 8) * m_hsl.S);
			break;
		case eDrawStyle.Brightness:
			m_iMarker_Start_Y = base.Height - 8 - Round((double)(base.Height - 8) * m_hsl.L);
			break;
		case eDrawStyle.Red:
			m_iMarker_Start_Y = base.Height - 8 - Round((double)(base.Height - 8) * (double)(int)m_rgb.R / 255.0);
			break;
		case eDrawStyle.Green:
			m_iMarker_Start_Y = base.Height - 8 - Round((double)(base.Height - 8) * (double)(int)m_rgb.G / 255.0);
			break;
		case eDrawStyle.Blue:
			m_iMarker_Start_Y = base.Height - 8 - Round((double)(base.Height - 8) * (double)(int)m_rgb.B / 255.0);
			break;
		}
	}

	private void ResetHSLRGB()
	{
		switch (m_eDrawStyle)
		{
		case eDrawStyle.Hue:
			m_hsl.H = 1.0 - (double)m_iMarker_Start_Y / (double)(base.Height - 9);
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			break;
		case eDrawStyle.Saturation:
			m_hsl.S = 1.0 - (double)m_iMarker_Start_Y / (double)(base.Height - 9);
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			break;
		case eDrawStyle.Brightness:
			m_hsl.L = 1.0 - (double)m_iMarker_Start_Y / (double)(base.Height - 9);
			m_rgb = AdobeColors.HSL_to_RGB(m_hsl);
			break;
		case eDrawStyle.Red:
			m_rgb = Color.FromArgb(m_rgb.A, 255 - Round(255.0 * (double)m_iMarker_Start_Y / (double)(base.Height - 9)), m_rgb.G, m_rgb.B);
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			break;
		case eDrawStyle.Green:
			m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, 255 - Round(255.0 * (double)m_iMarker_Start_Y / (double)(base.Height - 9)), m_rgb.B);
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			break;
		case eDrawStyle.Blue:
			m_rgb = Color.FromArgb(m_rgb.A, m_rgb.R, m_rgb.G, 255 - Round(255.0 * (double)m_iMarker_Start_Y / (double)(base.Height - 9)));
			m_hsl = AdobeColors.RGB_to_HSL(m_rgb);
			break;
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
}
