using System.Collections.Generic;
using IronPython.Compiler;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class GlobalDictionaryStorage : CustomDictionaryStorage
{
	private readonly Dictionary<string, PythonGlobal> _globals;

	private readonly PythonGlobal[] _data;

	private PythonGlobal _path;

	private PythonGlobal _package;

	private PythonGlobal _builtins;

	private PythonGlobal _name;

	public PythonGlobal[] Data => _data;

	public GlobalDictionaryStorage(Dictionary<string, PythonGlobal> globals)
	{
		_globals = globals;
	}

	public GlobalDictionaryStorage(Dictionary<string, PythonGlobal> globals, PythonGlobal[] data)
	{
		_globals = globals;
		_data = data;
	}

	protected override IEnumerable<KeyValuePair<string, object>> GetExtraItems()
	{
		foreach (KeyValuePair<string, PythonGlobal> global in _globals)
		{
			KeyValuePair<string, PythonGlobal> keyValuePair = global;
			if (keyValuePair.Value.RawValue != Uninitialized.Instance)
			{
				KeyValuePair<string, PythonGlobal> keyValuePair2 = global;
				string key = keyValuePair2.Key;
				KeyValuePair<string, PythonGlobal> keyValuePair3 = global;
				yield return new KeyValuePair<string, object>(key, keyValuePair3.Value.RawValue);
			}
		}
	}

	protected override bool? TryRemoveExtraValue(string key)
	{
		if (_globals.TryGetValue(key, out var value))
		{
			if (value.RawValue != Uninitialized.Instance)
			{
				value.RawValue = Uninitialized.Instance;
				return true;
			}
			return false;
		}
		return null;
	}

	protected override bool TrySetExtraValue(string key, object value)
	{
		if (_globals.TryGetValue(key, out var value2))
		{
			value2.CurrentValue = value;
			return true;
		}
		return false;
	}

	protected override bool TryGetExtraValue(string key, out object value)
	{
		if (_globals.TryGetValue(key, out var value2))
		{
			value = value2.RawValue;
			return true;
		}
		value = null;
		return false;
	}

	public override bool TryGetBuiltins(out object value)
	{
		return TryGetCachedValue(ref _builtins, "__builtins__", out value);
	}

	public override bool TryGetPath(out object value)
	{
		return TryGetCachedValue(ref _path, "__path__", out value);
	}

	public override bool TryGetPackage(out object value)
	{
		return TryGetCachedValue(ref _package, "__package__", out value);
	}

	public override bool TryGetName(out object value)
	{
		return TryGetCachedValue(ref _name, "__name__", out value);
	}

	private bool TryGetCachedValue(ref PythonGlobal storage, string name, out object value)
	{
		if (storage == null && !_globals.TryGetValue(name, out storage))
		{
			return TryGetValue(name, out value);
		}
		value = storage.RawValue;
		return value != Uninitialized.Instance;
	}
}
