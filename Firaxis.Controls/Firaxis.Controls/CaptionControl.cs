using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Firaxis.Controls.Resources;

namespace Firaxis.Controls;

[ToolboxBitmap(typeof(ResourceTag), "caption.bmp")]
[Description("Displays information bar with an optional image.")]
public class CaptionControl : ScrollUserControl
{
	private Image image;

	private Icon icon;

	private Color transparent = Color.Magenta;

	private const int pad = 4;

	private IContainer components = null;

	[Category("Appearance")]
	[Description("Caption text to display")]
	public string Caption
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
			Invalidate();
		}
	}

	[Category("Appearance")]
	[DisplayName("ImageTransparentColor")]
	[Description("Specifies the transparency color for images")]
	public Color Transparent
	{
		get
		{
			return transparent;
		}
		set
		{
			transparent = value;
		}
	}

	[Category("Appearance")]
	[Description("Image to display on the caption")]
	public Image Image
	{
		get
		{
			return image;
		}
		set
		{
			image = value;
		}
	}

	[Category("Appearance")]
	[Description("Icon to display on the caption")]
	public Icon Icon
	{
		get
		{
			return icon;
		}
		set
		{
			icon = value;
		}
	}

	public CaptionControl()
	{
		InitializeComponent();
		DoubleBuffered = true;
	}

	private void CaptionControl_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		StringFormat stringFormat = new StringFormat();
		stringFormat.LineAlignment = StringAlignment.Center;
		stringFormat.Alignment = StringAlignment.Near;
		RectangleF layoutRectangle = new RectangleF(0f, 0f, base.Size.Width, base.Size.Height);
		if (image != null)
		{
			ImageAttributes imageAttributes = new ImageAttributes();
			imageAttributes.SetColorKey(transparent, transparent);
			layoutRectangle.X += 4f;
			graphics.DrawImage(destRect: new Rectangle((int)layoutRectangle.X, (int)layoutRectangle.Y + (int)layoutRectangle.Height / 2 - image.Height / 2, image.Width, image.Height), image: image, srcX: 0, srcY: 0, srcWidth: image.Width, srcHeight: image.Height, srcUnit: GraphicsUnit.Pixel, imageAttr: imageAttributes);
			layoutRectangle.X += image.Width;
		}
		if (icon != null)
		{
			layoutRectangle.X += 4f;
			graphics.DrawIcon(targetRect: new Rectangle((int)layoutRectangle.X, (int)layoutRectangle.Y + (int)layoutRectangle.Height / 2 - icon.Height / 2, icon.Width, icon.Height), icon: icon);
			layoutRectangle.X += icon.Width;
		}
		layoutRectangle.X += 4f;
		using SolidBrush brush = new SolidBrush(ForeColor);
		graphics.DrawString(Text, Font, brush, layoutRectangle, stringFormat);
	}

	private void CaptionControl_SizeChanged(object sender, EventArgs e)
	{
		Invalidate();
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
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Name = "CaptionControl";
		base.Size = new System.Drawing.Size(312, 57);
		base.Paint += new System.Windows.Forms.PaintEventHandler(CaptionControl_Paint);
		base.SizeChanged += new System.EventHandler(CaptionControl_SizeChanged);
		base.ResumeLayout(false);
	}
}
