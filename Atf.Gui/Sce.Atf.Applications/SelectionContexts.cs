using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Applications;

public static class SelectionContexts
{
	public static void Clear(this ISelectionContext selectionContext)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		selectionContext.Selection = EmptyEnumerable<object>.Instance;
	}

	public static void Set(this ISelectionContext selectionContext, object item)
	{
		selectionContext.SetRange(new object[1] { item });
	}

	public static void SetRange(this ISelectionContext selectionContext, IEnumerable<object> items)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		selectionContext.Selection = items;
	}

	public static void SetRange(this ISelectionContext selectionContext, IEnumerable items)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		selectionContext.Selection = items.Cast<object>();
	}

	public static void Add(this ISelectionContext selectionContext, object item)
	{
		selectionContext.AddRange((IEnumerable)new object[1] { item });
	}

	public static void AddRange(this ISelectionContext selectionContext, IEnumerable<object> items)
	{
		selectionContext.AddRange((IEnumerable)items);
	}

	public static void AddRange(this ISelectionContext selectionContext, IEnumerable items)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		List<object> list = new List<object>();
		HashSet<object> hashSet = new HashSet<object>(items.Cast<object>());
		foreach (object item in selectionContext.Selection)
		{
			if (!hashSet.Contains(item))
			{
				list.Add(item);
			}
		}
		list.AddRange(items.Cast<object>());
		selectionContext.Selection = list;
	}

	public static void Remove(this ISelectionContext selectionContext, object item)
	{
		selectionContext.RemoveRange((IEnumerable)new object[1] { item });
	}

	public static void RemoveRange(this ISelectionContext selectionContext, IEnumerable<object> items)
	{
		selectionContext.RemoveRange((IEnumerable)items);
	}

	public static void RemoveRange(this ISelectionContext selectionContext, IEnumerable items)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		HashSet<object> hashSet = new HashSet<object>();
		foreach (object item in items)
		{
			hashSet.Add(item);
		}
		List<object> list = new List<object>();
		foreach (object item2 in selectionContext.Selection)
		{
			if (!hashSet.Contains(item2))
			{
				list.Add(item2);
			}
		}
		selectionContext.Selection = list;
	}

	public static void Toggle(this ISelectionContext selectionContext, object item)
	{
		selectionContext.ToggleRange((IEnumerable)new object[1] { item });
	}

	public static void ToggleRange(this ISelectionContext selectionContext, IEnumerable<object> items)
	{
		selectionContext.ToggleRange((IEnumerable)items);
	}

	public static void ToggleRange(this ISelectionContext selectionContext, IEnumerable items)
	{
		if (selectionContext == null)
		{
			throw new ArgumentNullException("selectionContext");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		HashSet<object> hashSet = new HashSet<object>();
		foreach (object item in items)
		{
			hashSet.Add(item);
		}
		List<object> list = new List<object>();
		foreach (object item2 in selectionContext.Selection)
		{
			if (!hashSet.Contains(item2))
			{
				list.Add(item2);
			}
		}
		foreach (object item3 in items)
		{
			if (!selectionContext.SelectionContains(item3))
			{
				list.Add(item3);
			}
		}
		selectionContext.Selection = list;
	}
}
