using System;
using System.Windows.Forms;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable;

public class D2dAdaptableControl : AdaptableControl, IPerformanceTarget
{
	private readonly D2dHwndGraphics m_d2dGraphics;

	public D2dGraphics D2dGraphics => m_d2dGraphics;

	public bool SuppressDraw { get; set; }

	public event EventHandler DrawingD2d;

	event EventHandler IPerformanceTarget.EventOccurred
	{
		add
		{
			DrawingD2d += value;
		}
		remove
		{
			DrawingD2d -= value;
		}
	}

	public D2dAdaptableControl()
	{
		DoubleBuffered = false;
		SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, value: true);
		m_d2dGraphics = D2dFactory.CreateD2dHwndGraphics(base.Handle);
	}

	public void DrawD2d()
	{
		if (!SuppressDraw)
		{
			OnBeginDrawD2d();
			OnDrawingD2d();
			OnEndDrawD2d();
		}
	}

	protected virtual void OnBeginDrawD2d()
	{
		D2dGraphics.BeginDraw();
		ITransformAdapter transformAdapter = As<ITransformAdapter>();
		D2dGraphics.Transform = ((transformAdapter == null) ? Matrix3x2F.Identity : ((Matrix3x2F)transformAdapter.Transform));
		D2dGraphics.Clear(BackColor);
		D2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
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

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
	}

	protected override void OnResize(EventArgs e)
	{
		m_d2dGraphics.Resize(base.ClientSize);
		base.OnResize(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_d2dGraphics.Dispose();
		}
		base.Dispose(disposing);
	}

	void IPerformanceTarget.DoEvent()
	{
		DrawD2d();
	}
}
