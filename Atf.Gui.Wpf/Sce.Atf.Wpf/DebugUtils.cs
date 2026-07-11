using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Sce.Atf.Wpf;

public static class DebugUtils
{
	public static readonly ResourceKey DebugConverterKey = new ComponentResourceKey(typeof(DebugUtils), "DebugConverter");

	public static DependencyObject FindLogicalTreeRoot(DependencyObject initial)
	{
		DependencyObject dependencyObject = initial;
		DependencyObject result = initial;
		while (dependencyObject != null)
		{
			result = dependencyObject;
			dependencyObject = ((dependencyObject is Visual || dependencyObject is Visual3D) ? VisualTreeHelper.GetParent(dependencyObject) : LogicalTreeHelper.GetParent(dependencyObject));
		}
		return result;
	}

	public static object GetDataContext(object item)
	{
		if (item == null)
		{
			return null;
		}
		object obj = null;
		if (item is FrameworkElement)
		{
			FrameworkElement frameworkElement = item as FrameworkElement;
			obj = frameworkElement.DataContext;
			if (obj == null)
			{
				obj = GetDataContext(frameworkElement.Parent);
			}
			if (obj == null)
			{
				obj = GetDataContext(frameworkElement.TemplatedParent);
			}
		}
		if (obj == null && item is FrameworkContentElement)
		{
			FrameworkContentElement frameworkContentElement = item as FrameworkContentElement;
			obj = frameworkContentElement.DataContext;
			if (obj == null)
			{
				obj = GetDataContext(frameworkContentElement.Parent);
			}
			if (obj == null)
			{
				obj = GetDataContext(frameworkContentElement.TemplatedParent);
			}
		}
		return obj;
	}
}
