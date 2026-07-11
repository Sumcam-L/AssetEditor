using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications;

public static class MenuExtensions
{
	public static IEnumerable<IMenu> Lineage(this IMenu menu)
	{
		while (menu != null)
		{
			yield return menu;
			menu = menu.Parent;
		}
	}
}
