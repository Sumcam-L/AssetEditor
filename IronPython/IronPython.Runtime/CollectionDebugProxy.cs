using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace IronPython.Runtime;

internal class CollectionDebugProxy
{
	private readonly ICollection _collection;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	internal IList Members
	{
		get
		{
			List<object> list = new List<object>(_collection.Count);
			foreach (object item in _collection)
			{
				list.Add(item);
			}
			return list;
		}
	}

	public CollectionDebugProxy(ICollection collection)
	{
		_collection = collection;
	}
}
