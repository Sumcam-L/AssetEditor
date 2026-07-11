using System.Collections;
using System.Collections.Generic;
using Firaxis.AssetBrowser.ViewModels;

namespace Firaxis.AssetBrowser.Commands;

public static class CommandHelpers
{
	public static IEnumerable<InstanceEntityViewModel> GetViewModels(object parameter)
	{
		if (parameter == null)
		{
			yield break;
		}
		if (parameter is InstanceEntityViewModel selectedVM)
		{
			yield return selectedVM;
		}
		else
		{
			if (!(parameter is IEnumerable selectedVMs))
			{
				yield break;
			}
			foreach (object item in selectedVMs)
			{
				if (item is InstanceEntityViewModel vm)
				{
					yield return vm;
				}
			}
		}
	}
}
