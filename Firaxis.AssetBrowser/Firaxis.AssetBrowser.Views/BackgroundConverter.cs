using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Firaxis.AssetBrowser.Views;

public sealed class BackgroundConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is ListViewItem container))
		{
			return SystemColors.ControlBrush;
		}
		ListView listView = (ListView)ItemsControl.ItemsControlFromItemContainer(container);
		if (listView == null)
		{
			return SystemColors.ControlBrush;
		}
		int num = listView.ItemContainerGenerator.IndexFromContainer(container);
		if ((num & 1) == 0)
		{
			return listView.FindResource(SystemColors.ControlBrushKey);
		}
		return listView.FindResource(SystemColors.ControlLightBrushKey);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
