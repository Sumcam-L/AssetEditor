using System;
using System.Collections.Generic;
using Microsoft.Scripting.Actions;

namespace IronPython.Runtime;

[Serializable]
internal class WrapperDictionaryStorage : DictionaryStorage
{
	private TopNamespaceTracker _data;

	public override int Count => _data.Count;

	public WrapperDictionaryStorage(TopNamespaceTracker data)
	{
		_data = data;
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		throw CannotModifyNamespaceDict();
	}

	private static InvalidOperationException CannotModifyNamespaceDict()
	{
		return new InvalidOperationException("cannot modify namespace dictionary");
	}

	public override bool Contains(object key)
	{
		if (key is string name)
		{
			return _data.ContainsKey(name);
		}
		return false;
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		throw CannotModifyNamespaceDict();
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (key is string name)
		{
			return _data.TryGetValue(name, out value);
		}
		value = null;
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		throw CannotModifyNamespaceDict();
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>(_data.Count);
		foreach (KeyValuePair<string, object> datum in _data)
		{
			list.Add(new KeyValuePair<object, object>(datum.Key, datum.Value));
		}
		return list;
	}
}
