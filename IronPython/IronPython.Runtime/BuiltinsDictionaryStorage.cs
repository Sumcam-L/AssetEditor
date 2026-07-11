using System;
using IronPython.Modules;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class BuiltinsDictionaryStorage : ModuleDictionaryStorage
{
	private readonly EventHandler<ModuleChangeEventArgs> _change;

	private object _import;

	public BuiltinsDictionaryStorage(EventHandler<ModuleChangeEventArgs> change)
		: base(typeof(Builtin))
	{
		_change = change;
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		if (key is string text)
		{
			if (text == "__import__")
			{
				_import = value;
			}
			_change(this, new ModuleChangeEventArgs(text, ModuleChangeType.Set, value));
		}
		base.Add(ref storage, key, value);
	}

	protected override void LazyAdd(object name, object value)
	{
		Add(name, value);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		if (key is string text)
		{
			if (text == "__import__")
			{
				_import = null;
			}
			_change(this, new ModuleChangeEventArgs(text, ModuleChangeType.Delete));
		}
		return base.Remove(ref storage, key);
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		_import = null;
		base.Clear(ref storage);
	}

	public override bool TryGetImport(out object value)
	{
		if (_import == null)
		{
			if (base.TryGetImport(out value))
			{
				_import = value;
				return true;
			}
			return false;
		}
		value = _import;
		return true;
	}

	public override void Reload()
	{
		_import = null;
		base.Reload();
	}
}
