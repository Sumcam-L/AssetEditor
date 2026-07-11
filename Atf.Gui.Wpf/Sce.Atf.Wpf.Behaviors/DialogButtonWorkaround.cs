using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public static class DialogButtonWorkaround
{
	public static readonly DependencyProperty IsDefaultProperty = DependencyProperty.RegisterAttached("IsDefault", typeof(bool), typeof(DialogButtonWorkaround), new PropertyMetadata(false, OnIsDefaultPropertyChanged));

	public static void SetIsDefault(UIElement element, bool value)
	{
		element.SetValue(IsDefaultProperty, value);
	}

	public static bool GetIsDefault(UIElement element)
	{
		return (bool)element.GetValue(IsDefaultProperty);
	}

	private static void OnIsDefaultPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!(d is Button button))
		{
			throw new InvalidOperationException("DialogButtonWorkaround.IsDefault attached property only allowed on Buttons");
		}
		if ((bool)e.NewValue)
		{
			button.Click += button_Click;
			button.IsDefault = true;
		}
		else
		{
			button.Click -= button_Click;
			button.IsDefault = false;
		}
	}

	private static void button_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Button { IsDefault: not false })
		{
			FrameworkElement frameworkElement = Keyboard.FocusedElement as FrameworkElement;
			if (frameworkElement is TextBox)
			{
				frameworkElement.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
			}
		}
	}
}
