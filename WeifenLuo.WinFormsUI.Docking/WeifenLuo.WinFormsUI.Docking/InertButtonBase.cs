using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public abstract class InertButtonBase : Control
{
	private bool m_isMouseOver;

	private bool m_isMouseDown;

	public abstract Bitmap HoverImage { get; }

	public abstract Bitmap PressImage { get; }

	public abstract Bitmap Image { get; }

	protected bool IsMouseOver
	{
		get
		{
			return m_isMouseOver;
		}
		private set
		{
			if (m_isMouseOver != value)
			{
				m_isMouseOver = value;
				Invalidate();
			}
		}
	}

	protected bool IsMouseDown
	{
		get
		{
			return m_isMouseDown;
		}
		private set
		{
			if (m_isMouseDown != value)
			{
				m_isMouseDown = value;
				Invalidate();
			}
		}
	}

	protected override Size DefaultSize => Resources.DockPane_Close.Size;

	protected InertButtonBase()
	{
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		BackColor = Color.Transparent;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		bool flag = base.ClientRectangle.Contains(e.X, e.Y);
		if (IsMouseOver != flag)
		{
			IsMouseOver = flag;
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		if (!IsMouseOver)
		{
			IsMouseOver = true;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (IsMouseOver)
		{
			IsMouseOver = false;
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		if (!IsMouseDown)
		{
			IsMouseDown = true;
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		if (IsMouseDown)
		{
			IsMouseDown = false;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (HoverImage != null)
		{
			if (IsMouseOver && base.Enabled)
			{
				e.Graphics.DrawImage(IsMouseDown ? PressImage : HoverImage, (PatchController.EnableHighDpi == true) ? base.ClientRectangle : new Rectangle(0, 0, Image.Width, Image.Height));
			}
			else
			{
				e.Graphics.DrawImage(Image, (PatchController.EnableHighDpi == true) ? base.ClientRectangle : new Rectangle(0, 0, Image.Width, Image.Height));
			}
			base.OnPaint(e);
			return;
		}
		if (IsMouseOver && base.Enabled)
		{
			using Pen pen = new Pen(ForeColor);
			e.Graphics.DrawRectangle(pen, Rectangle.Inflate(base.ClientRectangle, -1, -1));
		}
		using (ImageAttributes imageAttributes = new ImageAttributes())
		{
			ColorMap[] array = new ColorMap[2]
			{
				new ColorMap(),
				null
			};
			array[0].OldColor = Color.FromArgb(0, 0, 0);
			array[0].NewColor = ForeColor;
			array[1] = new ColorMap();
			array[1].OldColor = Image.GetPixel(0, 0);
			array[1].NewColor = Color.Transparent;
			imageAttributes.SetRemapTable(array);
			e.Graphics.DrawImage(Image, new Rectangle(0, 0, Image.Width, Image.Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes);
		}
		base.OnPaint(e);
	}

	public void RefreshChanges()
	{
		if (!base.IsDisposed)
		{
			bool flag = base.ClientRectangle.Contains(PointToClient(Control.MousePosition));
			if (flag != IsMouseOver)
			{
				IsMouseOver = flag;
			}
			OnRefreshChanges();
		}
	}

	protected virtual void OnRefreshChanges()
	{
	}
}
