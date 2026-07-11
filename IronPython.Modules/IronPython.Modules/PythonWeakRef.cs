using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonWeakRef
{
	[PythonType]
	public class @ref : IStructuralEquatable
	{
		private WeakHandle _target;

		private int _hashVal;

		private bool _fHasHash;

		public static object __new__(CodeContext context, PythonType cls, object @object)
		{
			IWeakReferenceable weakReferenceable = ConvertToWeakReferenceable(@object);
			if (cls == DynamicHelpers.GetPythonTypeFromType(typeof(@ref)))
			{
				WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
				if (weakRef != null)
				{
					for (int i = 0; i < weakRef.HandlerCount; i++)
					{
						if (weakRef.GetHandlerCallback(i) == null && weakRef.GetWeakRef(i) is @ref)
						{
							return weakRef.GetWeakRef(i);
						}
					}
				}
				return new @ref(@object);
			}
			return cls.CreateInstance(context, @object);
		}

		public static object __new__(CodeContext context, PythonType cls, object @object, object callback)
		{
			if (callback == null)
			{
				return __new__(context, cls, @object);
			}
			if (cls == DynamicHelpers.GetPythonTypeFromType(typeof(@ref)))
			{
				return new @ref(@object, callback);
			}
			return cls.CreateInstance(context, @object, callback);
		}

		public @ref(object @object)
			: this(@object, null)
		{
		}

		public @ref(object @object, object callback)
		{
			WeakRefHelpers.InitializeWeakRef(this, @object, callback);
			_target = new WeakHandle(@object, trackResurrection: false);
		}

		~@ref()
		{
			try
			{
				if (_target.IsAlive)
				{
					if (_target.Target is IWeakReferenceable weakReferenceable)
					{
						weakReferenceable.GetWeakRef()?.RemoveHandler(this);
					}
					_target.Free();
				}
			}
			catch (InvalidOperationException)
			{
			}
		}

		internal static int GetWeakRefCount(object o)
		{
			if (o is IWeakReferenceable weakReferenceable)
			{
				WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
				if (weakRef != null)
				{
					return weakRef.HandlerCount;
				}
			}
			return 0;
		}

		internal static List GetWeakRefs(object o)
		{
			List list = new List();
			if (o is IWeakReferenceable weakReferenceable)
			{
				WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
				if (weakRef != null)
				{
					for (int i = 0; i < weakRef.HandlerCount; i++)
					{
						list.AddNoLock(weakRef.GetWeakRef(i));
					}
				}
			}
			return list;
		}

		[SpecialName]
		public object Call(CodeContext context)
		{
			if (!_target.IsAlive)
			{
				throw PythonOps.ReferenceError("weak object has gone away");
			}
			try
			{
				object target = _target.Target;
				GC.KeepAlive(this);
				return target;
			}
			catch (InvalidOperationException)
			{
				throw PythonOps.ReferenceError("weak object has gone away");
			}
		}

		[return: MaybeNotImplemented]
		public static NotImplementedType operator >(@ref self, object other)
		{
			return PythonOps.NotImplemented;
		}

		[return: MaybeNotImplemented]
		public static NotImplementedType operator <(@ref self, object other)
		{
			return PythonOps.NotImplemented;
		}

		[return: MaybeNotImplemented]
		public static NotImplementedType operator <=(@ref self, object other)
		{
			return PythonOps.NotImplemented;
		}

		[return: MaybeNotImplemented]
		public static NotImplementedType operator >=(@ref self, object other)
		{
			return PythonOps.NotImplemented;
		}

		public int __hash__(CodeContext context)
		{
			if (!_fHasHash)
			{
				object target = _target.Target;
				if (target == null)
				{
					throw PythonOps.TypeError("weak object has gone away");
				}
				GC.KeepAlive(this);
				_hashVal = PythonContext.GetContext(context).EqualityComparerNonGeneric.GetHashCode(target);
				_fHasHash = true;
			}
			return _hashVal;
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			if (!_fHasHash)
			{
				object target = _target.Target;
				GC.KeepAlive(this);
				_hashVal = comparer.GetHashCode(target);
				_fHasHash = true;
			}
			return _hashVal;
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			return EqualsWorker(other, comparer);
		}

		private bool EqualsWorker(object other, IEqualityComparer comparer)
		{
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			bool result = false;
			if (other is @ref obj)
			{
				object target = _target.Target;
				object target2 = obj._target.Target;
				GC.KeepAlive(this);
				GC.KeepAlive(obj);
				if (target != null && target2 != null)
				{
					result = RefEquals(target, target2, comparer);
				}
			}
			GC.KeepAlive(this);
			return result;
		}

		private static bool RefEquals(object x, object y, IEqualityComparer comparer)
		{
			CodeContext context = ((comparer == null || !(comparer is PythonContext.PythonEqualityComparer)) ? DefaultContext.Default : ((PythonContext.PythonEqualityComparer)comparer).Context.SharedContext);
			if (PythonTypeOps.TryInvokeBinaryOperator(context, x, y, "__eq__", out var value) && value != NotImplementedType.Value)
			{
				return (bool)value;
			}
			if (PythonTypeOps.TryInvokeBinaryOperator(context, y, x, "__eq__", out value) && value != NotImplementedType.Value)
			{
				return (bool)value;
			}
			return comparer?.Equals(x, y) ?? x.Equals(y);
		}
	}

	[PythonType]
	[PythonHidden]
	[DynamicBaseType]
	public sealed class weakproxy : IPythonObject, ICodeFormattable, IProxyObject, IPythonMembersList, IMembersList, IStructuralEquatable
	{
		public const object __hash__ = null;

		private readonly WeakHandle _target;

		private readonly CodeContext _context;

		[SlotField]
		public static PythonTypeSlot __add__ = new SlotWrapper("__add__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __radd__ = new SlotWrapper("__radd__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __iadd__ = new SlotWrapper("__iadd__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __sub__ = new SlotWrapper("__sub__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rsub__ = new SlotWrapper("__rsub__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __isub__ = new SlotWrapper("__isub__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __pow__ = new SlotWrapper("__pow__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rpow__ = new SlotWrapper("__rpow__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __ipow__ = new SlotWrapper("__ipow__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __mul__ = new SlotWrapper("__mul__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rmul__ = new SlotWrapper("__rmul__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __imul__ = new SlotWrapper("__imul__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __floordiv__ = new SlotWrapper("__floordiv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rfloordiv__ = new SlotWrapper("__rfloordiv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __ifloordiv__ = new SlotWrapper("__ifloordiv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __div__ = new SlotWrapper("__div__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rdiv__ = new SlotWrapper("__rdiv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __idiv__ = new SlotWrapper("__idiv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __truediv__ = new SlotWrapper("__truediv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rtruediv__ = new SlotWrapper("__rtruediv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __itruediv__ = new SlotWrapper("__itruediv__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __mod__ = new SlotWrapper("__mod__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rmod__ = new SlotWrapper("__rmod__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __imod__ = new SlotWrapper("__imod__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __lshift__ = new SlotWrapper("__lshift__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rlshift__ = new SlotWrapper("__rlshift__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __ilshift__ = new SlotWrapper("__ilshift__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rshift__ = new SlotWrapper("__rshift__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rrshift__ = new SlotWrapper("__rrshift__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __irshift__ = new SlotWrapper("__irshift__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __and__ = new SlotWrapper("__and__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rand__ = new SlotWrapper("__rand__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __iand__ = new SlotWrapper("__iand__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __or__ = new SlotWrapper("__or__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __ror__ = new SlotWrapper("__ror__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __ior__ = new SlotWrapper("__ior__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __xor__ = new SlotWrapper("__xor__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rxor__ = new SlotWrapper("__rxor__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __ixor__ = new SlotWrapper("__ixor__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __delslice__ = new SlotWrapper("__delslice__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __divmod__ = new SlotWrapper("__divmod__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __float__ = new SlotWrapper("__float__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __getslice__ = new SlotWrapper("__getslice__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __index__ = new SlotWrapper("__index__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __int__ = new SlotWrapper("__int__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __iter__ = new SlotWrapper("__iter__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __long__ = new SlotWrapper("__long__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __rdivmod__ = new SlotWrapper("__rdivmod__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __setslice__ = new SlotWrapper("__setslice__", ProxyType);

		[SlotField]
		public static PythonTypeSlot next = new SlotWrapper("next", ProxyType);

		[SlotField]
		public static PythonTypeSlot __getitem__ = new SlotWrapper("__getitem__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __setitem__ = new SlotWrapper("__setitem__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __delitem__ = new SlotWrapper("__delitem__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __len__ = new SlotWrapper("__len__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __pos__ = new SlotWrapper("__pos__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __neg__ = new SlotWrapper("__neg__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __invert__ = new SlotWrapper("__invert__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __contains__ = new SlotWrapper("__contains__", ProxyType);

		[SlotField]
		public static PythonTypeSlot __abs__ = new SlotWrapper("__abs__", ProxyType);

		PythonDictionary IPythonObject.Dict
		{
			get
			{
				if (GetObject() is IPythonObject pythonObject)
				{
					return pythonObject.Dict;
				}
				return null;
			}
		}

		PythonType IPythonObject.PythonType => DynamicHelpers.GetPythonTypeFromType(typeof(weakproxy));

		object IProxyObject.Target => GetObject();

		internal static object MakeNew(CodeContext context, object @object, object callback)
		{
			IWeakReferenceable weakReferenceable = ConvertToWeakReferenceable(@object);
			if (callback == null)
			{
				WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
				if (weakRef != null)
				{
					for (int i = 0; i < weakRef.HandlerCount; i++)
					{
						if (weakRef.GetHandlerCallback(i) == null && weakRef.GetWeakRef(i) is weakproxy)
						{
							return weakRef.GetWeakRef(i);
						}
					}
				}
			}
			return new weakproxy(context, @object, callback);
		}

		private weakproxy(CodeContext context, object target, object callback)
		{
			WeakRefHelpers.InitializeWeakRef(this, target, callback);
			_target = new WeakHandle(target, trackResurrection: false);
			_context = context;
		}

		~weakproxy()
		{
			try
			{
				if (_target.Target is IWeakReferenceable weakReferenceable)
				{
					WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
					weakRef.RemoveHandler(this);
				}
				_target.Free();
			}
			catch (InvalidOperationException)
			{
			}
		}

		private object GetObject()
		{
			if (!TryGetObject(out var result))
			{
				throw PythonOps.ReferenceError("weakly referenced object no longer exists");
			}
			return result;
		}

		private bool TryGetObject(out object result)
		{
			try
			{
				result = _target.Target;
				if (result == null)
				{
					return false;
				}
				GC.KeepAlive(this);
				return true;
			}
			catch (InvalidOperationException)
			{
				result = null;
				return false;
			}
		}

		PythonDictionary IPythonObject.SetDict(PythonDictionary dict)
		{
			return (GetObject() as IPythonObject).SetDict(dict);
		}

		bool IPythonObject.ReplaceDict(PythonDictionary dict)
		{
			return (GetObject() as IPythonObject).ReplaceDict(dict);
		}

		void IPythonObject.SetPythonType(PythonType newType)
		{
			(GetObject() as IPythonObject).SetPythonType(newType);
		}

		object[] IPythonObject.GetSlots()
		{
			return null;
		}

		object[] IPythonObject.GetSlotsCreate()
		{
			return null;
		}

		public override string ToString()
		{
			return PythonOps.ToString(GetObject());
		}

		public string __repr__(CodeContext context)
		{
			object target = _target.Target;
			GC.KeepAlive(this);
			return $"<weakproxy at {IdDispenser.GetId(this)} to {PythonOps.GetPythonTypeName(target)} at {IdDispenser.GetId(target)}>";
		}

		[SpecialName]
		public object GetCustomMember(CodeContext context, string name)
		{
			object o = GetObject();
			if (PythonOps.TryGetBoundAttr(context, o, name, out var ret))
			{
				return ret;
			}
			return OperationFailed.Value;
		}

		[SpecialName]
		public void SetMember(CodeContext context, string name, object value)
		{
			object o = GetObject();
			PythonOps.SetAttr(context, o, name, value);
		}

		[SpecialName]
		public void DeleteMember(CodeContext context, string name)
		{
			object o = GetObject();
			PythonOps.DeleteAttr(context, o, name);
		}

		IList<string> IMembersList.GetMemberNames()
		{
			return PythonOps.GetStringMemberList(this);
		}

		IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
		{
			if (!TryGetObject(out var result))
			{
				return new List();
			}
			return PythonOps.GetAttrNames(context, result);
		}

		private bool EqualsWorker(weakproxy other)
		{
			return PythonOps.EqualRetBool(_context, GetObject(), other.GetObject());
		}

		[return: MaybeNotImplemented]
		public object __eq__(object other)
		{
			if (!(other is weakproxy))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(EqualsWorker((weakproxy)other));
		}

		[return: MaybeNotImplemented]
		public object __ne__(object other)
		{
			if (!(other is weakproxy))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(!EqualsWorker((weakproxy)other));
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			if (TryGetObject(out var result))
			{
				return comparer.GetHashCode(result);
			}
			return comparer.GetHashCode(null);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!TryGetObject(out var result))
			{
				result = null;
			}
			if (other is weakproxy)
			{
				if (!TryGetObject(out var result2))
				{
					result2 = null;
				}
				return comparer.Equals(result, result2);
			}
			return comparer.Equals(result, other);
		}

		public object __nonzero__()
		{
			return Converter.ConvertToBoolean(GetObject());
		}

		public static explicit operator bool(weakproxy self)
		{
			return Converter.ConvertToBoolean(self.GetObject());
		}
	}

	[PythonType]
	[DynamicBaseType]
	[PythonHidden]
	public sealed class weakcallableproxy : IPythonObject, ICodeFormattable, IProxyObject, IStructuralEquatable, IPythonMembersList, IMembersList
	{
		public const object __hash__ = null;

		private WeakHandle _target;

		private readonly CodeContext _context;

		[SlotField]
		public static PythonTypeSlot __add__ = new SlotWrapper("__add__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __radd__ = new SlotWrapper("__radd__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __iadd__ = new SlotWrapper("__iadd__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __sub__ = new SlotWrapper("__sub__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rsub__ = new SlotWrapper("__rsub__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __isub__ = new SlotWrapper("__isub__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __pow__ = new SlotWrapper("__pow__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rpow__ = new SlotWrapper("__rpow__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __ipow__ = new SlotWrapper("__ipow__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __mul__ = new SlotWrapper("__mul__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rmul__ = new SlotWrapper("__rmul__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __imul__ = new SlotWrapper("__imul__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __floordiv__ = new SlotWrapper("__floordiv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rfloordiv__ = new SlotWrapper("__rfloordiv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __ifloordiv__ = new SlotWrapper("__ifloordiv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __div__ = new SlotWrapper("__div__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rdiv__ = new SlotWrapper("__rdiv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __idiv__ = new SlotWrapper("__idiv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __truediv__ = new SlotWrapper("__truediv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rtruediv__ = new SlotWrapper("__rtruediv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __itruediv__ = new SlotWrapper("__itruediv__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __mod__ = new SlotWrapper("__mod__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rmod__ = new SlotWrapper("__rmod__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __imod__ = new SlotWrapper("__imod__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __lshift__ = new SlotWrapper("__lshift__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rlshift__ = new SlotWrapper("__rlshift__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __ilshift__ = new SlotWrapper("__ilshift__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rshift__ = new SlotWrapper("__rshift__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rrshift__ = new SlotWrapper("__rrshift__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __irshift__ = new SlotWrapper("__irshift__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __and__ = new SlotWrapper("__and__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rand__ = new SlotWrapper("__rand__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __iand__ = new SlotWrapper("__iand__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __or__ = new SlotWrapper("__or__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __ror__ = new SlotWrapper("__ror__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __ior__ = new SlotWrapper("__ior__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __xor__ = new SlotWrapper("__xor__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rxor__ = new SlotWrapper("__rxor__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __ixor__ = new SlotWrapper("__ixor__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __delslice__ = new SlotWrapper("__delslice__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __divmod__ = new SlotWrapper("__divmod__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __float__ = new SlotWrapper("__float__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __getslice__ = new SlotWrapper("__getslice__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __index__ = new SlotWrapper("__index__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __int__ = new SlotWrapper("__int__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __iter__ = new SlotWrapper("__iter__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __long__ = new SlotWrapper("__long__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __rdivmod__ = new SlotWrapper("__rdivmod__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __setslice__ = new SlotWrapper("__setslice__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot next = new SlotWrapper("next", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __getitem__ = new SlotWrapper("__getitem__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __setitem__ = new SlotWrapper("__setitem__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __delitem__ = new SlotWrapper("__delitem__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __len__ = new SlotWrapper("__len__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __pos__ = new SlotWrapper("__pos__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __neg__ = new SlotWrapper("__neg__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __invert__ = new SlotWrapper("__invert__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __contains__ = new SlotWrapper("__contains__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __abs__ = new SlotWrapper("__abs__", CallableProxyType);

		[SlotField]
		public static PythonTypeSlot __call__ = new SlotWrapper("__call__", CallableProxyType);

		PythonDictionary IPythonObject.Dict => (GetObject() as IPythonObject).Dict;

		PythonType IPythonObject.PythonType => DynamicHelpers.GetPythonTypeFromType(typeof(weakcallableproxy));

		object IProxyObject.Target => GetObject();

		internal static object MakeNew(CodeContext context, object @object, object callback)
		{
			IWeakReferenceable weakReferenceable = ConvertToWeakReferenceable(@object);
			if (callback == null)
			{
				WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
				if (weakRef != null)
				{
					for (int i = 0; i < weakRef.HandlerCount; i++)
					{
						if (weakRef.GetHandlerCallback(i) == null && weakRef.GetWeakRef(i) is weakcallableproxy)
						{
							return weakRef.GetWeakRef(i);
						}
					}
				}
			}
			return new weakcallableproxy(context, @object, callback);
		}

		private weakcallableproxy(CodeContext context, object target, object callback)
		{
			WeakRefHelpers.InitializeWeakRef(this, target, callback);
			_target = new WeakHandle(target, trackResurrection: false);
			_context = context;
		}

		~weakcallableproxy()
		{
			try
			{
				if (_target.Target is IWeakReferenceable weakReferenceable)
				{
					WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
					weakRef.RemoveHandler(this);
				}
				_target.Free();
			}
			catch (InvalidOperationException)
			{
			}
		}

		private object GetObject()
		{
			if (!TryGetObject(out var result))
			{
				throw PythonOps.ReferenceError("weakly referenced object no longer exists");
			}
			return result;
		}

		private bool TryGetObject(out object result)
		{
			try
			{
				result = _target.Target;
				if (result == null)
				{
					return false;
				}
				GC.KeepAlive(this);
				return true;
			}
			catch (InvalidOperationException)
			{
				result = null;
				return false;
			}
		}

		PythonDictionary IPythonObject.SetDict(PythonDictionary dict)
		{
			return (GetObject() as IPythonObject).SetDict(dict);
		}

		bool IPythonObject.ReplaceDict(PythonDictionary dict)
		{
			return (GetObject() as IPythonObject).ReplaceDict(dict);
		}

		void IPythonObject.SetPythonType(PythonType newType)
		{
			(GetObject() as IPythonObject).SetPythonType(newType);
		}

		object[] IPythonObject.GetSlots()
		{
			return null;
		}

		object[] IPythonObject.GetSlotsCreate()
		{
			return null;
		}

		public override string ToString()
		{
			return PythonOps.ToString(GetObject());
		}

		public string __repr__(CodeContext context)
		{
			object target = _target.Target;
			GC.KeepAlive(this);
			return $"<weakproxy at {IdDispenser.GetId(this)} to {PythonOps.GetPythonTypeName(target)} at {IdDispenser.GetId(target)}>";
		}

		[SpecialName]
		public object Call(CodeContext context, params object[] args)
		{
			return PythonContext.GetContext(context).CallSplat(GetObject(), args);
		}

		[SpecialName]
		public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			return PythonCalls.CallWithKeywordArgs(context, GetObject(), args, dict);
		}

		[SpecialName]
		public object GetCustomMember(CodeContext context, string name)
		{
			object o = GetObject();
			if (PythonOps.TryGetBoundAttr(context, o, name, out var ret))
			{
				return ret;
			}
			return OperationFailed.Value;
		}

		[SpecialName]
		public void SetMember(CodeContext context, string name, object value)
		{
			object o = GetObject();
			PythonOps.SetAttr(context, o, name, value);
		}

		[SpecialName]
		public void DeleteMember(CodeContext context, string name)
		{
			object o = GetObject();
			PythonOps.DeleteAttr(context, o, name);
		}

		IList<string> IMembersList.GetMemberNames()
		{
			return PythonOps.GetStringMemberList(this);
		}

		IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
		{
			if (!TryGetObject(out var result))
			{
				return new List();
			}
			return PythonOps.GetAttrNames(context, result);
		}

		public bool __eq__(object other)
		{
			if (other is weakcallableproxy weakcallableproxy2)
			{
				return GetObject().Equals(weakcallableproxy2.GetObject());
			}
			return PythonOps.EqualRetBool(_context, GetObject(), other);
		}

		public bool __ne__(object other)
		{
			return !__eq__(other);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			if (TryGetObject(out var result))
			{
				return comparer.GetHashCode(result);
			}
			return comparer.GetHashCode(null);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!TryGetObject(out var result))
			{
				result = null;
			}
			if (other is weakcallableproxy)
			{
				if (!TryGetObject(out var result2))
				{
					result2 = null;
				}
				return comparer.Equals(result, result2);
			}
			return comparer.Equals(result, other);
		}

		public object __nonzero__()
		{
			return Converter.ConvertToBoolean(GetObject());
		}
	}

	private static class WeakRefHelpers
	{
		public static void InitializeWeakRef(object self, object target, object callback)
		{
			IWeakReferenceable weakReferenceable = ConvertToWeakReferenceable(target);
			WeakRefTracker weakRef = weakReferenceable.GetWeakRef();
			if (weakRef == null)
			{
				if (!weakReferenceable.SetWeakRef(new WeakRefTracker(callback, self)))
				{
					throw PythonOps.TypeError("cannot create weak reference to '{0}' object", PythonOps.GetPythonTypeName(target));
				}
			}
			else
			{
				weakRef.ChainCallback(callback, self);
			}
		}
	}

	public const string __doc__ = "Provides support for creating weak references and proxies to objects";

	public static readonly PythonType CallableProxyType = DynamicHelpers.GetPythonTypeFromType(typeof(weakcallableproxy));

	public static readonly PythonType ProxyType = DynamicHelpers.GetPythonTypeFromType(typeof(weakproxy));

	public static readonly PythonType ReferenceType = DynamicHelpers.GetPythonTypeFromType(typeof(@ref));

	internal static IWeakReferenceable ConvertToWeakReferenceable(object obj)
	{
		if (obj is IWeakReferenceable result)
		{
			return result;
		}
		throw PythonOps.TypeError("cannot create weak reference to '{0}' object", PythonOps.GetPythonTypeName(obj));
	}

	public static int getweakrefcount(object @object)
	{
		return @ref.GetWeakRefCount(@object);
	}

	public static List getweakrefs(object @object)
	{
		return @ref.GetWeakRefs(@object);
	}

	public static object proxy(CodeContext context, object @object)
	{
		return proxy(context, @object, null);
	}

	public static object proxy(CodeContext context, object @object, object callback)
	{
		if (PythonOps.IsCallable(context, @object))
		{
			return weakcallableproxy.MakeNew(context, @object, callback);
		}
		return weakproxy.MakeNew(context, @object, callback);
	}
}
