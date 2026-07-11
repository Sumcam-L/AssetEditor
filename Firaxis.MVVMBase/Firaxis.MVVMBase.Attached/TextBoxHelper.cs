using System.Windows;
using System.Windows.Controls;

namespace Firaxis.MVVMBase.Attached;

public class TextBoxHelper
{
	public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false, SelectAllOnFocusChanged));

	public static bool GetSelectAllOnFocus(TextBox target)
	{
		return (bool)target.GetValue(SelectAllOnFocusProperty);
	}

	public static void SetSelectAllOnFocus(TextBox target, bool value)
	{
		target.SetValue(SelectAllOnFocusProperty, value);
	}

	private static void SelectAllOnFocusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is TextBox textBox)
		{
			textBox.GotFocus -= TextBox_GotFocus;
			if (GetSelectAllOnFocus(textBox))
			{
				textBox.GotFocus += TextBox_GotFocus;
			}
		}
	}

	private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
	{
		if (sender is TextBox textBox)
		{
			textBox.SelectionStart = 0;
			textBox.SelectionLength = textBox.Text.Length;
		}
	}
}
