using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

public static class SelectionPathProviders
{
	public static object Parent(this ISelectionPathProvider selectionPathProvider, object item)
	{
		if (selectionPathProvider != null)
		{
			AdaptablePath<object> selectionPath = selectionPathProvider.GetSelectionPath(item);
			if (selectionPath != null && selectionPath.Count > 1)
			{
				return selectionPath[selectionPath.Count - 2];
			}
		}
		return null;
	}

	public static IEnumerable<object> Ancestry(this ISelectionPathProvider selectionPathProvider, object item)
	{
		if (selectionPathProvider == null)
		{
			yield break;
		}
		AdaptablePath<object> selectionPath = selectionPathProvider.GetSelectionPath(item);
		if (selectionPath != null && selectionPath.Count > 1)
		{
			int i = selectionPath.Count - 2;
			while (i >= 0)
			{
				yield return selectionPath[i];
				int num = i - 1;
				i = num;
			}
		}
	}
}
