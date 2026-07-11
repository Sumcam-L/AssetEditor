using System.Collections;
using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

public static class DictionaryOps
{
	public static string __repr__(CodeContext context, IDictionary<object, object> self)
	{
		List<object> andCheckInfinite = PythonOps.GetAndCheckInfinite(self);
		if (andCheckInfinite == null)
		{
			return "{...}";
		}
		int count = andCheckInfinite.Count;
		andCheckInfinite.Add(self);
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			bool flag = true;
			foreach (KeyValuePair<object, object> item in self)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(", ");
				}
				if (CustomStringDictionary.IsNullObject(item.Key))
				{
					stringBuilder.Append("None");
				}
				else
				{
					stringBuilder.Append(PythonOps.Repr(context, item.Key));
				}
				stringBuilder.Append(": ");
				stringBuilder.Append(PythonOps.Repr(context, item.Value));
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		finally
		{
			andCheckInfinite.RemoveAt(count);
		}
	}

	public static object get(PythonDictionary self, object key)
	{
		return get(self, key, null);
	}

	public static object get(PythonDictionary self, object key, object defaultValue)
	{
		if (self.TryGetValueNoMissing(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public static bool has_key(IDictionary<object, object> self, object key)
	{
		return self.ContainsKey(key);
	}

	public static List items(IDictionary<object, object> self)
	{
		List list = PythonOps.MakeEmptyList(self.Count);
		foreach (KeyValuePair<object, object> item in self)
		{
			list.AddNoLock(PythonTuple.MakeTuple(item.Key, item.Value));
		}
		return list;
	}

	public static IEnumerator iteritems(IDictionary<object, object> self)
	{
		return ((IEnumerable)items(self)).GetEnumerator();
	}

	public static IEnumerator iterkeys(IDictionary<object, object> self)
	{
		return ((IEnumerable)keys(self)).GetEnumerator();
	}

	public static List keys(IDictionary<object, object> self)
	{
		return PythonOps.MakeListFromSequence(self.Keys);
	}

	public static object pop(PythonDictionary self, object key)
	{
		if (self.TryGetValueNoMissing(key, out var value))
		{
			self.Remove(key);
			return value;
		}
		throw PythonOps.KeyError(key);
	}

	public static object pop(PythonDictionary self, object key, object defaultValue)
	{
		if (self.TryGetValueNoMissing(key, out var value))
		{
			self.Remove(key);
			return value;
		}
		return defaultValue;
	}

	public static PythonTuple popitem(IDictionary<object, object> self)
	{
		IEnumerator<KeyValuePair<object, object>> enumerator = self.GetEnumerator();
		if (enumerator.MoveNext())
		{
			object key = enumerator.Current.Key;
			object value = enumerator.Current.Value;
			self.Remove(key);
			return PythonTuple.MakeTuple(key, value);
		}
		throw PythonOps.KeyError("dictionary is empty");
	}

	public static object setdefault(PythonDictionary self, object key)
	{
		return setdefault(self, key, null);
	}

	public static object setdefault(PythonDictionary self, object key, object defaultValue)
	{
		if (self.TryGetValueNoMissing(key, out var value))
		{
			return value;
		}
		self[key] = defaultValue;
		return defaultValue;
	}

	public static void update(CodeContext context, PythonDictionary self, object b)
	{
		if (b is PythonDictionary pythonDictionary)
		{
			pythonDictionary._storage.CopyTo(ref self._storage);
		}
		else
		{
			SlowUpdate(context, self, b);
		}
	}

	private static void SlowUpdate(CodeContext context, PythonDictionary self, object b)
	{
		if (b is DictProxy dictProxy)
		{
			update(context, self, dictProxy.Type.GetMemberDictionary(context, excludeDict: false));
			return;
		}
		if (b is IDictionary dictionary)
		{
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				self._storage.Add(ref self._storage, enumerator.Key, enumerator.Value);
			}
			return;
		}
		if (PythonOps.TryGetBoundAttr(b, "keys", out var ret))
		{
			IEnumerator enumerator2 = PythonOps.GetEnumerator(PythonCalls.Call(context, ret));
			while (enumerator2.MoveNext())
			{
				self._storage.Add(ref self._storage, enumerator2.Current, PythonOps.GetIndex(context, b, enumerator2.Current));
			}
			return;
		}
		IEnumerator enumerator3 = PythonOps.GetEnumerator(b);
		int num = 0;
		while (enumerator3.MoveNext())
		{
			if (!AddKeyValue(self, enumerator3.Current))
			{
				throw PythonOps.ValueError("dictionary update sequence element #{0} has bad length; 2 is required", num);
			}
			num++;
		}
	}

	internal static bool TryGetValueVirtual(CodeContext context, PythonDictionary self, object key, ref object DefaultGetItem, out object value)
	{
		if (self is IPythonObject { PythonType: var pythonType })
		{
			PythonTypeSlot slot;
			if (DefaultGetItem == null)
			{
				TypeCache.Dict.TryLookupSlot(context, "__getitem__", out slot);
				slot.TryGetValue(context, self, TypeCache.Dict, out DefaultGetItem);
			}
			if (pythonType.TryLookupSlot(context, "__getitem__", out slot))
			{
				slot.TryGetValue(context, self, pythonType, out var value2);
				if (value2 != DefaultGetItem)
				{
					try
					{
						value = self[key];
						return true;
					}
					catch (KeyNotFoundException)
					{
						value = null;
						return false;
					}
				}
			}
		}
		value = null;
		return false;
	}

	internal static bool AddKeyValue(PythonDictionary self, object o)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(o);
		if (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			if (enumerator.MoveNext())
			{
				object current2 = enumerator.Current;
				self._storage.Add(ref self._storage, current, current2);
				return !enumerator.MoveNext();
			}
		}
		return false;
	}

	internal static int CompareTo(CodeContext context, IDictionary<object, object> left, IDictionary<object, object> right)
	{
		int count = left.Count;
		int count2 = right.Count;
		if (count != count2)
		{
			if (count <= count2)
			{
				return -1;
			}
			return 1;
		}
		List ritems = items(right);
		return CompareToWorker(context, left, ritems);
	}

	internal static int CompareToWorker(CodeContext context, IDictionary<object, object> left, List ritems)
	{
		List list = items(left);
		list.sort(context);
		ritems.sort(context);
		return list.CompareToWorker(ritems);
	}
}
