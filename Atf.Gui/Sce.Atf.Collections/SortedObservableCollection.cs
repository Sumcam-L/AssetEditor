using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sce.Atf.Collections;

public class SortedObservableCollection<T> : ObservableCollection<T>
{
	private IComparer<T> m_sorter;

	public IComparer<T> Sorter
	{
		get
		{
			return m_sorter;
		}
		set
		{
			m_sorter = value;
			ApplySort();
		}
	}

	public SortedObservableCollection(IComparer<T> sorter)
	{
		Sorter = sorter;
	}

	protected override void InsertItem(int index, T item)
	{
		base.InsertItem(index, item);
		ApplySort();
	}

	protected override void SetItem(int index, T item)
	{
		base.SetItem(index, item);
		ApplySort();
	}

	private void ApplySort()
	{
		if (m_sorter == null || base.Count <= 1)
		{
			return;
		}
		List<T> list = new List<T>(this);
		list.Sort(m_sorter);
		foreach (T item in list)
		{
			Move(IndexOf(item), list.IndexOf(item));
		}
	}
}
