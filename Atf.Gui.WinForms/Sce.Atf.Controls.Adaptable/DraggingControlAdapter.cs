using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public abstract class DraggingControlAdapter : ControlAdapter
{
	private Point m_firstPoint;

	private Point m_currentPoint;

	private bool m_mouseDown;

	private bool m_isDragging;

	public Point FirstPoint => m_firstPoint;

	public Point CurrentPoint => m_currentPoint;

	public Point Delta => new Point(m_currentPoint.X - m_firstPoint.X, m_currentPoint.Y - m_firstPoint.Y);

	public bool IsDragging => m_isDragging;

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseDown += MouseDownHandler;
		control.MouseMove += MouseMoveHandler;
		control.MouseUp += MouseUpHandler;
		control.MouseClick += OnMouseClick;
		control.MouseDoubleClick += OnMouseDoubleClick;
		control.MouseLeave += MouseLeaveHandler;
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.MouseDown -= MouseDownHandler;
		control.MouseMove -= MouseMoveHandler;
		control.MouseUp -= MouseUpHandler;
		control.MouseClick -= OnMouseClick;
		control.MouseDoubleClick -= OnMouseDoubleClick;
		control.MouseLeave -= MouseLeaveHandler;
	}

	protected virtual void OnMouseDown(object sender, MouseEventArgs e)
	{
	}

	protected virtual void OnMouseMove(object sender, MouseEventArgs e)
	{
	}

	protected virtual void OnBeginDrag(MouseEventArgs e)
	{
	}

	protected virtual void OnDragging(MouseEventArgs e)
	{
	}

	protected virtual void OnEndDrag(MouseEventArgs e)
	{
	}

	protected virtual void OnMouseUp(object sender, MouseEventArgs e)
	{
	}

	protected virtual void OnMouseClick(object sender, MouseEventArgs e)
	{
	}

	protected virtual void OnMouseDoubleClick(object sender, MouseEventArgs e)
	{
	}

	protected virtual void OnMouseLeave(object sender, EventArgs e)
	{
	}

	private void MouseDownHandler(object sender, MouseEventArgs e)
	{
		m_firstPoint = (m_currentPoint = new Point(e.X, e.Y));
		m_mouseDown = true;
		OnMouseDown(sender, e);
	}

	private void MouseMoveHandler(object sender, MouseEventArgs e)
	{
		m_currentPoint = new Point(e.X, e.Y);
		if (m_mouseDown && !m_isDragging)
		{
			Size dragSize = SystemInformation.DragSize;
			if (Math.Abs(m_firstPoint.X - m_currentPoint.X) >= dragSize.Width || Math.Abs(m_firstPoint.Y - m_currentPoint.Y) >= dragSize.Height)
			{
				OnBeginDrag(e);
				m_isDragging = true;
			}
		}
		if (m_isDragging)
		{
			OnDragging(e);
		}
		OnMouseMove(sender, e);
	}

	private void MouseUpHandler(object sender, MouseEventArgs e)
	{
		m_mouseDown = false;
		if (m_isDragging)
		{
			OnEndDrag(e);
		}
		m_isDragging = false;
		OnMouseUp(sender, e);
	}

	private void MouseLeaveHandler(object sender, EventArgs e)
	{
		m_mouseDown = false;
		m_isDragging = false;
		OnMouseLeave(sender, e);
	}
}
