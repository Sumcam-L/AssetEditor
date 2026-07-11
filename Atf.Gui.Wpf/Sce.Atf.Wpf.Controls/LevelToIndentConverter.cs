using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.Controls;

public class LevelToIndentConverter : ConverterMarkupExtension<LevelToIndentConverter>
{
	public override object Convert(object o, Type type, object parameter, CultureInfo culture)
	{
		double num = 20.0;
		if (parameter != null)
		{
			num = double.Parse((string)parameter);
		}
		return (double)(int)o * num;
	}
}
