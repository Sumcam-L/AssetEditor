using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[Serializable]
[PythonType("instance")]
[DebuggerDisplay("old-style instance of {ClassName}")]
[DebuggerTypeProxy(typeof(OldInstanceDebugView))]
public sealed class OldInstance : ICodeFormattable, ICustomTypeDescriptor, ISerializable, IWeakReferenceable, IDynamicMetaObjectProvider, IPythonMembersList, IMembersList, IFastGettable
{
	internal class OldInstanceDebugView
	{
		private readonly OldInstance _userObject;

		public OldClass __class__ => _userObject._class;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		internal List<ObjectDebugView> Members
		{
			get
			{
				List<ObjectDebugView> list = new List<ObjectDebugView>();
				if (_userObject._dict != null)
				{
					foreach (KeyValuePair<object, object> item in _userObject._dict)
					{
						list.Add(new ObjectDebugView(item.Key, item.Value));
					}
				}
				return list;
			}
		}

		public OldInstanceDebugView(OldInstance userObject)
		{
			_userObject = userObject;
		}
	}

	private class FastOldInstanceGet
	{
		private readonly string _name;

		public FastOldInstanceGet(string name)
		{
			_name = name;
		}

		public object Target(CallSite site, object instance, CodeContext context)
		{
			if (instance is OldInstance oldInstance)
			{
				if (oldInstance.TryGetBoundCustomMember(context, _name, out var value))
				{
					return value;
				}
				throw PythonOps.AttributeError("{0} instance has no attribute '{1}'", oldInstance._class.Name, _name);
			}
			return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, instance, context);
		}

