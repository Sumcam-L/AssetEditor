using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpDX.Diagnostics;

public class CollectionDebugView
{
	private readonly ICollection collection;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public object[] Items
	{
		get
		{
			object[] array = new object[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}
	}

	public CollectionDebugView(ICollection collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		this.collection = collection;
	}
}
public class CollectionDebugView<T>
{
	private readonly ICollection<T> collection;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public T[] Items
	{
		get
		{
			T[] array = new T[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}
	}

	public CollectionDebugView(ICollection<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		this.collection = collection;
	}
}
