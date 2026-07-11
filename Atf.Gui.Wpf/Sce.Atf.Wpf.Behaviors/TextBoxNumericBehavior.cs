using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public static class TextBoxNumericBehavior
{
	public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.RegisterAttached("MinimumValue", typeof(double), typeof(TextBoxNumericBehavior), new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback));

	public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.RegisterAttached("MaximumValue", typeof(double), typeof(TextBoxNumericBehavior), new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback));

	public static readonly DependencyProperty MaskProperty = DependencyProperty.RegisterAttached("Mask", typeof(MaskType), typeof(TextBoxNumericBehavior), new FrameworkPropertyMetadata(MaskChangedCallback));

	public static double GetMinimumValue(DependencyObject obj)
	{
		return (double)obj.GetValue(MinimumValueProperty);
	}

	public static void SetMinimumValue(DependencyObject obj, double value)
	{
		obj.SetValue(MinimumValueProperty, value);
	}

	public static double GetMaximumValue(DependencyObject obj)
	{
		return (double)obj.GetValue(MaximumValueProperty);
	}

	public static void SetMaximumValue(DependencyObject obj, double value)
	{
		obj.SetValue(MaximumValueProperty, value);
	}

	public static MaskType GetMask(DependencyObject obj)
	{
		return (MaskType)obj.GetValue(MaskProperty);
	}

	public static void SetMask(DependencyObject obj, MaskType value)
	{
		obj.SetValue(MaskProperty, value);
	}

	private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBox tb = d as TextBox;
		ValidateTextBox(tb);
	}

	private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBox tb = d as TextBox;
		ValidateTextBox(tb);
	}

	private static void MaskChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextBox textBox = d as TextBox;
		if (e.NewValue == null || (MaskType)e.NewValue == MaskType.Any)
		{
			textBox.PreviewTextInput -= TextBox_PreviewTextInput;
			DataObject.RemovePastingHandler(textBox, TextBoxPastingEventHandler);
		}
		else
		{
			textBox.PreviewTextInput += TextBox_PreviewTextInput;
			DataObject.AddPastingHandler(textBox, TextBoxPastingEventHandler);
		}
		ValidateTextBox(textBox);
	}

	private static void ValidateTextBox(TextBox tb)
	{
		MaskType mask = GetMask(tb);
		if (mask != MaskType.Any)
		{
			tb.Text = ValidateValue(mask, tb.Text, GetMinimumValue(tb), GetMaximumValue(tb));
		}
	}

	private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
	{
		TextBox textBox = sender as TextBox;
		string value = e.DataObject.GetData(typeof(string)) as string;
		value = ValidateValue(GetMask(textBox), value, GetMinimumValue(textBox), GetMaximumValue(textBox));
		if (!string.IsNullOrEmpty(value))
		{
			textBox.Text = value;
		}
		e.CancelCommand();
		e.Handled = true;
	}

	private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		TextBox textBox = sender as TextBox;
		bool flag = IsSymbolValid(GetMask(textBox), e.Text);
		e.Handled = !flag;
		if (!flag)
		{
			return;
		}
		int num = textBox.CaretIndex;
		string text = textBox.Text;
		bool flag2 = false;
		int selectionLength = 0;
		if (textBox.SelectionLength > 0)
		{
			text = text.Substring(0, textBox.SelectionStart) + text.Substring(textBox.SelectionStart + textBox.SelectionLength);
			num = textBox.SelectionStart;
		}
		if (e.Text == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
		{
			while (true)
			{
				int num2 = text.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
				if (num2 == -1)
				{
					break;
				}
				text = text.Substring(0, num2) + text.Substring(num2 + 1);
				if (num > num2)
				{
					num--;
				}
			}
			switch (num)
			{
			case 0:
				text = "0" + text;
				num++;
				break;
			case 1:
				if (string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign)
				{
					text = NumberFormatInfo.CurrentInfo.NegativeSign + "0" + text.Substring(1);
					num++;
				}
				break;
			}
			if (num == text.Length)
			{
				selectionLength = 1;
				flag2 = true;
				text = text + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "0";
				num++;
			}
		}
		else if (e.Text == NumberFormatInfo.CurrentInfo.NegativeSign)
		{
			flag2 = true;
			if (textBox.Text.Contains(NumberFormatInfo.CurrentInfo.NegativeSign))
			{
				text = text.Replace(NumberFormatInfo.CurrentInfo.NegativeSign, string.Empty);
				if (num != 0)
				{
					num--;
				}
			}
			else
			{
				text = NumberFormatInfo.CurrentInfo.NegativeSign + textBox.Text;
				num++;
			}
		}
		if (!flag2)
		{
			text = text.Substring(0, num) + e.Text + ((num < textBox.Text.Length) ? text.Substring(num) : string.Empty);
			num++;
		}
		try
		{
			double num3 = Convert.ToDouble(text);
			double num4 = ValidateLimits(GetMinimumValue(textBox), GetMaximumValue(textBox), num3);
			if (num3 != num4)
			{
				text = num4.ToString();
			}
			else if (num3 == 0.0 && !text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
			{
				text = "0";
			}
		}
		catch
		{
			text = "0";
		}
		while (text.Length > 1 && text[0] == '0' && string.Empty + text[1] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
		{
			text = text.Substring(1);
			if (num > 0)
			{
				num--;
			}
		}
		while (text.Length > 2 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign && text[1] == '0' && string.Empty + text[2] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
		{
			text = NumberFormatInfo.CurrentInfo.NegativeSign + text.Substring(2);
			if (num > 1)
			{
				num--;
			}
		}
		if (num > text.Length)
		{
			num = text.Length;
		}
		textBox.Text = text;
		textBox.CaretIndex = num;
		textBox.SelectionStart = num;
		textBox.SelectionLength = selectionLength;
		e.Handled = true;
	}

	private static string ValidateValue(MaskType mask, string value, double min, double max)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		value = value.Trim();
		switch (mask)
		{
		case MaskType.Integer:
			try
			{
				Convert.ToInt64(value);
				return value;
			}
			catch
			{
			}
			return string.Empty;
		case MaskType.Decimal:
			try
			{
				Convert.ToDouble(value);
				return value;
			}
			catch
			{
			}
			return string.Empty;
		default:
			return value;
		}
	}

	private static double ValidateLimits(double min, double max, double value)
	{
		if (!min.Equals(double.NaN) && value < min)
		{
			return min;
		}
		if (!max.Equals(double.NaN) && value > max)
		{
			return max;
		}
		return value;
	}

	private static bool IsSymbolValid(MaskType mask, string str)
	{
		switch (mask)
		{
		case MaskType.Any:
			return true;
		case MaskType.Integer:
			if (str == NumberFormatInfo.CurrentInfo.NegativeSign)
			{
				return true;
			}
			break;
		case MaskType.Decimal:
			if (str == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator || str == NumberFormatInfo.CurrentInfo.NegativeSign)
			{
				return true;
			}
			break;
		}
		if (mask.Equals(MaskType.Integer) || mask.Equals(MaskType.Decimal))
		{
			foreach (char c in str)
			{
				if (!char.IsDigit(c))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}
}
