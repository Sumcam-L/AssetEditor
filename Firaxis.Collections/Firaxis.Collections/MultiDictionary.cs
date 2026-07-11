using System;
using System.Collections.Generic;

namespace Firaxis.Collections;

public class MultiDictionary<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
{
	public virtual void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		HashSet<TValue> value2 = null;
		if (!TryGetValue(key, out value2))
		{
			value2 = new HashSet<TValue>();
			Add(key, value2);
		}
		value2.Add(value);
	}

	public virtual void AddRange(TKey key, HashSet<TValue> newValues)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		HashSet<TValue> value = null;
		if (!TryGetValue(key, out value))
		{
			Add(key, newValues);
			return;
		}
		foreach (TValue newValue in newValues)
		{
			Add(key, newValue);
		}
	}

	public new void Clear()
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Clear();
			}
		}
		base.Clear();
	}

	public virtual bool Contains(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		HashSet<TValue> value2 = null;
		if (TryGetValue(key, out value2))
		{
			return value2.Contains(value);
		}
		return false;
	}

	public virtual bool ContainsValue(TValue value)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.Contains(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void Remove(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		HashSet<TValue> value2 = null;
		if (TryGetValue(key, out value2))
		{
			value2.Remove(value);
			if (value2.Count <= 0)
			{
				Remove(key);
			}
		}
	}

	public virtual void Remove(TValue value)
	{
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Remove(value);
		}
	}

	public virtual IEnumerable<TValue> GetValues()
	{
		List<TValue> list = new List<TValue>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				list.AddRange(enumerator.Current.Value);
			}
		}
		return list;
	}

	public override string ToString()
	{
		string text = "";
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, HashSet<TValue>> current = enumerator.Current;
				text = text + "Key: " + current.Key.ToString() + " Values: ";
				foreach (TValue item in current.Value)
				{
					text = text + item.ToString() + ", ";
				}
				text += "\n";
			}
		}
		return text;
	}
}
