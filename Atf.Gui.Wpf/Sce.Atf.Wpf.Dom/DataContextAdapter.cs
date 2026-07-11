using System.Windows;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Dom;

public static class DataContextAdapter<Telement, Tdata> where Telement : FrameworkElement where Tdata : class
{
	static DataContextAdapter()
	{
		FrameworkElement.DataContextProperty.OverrideMetadata(typeof(Telement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, null, CoerceDataContextValue));
	}

	public static void Register()
	{
	}

	private static object CoerceDataContextValue(DependencyObject d, object baseValue)
	{
		if (!(baseValue is Tdata) && baseValue is IAdaptable adaptable)
		{
			Tdata val = adaptable.As<Tdata>();
			if (val != null)
			{
				return val;
			}
		}
		return baseValue;
	}
}
