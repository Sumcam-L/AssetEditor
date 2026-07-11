using System;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Controls.ColorEditing;

namespace Firaxis.AssetPreviewing;

public class ColorPickerControl : Control
{
	private ColorBox m_colorPicker;

	private Panel m_huePicker;

	public Color SelectedColor
	{
		get
		{
			return m_colorPicker.RGB;
		}
		set
		{
			if (!(value == m_colorPicker.RGB))
			{
				m_colorPicker.RGB = value;
			}
		}
	}

	public event EventHandler ColorChanged;

	public event EventHandler ColorCommited;

	public ColorPickerControl()
	{
		m_huePicker = new Panel();
		m_huePicker.Paint += HuePicker_Paint;
		m_huePicker.MouseClick += HuePicker_MouseClick;
		m_huePicker.MouseMove += HuePicker_MouseMove;
		m_huePicker.BorderStyle = BorderStyle.Fixed3D;
		m_colorPicker = new ColorBox();
		m_colorPicker.ColorChanged += ColorPicker_ColorChanged;
		m_colorPicker.MouseMove += ColorPicker_MouseMove;
		m_colorPicker.MouseClick += ColorPicker_MouseClick;
		base.SizeChanged += ColorPickerControl_SizeChanged;
		SuspendLayout();
		base.Controls.Add(m_huePicker);
		base.Controls.Add(m_colorPicker);
		ResumeLayout();
	}

	private void ColorPickerControl_SizeChanged(object sender, EventArgs e)
	{
		int num = base.ClientRectangle.Height;
		int num2 = base.ClientRectangle.Width;
		int num3 = (int)((double)num2 * 0.85);
		int num4 = num2 - num3 - 1;
		int left = num3 + 1;
		Panel huePicker = m_huePicker;
		int num5 = (m_colorPicker.Height = num);
		huePicker.Height = num5;
		m_colorPicker.Width = num3;
		m_huePicker.Left = left;
		m_huePicker.Width = num4;
	}

	private void ColorPicker_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button.HasFlag(MouseButtons.Left))
		{
			this.ColorChanged.Raise(this, EventArgs.Empty);
		}
	}

	private void ColorPicker_MouseClick(object sender, MouseEventArgs e)
	{
		this.ColorChanged.Raise(this, EventArgs.Empty);
	}

	private void ColorPicker_ColorChanged(object sender, EventArgs e)
	{
		this.ColorChanged.Raise(this, EventArgs.Empty);
	}

	private Color HueToRGB(float hue)
	{
		BugSubmitter.Assert(hue >= 0f && hue < 360f, "Invalue hue!");
		int num = (int)(255f * (1f - Math.Abs(hue / 60f % 2f - 1f)));
		if (hue < 60f)
		{
			return Color.FromArgb(255, num, 0);
		}
		if (hue < 120f)
		{
			return Color.FromArgb(num, 255, 0);
		}
		if (hue < 180f)
		{
			return Color.FromArgb(0, 255, num);
		}
		if (hue < 240f)
		{
			return Color.FromArgb(0, num, 255);
		}
		if (hue < 300f)
		{
			return Color.FromArgb(num, 0, 255);
		}
		if (hue < 360f)
		{
			return Color.FromArgb(255, 0, num);
		}
		return Color.Magenta;
	}

	private void UpdateHue(int mousey)
	{
		float hue = Math.Min(Math.Max((float)(360 * mousey) / (float)base.ClientRectangle.Height, 0f), 360f);
		m_colorPicker.RGB = HueToRGB(hue);
	}

	private void HuePicker_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button.HasFlag(MouseButtons.Left))
		{
			UpdateHue(e.Location.Y);
		}
	}

	private void HuePicker_MouseClick(object sender, MouseEventArgs e)
	{
		UpdateHue(e.Location.Y);
	}

	private void HuePicker_Paint(object sender, PaintEventArgs e)
	{
		int num = base.ClientRectangle.Height;
		int x = base.ClientRectangle.Width;
		float num2 = 360f / (float)num;
		for (int i = 0; i < num; i++)
		{
			float hue = Math.Max(Math.Min(num2 * (float)i, 360f), 0f);
			Color color = HueToRGB(hue);
			using Pen pen = new Pen(color, 1f);
			e.Graphics.DrawLine(pen, 0, i, x, i);
		}
	}
}
