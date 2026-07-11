using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class OverlayButton : IDisposable
{
	private bool m_disposed;

	private bool m_visible;

	private bool m_pressed;

	private bool m_mouseIn;

	private Bitmap m_backgroundImage;

	private Bitmap m_pressedImage;

	private Bitmap m_hoverImage;

	private Rectangle m_bound;

	public bool IsDisposed => m_disposed;

	public string ToolTipText { get; set; }

	public ToolTip ToolTip { get; set; }

	public Control Parent { get; set; }

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_visible = value;
		}
	}

	public string Text { get; set; }

	public int Top
	{
		get
		{
			return m_bound.Y;
		}
		set
		{
			m_bound.Y = value;
		}
	}

	public int Left
	{
		get
		{
			return m_bound.X;
		}
		set
		{
			m_bound.X = value;
		}
	}

	public int Width
	{
		get
		{
			return m_bound.Width;
		}
		set
		{
			m_bound.Width = value;
		}
	}

	public int Height
	{
		get
		{
			return m_bound.Height;
		}
		set
		{
			m_bound.Height = value;
		}
	}

	public Image PressedImage
	{
		get
		{
			return m_pressedImage;
		}
		set
		{
			SetImage(value, ref m_pressedImage);
		}
	}

	public Image HoverImage
	{
		get
		{
			return m_hoverImage;
		}
		set
		{
			SetImage(value, ref m_hoverImage);
		}
	}

	public Image BackgroundImage
	{
		get
		{
			return m_backgroundImage;
		}
		set
		{
			SetImage(value, ref m_backgroundImage);
		}
	}

	public event EventHandler Click = delegate
	{
	};

	public OverlayButton(Control parent)
	{
		Parent = parent;
		m_bound = new Rectangle(0, 0, 16, 16);
		Visible = true;
	}

	public void Dispose()
	{
		if (!m_disposed)
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				if (m_backgroundImage != null)
				{
					m_backgroundImage.Dispose();
				}
				if (m_pressedImage != null)
				{
					m_pressedImage.Dispose();
				}
				if (m_hoverImage != null)
				{
					m_hoverImage.Dispose();
				}
				m_backgroundImage = null;
				m_pressedImage = null;
				m_hoverImage = null;
				Parent = null;
			}
		}
		finally
		{
			m_disposed = true;
		}
	}

	~OverlayButton()
	{
		Dispose(disposing: false);
	}

	public bool MouseDown(MouseEventArgs e)
	{
		if (!Visible)
		{
			return false;
		}
		m_pressed = false;
		if (m_bound.Contains(e.Location) && e.Button == MouseButtons.Left)
		{
			m_pressed = true;
			this.Click(this, EventArgs.Empty);
		}
		return m_pressed;
	}

	public bool MouseMove(MouseEventArgs e)
	{
		if (!Visible)
		{
			return false;
		}
		bool mouseIn = m_mouseIn;
		m_mouseIn = m_bound.Contains(e.Location);
		if (m_mouseIn != mouseIn && Parent != null)
		{
			Parent.Invalidate();
		}
		return m_mouseIn;
	}

	public bool MouseUp(MouseEventArgs e)
	{
		m_pressed = false;
		if (!Visible)
		{
			return false;
		}
		return m_bound.Contains(e.Location);
	}

	public void Draw(Graphics g)
	{
		if (Parent == null || !Visible)
		{
			return;
		}
		if (m_backgroundImage == null)
		{
			g.DrawLine(Pens.Red, m_bound.Left, m_bound.Top, m_bound.Width, m_bound.Height);
			g.DrawLine(Pens.Red, m_bound.Right, m_bound.Top, m_bound.Left, m_bound.Height);
			return;
		}
		if (m_pressedImage == null)
		{
			m_pressedImage = CreateNewBitmap(m_backgroundImage, 0f, -0.1f);
		}
		if (m_hoverImage == null)
		{
			m_hoverImage = CreateNewBitmap(m_backgroundImage, 0.8f, 0.1f);
		}
		Image image = (m_pressed ? m_pressedImage : ((!m_mouseIn) ? m_backgroundImage : m_hoverImage));
		g.DrawImage(image, m_bound);
		if (string.IsNullOrWhiteSpace(Text))
		{
			return;
		}
		using Brush brush = new SolidBrush(Parent.ForeColor);
		SizeF sizeF = g.MeasureString(Text, Parent.Font);
		g.DrawString(Text, Parent.Font, brush, (float)m_bound.Left + ((float)m_bound.Width - sizeF.Width) / 2f, (float)m_bound.Top + ((float)m_bound.Height - sizeF.Height) / 2f);
	}

	private void SetImage(Image img, ref Bitmap target)
	{
		if (target != null)
		{
			target.Dispose();
			target = null;
		}
		if (img != null)
		{
			target = new Bitmap(img);
		}
	}

	private Bitmap CreateNewBitmap(Bitmap src, float saturation, float brightness)
	{
		Bitmap bitmap = (Bitmap)src.Clone();
		for (int i = 0; i < src.Height; i++)
		{
			for (int j = 0; j < src.Width; j++)
			{
				Color pixel = src.GetPixel(j, i);
				float s = MathUtil.Clamp(pixel.GetSaturation() + saturation, 0f, 1f);
				float b = MathUtil.Clamp(pixel.GetBrightness() + brightness, 0f, 1f);
				Color color = ColorUtil.FromAhsb(pixel.A, pixel.GetHue(), s, b);
				bitmap.SetPixel(j, i, color);
			}
		}
		return bitmap;
	}
}
