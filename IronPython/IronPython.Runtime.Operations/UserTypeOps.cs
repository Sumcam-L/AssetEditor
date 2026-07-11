using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class UserTypeOps
{
	public static string ToStringReturnHelper(object o)
	{
		if (o is string && o != null)
		{
			return (string)o;
		}
		throw PythonOps.TypeError("__str__ returned non-string type ({0})", PythonTypeOps.GetName(o));
	}

	public static PythonDictionary SetDictHelper(ref PythonDictionary dict, PythonDictionary value)
	{
		if (Interlocked.CompareExchange(ref dict, value, null) == null)
		{
			return value;
		}
		return dict;
	}

	public static object GetPropertyHelper(object prop, object instance, string name)
	{
		if (!(prop is PythonTypeSlot pythonTypeSlot))
		{
			throw PythonOps.TypeError("Expected property for {0}, but found {1}", name.ToString(), DynamicHelpers.GetPythonType(prop).Name);
		}
		pythonTypeSlot.TryGetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), out var value);
		return value;
	}

	public static void SetPropertyHelper(object prop, object instance, object newValue, string name)
	{
		if (!(prop is PythonTypeSlot pythonTypeSlot))
		{
			throw PythonOps.TypeError("Expected settable property for {0}, but found {1}", name.ToString(), DynamicHelpers.GetPythonType(prop).Name);
		}
		pythonTypeSlot.TrySetValue(DefaultContext.Default, instance, DynamicHelpers.GetPythonType(instance), newValue);
	}

	public static bool SetWeakRefHelper(IPythonObject obj, WeakRefTracker value)
	{
		if (!obj.PythonType.IsWeakReferencable)
		{
			return false;
		}
		object[] slotsCreate = obj.GetSlotsCreate();
		slotsCreate[slotsCreate.Length - 1] = value;
		return true;
	}

	public static WeakRefTracker GetWeakRefHelper(IPythonObject obj)
	{
		object[] slots = obj.GetSlots();
		if (slots == null)
		{
			return null;
		}
		return (WeakRefTracker)slots[slots.Length - 1];
	}

	public static void SetFinalizerHelper(IPythonObject obj, WeakRefTracker value)
	{
		object[] slotsCreate = obj.GetSlotsCreate();
		if (Interlocked.CompareExchange(ref slotsCreate[slotsCreate.Length - 1], value, null) != null)
		{
			GC.SuppressFinalize(value);
		}
	}

	public static object[] GetSlotsCreate(IPythonObject obj, ref object[] slots)
	{
		if (slots != null)
		{
			return slots;
		}
		Interlocked.CompareExchange(ref slots, new object[obj.PythonType.SlotCount + 1], null);
		return slots;
	}

	public static void AddRemoveEventHelper(object method, IPythonObject instance, object eventValue, string name)
	{
		object value = method;
		PythonType pythonType = instance.PythonType;
		if (method is PythonTypeSlot pythonTypeSlot && !pythonTypeSlot.TryGetValue(DefaultContext.Default, instance, pythonType, out value))
		{
			throw PythonOps.AttributeErrorForMissingAttribute(pythonType.Name, name);
		}
		if (!PythonOps.IsCallable(DefaultContext.Default, value))
		{
			throw PythonOps.TypeError("Expected callable value for {0}, but found {1}", name.ToString(), PythonTypeOps.GetName(method));
		}
		PythonCalls.Call(value, eventValue);
	}

	public static DynamicMetaObject GetMetaObjectHelper(IPythonObject self, Expression parameter, DynamicMetaObject baseMetaObject)
	{
		return new MetaUserObject(parameter, BindingRestrictions.Empty, baseMetaObject, self);
	}

	public static bool TryGetMixedNewStyleOldStyleSlot(CodeContext context, object instance, string name, out object value)
	{
		if (instance is IPythonObject { Dict: { } dict } && dict.TryGetValue(name, out value))
		{
			return true;
		}
		PythonType pythonType = DynamicHelpers.GetPythonType(instance);
		foreach (PythonType item in pythonType.ResolutionOrder)
		{
			PythonTypeSlot slot;
			if (item != TypeCache.Object && item.OldClass != null)
			{
				OldClass oldClass = item.OldClass;
				if (oldClass.TryGetBoundCustomMember(context, name, out value))
				{
					value = oldClass.GetOldStyleDescriptor(context, value, instance, oldClass);
					return true;
				}
			}
			else if (item.TryLookupSlot(context, name, out slot))
			{
				return slot.TryGetValue(context, instance, pythonType, out value);
			}
		}
		value = null;
		return false;
	}

	public static bool TryGetDictionaryValue(PythonDictionary dict, string name, int keyVersion, int keyIndex, out object res)
	{
		if (dict != null)
		{
			if (dict._storage is CustomInstanceDictionaryStorage customInstanceDictionaryStorage && customInstanceDictionaryStorage.KeyVersion == keyVersion)
			{
				if (customInstanceDictionaryStorage.TryGetValue(keyIndex, out res))
				{
					return true;
				}
			}
			else if (dict.TryGetValue(name, out res))
			{
				return true;
			}
		}
		res = null;
		return false;
	}

	public static object SetDictionaryValue(IPythonObject self, string name, object value)
	{
		PythonDictionary dictionary = GetDictionary(self);
		return dictionary[name] = value;
	}

	public static object SetDictionaryValueOptimized(IPythonObject ipo, string name, object value, int keysVersion, int index)
	{
		PythonDictionary dictionary = GetDictionary(ipo);
		if (dictionary._storage is CustomInstanceDictionaryStorage customInstanceDictionaryStorage && customInstanceDictionaryStorage.KeyVersion == keysVersion)
		{
			customInstanceDictionaryStorage.SetExtraValue(index, value);
		}
		else
		{
			dictionary[name] = value;
		}
		return value;
	}

	public static object FastSetDictionaryValue(ref PythonDictionary dict, string name, object value)
	{
		if (dict == null)
		{
			Interlocked.CompareExchange(ref dict, PythonDictionary.MakeSymbolDictionary(), null);
		}
		return dict[name] = value;
	}

	public static object FastSetDictionaryValueOptimized(PythonType type, ref PythonDictionary dict, string name, object value, int keysVersion, int index)
	{
		if (dict == null)
		{
			Interlocked.CompareExchange(ref dict, type.MakeDictionary(), null);
		}
		if (dict._storage is CustomInstanceDictionaryStorage customInstanceDictionaryStorage && customInstanceDictionaryStorage.KeyVersion == keysVersion)
		{
			customInstanceDictionaryStorage.SetExtraValue(index, value);
			return value;
		}
		return dict[name] = value;
	}

	public static object RemoveDictionaryValue(IPythonObject self, string name)
	{
		PythonDictionary dict = self.Dict;
		if (dict != null && dict.Remove(name))
		{
			return null;
		}
		throw PythonOps.AttributeErrorForMissingAttribute(self.PythonType, name);
	}

	internal static PythonDictionary GetDictionary(IPythonObject self)
	{
		PythonDictionary pythonDictionary = self.Dict;
		if (pythonDictionary == null && self.PythonType.HasDictionary)
		{
			pythonDictionary = self.SetDict(self.PythonType.MakeDictionary());
		}
		return pythonDictionary;
	}

	public static string ToStringHelper(IPythonObject o)
	{
		return ObjectOps.__str__(DefaultContext.Default, o);
	}

	public static bool TryGetNonInheritedMethodHelper(PythonType dt, object instance, string name, out object callTarget)
	{
		foreach (PythonType item in dt.ResolutionOrder)
		{
			if (!item.IsSystemType)
			{
				if (LookupValue(item, instance, name, out callTarget))
				{
					return true;
				}
				continue;
			}
			break;
		}
		PythonDictionary dict;
		if (instance is IPythonObject pythonObject && (dict = pythonObject.Dict) != null && dict.TryGetValue(name, out callTarget))
		{
			return true;
		}
		callTarget = null;
		return false;
	}

	private static bool LookupValue(PythonType dt, object instance, string name, out object value)
	{
		if (dt.TryLookupSlot(DefaultContext.Default, name, out var slot) && slot.TryGetValue(DefaultContext.Default, instance, dt, out value))
		{
			return true;
		}
		value = null;
		return false;
	}

	public static bool TryGetNonInheritedValueHelper(IPythonObject instance, string name, out object callTarget)
	{
		PythonType pythonType = instance.PythonType;
		foreach (PythonType item in pythonType.ResolutionOrder)
		{
			if (!item.IsSystemType)
			{
				if (item.TryLookupSlot(DefaultContext.Default, name, out var slot))
				{
					callTarget = slot;
					return true;
				}
				continue;
			}
			break;
		}
		PythonDictionary dict;
		if (instance != null && (dict = instance.Dict) != null && dict.TryGetValue(name, out callTarget))
		{
			return true;
		}
		callTarget = null;
		return false;
	}

	public static object GetAttribute(CodeContext context, object self, string name, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>> callSite)
	{
		if (callSite.Data == null)
		{
			callSite.Data = MakeGetAttrSite(context);
		}
		object value;
		try
		{
			if (getAttributeSlot.TryGetValue(context, self, ((IPythonObject)self).PythonType, out value))
			{
				return callSite.Data.Target(callSite.Data, context, value, name);
			}
		}
		catch (MissingMemberException)
		{
			if (getAttrSlot != null && getAttrSlot.TryGetValue(context, self, ((IPythonObject)self).PythonType, out value))
			{
				return callSite.Data.Target(callSite.Data, context, value, name);
			}
			throw;
		}
		if (getAttrSlot != null && getAttrSlot.TryGetValue(context, self, ((IPythonObject)self).PythonType, out value))
		{
			return callSite.Data.Target(callSite.Data, context, value, name);
		}
		throw PythonOps.AttributeError(name);
	}

	public static object GetAttributeNoThrow(CodeContext context, object self, string name, PythonTypeSlot getAttributeSlot, PythonTypeSlot getAttrSlot, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, string, object>>> callSite)
	{
		if (callSite.Data == null)
		{
			callSite.Data = MakeGetAttrSite(context);
		}
		object value;
		try
		{
			if (getAttributeSlot.TryGetValue(context, self, ((IPythonObject)self).PythonType, out value))
			{
				return callSite.Data.Target(callSite.Data, context, value, name);
			}
		}
		catch (MissingMemberException)
		{
			try
			{
				if (getAttrSlot != null && getAttrSlot.TryGetValue(context, self, ((IPythonObject)self).PythonType, out value))
				{
					return callSite.Data.Target(callSite.Data, context, value, name);
				}
				return OperationFailed.Value;
			}
			catch (MissingMemberException)
			{
				return OperationFailed.Value;
			}
		}
		try
		{
			if (getAttrSlot != null && getAttrSlot.TryGetValue(context, self, ((IPythonObject)self).PythonType, out value))
			{
				return callSite.Data.Target(callSite.Data, context, value, name);
			}
		}
		catch (MissingMemberException)
		{
		}
		return OperationFailed.Value;
	}

	private static CallSite<Func<CallSite, CodeContext, object, string, object>> MakeGetAttrSite(CodeContext context)
	{
		return CallSite<Func<CallSite, CodeContext, object, string, object>>.Create(PythonContext.GetContext(context).InvokeOne);
	}

	internal static FastBindResult<T> MakeGetBinding<T>(CodeContext codeContext, CallSite<T> site, IPythonObject self, PythonGetMemberBinder getBinder) where T : class
	{
		Type finalSystemType = self.PythonType.FinalSystemType;
		if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(finalSystemType) && !(self is IFastGettable))
		{
			return default(FastBindResult<T>);
		}
		return (FastBindResult<T>)(object)new MetaUserObject.FastGetBinderHelper(codeContext, (CallSite<Func<CallSite, object, CodeContext, object>>)(object)site, self, getBinder).GetBinding(codeContext, getBinder.Name);
	}

	internal static FastBindResult<T> MakeSetBinding<T>(CodeContext codeContext, CallSite<T> site, IPythonObject self, object value, PythonSetMemberBinder setBinder) where T : class
	{
		if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(self.GetType().BaseType))
		{
			return default(FastBindResult<T>);
		}
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(Func<CallSite, object, object, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<object>(codeContext, self, value, setBinder).MakeSet();
		}
		if (typeFromHandle == typeof(Func<CallSite, object, string, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<string>(codeContext, self, value, setBinder).MakeSet();
		}
		if (typeFromHandle == typeof(Func<CallSite, object, int, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<int>(codeContext, self, value, setBinder).MakeSet();
		}
		if (typeFromHandle == typeof(Func<CallSite, object, double, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<double>(codeContext, self, value, setBinder).MakeSet();
		}
		if (typeFromHandle == typeof(Func<CallSite, object, List, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<List>(codeContext, self, value, setBinder).MakeSet();
		}
		if (typeFromHandle == typeof(Func<CallSite, object, PythonTuple, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<PythonTuple>(codeContext, self, value, setBinder).MakeSet();
		}
		if (typeFromHandle == typeof(Func<CallSite, object, PythonDictionary, object>))
		{
			return (FastBindResult<T>)(object)new MetaUserObject.FastSetBinderHelper<PythonDictionary>(codeContext, self, value, setBinder).MakeSet();
		}
		return default(FastBindResult<T>);
	}
}
