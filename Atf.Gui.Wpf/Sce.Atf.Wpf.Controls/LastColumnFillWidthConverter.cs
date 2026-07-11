using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.Controls;

public class LastColumnFillWidthConverter : MultiConverterMarkupExtension<LastColumnFillWidthConverter>
{
	public override object Convert(object[] values, Type type, object parameter, CultureInfo culture)
	{
		if (ValuesPopulated(values))
		{
			ListView listView = values[0] as ListView;
			GridView gridView = listView.View as GridView;
			double num = 0.0;
			for (int i = 0; i < gridView.Columns.Count - 1; i++)
			{
				num += gridView.Columns[i].Width;
			}
			return (double)values[1] - num;
		}
		return 0.0;
	}

	private static bool ValuesPopulated(object[] values)
	{
		foreach (object obj in values)
		{
			if (obj == null || obj.Equals(DependencyProperty.UnsetValue))
			{
				return false;
			}
		}
		return true;
	}
}
