using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing;

public class SortableBindingList<T> : BindingList<T>, IBindingListView, IBindingList, IList, ICollection, IEnumerable
{
	private PropertyComparerCollection<T> _sorts;

	private PropertyDescriptor _lastProperty;

	private ListSortDirection _lastDirection;

	protected override bool IsSortedCore => _sorts != null;

	protected override bool SupportsSortingCore => true;

	protected override ListSortDirection SortDirectionCore => (_sorts != null) ? _sorts.PrimaryDirection : ListSortDirection.Ascending;

	protected override PropertyDescriptor SortPropertyCore => (_sorts == null) ? null : _sorts.PrimaryProperty;

	string IBindingListView.Filter
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	ListSortDescriptionCollection IBindingListView.SortDescriptions => _sorts.Sorts;

	bool IBindingListView.SupportsAdvancedSorting => true;

	bool IBindingListView.SupportsFiltering => false;

	public void AddRange(IEnumerable<T> collection)
	{
		bool flag = base.RaiseListChangedEvents;
		base.RaiseListChangedEvents = false;
		foreach (T item in collection)
		{
			Add(item);
		}
		base.RaiseListChangedEvents = flag;
		ListChangedEventArgs e = new ListChangedEventArgs(ListChangedType.Reset, -1);
		OnListChanged(e);
	}

	protected override void RemoveSortCore()
	{
		_sorts = null;
	}

	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		if (prop.Equals(_lastProperty))
		{
			direction = ((_lastDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
			_lastDirection = direction;
		}
		else
		{
			_lastProperty = prop;
			_lastDirection = direction;
		}
		ListSortDescription[] sorts = new ListSortDescription[1]
		{
			new ListSortDescription(prop, direction)
		};
		ApplySort(new ListSortDescriptionCollection(sorts));
	}

	public void ApplySort(ListSortDescriptionCollection sortCollection)
	{
		bool flag = base.RaiseListChangedEvents;
		base.RaiseListChangedEvents = false;
		try
		{
			PropertyComparerCollection<T> propertyComparerCollection = new PropertyComparerCollection<T>(sortCollection);
			List<T> list = new List<T>(this);
			list.Sort(propertyComparerCollection);
			int num = 0;
			foreach (T item in list)
			{
				SetItem(num++, item);
			}
			_sorts = propertyComparerCollection;
		}
		finally
		{
			base.RaiseListChangedEvents = flag;
			ResetBindings();
		}
	}

	void IBindingListView.RemoveFilter()
	{
		throw new NotImplementedException();
	}
}
