using System.Windows;

namespace Sce.Atf.Wpf.Behaviors;

public static class GridViewColumnHeaderBehavior
{
	public static DependencyProperty IsResizableProperty = DependencyProperty.RegisterAttached("IsResizable", typeof(bool), typeof(GridViewColumnHeaderBehavior), new PropertyMetadata(true, OnIsResizableChanged));

	public static DependencyProperty IsClickableProperty = DependencyProperty.RegisterAttached("IsClickable", typeof(bool), typeof(GridViewColumnHeaderBehavior), new PropertyMetadata(true));

	public static void SetIsResizable(DependencyObject element, bool value)
	{
		element.SetValue(IsResizableProperty, value);
	}

	public static bool GetIsResizable(DependencyObject element)
	{
		return (bool)element.GetValue(IsResizableProperty);
	}

	private static void OnIsResizableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
	}

	public static void SetIsClickable(DependencyObject element, bool value)
	{
		element.SetValue(IsClickableProperty, value);
	}

	public static bool GetIsClickable(DependencyObject element)
	{
		return (bool)element.GetValue(IsClickableProperty);
	}
}
