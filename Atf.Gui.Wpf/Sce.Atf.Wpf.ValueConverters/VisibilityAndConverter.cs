using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class VisibilityAndConverter : MultiConverterMarkupExtension<VisibilityAndConverter>
{
	public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		return values.Any((object obj2) => !(obj2 is Visibility) || (Visibility)obj2 != Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
	}
}
