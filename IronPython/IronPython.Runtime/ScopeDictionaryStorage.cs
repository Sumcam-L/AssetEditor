using System.Collections.Generic;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class ScopeDictionaryStorage : DictionaryStorage
{
	private readonly Scope _scope;

	private readonly PythonContext _context;

	public override int Count => GetItems().Count;

	internal Scope Scope => _scope;

	public ScopeDictionaryStorage(PythonContext context, Scope scope)
	{
		_scope = scope;
		_context = context;
	}

	public override void Add(ref DictionaryStorage storage, object key, object value)
	{
		if (key is string name)
		{
			PythonOps.ScopeSetMember(_context.SharedContext, _scope, name, value);
			return;
		}
		PythonScopeExtension pythonScopeExtension = (PythonScopeExtension)_context.EnsureScopeExtension(_scope);
		pythonScopeExtension.EnsureObjectKeys().Add(key, value);
	}

	public override bool Contains(object key)
	{
		object value;
		return TryGetValue(key, out value);
	}

	public override bool Remove(ref DictionaryStorage storage, object key)
	{
		return Remove(key);
	}

	private bool Remove(object key)
	{
		if (key is string name)
		{
			if (Contains(key))
			{
				return PythonOps.ScopeDeleteMember(_context.SharedContext, Scope, name);
			}
			return false;
		}
		PythonScopeExtension pythonScopeExtension = (PythonScopeExtension)_context.EnsureScopeExtension(_scope);
		if (pythonScopeExtension.ObjectKeys != null)
		{
			return pythonScopeExtension.ObjectKeys.Remove(key);
		}
		return false;
	}

	public override bool TryGetValue(object key, out object value)
	{
		if (key is string name)
		{
			return PythonOps.ScopeTryGetMember(_context.SharedContext, _scope, name, out value);
		}
		PythonScopeExtension pythonScopeExtension = (PythonScopeExtension)_context.EnsureScopeExtension(_scope);
		if (pythonScopeExtension.ObjectKeys != null && pythonScopeExtension.ObjectKeys.TryGetValue(key, out value))
		{
			return true;
		}
		value = null;
		return false;
	}

	public override void Clear(ref DictionaryStorage storage)
	{
		foreach (KeyValuePair<object, object> item in GetItems())
		{
			Remove(item.Key);
		}
	}

	public override List<KeyValuePair<object, object>> GetItems()
	{
		List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();
		foreach (object item in PythonOps.ScopeGetMemberNames(_context.SharedContext, _scope))
		{
			if (TryGetValue(item, out var value))
			{
				list.Add(new KeyValuePair<object, object>(item, value));
			}
		}
		return list;
	}
}
