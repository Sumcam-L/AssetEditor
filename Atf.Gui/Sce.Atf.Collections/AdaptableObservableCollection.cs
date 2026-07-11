using System;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Collections;

public class AdaptableObservableCollection<T, U> : ObservableCollectionAdapter<T, U> where T : class where U : class
{
	public AdaptableObservableCollection(IObservableCollection<T> collection)
		: base(collection)
	{
	}

	protected override T Convert(U item)
	{
		T val = item.As<T>();
		if (val == null && item != null)
		{
			throw new InvalidOperationException("Item of wrong type for underlying collection");
		}
		return val;
	}

	protected override U Convert(T item)
	{
		return item.As<U>();
	}
}
