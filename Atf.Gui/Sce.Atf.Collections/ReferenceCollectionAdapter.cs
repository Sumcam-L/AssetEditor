using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Collections;

public class ReferenceCollectionAdapter<TRefTarget, U> : ObservableCollectionAdapter<IReference<TRefTarget>, U> where TRefTarget : class where U : class
{
	private readonly Dictionary<U, IReference<TRefTarget>> m_targetToRefMap;

	private readonly IObservableCollection<IReference<TRefTarget>> m_collection;

	private readonly Func<U, IReference<TRefTarget>> m_createReference;

	public ReferenceCollectionAdapter(IObservableCollection<IReference<TRefTarget>> collection, Func<U, IReference<TRefTarget>> createReference)
		: base(collection)
	{
		m_collection = collection;
		m_createReference = createReference;
		m_targetToRefMap = new Dictionary<U, IReference<TRefTarget>>();
		foreach (IReference<TRefTarget> item in m_collection)
		{
			m_targetToRefMap.Add(item.Target.As<U>(), item);
		}
		m_collection.CollectionChanged += CollectionCollectionChanged;
	}

	protected override U Convert(IReference<TRefTarget> item)
	{
		return item.Target.As<U>();
	}

	protected override IReference<TRefTarget> Convert(U item)
	{
		if (!m_targetToRefMap.TryGetValue(item, out var value) && m_createReference != null)
		{
			return m_createReference(item);
		}
		return value;
	}

	private void CollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
		{
			foreach (IReference<TRefTarget> oldItem in e.OldItems)
			{
				m_targetToRefMap.Remove(oldItem.Target.As<U>());
			}
		}
		if (e.NewItems == null)
		{
			return;
		}
		foreach (IReference<TRefTarget> newItem in e.NewItems)
		{
			m_targetToRefMap.Add(newItem.Target.As<U>(), newItem);
		}
	}
}
