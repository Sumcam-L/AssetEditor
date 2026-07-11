using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Behaviors;

public class TreeViewItemLabelEditBehavior : Behavior<TreeViewItem>
{
	private bool m_mouseDown;

	private DispatcherTimer m_timer;

	private static uint s_doubleClickTime = GetDoubleClickTime();

	private static readonly string s_isInLabelEditModePropertyName = TypeUtil.GetProperty((Node x) => x.IsInLabelEditMode).Name;

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
		base.AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
		base.AssociatedObject.LostFocus += AssociatedObject_LostFocus;
		base.AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
		if (base.AssociatedObject.DataContext is Node node)
		{
			node.PropertyChanged += Node_PropertyChanged;
		}
	}

	private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!(e.PropertyName == s_isInLabelEditModePropertyName))
		{
			return;
		}
		Node node = sender as Node;
		if (node.IsInLabelEditMode)
		{
			return;
		}
		TreeView treeView = base.AssociatedObject.FindAncestor<TreeView>();
		if (treeView != null && treeView.SelectedItem == node)
		{
			base.AssociatedObject.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)delegate
			{
				base.AssociatedObject.Focus();
			});
		}
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
		base.AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
		base.AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
		base.AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;
		if (base.AssociatedObject.DataContext is Node node)
		{
			node.PropertyChanged -= Node_PropertyChanged;
		}
	}

	private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		StopTimer();
	}

	private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
	{
		StopTimer();
	}

	private void AssociatedObject_PreviewMouseDown(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource.GetType() == typeof(TextBlock))
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed && base.AssociatedObject.IsFocused)
			{
				m_mouseDown = true;
			}
			else
			{
				StopTimer();
			}
		}
	}

	private void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!m_mouseDown)
		{
			return;
		}
		m_mouseDown = false;
		if (base.AssociatedObject.IsMouseOver && base.AssociatedObject.IsFocused)
		{
			if (m_timer == null)
			{
				m_timer = new DispatcherTimer();
				m_timer.Interval = TimeSpan.FromMilliseconds(s_doubleClickTime + 100);
				m_timer.Tick += Tick;
				m_timer.Start();
			}
		}
		else
		{
			StopTimer();
		}
	}

	private void Tick(object sender, EventArgs e)
	{
		StopTimer();
		if (base.AssociatedObject.IsFocused && base.AssociatedObject.IsMouseOver && base.AssociatedObject.DataContext is Node node)
		{
			node.IsInLabelEditMode = true;
		}
	}

	private void StopTimer()
	{
		if (m_timer != null)
		{
			m_timer.Tick -= Tick;
			m_timer.Stop();
			m_timer = null;
		}
	}

	[DllImport("user32.dll")]
	private static extern uint GetDoubleClickTime();
}
