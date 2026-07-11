using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class ObjectOps
{
	private static Dictionary<PythonType, object> _nativelyPickleableTypes;

	[SlotField]
	public static PythonTypeSlot __class__ = new PythonTypeTypeSlot();

	private static Dictionary<PythonType, object> NativelyPickleableTypes
	{
		get
		{
			if (_nativelyPickleableTypes == null)
			{
				Dictionary<PythonType, object> dictionary = new Dictionary<PythonType, object>();
				dictionary.Add(TypeCache.Null, null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(bool)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(int)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(double)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(Complex)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(string)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(List)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(OldInstance)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(OldClass)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(PythonFunction)), null);
				dictionary.Add(DynamicHelpers.GetPythonTypeFromType(typeof(BuiltinFunction)), null);
				Thread.MemoryBarrier();
				_nativelyPickleableTypes = dictionary;
			}
			return _nativelyPickleableTypes;
		}
	}

	public static void __delattr__(CodeContext context, object self, string name)
	{
		if (self is PythonType)
		{
			throw PythonOps.TypeError("can't apply this __delattr__ to type object");
		}
		PythonOps.ObjectDeleteAttribute(context, self, name);
	}

	public static int __hash__(object self)
	{
		return self?.GetHashCode() ?? 505032256;
	}

	public static object __getattribute__(CodeContext context, object self, string name)
	{
		return PythonOps.ObjectGetAttribute(context, self, name);
	}

	public static void __init__(CodeContext context, object self)
	{
	}

	public static void __init__(CodeContext context, object self, [NotNull] params object[] argsø)
	{
		InstanceOps.CheckInitArgs(context, null, argsø, self);
	}

	public static void __init__(CodeContext context, object self, [ParamDictionary] IDictionary<object, object> kwargs, params object[] argsø)
	{
		InstanceOps.CheckInitArgs(context, kwargs, argsø, self);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(cls)));
		}
		return cls.CreateInstance(context);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [NotNull] params object[] argsø)
	{
		if (cls == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(cls)));
		}
		InstanceOps.CheckNewArgs(context, null, argsø, cls);
		return cls.CreateInstance(context);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [ParamDictionary] IDictionary<object, object> kwargsø, [NotNull] params object[] argsø)
	{
		if (cls == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(cls)));
		}
		InstanceOps.CheckNewArgs(context, kwargsø, argsø, cls);
		return cls.CreateInstance(context);
	}

	public static object __reduce__(CodeContext context, object self)
	{
		return __reduce_ex__(context, self, 0);
	}

	public static object __reduce_ex__(CodeContext context, object self)
	{
		return __reduce_ex__(context, self, 0);
	}

	public static object __reduce_ex__(CodeContext context, object self, object protocol)
	{
		object boundAttr = PythonOps.GetBoundAttr(context, DynamicHelpers.GetPythonTypeFromType(typeof(object)), "__reduce__");
		if (PythonOps.TryGetBoundAttr(context, DynamicHelpers.GetPythonType(self), "__reduce__", out var ret) && !PythonOps.IsRetBool(ret, boundAttr))
		{
			return PythonOps.CallWithContext(context, ret, self);
		}
		if (PythonContext.GetContext(context).ConvertToInt32(protocol) < 2)
		{
			return ReduceProtocol0(context, self);
		}
		return ReduceProtocol2(context, self);
	}

	public static string __repr__(object self)
	{
		return $"<{DynamicHelpers.GetPythonType(self).Name} object at {PythonOps.HexId(self)}>";
	}

	public static void __setattr__(CodeContext context, object self, string name, object value)
	{
		if (self is PythonType)
		{
			throw PythonOps.TypeError("can't apply this __setattr__ to type object");
		}
		PythonOps.ObjectSetAttribute(context, self, name, value);
	}

	private static int AdjustPointerSize(int size)
	{
		if (IntPtr.Size == 4)
		{
			return size;
		}
		return size * 2;
	}

	public static int __sizeof__(object self)
	{
		IPythonObject pythonObject = self as IPythonObject;
		int num = AdjustPointerSize(8);
		if (pythonObject != null)
		{
			num += AdjustPointerSize(12);
		}
		Type finalSystemType = DynamicHelpers.GetPythonType(self).FinalSystemType;
		return num + GetTypeSize(finalSystemType);
	}

	private static int GetTypeSize(Type t)
	{
		FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		int num = 0;
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo.FieldType.IsClass || fieldInfo.FieldType.IsInterface)
			{
				num += AdjustPointerSize(4);
				continue;
			}
			if (fieldInfo.FieldType.IsPrimitive)
			{
				return Marshal.SizeOf(fieldInfo.FieldType);
			}
			num += GetTypeSize(fieldInfo.FieldType);
		}
		return num;
	}

	public static string __str__(CodeContext context, object o)
	{
		return PythonOps.Repr(context, o);
	}

	public static NotImplementedType __subclasshook__(params object[] args)
	{
		return NotImplementedType.Value;
	}

	public static string __format__(CodeContext context, object self, [NotNull] string formatSpec)
	{
		string text = PythonOps.ToString(context, self);
		StringFormatSpec stringFormatSpec = StringFormatSpec.FromString(formatSpec);
		if (((int?)stringFormatSpec.Type).HasValue && stringFormatSpec.Type != 's')
		{
			throw PythonOps.ValueError("Unknown format code '{0}' for object of type 'str'", stringFormatSpec.Type.Value.ToString());
		}
		if (((int?)stringFormatSpec.Sign).HasValue)
		{
			throw PythonOps.ValueError("Sign not allowed in string format specifier");
		}
		if (stringFormatSpec.Alignment == '=')
		{
			throw PythonOps.ValueError("'=' alignment not allowed in string format specifier");
		}
		if (stringFormatSpec.ThousandsComma)
		{
			throw PythonOps.ValueError("Cannot specify ',' with 's'.");
		}
		if (stringFormatSpec.Precision.HasValue)
		{
			int value = stringFormatSpec.Precision.Value;
			if (text.Length > value)
			{
				text = text.Substring(0, value);
			}
		}
		return stringFormatSpec.AlignText(text);
	}

	private static PythonDictionary GetInitializedSlotValues(object obj)
	{
		PythonDictionary pythonDictionary = new PythonDictionary();
		IList<PythonType> resolutionOrder = DynamicHelpers.GetPythonType(obj).ResolutionOrder;
		foreach (PythonType item in resolutionOrder)
		{
			if (!PythonOps.TryGetBoundAttr(item, "__slots__", out var ret))
			{
				continue;
			}
			List<string> list = PythonType.SlotsToList(ret);
			foreach (string item2 in list)
			{
				if (!(item2 == "__dict__") && !pythonDictionary.__contains__(item2) && PythonOps.TryGetBoundAttr(obj, item2, out var ret2))
				{
					pythonDictionary[item2] = ret2;
				}
			}
		}
		if (pythonDictionary.Count == 0)
		{
			return null;
		}
		return pythonDictionary;
	}

	internal static PythonTuple ReduceProtocol0(CodeContext context, object self)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(self);
		ThrowIfNativelyPickable(pythonType);
		object ret;
		bool flag = PythonOps.TryGetBoundAttr(context, self, "__getstate__", out ret);
		if (PythonOps.TryGetBoundAttr(context, pythonType, "__slots__", out var ret2) && PythonOps.Length(ret2) > 0 && !flag)
		{
			throw PythonOps.TypeError("a class that defines __slots__ without defining __getstate__ cannot be pickled with protocols 0 or 1");
		}
		PythonType pythonType2 = FindClosestNonPythonBase(pythonType);
		object pythonReconstructor = PythonContext.GetContext(context).PythonReconstructor;
		object obj = PythonTuple.MakeTuple(pythonType, pythonType2, (TypeCache.Object == pythonType2) ? null : PythonCalls.Call(context, pythonType2, self));
		object ret3;
		if (flag)
		{
			ret3 = PythonOps.CallWithContext(context, ret);
		}
		else if (self is IPythonObject pythonObject)
		{
			ret3 = pythonObject.Dict;
		}
		else if (!PythonOps.TryGetBoundAttr(context, self, "__dict__", out ret3))
		{
			ret3 = null;
		}
		if (!PythonOps.IsTrue(ret3))
		{
			ret3 = null;
		}
		return PythonTuple.MakeTuple(pythonReconstructor, obj, ret3);
	}

	private static void ThrowIfNativelyPickable(PythonType type)
	{
		if (NativelyPickleableTypes.ContainsKey(type))
		{
			throw PythonOps.TypeError("can't pickle {0} objects", type.Name);
		}
	}

	private static PythonType FindClosestNonPythonBase(PythonType type)
	{
		foreach (PythonType item in type.ResolutionOrder)
		{
			if (item.IsSystemType)
			{
				return item;
			}
		}
		throw PythonOps.TypeError("can't pickle {0} instance: no non-Python bases found", type.Name);
	}

	private static PythonTuple ReduceProtocol2(CodeContext context, object self)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(self);
		object newObject = PythonContext.GetContext(context).NewObject;
		object[] array;
		if (PythonOps.TryGetBoundAttr(context, pythonType, "__getnewargs__", out var ret))
		{
			if (!(PythonOps.CallWithContext(context, ret, self) is PythonTuple pythonTuple))
			{
				throw PythonOps.TypeError("__getnewargs__ should return a tuple");
			}
			array = new object[1 + pythonTuple.Count];
			array[0] = pythonType;
			for (int i = 0; i < pythonTuple.Count; i++)
			{
				array[i + 1] = pythonTuple[i];
			}
		}
		else
		{
			array = new object[1] { pythonType };
		}
		if (!PythonTypeOps.TryInvokeUnaryOperator(context, self, "__getstate__", out var value))
		{
			object ret2;
			if (self is IPythonObject pythonObject)
			{
				ret2 = pythonObject.Dict;
			}
			else if (!PythonOps.TryGetBoundAttr(context, self, "__dict__", out ret2))
			{
				ret2 = null;
			}
			PythonDictionary pythonDictionary = GetInitializedSlotValues(self);
			if (pythonDictionary != null && pythonDictionary.Count == 0)
			{
				pythonDictionary = null;
			}
			value = ((ret2 == null && pythonDictionary == null) ? null : ((ret2 != null && pythonDictionary == null) ? ret2 : ((ret2 == null || pythonDictionary == null) ? PythonTuple.MakeTuple(null, pythonDictionary) : PythonTuple.MakeTuple(ret2, pythonDictionary))));
		}
		object obj = null;
		if (self is List)
		{
			obj = PythonOps.GetEnumerator(self);
		}
		object obj2 = null;
		if (self is PythonDictionary || self is PythonDictionary)
		{
			obj2 = PythonOps.Invoke(context, self, "iteritems", ArrayUtils.EmptyObjects);
		}
		return PythonTuple.MakeTuple(newObject, PythonTuple.MakeTuple(array), value, obj, obj2);
	}
}
