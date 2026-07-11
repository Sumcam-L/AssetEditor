using System.Collections.Generic;

namespace Sce.Atf;

public interface IQueryableContext
{
	IEnumerable<object> Query(IQueryPredicate predicate);
}
