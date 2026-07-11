using System.Collections.Generic;
using System.Diagnostics;

namespace IronPython.Runtime;

internal class ObjectCollectionDebugProxy
{
	private readonly ICollection<object> _collection;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	internal IList<object> Members => new List<object>(_collection);

	public ObjectCollectionDebugProxy(ICollection<object> collection)
	{
		_collection = collection;
	}
}
