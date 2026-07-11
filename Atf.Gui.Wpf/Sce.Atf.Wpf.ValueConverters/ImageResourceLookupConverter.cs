using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ImageResourceLookupConverter : ConverterMarkupExtension<ImageResourceLookupConverter>
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
		ComponentResourceKey componentResourceKey = new ComponentResourceKey(typeof(ImageResourceLookupConverter), value);
		object obj = Application.Current.TryFindResource(componentResourceKey);
		if (obj == null && Application.Current.TryFindResource(value) is ImageSource source)
		{
			Image image = new Image();
			image.Source = source;
			image.Style = Application.Current.FindResource(Resources.MenuItemImageStyleKey) as Style;
			Application.Current.Resources.Add(componentResourceKey, image);
			obj = image;
		}
		return obj;
	}
}
