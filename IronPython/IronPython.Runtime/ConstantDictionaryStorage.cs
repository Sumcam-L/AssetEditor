using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[Serializable]
internal class ConstantDictionaryStorage : DictionaryStorage, IExpressionSerializable
{
	private readonly CommonDictionaryStorage _storage;

	public override int Count => _storage.Count;

	public ConstantDictionaryStorage(CommonDictionaryStorage storage)
	{
		_storage = storage;
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		lock (this)
		{
			if (storage == this)
			{
				CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
				_storage.CopyTo(commonDictionaryStorage);
				commonDictionaryStorage.AddNoLock(key, value);
				storage = commonDictionaryStorage;
				return;
			}
		}
		storage.Add(ref storage, key, value);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		if (_storage.Contains(key))
		{
			lock (this)
			{
				if (storage == this)
				{
					CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
					_storage.CopyTo(commonDictionaryStorage);
					commonDictionaryStorage.Remove(key);
					storage = commonDictionaryStorage;
					return true;
				}
			}
			return storage.Remove(ref storage, key);
		}
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		lock (this)
		{
			if (storage == this)
			{
				storage = EmptyDictionaryStorage.Instance;
				return;
			}
		}
		storage.Clear(ref storage);
	}

	public override bool Contains(object key)
	{
		return _storage.Contains(key);
	}

	public override bool TryGetValue(object key, out object value)
	{
		return _storage.TryGetValue(key, out value);
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		return _storage.GetItems();
	}

	public override DictionaryStorage Clone()
	{
		return _storage.Clone();
	}

	public override bool HasNonStringAttributes()
	{
		return _storage.HasNonStringAttributes();
	}

	public Expression CreateExpression()
	{
		Expression[] array = new Expression[Count * 2];
		int num = 0;
		foreach (KeyValuePair<object, object> item in GetItems())
		{
			array[num++] = Utils.Convert(Utils.Constant(item.Value), typeof(object));
			array[num++] = Utils.Convert(Utils.Constant(item.Key), typeof(object));
		}
		return Expression.Call(typeof(PythonOps).GetMethod("MakeConstantDictStorage"), Expression.NewArrayInit(typeof(object), array));
	}
}
