using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public abstract class CanvasControl : Control
{
	private const float FrameScale = 0.86f;

	private float m_xZoom = 1f;

	private float m_yZoom = 1f;

	private float m_minXZoom = 0.125f;

	private float m_minYZoom = 0.125f;

	private float m_maxXZoom = 200f;

	private float m_maxYZoom = 200f;

	private Size m_canvasSize = new Size(1, 1);

	private Point m_scroll;

	private readonly VScrollBar m_vScrollBar;

	private readonly HScrollBar m_hScrollBar;

	private bool m_autoScroll;

	private Point m_autoScrollPositionStart;

	private readonly Timer m_autoScrollTimer;

	private Point m_firstPoint;

	private Point m_currentPoint;

	private Point m_last;

	private int m_dragThreshold = 3;

	private float m_xZoomStart;

	private float m_yZoomStart;

	private PointF m_zoomCenterStart;

	private bool m_isDragging;

	private bool m_isMultiSelecting;

	private bool m_isScrolling;

	private bool m_dragOverThreshold;

	private bool m_constrain;

	private bool m_constrainX;

	private bool m_isZooming;

	private bool m_uniformZoom = true;

	public int DragThreshold
	{
		get
		{
			return m_dragThreshold;
		}
		set
		{
			m_dragThreshold = value;
		}
	}

	public virtual Matrix Transform => GdiUtil.GetTransform(m_scroll, m_xZoom, m_yZoom);

	public bool UniformZoom => m_uniformZoom;

	public float Zoom
	{
		get
		{
			return m_xZoom;
		}
		set
		{
			ZoomAboutCenter(value, value);
		}
	}

	public float XZoom
	{
		get
		{
			return m_xZoom;
		}
		set
		{
			if (m_xZoom != value)
			{
				ZoomAboutCenter(value, m_yZoom);
			}
		}
	}

	public float YZoom
	{
		get
		{
			return m_yZoom;
		}
		set
		{
			if (m_yZoom != value)
			{
				ZoomAboutCenter(m_xZoom, value);
			}
		}
	}

	public bool DragOverThreshold => m_isDragging && m_dragOverThreshold;

	public Point FirstPoint => m_firstPoint;

	public Point CurrentPoint => m_currentPoint;

	public Point DragPoint => m_dragOverThreshold ? m_currentPoint : m_firstPoint;

	public virtual Rectangle VisibleClientRectangle
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (m_hScrollBar.Visible)
			{
				clientRectangle.Height -= m_hScrollBar.Height;
			}
			if (m_vScrollBar.Visible)
			{
				clientRectangle.Width -= m_vScrollBar.Width;
			}
			return clientRectangle;
		}
	}

	public bool Constrain
	{
		get
		{
			return m_constrain;
		}
		set
		{
			m_constrain = value;
		}
	}

	public Point DragDelta
	{
		get
		{
			Point dragPoint = DragPoint;
			return new Point(dragPoint.X - m_firstPoint.X, dragPoint.Y - m_firstPoint.Y);
		}
	}

	public Point ScrollPosition
	{
		get
		{
			return m_scroll;
		}
		set
		{
			int num = -Math.Max(m_hScrollBar.Minimum, Math.Min(m_hScrollBar.Maximum - m_hScrollBar.LargeChange, -value.X));
			int num2 = -Math.Max(m_vScrollBar.Minimum, Math.Min(m_vScrollBar.Maximum - m_vScrollBar.LargeChange, -value.Y));
			m_scroll = new Point(num, num2);
			m_hScrollBar.Value = -num;
			m_vScrollBar.Value = -num2;
			OnScroll();
			Invalidate();
		}
	}

	protected Rectangle SelectionRect => GdiUtil.MakeRectangle(m_currentPoint, m_firstPoint);

	public bool IsMultiSelecting
	{
		get
		{
			return m_isMultiSelecting;
		}
		set
		{
			if (value)
			{
				m_isScrolling = (m_isZooming = false);
				Cursor = Cursors.Cross;
			}
			m_isMultiSelecting = value;
		}
	}

	public bool IsScrolling
	{
		get
		{
			return m_isScrolling;
		}
		set
		{
			if (value)
			{
				m_isZooming = (m_isMultiSelecting = false);
				Cursor = Cursors.Hand;
			}
			m_isScrolling = value;
		}
	}

	public bool IsZooming
	{
		get
		{
			return m_isZooming;
		}
		set
		{
			if (value)
			{
				m_xZoomStart = m_xZoom;
				m_yZoomStart = m_yZoom;
				m_zoomCenterStart = new PointF((float)(-m_scroll.X + m_firstPoint.X) / m_xZoom, (float)(-m_scroll.Y + m_firstPoint.Y) / m_yZoom);
				m_isMultiSelecting = (m_isScrolling = false);
				Cursor = Cursors.SizeAll;
			}
			m_isZooming = value;
		}
	}

	public bool IsDragging => m_isMultiSelecting || m_isScrolling || m_isZooming;

	public bool AutoScroll
	{
		get
		{
			return m_autoScroll;
		}
		set
		{
			m_autoScroll = value;
		}
	}

	protected VScrollBar VerticalScrollBar => m_vScrollBar;

	protected HScrollBar HorizontalScrollBar => m_hScrollBar;

	public event EventHandler Scrolled;

	public event EventHandler Zoomed;

	public CanvasControl()
	{
		SuspendLayout();
		m_vScrollBar = new VScrollBar();
		m_vScrollBar.Dock = DockStyle.Right;
		m_vScrollBar.ValueChanged += vScrollBar_ValueChanged;
		m_hScrollBar = new HScrollBar();
		m_hScrollBar.Dock = DockStyle.Bottom;
		m_hScrollBar.ValueChanged += hScrollBar_ValueChanged;
		base.Controls.Add(m_vScrollBar);
		base.Controls.Add(m_hScrollBar);
		ResumeLayout();
		m_autoScrollTimer = new Timer();
		m_autoScrollTimer.Interval = 10;
		m_autoScrollTimer.Tick += autoScrollTimer_Tick;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			m_autoScrollTimer.Dispose();
		}
		base.Dispose(disposing);
	}

	public void SetZoomRange(float minXZoom, float maxXZoom, float minYZoom, float maxYZoom)
	{
		m_minXZoom = minXZoom;
		m_maxXZoom = maxXZoom;
		m_minYZoom = minYZoom;
		m_maxYZoom = maxYZoom;
		m_uniformZoom = false;
		SetZoom(m_xZoom, m_yZoom);
	}

	public void SetZoomRange(float minZoom, float maxZoom)
	{
		m_minXZoom = (m_minYZoom = minZoom);
		m_maxXZoom = (m_maxYZoom = maxZoom);
		m_uniformZoom = true;
		SetZoom(m_xZoom, m_yZoom);
	}

	public Point ClientToCanvas(Point x)
	{
		return GdiUtil.InverseTransform(Transform, x);
	}

	public PointF ClientToCanvas(PointF x)
	{
		return GdiUtil.InverseTransform(Transform, x);
	}

	public Rectangle ClientToCanvas(Rectangle x)
	{
		return GdiUtil.InverseTransform(Transform, x);
	}

	public RectangleF ClientToCanvas(RectangleF x)
	{
		return GdiUtil.InverseTransform(Transform, x);
	}

	public Point CanvasToClient(Point x)
	{
		return GdiUtil.Transform(Transform, x);
	}

	public PointF CanvasToClient(PointF x)
	{
		return GdiUtil.Transform(Transform, x);
	}

	public Rectangle CanvasToClient(Rectangle x)
	{
		return GdiUtil.Transform(Transform, x);
	}

	public RectangleF CanvasToClient(RectangleF x)
	{
		return GdiUtil.Transform(Transform, x);
	}

	public void Frame(RectangleF bounds)
	{
		Frame(bounds, fillClientWindow: true);
	}

	public void EnsureVisible(RectangleF bounds)
	{
		Frame(bounds, fillClientWindow: false);
	}

	private void Frame(RectangleF bounds, bool fillClientWindow)
	{
		RectangleF rectangleF = VisibleClientRectangle;
		if (fillClientWindow || !rectangleF.Contains(bounds))
		{
			Point point = new Point((int)(rectangleF.Left + rectangleF.Width / 2f), (int)(rectangleF.Top + rectangleF.Height / 2f));
			RectangleF rectangleF2 = ClientToCanvas(bounds);
			Point point2 = new Point((int)(rectangleF2.Left + rectangleF2.Width / 2f), (int)(rectangleF2.Top + rectangleF2.Height / 2f));
			float xZoom = XZoom;
			if (fillClientWindow || bounds.Width > rectangleF.Width)
			{
				xZoom = Math.Abs(rectangleF.Width / rectangleF2.Width) * 0.86f;
			}
			float yZoom = YZoom;
			if (fillClientWindow || bounds.Height > rectangleF.Height)
			{
				yZoom = Math.Abs(rectangleF.Height / rectangleF2.Height) * 0.86f;
			}
			SetZoom(xZoom, yZoom);
			UpdateScrollBars(m_vScrollBar, m_hScrollBar);
			ScrollPosition = new Point((int)((float)point.X - (float)point2.X * m_xZoom - rectangleF.Left), (int)((float)point.Y - (float)point2.Y * m_yZoom - rectangleF.Top));
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		m_firstPoint = (m_currentPoint = (m_last = new Point(e.X, e.Y)));
		m_autoScrollPositionStart = ScrollPosition;
		m_isDragging = true;
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		m_last = m_currentPoint;
		m_currentPoint = new Point(e.X, e.Y);
		int num = Math.Abs(m_currentPoint.X - m_firstPoint.X);
		int num2 = Math.Abs(m_currentPoint.Y - m_firstPoint.Y);
		if (m_isDragging && !m_dragOverThreshold && (num > m_dragThreshold || num2 > m_dragThreshold))
		{
			m_dragOverThreshold = true;
			if (m_constrain)
			{
				m_constrainX = num < num2;
			}
		}
		if (m_constrain)
		{
			if (m_constrainX)
			{
				m_currentPoint.X = m_firstPoint.X;
			}
			else
			{
				m_currentPoint.Y = m_firstPoint.Y;
			}
		}
		if (m_dragOverThreshold)
		{
			Point dragDelta = DragDelta;
			if (m_autoScroll && !VisibleClientRectangle.Contains(m_currentPoint) && !m_autoScrollTimer.Enabled)
			{
				m_autoScrollTimer.Start();
			}
			if (m_isMultiSelecting)
			{
				Rectangle rectangle = MakeSelectionRect(m_last, m_firstPoint);
				ControlPaint.DrawReversibleFrame(rectangle, BackColor, FrameStyle.Dashed);
				rectangle = MakeSelectionRect(m_currentPoint, m_firstPoint);
				ControlPaint.DrawReversibleFrame(rectangle, BackColor, FrameStyle.Dashed);
			}
			else if (m_isScrolling)
			{
				ScrollPosition = new Point(m_autoScrollPositionStart.X + dragDelta.X, m_autoScrollPositionStart.Y + dragDelta.Y);
			}
			else if (m_isZooming)
			{
				float num3 = 1f + (float)(2 * dragDelta.X) / (float)base.Width;
				float num4 = 1f + (float)(2 * dragDelta.Y) / (float)base.Height;
				if (m_constrain || UniformZoom)
				{
					num3 = (num4 = Math.Max(num3, num4));
				}
				SetZoom(m_xZoomStart * num3, m_yZoomStart * num4);
				ScrollPosition = new Point((int)((float)m_firstPoint.X - m_zoomCenterStart.X * m_xZoom), (int)((float)m_firstPoint.Y - m_zoomCenterStart.Y * m_yZoom));
			}
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		CancelAction();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Escape)
		{
			CancelAction();
		}
		base.OnKeyDown(e);
	}

	protected abstract Size GetCanvasSize();

	protected Point ForceOnCanvas(Rectangle offsetRect)
	{
		Point currentPoint = m_currentPoint;
		Rectangle rectangle = offsetRect;
		rectangle.Offset(new Point(m_currentPoint.X - m_scroll.X, m_currentPoint.Y - m_scroll.Y));
		if (rectangle.Left < 0)
		{
			currentPoint.X -= rectangle.Left;
		}
		if (rectangle.Top < 0)
		{
			currentPoint.Y -= rectangle.Top;
		}
		return currentPoint;
	}

	protected virtual void OnAutoScroll()
	{
	}

	protected virtual void UpdateScrollBars(VScrollBar vScrollBar, HScrollBar hScrollBar)
	{
		Size canvasSize = GetCanvasSize();
		RectangleF rectangleF = VisibleClientRectangle;
		Size visibleSize = new Size((int)rectangleF.Width, (int)rectangleF.Height);
		WinFormsUtil.UpdateScrollbars(vScrollBar, hScrollBar, visibleSize, canvasSize);
	}

	protected virtual void OnScroll()
	{
		this.Scrolled?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnZoom()
	{
		this.Zoomed?.Invoke(this, EventArgs.Empty);
	}

	private void autoScrollTimer_Tick(object sender, EventArgs e)
	{
		m_autoScrollTimer.Stop();
		if (!VisibleClientRectangle.Contains(m_currentPoint))
		{
			if (m_isMultiSelecting)
			{
				Rectangle rectangle = MakeSelectionRect(m_currentPoint, m_firstPoint);
				ControlPaint.DrawReversibleFrame(rectangle, BackColor, FrameStyle.Dashed);
			}
			Rectangle visibleClientRectangle = VisibleClientRectangle;
			Point scroll = m_scroll;
			if (m_currentPoint.X < 0)
			{
				scroll.X += 10;
			}
			else if (m_currentPoint.X > visibleClientRectangle.Width)
			{
				scroll.X -= 10;
			}
			if (m_currentPoint.Y < 0)
			{
				scroll.Y += 10;
			}
			else if (m_currentPoint.Y > visibleClientRectangle.Height)
			{
				scroll.Y -= 10;
			}
			Point scroll2 = m_scroll;
			ScrollPosition = new Point(scroll.X, scroll.Y);
			scroll2.X = m_scroll.X - scroll2.X;
			scroll2.Y = m_scroll.Y - scroll2.Y;
			m_firstPoint.X += scroll2.X;
			m_firstPoint.Y += scroll2.Y;
			m_last = m_currentPoint;
			OnAutoScroll();
			Update();
			if (m_isMultiSelecting)
			{
				Rectangle rectangle2 = MakeSelectionRect(m_currentPoint, m_firstPoint);
				ControlPaint.DrawReversibleFrame(rectangle2, BackColor, FrameStyle.Dashed);
			}
			m_autoScrollTimer.Start();
		}
	}

	private void SetZoom(float xZoom, float yZoom)
	{
		xZoom = Math.Max(m_minXZoom, xZoom);
		xZoom = Math.Min(m_maxXZoom, xZoom);
		yZoom = Math.Max(m_minYZoom, yZoom);
		yZoom = Math.Min(m_maxYZoom, yZoom);
		if (xZoom != m_xZoom || yZoom != m_yZoom)
		{
			if (UniformZoom)
			{
				m_xZoom = (m_yZoom = xZoom);
			}
			else
			{
				m_xZoom = xZoom;
				m_yZoom = yZoom;
			}
			OnZoom();
			Invalidate();
		}
	}

	private void ZoomAboutCenter(float xZoom, float yZoom)
	{
		PointF pointF = new PointF((float)(m_scroll.X - base.Width / 2) / (float)m_canvasSize.Width, (float)(m_scroll.Y - base.Height / 2) / (float)m_canvasSize.Height);
		SetZoom(xZoom, yZoom);
		Invalidate();
		ScrollPosition = new Point((int)((0f - pointF.X) * (float)m_canvasSize.Width - (float)(base.Width / 2)), (int)((0f - pointF.Y) * (float)m_canvasSize.Height - (float)(base.Height / 2)));
	}

	private Rectangle MakeSelectionRect(Point p1, Point p2)
	{
		Rectangle r = GdiUtil.MakeRectangle(p1, p2);
		r.Intersect(VisibleClientRectangle);
		return RectangleToScreen(r);
	}

	private void vScrollBar_ValueChanged(object sender, EventArgs e)
	{
		m_scroll.Y = -m_vScrollBar.Value;
		OnScroll();
		Invalidate();
	}

	private void hScrollBar_ValueChanged(object sender, EventArgs e)
	{
		m_scroll.X = -m_hScrollBar.Value;
		OnScroll();
		Invalidate();
	}

	private void CancelAction()
	{
		m_isDragging = false;
		m_isMultiSelecting = false;
		m_isScrolling = false;
		m_isZooming = false;
		m_constrain = false;
		m_dragOverThreshold = false;
		m_autoScrollTimer.Stop();
		Cursor = Cursors.Arrow;
	}
}
