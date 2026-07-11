using System;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.ValueConverters;

public class InterfaceTemplateSelector : DataTemplateSelector
{
	private static InterfaceTemplateSelector s_instance;

	public static InterfaceTemplateSelector Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new InterfaceTemplateSelector();
			}
			return s_instance;
		}
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (container is FrameworkElement frameworkElement)
		{
			Type[] array = item.GetType().FindInterfaces(TypeFilter, null);
			foreach (Type resourceKey in array)
			{
				if (frameworkElement.TryFindResource(resourceKey) is DataTemplate result)
				{
					return result;
				}
			}
		}
		return base.SelectTemplate(item, container);
	}

	private bool TypeFilter(Type m, object filterCriteria)
	{
		return true;
	}
}
