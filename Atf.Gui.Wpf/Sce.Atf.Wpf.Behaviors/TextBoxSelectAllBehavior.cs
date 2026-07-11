using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public static class TextBoxSelectAllBehavior
{
	public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxSelectAllBehavior), new UIPropertyMetadata(false, OnSelectAllOnFocusPropertyChanged));

	public static bool GetSelectAllOnFocus(DependencyObject obj)
	{
		return (bool)obj.GetValue(SelectAllOnFocusProperty);
	}

	public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
	{
		obj.SetValue(SelectAllOnFocusProperty, value);
	}

	private static void OnSelectAllOnFocusPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	{
		if (!(obj is TextBox textBox))
		{
			throw new Exception("Invalid type for SelectAllOnFocus property");
		}
		if ((bool)e.NewValue)
		{
			textBox.PreviewMouseLeftButtonDown += tb_PreviewMouseLeftButtonDown;
			textBox.GotKeyboardFocus += tb_GotKeyboardFocus;
		}
		else
		{
			textBox.PreviewMouseLeftButtonDown -= tb_PreviewMouseLeftButtonDown;
			textBox.GotKeyboardFocus -= tb_GotKeyboardFocus;
		}
	}

	private static void tb_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		TextBox textBox = sender as TextBox;
		textBox.SelectAll();
	}

	private static void tb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		TextBox textBox = sender as TextBox;
		if (!textBox.IsFocused)
		{
			textBox.SelectAll();
			Keyboard.Focus(textBox);
			e.Handled = true;
		}
	}
}
