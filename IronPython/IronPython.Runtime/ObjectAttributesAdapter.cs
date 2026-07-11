using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

internal class ObjectAttributesAdapter : DictionaryStorage
{
	private readonly object _backing;

	private readonly CodeContext _context;

	internal object Backing => _backing;

	public override int Count => PythonOps.Length(_backing);

	private ICollection<object> Keys => (ICollection<object>)Converter.Convert(PythonOps.Invoke(_context, _backing, "keys"), typeof(ICollection<object>));

	public ObjectAttributesAdapter(CodeContext context, object backing)
	{
		_backing = backing;
		_context = context;
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		PythonContext.GetContext(_context).SetIndex(_backing, key, value);
	}

	public override bool Contains(object key)
	{
		object value;
		return TryGetValue(key, out value);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		try
		{
			PythonContext.GetContext(_context).DelIndex(_backing, key);
			return true;
		}
		catch (KeyNotFoundException)
		{
			return false;
		}
	}

	public override bool TryGetValue(object key, out object value)
	{
		try
		{
			value = PythonOps.GetIndex(_context, _backing, key);
			return true;
		}
		catch (KeyNotFoundException)
		{
		}
		value = null;
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		PythonOps.Invoke(_context, _backing, "clear");
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();
		foreach (object key in Keys)
		{
			TryGetValue(key, out var value);
			list.Add(new KeyValuePair<object, object>(key, value));
		}
		return list;
	}
}
