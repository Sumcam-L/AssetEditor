using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public class DragForm : Form
{
	public Color? BackgroundColor { get; set; }

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams obj = base.CreateParams;
			obj.ExStyle |= 134217856;
			return obj;
		}
	}

	protected override bool ShowWithoutActivation => true;

	public DragForm()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		SetStyle(ControlStyles.Selectable, value: false);
		base.Enabled = false;
		base.TopMost = true;
		base.SizeChanged += delegate
		{
			if (BackgroundColor.HasValue)
			{
				Invalidate();
			}
		};
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 132)
		{
			m.Result = (IntPtr)(-1);
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (BackgroundColor.HasValue)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 10 && clientRectangle.Height > 10)
			{
				Point location = new Point(clientRectangle.Location.X + 5, clientRectangle.Location.Y + 5);
				Rectangle rect = new Rectangle(size: new Size(clientRectangle.Width - 10, clientRectangle.Height - 10), location: location);
				e.Graphics.FillRectangle(new SolidBrush(BackgroundColor.Value), rect);
			}
		}
	}

	public virtual void Show(bool bActivate)
	{
		Show();
		if (bActivate)
		{
			Activate();
		}
	}
}
