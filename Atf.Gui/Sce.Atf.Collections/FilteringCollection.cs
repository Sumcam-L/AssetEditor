using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Collections;

public class FilteringCollection<T, M> : AdaptingCollection<T, M>
{
	public FilteringCollection(Func<Lazy<T, M>, bool> filter)
		: base((Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>>)((IEnumerable<Lazy<T, M>> e) => e.Where(filter)))
	{
	}
}
