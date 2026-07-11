using System;
using System.Diagnostics;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf;

public class DebugConverter : ConverterMarkupExtension<DebugConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		object dataContext = DebugUtils.GetDataContext(value);
		if (dataContext != null)
		{
		}
		return value;
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}

	[Conditional("DEBUG")]
	private static void TryBreaking()
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
	}
}
