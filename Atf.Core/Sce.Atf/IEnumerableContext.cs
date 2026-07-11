using System.Collections.Generic;

namespace Sce.Atf;

public interface IEnumerableContext
{
	IEnumerable<object> Items { get; }
}
