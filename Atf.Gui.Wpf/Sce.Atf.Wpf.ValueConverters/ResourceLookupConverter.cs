using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ResourceLookupConverter : ConverterMarkupExtension<ResourceLookupConverter>
{
	public FrameworkElement Source { get; set; }

	public ResourceLookupConverter()
	{
	}

	public ResourceLookupConverter(FrameworkElement element)
	{
		Source = element;
	}

	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (value == null) ? null : ((Source != null) ? Source.TryFindResource(value) : Application.Current.TryFindResource(value));
	}
}
