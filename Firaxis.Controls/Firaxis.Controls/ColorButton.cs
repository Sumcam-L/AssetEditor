using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class ColorButton : UserControl
{
	private Color m_kColor;

	private IContainer components = null;

	private ColorDialog ColorPicker;

	public Color Color
	{
		get
		{
			return m_kColor;
		}
		set
		{
			m_kColor = value;
			Invalidate();
			this.ValueChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public event EventHandler ValueChanged;

	public ColorButton()
	{
		InitializeComponent();
		Color = Color.FromArgb(255, 255, 255, 255);
		Invalidate();
	}

	private void ColorButton_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.Width--;
		clientRectangle.Height--;
		clientRectangle.Offset(1, 1);
		Pen black = Pens.Black;
		graphics.DrawRectangle(black, base.ClientRectangle);
		using SolidBrush brush = new SolidBrush(Color);
		using SolidBrush brush2 = new SolidBrush(Color.FromArgb(255, 255 - Color.R, 255 - Color.G, 255 - Color.B));
		graphics.FillRectangle(brush, clientRectangle);
		graphics.DrawString(Text, Font, brush2, base.ClientRectangle);
	}

	private void ColorButton_Click(object sender, EventArgs e)
	{
		if (ColorPicker.ShowDialog(this) == DialogResult.OK)
		{
			Color = ColorPicker.Color;
		}
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
		this.ColorPicker = new System.Windows.Forms.ColorDialog();
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Name = "ColorButton";
		base.Size = new System.Drawing.Size(50, 27);
		base.Paint += new System.Windows.Forms.PaintEventHandler(ColorButton_Paint);
		base.Click += new System.EventHandler(ColorButton_Click);
		base.ResumeLayout(false);
	}
}
