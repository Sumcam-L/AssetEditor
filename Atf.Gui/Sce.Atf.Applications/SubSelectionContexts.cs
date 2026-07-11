using System;

namespace Sce.Atf.Applications;

public static class SubSelectionContexts
{
	public static void Set(this ISubSelectionContext selectionContext, object item)
	{
		if (item != null)
		{
			selectionContext.SubSelectionContext.SetRange(new object[1] { item });
		}
	}

	public static void Clear(this ISubSelectionContext selectionContext)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		selectionContext.SubSelectionContext.Clear();
	}
}
