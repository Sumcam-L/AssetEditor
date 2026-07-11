using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime.Types;

internal class SetMemberDelegates<TValue> : FastSetBase<TValue>
{
	private readonly string _name;

	private readonly PythonTypeSlot _slot;

	private readonly SlotSetValue _slotFunc;

	private readonly CodeContext _context;

	private readonly int _index;

	private readonly int _keysVersion;

	public SetMemberDelegates(CodeContext context, PythonType type, OptimizedSetKind kind, string name, int version, PythonTypeSlot slot, SlotSetValue slotFunc)
		: base(version)
	{
		_slot = slot;
		_name = name;
		_slotFunc = slotFunc;
		_context = context;
		switch (kind)
		{
		case OptimizedSetKind.SetAttr:
			_func = new Func<CallSite, object, TValue, object>(SetAttr);
			break;
		case OptimizedSetKind.UserSlot:
			_func = new Func<CallSite, object, TValue, object>(UserSlot);
			break;
		case OptimizedSetKind.SetDict:
		{
			IList<string> optimizedInstanceNames = type.GetOptimizedInstanceNames();
			int index;
			if (optimizedInstanceNames != null && (index = optimizedInstanceNames.IndexOf(name)) != -1)
			{
				_index = index;
				_keysVersion = type.GetOptimizedInstanceVersion();
				_func = new Func<CallSite, object, TValue, object>(SetDictOptimized);
			}
			else
			{
				_func = new Func<CallSite, object, TValue, object>(SetDict);
			}
			break;
		}
		case OptimizedSetKind.Error:
			_func = new Func<CallSite, object, TValue, object>(Error);
			break;
		}
	}

	public object SetAttr(CallSite site, object self, TValue value)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite)
		{
			_hitCount++;
			if (_slot.TryGetValue(_context, self, pythonObject.PythonType, out var value2))
			{
				return PythonOps.CallWithContext(_context, value2, _name, value);
			}
			return TypeError(pythonObject);
		}
		return FastSetBase<TValue>.Update(site, self, value);
	}

	public object SetDictOptimized(CallSite site, object self, TValue value)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite)
		{
			_hitCount++;
			return UserTypeOps.SetDictionaryValueOptimized(pythonObject, _name, value, _keysVersion, _index);
		}
		return FastSetBase<TValue>.Update(site, self, value);
	}

	public object SetDict(CallSite site, object self, TValue value)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite)
		{
			_hitCount++;
			UserTypeOps.SetDictionaryValue(pythonObject, _name, value);
			return null;
		}
		return FastSetBase<TValue>.Update(site, self, value);
	}

	public object Error(CallSite site, object self, TValue value)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version)
		{
			return TypeError(pythonObject);
		}
		return FastSetBase<TValue>.Update(site, self, value);
	}

	public object UserSlot(CallSite site, object self, TValue value)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite)
		{
			_hitCount++;
			_slotFunc(self, value);
			return null;
		}
		return FastSetBase<TValue>.Update(site, self, value);
	}

	private object TypeError(IPythonObject ipo)
	{
		throw PythonOps.AttributeErrorForMissingAttribute(ipo.PythonType.Name, _name);
	}
}
