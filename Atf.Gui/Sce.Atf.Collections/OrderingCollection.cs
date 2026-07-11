using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Collections;

public class OrderingCollection<T, M> : AdaptingCollection<T, M>
{
	public OrderingCollection(Func<Lazy<T, M>, object> keySelector, bool descending = false)
		: base((Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>>)((IEnumerable<Lazy<T, M>> e) => descending ? e.OrderByDescending(keySelector) : e.OrderBy(keySelector)))
	{
	}
}
