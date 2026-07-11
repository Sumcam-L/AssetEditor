using System;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Direct2D;

public class Direct2DControl : Control
{
	public D2dHwndGraphics D2dGraphics { get; private set; }

	public event EventHandler DrawingD2d;

	public Direct2DControl()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, value: true);
		D2dGraphics = D2dFactory.CreateD2dHwndGraphics(base.Handle);
	}

	public void DrawD2d()
	{
		OnBeginDrawD2d();
		OnDrawingD2d();
		OnEndDrawD2d();
	}

	protected virtual void OnBeginDrawD2d()
	{
		D2dGraphics.BeginDraw();
		D2dGraphics.Transform = Matrix3x2F.Identity;
		D2dGraphics.Clear(BackColor);
	}

	protected virtual void OnDrawingD2d()
	{
		this.DrawingD2d.Raise(this, EventArgs.Empty);
	}

	protected virtual void OnEndDrawD2d()
	{
		D2dGraphics.EndDraw();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		DrawD2d();
	}

	protected override void OnResize(EventArgs e)
	{
		D2dGraphics.Resize(base.ClientSize);
		base.OnResize(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			D2dGraphics.Dispose();
		}
		base.Dispose(disposing);
	}
}
