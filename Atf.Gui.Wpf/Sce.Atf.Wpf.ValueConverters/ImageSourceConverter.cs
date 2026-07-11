using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ImageSourceConverter : ConverterMarkupExtension<ImageSourceConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		string text = value as string;
		if (!string.IsNullOrEmpty(text))
		{
			value = new Uri(text, UriKind.RelativeOrAbsolute);
		}
		if (value is Uri)
		{
			return BitmapFrame.Create(value as Uri);
		}
		return null;
	}
}
