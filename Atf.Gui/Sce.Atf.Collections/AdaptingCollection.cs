using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sce.Atf.Collections;

public class AdaptingCollection<T> : AdaptingCollection<T, IDictionary<string, object>>
{
	public AdaptingCollection(Func<IEnumerable<Lazy<T, IDictionary<string, object>>>, IEnumerable<Lazy<T, IDictionary<string, object>>>> adaptor)
		: base(adaptor)
	{
	}
}
public class AdaptingCollection<T, M> : ICollection<Lazy<T, M>>, IEnumerable<Lazy<T, M>>, IEnumerable, INotifyCollectionChanged
{
	private readonly List<Lazy<T, M>> m_allItems = new List<Lazy<T, M>>();

	private readonly Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> m_adaptor = null;

	private List<Lazy<T, M>> m_adaptedItems = null;

	public int Count => AdaptedItems.Count;

	public bool IsReadOnly => false;

	private List<Lazy<T, M>> AdaptedItems => m_adaptedItems ?? (m_adaptedItems = Adapt(m_allItems).ToList());

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public AdaptingCollection()
		: this((Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>>)null)
	{
	}

	public AdaptingCollection(Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> adaptor)
	{
		m_adaptor = adaptor;
	}

	public void ReapplyAdaptor()
	{
		if (m_adaptedItems != null)
		{
			m_adaptedItems = null;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}

	public bool Contains(Lazy<T, M> item)
	{
		return AdaptedItems.Contains(item);
	}

	public void CopyTo(Lazy<T, M>[] array, int arrayIndex)
	{
		AdaptedItems.CopyTo(array, arrayIndex);
	}

	public IEnumerator<Lazy<T, M>> GetEnumerator()
	{
		return AdaptedItems.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(Lazy<T, M> item)
	{
		m_allItems.Add(item);
		ReapplyAdaptor();
	}

	public void Clear()
	{
		m_allItems.Clear();
		ReapplyAdaptor();
	}

	public bool Remove(Lazy<T, M> item)
	{
		bool result = m_allItems.Remove(item);
		ReapplyAdaptor();
		return result;
	}

	protected virtual IEnumerable<Lazy<T, M>> Adapt(IEnumerable<Lazy<T, M>> collection)
	{
		if (m_adaptor != null)
		{
			return m_adaptor(collection);
		}
		return collection;
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		this.CollectionChanged?.Invoke(this, e);
	}
}
