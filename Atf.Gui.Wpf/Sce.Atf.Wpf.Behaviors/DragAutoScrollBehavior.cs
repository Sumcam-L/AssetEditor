using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class DragAutoScrollBehavior : Behavior<FrameworkElement>
{
	private ScrollViewer m_scrollViewer;

	public int Tolerance { get; set; }

	public int Offset { get; set; }

	public bool CanAutoScrollX { get; set; }

	public bool CanAutoScrollY { get; set; }

	public DragAutoScrollBehavior()
	{
		Tolerance = 10;
		Offset = 10;
		CanAutoScrollY = true;
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.Loaded += AssociatedObject_Loaded;
		DragDrop.AddPreviewQueryContinueDragHandler(base.AssociatedObject, OnQueryContinueDrag);
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.Loaded -= AssociatedObject_Loaded;
		DragDrop.RemovePreviewQueryContinueDragHandler(base.AssociatedObject, OnQueryContinueDrag);
	}

	private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
	{
		m_scrollViewer = base.AssociatedObject.GetFrameworkElementByType<ScrollViewer>();
	}

	private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		if (m_scrollViewer == null)
		{
			return;
		}
		Point point = MouseUtilities.CorrectGetPosition(base.AssociatedObject);
		if (CanAutoScrollY)
		{
			if (point.Y < (double)Tolerance)
			{
				m_scrollViewer.ScrollToVerticalOffset(m_scrollViewer.VerticalOffset - (double)Offset);
			}
			else if (point.Y > base.AssociatedObject.ActualHeight - (double)Tolerance)
			{
				m_scrollViewer.ScrollToVerticalOffset(m_scrollViewer.VerticalOffset + (double)Offset);
			}
		}
		if (CanAutoScrollX)
		{
			if (point.X < (double)Tolerance)
			{
				m_scrollViewer.ScrollToHorizontalOffset(m_scrollViewer.HorizontalOffset - (double)Offset);
			}
			else if (point.X > base.AssociatedObject.ActualWidth - (double)Tolerance)
			{
				m_scrollViewer.ScrollToHorizontalOffset(m_scrollViewer.HorizontalOffset + (double)Offset);
			}
		}
	}
}
