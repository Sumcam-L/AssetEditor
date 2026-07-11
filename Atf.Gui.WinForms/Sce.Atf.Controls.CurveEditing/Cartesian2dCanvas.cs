using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public class Cartesian2dCanvas : Control
{
	private bool m_flipY;

	private Brush m_scaleTextBrush = new SolidBrush(Color.Black);

	private Font m_scaleTextFont;

	private Pen m_majorGridlinePen = new Pen(Color.FromArgb(180, 180, 180));

	private Pen m_minorGridlinePen = new Pen(Color.FromArgb(200, 200, 200));

	private Pen m_axisPen = new Pen(Color.Black);

	private Font m_axisLabelFont = new Font("Trebuchet MS", 13f);

	private static readonly double[] s_spanTable = new double[7] { 0.1, 0.2, 0.5, 1.0, 2.0, 4.0, 5.0 };

	protected Vec2F ClickPoint;

	protected Vec2F PreviousPoint;

	protected Vec2F CurrentPoint;

	protected Vec2F ClickGraphPoint;

	protected Vec2F PreviousGraphPoint;

	protected Vec2F CurrentGraphPoint;

	protected PointD ClickPan_d;

	protected PointD ClickZoom_d;

	protected RectangleF SelectionRect;

	private bool m_startDrag;

	private float m_dragThreshold = 3f;

	private bool m_dragOverThreshold;

	private PointD m_clientSize;

	private PointD m_trans;

	private PointD m_scale = new PointD(1.0, 1.0);

	private readonly Matrix m_xform = new Matrix();

	private double m_minOffsetX = -3.4028234663852886E+38;

	private double m_maxOffsetX = 3.4028234663852886E+38;

	private double m_minOffsetY = -3.4028234663852886E+38;

	private double m_maxOffsetY = 3.4028234663852886E+38;

	private double m_minZoom = 0.0001;

	private double m_maxZoom = 100000.0;

	private double m_numOfMinorTicks = 5.0;

	private double m_majorTickSpacing = 50.0;

	private const float GridTextMargin = 3f;

	private double m_majorTickY = 60.0;

	private double m_majorTickX = 60.0;

	private OriginLockMode m_lockorg = OriginLockMode.Free;

	public bool FlipY
	{
		get
		{
			return m_flipY;
		}
		set
		{
			m_flipY = value;
			m_scale.Y = (m_flipY ? Math.Abs(m_scale.Y) : (0.0 - Math.Abs(m_scale.Y)));
		}
	}

	public OriginLockMode LockOrigin
	{
		get
		{
			return m_lockorg;
		}
		set
		{
			m_lockorg = value;
			UpdatePan();
			Invalidate();
		}
	}

	public double MinZoom
	{
		get
		{
			return m_minZoom;
		}
		set
		{
			m_minZoom = Math.Min(value, m_maxZoom);
			Zoom_d = m_scale;
			Invalidate();
		}
	}

	public double MaxZoom
	{
		get
		{
			return m_maxZoom;
		}
		set
		{
			m_maxZoom = Math.Max(value, m_minZoom);
			Zoom_d = m_scale;
			Invalidate();
		}
	}

	public Brush ScaleTextBrush
	{
		get
		{
			return m_scaleTextBrush;
		}
		set
		{
			SetDisposableVar(ref m_scaleTextBrush, value);
		}
	}

	public Font ScaleTextFont
	{
		get
		{
			return m_scaleTextFont;
		}
		set
		{
			SetDisposableVar(ref m_scaleTextFont, value);
		}
	}

	public Pen MajorGridlinePen
	{
		get
		{
			return m_majorGridlinePen;
		}
		set
		{
			SetDisposableVar(ref m_majorGridlinePen, value);
		}
	}

	public Pen MinorGridlinePen
	{
		get
		{
			return m_minorGridlinePen;
		}
		set
		{
			SetDisposableVar(ref m_minorGridlinePen, value);
		}
	}

	public Pen AxisPen
	{
		get
		{
			return m_axisPen;
		}
		set
		{
			SetDisposableVar(ref m_axisPen, value);
		}
	}

	public Font AxisLabelFont
	{
		get
		{
			return m_axisLabelFont;
		}
		set
		{
			SetDisposableVar(ref m_axisLabelFont, value);
		}
	}

	public Color AxisColor
	{
		get
		{
			return m_axisPen.Color;
		}
		set
		{
			m_axisPen.Color = value;
		}
	}

	public Color GridColor
	{
		get
		{
			return m_majorGridlinePen.Color;
		}
		set
		{
			m_majorGridlinePen.Color = value;
		}
	}

	public Color TextColor
	{
		get
		{
			if (!(m_scaleTextBrush is SolidBrush solidBrush))
			{
				throw new InvalidOperationException("ScaleTextBrush is null or not a SolidBruhs");
			}
			return solidBrush.Color;
		}
		set
		{
			if (!(m_scaleTextBrush is SolidBrush solidBrush))
			{
				throw new InvalidOperationException("ScaleTextBrush is null or not a SolidBruhs");
			}
			solidBrush.Color = value;
		}
	}

	public PointD Pan_d
	{
		get
		{
			return m_trans;
		}
		set
		{
			double num = Math.Max(m_minOffsetX, Math.Min(m_maxOffsetX, value.X));
			double num2 = Math.Max(m_minOffsetY, Math.Min(m_maxOffsetY, value.Y));
			m_trans = new PointD(num, num2);
		}
	}

	public Vec2F Pan
	{
		get
		{
			return (Vec2F)m_trans;
		}
		set
		{
			double num = Math.Max(m_minOffsetX, Math.Min(m_maxOffsetX, value.X));
			double num2 = Math.Max(m_minOffsetY, Math.Min(m_maxOffsetY, value.Y));
			m_trans = new PointD(num, num2);
		}
	}

	public PointD Zoom_d
	{
		get
		{
			return new PointD
			{
				X = m_scale.X,
				Y = Math.Abs(m_scale.Y)
			};
		}
		set
		{
			double num = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.X));
			double num2 = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.Y));
			m_scale = (m_flipY ? new PointD(num, num2) : new PointD(num, 0.0 - num2));
			m_majorTickX = ComputeGridSpan(m_majorTickSpacing / num);
			m_majorTickY = ComputeGridSpan(m_majorTickSpacing / num2);
		}
	}

	public Vec2F Zoom
	{
		get
		{
			return new Vec2F
			{
				X = (float)m_scale.X,
				Y = (float)Math.Abs(m_scale.Y)
			};
		}
		set
		{
			double num = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.X));
			double num2 = Math.Max(m_minZoom, Math.Min(m_maxZoom, value.Y));
			m_scale = (m_flipY ? new PointD(num, num2) : new PointD(num, 0.0 - num2));
			m_majorTickX = ComputeGridSpan(m_majorTickSpacing / num);
			m_majorTickY = ComputeGridSpan(m_majorTickSpacing / num2);
		}
	}

	public float MajorTickX => (float)m_majorTickX;

	public float MajorTickY => (float)m_majorTickY;

	public float MinorTickX => (float)(m_majorTickX / m_numOfMinorTicks);

	public float MinorTickY => (float)(m_majorTickY / m_numOfMinorTicks);

	public float MajorTickSpacing
	{
		get
		{
			return (float)m_majorTickSpacing;
		}
		set
		{
			if (value < 10f)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			m_majorTickSpacing = value;
		}
	}

	public float NumOfMinorTicks
	{
		get
		{
			return (float)m_numOfMinorTicks;
		}
		set
		{
			if ((double)value >= m_majorTickSpacing)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			m_numOfMinorTicks = value;
		}
	}

	protected bool DraggingOverThreshold => m_dragOverThreshold;

	public Cartesian2dCanvas()
	{
		SuspendLayout();
		base.Name = "Cartesian2dCanvas";
		base.Size = new Size(400, 400);
		BackColor = Color.FromArgb(161, 161, 161);
		ResumeLayout(performLayout: false);
		m_scaleTextFont = new Font(Font.Name, 8.25f);
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		MinimumSize = new Size(10, 10);
		Zoom_d = new PointD(1.0, 1.0);
	}

	public void Frame(RectangleF rect)
	{
		float num = base.ClientSize.Height;
		float num2 = base.ClientSize.Width;
		float num3 = num / 2f;
		float num4 = num2 / 2f;
		if (rect.Width == 0f || rect.Height == 0f)
		{
			return;
		}
		switch (m_lockorg)
		{
		case OriginLockMode.Free:
		{
			Zoom = new Vec2F(num2 / rect.Width, num / rect.Height);
			Vec2F vec2F3 = new Vec2F(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
			Pan = new Vec2F(num4 - vec2F3.X * Zoom.X, num3 + vec2F3.Y * Zoom.Y);
			break;
		}
		case OriginLockMode.Center:
		{
			float val3 = Math.Abs(rect.Left);
			float val4 = Math.Abs(rect.Right);
			float num8 = Math.Max(val3, val4);
			float val5 = Math.Abs(rect.Top);
			float val6 = Math.Abs(rect.Bottom);
			float num9 = Math.Max(val5, val6);
			Zoom = new Vec2F(num4 / num8, num3 / num9);
			break;
		}
		case OriginLockMode.Left:
			if (rect.Right > 0f)
			{
				float num10 = Math.Max(0f, rect.Left);
				float num11 = ((num10 > 0f) ? rect.Width : rect.Right);
				Zoom = new Vec2F(num2 / num11, num / rect.Height);
				Vec2F vec2F = new Vec2F(num10 + num11 / 2f, rect.Y + rect.Height / 2f);
				Pan = new Vec2F(num4 - vec2F.X * Zoom.X, num3 + vec2F.Y * Zoom.Y);
			}
			break;
		case OriginLockMode.LeftTop:
			if (rect.Right > 0f && rect.Bottom > 0f)
			{
				float num12 = Math.Max(0f, rect.Left);
				float num13 = ((num12 > 0f) ? rect.Width : rect.Right);
				float num14 = Math.Max(0f, rect.Top);
				float num15 = ((num14 > 0f) ? rect.Height : rect.Bottom);
				Zoom = new Vec2F(num2 / num13, num / num15);
				Vec2F vec2F2 = new Vec2F(num12 + num13 / 2f, num14 + num15 / 2f);
				Pan = new Vec2F(num4 - vec2F2.X * Zoom.X, num3 + vec2F2.Y * Zoom.Y);
			}
			break;
		case OriginLockMode.LeftBottom:
			if (rect.Right > 0f && rect.Bottom > 0f)
			{
				float num16 = Math.Max(0f, rect.Left);
				float num17 = ((num16 > 0f) ? rect.Width : rect.Right);
				float num18 = Math.Max(0f, rect.Top);
				float num19 = ((num18 > 0f) ? rect.Height : rect.Bottom);
				Zoom = new Vec2F(num2 / num17, num / num19);
				Vec2F vec2F4 = new Vec2F(num16 + num17 / 2f, num18 + num19 / 2f);
				Pan = new Vec2F(num4 - vec2F4.X * Zoom.X, num3 + vec2F4.Y * Zoom.Y);
			}
			break;
		case OriginLockMode.LeftMiddle:
			if (rect.Right > 0f)
			{
				float num5 = Math.Max(0f, rect.Left);
				float num6 = ((num5 > 0f) ? rect.Width : rect.Right);
				float val = Math.Abs(rect.Top);
				float val2 = Math.Abs(rect.Bottom);
				float num7 = Math.Max(val, val2);
				Zoom = new Vec2F(num2 / num6, num3 / num7);
				Pan = new Vec2F(num4 - new Vec2F(num5 + num6 / 2f, 0f).X * Zoom.X, Pan.Y);
			}
			break;
		}
	}

	public void PanToOrigin(bool invalidate = true)
	{
		switch (m_lockorg)
		{
		case OriginLockMode.Free:
			Pan_d = new PointD(base.ClientSize.Width / 2, base.ClientSize.Height / 2);
			break;
		case OriginLockMode.LeftTop:
			Pan_d = new PointD(1.0, 1.0);
			break;
		case OriginLockMode.Left:
		case OriginLockMode.LeftMiddle:
			Pan_d = new PointD(1.0, base.ClientSize.Height / 2);
			break;
		case OriginLockMode.LeftBottom:
			Pan_d = new PointD(1.0, base.ClientSize.Height);
			break;
		}
		if (invalidate)
		{
			Invalidate();
		}
	}

	public void SetZoom(Point at, float xs, float ys)
	{
		PointD zoom_d = Zoom_d;
		PointD pointD = new PointD(((double)at.X - m_trans.X) / zoom_d.X, ((double)at.Y - m_trans.Y) / zoom_d.Y);
		Zoom_d = new PointD(zoom_d.X * (double)xs, zoom_d.Y * (double)ys);
		zoom_d = Zoom_d;
		Pan_d = new PointD((double)at.X - pointD.X * zoom_d.X, (double)at.Y - pointD.Y * zoom_d.Y);
		Invalidate();
	}

	public void ResetTransform()
	{
		Zoom_d = new PointD(1.0, 1.0);
		PanToOrigin();
		Invalidate();
	}

	public Vec2F GraphToClient(Vec2F p)
	{
		return new Vec2F
		{
			X = (float)(m_trans.X + (double)p.X * m_scale.X),
			Y = (float)(m_trans.Y + (double)p.Y * m_scale.Y)
		};
	}

	public Vec2F GraphToClient(double x, double y)
	{
		return new Vec2F
		{
			X = (float)(m_trans.X + x * m_scale.X),
			Y = (float)(m_trans.Y + y * m_scale.Y)
		};
	}

	public Vec2F GraphToClient(float x, float y)
	{
		return new Vec2F
		{
			X = (float)(m_trans.X + (double)x * m_scale.X),
			Y = (float)(m_trans.Y + (double)y * m_scale.Y)
		};
	}

	public float GraphToClient(double x)
	{
		return (float)(m_trans.X + x * m_scale.X);
	}

	public float GraphToClient(float x)
	{
		return (float)(m_trans.X + (double)x * m_scale.X);
	}

	public Vec2F GraphToClientTangent(Vec2F tan)
	{
		return new Vec2F(tan.X * (float)m_scale.X, tan.Y * (float)m_scale.Y);
	}

	public Vec2F ClientToGraph(float px, float py)
	{
		return new Vec2F
		{
			X = (float)(((double)px - m_trans.X) / m_scale.X),
			Y = (float)(((double)py - m_trans.Y) / m_scale.Y)
		};
	}

	public PointD ClientToGraph_d(double px, double py)
	{
		double num = (px - m_trans.X) / m_scale.X;
		double num2 = (py - m_trans.Y) / m_scale.Y;
		return new PointD(num, num2);
	}

	public double ClientToGraph_d(double px)
	{
		return (px - m_trans.X) / m_scale.X;
	}

	public Vec2F ClientToGraph(Vec2F p)
	{
		return new Vec2F
		{
			X = (float)(((double)p.X - m_trans.X) / m_scale.X),
			Y = (float)(((double)p.Y - m_trans.Y) / m_scale.Y)
		};
	}

	public float ClientToGraph(float px)
	{
		return (float)(((double)px - m_trans.X) / m_scale.X);
	}

	public virtual Matrix GetTransform()
	{
		m_xform.Reset();
		m_xform.Translate((float)m_trans.X, (float)m_trans.Y);
		m_xform.Scale((float)m_scale.X, (float)m_scale.Y);
		return m_xform;
	}

	public virtual void DrawHorizontalScale(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		double num = ClientToGraph_d(0.0);
		double num2 = ClientToGraph_d(m_clientSize.X);
		double num3 = num - num % m_majorTickX;
		float num4 = (float)m_clientSize.Y;
		float num5 = num4 - 3f;
		float num6 = -base.Width;
		for (; num3 <= num2; num3 += m_majorTickX)
		{
			float num7 = GraphToClient(num3);
			string s = Math.Round(num3, 9).ToString();
			SizeF sizeF = g.MeasureString(s, m_scaleTextFont);
			float num8 = sizeF.Width * 0.5f;
			if (num6 < num7 - num8)
			{
				g.DrawString(s, m_scaleTextFont, m_scaleTextBrush, num7 - num8, num5 - sizeF.Height);
				num6 = num7 + num8;
			}
		}
	}

	public virtual void DrawVerticalScale(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		float num = g.MeasureString("+1289E", m_scaleTextFont).Height;
		double num2 = ClientToGraph_d(0.0, 0.0).Y;
		double num3 = ClientToGraph_d(0.0, m_clientSize.Y).Y;
		if (m_flipY)
		{
			double num4 = num2;
			num2 = num3;
			num3 = num4;
		}
		for (double num5 = num3 - num3 % m_majorTickY; num5 <= num2; num5 += m_majorTickY)
		{
			float num6 = (float)(m_trans.Y + num5 * m_scale.Y);
			string s = Math.Round(num5, 9).ToString();
			g.DrawString(s, m_scaleTextFont, m_scaleTextBrush, 3f, num6 - num / 2f);
		}
	}

	public virtual void DrawHorizontalMajorTicks(Graphics g)
	{
		DrawHorizontalTicks(g, m_majorGridlinePen, m_majorTickY);
	}

	public virtual void DrawVerticalMajorTicks(Graphics g)
	{
		DrawVerticalTicks(g, m_majorGridlinePen, m_majorTickX);
	}

	public virtual void DrawHorizontalMinorTicks(Graphics g)
	{
		DrawHorizontalTicks(g, m_minorGridlinePen, m_majorTickY / m_numOfMinorTicks);
	}

	public virtual void DrawVerticalMinorTicks(Graphics g)
	{
		DrawVerticalTicks(g, m_minorGridlinePen, m_majorTickX / m_numOfMinorTicks);
	}

	public virtual void DrawXYLabel(Graphics g, string xLabel, string yLabel, Color? color = null)
	{
		Color color2 = (color.HasValue ? color.Value : Color.Black);
		using SolidBrush brush = new SolidBrush(color2);
		float num = Math.Min(2f * (float)m_axisLabelFont.Height, 40f);
		float num2 = 120f;
		g.DrawString(xLabel, m_axisLabelFont, brush, num2, (float)base.ClientSize.Height - num);
		Matrix transform = g.Transform;
		g.RotateTransform(-90f);
		g.TranslateTransform(num, (float)base.ClientSize.Height - num2, MatrixOrder.Append);
		g.DrawString(yLabel, m_axisLabelFont, brush, 0f, 0f);
		g.Transform = transform;
	}

	public virtual void DrawCoordinateAxes(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		Vec2F vec2F = GraphToClient(0f, 0f);
		float num = base.ClientSize.Width;
		float num2 = base.ClientSize.Height;
		if (vec2F.Y > 0f && vec2F.Y < num2)
		{
			g.DrawLine(m_axisPen, 0f, vec2F.Y, num, vec2F.Y);
		}
		if (vec2F.X > 0f && vec2F.X < num)
		{
			g.DrawLine(m_axisPen, vec2F.X, 0f, vec2F.X, num2);
		}
	}

	public virtual void DrawCartesianGrid(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		g.SmoothingMode = SmoothingMode.None;
		g.CompositingMode = CompositingMode.SourceCopy;
		DrawHorizontalMinorTicks(g);
		DrawVerticalMinorTicks(g);
		DrawVerticalMajorTicks(g);
		DrawHorizontalMajorTicks(g);
		DrawCoordinateAxes(g);
		g.CompositingMode = CompositingMode.SourceOver;
		DrawVerticalScale(g);
		DrawHorizontalScale(g);
	}

	protected RectangleF MakeRect(PointF p1, PointF p2)
	{
		PointF pointF = new PointF
		{
			X = Math.Min(p1.X, p2.X),
			Y = Math.Min(p1.Y, p2.Y)
		};
		PointF pointF2 = new PointF
		{
			X = Math.Max(p1.X, p2.X),
			Y = Math.Max(p1.Y, p2.Y)
		};
		return new RectangleF(pointF.X, pointF.Y, pointF2.X - pointF.X, pointF2.Y - pointF.Y);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		CurrentPoint = new Vec2F(-1f, -1f);
		PreviousPoint = CurrentPoint;
		CurrentGraphPoint = ClientToGraph(CurrentPoint);
		PreviousGraphPoint = CurrentGraphPoint;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		Focus();
		base.Capture = true;
		ClickPoint = new Vec2F(e.X, e.Y);
		ClickGraphPoint = ClientToGraph(ClickPoint);
		PreviousPoint = ClickPoint;
		PreviousGraphPoint = ClickGraphPoint;
		CurrentPoint = ClickPoint;
		CurrentGraphPoint = ClickGraphPoint;
		ClickPan_d = Pan_d;
		ClickZoom_d = Zoom_d;
		m_startDrag = true;
		SelectionRect.Location = ClickPoint;
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		PreviousPoint = CurrentPoint;
		PreviousGraphPoint = CurrentGraphPoint;
		CurrentPoint = new Vec2F(e.X, e.Y);
		CurrentGraphPoint = ClientToGraph(CurrentPoint);
		float value = CurrentPoint.X - ClickPoint.X;
		float value2 = CurrentPoint.Y - ClickPoint.Y;
		if (m_startDrag && !m_dragOverThreshold && (Math.Abs(value) > m_dragThreshold || Math.Abs(value2) > m_dragThreshold))
		{
			m_dragOverThreshold = true;
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		SelectionRect = RectangleF.Empty;
		m_startDrag = false;
		m_dragOverThreshold = false;
		base.OnMouseUp(e);
		base.Capture = false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		if (Control.MouseButtons == MouseButtons.None)
		{
			float num = 1f + (float)e.Delta / 1200f;
			SetZoom(e.Location, num, num);
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		m_clientSize.X = base.ClientSize.Width;
		m_clientSize.Y = base.ClientSize.Height;
		UpdatePan();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ScaleTextBrush = null;
			ScaleTextFont = null;
			MajorGridlinePen = null;
			MinorGridlinePen = null;
			AxisPen = null;
			AxisLabelFont = null;
		}
		base.Dispose(disposing);
	}

	private void SetDisposableVar<T>(ref T mvar, T value) where T : class, IDisposable
	{
		if (mvar != null)
		{
			mvar.Dispose();
		}
		mvar = value;
	}

	private void DrawVerticalTicks(Graphics g, Pen p, double tickLength)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		double num = ClientToGraph_d(0.0);
		double num2 = ClientToGraph_d(m_clientSize.X);
		double num3 = num - num % tickLength;
		float y = (float)m_clientSize.Y;
		for (; num3 <= num2; num3 += tickLength)
		{
			float num4 = (float)(m_trans.X + num3 * m_scale.X);
			g.DrawLine(p, num4, 0f, num4, y);
		}
	}

	private void DrawHorizontalTicks(Graphics g, Pen p, double tickLength)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		double num = ClientToGraph_d(0.0, 0.0).Y;
		double num2 = ClientToGraph_d(0.0, m_clientSize.Y).Y;
		if (m_flipY)
		{
			double num3 = num;
			num = num2;
			num2 = num3;
		}
		double num4 = num2 - num2 % tickLength;
		float x = (float)m_clientSize.X;
		for (; num4 <= num; num4 += tickLength)
		{
			float num5 = (float)(m_trans.Y + num4 * m_scale.Y);
			g.DrawLine(p, 0f, num5, x, num5);
		}
	}

	private void UpdatePan()
	{
		double num = base.ClientSize.Width;
		double num2 = base.ClientSize.Height;
		switch (m_lockorg)
		{
		case OriginLockMode.Free:
			m_minOffsetX = -3.4028234663852886E+38;
			m_maxOffsetX = 3.4028234663852886E+38;
			m_minOffsetY = -3.4028234663852886E+38;
			m_maxOffsetY = 3.4028234663852886E+38;
			break;
		case OriginLockMode.Center:
			m_minOffsetX = num / 2.0;
			m_maxOffsetX = num / 2.0;
			m_minOffsetY = num2 / 2.0;
			m_maxOffsetY = num2 / 2.0;
			break;
		case OriginLockMode.Left:
			m_minOffsetX = -3.4028234663852886E+38;
			m_maxOffsetX = 1.0;
			m_minOffsetY = -3.4028234663852886E+38;
			m_maxOffsetY = 3.4028234663852886E+38;
			break;
		case OriginLockMode.LeftTop:
			m_minOffsetX = -3.4028234663852886E+38;
			m_maxOffsetX = 1.0;
			m_minOffsetY = -3.4028234663852886E+38;
			m_maxOffsetY = 1.0;
			break;
		case OriginLockMode.LeftMiddle:
			m_minOffsetX = -3.4028234663852886E+38;
			m_maxOffsetX = 1.0;
			m_minOffsetY = num2 / 2.0;
			m_maxOffsetY = num2 / 2.0;
			break;
		case OriginLockMode.LeftBottom:
			m_minOffsetX = -3.4028234663852886E+38;
			m_maxOffsetX = 1.0;
			m_minOffsetY = num2 - 1.0;
			m_maxOffsetY = 3.4028234663852886E+38;
			break;
		}
		if (m_lockorg != OriginLockMode.Free)
		{
			Pan_d = m_trans;
		}
	}

	private static double ComputeGridSpan(double attendedSpan)
	{
		double num = Math.Truncate(Math.Log10(attendedSpan));
		double num2 = Math.Pow(10.0, num);
		double result = 0.0;
		double num3 = double.MaxValue;
		for (int i = 0; i < s_spanTable.Length; i++)
		{
			double num4 = num2 * s_spanTable[i];
			double num5 = Math.Abs(num4 - attendedSpan);
			if (num5 < num3)
			{
				num3 = num5;
				result = num4;
			}
		}
		return result;
	}
}
