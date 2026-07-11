using System.Collections.Generic;

namespace Sce.Atf;

public interface IQueryableReplaceContext
{
	IEnumerable<object> Replace(object replaceInfo);
}
