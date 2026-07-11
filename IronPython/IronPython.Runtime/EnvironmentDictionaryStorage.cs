using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;

namespace IronPython.Runtime;

[Serializable]
internal sealed class EnvironmentDictionaryStorage : DictionaryStorage
{
	private readonly CommonDictionaryStorage _storage = new CommonDictionaryStorage();

	public override int Count => _storage.Count;

	public EnvironmentDictionaryStorage()
	{
		AddEnvironmentVars();
	}

	private void AddEnvironmentVars()
	{
		try
		{
			foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
			{
				_storage.Add(environmentVariable.Key, environmentVariable.Value);
			}
		}
		catch (SecurityException)
		{
		}
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		_storage.Add(key, value);
		string text = key as string;
		string text2 = value as string;
		if (text != null && text2 != null)
		{
			Environment.SetEnvironmentVariable(text, text2);
		}
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		bool result = _storage.Remove(key);
		if (key is string variable)
		{
			Environment.SetEnvironmentVariable(variable, string.Empty);
		}
		return result;
	}

	public override bool Contains(object key)
	{
		return _storage.Contains(key);
	}

	public override bool TryGetValue(object key, out object value)
	{
		return _storage.TryGetValue(key, out value);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		foreach (KeyValuePair<object, object> item in GetItems())
		{
			if (item.Key is string variable)
			{
				Environment.SetEnvironmentVariable(variable, string.Empty);
			}
		}
		_storage.Clear(ref storage);
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		return _storage.GetItems();
	}
}
