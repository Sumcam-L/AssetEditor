using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.ValueConverters;

public class MenuItemStyleSelector : StyleSelector
{
	public override Style SelectStyle(object item, DependencyObject container)
	{
		object resourceKey = Resources.SubMenuItemStyleKey;
		if (item is ICommandItem)
		{
			resourceKey = Resources.CommandMenuItemStyleKey;
		}
		else if (item is Sce.Atf.Wpf.Models.Separator)
		{
			resourceKey = Resources.MenuSeparatorStyleKey;
		}
		return Application.Current.FindResource(resourceKey) as Style;
	}
}
