using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class InstanceOps
{
	internal const string ObjectNewNoParameters = "object.__new__() takes no parameters";

	private static BuiltinFunction _New;

	internal static readonly BuiltinFunction NewCls = CreateFunction("__new__", "DefaultNew", "DefaultNewClsKW");

	internal static readonly BuiltinFunction OverloadedNew = CreateFunction("__new__", "OverloadedNewBasic", "OverloadedNewKW", "OverloadedNewClsKW");

	internal static readonly BuiltinFunction NonDefaultNewInst = CreateNonDefaultNew();

	internal static BuiltinMethodDescriptor _Init;

	internal static BuiltinMethodDescriptor Init
	{
		get
		{
			if (_Init == null)
			{
				_Init = GetInitMethod();
			}
			return _Init;
		}
	}

	internal static BuiltinFunction New
	{
		get
		{
			if (_New == null)
			{
				_New = (BuiltinFunction)PythonTypeOps.GetSlot(IronPython.Runtime.Types.TypeInfo.GetExtensionMemberGroup(typeof(object), typeof(ObjectOps).GetMember("__new__")), "__new__", privateBinding: false);
			}
			return _New;
		}
	}

	internal static BuiltinFunction CreateNonDefaultNew()
	{
		return CreateFunction("__new__", "NonDefaultNew", "NonDefaultNewKW", "NonDefaultNewKWNoParams");
	}

	public static object DefaultNew(CodeContext context, PythonType typeø, params object[] argsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		CheckNewArgs(context, null, argsø, typeø);
		return typeø.CreateInstance(context);
	}

	public static object DefaultNewClsKW(CodeContext context, PythonType typeø, [ParamDictionary] IDictionary<object, object> kwargsø, params object[] argsø)
	{
		object obj = DefaultNew(context, typeø, argsø);
		if (kwargsø.Count > 0)
		{
			foreach (KeyValuePair<object, object> item in kwargsø)
			{
				PythonOps.SetAttr(context, obj, item.Key.ToString(), item.Value);
			}
		}
		return obj;
	}

	public static object OverloadedNewBasic(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, BuiltinFunction overloadsø, PythonType typeø, params object[] argsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		if (argsø == null)
		{
			argsø = new object[1];
		}
		return overloadsø.Call(context, storage, null, argsø);
	}

	public static object OverloadedNewKW(CodeContext context, BuiltinFunction overloadsø, PythonType typeø, [ParamDictionary] IDictionary<object, object> kwargsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		return overloadsø.Call(context, null, null, ArrayUtils.EmptyObjects, kwargsø);
	}

	public static object OverloadedNewClsKW(CodeContext context, BuiltinFunction overloadsø, PythonType typeø, [ParamDictionary] IDictionary<object, object> kwargsø, params object[] argsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		if (argsø == null)
		{
			argsø = new object[1];
		}
		return overloadsø.Call(context, null, null, argsø, kwargsø);
	}

	public static void DefaultInit(CodeContext context, object self, params object[] argsø)
	{
	}

	public static void DefaultInitKW(CodeContext context, object self, [ParamDictionary] IDictionary<object, object> kwargsø, params object[] argsø)
	{
	}

	[StaticExtensionMethod]
	public static object NonDefaultNew(CodeContext context, PythonType typeø, params object[] argsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		if (argsø == null)
		{
			argsø = new object[1];
		}
		return typeø.CreateInstance(context, argsø);
	}

	[StaticExtensionMethod]
	public static object NonDefaultNewKW(CodeContext context, PythonType typeø, [ParamDictionary] IDictionary<object, object> kwargsø, params object[] argsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		if (argsø == null)
		{
			argsø = new object[1];
		}
		GetKeywordArgs(kwargsø, argsø, out argsø, out var names);
		return typeø.CreateInstance(context, argsø, names);
	}

	[StaticExtensionMethod]
	public static object NonDefaultNewKWNoParams(CodeContext context, PythonType typeø, [ParamDictionary] IDictionary<object, object> kwargsø)
	{
		if (typeø == null)
		{
			throw PythonOps.TypeError("__new__ expected type object, got {0}", PythonOps.Repr(context, DynamicHelpers.GetPythonType(typeø)));
		}
		GetKeywordArgs(kwargsø, ArrayUtils.EmptyObjects, out var finalArgs, out var names);
		return typeø.CreateInstance(context, finalArgs, names);
	}

	public static object IterMethodForString(string self)
	{
		return PythonOps.StringEnumerator(self);
	}

	public static object IterMethodForBytes(Bytes self)
	{
		return PythonOps.BytesIntEnumerator(self);
	}

	public static object IterMethodForEnumerator(IEnumerator self)
	{
		return self;
	}

	public static object IterMethodForEnumerable(IEnumerable self)
	{
		return self.GetEnumerator();
	}

	public static object IterMethodForGenericEnumerator<T>(IEnumerator<T> self)
	{
		return self;
	}

	public static object IterMethodForGenericEnumerable<T>(IEnumerable<T> self)
	{
		return self.GetEnumerator();
	}

	public static object NextMethod(object self)
	{
		IEnumerator enumerator = (IEnumerator)self;
		if (enumerator.MoveNext())
		{
			return enumerator.Current;
		}
		throw PythonOps.StopIteration();
	}

	public static List DynamicDir(CodeContext context, IDynamicMetaObjectProvider self)
	{
		List list = new List(self.GetMetaObject(Expression.Parameter(typeof(object))).GetDynamicMemberNames());
		Type type = self.GetType();
		while (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
		{
			type = type.BaseType;
		}
		list.extend(DynamicHelpers.GetPythonTypeFromType(type).GetMemberNames(context));
		list.sort(context);
		return list;
	}

	public static int LengthMethod(ICollection self)
	{
		return self.Count;
	}

	public static int GenericLengthMethod<T>(ICollection<T> self)
	{
		return self.Count;
	}

	public static string SimpleRepr(object self)
	{
		return $"<{PythonTypeOps.GetName(self)} object at {PythonOps.HexId(self)}>";
	}

	public static string FancyRepr(object self)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(self);
		if (pythonType.IsSystemType)
		{
			string text = self.ToString();
			if (text == null)
			{
				text = string.Empty;
			}
			Type underlyingSystemType = pythonType.UnderlyingSystemType;
			string fullName = underlyingSystemType.FullName;
			int i;
			for (i = 0; i < text.Length && (text[i] == '\r' || text[i] == '\n'); i++)
			{
			}
			int j;
			for (j = i; j < text.Length && text[j] != '\r' && text[j] != '\n'; j++)
			{
			}
			int k;
			for (k = j; k < text.Length && (text[k] == '\r' || text[k] == '\n'); k++)
			{
			}
			if (j > i)
			{
				string text2 = text.Substring(i, j - i);
				bool flag = k < text.Length;
				return string.Format("<{0} object at {1} [{2}{3}]>", fullName, PythonOps.HexId(self), text2, flag ? "..." : string.Empty);
			}
			return $"<{fullName} object at {PythonOps.HexId(self)}>";
		}
		return SimpleRepr(self);
	}

	public static object ReprHelper(CodeContext context, object self)
	{
		return ((ICodeFormattable)self).__repr__(context);
	}

	public static string ToStringMethod(object self)
	{
		string text = self.ToString();
		if (text == null)
		{
			return string.Empty;
		}
		return text;
	}

	public static string Format(IFormattable formattable, string format)
	{
		return formattable.ToString(format, null);
	}

	public static bool EqualsMethod(object x, object y)
	{
		return x.Equals(y);
	}

	public static bool NotEqualsMethod(object x, object y)
	{
		return !x.Equals(y);
	}

	public static int StructuralHashMethod(CodeContext context, IStructuralEquatable x)
	{
		return x.GetHashCode(PythonContext.GetContext(context).EqualityComparerNonGeneric);
	}

	public static bool StructuralEqualityMethod<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralEquatable
	{
		return x.Equals(y, PythonContext.GetContext(context).EqualityComparerNonGeneric);
	}

	public static bool StructuralInequalityMethod<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralEquatable
	{
		return !x.Equals(y, PythonContext.GetContext(context).EqualityComparerNonGeneric);
	}

	[return: MaybeNotImplemented]
	public static object StructuralEqualityMethod<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralEquatable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.Equals(y, PythonContext.GetContext(context).EqualityComparerNonGeneric));
	}

	[return: MaybeNotImplemented]
	public static object StructuralInequalityMethod<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralEquatable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(!x.Equals(y, PythonContext.GetContext(context).EqualityComparerNonGeneric));
	}

	[return: MaybeNotImplemented]
	public static object StructuralEqualityMethod<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralEquatable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.Equals(y, PythonContext.GetContext(context).EqualityComparerNonGeneric));
	}

	[return: MaybeNotImplemented]
	public static object StructuralInequalityMethod<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralEquatable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(!x.Equals(y, PythonContext.GetContext(context).EqualityComparerNonGeneric));
	}

	private static int StructuralCompare(CodeContext context, IStructuralComparable x, object y)
	{
		return x.CompareTo(y, PythonContext.GetContext(context).GetComparer(null, null));
	}

	public static bool StructuralComparableEquality<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralComparable
	{
		return StructuralCompare(context, x, y) == 0;
	}

	public static bool StructuralComparableInequality<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralComparable
	{
		return StructuralCompare(context, x, y) != 0;
	}

	public static bool StructuralComparableGreaterThan<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralComparable
	{
		return StructuralCompare(context, x, y) > 0;
	}

	public static bool StructuralComparableLessThan<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralComparable
	{
		return StructuralCompare(context, x, y) < 0;
	}

	public static bool StructuralComparableGreaterEqual<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralComparable
	{
		return StructuralCompare(context, x, y) >= 0;
	}

	public static bool StructuralComparableLessEqual<T>(CodeContext context, T x, [NotNull] T y) where T : IStructuralComparable
	{
		return StructuralCompare(context, x, y) <= 0;
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableEquality<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) == 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableInequality<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) != 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableGreaterThan<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) > 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableLessThan<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) < 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableGreaterEqual<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) >= 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableLessEqual<T>(CodeContext context, [NotNull] T x, object y) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) <= 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableEquality<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) == 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableInequality<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) != 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableGreaterThan<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) < 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableLessThan<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) > 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableGreaterEqual<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) <= 0);
	}

	[return: MaybeNotImplemented]
	public static object StructuralComparableLessEqual<T>(CodeContext context, object y, [NotNull] T x) where T : IStructuralComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(StructuralCompare(context, x, y) >= 0);
	}

	public static bool ComparableEquality<T>(T x, [NotNull] T y) where T : IComparable
	{
		return x.CompareTo(y) == 0;
	}

	public static bool ComparableInequality<T>(T x, [NotNull] T y) where T : IComparable
	{
		return x.CompareTo(y) != 0;
	}

	public static bool ComparableGreaterThan<T>(T x, [NotNull] T y) where T : IComparable
	{
		return x.CompareTo(y) > 0;
	}

	public static bool ComparableLessThan<T>(T x, [NotNull] T y) where T : IComparable
	{
		return x.CompareTo(y) < 0;
	}

	public static bool ComparableGreaterEqual<T>(T x, [NotNull] T y) where T : IComparable
	{
		return x.CompareTo(y) >= 0;
	}

	public static bool ComparableLessEqual<T>(T x, [NotNull] T y) where T : IComparable
	{
		return x.CompareTo(y) <= 0;
	}

	[return: MaybeNotImplemented]
	public static object ComparableEquality<T>([NotNull] T x, object y) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) == 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableInequality<T>([NotNull] T x, object y) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) != 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableGreaterThan<T>([NotNull] T x, object y) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) > 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableLessThan<T>([NotNull] T x, object y) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) < 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableGreaterEqual<T>([NotNull] T x, object y) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) >= 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableLessEqual<T>([NotNull] T x, object y) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) <= 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableEquality<T>(object y, [NotNull] T x) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) == 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableInequality<T>(object y, [NotNull] T x) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) != 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableGreaterThan<T>(object y, [NotNull] T x) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) < 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableLessThan<T>(object y, [NotNull] T x) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) > 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableGreaterEqual<T>(object y, [NotNull] T x) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) <= 0);
	}

	[return: MaybeNotImplemented]
	public static object ComparableLessEqual<T>(object y, [NotNull] T x) where T : IComparable
	{
		if (!(y is T))
		{
			return NotImplementedType.Value;
		}
		return ScriptingRuntimeHelpers.BooleanToObject(x.CompareTo(y) >= 0);
	}

	public static object EnterMethod(IDisposable self)
	{
		return self;
	}

	public static void ExitMethod(IDisposable self, object exc_type, object exc_value, object exc_back)
	{
		self.Dispose();
	}

	[StaticExtensionMethod]
	[PropertyMethod]
	public static List Get__all__<T>(CodeContext context)
	{
		PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(typeof(T));
		List list = new List();
		foreach (string memberName in pythonTypeFromType.GetMemberNames(context))
		{
			if (IsStaticTypeMemberInAll(context, pythonTypeFromType, memberName, out var _))
			{
				list.AddNoLock(memberName);
			}
		}
		return list;
	}

	private static bool IsStaticTypeMemberInAll(CodeContext context, PythonType pt, string name, out object res)
	{
		res = null;
		if (pt.TryResolveSlot(context, name, out var slot))
		{
			if (name == "__doc__" || name == "__class__")
			{
				return false;
			}
			if (slot is ReflectedGetterSetter)
			{
				return false;
			}
			if (slot is ReflectedField reflectedField && !reflectedField._info.IsInitOnly && !reflectedField._info.IsLiteral)
			{
				return false;
			}
			if (slot is BuiltinMethodDescriptor builtinMethodDescriptor && (!builtinMethodDescriptor.DeclaringType.IsSealed() || !builtinMethodDescriptor.DeclaringType.IsAbstract()))
			{
				return false;
			}
			if (slot is BuiltinFunction builtinFunction && (!builtinFunction.DeclaringType.IsSealed() || !builtinFunction.DeclaringType.IsAbstract()))
			{
				return false;
			}
			if (slot.TryGetValue(context, null, pt, out res))
			{
				return true;
			}
		}
		res = null;
		return false;
	}

	public static bool ContainsGenericMethod<T>(CodeContext context, IEnumerable<T> enumerable, T value)
	{
		foreach (T item in enumerable)
		{
			if (PythonOps.EqualRetBool(context, item, value))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsMethod(CodeContext context, IEnumerable enumerable, object value)
	{
		IEnumerator enumerator = enumerable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (PythonOps.EqualRetBool(context, enumerator.Current, value))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsGenericMethodIEnumerator<T>(CodeContext context, IEnumerator<T> enumerator, T value)
	{
		while (enumerator.MoveNext())
		{
			if (PythonOps.EqualRetBool(context, enumerator.Current, value))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsMethodIEnumerator(CodeContext context, IEnumerator enumerator, object value)
	{
		while (enumerator.MoveNext())
		{
			if (PythonOps.EqualRetBool(context, enumerator.Current, value))
			{
				return true;
			}
		}
		return false;
	}

	public static PythonTuple SerializeReduce(CodeContext context, object self, int protocol)
	{
		PythonTuple pythonTuple = ClrModule.Serialize(self);
		PythonContext.GetContext(context).ClrModule.__dict__.TryGetValue("Deserialize", out var value);
		return PythonTuple.MakeTuple(value, pythonTuple, null);
	}

	internal static void CheckNewArgs(CodeContext context, IDictionary<object, object> dict, object[] args, PythonType pt)
	{
		if ((args != null && args.Length > 0) || (dict != null && dict.Count > 0))
		{
			bool flag = pt.HasObjectInit(context);
			bool flag2 = pt.HasObjectNew(context);
			if (flag)
			{
				throw PythonOps.TypeError("object.__new__() takes no parameters");
			}
			if (!flag2 && !flag)
			{
				PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "object.__new__() takes no parameters");
			}
		}
	}

	internal static void CheckInitArgs(CodeContext context, IDictionary<object, object> dict, object[] args, object self)
	{
		if ((args != null && args.Length > 0) || (dict != null && dict.Count > 0))
		{
			PythonType pythonType = DynamicHelpers.GetPythonType(self);
			bool flag = pythonType.HasObjectInit(context);
			bool flag2 = pythonType.HasObjectNew(context);
			if (flag2 && self != null)
			{
				throw PythonOps.TypeError("object.__init__() takes no parameters");
			}
			if ((!flag2 && !flag) || self == null)
			{
				PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "object.__init__() takes no parameters for type {0}", PythonTypeOps.GetName(self));
			}
		}
	}

	private static BuiltinMethodDescriptor GetInitMethod()
	{
		TypeCache.Object.TryResolveSlot(DefaultContext.Default, "__init__", out var slot);
		return (BuiltinMethodDescriptor)slot;
	}

	private static BuiltinFunction CreateFunction(string name, params string[] methodNames)
	{
		MethodBase[] array = new MethodBase[methodNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = typeof(InstanceOps).GetMethod(methodNames[i]);
		}
		return BuiltinFunction.MakeFunction(name, array, typeof(object));
	}

	private static void GetKeywordArgs(IDictionary<object, object> dict, object[] args, out object[] finalArgs, out string[] names)
	{
		finalArgs = new object[args.Length + dict.Count];
		Array.Copy(args, finalArgs, args.Length);
		names = new string[dict.Count];
		int num = 0;
		foreach (KeyValuePair<object, object> item in dict)
		{
			names[num] = (string)item.Key;
			finalArgs[num + args.Length] = item.Value;
			num++;
		}
	}
}
