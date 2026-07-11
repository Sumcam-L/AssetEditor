using System.Collections.Generic;

namespace Sce.Atf;

public interface IQueryPredicate
{
	bool Test(object item, out IList<IQueryMatch> matchList);

	void Replace(IList<IQueryMatch> matchList, object replaceValue);
}
