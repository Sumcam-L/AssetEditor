using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

internal class GetMemberDelegates : UserGetBase
{
	private readonly string _name;

	private readonly bool _isNoThrow;

	private readonly PythonTypeSlot _slot;

	private readonly PythonTypeSlot _getattrSlot;

	private readonly SlotGetValue _slotFunc;

	private readonly Func<CallSite, object, CodeContext, object> _fallback;

	private readonly int _dictVersion;

	private readonly int _dictIndex;

	private readonly ExtensionMethodSet _extMethods;

	public GetMemberDelegates(OptimizedGetKind getKind, PythonType type, PythonGetMemberBinder binder, string name, int version, PythonTypeSlot slot, PythonTypeSlot getattrSlot, SlotGetValue slotFunc, Func<CallSite, object, CodeContext, object> fallback, ExtensionMethodSet extMethods)
		: base(binder, version)
	{
		_slot = slot;
		_name = name;
		_getattrSlot = getattrSlot;
		_slotFunc = slotFunc;
		_fallback = fallback;
		_isNoThrow = binder.IsNoThrow;
		_extMethods = extMethods;
		IList<string> optimizedInstanceNames = type.GetOptimizedInstanceNames();
		switch (getKind)
		{
		case OptimizedGetKind.SlotDict:
			if (optimizedInstanceNames != null)
			{
				_dictIndex = optimizedInstanceNames.IndexOf(name);
			}
			if (optimizedInstanceNames != null && _dictIndex != -1)
			{
				_func = SlotDictOptimized;
				_dictVersion = type.GetOptimizedInstanceVersion();
			}
			else
			{
				_func = SlotDict;
			}
			break;
		case OptimizedGetKind.SlotOnly:
			_func = SlotOnly;
			break;
		case OptimizedGetKind.PropertySlot:
			_func = UserSlot;
			break;
		case OptimizedGetKind.UserSlotDict:
			if (optimizedInstanceNames != null)
			{
				_dictIndex = optimizedInstanceNames.IndexOf(name);
			}
			if (optimizedInstanceNames != null && _dictIndex != -1)
			{
				_dictVersion = type.GetOptimizedInstanceVersion();
				if (_getattrSlot != null)
				{
					_func = UserSlotDictGetAttrOptimized;
				}
				else
				{
					_func = UserSlotDictOptimized;
				}
			}
			else if (_getattrSlot != null)
			{
				_func = UserSlotDictGetAttr;
			}
			else
			{
				_func = UserSlotDict;
			}
			break;
		case OptimizedGetKind.UserSlotOnly:
			if (_getattrSlot != null)
			{
				_func = UserSlotOnlyGetAttr;
			}
			else
			{
				_func = UserSlotOnly;
			}
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	public object SlotDict(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			_hitCount++;
			if (pythonObject.Dict != null && pythonObject.Dict.TryGetValue(_name, out var value))
			{
				return value;
			}
			if (_slot != null && _slot.TryGetValue(context, self, pythonObject.PythonType, out value))
			{
				return value;
			}
			if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out value))
			{
				return GetAttr(context, value);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object SlotDictOptimized(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			_hitCount++;
			PythonDictionary dict = pythonObject.Dict;
			if (UserTypeOps.TryGetDictionaryValue(dict, _name, _dictVersion, _dictIndex, out var res))
			{
				return res;
			}
			if (_slot != null && _slot.TryGetValue(context, self, pythonObject.PythonType, out res))
			{
				return res;
			}
			if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out res))
			{
				return GetAttr(context, res);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object SlotOnly(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			_hitCount++;
			if (_slot != null && _slot.TryGetValue(context, self, pythonObject.PythonType, out var value))
			{
				return value;
			}
			if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out value))
			{
				return GetAttr(context, value);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlotDict(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			if (pythonObject.Dict != null && pythonObject.Dict.TryGetValue(_name, out var value))
			{
				return value;
			}
			return ((PythonTypeUserDescriptorSlot)_slot).GetValue(context, self, pythonObject.PythonType);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlotDictOptimized(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			PythonDictionary dict = pythonObject.Dict;
			if (UserTypeOps.TryGetDictionaryValue(dict, _name, _dictVersion, _dictIndex, out var res))
			{
				return res;
			}
			return ((PythonTypeUserDescriptorSlot)_slot).GetValue(context, self, pythonObject.PythonType);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlotOnly(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			return ((PythonTypeUserDescriptorSlot)_slot).GetValue(context, self, pythonObject.PythonType);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlotDictGetAttr(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			if (pythonObject.Dict != null && pythonObject.Dict.TryGetValue(_name, out var value))
			{
				return value;
			}
			try
			{
				return ((PythonTypeUserDescriptorSlot)_slot).GetValue(context, self, pythonObject.PythonType);
			}
			catch (MissingMemberException)
			{
			}
			if (_getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out value))
			{
				return GetAttr(context, value);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlotDictGetAttrOptimized(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			PythonDictionary dict = pythonObject.Dict;
			if (UserTypeOps.TryGetDictionaryValue(dict, _name, _dictVersion, _dictIndex, out var res))
			{
				return res;
			}
			try
			{
				return ((PythonTypeUserDescriptorSlot)_slot).GetValue(context, self, pythonObject.PythonType);
			}
			catch (MissingMemberException)
			{
			}
			if (_getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out res))
			{
				return GetAttr(context, res);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlotOnlyGetAttr(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			try
			{
				return ((PythonTypeUserDescriptorSlot)_slot).GetValue(context, self, pythonObject.PythonType);
			}
			catch (MissingMemberException)
			{
			}
			if (_getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out var value))
			{
				return GetAttr(context, value);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	public object UserSlot(CallSite site, object self, CodeContext context)
	{
		if (self is IPythonObject pythonObject && pythonObject.PythonType.Version == _version && base.ShouldUseNonOptimizedSite && (object)context.ModuleContext.ExtensionMethods == _extMethods)
		{
			object value = _slotFunc(self);
			if (value != Uninitialized.Instance)
			{
				return value;
			}
			if (_getattrSlot != null && _getattrSlot.TryGetValue(context, self, pythonObject.PythonType, out value))
			{
				return GetAttr(context, value);
			}
			return TypeError(site, pythonObject, context);
		}
		return FastGetBase.Update(site, self, context);
	}

	private object GetAttr(CodeContext context, object res)
	{
		if (_isNoThrow)
		{
			try
			{
				return PythonContext.GetContext(context).Call(context, res, _name);
			}
			catch (MissingMemberException)
			{
				return OperationFailed.Value;
			}
		}
		return PythonContext.GetContext(context).Call(context, res, _name);
	}

	private object TypeError(CallSite site, IPythonObject ipo, CodeContext context)
	{
		return _fallback(site, ipo, context);
	}
}
