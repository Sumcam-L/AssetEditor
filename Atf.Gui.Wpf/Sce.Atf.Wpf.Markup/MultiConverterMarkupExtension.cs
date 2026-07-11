using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup;

public abstract class MultiConverterMarkupExtension<T> : MarkupExtension, IMultiValueConverter where T : class, IMultiValueConverter, new()
{
	private static T s_converter = null;

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return s_converter ?? (s_converter = new T());
	}

	public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
