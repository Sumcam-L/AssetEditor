using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyComparerCollection<T> : IComparer<T>
{
	private readonly ListSortDescriptionCollection _sorts;

	private readonly PropertyComparer<T>[] _comparers;

	public ListSortDescriptionCollection Sorts => _sorts;

	public PropertyDescriptor PrimaryProperty => (_comparers.Length == 0) ? null : _comparers[0].Property;

	public ListSortDirection PrimaryDirection => (_comparers.Length != 0) ? (_comparers[0].Descending ? ListSortDirection.Descending : ListSortDirection.Ascending) : ListSortDirection.Ascending;

	public PropertyComparerCollection(ListSortDescriptionCollection sorts)
	{
		if (sorts == null)
		{
			throw new ArgumentNullException("sorts");
		}
		_sorts = sorts;
		List<PropertyComparer<T>> list = new List<PropertyComparer<T>>();
		foreach (ListSortDescription item in (IEnumerable)_sorts)
		{
			list.Add(new PropertyComparer<T>(item.PropertyDescriptor, item.SortDirection == ListSortDirection.Descending));
			_comparers = list.ToArray();
		}
	}

	int IComparer<T>.Compare(T x, T y)
	{
		int num = 0;
		for (int i = 0; i < _comparers.Length; i++)
		{
			num = _comparers[i].Compare(x, y);
			if (num != 0)
			{
				break;
			}
		}
		return num;
	}
}
