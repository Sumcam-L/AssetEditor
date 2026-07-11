using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Controls;

public class ToolBarItemTemplateSelector : DataTemplateSelector
{
	private static ToolBarItemTemplateSelector s_instance;

	public static ToolBarItemTemplateSelector Instance => s_instance ?? (s_instance = new ToolBarItemTemplateSelector());

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (container is FrameworkElement frameworkElement)
		{
			if (frameworkElement.TryFindResource(frameworkElement.GetType()) is DataTemplate result)
			{
				return result;
			}
			if (item is ICommandItem)
			{
				return frameworkElement.FindResource(Resources.ToolBarItemTemplateKey) as DataTemplate;
			}
		}
		return null;
	}
}
