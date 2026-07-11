using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Firaxis.MVVMBase.Attached;

public class ListBoxHelper
{
	public static readonly DependencyProperty SelectionChangedActionProperty = DependencyProperty.RegisterAttached("SelectionChangedAction", typeof(Action<IList, IList, IList>), typeof(ListBoxHelper), new PropertyMetadata(null, SelectionChangedActionChanged));

	public static readonly DependencyProperty UnselectOnClickEmptyProperty = DependencyProperty.RegisterAttached("UnselectOnClickEmpty", typeof(bool), typeof(ListBoxHelper), new PropertyMetadata(false, UnselectOnClickEmptyChanged));

	public static readonly DependencyProperty PassFocusToSelectedIndexProperty = DependencyProperty.RegisterAttached("PassFocusToSelectedIndex", typeof(bool), typeof(ListBoxHelper), new PropertyMetadata(false, PassFocusToSelectedIndexChanged));

	public static Action<IList, IList, IList> GetSelectionChangedAction(ListBox target)
	{
		return (Action<IList, IList, IList>)target.GetValue(SelectionChangedActionProperty);
	}

	public static void SetSelectionChangedAction(ListBox target, Action<IList, IList, IList> value)
	{
		target.SetValue(SelectionChangedActionProperty, value);
	}

	private static void SelectionChangedActionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is ListBox listBox)
		{
			listBox.SelectionChanged -= ListBox_SelectionChanged;
			if (GetSelectionChangedAction(listBox) != null)
			{
				listBox.SelectionChanged += ListBox_SelectionChanged;
			}
		}
	}

	private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is ListBox listBox)
		{
			GetSelectionChangedAction(listBox)?.Invoke(listBox.SelectedItems, e.AddedItems, e.RemovedItems);
		}
	}

	public static bool GetUnselectOnClickEmpty(ListBox target)
	{
		return (bool)target.GetValue(UnselectOnClickEmptyProperty);
	}

	public static void SetUnselectOnClickEmpty(ListBox target, bool value)
	{
		target.SetValue(UnselectOnClickEmptyProperty, value);
	}

	private static void UnselectOnClickEmptyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is ListBox listBox)
		{
			listBox.MouseDown -= ListBox_MouseDown;
			if (GetUnselectOnClickEmpty(listBox))
			{
				listBox.MouseDown += ListBox_MouseDown;
			}
		}
	}

	private static void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (sender is ListBox listBox)
		{
			HitTestResult hitTestResult = VisualTreeHelper.HitTest(listBox, e.GetPosition(listBox));
			if (hitTestResult.VisualHit.GetType() != typeof(ListBoxItem))
			{
				listBox.UnselectAll();
			}
		}
	}

	public static bool GetPassFocusToSelectedIndex(ListBox target)
	{
		return (bool)target.GetValue(PassFocusToSelectedIndexProperty);
	}

	public static void SetPassFocusToSelectedIndex(ListBox target, bool value)
	{
		target.SetValue(PassFocusToSelectedIndexProperty, value);
	}

	private static void PassFocusToSelectedIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is ListBox listBox)
		{
			listBox.GotFocus -= ListBox_GotFocus;
			if (GetPassFocusToSelectedIndex(listBox))
			{
				listBox.GotFocus += ListBox_GotFocus;
			}
		}
	}

	private static void ListBox_GotFocus(object sender, RoutedEventArgs e)
	{
		if (sender is ListBox listBox && listBox == e.OriginalSource && listBox.Items.Count != 0 && listBox.ItemContainerGenerator.ContainerFromIndex(Math.Max(listBox.SelectedIndex, 0)) is ListViewItem element)
		{
			Keyboard.Focus(element);
		}
	}
}
