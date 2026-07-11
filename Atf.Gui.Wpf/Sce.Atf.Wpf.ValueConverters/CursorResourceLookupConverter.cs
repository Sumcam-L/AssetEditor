using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class CursorResourceLookupConverter : ConverterMarkupExtension<CursorResourceLookupConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Convert(value);
	}

	public static object Convert(object value)
	{
		if (value == null)
		{
			return null;
		}
		if (Application.Current.TryFindResource(value) is FreezableCursor freezableCursor)
		{
			return freezableCursor.Cursor;
		}
		return null;
	}
}
