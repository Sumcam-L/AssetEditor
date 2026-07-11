using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public abstract class DragSourceBehavior<T> : AdaptableBehavior<T> where T : FrameworkElement
{
	private Point? m_dragStartPoint = null;

	protected abstract void BeginDrag(MouseEventArgs e);

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
		base.AssociatedObject.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
		base.AssociatedObject.MouseMove += OnMouseMove;
		DragDrop.AddPreviewQueryContinueDragHandler(base.AssociatedObject, OnQueryContinueDrag);
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
		base.AssociatedObject.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
		base.AssociatedObject.MouseMove -= OnMouseMove;
		DragDrop.RemovePreviewQueryContinueDragHandler(base.AssociatedObject, OnQueryContinueDrag);
	}

	private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		DependencyObject dep = e.OriginalSource as DependencyObject;
		if (dep.FindAncestor<RangeBase>() == null)
		{
			m_dragStartPoint = e.GetPosition(base.AssociatedObject);
		}
	}

	private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		m_dragStartPoint = null;
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (m_dragStartPoint.HasValue)
		{
			Point position = e.GetPosition(base.AssociatedObject);
			if (IsDragStarted(position))
			{
				BeginDrag(e);
				m_dragStartPoint = null;
				e.Handled = true;
			}
		}
	}

	private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		if (e.EscapePressed)
		{
			e.Action = DragAction.Cancel;
			m_dragStartPoint = null;
			e.Handled = true;
		}
	}

	private bool IsDragStarted(Point pos)
	{
		bool flag = Math.Abs(pos.X - m_dragStartPoint.Value.X) > SystemParameters.MinimumHorizontalDragDistance;
		bool flag2 = Math.Abs(pos.Y - m_dragStartPoint.Value.Y) > SystemParameters.MinimumVerticalDragDistance;
		return flag || flag2;
	}
}
