using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace Sce.Atf.Wpf.Behaviors;

public class TreeViewDragHoverExpandBehavior : Behavior<TreeView>
{
	private TreeViewItem m_lastHoveredItem;

	private DispatcherTimer m_timer;

	public int ExpandDelay
	{
		get
		{
			return m_timer.Interval.Milliseconds;
		}
		set
		{
			m_timer.Interval = TimeSpan.FromMilliseconds(value);
		}
	}

	public TreeViewDragHoverExpandBehavior()
	{
		m_timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1000.0), DispatcherPriority.Normal, TimerElapsed, Dispatcher.CurrentDispatcher);
		m_timer.Stop();
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.PreviewDragOver += AssociatedObject_PreviewDragOver;
		base.AssociatedObject.PreviewDragLeave += AssociatedObject_PreviewDragLeave;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.PreviewDragOver -= AssociatedObject_PreviewDragOver;
		base.AssociatedObject.PreviewDragLeave -= AssociatedObject_PreviewDragLeave;
	}

	private void AssociatedObject_PreviewDragOver(object sender, DragEventArgs e)
	{
		Point position = e.GetPosition(base.AssociatedObject);
		TreeViewItem itemContainerAtPoint = base.AssociatedObject.GetItemContainerAtPoint(position);
		if (itemContainerAtPoint != m_lastHoveredItem)
		{
			m_timer.Stop();
			m_lastHoveredItem = itemContainerAtPoint;
			if (m_lastHoveredItem != null && !m_lastHoveredItem.IsExpanded)
			{
				m_timer.Start();
			}
		}
	}

	private void AssociatedObject_PreviewDragLeave(object sender, DragEventArgs e)
	{
		m_lastHoveredItem = null;
	}

	private void TimerElapsed(object sender, EventArgs e)
	{
		m_timer.Stop();
		if (m_lastHoveredItem != null)
		{
			m_lastHoveredItem.IsExpanded = true;
			m_lastHoveredItem = null;
		}
	}
}