		public object LightThrowTarget(CallSite site, object instance, CodeContext context)
		{
			if (instance is OldInstance oldInstance)
			{
				if (oldInstance.TryGetBoundCustomMember(context, _name, out var value))
				{
					return value;
				}
				return LightExceptions.Throw(PythonOps.AttributeError("{0} instance has no attribute '{1}'", oldInstance._class.Name, _name));
			}
			return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, instance, context);
		}

		public object NoThrowTarget(CallSite site, object instance, CodeContext context)
		{
			if (instance is OldInstance oldInstance)
			{
				if (oldInstance.TryGetBoundCustomMember(context, _name, out var value))
				{
					return value;
				}
				return OperationFailed.Value;
			}
			return ((CallSite<Func<CallSite, object, CodeContext, object>>)site).Update(site, instance, context);
		}
	}

	private PythonDictionary _dict;

	internal OldClass _class;

	private WeakRefTracker _weakRef;

	internal PythonDictionary Dictionary => _dict;

	internal string ClassName => _class.Name;

	private static PythonDictionary MakeDictionary(OldClass oldClass)
	{
		return new PythonDictionary(new CustomInstanceDictionaryStorage(oldClass.OptimizedInstanceNames, oldClass.OptimizedInstanceNamesVersion));
	}

	public OldInstance(CodeContext context, OldClass @class)
	{
		_class = @class;
		_dict = MakeDictionary(@class);
		if (_class.HasFinalizer)
		{
			AddFinalizer(context);
		}
	}

	public OldInstance(CodeContext context, OldClass @class, PythonDictionary dict)
	{
		_class = @class;
		_dict = dict ?? PythonDictionary.MakeSymbolDictionary();
		if (_class.HasFinalizer)
		{
			AddFinalizer(context);
		}
	}

	private OldInstance(SerializationInfo info, StreamingContext context)
	{
		_class = (OldClass)info.GetValue("__class__", typeof(OldClass));
		_dict = MakeDictionary(_class);
		List<object> list = (List<object>)info.GetValue("keys", typeof(List<object>));
		List<object> list2 = (List<object>)info.GetValue("values", typeof(List<object>));
		for (int i = 0; i < list.Count; i++)
		{
			_dict[list[i]] = list2[i];
		}
	}

	private void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		ContractUtils.RequiresNotNull(info, "info");
		info.AddValue("__class__", _class);
		List<object> list = new List<object>();
		List<object> list2 = new List<object>();
		foreach (object item in _dict.keys())
		{
			list.Add(item);
			_dict.TryGetValue(item, out var value);
			list2.Add(value);
		}
		info.AddValue("keys", list);
		info.AddValue("values", list2);
	}

	public static bool operator true(OldInstance self)
	{
		return (bool)self.__nonzero__(DefaultContext.Default);
	}

	public static bool operator false(OldInstance self)
	{
		return !(bool)self.__nonzero__(DefaultContext.Default);
	}

	public override string ToString()
	{
		object obj = InvokeOne(this, "__str__");
		if (obj != NotImplementedType.Value)
		{
			if (Converter.TryConvertToString(obj, out var result) && result != null)
			{
				return result;
			}
			throw PythonOps.TypeError("__str__ returned non-string type ({0})", PythonTypeOps.GetName(obj));
		}
		return __repr__(DefaultContext.Default);
	}

	public string __repr__(CodeContext context)
	{
		object obj = InvokeOne(this, "__repr__");
		if (obj != NotImplementedType.Value)
		{
			if (Converter.TryConvertToString(obj, out var result) && result != null)
			{
				return result;
			}
			throw PythonOps.TypeError("__repr__ returned non-string type ({0})", PythonTypeOps.GetName(obj));
		}
		return $"<{_class.FullName} instance at {PythonOps.HexId(this)}>";
	}

	[return: MaybeNotImplemented]
	public object __divmod__(CodeContext context, object divmod)
	{
		if (TryGetBoundCustomMember(context, "__divmod__", out var value))
		{
			return PythonCalls.Call(context, value, divmod);
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object __rdivmod__(CodeContext context, object divmod, [NotNull] OldInstance self)
	{
		if (self.TryGetBoundCustomMember(context, "__rdivmod__", out var value))
		{
			return PythonCalls.Call(context, value, divmod);
		}
		return NotImplementedType.Value;
	}

	public object __coerce__(CodeContext context, object other)
	{
		if (TryGetBoundCustomMember(context, "__coerce__", out var value))
		{
			return PythonCalls.Call(context, value, other);
		}
		return NotImplementedType.Value;
	}

	public object __len__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__len__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__len__");
	}

	public object __pos__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__pos__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__pos__");
	}

	[SpecialName]
	public object GetItem(CodeContext context, object item)
	{
		return PythonOps.Invoke(context, this, "__getitem__", item);
	}

	[SpecialName]
	public void SetItem(CodeContext context, object item, object value)
	{
		PythonOps.Invoke(context, this, "__setitem__", item, value);
	}

	[SpecialName]
	public object DeleteItem(CodeContext context, object item)
	{
		if (TryGetBoundCustomMember(context, "__delitem__", out var value))
		{
			return PythonCalls.Call(context, value, item);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__delitem__");
	}

	public object __getslice__(CodeContext context, int i, int j)
	{
		if (TryRawGetAttr(context, "__getslice__", out var ret))
		{
			return PythonCalls.Call(context, ret, i, j);
		}
		if (TryRawGetAttr(context, "__getitem__", out ret))
		{
			return PythonCalls.Call(context, ret, new Slice(i, j));
		}
		throw PythonOps.TypeError("instance {0} does not have __getslice__ or __getitem__", _class.Name);
	}

	public void __setslice__(CodeContext context, int i, int j, object value)
	{
		if (TryRawGetAttr(context, "__setslice__", out var ret))
		{
			PythonCalls.Call(context, ret, i, j, value);
			return;
		}
		if (TryRawGetAttr(context, "__setitem__", out ret))
		{
			PythonCalls.Call(context, ret, new Slice(i, j), value);
			return;
		}
		throw PythonOps.TypeError("instance {0} does not have __setslice__ or __setitem__", _class.Name);
	}

	public object __delslice__(CodeContext context, int i, int j)
	{
		if (TryRawGetAttr(context, "__delslice__", out var ret))
		{
			return PythonCalls.Call(context, ret, i, j);
		}
		if (TryRawGetAttr(context, "__delitem__", out ret))
		{
			return PythonCalls.Call(context, ret, new Slice(i, j));
		}
		throw PythonOps.TypeError("instance {0} does not have __delslice__ or __delitem__", _class.Name);
	}

	public object __index__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__int__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.TypeError("object cannot be converted to an index");
	}

	public object __neg__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__neg__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__neg__");
	}

	public object __abs__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__abs__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__abs__");
	}

	public object __invert__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__invert__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__invert__");
	}

	public object __contains__(CodeContext context, object index)
	{
		if (TryGetBoundCustomMember(context, "__contains__", out var value))
		{
			return PythonCalls.Call(context, value, index);
		}
		IEnumerator enumerator = PythonOps.GetEnumerator(this);
		while (enumerator.MoveNext())
		{
			if (PythonOps.EqualRetBool(context, enumerator.Current, index))
			{
				return ScriptingRuntimeHelpers.True;
			}
		}
		return ScriptingRuntimeHelpers.False;
	}

	[SpecialName]
	public object Call(CodeContext context)
	{
		return Call(context, ArrayUtils.EmptyObjects);
	}

	[SpecialName]
	public object Call(CodeContext context, object args)
	{
		try
		{
			PythonOps.FunctionPushFrame(PythonContext.GetContext(context));
			if (TryGetBoundCustomMember(context, "__call__", out var value))
			{
				return PythonOps.CallWithContext(context, value, args);
			}
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
		throw PythonOps.AttributeError("{0} instance has no __call__ method", _class.Name);
	}

	[SpecialName]
	public object Call(CodeContext context, params object[] args)
	{
		try
		{
			PythonOps.FunctionPushFrame(PythonContext.GetContext(context));
			if (TryGetBoundCustomMember(context, "__call__", out var value))
			{
				return PythonOps.CallWithContext(context, value, args);
			}
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
		throw PythonOps.AttributeError("{0} instance has no __call__ method", _class.Name);
	}

	[SpecialName]
	public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
		try
		{
			PythonOps.FunctionPushFrame(PythonContext.GetContext(context));
			if (TryGetBoundCustomMember(context, "__call__", out var value))
			{
				return context.LanguageContext.CallWithKeywords(value, args, dict);
			}
		}
		finally
		{
			PythonOps.FunctionPopFrame();
		}
		throw PythonOps.AttributeError("{0} instance has no __call__ method", _class.Name);
	}

	public object __nonzero__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__nonzero__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		if (TryGetBoundCustomMember(context, "__len__", out value))
		{
			value = PythonOps.CallWithContext(context, value);
			if (value is int || value is BigInteger)
			{
				return ScriptingRuntimeHelpers.BooleanToObject(Converter.ConvertToBoolean(value));
			}
			throw PythonOps.TypeError("an integer is required, got {0}", PythonTypeOps.GetName(value));
		}
		return ScriptingRuntimeHelpers.True;
	}

	public object __hex__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__hex__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__hex__");
	}

	public object __oct__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__oct__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		throw PythonOps.AttributeErrorForOldInstanceMissingAttribute(_class.Name, "__oct__");
	}

	public object __int__(CodeContext context)
	{
		if (PythonOps.TryGetBoundAttr(context, this, "__int__", out var ret))
		{
			return PythonOps.CallWithContext(context, ret);
		}
		return NotImplementedType.Value;
	}

	public object __long__(CodeContext context)
	{
		if (PythonOps.TryGetBoundAttr(context, this, "__long__", out var ret))
		{
			return PythonOps.CallWithContext(context, ret);
		}
		return NotImplementedType.Value;
	}

	public object __float__(CodeContext context)
	{
		if (PythonOps.TryGetBoundAttr(context, this, "__float__", out var ret))
		{
			return PythonOps.CallWithContext(context, ret);
		}
		return NotImplementedType.Value;
	}

	public object __complex__(CodeContext context)
	{
		if (TryGetBoundCustomMember(context, "__complex__", out var value))
		{
			return PythonOps.CallWithContext(context, value);
		}
		return NotImplementedType.Value;
	}

	public object __getattribute__(CodeContext context, string name)
	{
		if (TryGetBoundCustomMember(context, name, out var value))
		{
			return value;
		}
		throw PythonOps.AttributeError("{0} instance has no attribute '{1}'", _class._name, name);
	}

	internal object GetBoundMember(CodeContext context, string name)
	{
		if (TryGetBoundCustomMember(context, name, out var value))
		{
			return value;
		}
		throw PythonOps.AttributeError("'{0}' object has no attribute '{1}'", PythonTypeOps.GetName(this), name);
	}

	internal bool TryGetBoundCustomMember(CodeContext context, string name, out object value)
	{
		if (name == "__dict__")
		{
			value = _dict;
			return true;
		}
		if (name == "__class__")
		{
			value = _class;
			return true;
		}
		if (TryRawGetAttr(context, name, out value))
		{
			return true;
		}
		if (name != "__getattr__" && TryRawGetAttr(context, "__getattr__", out var ret))
		{
			try
			{
				value = PythonCalls.Call(context, ret, name);
				return true;
			}
			catch (MissingMemberException)
			{
			}
		}
		return false;
	}

	internal void SetCustomMember(CodeContext context, string name, object value)
	{
		object ret;
		if (name == "__class__")
		{
			SetClass(value);
		}
		else if (name == "__dict__")
		{
			SetDict(context, value);
		}
		else if (_class.HasSetAttr && _class.TryLookupSlot("__setattr__", out ret))
		{
			PythonCalls.Call(context, _class.GetOldStyleDescriptor(context, ret, this, _class), name.ToString(), value);
		}
		else if (name == "__del__")
		{
			SetFinalizer(context, name, value);
		}
		else
		{
			_dict[name] = value;
		}
	}

	private void SetFinalizer(CodeContext context, string name, object value)
	{
		if (!HasFinalizer())
		{
			AddFinalizer(context);
		}
		_dict[name] = value;
	}

	private void SetDict(CodeContext context, object value)
	{
		if (!(value is PythonDictionary pythonDictionary))
		{
			throw PythonOps.TypeError("__dict__ must be set to a dictionary");
		}
		if (HasFinalizer() && !_class.HasFinalizer)
		{
			if (!pythonDictionary.ContainsKey("__del__"))
			{
				ClearFinalizer();
			}
		}
		else if (pythonDictionary.ContainsKey("__del__"))
		{
			AddFinalizer(context);
		}
		_dict = pythonDictionary;
	}

	private void SetClass(object value)
	{
		if (!(value is OldClass oldClass))
		{
			throw PythonOps.TypeError("__class__ must be set to class");
		}
		_class = oldClass;
	}

	internal bool DeleteCustomMember(CodeContext context, string name)
	{
		if (name == "__class__")
		{
			throw PythonOps.TypeError("__class__ must be set to class");
		}
		if (name == "__dict__")
		{
			throw PythonOps.TypeError("__dict__ must be set to a dictionary");
		}
		if (_class.HasDelAttr && _class.TryLookupSlot("__delattr__", out var ret))
		{
			PythonCalls.Call(context, _class.GetOldStyleDescriptor(context, ret, this, _class), name.ToString());
			return true;
		}
		if (name == "__del__" && HasFinalizer() && !_class.HasFinalizer)
		{
			ClearFinalizer();
		}
		if (!_dict.Remove(name))
		{
			throw PythonOps.AttributeError("{0} is not a valid attribute", name);
		}
		return true;
	}

	IList<string> IMembersList.GetMemberNames()
	{
		return PythonOps.GetStringMemberList(this);
	}

	IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
	{
		PythonDictionary pythonDictionary = new PythonDictionary(_dict);
		OldClass.RecurseAttrHierarchy(_class, pythonDictionary);
		return PythonOps.MakeListFromSequence(pythonDictionary);
	}

	[return: MaybeNotImplemented]
	public object __cmp__(CodeContext context, object other)
	{
		OldInstance oldInstance = other as OldInstance;
		object obj = InternalCompare("__cmp__", other);
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (oldInstance != null)
		{
			obj = oldInstance.InternalCompare("__cmp__", this);
			if (obj != NotImplementedType.Value)
			{
				return (int)obj * -1;
			}
		}
		return NotImplementedType.Value;
	}

	private object CompareForwardReverse(object other, string forward, string reverse)
	{
		object obj = InternalCompare(forward, other);
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance oldInstance)
		{
			return oldInstance.InternalCompare(reverse, this);
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator >([NotNull] OldInstance self, object other)
	{
		return self.CompareForwardReverse(other, "__gt__", "__lt__");
	}

	[return: MaybeNotImplemented]
	public static object operator <([NotNull] OldInstance self, object other)
	{
		return self.CompareForwardReverse(other, "__lt__", "__gt__");
	}

	[return: MaybeNotImplemented]
	public static object operator >=([NotNull] OldInstance self, object other)
	{
		return self.CompareForwardReverse(other, "__ge__", "__le__");
	}

	[return: MaybeNotImplemented]
	public static object operator <=([NotNull] OldInstance self, object other)
	{
		return self.CompareForwardReverse(other, "__le__", "__ge__");
	}

	private object InternalCompare(string cmp, object other)
	{
		return InvokeOne(this, other, cmp);
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return CustomTypeDescHelpers.GetAttributes(this);
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return CustomTypeDescHelpers.GetClassName(this);
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return CustomTypeDescHelpers.GetComponentName(this);
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return CustomTypeDescHelpers.GetConverter(this);
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return CustomTypeDescHelpers.GetDefaultEvent(this);
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return CustomTypeDescHelpers.GetDefaultProperty(this);
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return CustomTypeDescHelpers.GetEditor(this, editorBaseType);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return CustomTypeDescHelpers.GetEvents(this, attributes);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return CustomTypeDescHelpers.GetEvents(this);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return CustomTypeDescHelpers.GetProperties(this, attributes);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return CustomTypeDescHelpers.GetProperties(this);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return CustomTypeDescHelpers.GetPropertyOwner(this, pd);
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		return _weakRef;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		_weakRef = value;
		return true;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		((IWeakReferenceable)this).SetWeakRef(value);
	}

	public int __hash__(CodeContext context)
	{
		object obj = InvokeOne(this, "__hash__");
		if (obj != NotImplementedType.Value)
		{
			if (obj is BigInteger)
			{
				return BigIntegerOps.__hash__((BigInteger)obj);
			}
			if (!(obj is int))
			{
				throw PythonOps.TypeError("expected int from __hash__, got {0}", PythonTypeOps.GetName(obj));
			}
			return (int)obj;
		}
		if (TryGetBoundCustomMember(context, "__cmp__", out var value) || TryGetBoundCustomMember(context, "__eq__", out value))
		{
			throw PythonOps.TypeError("unhashable instance");
		}
		return base.GetHashCode();
	}

	public override int GetHashCode()
	{
		object obj;
		try
		{
			obj = InvokeOne(this, "__hash__");
		}
		catch
		{
			return base.GetHashCode();
		}
		if (obj != NotImplementedType.Value)
		{
			if (obj is int)
			{
				return (int)obj;
			}
			if (obj is BigInteger)
			{
				return BigIntegerOps.__hash__((BigInteger)obj);
			}
		}
		return base.GetHashCode();
	}

	[return: MaybeNotImplemented]
	public object __eq__(object other)
	{
		object obj = InvokeBoth(other, "__eq__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		return NotImplementedType.Value;
	}

	private object InvokeBoth(object other, string si)
	{
		object obj = InvokeOne(this, other, si);
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self)
		{
			obj = InvokeOne(self, this, si);
			if (obj != NotImplementedType.Value)
			{
				return obj;
			}
		}
		return NotImplementedType.Value;
	}

	private static object InvokeOne(OldInstance self, object other, string si)
	{
		object value;
		try
		{
			if (!self.TryGetBoundCustomMember(DefaultContext.Default, si, out value))
			{
				return NotImplementedType.Value;
			}
		}
		catch (MissingMemberException)
		{
			return NotImplementedType.Value;
		}
		return PythonOps.CallWithContext(DefaultContext.Default, value, other);
	}

	private static object InvokeOne(OldInstance self, object other, object other2, string si)
	{
		object value;
		try
		{
			if (!self.TryGetBoundCustomMember(DefaultContext.Default, si, out value))
			{
				return NotImplementedType.Value;
			}
		}
		catch (MissingMemberException)
		{
			return NotImplementedType.Value;
		}
		return PythonOps.CallWithContext(DefaultContext.Default, value, other, other2);
	}

	private static object InvokeOne(OldInstance self, string si)
	{
		object value;
		try
		{
			if (!self.TryGetBoundCustomMember(DefaultContext.Default, si, out value))
			{
				return NotImplementedType.Value;
			}
		}
		catch (MissingMemberException)
		{
			return NotImplementedType.Value;
		}
		return PythonOps.CallWithContext(DefaultContext.Default, value);
	}

	[return: MaybeNotImplemented]
	public object __ne__(object other)
	{
		object obj = InvokeBoth(other, "__ne__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object Power([NotNull] OldInstance self, object other, object mod)
	{
		object obj = InvokeOne(self, other, mod, "__pow__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		return NotImplementedType.Value;
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("__class__", _class);
		info.AddValue("__dict__", _dict);
	}

	private void RecurseAttrHierarchyInt(OldClass oc, IDictionary<string, object> attrs)
	{
		foreach (KeyValuePair<object, object> item in oc._dict._storage.GetItems())
		{
			if (item.Key is string text && !attrs.ContainsKey(text))
			{
				attrs.Add(text, text);
			}
		}
		if (oc.BaseClasses.Count == 0)
		{
			return;
		}
		foreach (OldClass baseClass in oc.BaseClasses)
		{
			RecurseAttrHierarchyInt(baseClass, attrs);
		}
	}

	private void AddFinalizer(CodeContext context)
	{
		InstanceFinalizer instanceFinalizer = new InstanceFinalizer(context, this);
		_weakRef = new WeakRefTracker(instanceFinalizer, instanceFinalizer);
	}

	private void ClearFinalizer()
	{
		if (_weakRef == null)
		{
			return;
		}
		WeakRefTracker weakRef = _weakRef;
		if (weakRef == null)
		{
			return;
		}
		for (int i = 0; i < weakRef.HandlerCount; i++)
		{
			if (weakRef.GetHandlerCallback(i) is InstanceFinalizer)
			{
				weakRef.RemoveHandlerAt(i);
				break;
			}
		}
		if (weakRef.HandlerCount == 0)
		{
			GC.SuppressFinalize(weakRef);
			_weakRef = null;
		}
	}

	private bool HasFinalizer()
	{
		if (_weakRef != null)
		{
			WeakRefTracker weakRef = _weakRef;
			if (weakRef != null)
			{
				for (int i = 0; i < weakRef.HandlerCount; i++)
				{
					if (weakRef.GetHandlerCallback(i) is InstanceFinalizer)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool TryRawGetAttr(CodeContext context, string name, out object ret)
	{
		if (_dict._storage.TryGetValue(name, out ret))
		{
			return true;
		}
		if (_class.TryLookupSlot(name, out ret))
		{
			ret = _class.GetOldStyleDescriptor(context, ret, this, _class);
			return true;
		}
		return false;
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new MetaOldInstance(parameter, BindingRestrictions.Empty, this);
	}

	T IFastGettable.MakeGetBinding<T>(CallSite<T> site, PythonGetMemberBinder binder, CodeContext state, string name)
	{
		if (binder.IsNoThrow)
		{
			return (T)(object)new Func<CallSite, object, CodeContext, object>(new FastOldInstanceGet(name).NoThrowTarget);
		}
		if (binder.SupportsLightThrow)
		{
			return (T)(object)new Func<CallSite, object, CodeContext, object>(new FastOldInstanceGet(name).LightThrowTarget);
		}
		return (T)(object)new Func<CallSite, object, CodeContext, object>(new FastOldInstanceGet(name).Target);
	}

	[return: MaybeNotImplemented]
	public static object operator +([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__add__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__radd__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator +(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__radd__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceAdd(object other)
	{
		return InvokeOne(this, other, "__iadd__");
	}

	[return: MaybeNotImplemented]
	public static object operator -([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__sub__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rsub__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator -(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rsub__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceSubtract(object other)
	{
		return InvokeOne(this, other, "__isub__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object Power([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__pow__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rpow__");
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object Power(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rpow__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlacePower(object other)
	{
		return InvokeOne(this, other, "__ipow__");
	}

	[return: MaybeNotImplemented]
	public static object operator *([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__mul__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rmul__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator *(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rmul__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceMultiply(object other)
	{
		return InvokeOne(this, other, "__imul__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object FloorDivide([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__floordiv__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rfloordiv__");
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object FloorDivide(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rfloordiv__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceFloorDivide(object other)
	{
		return InvokeOne(this, other, "__ifloordiv__");
	}

	[return: MaybeNotImplemented]
	public static object operator /([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__div__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rdiv__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator /(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rdiv__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceDivide(object other)
	{
		return InvokeOne(this, other, "__idiv__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object TrueDivide([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__truediv__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rtruediv__");
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object TrueDivide(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rtruediv__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceTrueDivide(object other)
	{
		return InvokeOne(this, other, "__itruediv__");
	}

	[return: MaybeNotImplemented]
	public static object operator %([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__mod__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rmod__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator %(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rmod__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceMod(object other)
	{
		return InvokeOne(this, other, "__imod__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object LeftShift([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__lshift__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rlshift__");
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object LeftShift(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rlshift__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceLeftShift(object other)
	{
		return InvokeOne(this, other, "__ilshift__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object RightShift([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__rshift__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rrshift__");
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object RightShift(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rrshift__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceRightShift(object other)
	{
		return InvokeOne(this, other, "__irshift__");
	}

	[return: MaybeNotImplemented]
	public static object operator &([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__and__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rand__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator &(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rand__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceBitwiseAnd(object other)
	{
		return InvokeOne(this, other, "__iand__");
	}

	[return: MaybeNotImplemented]
	public static object operator |([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__or__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__ror__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator |(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__ror__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceBitwiseOr(object other)
	{
		return InvokeOne(this, other, "__ior__");
	}

	[return: MaybeNotImplemented]
	public static object operator ^([NotNull] OldInstance self, object other)
	{
		object obj = InvokeOne(self, other, "__xor__");
		if (obj != NotImplementedType.Value)
		{
			return obj;
		}
		if (other is OldInstance self2)
		{
			return InvokeOne(self2, self, "__rxor__");
		}
		return NotImplementedType.Value;
	}

	[return: MaybeNotImplemented]
	public static object operator ^(object other, [NotNull] OldInstance self)
	{
		return InvokeOne(self, other, "__rxor__");
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public object InPlaceExclusiveOr(object other)
	{
		return InvokeOne(this, other, "__ixor__");
	}
}
