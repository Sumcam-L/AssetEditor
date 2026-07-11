using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public static class ContextMenuCommandProvider
{
	public static IEnumerable<object> GetCommands(this IEnumerable<IContextMenuCommandProvider> providers, object context, object target)
	{
		foreach (IContextMenuCommandProvider provider in providers)
		{
			foreach (object command in provider.GetCommands(context, target))
			{
				yield return command;
			}
		}
	}

	public static IEnumerable<object> GetCommands(this IEnumerable<Lazy<IContextMenuCommandProvider>> providers, object context, object target)
	{
		return providers.GetValues().GetCommands(context, target);
	}
}
