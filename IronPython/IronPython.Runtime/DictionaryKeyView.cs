using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

[PythonType("dict_keys")]
public sealed class DictionaryKeyView : ICollection<object>, IEnumerable<object>, IEnumerable, ICodeFormattable
{
	internal readonly PythonDictionary _dict;

	public int Count
	{
		[PythonHidden]
		get
		{
			return _dict.Count;
		}
	}

	public bool IsReadOnly
	{
		[PythonHidden]
		get
		{
			return true;
		}
	}

	internal DictionaryKeyView(PythonDictionary dict)
	{
		_dict = dict;
	}

	[PythonHidden]
	public IEnumerator GetEnumerator()
	{
		return _dict.iterkeys();
	}

	IEnumerator<object> IEnumerable<object>.GetEnumerator()
	{
		return new DictionaryKeyEnumerator(_dict._storage);
	}

	void ICollection<object>.Add(object key)
	{
		throw new NotSupportedException("Collection is read-only");
	}

	void ICollection<object>.Clear()
	{
		throw new NotSupportedException("Collection is read-only");
	}

	[PythonHidden]
	public bool Contains(object key)
	{
		return _dict.__contains__(key);
	}

	[PythonHidden]
	public void CopyTo(object[] array, int arrayIndex)
	{
		int num = arrayIndex;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				array[num++] = current;
				if (num >= array.Length)
				{
					break;
				}
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	bool ICollection<object>.Remove(object item)
	{
		throw new NotSupportedException("Collection is read-only");
	}

	public static SetCollection operator |(DictionaryKeyView x, IEnumerable y)
	{
		return new SetCollection(SetStorage.Union(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator |(IEnumerable y, DictionaryKeyView x)
	{
		return new SetCollection(SetStorage.Union(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator &(DictionaryKeyView x, IEnumerable y)
	{
		return new SetCollection(SetStorage.Intersection(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator &(IEnumerable y, DictionaryKeyView x)
	{
		return new SetCollection(SetStorage.Intersection(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator ^(DictionaryKeyView x, IEnumerable y)
	{
		return new SetCollection(SetStorage.SymmetricDifference(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator ^(IEnumerable y, DictionaryKeyView x)
	{
		return new SetCollection(SetStorage.SymmetricDifference(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator -(DictionaryKeyView x, IEnumerable y)
	{
		return new SetCollection(SetStorage.Difference(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public static SetCollection operator -(IEnumerable y, DictionaryKeyView x)
	{
		return new SetCollection(SetStorage.Difference(SetStorage.GetItemsWorker(x.GetEnumerator()), SetStorage.GetItems(y)));
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is DictionaryKeyView)
		{
			return this == (DictionaryKeyView)obj;
		}
		if (obj is DictionaryItemView)
		{
			return this == (DictionaryItemView)obj;
		}
		if (obj is SetCollection)
		{
			return this == (SetCollection)obj;
		}
		if (obj is FrozenSetCollection)
		{
			return this == (FrozenSetCollection)obj;
		}
		return false;
	}

	public static bool operator ==(DictionaryKeyView x, DictionaryKeyView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return true;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		if (itemsWorker.Count == itemsWorker2.Count)
		{
			return itemsWorker.IsSubset(itemsWorker2);
		}
		return false;
	}

	public static bool operator !=(DictionaryKeyView x, DictionaryKeyView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return false;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		if (itemsWorker.Count == itemsWorker2.Count)
		{
			return !itemsWorker.IsSubset(itemsWorker2);
		}
		return true;
	}

	public static bool operator >(DictionaryKeyView x, DictionaryKeyView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return false;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker2.IsStrictSubset(itemsWorker);
	}

	public static bool operator <(DictionaryKeyView x, DictionaryKeyView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return false;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker.IsStrictSubset(itemsWorker2);
	}

	public static bool operator >=(DictionaryKeyView x, DictionaryKeyView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return true;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker2.IsSubset(itemsWorker);
	}

	public static bool operator <=(DictionaryKeyView x, DictionaryKeyView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return true;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker.IsSubset(itemsWorker2);
	}

	public static bool operator ==(DictionaryKeyView x, DictionaryItemView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return false;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		if (itemsWorker.Count == itemsWorker2.Count)
		{
			return itemsWorker.IsSubset(itemsWorker2);
		}
		return false;
	}

	public static bool operator !=(DictionaryKeyView x, DictionaryItemView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return true;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		if (itemsWorker.Count == itemsWorker2.Count)
		{
			return !itemsWorker.IsSubset(itemsWorker2);
		}
		return true;
	}

	public static bool operator >(DictionaryKeyView x, DictionaryItemView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return true;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker2.IsStrictSubset(itemsWorker);
	}

	public static bool operator <(DictionaryKeyView x, DictionaryItemView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return true;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker.IsStrictSubset(itemsWorker2);
	}

	public static bool operator >=(DictionaryKeyView x, DictionaryItemView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return false;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker2.IsSubset(itemsWorker);
	}

	public static bool operator <=(DictionaryKeyView x, DictionaryItemView y)
	{
		if (object.ReferenceEquals(x._dict, y._dict))
		{
			return false;
		}
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage itemsWorker2 = SetStorage.GetItemsWorker(y.GetEnumerator());
		return itemsWorker.IsSubset(itemsWorker2);
	}

	public static bool operator ==(DictionaryKeyView x, SetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		if (itemsWorker.Count == items.Count)
		{
			return itemsWorker.IsSubset(items);
		}
		return false;
	}

	public static bool operator !=(DictionaryKeyView x, SetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		if (itemsWorker.Count == items.Count)
		{
			return !itemsWorker.IsSubset(items);
		}
		return true;
	}

	public static bool operator >(DictionaryKeyView x, SetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return items.IsStrictSubset(itemsWorker);
	}

	public static bool operator <(DictionaryKeyView x, SetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return itemsWorker.IsStrictSubset(items);
	}

	public static bool operator >=(DictionaryKeyView x, SetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return items.IsSubset(itemsWorker);
	}

	public static bool operator <=(DictionaryKeyView x, SetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return itemsWorker.IsSubset(items);
	}

	public static bool operator ==(DictionaryKeyView x, FrozenSetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		if (itemsWorker.Count == items.Count)
		{
			return itemsWorker.IsSubset(items);
		}
		return false;
	}

	public static bool operator !=(DictionaryKeyView x, FrozenSetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		if (itemsWorker.Count == items.Count)
		{
			return !itemsWorker.IsSubset(items);
		}
		return true;
	}

	public static bool operator >(DictionaryKeyView x, FrozenSetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return items.IsStrictSubset(itemsWorker);
	}

	public static bool operator <(DictionaryKeyView x, FrozenSetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return itemsWorker.IsStrictSubset(items);
	}

	public static bool operator >=(DictionaryKeyView x, FrozenSetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return items.IsSubset(itemsWorker);
	}

	public static bool operator <=(DictionaryKeyView x, FrozenSetCollection y)
	{
		SetStorage itemsWorker = SetStorage.GetItemsWorker(x.GetEnumerator());
		SetStorage items = y._items;
		return itemsWorker.IsSubset(items);
	}

	public string __repr__(CodeContext context)
	{
		StringBuilder stringBuilder = new StringBuilder(20);
		stringBuilder.Append("dict_keys([");
		string value = "";
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				stringBuilder.Append(value);
				value = ", ";
				stringBuilder.Append(PythonOps.Repr(context, current));
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		stringBuilder.Append("])");
		return stringBuilder.ToString();
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
