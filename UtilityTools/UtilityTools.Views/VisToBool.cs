using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UtilityTools.Views;

public class VisToBool : IValueConverter
{
	private bool m_inverted = false;

	private bool m_not = false;

	public bool Inverted
	{
		get
		{
			return m_inverted;
		}
		set
		{
			m_inverted = value;
		}
	}

	public bool Not
	{
		get
		{
			return m_not;
		}
		set
		{
			m_not = value;
		}
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Inverted ? BoolToVisibility(value) : VisibilityToBool(value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Inverted ? VisibilityToBool(value) : BoolToVisibility(value);
	}

	private object BoolToVisibility(object value)
	{
		if (!(value is bool))
		{
			return DependencyProperty.UnsetValue;
		}
		return (!((bool)value ^ Not)) ? Visibility.Collapsed : Visibility.Visible;
	}

	private object VisibilityToBool(object value)
	{
		if (!(value is Visibility))
		{
			return DependencyProperty.UnsetValue;
		}
		return ((Visibility)value == Visibility.Visible) ^ Not;
	}
}
