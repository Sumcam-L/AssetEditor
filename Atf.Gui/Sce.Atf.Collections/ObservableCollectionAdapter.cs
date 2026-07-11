using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf.Collections;

public class ObservableCollectionAdapter<T, U> : ListAdapter<T, U>, IObservableCollection<U>, IList<U>, ICollection<U>, IEnumerable<U>, IEnumerable, INotifyPropertyChanged, INotifyCollectionChanged where T : class where U : class
{
	private readonly IObservableCollection<T> m_collection;

	public bool IsFixedSize => false;

	public new object this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			base[index] = value as U;
		}
	}

	public bool IsSynchronized => false;

	public object SyncRoot => null;

	public event PropertyChangedEventHandler PropertyChanged;

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public ObservableCollectionAdapter(IObservableCollection<T> collection)
		: base((IList<T>)collection)
	{
		m_collection = collection;
		m_collection.PropertyChanged += collection_PropertyChanged;
		m_collection.CollectionChanged += collection_CollectionChanged;
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
	}

	private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		NotifyCollectionChangedEventArgs e2 = ConvertArgs(e);
		this.CollectionChanged?.Invoke(this, e2);
	}

	private void collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	private NotifyCollectionChangedEventArgs ConvertArgs(NotifyCollectionChangedEventArgs e)
	{
		NotifyCollectionChangedEventArgs e2 = null;
		return e.Action switch
		{
			NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ConvertList(e.NewItems), e.NewStartingIndex), 
			NotifyCollectionChangedAction.Move => throw new NotSupportedException(), 
			NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, ConvertList(e.OldItems), e.OldStartingIndex), 
			NotifyCollectionChangedAction.Replace => throw new NotSupportedException(), 
			NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IList ConvertList(IList list)
	{
		List<object> list2 = new List<object>();
		foreach (object item in list)
		{
			list2.Add(Convert(item as T));
		}
		return list2;
	}

	public new IEnumerator GetEnumerator()
	{
		return base.GetEnumerator();
	}

	public int Add(object value)
	{
		base.Add(value as U);
		return base.IndexOf(value as U);
	}

	public bool Contains(object value)
	{
		return base.Contains(value as U);
	}

	public int IndexOf(object value)
	{
		return base.IndexOf(value as U);
	}

	public void Insert(int index, object value)
	{
		base.Insert(index, value as U);
	}

	public void Remove(object value)
	{
		base.Remove(value as U);
	}

	public void CopyTo(Array array, int index)
	{
		for (int i = index; i < base.Count; i++)
		{
			array.SetValue(this[i], i);
		}
	}
}
