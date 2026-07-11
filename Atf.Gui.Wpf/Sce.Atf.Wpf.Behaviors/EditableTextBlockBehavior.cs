using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public static class EditableTextBlockBehavior
{
	public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.RegisterAttached("IsInEditMode", typeof(bool), typeof(EditableTextBlockBehavior), new UIPropertyMetadata(false, OnIsInEditModePropertyChanged));

	public static readonly DependencyProperty EditOnDoubleClickProperty = DependencyProperty.RegisterAttached("EditOnDoubleClick", typeof(bool), typeof(EditableTextBlockBehavior), new UIPropertyMetadata(false, OnEditOnDoubleClickPropertyChanged));

	public static bool GetIsInEditMode(DependencyObject obj)
	{
		return (bool)obj.GetValue(IsInEditModeProperty);
	}

	public static void SetIsInEditMode(DependencyObject obj, bool value)
	{
		obj.SetValue(IsInEditModeProperty, value);
	}

	public static bool GetEditOnDoubleClick(DependencyObject obj)
	{
		return (bool)obj.GetValue(EditOnDoubleClickProperty);
	}

	public static void SetEditOnDoubleClick(DependencyObject obj, bool value)
	{
		obj.SetValue(EditOnDoubleClickProperty, value);
	}

	private static void OnEditOnDoubleClickPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	{
		if (!(obj is TextBlock textBlock))
		{
			throw new ArgumentException("EditOnDoubleClick property can only be set on TextBlock");
		}
		if ((bool)e.NewValue)
		{
			textBlock.PreviewMouseLeftButtonUp += textBlock_MouseLeftButtonUp;
			textBlock.PreviewMouseLeftButtonDown += textBlock_PreviewMouseLeftButtonDown;
		}
		else
		{
			textBlock.PreviewMouseLeftButtonUp -= textBlock_MouseLeftButtonUp;
			textBlock.PreviewMouseLeftButtonDown -= textBlock_PreviewMouseLeftButtonDown;
		}
	}

	private static void textBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			(sender as TextBlock).SetValue(IsInEditModeProperty, true);
			e.Handled = true;
		}
	}

	private static void textBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			(sender as TextBlock).SetValue(IsInEditModeProperty, true);
		}
	}

	private static void OnIsInEditModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	{
		if (!(obj is TextBlock textBlock))
		{
			throw new ArgumentException("IsInEditMode property can only be set on TextBlock");
		}
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(textBlock);
		if (adornerLayer == null)
		{
			return;
		}
		if ((bool)e.NewValue)
		{
			adornerLayer.Add(new EditableTextBlockAdorner(textBlock));
			textBlock.CaptureMouse();
		}
		else if ((bool)e.OldValue)
		{
			textBlock.ReleaseMouseCapture();
			EditableTextBlockAdorner adorner = GetAdorner(textBlock);
			if (adorner != null)
			{
				adornerLayer.Remove(adorner);
				adorner.Dispose();
			}
			textBlock.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
		}
	}

	private static EditableTextBlockAdorner GetAdorner(TextBlock textBlock)
	{
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(textBlock);
		Adorner[] adorners = adornerLayer.GetAdorners(textBlock);
		if (adorners != null)
		{
			return (EditableTextBlockAdorner)adorners.FirstOrDefault((Adorner x) => x is EditableTextBlockAdorner);
		}
		return null;
	}
}
