using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation;

public class AdaptableList<T, U> : ListAdapter<T, U> where T : class where U : class
{
	public AdaptableList(IList<T> list)
		: base(list)
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
