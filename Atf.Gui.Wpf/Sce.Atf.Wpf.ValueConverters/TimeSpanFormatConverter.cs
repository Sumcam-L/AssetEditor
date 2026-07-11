using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(TimeSpan), typeof(string))]
public class TimeSpanFormatConverter : ConverterMarkupExtension<TimeSpanFormatConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		TimeSpan timeSpan = (TimeSpan)value;
		return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
	}
}
