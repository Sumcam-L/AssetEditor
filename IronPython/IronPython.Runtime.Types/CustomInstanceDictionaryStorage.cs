using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

[Serializable]
internal sealed class CustomInstanceDictionaryStorage : StringDictionaryStorage
{
	private readonly int _keyVersion;

	private readonly string[] _extraKeys;

	private readonly object[] _values;

	private static int _namesVersion;

	public override int Count
	{
		get
		{
			int num = base.Count;
			object[] values = _values;
			foreach (object obj in values)
			{
				if (obj != Uninitialized.Instance)
				{
					num++;
				}
			}
			return num;
		}
	}

	public int KeyVersion => _keyVersion;

	internal static int AllocateVersion()
	{
		return Interlocked.Increment(ref _namesVersion);
	}

	public CustomInstanceDictionaryStorage(string[] extraKeys, int keyVersion)
	{
		_extraKeys = extraKeys;
		_keyVersion = keyVersion;
		_values = new object[extraKeys.Length];
		for (int i = 0; i < _values.Length; i++)
		{
			_values[i] = Uninitialized.Instance;
		}
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		int num = FindKey(key);
		if (num != -1)
		{
			_values[num] = value;
		}
		else
		{
			base.Add(ref storage, key, value);
		}
	}

	public override void AddNoLock(ref DictionaryStorage storage, object key, object value)
	{
		int num = FindKey(key);
		if (num != -1)
		{
			_values[num] = value;
		}
		else
		{
			base.AddNoLock(ref storage, key, value);
		}
	}

	public override bool Contains(object key)
	{
		int num = FindKey(key);
		if (num != -1)
		{
			return _values[num] != Uninitialized.Instance;
		}
		return base.Contains(key);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		int num = FindKey(key);
		if (num != -1)
		{
			if (Interlocked.Exchange<object>(ref _values[num], (object)Uninitialized.Instance) != Uninitialized.Instance)
			{
				return true;
			}
			return false;
		}
		return base.Remove(ref storage, key);
	}

	public override bool TryGetValue(object key, out object value)
	{
		int num = FindKey(key);
		if (num != -1)
		{
			value = _values[num];
			if (value != Uninitialized.Instance)
			{
				return true;
			}
			value = null;
			return false;
		}
		return base.TryGetValue(key, out value);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		for (int i = 0; i < _values.Length; i++)
		{
			_values[i] = Uninitialized.Instance;
		}
		base.Clear(ref storage);
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> items = base.GetItems();
		for (int i = 0; i < _extraKeys.Length; i++)
		{
			if (!string.IsNullOrEmpty(_extraKeys[i]) && _values[i] != Uninitialized.Instance)
			{
				items.Add(new KeyValuePair<object, object>(_extraKeys[i], _values[i]));
			}
		}
		return items;
	}

	public int FindKey(object key)
	{
		if (key is string key2)
		{
			return FindKey(key2);
		}
		return -1;
	}

	public int FindKey(string key)
	{
		for (int i = 0; i < _extraKeys.Length; i++)
		{
			if (_extraKeys[i] == key)
			{
				return i;
			}
		}
		return -1;
	}

	public bool TryGetValue(int index, out object value)
	{
		value = _values[index];
		return value != Uninitialized.Instance;
	}

	public object GetValueHelper(int index, object oldInstance)
	{
		object obj = _values[index];
		if (obj != Uninitialized.Instance)
		{
			return obj;
		}
		return ((OldInstance)oldInstance).GetBoundMember(null, _extraKeys[index]);
	}

	public bool TryGetValueHelper(int index, object oldInstance, out object res)
	{
		res = _values[index];
		if (res != Uninitialized.Instance)
		{
			return true;
		}
		return ((OldInstance)oldInstance).TryGetBoundCustomMember(null, _extraKeys[index], out res);
	}

	public void SetExtraValue(int index, object value)
	{
		_values[index] = value;
	}
}
