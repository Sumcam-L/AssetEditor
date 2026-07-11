using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup;

public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter where T : class, IValueConverter, new()
{
	private static T s_converter = null;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return s_converter ?? (s_converter = new T());
	}

	public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
