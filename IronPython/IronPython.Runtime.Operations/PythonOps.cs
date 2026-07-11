using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Modules;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class PythonOps
{
	internal delegate T MultiplySequenceWorker<T>(T self, int count);

	[ThreadStatic]
	private static List<object> InfiniteRepr;

	[ThreadStatic]
	internal static Exception RawException;

	public static readonly PythonTuple EmptyTuple = PythonTuple.EMPTY;

	private static readonly Type[] _DelegateCtorSignature = new Type[2]
	{
		typeof(object),
		typeof(IntPtr)
	};

	[ThreadStatic]
	private static List<FunctionStack> _funcStack;

	public static Ellipsis Ellipsis => Ellipsis.Value;

	public static NotImplementedType NotImplemented => NotImplementedType.Value;

	public static BigInteger MakeIntegerFromHex(string s)
	{
		return LiteralParser.ParseBigInteger(s, 16);
	}

	public static PythonDictionary MakeDict(int size)
	{
		return new PythonDictionary(size);
	}

	public static PythonDictionary MakeEmptyDict()
	{
		return new PythonDictionary(EmptyDictionaryStorage.Instance);
	}

	public static PythonDictionary MakeDictFromItems(params object[] data)
	{
		return new PythonDictionary(new CommonDictionaryStorage(data, isHomogeneous: false));
	}

	public static PythonDictionary MakeConstantDict(object items)
	{
		return new PythonDictionary((ConstantDictionaryStorage)items);
	}

	public static object MakeConstantDictStorage(params object[] data)
	{
		return new ConstantDictionaryStorage(new CommonDictionaryStorage(data, isHomogeneous: false));
	}

	public static SetCollection MakeSet(params object[] items)
	{
		return new SetCollection(items);
	}

	public static SetCollection MakeEmptySet()
	{
		return new SetCollection();
	}

	public static PythonDictionary MakeHomogeneousDictFromItems(object[] data)
	{
		return new PythonDictionary(new CommonDictionaryStorage(data, isHomogeneous: true));
	}

	public static bool IsCallable(CodeContext context, object o)
	{
		return PythonContext.GetContext(context).IsCallable(o);
	}

	public static bool UserObjectIsCallable(CodeContext context, object o)
	{
		if (TryGetBoundAttr(context, o, "__call__", out var ret))
		{
			return ret != null;
		}
		return false;
	}

	public static bool IsTrue(object o)
	{
		return Converter.ConvertToBoolean(o);
	}

	public static List<object> GetReprInfinite()
	{
		if (InfiniteRepr == null)
		{
			InfiniteRepr = new List<object>();
		}
		return InfiniteRepr;
	}

	[LightThrowing]
	internal static object LookupEncodingError(CodeContext context, string name)
	{
		Dictionary<string, object> errorHandlers = PythonContext.GetContext(context).ErrorHandlers;
		lock (errorHandlers)
		{
			if (errorHandlers.ContainsKey(name))
			{
				return errorHandlers[name];
			}
			return LightExceptions.Throw(LookupError("unknown error handler name '{0}'", name));
		}
	}

	internal static void RegisterEncodingError(CodeContext context, string name, object handler)
	{
		Dictionary<string, object> errorHandlers = PythonContext.GetContext(context).ErrorHandlers;
		lock (errorHandlers)
		{
			if (!IsCallable(context, handler))
			{
				throw TypeError("handler must be callable");
			}
			errorHandlers[name] = handler;
		}
	}

	internal static PythonTuple LookupEncoding(CodeContext context, string encoding)
	{
		PythonContext.GetContext(context).EnsureEncodings();
		List<object> searchFunctions = PythonContext.GetContext(context).SearchFunctions;
		string text = encoding.ToLower().Replace(' ', '-');
		if (text.IndexOf('\0') != -1)
		{
			throw TypeError("lookup string cannot contain null character");
		}
		lock (searchFunctions)
		{
			for (int i = 0; i < searchFunctions.Count; i++)
			{
				object obj = PythonCalls.Call(context, searchFunctions[i], text);
				if (obj != null)
				{
					return (PythonTuple)obj;
				}
			}
		}
		throw LookupError("unknown encoding: {0}", encoding);
	}

	internal static void RegisterEncoding(CodeContext context, object search_function)
	{
		if (!IsCallable(context, search_function))
		{
			throw TypeError("search_function must be callable");
		}
		List<object> searchFunctions = PythonContext.GetContext(context).SearchFunctions;
		lock (searchFunctions)
		{
			searchFunctions.Add(search_function);
		}
	}

	internal static string GetPythonTypeName(object obj)
	{
		if (obj is OldInstance oldInstance)
		{
			return oldInstance._class._name.ToString();
		}
		return PythonTypeOps.GetName(obj);
	}

	public static string Repr(CodeContext context, object o)
	{
		if (o == null)
		{
			return "None";
		}
		if (o is string self)
		{
			return StringOps.__repr__(self);
		}
		if (o is int)
		{
			return Int32Ops.__repr__((int)o);
		}
		if (o is long num)
		{
			return num + "L";
		}
		if (o is ICodeFormattable codeFormattable)
		{
			return codeFormattable.__repr__(context);
		}
		return PythonContext.InvokeUnaryOperator(context, UnaryOperators.Repr, o) as string;
	}

	public static List<object> GetAndCheckInfinite(object o)
	{
		List<object> reprInfinite = GetReprInfinite();
		foreach (object item in reprInfinite)
		{
			if (o == item)
			{
				return null;
			}
		}
		return reprInfinite;
	}

	public static string ToString(object o)
	{
		return ToString(DefaultContext.Default, o);
	}

	public static string ToString(CodeContext context, object o)
	{
		if (o is string result)
		{
			return result;
		}
		if (o == null)
		{
			return "None";
		}
		if (o is double)
		{
			return DoubleOps.__str__(context, (double)o);
		}
		if (o is PythonType pythonType)
		{
			return pythonType.__repr__(DefaultContext.Default);
		}
		if (o is OldClass oldClass)
		{
			return oldClass.ToString();
		}
		object obj = PythonContext.InvokeUnaryOperator(context, UnaryOperators.String, o);
		string text = obj as string;
		if (text == null)
		{
			if (!(obj is Extensible<string> extensible))
			{
				throw TypeError("expected str, got {0} from __str__", PythonTypeOps.GetName(obj));
			}
			text = extensible.Value;
		}
		return text;
	}

	public static string FormatString(CodeContext context, string str, object data)
	{
		return new StringFormatter(context, str, data).Format();
	}

	public static string FormatUnicode(CodeContext context, string str, object data)
	{
		return new StringFormatter(context, str, data, isUnicode: true).Format();
	}

	public static object Plus(object o)
	{
		if (o is int)
		{
			return o;
		}
		if (o is double)
		{
			return o;
		}
		if (o is BigInteger)
		{
			return o;
		}
		if (o is Complex)
		{
			return o;
		}
		if (o is long)
		{
			return o;
		}
		if (o is float)
		{
			return o;
		}
		if (o is bool)
		{
			return ScriptingRuntimeHelpers.Int32ToObject(((bool)o) ? 1 : 0);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__pos__", out var value) && value != NotImplementedType.Value)
		{
			return value;
		}
		throw TypeError("bad operand type for unary +");
	}

	public static object Negate(object o)
	{
		if (o is int)
		{
			return Int32Ops.Negate((int)o);
		}
		if (o is double)
		{
			return DoubleOps.Negate((double)o);
		}
		if (o is long)
		{
			return Int64Ops.Negate((long)o);
		}
		if (o is BigInteger)
		{
			return BigIntegerOps.Negate((BigInteger)o);
		}
		if (o is Complex)
		{
			return -(Complex)o;
		}
		if (o is float)
		{
			return DoubleOps.Negate((float)o);
		}
		if (o is bool)
		{
			return ScriptingRuntimeHelpers.Int32ToObject(((bool)o) ? (-1) : 0);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__neg__", out var value) && value != NotImplementedType.Value)
		{
			return value;
		}
		throw TypeError("bad operand type for unary -");
	}

	public static bool IsSubClass(PythonType c, PythonType typeinfo)
	{
		if (c.OldClass != null)
		{
			return typeinfo.__subclasscheck__(c.OldClass);
		}
		return typeinfo.__subclasscheck__(c);
	}

	public static bool IsSubClass(CodeContext context, PythonType c, object typeinfo)
	{
		if (c == null)
		{
			throw TypeError("issubclass: arg 1 must be a class");
		}
		if (typeinfo == null)
		{
			throw TypeError("issubclass: arg 2 must be a class");
		}
		PythonTuple pythonTuple = typeinfo as PythonTuple;
		PythonContext context2 = PythonContext.GetContext(context);
		if (pythonTuple != null)
		{
			foreach (object item in pythonTuple)
			{
				try
				{
					FunctionPushFrame(context2);
					if (IsSubClass(context, c, item))
					{
						return true;
					}
				}
				finally
				{
					FunctionPopFrame();
				}
			}
			return false;
		}
		if (typeinfo is OldClass oldClass)
		{
			return c.IsSubclassOf(oldClass.TypeObject);
		}
		Type type = typeinfo as Type;
		if (type != null)
		{
			typeinfo = DynamicHelpers.GetPythonTypeFromType(type);
		}
		if (!(typeinfo is PythonType typeinfo2))
		{
			if (!TryGetBoundAttr(typeinfo, "__bases__", out var ret))
			{
				throw TypeErrorForBadInstance("issubclass(): {0} is not a class nor a tuple of classes", typeinfo);
			}
			IEnumerator enumerator2 = GetEnumerator(ret);
			while (enumerator2.MoveNext())
			{
				PythonType pythonType = enumerator2.Current as PythonType;
				if (pythonType == null)
				{
					if (!(enumerator2.Current is OldClass oldClass2))
					{
						continue;
					}
					pythonType = oldClass2.TypeObject;
				}
				if (c.IsSubclassOf(pythonType))
				{
					return true;
				}
			}
			return false;
		}
		return IsSubClass(c, typeinfo2);
	}

	public static bool IsInstance(object o, PythonType typeinfo)
	{
		if (typeinfo.__instancecheck__(o))
		{
			return true;
		}
		return IsInstanceDynamic(o, typeinfo, DynamicHelpers.GetPythonType(o));
	}

	public static bool IsInstance(CodeContext context, object o, PythonTuple typeinfo)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		foreach (object item in typeinfo)
		{
			try
			{
				FunctionPushFrame(context2);
				if (item is PythonType)
				{
					if (IsInstance(o, (PythonType)item))
					{
						return true;
					}
				}
				else if (item is PythonTuple)
				{
					if (IsInstance(context, o, (PythonTuple)item))
					{
						return true;
					}
				}
				else if (IsInstance(context, o, item))
				{
					return true;
				}
			}
			finally
			{
				FunctionPopFrame();
			}
		}
		return false;
	}

	public static bool IsInstance(CodeContext context, object o, object typeinfo)
	{
		if (typeinfo == null)
		{
			throw TypeError("isinstance: arg 2 must be a class, type, or tuple of classes and types");
		}
		if (typeinfo is PythonTuple typeinfo2)
		{
			return IsInstance(context, o, typeinfo2);
		}
		if (typeinfo is OldClass && o is OldInstance oldInstance)
		{
			return oldInstance._class.IsSubclassOf(typeinfo);
		}
		PythonType pythonType = DynamicHelpers.GetPythonType(o);
		if (IsSubClass(context, pythonType, typeinfo))
		{
			return true;
		}
		return IsInstanceDynamic(o, typeinfo);
	}

	private static bool IsInstanceDynamic(object o, object typeinfo)
	{
		return IsInstanceDynamic(o, typeinfo, DynamicHelpers.GetPythonType(o));
	}

	private static bool IsInstanceDynamic(object o, object typeinfo, PythonType odt)
	{
		if ((o is IPythonObject || o is OldInstance) && TryGetBoundAttr(o, "__class__", out var ret) && !object.ReferenceEquals(odt, ret))
		{
			return IsSubclassSlow(ret, typeinfo);
		}
		return false;
	}

	private static bool IsSubclassSlow(object cls, object typeinfo)
	{
		if (cls == null)
		{
			return false;
		}
		if (cls.Equals(typeinfo))
		{
			return true;
		}
		if (!TryGetBoundAttr(cls, "__bases__", out var ret))
		{
			return false;
		}
		if (!(ret is PythonTuple pythonTuple))
		{
			return false;
		}
		foreach (object item in pythonTuple)
		{
			if (IsSubclassSlow(item, typeinfo))
			{
				return true;
			}
		}
		return false;
	}

	public static object OnesComplement(object o)
	{
		if (o is int)
		{
			return ~(int)o;
		}
		if (o is long)
		{
			return ~(long)o;
		}
		if (o is BigInteger)
		{
			return ~(BigInteger)o;
		}
		if (o is bool)
		{
			return ScriptingRuntimeHelpers.Int32ToObject(((bool)o) ? (-2) : (-1));
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__invert__", out var value) && value != NotImplementedType.Value)
		{
			return value;
		}
		throw TypeError("bad operand type for unary ~");
	}

	public static bool Not(object o)
	{
		return !IsTrue(o);
	}

	public static object Is(object x, object y)
	{
		if (x != y)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static bool IsRetBool(object x, object y)
	{
		return x == y;
	}

	public static object IsNot(object x, object y)
	{
		if (x == y)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static bool IsNotRetBool(object x, object y)
	{
		return x != y;
	}

	internal static object MultiplySequence<T>(MultiplySequenceWorker<T> multiplier, T sequence, Index count, bool isForward)
	{
		if (isForward && count != null && PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, count.Value, sequence, "__rmul__", out var value) && value != NotImplementedType.Value)
		{
			return value;
		}
		int num = GetSequenceMultiplier(sequence, count.Value);
		if (num < 0)
		{
			num = 0;
		}
		return multiplier(sequence, num);
	}

	internal static int GetSequenceMultiplier(object sequence, object count)
	{
		if (!Converter.TryConvertToIndex(count, out int index))
		{
			PythonTuple pythonTuple = null;
			if (count is OldInstance || !DynamicHelpers.GetPythonType(count).IsSystemType)
			{
				pythonTuple = Builtin.TryCoerce(DefaultContext.Default, count, sequence) as PythonTuple;
			}
			if (pythonTuple == null || !Converter.TryConvertToIndex(pythonTuple[0], out index))
			{
				throw TypeError("can't multiply sequence by non-int of type '{0}'", PythonTypeOps.GetName(count));
			}
		}
		return index;
	}

	public static object Equal(CodeContext context, object x, object y)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		return context2.EqualSite.Target(context2.EqualSite, x, y);
	}

	public static bool EqualRetBool(object x, object y)
	{
		if (x is int && y is int)
		{
			return (int)x == (int)y;
		}
		if (x is string && y is string)
		{
			return ((string)x).Equals((string)y);
		}
		return DynamicHelpers.GetPythonType(x).EqualRetBool(x, y);
	}

	public static bool EqualRetBool(CodeContext context, object x, object y)
	{
		if (x is int && y is int)
		{
			return (int)x == (int)y;
		}
		if (x is string && y is string)
		{
			return ((string)x).Equals((string)y);
		}
		return DynamicHelpers.GetPythonType(x).EqualRetBool(x, y);
	}

	public static int Compare(object x, object y)
	{
		return Compare(DefaultContext.Default, x, y);
	}

	public static int Compare(CodeContext context, object x, object y)
	{
		if (x == y)
		{
			return 0;
		}
		return DynamicHelpers.GetPythonType(x).Compare(x, y);
	}

	public static object CompareEqual(int res)
	{
		if (res != 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static object CompareNotEqual(int res)
	{
		if (res != 0)
		{
			return ScriptingRuntimeHelpers.True;
		}
		return ScriptingRuntimeHelpers.False;
	}

	public static object CompareGreaterThan(int res)
	{
		if (res <= 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static object CompareGreaterThanOrEqual(int res)
	{
		if (res < 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static object CompareLessThan(int res)
	{
		if (res >= 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static object CompareLessThanOrEqual(int res)
	{
		if (res > 0)
		{
			return ScriptingRuntimeHelpers.False;
		}
		return ScriptingRuntimeHelpers.True;
	}

	public static bool CompareTypesEqual(CodeContext context, object x, object y)
	{
		if (x == null && y == null)
		{
			return true;
		}
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		if (DynamicHelpers.GetPythonType(x) == DynamicHelpers.GetPythonType(y))
		{
			return x == y;
		}
		return CompareTypesWorker(context, shouldWarn: false, x, y) == 0;
	}

	public static bool CompareTypesNotEqual(CodeContext context, object x, object y)
	{
		return CompareTypesWorker(context, shouldWarn: false, x, y) != 0;
	}

	public static bool CompareTypesGreaterThan(CodeContext context, object x, object y)
	{
		return CompareTypes(context, x, y) > 0;
	}

	public static bool CompareTypesLessThan(CodeContext context, object x, object y)
	{
		return CompareTypes(context, x, y) < 0;
	}

	public static bool CompareTypesGreaterThanOrEqual(CodeContext context, object x, object y)
	{
		return CompareTypes(context, x, y) >= 0;
	}

	public static bool CompareTypesLessThanOrEqual(CodeContext context, object x, object y)
	{
		return CompareTypes(context, x, y) <= 0;
	}

	public static int CompareTypesWorker(CodeContext context, bool shouldWarn, object x, object y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		int num;
		if (DynamicHelpers.GetPythonType(x) != DynamicHelpers.GetPythonType(y))
		{
			if (shouldWarn && PythonContext.GetContext(context).PythonOptions.WarnPython30)
			{
				Warn(context, PythonExceptions.DeprecationWarning, "comparing unequal types not supported in 3.x");
			}
			string name;
			string name2;
			if (x.GetType() == typeof(OldInstance))
			{
				name = ((OldInstance)x)._class.Name;
				if (!(y.GetType() == typeof(OldInstance)))
				{
					return -1;
				}
				name2 = ((OldInstance)y)._class.Name;
			}
			else
			{
				if (y.GetType() == typeof(OldInstance))
				{
					return 1;
				}
				name = PythonTypeOps.GetName(x);
				name2 = PythonTypeOps.GetName(y);
			}
			num = string.CompareOrdinal(name, name2);
			if (num == 0)
			{
				num = (int)(IdDispenser.GetId(DynamicHelpers.GetPythonType(x)) - IdDispenser.GetId(DynamicHelpers.GetPythonType(y)));
			}
		}
		else
		{
			num = (int)(IdDispenser.GetId(x) - IdDispenser.GetId(y));
		}
		if (num < 0)
		{
			return -1;
		}
		if (num == 0)
		{
			return 0;
		}
		return 1;
	}

	public static int CompareTypes(CodeContext context, object x, object y)
	{
		return CompareTypesWorker(context, shouldWarn: true, x, y);
	}

	public static object GreaterThanHelper(CodeContext context, object self, object other)
	{
		return InternalCompare(context, PythonOperationKind.GreaterThan, self, other);
	}

	public static object LessThanHelper(CodeContext context, object self, object other)
	{
		return InternalCompare(context, PythonOperationKind.LessThan, self, other);
	}

	public static object GreaterThanOrEqualHelper(CodeContext context, object self, object other)
	{
		return InternalCompare(context, PythonOperationKind.GreaterThanOrEqual, self, other);
	}

	public static object LessThanOrEqualHelper(CodeContext context, object self, object other)
	{
		return InternalCompare(context, PythonOperationKind.LessThanOrEqual, self, other);
	}

	internal static object InternalCompare(CodeContext context, PythonOperationKind op, object self, object other)
	{
		if (PythonTypeOps.TryInvokeBinaryOperator(context, self, other, Symbols.OperatorToSymbol(op), out var value))
		{
			return value;
		}
		return NotImplementedType.Value;
	}

	public static int CompareToZero(object value)
	{
		if (Converter.TryConvertToDouble(value, out var result))
		{
			if (result > 0.0)
			{
				return 1;
			}
			if (result < 0.0)
			{
				return -1;
			}
			return 0;
		}
		throw TypeErrorForBadInstance("an integer is required (got {0})", value);
	}

	public static int CompareArrays(object[] data0, int size0, object[] data1, int size1)
	{
		int num = Math.Min(size0, size1);
		for (int i = 0; i < num; i++)
		{
			int num2 = Compare(data0[i], data1[i]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		if (size0 == size1)
		{
			return 0;
		}
		if (size0 <= size1)
		{
			return -1;
		}
		return 1;
	}

	public static int CompareArrays(object[] data0, int size0, object[] data1, int size1, IComparer comparer)
	{
		int num = Math.Min(size0, size1);
		for (int i = 0; i < num; i++)
		{
			int num2 = comparer.Compare(data0[i], data1[i]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		if (size0 == size1)
		{
			return 0;
		}
		if (size0 <= size1)
		{
			return -1;
		}
		return 1;
	}

	public static bool ArraysEqual(object[] data0, int size0, object[] data1, int size1)
	{
		if (size0 != size1)
		{
			return false;
		}
		for (int i = 0; i < size0; i++)
		{
			if (data0[i] != null)
			{
				if (!EqualRetBool(data0[i], data1[i]))
				{
					return false;
				}
			}
			else if (data1[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool ArraysEqual(object[] data0, int size0, object[] data1, int size1, IEqualityComparer comparer)
	{
		if (size0 != size1)
		{
			return false;
		}
		for (int i = 0; i < size0; i++)
		{
			if (data0[i] != null)
			{
				if (!comparer.Equals(data0[i], data1[i]))
				{
					return false;
				}
			}
			else if (data1[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	public static object PowerMod(CodeContext context, object x, object y, object z)
	{
		if (z == null)
		{
			return PythonContext.GetContext(context).Operation(PythonOperationKind.Power, x, y);
		}
		object value;
		if (x is int && y is int && z is int)
		{
			value = Int32Ops.Power((int)x, (int)y, (int)z);
			if (value != NotImplementedType.Value)
			{
				return value;
			}
		}
		else if (x is BigInteger)
		{
			value = BigIntegerOps.Power((BigInteger)x, y, z);
			if (value != NotImplementedType.Value)
			{
				return value;
			}
		}
		if (x is Complex || y is Complex || z is Complex)
		{
			throw ValueError("complex modulo");
		}
		if (PythonTypeOps.TryInvokeTernaryOperator(context, x, y, z, "__pow__", out value))
		{
			if (value != NotImplementedType.Value)
			{
				return value;
			}
			if (!IsNumericObject(y) || !IsNumericObject(z))
			{
				throw TypeError("pow() 3rd argument not allowed unless all arguments are integers");
			}
		}
		throw TypeErrorForBinaryOp("power with modulus", x, y);
	}

	public static long Id(object o)
	{
		return IdDispenser.GetId(o);
	}

	public static string HexId(object o)
	{
		return $"0x{Id(o):X16}";
	}

	public static int Hash(CodeContext context, object o)
	{
		return PythonContext.Hash(o);
	}

	public static object Hex(object o)
	{
		if (o is int)
		{
			return Int32Ops.__hex__((int)o);
		}
		if (o is BigInteger)
		{
			return BigIntegerOps.__hex__((BigInteger)o);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__hex__", out var value))
		{
			if (!(value is string) && !(value is ExtensibleString))
			{
				throw TypeError("hex expected string type as return, got '{0}'", PythonTypeOps.GetName(value));
			}
			return value;
		}
		throw TypeError("hex() argument cannot be converted to hex");
	}

	public static object Oct(object o)
	{
		if (o is int)
		{
			return Int32Ops.__oct__((int)o);
		}
		if (o is BigInteger)
		{
			return BigIntegerOps.__oct__((BigInteger)o);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(DefaultContext.Default, o, "__oct__", out var value))
		{
			if (!(value is string) && !(value is ExtensibleString))
			{
				throw TypeError("hex expected string type as return, got '{0}'", PythonTypeOps.GetName(value));
			}
			return value;
		}
		throw TypeError("oct() argument cannot be converted to octal");
	}

	public static int Length(object o)
	{
		if (o is string text)
		{
			return text.Length;
		}
		if (o is object[] array)
		{
			return array.Length;
		}
		object obj = PythonContext.InvokeUnaryOperator(DefaultContext.Default, UnaryOperators.Length, o, "len() of unsized object");
		int num = ((!(obj is int)) ? Converter.ConvertToInt32(obj) : ((int)obj));
		if (num < 0)
		{
			throw ValueError("__len__ should return >= 0, got {0}", num);
		}
		return num;
	}

	public static object CallWithContext(CodeContext context, object func, params object[] args)
	{
		return PythonCalls.Call(context, func, args);
	}

	public static object CallWithContextAndThis(CodeContext context, object func, object instance, params object[] args)
	{
		return CallWithContext(context, func, args);
	}

	public static object ToPythonType(PythonType dt)
	{
		if (dt != null)
		{
			return ((object)dt.OldClass) ?? ((object)dt);
		}
		return null;
	}

	public static object CallWithArgsTupleAndContext(CodeContext context, object func, object[] args, object argsTuple)
	{
		if (argsTuple is PythonTuple pythonTuple)
		{
			object[] array = new object[args.Length + pythonTuple.__len__()];
			for (int i = 0; i < args.Length; i++)
			{
				array[i] = args[i];
			}
			for (int j = 0; j < pythonTuple.__len__(); j++)
			{
				array[j + args.Length] = pythonTuple[j];
			}
			return CallWithContext(context, func, array);
		}
		List list = MakeEmptyList(args.Length + 10);
		list.AddRange(args);
		IEnumerator enumerator = GetEnumerator(argsTuple);
		while (enumerator.MoveNext())
		{
			list.AddNoLock(enumerator.Current);
		}
		return CallWithContext(context, func, list.GetObjectArray());
	}

	[Obsolete("Use ObjectOpertaions instead")]
	public static object CallWithArgsTupleAndKeywordDictAndContext(CodeContext context, object func, object[] args, string[] names, object argsTuple, object kwDict)
	{
		IDictionary dictionary = kwDict as IDictionary;
		if (dictionary == null && kwDict != null)
		{
			throw TypeError("argument after ** must be a dictionary");
		}
		if ((dictionary == null || dictionary.Count == 0) && names.Length == 0)
		{
			List<object> list = new List<object>(args);
			if (argsTuple != null)
			{
				foreach (object item in GetCollection(argsTuple))
				{
					list.Add(item);
				}
			}
			return CallWithContext(context, func, list.ToArray());
		}
		List<object> list2;
		if (argsTuple != null && args.Length == names.Length)
		{
			PythonTuple pythonTuple = argsTuple as PythonTuple;
			if (pythonTuple == null)
			{
				pythonTuple = new PythonTuple(argsTuple);
			}
			list2 = new List<object>(pythonTuple);
			list2.AddRange(args);
		}
		else
		{
			list2 = new List<object>(args);
			if (argsTuple != null)
			{
				list2.InsertRange(args.Length - names.Length, PythonTuple.Make(argsTuple));
			}
		}
		List<string> list3 = new List<string>(names);
		if (dictionary != null)
		{
			IDictionaryEnumerator enumerator2 = dictionary.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				list3.Add((string)enumerator2.Key);
				list2.Add(enumerator2.Value);
			}
		}
		return PythonCalls.CallWithKeywordArgs(context, func, list2.ToArray(), list3.ToArray());
	}

	public static object CallWithKeywordArgs(CodeContext context, object func, object[] args, string[] names)
	{
		return PythonCalls.CallWithKeywordArgs(context, func, args, names);
	}

	public static object CallWithArgsTuple(object func, object[] args, object argsTuple)
	{
		if (argsTuple is PythonTuple pythonTuple)
		{
			object[] array = new object[args.Length + pythonTuple.__len__()];
			for (int i = 0; i < args.Length; i++)
			{
				array[i] = args[i];
			}
			for (int j = 0; j < pythonTuple.__len__(); j++)
			{
				array[j + args.Length] = pythonTuple[j];
			}
			return PythonCalls.Call(func, array);
		}
		List list = MakeEmptyList(args.Length + 10);
		list.AddRange(args);
		IEnumerator enumerator = GetEnumerator(argsTuple);
		while (enumerator.MoveNext())
		{
			list.AddNoLock(enumerator.Current);
		}
		return PythonCalls.Call(func, list.GetObjectArray());
	}

	public static object GetIndex(CodeContext context, object o, object index)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		return context2.GetIndexSite.Target(context2.GetIndexSite, o, index);
	}

	public static bool TryGetBoundAttr(object o, string name, out object ret)
	{
		return TryGetBoundAttr(DefaultContext.Default, o, name, out ret);
	}

	public static void SetAttr(CodeContext context, object o, string name, object value)
	{
		PythonContext.GetContext(context).SetAttr(context, o, name, value);
	}

	public static bool TryGetBoundAttr(CodeContext context, object o, string name, out object ret)
	{
		return DynamicHelpers.GetPythonType(o).TryGetBoundAttr(context, o, name, out ret);
	}

	public static void DeleteAttr(CodeContext context, object o, string name)
	{
		PythonContext.GetContext(context).DeleteAttr(context, o, name);
	}

	public static bool HasAttr(CodeContext context, object o, string name)
	{
		try
		{
			object ret;
			return TryGetBoundAttr(context, o, name, out ret);
		}
		catch (SystemExitException)
		{
			throw;
		}
		catch (KeyboardInterruptException)
		{
			throw;
		}
		catch
		{
			return false;
		}
	}

	public static object GetBoundAttr(CodeContext context, object o, string name)
	{
		if (!DynamicHelpers.GetPythonType(o).TryGetBoundAttr(context, o, name, out var ret))
		{
			if (o is OldClass)
			{
				throw AttributeError("type object '{0}' has no attribute '{1}'", ((OldClass)o).Name, name);
			}
			throw AttributeError("'{0}' object has no attribute '{1}'", PythonTypeOps.GetName(o), name);
		}
		return ret;
	}

	public static void ObjectSetAttribute(CodeContext context, object o, string name, object value)
	{
		if (!DynamicHelpers.GetPythonType(o).TrySetNonCustomMember(context, o, name, value))
		{
			throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetPythonType(o), name);
		}
	}

	public static void ObjectDeleteAttribute(CodeContext context, object o, string name)
	{
		if (!DynamicHelpers.GetPythonType(o).TryDeleteNonCustomMember(context, o, name))
		{
			throw AttributeErrorForMissingOrReadonly(context, DynamicHelpers.GetPythonType(o), name);
		}
	}

	public static object ObjectGetAttribute(CodeContext context, object o, string name)
	{
		if (o is OldClass oldClass)
		{
			return oldClass.GetMember(context, name);
		}
		if (DynamicHelpers.GetPythonType(o).TryGetNonCustomMember(context, o, name, out var value))
		{
			return value;
		}
		throw AttributeErrorForObjectMissingAttribute(o, name);
	}

	internal static IList<string> GetStringMemberList(IPythonMembersList pyMemList)
	{
		List<string> list = new List<string>();
		foreach (object memberName in pyMemList.GetMemberNames(DefaultContext.Default))
		{
			if (memberName is string)
			{
				list.Add((string)memberName);
			}
		}
		return list;
	}

	public static IList<object> GetAttrNames(CodeContext context, object o)
	{
		if (o is IPythonMembersList pythonMembersList)
		{
			return pythonMembersList.GetMemberNames(context);
		}
		if (o is IMembersList membersList)
		{
			return new List(membersList.GetMemberNames());
		}
		if (o is IPythonObject pythonObject)
		{
			return pythonObject.PythonType.GetMemberNames(context, o);
		}
		List memberNames = DynamicHelpers.GetPythonType(o).GetMemberNames(context, o);
		if (o != null && ComBinder.IsComObject(o))
		{
			foreach (string dynamicMemberName in ComBinder.GetDynamicMemberNames(o))
			{
				if (!memberNames.Contains(dynamicMemberName))
				{
					memberNames.AddNoLock(dynamicMemberName);
				}
			}
		}
		return memberNames;
	}

	public static void CheckInitializedAttribute(object o, object self, string name)
	{
		if (o == Uninitialized.Instance)
		{
			throw AttributeError("'{0}' object has no attribute '{1}'", PythonTypeOps.GetName(self), name);
		}
	}

	public static object GetUserSlotValue(CodeContext context, PythonTypeUserDescriptorSlot slot, object instance, PythonType type)
	{
		return slot.GetValue(context, instance, type);
	}

	public static object GetUserDescriptor(object o, object instance, object context)
	{
		if (o is IPythonObject && PythonContext.TryInvokeTernaryOperator(DefaultContext.Default, TernaryOperators.GetDescriptor, o, instance, context, out var res))
		{
			return res;
		}
		return o;
	}

	public static bool TrySetUserDescriptor(object o, object instance, object value)
	{
		if (o != null && o.GetType() == typeof(OldInstance))
		{
			return false;
		}
		object res;
		return PythonContext.TryInvokeTernaryOperator(DefaultContext.Default, TernaryOperators.SetDescriptor, o, instance, value, out res);
	}

	public static bool TryDeleteUserDescriptor(object o, object instance)
	{
		if (o != null && o.GetType() == typeof(OldInstance))
		{
			return false;
		}
		object value;
		return PythonTypeOps.TryInvokeBinaryOperator(DefaultContext.Default, o, instance, "__delete__", out value);
	}

	public static object Invoke(CodeContext context, object target, string name, params object[] args)
	{
		return PythonCalls.Call(context, GetBoundAttr(context, target, name), args);
	}

	public static Delegate CreateDynamicDelegate(DynamicMethod meth, Type delegateType, object target)
	{
		return meth.CreateDelegate(delegateType, target);
	}

	public static double CheckMath(double v)
	{
		if (double.IsInfinity(v))
		{
			throw OverflowError("math range error");
		}
		if (double.IsNaN(v))
		{
			throw ValueError("math domain error");
		}
		return v;
	}

	public static double CheckMath(double input, double output)
	{
		if ((double.IsInfinity(input) && double.IsInfinity(output)) || (double.IsNaN(input) && double.IsNaN(output)))
		{
			return output;
		}
		return CheckMath(output);
	}

	public static double CheckMath(double in0, double in1, double output)
	{
		if (((double.IsInfinity(in0) || double.IsInfinity(in1)) && double.IsInfinity(output)) || ((double.IsNaN(in0) || double.IsNaN(in1)) && double.IsNaN(output)))
		{
			return output;
		}
		return CheckMath(output);
	}

	public static object IsMappingType(CodeContext context, object o)
	{
		if (o is IDictionary || o is PythonDictionary || o is IDictionary<object, object> || o is PythonDictionary)
		{
			return ScriptingRuntimeHelpers.True;
		}
		if ((o is IPythonObject || o is OldInstance) && TryGetBoundAttr(context, o, "__getitem__", out var _))
		{
			if (!IsClsVisible(context) && o is BuiltinFunction)
			{
				return ScriptingRuntimeHelpers.False;
			}
			return ScriptingRuntimeHelpers.True;
		}
		return ScriptingRuntimeHelpers.False;
	}

	public static int FixSliceIndex(int v, int len)
	{
		if (v < 0)
		{
			v = len + v;
		}
		if (v < 0)
		{
			return 0;
		}
		if (v > len)
		{
			return len;
		}
		return v;
	}

	public static long FixSliceIndex(long v, long len)
	{
		if (v < 0)
		{
			v = len + v;
		}
		if (v < 0)
		{
			return 0L;
		}
		if (v > len)
		{
			return len;
		}
		return v;
	}

	public static void FixSlice(int length, object start, object stop, object step, out int ostart, out int ostop, out int ostep)
	{
		if (step == null)
		{
			ostep = 1;
		}
		else
		{
			ostep = Converter.ConvertToIndex(step);
			if (ostep == 0)
			{
				throw ValueError("step cannot be zero");
			}
		}
		if (start == null)
		{
			ostart = ((ostep <= 0) ? (length - 1) : 0);
		}
		else
		{
			ostart = Converter.ConvertToIndex(start);
			if (ostart < 0)
			{
				ostart += length;
				if (ostart < 0)
				{
					ostart = ((ostep > 0) ? Math.Min(length, 0) : Math.Min(length - 1, -1));
				}
			}
			else if (ostart >= length)
			{
				ostart = ((ostep > 0) ? length : (length - 1));
			}
		}
		if (stop == null)
		{
			ostop = ((ostep > 0) ? length : (-1));
			return;
		}
		ostop = Converter.ConvertToIndex(stop);
		if (ostop < 0)
		{
			ostop += length;
			if (ostop < 0)
			{
				ostop = ((ostep > 0) ? Math.Min(length, 0) : Math.Min(length - 1, -1));
			}
		}
		else if (ostop >= length)
		{
			ostop = ((ostep > 0) ? length : (length - 1));
		}
	}

	public static void FixSlice(long length, long? start, long? stop, long? step, out long ostart, out long ostop, out long ostep, out long ocount)
	{
		if (!step.HasValue)
		{
			ostep = 1L;
		}
		else
		{
			if (step == 0)
			{
				throw ValueError("step cannot be zero");
			}
			ostep = step.Value;
		}
		if (!start.HasValue)
		{
			ostart = ((ostep > 0) ? 0 : (length - 1));
		}
		else
		{
			ostart = start.Value;
			if (ostart < 0)
			{
				ostart += length;
				if (ostart < 0)
				{
					ostart = ((ostep > 0) ? Math.Min(length, 0L) : Math.Min(length - 1, -1L));
				}
			}
			else if (ostart >= length)
			{
				ostart = ((ostep > 0) ? length : (length - 1));
			}
		}
		if (!stop.HasValue)
		{
			ostop = ((ostep > 0) ? length : (-1));
		}
		else
		{
			ostop = stop.Value;
			if (ostop < 0)
			{
				ostop += length;
				if (ostop < 0)
				{
					ostop = ((ostep > 0) ? Math.Min(length, 0L) : Math.Min(length - 1, -1L));
				}
			}
			else if (ostop >= length)
			{
				ostop = ((ostep > 0) ? length : (length - 1));
			}
		}
		ocount = Math.Max(0L, (ostep > 0) ? ((ostop - ostart + ostep - 1) / ostep) : ((ostop - ostart + ostep + 1) / ostep));
	}

	public static int FixIndex(int v, int len)
	{
		if (v < 0)
		{
			v += len;
			if (v < 0)
			{
				throw IndexError("index out of range: {0}", v - len);
			}
		}
		else if (v >= len)
		{
			throw IndexError("index out of range: {0}", v);
		}
		return v;
	}

	public static void InitializeForFinalization(CodeContext context, object newObject)
	{
		IWeakReferenceable weakReferenceable = newObject as IWeakReferenceable;
		InstanceFinalizer instanceFinalizer = new InstanceFinalizer(context, newObject);
		weakReferenceable.SetFinalizer(new WeakRefTracker(instanceFinalizer, instanceFinalizer));
	}

	private static object FindMetaclass(CodeContext context, PythonTuple bases, PythonDictionary dict)
	{
		if (dict.TryGetValue("__metaclass__", out var value) && value != null)
		{
			return value;
		}
		for (int i = 0; i < bases.__len__(); i++)
		{
			if (!(bases[i] is OldClass))
			{
				return DynamicHelpers.GetPythonType(bases[i]);
			}
		}
		if (context.TryGetGlobalVariable("__metaclass__", out value) && value != null)
		{
			return value;
		}
		return TypeCache.OldInstance;
	}

	public static object MakeClass(FunctionCode funcCode, Func<CodeContext, CodeContext> body, CodeContext parentContext, string name, object[] bases, string selfNames)
	{
		Func<CodeContext, CodeContext> classCode = GetClassCode(parentContext, funcCode, body);
		return MakeClass(parentContext, name, bases, selfNames, classCode(parentContext).Dict);
	}

	private static Func<CodeContext, CodeContext> GetClassCode(CodeContext context, FunctionCode funcCode, Func<CodeContext, CodeContext> body)
	{
		if (body == null)
		{
			if ((object)funcCode.Target == null)
			{
				funcCode.UpdateDelegate(context.LanguageContext, forceCreation: true);
			}
			return (Func<CodeContext, CodeContext>)funcCode.Target;
		}
		if ((object)funcCode.Target == null)
		{
			funcCode.SetTarget(body);
			funcCode._normalDelegate = body;
		}
		return body;
	}

	internal static object MakeClass(CodeContext context, string name, object[] bases, string selfNames, PythonDictionary vars)
	{
		object[] array = bases;
		foreach (object obj in array)
		{
			if (!(obj is TypeGroup))
			{
				continue;
			}
			object[] array2 = new object[bases.Length];
			for (int j = 0; j < bases.Length; j++)
			{
				if (bases[j] is TypeGroup typeGroup)
				{
					if (!typeGroup.TryGetNonGenericType(out var nonGenericType))
					{
						throw TypeError("cannot derive from open generic types " + Builtin.repr(context, typeGroup).ToString());
					}
					array2[j] = DynamicHelpers.GetPythonTypeFromType(nonGenericType);
				}
				else
				{
					array2[j] = bases[j];
				}
			}
			bases = array2;
			break;
		}
		PythonTuple pythonTuple = PythonTuple.MakeTuple(bases);
		object obj2 = FindMetaclass(context, pythonTuple, vars);
		if (obj2 == TypeCache.OldInstance)
		{
			return new OldClass(name, pythonTuple, vars, selfNames);
		}
		if (obj2 == TypeCache.PythonType)
		{
			return PythonType.__new__(context, TypeCache.PythonType, name, pythonTuple, vars, selfNames);
		}
		PythonContext context2 = PythonContext.GetContext(context);
		return context2.MetaClassCallSite.Target(context2.MetaClassCallSite, context, obj2, name, pythonTuple, vars);
	}

	public static void RaiseAssertionError(object msg)
	{
		if (msg == null)
		{
			throw AssertionError(string.Empty, ArrayUtils.EmptyObjects);
		}
		string text = ToString(msg);
		throw AssertionError("{0}", text);
	}

	public static List MakeList()
	{
		return new List();
	}

	public static List MakeList(params object[] items)
	{
		return new List(items);
	}

	[NoSideEffects]
	public static List MakeListNoCopy(params object[] items)
	{
		return List.FromArrayNoCopy(items);
	}

	public static List MakeListFromSequence(object sequence)
	{
		return new List(sequence);
	}

	[NoSideEffects]
	public static List MakeEmptyList(int capacity)
	{
		return new List(capacity);
	}

	[NoSideEffects]
	public static List MakeEmptyListFromCode()
	{
		return List.FromArrayNoCopy(ArrayUtils.EmptyObjects);
	}

	[NoSideEffects]
	public static PythonTuple MakeTuple(params object[] items)
	{
		return PythonTuple.MakeTuple(items);
	}

	[NoSideEffects]
	public static PythonTuple MakeTupleFromSequence(object items)
	{
		return PythonTuple.Make(items);
	}

	[LightThrowing]
	public static object GetEnumeratorValues(CodeContext context, object e, int expected)
	{
		if (e != null && e.GetType() == typeof(PythonTuple))
		{
			return GetEnumeratorValuesFromTuple((PythonTuple)e, expected);
		}
		IEnumerator enumeratorForUnpack = GetEnumeratorForUnpack(context, e);
		int i = 0;
		object[] array = new object[expected];
		for (; i < expected; i++)
		{
			if (!enumeratorForUnpack.MoveNext())
			{
				return LightExceptions.Throw(ValueErrorForUnpackMismatch(expected, i));
			}
			array[i] = enumeratorForUnpack.Current;
		}
		if (enumeratorForUnpack.MoveNext())
		{
			return LightExceptions.Throw(ValueErrorForUnpackMismatch(expected, i + 1));
		}
		return array;
	}

	[LightThrowing]
	public static object GetEnumeratorValuesNoComplexSets(CodeContext context, object e, int expected)
	{
		if (e != null && e.GetType() == typeof(List))
		{
			return GetEnumeratorValuesFromList((List)e, expected);
		}
		return GetEnumeratorValues(context, e, expected);
	}

	[LightThrowing]
	private static object GetEnumeratorValuesFromTuple(PythonTuple pythonTuple, int expected)
	{
		if (pythonTuple.Count == expected)
		{
			return pythonTuple._data;
		}
		return LightExceptions.Throw(ValueErrorForUnpackMismatch(expected, pythonTuple.Count));
	}

	private static object[] GetEnumeratorValuesFromList(List list, int expected)
	{
		if (list._size == expected)
		{
			return list._data;
		}
		throw ValueErrorForUnpackMismatch(expected, list._size);
	}

	public static Slice MakeSlice(object start, object stop, object step)
	{
		return new Slice(start, stop, step);
	}

	public static void Write(CodeContext context, object f, string text)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (f == null)
		{
			f = context2.SystemStandardOut;
		}
		if (f == null || f == Uninitialized.Instance)
		{
			throw RuntimeError("lost sys.stdout");
		}
		if (f is PythonFile pythonFile)
		{
			pythonFile.write(text);
		}
		else
		{
			context2.WriteCallSite.Target(context2.WriteCallSite, context, GetBoundAttr(context, f, "write"), text);
		}
	}

	private static object ReadLine(CodeContext context, object f)
	{
		if (f == null || f == Uninitialized.Instance)
		{
			throw RuntimeError("lost sys.std_in");
		}
		return Invoke(context, f, "readline");
	}

	public static void WriteSoftspace(CodeContext context, object f)
	{
		if (CheckSoftspace(f))
		{
			SetSoftspace(f, ScriptingRuntimeHelpers.False);
			Write(context, f, " ");
		}
	}

	public static void SetSoftspace(object f, object value)
	{
		SetAttr(DefaultContext.Default, f, "softspace", value);
	}

	public static bool CheckSoftspace(object f)
	{
		if (f is PythonFile pythonFile)
		{
			return pythonFile.softspace;
		}
		if (TryGetBoundAttr(f, "softspace", out var ret))
		{
			return IsTrue(ret);
		}
		return false;
	}

	public static void Print(CodeContext context, object o)
	{
		PrintWithDest(context, PythonContext.GetContext(context).SystemStandardOut, o);
	}

	public static void PrintNoNewline(CodeContext context, object o)
	{
		PrintWithDestNoNewline(context, PythonContext.GetContext(context).SystemStandardOut, o);
	}

	public static void PrintWithDest(CodeContext context, object dest, object o)
	{
		PrintWithDestNoNewline(context, dest, o);
		Write(context, dest, "\n");
	}

	public static void PrintWithDestNoNewline(CodeContext context, object dest, object o)
	{
		WriteSoftspace(context, dest);
		Write(context, dest, (o == null) ? "None" : ToString(o));
	}

	public static object ReadLineFromSrc(CodeContext context, object src)
	{
		return ReadLine(context, src);
	}

	public static void PrintNewline(CodeContext context)
	{
		PrintNewlineWithDest(context, PythonContext.GetContext(context).SystemStandardOut);
	}

	public static void PrintNewlineWithDest(CodeContext context, object dest)
	{
		Write(context, dest, "\n");
		SetSoftspace(dest, ScriptingRuntimeHelpers.False);
	}

	public static void PrintComma(CodeContext context, object o)
	{
		PrintCommaWithDest(context, PythonContext.GetContext(context).SystemStandardOut, o);
	}

	public static void PrintCommaWithDest(CodeContext context, object dest, object o)
	{
		WriteSoftspace(context, dest);
		string text = ((o == null) ? "None" : ToString(o));
		Write(context, dest, text);
		SetSoftspace(dest, !text.EndsWith("\n"));
	}

	public static void PrintExpressionValue(CodeContext context, object value)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		object systemStateValue = context2.GetSystemStateValue("displayhook");
		context2.CallWithContext(context, systemStateValue, value);
	}

	public static void PrintException(CodeContext context, Exception exception, IConsole console)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonTuple exceptionInfoLocal = GetExceptionInfoLocal(context, exception);
		context2.SetSystemStateValue("last_type", exceptionInfoLocal[0]);
		context2.SetSystemStateValue("last_value", exceptionInfoLocal[1]);
		context2.SetSystemStateValue("last_traceback", exceptionInfoLocal[2]);
		object systemStateValue = context2.GetSystemStateValue("excepthook");
		BuiltinFunction builtinFunction = systemStateValue as BuiltinFunction;
		if (console != null && builtinFunction != null && builtinFunction.DeclaringType == typeof(SysModule) && builtinFunction.Name == "excepthook")
		{
			console.WriteLine(context2.FormatException(exception), Style.Error);
			return;
		}
		try
		{
			PythonCalls.Call(context, systemStateValue, exceptionInfoLocal[0], exceptionInfoLocal[1], exceptionInfoLocal[2]);
		}
		catch (Exception exception2)
		{
			PrintWithDest(context, context2.SystemStandardError, "Error in sys.excepthook:");
			PrintWithDest(context, context2.SystemStandardError, context2.FormatException(exception2));
			PrintNewlineWithDest(context, context2.SystemStandardError);
			PrintWithDest(context, context2.SystemStandardError, "Original exception was:");
			PrintWithDest(context, context2.SystemStandardError, context2.FormatException(exception));
		}
	}

	[ProfilerTreatsAsExternal]
	[LightThrowing]
	public static object ImportTop(CodeContext context, string fullName, int level)
	{
		return Importer.ImportLightThrow(context, fullName, null, level);
	}

	[ProfilerTreatsAsExternal]
	[LightThrowing]
	public static object ImportBottom(CodeContext context, string fullName, int level)
	{
		object obj = Importer.ImportLightThrow(context, fullName, null, level);
		if (fullName.IndexOf('.') >= 0)
		{
			string[] array = fullName.Split('.');
			for (int i = 1; i < array.Length; i++)
			{
				obj = GetBoundAttr(context, obj, array[i]);
			}
		}
		return obj;
	}

	[ProfilerTreatsAsExternal]
	[LightThrowing]
	public static object ImportWithNames(CodeContext context, string fullName, string[] names, int level)
	{
		return Importer.ImportLightThrow(context, fullName, PythonTuple.MakeTuple(names), level);
	}

	public static object ImportFrom(CodeContext context, object module, string name)
	{
		return Importer.ImportFrom(context, module, name);
	}

	[ProfilerTreatsAsExternal]
	public static void ImportStar(CodeContext context, string fullName, int level)
	{
		object obj = Importer.Import(context, fullName, PythonTuple.MakeTuple("*"), level);
		PythonModule pythonModule = obj as PythonModule;
		NamespaceTracker namespaceTracker = obj as NamespaceTracker;
		PythonType pythonType = obj as PythonType;
		if (pythonType != null && !pythonType.UnderlyingSystemType.IsEnum() && (!pythonType.UnderlyingSystemType.IsAbstract() || !pythonType.UnderlyingSystemType.IsSealed()))
		{
			throw ImportError("no module named {0}", pythonType.Name);
		}
		bool flag = false;
		IEnumerator enumerator;
		if (TryGetBoundAttr(context, obj, "__all__", out var ret))
		{
			enumerator = GetEnumerator(ret);
		}
		else
		{
			enumerator = GetAttrNames(context, obj).GetEnumerator();
			flag = true;
		}
		while (enumerator.MoveNext())
		{
			if (!(enumerator.Current is string text))
			{
				throw TypeErrorForNonStringAttribute();
			}
			if (flag && text.Length > 0 && text[0] == '_')
			{
				continue;
			}
			if (pythonModule != null)
			{
				context.SetVariable(text, pythonModule.__dict__[text]);
			}
			else if (namespaceTracker != null)
			{
				object customMember = NamespaceTrackerOps.GetCustomMember(context, namespaceTracker, text);
				if (customMember != OperationFailed.Value)
				{
					context.SetVariable(text, customMember);
				}
			}
			else if (pythonType != null)
			{
				if (pythonType.TryResolveSlot(context, text, out var slot) && slot.TryGetValue(context, null, pythonType, out var value))
				{
					context.SetVariable(text, value);
				}
			}
			else
			{
				context.SetVariable(text, GetBoundAttr(context, obj, text));
			}
		}
	}

	[ProfilerTreatsAsExternal]
	public static void UnqualifiedExec(CodeContext context, object code)
	{
		PythonDictionary pythonDictionary = null;
		PythonDictionary pythonDictionary2 = null;
		if (code is PythonTuple pythonTuple && pythonTuple.__len__() > 0 && pythonTuple.__len__() <= 3)
		{
			code = pythonTuple[0];
			if (pythonTuple.__len__() > 1 && pythonTuple[1] != null)
			{
				pythonDictionary2 = pythonTuple[1] as PythonDictionary;
				if (pythonDictionary2 == null)
				{
					throw TypeError("globals must be dictionary or none");
				}
			}
			if (pythonTuple.__len__() > 2 && pythonTuple[2] != null)
			{
				pythonDictionary = pythonTuple[2] as PythonDictionary;
				if (pythonDictionary == null)
				{
					throw TypeError("locals must be dictionary or none");
				}
			}
			else
			{
				pythonDictionary = pythonDictionary2;
			}
		}
		QualifiedExec(context, code, pythonDictionary2, pythonDictionary);
	}

	[ProfilerTreatsAsExternal]
	public static void QualifiedExec(CodeContext context, object code, PythonDictionary globals, object locals)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		bool flag = true;
		if (code is PythonFile pythonFile)
		{
			List list = pythonFile.readlines();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.__len__(); i++)
			{
				stringBuilder.Append(list[i]);
			}
			code = stringBuilder.ToString();
		}
		else if (code is Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				code = streamReader.ReadToEnd();
			}
			flag = false;
		}
		if (code is string code2)
		{
			SourceUnit sourceUnit = ((!flag) ? context2.CreateSnippet(code2, SourceCodeKind.Statements) : context2.CreateSourceUnit(new NoLineFeedSourceContentProvider(code2), "<string>", SourceCodeKind.Statements));
			PythonCompilerOptions runtimeGeneratedCodeCompilerOptions = Builtin.GetRuntimeGeneratedCodeCompilerOptions(context, inheritContext: true, (CompileFlags)0);
			code = FunctionCode.FromSourceUnit(sourceUnit, runtimeGeneratedCodeCompilerOptions, register: false);
		}
		if (!(code is FunctionCode functionCode))
		{
			throw TypeError("arg 1 must be a string, file, Stream, or code object, not {0}", PythonTypeOps.GetName(code));
		}
		if (locals == null)
		{
			locals = globals;
		}
		if (globals == null)
		{
			globals = context.GlobalDict;
		}
		if (locals != null && IsMappingType(context, locals) != ScriptingRuntimeHelpers.True)
		{
			throw TypeError("exec: arg 3 must be mapping or None");
		}
		CodeContext execEvalScope = Builtin.GetExecEvalScope(context, globals, Builtin.GetAttrLocals(context, locals), copyModule: true, setBuiltinsToModule: false);
		if (context.LanguageContext.PythonOptions.Frames)
		{
			List<FunctionStack> list2 = PushFrame(execEvalScope, functionCode);
			try
			{
				functionCode.Call(execEvalScope);
				return;
			}
			finally
			{
				list2.RemoveAt(list2.Count - 1);
			}
		}
		functionCode.Call(execEvalScope);
	}

	public static ICollection GetCollection(object o)
	{
		if (o is ICollection result)
		{
			return result;
		}
		List<object> list = new List<object>();
		IEnumerator enumerator = GetEnumerator(o);
		while (enumerator.MoveNext())
		{
			list.Add(enumerator.Current);
		}
		return list;
	}

	public static IEnumerator GetEnumerator(object o)
	{
		return GetEnumerator(DefaultContext.Default, o);
	}

	public static IEnumerator GetEnumerator(CodeContext context, object o)
	{
		if (!TryGetEnumerator(context, o, out var enumerator))
		{
			throw TypeError("{0} is not iterable", PythonTypeOps.GetName(o));
		}
		return enumerator;
	}

	public static object GetEnumeratorObject(CodeContext context, object o)
	{
		if (TryGetBoundAttr(context, o, "__iter__", out var ret) && !object.ReferenceEquals(ret, NotImplementedType.Value))
		{
			return CallWithContext(context, ret);
		}
		return GetEnumerator(context, o);
	}

	public static IEnumerator GetEnumeratorForUnpack(CodeContext context, object enumerable)
	{
		if (!TryGetEnumerator(context, enumerable, out var enumerator))
		{
			throw TypeErrorForNotIterable(enumerable);
		}
		return enumerator;
	}

	public static Exception TypeErrorForNotIterable(object enumerable)
	{
		return TypeError("'{0}' object is not iterable", PythonTypeOps.GetName(enumerable));
	}

	public static KeyValuePair<IEnumerator, IDisposable> ThrowTypeErrorForBadIteration(CodeContext context, object enumerable)
	{
		throw TypeError("iteration over non-sequence of type {0}", PythonTypeOps.GetName(enumerable));
	}

	internal static bool TryGetEnumerator(CodeContext context, object enumerable, out IEnumerator enumerator)
	{
		enumerator = null;
		if (PythonContext.GetContext(context).TryConvertToIEnumerable(enumerable, out var res))
		{
			enumerator = res.GetEnumerator();
			return true;
		}
		return false;
	}

	public static void ForLoopDispose(KeyValuePair<IEnumerator, IDisposable> iteratorInfo)
	{
		if (iteratorInfo.Value != null)
		{
			iteratorInfo.Value.Dispose();
		}
	}

	public static KeyValuePair<IEnumerator, IDisposable> StringEnumerator(string str)
	{
		return new KeyValuePair<IEnumerator, IDisposable>(StringOps.StringEnumerator(str), null);
	}

	public static KeyValuePair<IEnumerator, IDisposable> BytesEnumerator(IList<byte> bytes)
	{
		return new KeyValuePair<IEnumerator, IDisposable>(IListOfByteOps.BytesEnumerator(bytes), null);
	}

	public static KeyValuePair<IEnumerator, IDisposable> BytesIntEnumerator(IList<byte> bytes)
	{
		return new KeyValuePair<IEnumerator, IDisposable>(IListOfByteOps.BytesIntEnumerator(bytes), null);
	}

	public static KeyValuePair<IEnumerator, IDisposable> GetEnumeratorFromEnumerable(IEnumerable enumerable)
	{
		IEnumerator enumerator = enumerable.GetEnumerator();
		return new KeyValuePair<IEnumerator, IDisposable>(enumerator, enumerator as IDisposable);
	}

	public static IEnumerable StringEnumerable(string str)
	{
		return StringOps.StringEnumerable(str);
	}

	public static IEnumerable BytesEnumerable(IList<byte> bytes)
	{
		return IListOfByteOps.BytesEnumerable(bytes);
	}

	public static IEnumerable BytesIntEnumerable(IList<byte> bytes)
	{
		return IListOfByteOps.BytesIntEnumerable(bytes);
	}

	public static object SetCurrentException(CodeContext context, Exception clrException)
	{
		object result = PythonExceptions.ToPython(clrException);
		if (clrException is ThreadAbortException ex && ex.ExceptionState is KeyboardInterruptException)
		{
			Thread.ResetAbort();
		}
		RawException = clrException;
		return result;
	}

	public static void BuildExceptionInfo(CodeContext context, Exception clrException)
	{
		object obj = PythonExceptions.ToPython(clrException);
		List<DynamicStackFrame> frameList = clrException.GetFrameList();
		object type = ((!(obj is IPythonObject pythonObject)) ? GetBoundAttr(context, obj, "__class__") : pythonObject.PythonType);
		context.LanguageContext.UpdateExceptionInfo(clrException, type, obj, frameList);
	}

	public static void ClearCurrentException()
	{
		RestoreCurrentException(null);
	}

	public static void ExceptionHandled(CodeContext context)
	{
		PythonContext languageContext = context.LanguageContext;
		languageContext.ExceptionHandled();
	}

	public static Exception SaveCurrentException()
	{
		return RawException;
	}

	public static void RestoreCurrentException(Exception clrException)
	{
		RawException = clrException;
	}

	public static object CheckException(CodeContext context, object exception, object test)
	{
		if (exception is ObjectException ex)
		{
			if (IsSubClass(context, ex.Type, test))
			{
				return ex.Instance;
			}
			return null;
		}
		if (test is PythonType)
		{
			if (IsSubClass(test as PythonType, TypeCache.BaseException))
			{
				if (IsInstance(context, exception, test))
				{
					return exception;
				}
			}
			else if (IsSubClass(test as PythonType, DynamicHelpers.GetPythonTypeFromType(typeof(Exception))))
			{
				Exception ex2 = PythonExceptions.ToClr(exception);
				if (IsInstance(context, ex2, test))
				{
					return ex2;
				}
			}
		}
		else if (test is PythonTuple)
		{
			PythonTuple pythonTuple = test as PythonTuple;
			for (int i = 0; i < pythonTuple.__len__(); i++)
			{
				object obj = CheckException(context, exception, pythonTuple[i]);
				if (obj != null)
				{
					return obj;
				}
			}
		}
		else if (test is OldClass && IsInstance(context, exception, test))
		{
			return exception;
		}
		return null;
	}

	private static TraceBack CreateTraceBack(PythonContext pyContext, Exception e)
	{
		TraceBack traceBack = e.GetTraceBack();
		if (traceBack != null)
		{
			return traceBack;
		}
		IList<DynamicStackFrame> list = e.GetFrameList() ?? ((IList<DynamicStackFrame>)new DynamicStackFrame[0]);
		return CreateTraceBack(e, list, list.Count);
	}

	internal static TraceBack CreateTraceBack(Exception e, IList<DynamicStackFrame> frames, int frameCount)
	{
		TraceBack traceBack = null;
		for (int i = 0; i < frameCount; i++)
		{
			DynamicStackFrame dynamicStackFrame = frames[i];
			string methodName = dynamicStackFrame.GetMethodName();
			if (methodName.IndexOf('#') > 0)
			{
				methodName = methodName.Substring(0, methodName.IndexOf('#'));
			}
			if (dynamicStackFrame is PythonDynamicStackFrame { CodeContext: not null, CodeContext: var codeContext, Code: var code })
			{
				TraceBackFrame fromFrame = new TraceBackFrame(codeContext, codeContext.GlobalDict, codeContext.Dict, code, traceBack?.tb_frame);
				traceBack = new TraceBack(traceBack, fromFrame);
				traceBack.SetLine(dynamicStackFrame.GetFileLineNumber());
			}
		}
		e.SetTraceBack(traceBack);
		return traceBack;
	}

	public static PythonTuple GetExceptionInfo(CodeContext context)
	{
		return GetExceptionInfoLocal(context, RawException);
	}

	public static PythonTuple GetExceptionInfoLocal(CodeContext context, Exception ex)
	{
		if (ex == null)
		{
			object[] items = new object[3];
			return PythonTuple.MakeTuple(items);
		}
		PythonContext languageContext = context.LanguageContext;
		object obj = PythonExceptions.ToPython(ex);
		TraceBack traceBack = CreateTraceBack(languageContext, ex);
		object obj2 = ((!(obj is IPythonObject pythonObject)) ? GetBoundAttr(context, obj, "__class__") : pythonObject.PythonType);
		languageContext.UpdateExceptionInfo(obj2, obj, traceBack);
		return PythonTuple.MakeTuple(obj2, obj, traceBack);
	}

	public static Exception MakeRethrownException(CodeContext context)
	{
		PythonTuple exceptionInfo = GetExceptionInfo(context);
		Exception e = MakeExceptionWorker(context, exceptionInfo[0], exceptionInfo[1], exceptionInfo[2], forRethrow: true);
		return MakeRethrowExceptionWorker(e);
	}

	public static Exception MakeRethrowExceptionWorker(Exception e)
	{
		e.RemoveTraceBack();
		ExceptionHelpers.UpdateForRethrow(e);
		return e;
	}

	public static Exception MakeException(CodeContext context, object type, object value, object traceback)
	{
		Exception ex = MakeExceptionWorker(context, type, value, traceback, forRethrow: false);
		ex.RemoveFrameList();
		return ex;
	}

	private static Exception MakeExceptionWorker(CodeContext context, object type, object value, object traceback, bool forRethrow)
	{
		Exception ex = ((type is PythonExceptions.BaseException) ? PythonExceptions.ToClr(type) : ((type is Exception) ? (type as Exception) : ((type is PythonType pythonType && typeof(PythonExceptions.BaseException).IsAssignableFrom(pythonType.UnderlyingSystemType)) ? PythonExceptions.CreateThrowableForRaise(context, pythonType, value) : ((type is OldClass) ? ((value != null) ? PythonExceptions.CreateThrowableForRaise(context, (OldClass)type, value) : new OldInstanceException((OldInstance)PythonCalls.Call(context, type))) : ((!(type is OldInstance)) ? MakeExceptionTypeError(type) : new OldInstanceException((OldInstance)type))))));
		if (traceback != null)
		{
			if (!forRethrow)
			{
				if (!(traceback is TraceBack traceback2))
				{
					throw TypeError("traceback argument must be a traceback object");
				}
				ex.SetTraceBack(traceback2);
			}
		}
		else
		{
			ex.RemoveTraceBack();
		}
		return ex;
	}

	public static Exception CreateThrowable(PythonType type, params object[] args)
	{
		return PythonExceptions.CreateThrowable(type, args);
	}

	public static string[] GetFunctionSignature(PythonFunction function)
	{
		return new string[1] { function.GetSignatureString() };
	}

	public static PythonDictionary CopyAndVerifyDictionary(PythonFunction function, IDictionary dict)
	{
		foreach (object key in dict.Keys)
		{
			if (!(key is string))
			{
				throw TypeError("{0}() keywords must be strings", function.__name__);
			}
		}
		return new PythonDictionary(dict);
	}

	public static PythonDictionary CopyAndVerifyUserMapping(PythonFunction function, object dict)
	{
		return UserMappingToPythonDictionary(function.Context, dict, function.func_name);
	}

	public static PythonDictionary UserMappingToPythonDictionary(CodeContext context, object dict, string funcName)
	{
		if (!PythonTypeOps.TryInvokeUnaryOperator(context, dict, "keys", out var value))
		{
			throw TypeError("{0}() argument after ** must be a mapping, not {1}", funcName, PythonTypeOps.GetName(dict));
		}
		PythonDictionary pythonDictionary = new PythonDictionary();
		IEnumerator enumerator = GetEnumerator(value);
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			string text = current as string;
			if (text == null && !(current is Extensible<string> { Value: var value2 }))
			{
				throw TypeError("{0}() keywords must be strings, not {0}", funcName, PythonTypeOps.GetName(dict));
			}
			pythonDictionary[current] = GetIndex(context, dict, current);
		}
		return pythonDictionary;
	}

	public static PythonDictionary CopyAndVerifyPythonDictionary(PythonFunction function, PythonDictionary dict)
	{
		if (dict._storage.HasNonStringAttributes())
		{
			throw TypeError("{0}() keywords must be strings", function.__name__);
		}
		return new PythonDictionary(dict);
	}

	public static object ExtractDictionaryArgument(PythonFunction function, string name, int argCnt, PythonDictionary dict)
	{
		if (dict.TryGetValue(name, out var value))
		{
			dict.Remove(name);
			return value;
		}
		throw TypeError("{0}() takes exactly {1} arguments ({2} given)", function.__name__, function.NormalArgumentCount, argCnt);
	}

	public static void AddDictionaryArgument(PythonFunction function, string name, object value, PythonDictionary dict)
	{
		if (dict.ContainsKey(name))
		{
			throw MultipleKeywordArgumentError(function, name);
		}
		dict[name] = value;
	}

	public static void VerifyUnduplicatedByPosition(PythonFunction function, string name, int position, int listlen)
	{
		if (listlen > 0 && listlen > position)
		{
			throw MultipleKeywordArgumentError(function, name);
		}
	}

	public static List CopyAndVerifyParamsList(PythonFunction function, object list)
	{
		return new List(list);
	}

	public static PythonTuple GetOrCopyParamsTuple(PythonFunction function, object input)
	{
		if (input == null)
		{
			throw TypeError("{0}() argument after * must be a sequence, not NoneType", function.func_name);
		}
		if (input.GetType() == typeof(PythonTuple))
		{
			return (PythonTuple)input;
		}
		return PythonTuple.Make(input);
	}

	public static object ExtractParamsArgument(PythonFunction function, int argCnt, List list)
	{
		if (list.__len__() != 0)
		{
			return list.pop(0);
		}
		throw function.BadArgumentError(argCnt);
	}

	public static void AddParamsArguments(List list, params object[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			list.insert(i, args[i]);
		}
	}

	public static object ExtractAnyArgument(PythonFunction function, string name, int argCnt, List list, IDictionary dict)
	{
		if (dict.Contains(name))
		{
			if (list.__len__() != 0)
			{
				throw MultipleKeywordArgumentError(function, name);
			}
			object result = dict[name];
			dict.Remove(name);
			return result;
		}
		if (list.__len__() != 0)
		{
			return list.pop(0);
		}
		if (function.ExpandDictPosition == -1 && dict.Count > 0)
		{
			foreach (string key in dict.Keys)
			{
				bool flag = false;
				string[] argNames = function.ArgNames;
				foreach (string text2 in argNames)
				{
					if (key == text2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw UnexpectedKeywordArgumentError(function, key);
				}
			}
		}
		throw BinderOps.TypeErrorForIncorrectArgumentCount(function.__name__, function.NormalArgumentCount, function.Defaults.Length, argCnt, function.ExpandListPosition != -1, dict.Count > 0);
	}

	public static ArgumentTypeException SimpleTypeError(string message)
	{
		return new TypeErrorException(message);
	}

	public static object GetParamsValueOrDefault(PythonFunction function, int index, List extraArgs)
	{
		if (extraArgs.__len__() > 0)
		{
			return extraArgs.pop(0);
		}
		return function.Defaults[index];
	}

	public static object GetFunctionParameterValue(PythonFunction function, int index, string name, List extraArgs, PythonDictionary dict)
	{
		if (extraArgs != null && extraArgs.__len__() > 0)
		{
			return extraArgs.pop(0);
		}
		if (dict != null && dict.TryRemoveValue(name, out var value))
		{
			return value;
		}
		return function.Defaults[index];
	}

	public static void CheckParamsZero(PythonFunction function, List extraArgs)
	{
		if (extraArgs.__len__() != 0)
		{
			throw function.BadArgumentError(extraArgs.__len__() + function.NormalArgumentCount);
		}
	}

	public static void CheckUserParamsZero(PythonFunction function, object sequence)
	{
		int num = Length(sequence);
		if (num != 0)
		{
			throw function.BadArgumentError(num + function.NormalArgumentCount);
		}
	}

	public static void CheckDictionaryZero(PythonFunction function, IDictionary dict)
	{
		if (dict.Count != 0)
		{
			IDictionaryEnumerator enumerator = dict.GetEnumerator();
			enumerator.MoveNext();
			throw UnexpectedKeywordArgumentError(function, (string)enumerator.Key);
		}
	}

	public static bool CheckDictionaryMembers(PythonDictionary dict, string[] names)
	{
		if (dict.Count != names.Length)
		{
			return false;
		}
		foreach (string key in names)
		{
			if (!dict.ContainsKey(key))
			{
				return false;
			}
		}
		return true;
	}

	public static object PythonFunctionGetMember(PythonFunction function, string name)
	{
		if (function._dict != null && function._dict.TryGetValue(name, out var value))
		{
			return value;
		}
		return OperationFailed.Value;
	}

	public static object PythonFunctionSetMember(PythonFunction function, string name, object value)
	{
		return function.__dict__[name] = value;
	}

	public static void PythonFunctionDeleteDict()
	{
		throw TypeError("function's dictionary may not be deleted");
	}

	public static void PythonFunctionDeleteDoc(PythonFunction function)
	{
		function.__doc__ = null;
	}

	public static void PythonFunctionDeleteDefaults(PythonFunction function)
	{
		function.__defaults__ = null;
	}

	public static bool PythonFunctionDeleteMember(PythonFunction function, string name)
	{
		if (function._dict == null)
		{
			return false;
		}
		return function._dict.Remove(name);
	}

	public static object[] InitializeUserTypeSlots(PythonType type)
	{
		if (type.SlotCount == 0)
		{
			return null;
		}
		object[] array = new object[type.SlotCount + 1];
		for (int i = 0; i < array.Length - 1; i++)
		{
			array[i] = Uninitialized.Instance;
		}
		return array;
	}

	public static bool IsClsVisible(CodeContext context)
	{
		return context.ModuleContext.ShowCls;
	}

	public static object GetInitMember(CodeContext context, PythonType type, object instance)
	{
		type.TryGetNonCustomBoundMember(context, instance, "__init__", out var value);
		return value;
	}

	public static object GetInitSlotMember(CodeContext context, PythonType type, PythonTypeSlot slot, object instance)
	{
		if (!slot.TryGetValue(context, instance, type, out var value))
		{
			throw TypeError("bad __init__");
		}
		return value;
	}

	public static object GetMixedMember(CodeContext context, PythonType type, object instance, string name)
	{
		foreach (PythonType item in type.ResolutionOrder)
		{
			PythonTypeSlot slot;
			if (item.IsOldClass)
			{
				OldClass oldClass = (OldClass)ToPythonType(item);
				if (oldClass._dict._storage.TryGetValue(name, out var value))
				{
					if (instance != null)
					{
						return oldClass.GetOldStyleDescriptor(context, value, instance, oldClass);
					}
					return value;
				}
			}
			else if (item.TryLookupSlot(context, name, out slot))
			{
				if (slot.TryGetValue(context, instance, type, out var value2))
				{
					return value2;
				}
				return slot;
			}
		}
		throw AttributeErrorForMissingAttribute(type, name);
	}

	public static bool IsNumericObject(object value)
	{
		if (!(value is int) && !(value is Extensible<int>) && !(value is BigInteger) && !(value is Extensible<BigInteger>))
		{
			return value is bool;
		}
		return true;
	}

	internal static bool IsNumericType(Type t)
	{
		if (!IsNonExtensibleNumericType(t) && !t.IsSubclassOf(typeof(Extensible<int>)))
		{
			return t.IsSubclassOf(typeof(Extensible<BigInteger>));
		}
		return true;
	}

	internal static bool IsNonExtensibleNumericType(Type t)
	{
		if (!(t == typeof(int)) && !(t == typeof(bool)))
		{
			return t == typeof(BigInteger);
		}
		return true;
	}

	public static int NormalizeBigInteger(object self, BigInteger bi, ref int? length)
	{
		int ret;
		if (bi < BigInteger.Zero)
		{
			GetLengthOnce(self, ref length);
			if (bi.AsInt32(out ret))
			{
				return ret + length.Value;
			}
			return -1;
		}
		if (bi.AsInt32(out ret))
		{
			return ret;
		}
		return int.MaxValue;
	}

	public static int GetLengthOnce(object self, ref int? length)
	{
		if (length.HasValue)
		{
			return length.Value;
		}
		length = Length(self);
		return length.Value;
	}

	public static ReflectedEvent.BoundEvent MakeBoundEvent(ReflectedEvent eventObj, object instance, Type type)
	{
		return new ReflectedEvent.BoundEvent(eventObj, instance, DynamicHelpers.GetPythonTypeFromType(type));
	}

	public static bool CheckTypeVersion(object o, int version)
	{
		if (!(o is IPythonObject pythonObject))
		{
			return false;
		}
		return pythonObject.PythonType.Version == version;
	}

	public static bool CheckSpecificTypeVersion(PythonType type, int version)
	{
		return type.Version == version;
	}

	internal static MethodInfo GetConversionHelper(string name, ConversionResultKind resultKind)
	{
		switch (resultKind)
		{
		case ConversionResultKind.ImplicitCast:
		case ConversionResultKind.ExplicitCast:
			return typeof(PythonOps).GetMethod("Throwing" + name);
		case ConversionResultKind.ImplicitTry:
		case ConversionResultKind.ExplicitTry:
			return typeof(PythonOps).GetMethod("NonThrowing" + name);
		default:
			throw new InvalidOperationException();
		}
	}

	public static IEnumerable OldInstanceConvertToIEnumerableNonThrowing(CodeContext context, OldInstance self)
	{
		if (self.TryGetBoundCustomMember(context, "__iter__", out var value))
		{
			return CreatePythonEnumerable(self);
		}
		if (self.TryGetBoundCustomMember(context, "__getitem__", out value))
		{
			return CreateItemEnumerable(value, PythonContext.GetContext(context).GetItemCallSite);
		}
		return null;
	}

	public static IEnumerable OldInstanceConvertToIEnumerableThrowing(CodeContext context, OldInstance self)
	{
		IEnumerable enumerable = OldInstanceConvertToIEnumerableNonThrowing(context, self);
		if (enumerable == null)
		{
			throw TypeErrorForTypeMismatch("IEnumerable", self);
		}
		return enumerable;
	}

	public static IEnumerable<T> OldInstanceConvertToIEnumerableOfTNonThrowing<T>(CodeContext context, OldInstance self)
	{
		if (self.TryGetBoundCustomMember(context, "__iter__", out var value))
		{
			return new IEnumerableOfTWrapper<T>(CreatePythonEnumerable(self));
		}
		if (self.TryGetBoundCustomMember(context, "__getitem__", out value))
		{
			return new IEnumerableOfTWrapper<T>(CreateItemEnumerable(value, PythonContext.GetContext(context).GetItemCallSite));
		}
		return null;
	}

	public static IEnumerable<T> OldInstanceConvertToIEnumerableOfTThrowing<T>(CodeContext context, OldInstance self)
	{
		IEnumerable<T> enumerable = OldInstanceConvertToIEnumerableOfTNonThrowing<T>(context, self);
		if (enumerable == null)
		{
			throw TypeErrorForTypeMismatch("IEnumerable[T]", self);
		}
		return enumerable;
	}

	public static IEnumerator OldInstanceConvertToIEnumeratorNonThrowing(CodeContext context, OldInstance self)
	{
		if (self.TryGetBoundCustomMember(context, "__iter__", out var value))
		{
			return CreatePythonEnumerator(self);
		}
		if (self.TryGetBoundCustomMember(context, "__getitem__", out value))
		{
			return CreateItemEnumerator(value, PythonContext.GetContext(context).GetItemCallSite);
		}
		return null;
	}

	public static IEnumerator OldInstanceConvertToIEnumeratorThrowing(CodeContext context, OldInstance self)
	{
		IEnumerator enumerator = OldInstanceConvertToIEnumeratorNonThrowing(context, self);
		if (enumerator == null)
		{
			throw TypeErrorForTypeMismatch("IEnumerator", self);
		}
		return enumerator;
	}

	public static bool? OldInstanceConvertToBoolNonThrowing(CodeContext context, OldInstance oi)
	{
		int result;
		if (oi.TryGetBoundCustomMember(context, "__nonzero__", out var value))
		{
			object obj = NonThrowingConvertToNonZero(PythonCalls.Call(context, value));
			if (obj is int)
			{
				return (int)obj != 0;
			}
			if (obj is bool)
			{
				return (bool)obj;
			}
		}
		else if (oi.TryGetBoundCustomMember(context, "__len__", out value) && Converter.TryConvertToInt32(PythonCalls.Call(context, value), out result))
		{
			return result != 0;
		}
		return null;
	}

	public static object OldInstanceConvertToBoolThrowing(CodeContext context, OldInstance oi)
	{
		if (oi.TryGetBoundCustomMember(context, "__nonzero__", out var value))
		{
			return ThrowingConvertToNonZero(PythonCalls.Call(context, value));
		}
		if (oi.TryGetBoundCustomMember(context, "__len__", out value))
		{
			return PythonContext.GetContext(context).ConvertToInt32(PythonCalls.Call(context, value)) != 0;
		}
		return null;
	}

	public static object OldInstanceConvertNonThrowing(CodeContext context, OldInstance oi, string conversion)
	{
		if (oi.TryGetBoundCustomMember(context, conversion, out var value))
		{
			switch (conversion)
			{
			case "__int__":
				return NonThrowingConvertToInt(PythonCalls.Call(context, value));
			case "__long__":
				return NonThrowingConvertToLong(PythonCalls.Call(context, value));
			case "__float__":
				return NonThrowingConvertToFloat(PythonCalls.Call(context, value));
			case "__complex__":
				return NonThrowingConvertToComplex(PythonCalls.Call(context, value));
			case "__str__":
				return NonThrowingConvertToString(PythonCalls.Call(context, value));
			}
		}
		else if (conversion == "__complex__")
		{
			object obj = OldInstanceConvertNonThrowing(context, oi, "__float__");
			if (obj == null)
			{
				return null;
			}
			return Converter.ConvertToComplex(obj);
		}
		return null;
	}

	public static object OldInstanceConvertThrowing(CodeContext context, OldInstance oi, string conversion)
	{
		if (oi.TryGetBoundCustomMember(context, conversion, out var value))
		{
			switch (conversion)
			{
			case "__int__":
				return ThrowingConvertToInt(PythonCalls.Call(context, value));
			case "__long__":
				return ThrowingConvertToLong(PythonCalls.Call(context, value));
			case "__float__":
				return ThrowingConvertToFloat(PythonCalls.Call(context, value));
			case "__complex__":
				return ThrowingConvertToComplex(PythonCalls.Call(context, value));
			case "__str__":
				return ThrowingConvertToString(PythonCalls.Call(context, value));
			}
		}
		else if (conversion == "__complex__")
		{
			return OldInstanceConvertThrowing(context, oi, "__float__");
		}
		return null;
	}

	public static object ConvertFloatToComplex(object value)
	{
		if (value == null)
		{
			return null;
		}
		double real = (double)value;
		return new Complex(real, 0.0);
	}

	internal static bool CheckingConvertToInt(object value)
	{
		if (!(value is int) && !(value is BigInteger) && !(value is Extensible<int>))
		{
			return value is Extensible<BigInteger>;
		}
		return true;
	}

	internal static bool CheckingConvertToLong(object value)
	{
		return CheckingConvertToInt(value);
	}

	internal static bool CheckingConvertToFloat(object value)
	{
		if (!(value is double))
		{
			if (value != null)
			{
				return value is Extensible<double>;
			}
			return false;
		}
		return true;
	}

	internal static bool CheckingConvertToComplex(object value)
	{
		if (!(value is Complex) && !(value is Extensible<Complex>) && !CheckingConvertToInt(value))
		{
			return CheckingConvertToFloat(value);
		}
		return true;
	}

	internal static bool CheckingConvertToString(object value)
	{
		if (!(value is string))
		{
			return value is Extensible<string>;
		}
		return true;
	}

	public static bool CheckingConvertToNonZero(object value)
	{
		if (!(value is bool))
		{
			return value is int;
		}
		return true;
	}

	public static object NonThrowingConvertToInt(object value)
	{
		if (!CheckingConvertToInt(value))
		{
			return null;
		}
		return value;
	}

	public static object NonThrowingConvertToLong(object value)
	{
		if (!CheckingConvertToInt(value))
		{
			return null;
		}
		return value;
	}

	public static object NonThrowingConvertToFloat(object value)
	{
		if (!CheckingConvertToFloat(value))
		{
			return null;
		}
		return value;
	}

	public static object NonThrowingConvertToComplex(object value)
	{
		if (!CheckingConvertToComplex(value))
		{
			return null;
		}
		return value;
	}

	public static object NonThrowingConvertToString(object value)
	{
		if (!CheckingConvertToString(value))
		{
			return null;
		}
		return value;
	}

	public static object NonThrowingConvertToNonZero(object value)
	{
		if (!CheckingConvertToNonZero(value))
		{
			return null;
		}
		return value;
	}

	public static object ThrowingConvertToInt(object value)
	{
		if (!CheckingConvertToInt(value))
		{
			throw TypeError(" __int__ returned non-int (type {0})", PythonTypeOps.GetName(value));
		}
		return value;
	}

	public static object ThrowingConvertToFloat(object value)
	{
		if (!CheckingConvertToFloat(value))
		{
			throw TypeError(" __float__ returned non-float (type {0})", PythonTypeOps.GetName(value));
		}
		return value;
	}

	public static object ThrowingConvertToComplex(object value)
	{
		if (!CheckingConvertToComplex(value))
		{
			throw TypeError(" __complex__ returned non-complex (type {0})", PythonTypeOps.GetName(value));
		}
		return value;
	}

	public static object ThrowingConvertToLong(object value)
	{
		if (!CheckingConvertToComplex(value))
		{
			throw TypeError(" __long__ returned non-long (type {0})", PythonTypeOps.GetName(value));
		}
		return value;
	}

	public static object ThrowingConvertToString(object value)
	{
		if (!CheckingConvertToString(value))
		{
			throw TypeError(" __str__ returned non-str (type {0})", PythonTypeOps.GetName(value));
		}
		return value;
	}

	public static bool ThrowingConvertToNonZero(object value)
	{
		if (!CheckingConvertToNonZero(value))
		{
			throw TypeError("__nonzero__ should return bool or int, returned {0}", PythonTypeOps.GetName(value));
		}
		if (value is bool)
		{
			return (bool)value;
		}
		return (int)value != 0;
	}

	public static bool SlotTryGetBoundValue(CodeContext context, PythonTypeSlot slot, object instance, PythonType owner, out object value)
	{
		return slot.TryGetValue(context, instance, owner, out value);
	}

	public static bool SlotTryGetValue(CodeContext context, PythonTypeSlot slot, object instance, PythonType owner, out object value)
	{
		return slot.TryGetValue(context, instance, owner, out value);
	}

	public static object SlotGetValue(CodeContext context, PythonTypeSlot slot, object instance, PythonType owner)
	{
		if (!slot.TryGetValue(context, instance, owner, out var value))
		{
			throw new InvalidOperationException();
		}
		return value;
	}

	public static bool SlotTrySetValue(CodeContext context, PythonTypeSlot slot, object instance, PythonType owner, object value)
	{
		return slot.TrySetValue(context, instance, owner, value);
	}

	public static object SlotSetValue(CodeContext context, PythonTypeSlot slot, object instance, PythonType owner, object value)
	{
		if (!slot.TrySetValue(context, instance, owner, value))
		{
			throw new InvalidOperationException();
		}
		return value;
	}

	public static bool SlotTryDeleteValue(CodeContext context, PythonTypeSlot slot, object instance, PythonType owner)
	{
		return slot.TryDeleteValue(context, instance, owner);
	}

	public static BuiltinFunction MakeBoundBuiltinFunction(BuiltinFunction function, object target)
	{
		return function.BindToInstance(target);
	}

	public static object GetBuiltinFunctionSelf(BuiltinFunction function)
	{
		return function.BindingSelf;
	}

	public static bool TestBoundBuiltinFunction(BuiltinFunction function, object data)
	{
		if (function.IsUnbound)
		{
			return false;
		}
		return function.TestData(data);
	}

	public static BuiltinFunction GetBuiltinMethodDescriptorTemplate(BuiltinMethodDescriptor descriptor)
	{
		return descriptor.Template;
	}

	public static int GetTypeVersion(PythonType type)
	{
		return type.Version;
	}

	public static bool TryResolveTypeSlot(CodeContext context, PythonType type, string name, out PythonTypeSlot slot)
	{
		return type.TryResolveSlot(context, name, out slot);
	}

	public static T[] ConvertTupleToArray<T>(PythonTuple tuple)
	{
		T[] array = new T[tuple.__len__()];
		for (int i = 0; i < tuple.__len__(); i++)
		{
			try
			{
				array[i] = (T)tuple[i];
			}
			catch (InvalidCastException)
			{
				array[i] = Converter.Convert<T>(tuple[i]);
			}
		}
		return array;
	}

	public static PythonGenerator MakeGenerator(PythonFunction function, MutableTuple data, object generatorCode)
	{
		Func<MutableTuple, object> func = generatorCode as Func<MutableTuple, object>;
		if (func == null)
		{
			func = ((LazyCode<Func<MutableTuple, object>>)generatorCode).EnsureDelegate();
		}
		return new PythonGenerator(function, func, data);
	}

	public static object MakeGeneratorExpression(object function, object input)
	{
		PythonFunction pythonFunction = (PythonFunction)function;
		return ((Func<PythonFunction, object, object>)pythonFunction.func_code.Target)(pythonFunction, input);
	}

	public static FunctionCode MakeFunctionCode(CodeContext context, string name, string documentation, string[] argNames, FunctionAttributes flags, int startIndex, int endIndex, string path, Delegate code, string[] freeVars, string[] names, string[] cellVars, string[] varNames, int localCount)
	{
		SerializedScopeStatement scope = new SerializedScopeStatement(name, argNames, flags, startIndex, endIndex, path, freeVars, names, cellVars, varNames);
		return new FunctionCode(context.LanguageContext, code, scope, documentation, localCount);
	}

	[NoSideEffects]
	public static object MakeFunction(CodeContext context, FunctionCode funcInfo, object modName, object[] defaults)
	{
		return new PythonFunction(context, funcInfo, modName, defaults, null);
	}

	[NoSideEffects]
	public static object MakeFunctionDebug(CodeContext context, FunctionCode funcInfo, object modName, object[] defaults, Delegate target)
	{
		funcInfo.SetDebugTarget(PythonContext.GetContext(context), target);
		return new PythonFunction(context, funcInfo, modName, defaults, null);
	}

	public static CodeContext FunctionGetContext(PythonFunction func)
	{
		return func.Context;
	}

	public static object FunctionGetDefaultValue(PythonFunction func, int index)
	{
		return func.Defaults[index];
	}

	public static int FunctionGetCompatibility(PythonFunction func)
	{
		return func.FunctionCompatibility;
	}

	public static int FunctionGetID(PythonFunction func)
	{
		return func.FunctionID;
	}

	public static Delegate FunctionGetTarget(PythonFunction func)
	{
		return func.func_code.Target;
	}

	public static Delegate FunctionGetLightThrowTarget(PythonFunction func)
	{
		return func.func_code.LightThrowTarget;
	}

	public static void FunctionPushFrame(PythonContext context)
	{
		if (PythonFunction.AddRecursionDepth(1) > context.RecursionLimit)
		{
			throw RuntimeError("maximum recursion depth exceeded");
		}
	}

	public static void FunctionPushFrameCodeContext(CodeContext context)
	{
		FunctionPushFrame(PythonContext.GetContext(context));
	}

	public static void FunctionPopFrame()
	{
		PythonFunction.AddRecursionDepth(-1);
	}

	public static object ReturnConversionResult(object value)
	{
		if (value is PythonTuple pythonTuple)
		{
			return pythonTuple[0];
		}
		return NotImplementedType.Value;
	}

	public static T ConvertFromObject<T>(object obj)
	{
		Type typeFromHandle = typeof(T);
		MethodInfo fastConvertMethod = PythonBinder.GetFastConvertMethod(typeFromHandle);
		object obj2 = ((fastConvertMethod != null) ? fastConvertMethod.Invoke(null, new object[1] { obj }) : ((!typeof(Delegate).IsAssignableFrom(typeFromHandle)) ? obj : Converter.ConvertToDelegate(obj, typeFromHandle)));
		return (T)obj2;
	}

	public static DynamicMetaObjectBinder MakeComplexCallAction(int count, bool list, string[] keywords)
	{
		Argument[] array = CompilerHelpers.MakeRepeatedArray(Argument.Simple, count + keywords.Length);
		if (list)
		{
			ref Argument reference = ref array[checked(count - 1)];
			reference = new Argument(ArgumentType.List);
		}
		for (int i = 0; i < keywords.Length; i++)
		{
			ref Argument reference2 = ref array[count + i];
			reference2 = new Argument(keywords[i]);
		}
		return DefaultContext.DefaultPythonContext.Invoke(new CallSignature(array));
	}

	public static DynamicMetaObjectBinder MakeSimpleCallAction(int count)
	{
		return DefaultContext.DefaultPythonContext.Invoke(new CallSignature(CompilerHelpers.MakeRepeatedArray(Argument.Simple, count)));
	}

	public static PythonTuple ValidateCoerceResult(object coerceResult)
	{
		if (coerceResult == null || coerceResult == NotImplementedType.Value)
		{
			return null;
		}
		if (!(coerceResult is PythonTuple result))
		{
			throw TypeError("coercion should return None, NotImplemented, or 2-tuple, got {0}", PythonTypeOps.GetName(coerceResult));
		}
		return result;
	}

	public static object GetCoerceResultOne(PythonTuple coerceResult)
	{
		return coerceResult._data[0];
	}

	public static object GetCoerceResultTwo(PythonTuple coerceResult)
	{
		return coerceResult._data[1];
	}

	public static object MethodCheckSelf(CodeContext context, Method method, object self)
	{
		return method.CheckSelf(context, self);
	}

	[LightThrowing]
	public static object GeneratorCheckThrowableAndReturnSendValue(object self)
	{
		return ((PythonGenerator)self).CheckThrowableAndReturnSendValue();
	}

	public static ItemEnumerable CreateItemEnumerable(object callable, CallSite<Func<CallSite, CodeContext, object, int, object>> site)
	{
		return new ItemEnumerable(callable, site);
	}

	public static DictionaryKeyEnumerator MakeDictionaryKeyEnumerator(PythonDictionary dict)
	{
		return new DictionaryKeyEnumerator(dict._storage);
	}

	public static IEnumerable CreatePythonEnumerable(object baseObject)
	{
		return PythonEnumerable.Create(baseObject);
	}

	public static IEnumerator CreateItemEnumerator(object callable, CallSite<Func<CallSite, CodeContext, object, int, object>> site)
	{
		return new ItemEnumerator(callable, site);
	}

	public static IEnumerator CreatePythonEnumerator(object baseObject)
	{
		return PythonEnumerator.Create(baseObject);
	}

	public static bool ContainsFromEnumerable(CodeContext context, object enumerable, object value)
	{
		IEnumerator enumerator = enumerable as IEnumerator;
		if (enumerator == null)
		{
			enumerator = ((!(enumerable is IEnumerable enumerable2)) ? Converter.ConvertToIEnumerator(enumerable) : enumerable2.GetEnumerator());
		}
		while (enumerator.MoveNext())
		{
			if (EqualRetBool(context, enumerator.Current, value))
			{
				return true;
			}
		}
		return false;
	}

	public static object PythonTypeGetMember(CodeContext context, PythonType type, object instance, string name)
	{
		return type.GetMember(context, instance, name);
	}

	[NoSideEffects]
	public static object CheckUninitialized(object value, string name)
	{
		if (value == Uninitialized.Instance)
		{
			throw new UnboundLocalException($"Local variable '{name}' referenced before assignment.");
		}
		return value;
	}

	public static PythonDictionary OldClassGetDictionary(OldClass klass)
	{
		return klass._dict;
	}

	public static string OldClassGetName(OldClass klass)
	{
		return klass.Name;
	}

	public static bool OldInstanceIsCallable(CodeContext context, OldInstance self)
	{
		object value;
		return self.TryGetBoundCustomMember(context, "__call__", out value);
	}

	public static object OldClassCheckCallError(OldClass self, object dictionary, object list)
	{
		if ((dictionary != null && Length(dictionary) != 0) || (list != null && Length(list) != 0))
		{
			return OldClass.MakeCallError();
		}
		return null;
	}

	public static object OldClassSetBases(OldClass oc, object value)
	{
		oc.SetBases(value);
		return value;
	}

	public static object OldClassSetName(OldClass oc, object value)
	{
		oc.SetName(value);
		return value;
	}

	public static object OldClassSetDictionary(OldClass oc, object value)
	{
		oc.SetDictionary(value);
		return value;
	}

	public static object OldClassSetNameHelper(OldClass oc, string name, object value)
	{
		oc.SetNameHelper(name, value);
		return value;
	}

	public static object OldClassTryLookupInit(OldClass oc, object inst)
	{
		if (oc.TryLookupInit(inst, out var ret))
		{
			return ret;
		}
		return OperationFailed.Value;
	}

	public static object OldClassMakeCallError(OldClass oc)
	{
		return OldClass.MakeCallError();
	}

	public static PythonTuple OldClassGetBaseClasses(OldClass oc)
	{
		return PythonTuple.MakeTuple(oc.BaseClasses.ToArray());
	}

	public static void OldClassDictionaryIsPublic(OldClass oc)
	{
		oc.DictionaryIsPublic();
	}

	public static object OldClassTryLookupValue(CodeContext context, OldClass oc, string name)
	{
		if (oc.TryLookupValue(context, name, out var value))
		{
			return value;
		}
		return OperationFailed.Value;
	}

	public static object OldClassLookupValue(CodeContext context, OldClass oc, string name)
	{
		return oc.LookupValue(context, name);
	}

	public static object OldInstanceGetOptimizedDictionary(OldInstance instance, int keyVersion)
	{
		if (!(instance.Dictionary._storage is CustomInstanceDictionaryStorage customInstanceDictionaryStorage) || instance._class.HasSetAttr || customInstanceDictionaryStorage.KeyVersion != keyVersion)
		{
			return null;
		}
		return customInstanceDictionaryStorage;
	}

	public static object OldInstanceDictionaryGetValueHelper(object dict, int index, object oldInstance)
	{
		return ((CustomInstanceDictionaryStorage)dict).GetValueHelper(index, oldInstance);
	}

	public static bool TryOldInstanceDictionaryGetValueHelper(object dict, int index, object oldInstance, out object res)
	{
		return ((CustomInstanceDictionaryStorage)dict).TryGetValueHelper(index, oldInstance, out res);
	}

	public static object OldInstanceGetBoundMember(CodeContext context, OldInstance instance, string name)
	{
		return instance.GetBoundMember(context, name);
	}

	public static object OldInstanceDictionarySetExtraValue(object dict, int index, object value)
	{
		((CustomInstanceDictionaryStorage)dict).SetExtraValue(index, value);
		return value;
	}

	public static object OldClassDeleteMember(CodeContext context, OldClass self, string name)
	{
		self.DeleteCustomMember(context, name);
		return null;
	}

	public static bool OldClassTryLookupOneSlot(PythonType type, OldClass self, string name, out object value)
	{
		return self.TryLookupOneSlot(type, name, out value);
	}

	public static object OldInstanceTryGetBoundCustomMember(CodeContext context, OldInstance self, string name)
	{
		if (self.TryGetBoundCustomMember(context, name, out var value))
		{
			return value;
		}
		return OperationFailed.Value;
	}

	public static object OldInstanceSetCustomMember(CodeContext context, OldInstance self, string name, object value)
	{
		self.SetCustomMember(context, name, value);
		return value;
	}

	public static object OldInstanceDeleteCustomMember(CodeContext context, OldInstance self, string name)
	{
		self.DeleteCustomMember(context, name);
		return null;
	}

	public static object PythonTypeSetCustomMember(CodeContext context, PythonType self, string name, object value)
	{
		self.SetCustomMember(context, name, value);
		return value;
	}

	public static object PythonTypeDeleteCustomMember(CodeContext context, PythonType self, string name)
	{
		self.DeleteCustomMember(context, name);
		return null;
	}

	public static bool IsPythonType(PythonType type)
	{
		return type.IsPythonType;
	}

	public static object PublishModule(CodeContext context, string name)
	{
		object value = null;
		context.LanguageContext.SystemStateModules.TryGetValue(name, out value);
		PythonModule module = ((PythonScopeExtension)context.GlobalScope.GetExtension(context.LanguageContext.ContextId)).Module;
		context.LanguageContext.SystemStateModules[name] = module;
		return value;
	}

	public static void RemoveModule(CodeContext context, string name, object oldValue)
	{
		if (oldValue != null)
		{
			PythonContext.GetContext(context).SystemStateModules[name] = oldValue;
		}
		else
		{
			PythonContext.GetContext(context).SystemStateModules.Remove(name);
		}
	}

	public static void ListAddForComprehension(List l, object o)
	{
		l.AddNoLock(o);
	}

	public static void SetAddForComprehension(SetCollection s, object o)
	{
		s._items.AddNoLock(o);
	}

	public static void DictAddForComprehension(PythonDictionary d, object k, object v)
	{
		d._storage.AddNoLock(ref d._storage, k, v);
	}

	public static void ModuleStarted(CodeContext context, ModuleOptions features)
	{
		context.ModuleContext.Features |= features;
	}

	public static void Warn(CodeContext context, PythonType category, string message, params object[] args)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		object warningsModule = context2.GetWarningsModule();
		object obj = null;
		if (warningsModule != null)
		{
			obj = GetBoundAttr(context, warningsModule, "warn");
		}
		message = FormatWarning(message, args);
		if (obj == null)
		{
			PrintWithDest(context, context2.SystemStandardError, "warning: " + category.Name + ": " + message);
			return;
		}
		CallWithContext(context, obj, message, category);
	}

	public static void ShowWarning(CodeContext context, PythonType category, string message, string filename, int lineNo)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		object warningsModule = context2.GetWarningsModule();
		object obj = null;
		if (warningsModule != null)
		{
			obj = GetBoundAttr(context, warningsModule, "showwarning");
		}
		if (obj == null)
		{
			PrintWithDestNoNewline(context, context2.SystemStandardError, $"{filename}:{lineNo}: {category.Name}: {message}\n");
		}
		else
		{
			CallWithContext(context, obj, message, category, filename ?? "", lineNo);
		}
	}

	private static string FormatWarning(string message, object[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			args[i] = ToString(args[i]);
		}
		message = string.Format(message, args);
		return message;
	}

	private static bool IsPrimitiveNumber(object o)
	{
		if (!IsNumericObject(o) && !(o is Complex) && !(o is double) && !(o is Extensible<Complex>))
		{
			return o is Extensible<double>;
		}
		return true;
	}

	public static void WarnDivision(CodeContext context, PythonDivisionOptions options, object self, object other)
	{
		if (options == PythonDivisionOptions.WarnAll)
		{
			if (IsPrimitiveNumber(self) && IsPrimitiveNumber(other))
			{
				if (self is Complex || other is Complex || self is Extensible<Complex> || other is Extensible<Complex>)
				{
					Warn(context, PythonExceptions.DeprecationWarning, "classic complex division");
				}
				else if (self is double || other is double || self is Extensible<double> || other is Extensible<double>)
				{
					Warn(context, PythonExceptions.DeprecationWarning, "classic float division");
				}
				else
				{
					WarnDivisionInts(context, self, other);
				}
			}
		}
		else if (IsNumericObject(self) && IsNumericObject(other))
		{
			WarnDivisionInts(context, self, other);
		}
	}

	private static void WarnDivisionInts(CodeContext context, object self, object other)
	{
		if (self is BigInteger || other is BigInteger || self is Extensible<BigInteger> || other is Extensible<BigInteger>)
		{
			Warn(context, PythonExceptions.DeprecationWarning, "classic long division");
		}
		else
		{
			Warn(context, PythonExceptions.DeprecationWarning, "classic int division");
		}
	}

	public static DynamicMetaObjectBinder MakeComboAction(CodeContext context, DynamicMetaObjectBinder opBinder, DynamicMetaObjectBinder convBinder)
	{
		return PythonContext.GetContext(context).BinaryOperationRetType((PythonBinaryOperationBinder)opBinder, (PythonConversionBinder)convBinder);
	}

	public static DynamicMetaObjectBinder MakeInvokeAction(CodeContext context, CallSignature signature)
	{
		return PythonContext.GetContext(context).Invoke(signature);
	}

	public static DynamicMetaObjectBinder MakeGetAction(CodeContext context, string name, bool isNoThrow)
	{
		return PythonContext.GetContext(context).GetMember(name);
	}

	public static DynamicMetaObjectBinder MakeCompatGetAction(CodeContext context, string name)
	{
		return PythonContext.GetContext(context).CompatGetMember(name, isNoThrow: false);
	}

	public static DynamicMetaObjectBinder MakeCompatInvokeAction(CodeContext context, CallInfo callInfo)
	{
		return PythonContext.GetContext(context).CompatInvoke(callInfo);
	}

	public static DynamicMetaObjectBinder MakeCompatConvertAction(CodeContext context, Type toType, bool isExplicit)
	{
		return PythonContext.GetContext(context).Convert(toType, isExplicit ? ConversionResultKind.ExplicitCast : ConversionResultKind.ImplicitCast).CompatBinder;
	}

	public static DynamicMetaObjectBinder MakeSetAction(CodeContext context, string name)
	{
		return PythonContext.GetContext(context).SetMember(name);
	}

	public static DynamicMetaObjectBinder MakeDeleteAction(CodeContext context, string name)
	{
		return PythonContext.GetContext(context).DeleteMember(name);
	}

	public static DynamicMetaObjectBinder MakeConversionAction(CodeContext context, Type type, ConversionResultKind kind)
	{
		return PythonContext.GetContext(context).Convert(type, kind);
	}

	public static DynamicMetaObjectBinder MakeTryConversionAction(CodeContext context, Type type, ConversionResultKind kind)
	{
		return PythonContext.GetContext(context).Convert(type, kind);
	}

	public static DynamicMetaObjectBinder MakeOperationAction(CodeContext context, int operationName)
	{
		return PythonContext.GetContext(context).Operation((PythonOperationKind)operationName);
	}

	public static DynamicMetaObjectBinder MakeUnaryOperationAction(CodeContext context, ExpressionType expressionType)
	{
		return PythonContext.GetContext(context).UnaryOperation(expressionType);
	}

	public static DynamicMetaObjectBinder MakeBinaryOperationAction(CodeContext context, ExpressionType expressionType)
	{
		return PythonContext.GetContext(context).BinaryOperation(expressionType);
	}

	public static DynamicMetaObjectBinder MakeGetIndexAction(CodeContext context, int argCount)
	{
		return PythonContext.GetContext(context).GetIndex(argCount);
	}

	public static DynamicMetaObjectBinder MakeSetIndexAction(CodeContext context, int argCount)
	{
		return PythonContext.GetContext(context).SetIndex(argCount);
	}

	public static DynamicMetaObjectBinder MakeDeleteIndexAction(CodeContext context, int argCount)
	{
		return PythonContext.GetContext(context).DeleteIndex(argCount);
	}

	public static DynamicMetaObjectBinder MakeGetSliceBinder(CodeContext context)
	{
		return PythonContext.GetContext(context).GetSlice;
	}

	public static DynamicMetaObjectBinder MakeSetSliceBinder(CodeContext context)
	{
		return PythonContext.GetContext(context).SetSliceBinder;
	}

	public static DynamicMetaObjectBinder MakeDeleteSliceBinder(CodeContext context)
	{
		return PythonContext.GetContext(context).DeleteSlice;
	}

	public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
	{
		return AppDomain.CurrentDomain.DefineDynamicAssembly(name, access);
	}

	public static Type MakeNewCustomDelegate(Type[] types)
	{
		return MakeNewCustomDelegate(types, null);
	}

	public static Type MakeNewCustomDelegate(Type[] types, CallingConvention? callingConvention)
	{
		Type returnType = types[types.Length - 1];
		Type[] parameterTypes = ArrayUtils.RemoveLast(types);
		TypeBuilder typeBuilder = Snippets.Shared.DefineDelegateType("Delegate" + types.Length);
		typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask, returnType, parameterTypes).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
		if (callingConvention.HasValue)
		{
			typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(UnmanagedFunctionPointerAttribute).GetConstructor(new Type[1] { typeof(CallingConvention) }), new object[1] { callingConvention }));
		}
		return typeBuilder.CreateType();
	}

	public static int InitializeModule(Assembly precompiled, string main, string[] references)
	{
		return InitializeModuleEx(precompiled, main, references, ignoreEnvVars: false);
	}

	public static int InitializeModuleEx(Assembly precompiled, string main, string[] references, bool ignoreEnvVars)
	{
		ContractUtils.RequiresNotNull(precompiled, "precompiled");
		ContractUtils.RequiresNotNull(main, "main");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["Arguments"] = Environment.GetCommandLineArgs();
		ScriptEngine engine = Python.CreateEngine(dictionary);
		PythonContext pythonContext = (PythonContext)HostingHelpers.GetLanguageContext(engine);
		if (!ignoreEnvVars)
		{
			int count = pythonContext.PythonOptions.SearchPaths.Count;
			string environmentVariable = Environment.GetEnvironmentVariable("IRONPYTHONPATH");
			if (environmentVariable != null && environmentVariable.Length > 0)
			{
				string[] array = environmentVariable.Split(Path.PathSeparator);
				string[] array2 = array;
				foreach (string directory in array2)
				{
					pythonContext.InsertIntoPath(count++, directory);
				}
			}
		}
		ScriptCode[] array3 = SavableScriptCode.LoadFromAssembly(pythonContext.DomainManager, precompiled);
		foreach (ScriptCode code in array3)
		{
			pythonContext.GetCompiledLoader().AddScriptCode(code);
		}
		if (references != null)
		{
			foreach (string assemblyString in references)
			{
				pythonContext.DomainManager.LoadAssembly(Assembly.Load(assemblyString));
			}
		}
		ModuleContext moduleContext = new ModuleContext(new PythonDictionary(), pythonContext);
		try
		{
			Importer.Import(moduleContext.GlobalContext, main, PythonTuple.EMPTY, 0);
		}
		catch (SystemExitException ex)
		{
			object otherCode;
			return ex.GetExitCode(out otherCode);
		}
		return 0;
	}

	public static CodeContext GetPythonTypeContext(PythonType pt)
	{
		return pt.PythonContext.SharedContext;
	}

	public static Delegate GetDelegate(CodeContext context, object target, Type type)
	{
		return context.LanguageContext.DelegateCreator.GetDelegate(target, type);
	}

	public static int CompareLists(List self, List other)
	{
		return self.CompareTo(other);
	}

	public static int CompareTuples(PythonTuple self, PythonTuple other)
	{
		return self.CompareTo(other);
	}

	public static int CompareFloats(double self, double other)
	{
		return DoubleOps.Compare(self, other);
	}

	public static Bytes MakeBytes(byte[] bytes)
	{
		return new Bytes(bytes);
	}

	public static byte[] MakeByteArray(this string s)
	{
		byte[] array = new byte[s.Length];
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] < 'Ā')
			{
				array[i] = (byte)s[i];
				continue;
			}
			throw UnicodeEncodeError("'ascii' codec can't decode byte {0:X} in position {1}: ordinal not in range", (int)array[i], i);
		}
		return array;
	}

	public static string MakeString(this IList<byte> bytes)
	{
		return bytes.MakeString(bytes.Count);
	}

	internal static string MakeString(this byte[] preamble, IList<byte> bytes)
	{
		char[] array = new char[preamble.Length + bytes.Count];
		for (int i = 0; i < preamble.Length; i++)
		{
			array[i] = (char)preamble[i];
		}
		for (int j = 0; j < bytes.Count; j++)
		{
			array[j + preamble.Length] = (char)bytes[j];
		}
		return new string(array);
	}

	internal static string MakeString(this IList<byte> bytes, int maxBytes)
	{
		int num = Math.Min(bytes.Count, maxBytes);
		StringBuilder stringBuilder = new StringBuilder(num);
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append((char)bytes[i]);
		}
		return stringBuilder.ToString();
	}

	public static void RemoveName(CodeContext context, string name)
	{
		if (!context.TryRemoveVariable(name))
		{
			throw NameError(name);
		}
	}

	public static object LookupName(CodeContext context, string name)
	{
		if (context.TryLookupName(name, out var value))
		{
			return value;
		}
		if (context.TryLookupBuiltin(name, out value))
		{
			return value;
		}
		throw NameError(name);
	}

	public static object SetName(CodeContext context, string name, object value)
	{
		context.SetVariable(name, value);
		return value;
	}

	public static object ToPython(this IntPtr handle)
	{
		long num = handle.ToInt64();
		if (num >= int.MinValue && num <= int.MaxValue)
		{
			return ScriptingRuntimeHelpers.Int32ToObject((int)num);
		}
		return (BigInteger)num;
	}

	public static CodeContext CreateLocalContext(CodeContext outerContext, MutableTuple boxes, string[] args)
	{
		return new CodeContext(new PythonDictionary(new RuntimeVariablesDictionaryStorage(boxes, args)), outerContext.ModuleContext);
	}

	public static CodeContext GetGlobalContext(CodeContext context)
	{
		return context.ModuleContext.GlobalContext;
	}

	public static ClosureCell MakeClosureCell()
	{
		return new ClosureCell(Uninitialized.Instance);
	}

	public static ClosureCell MakeClosureCellWithValue(object initialValue)
	{
		return new ClosureCell(initialValue);
	}

	public static MutableTuple GetClosureTupleFromFunction(PythonFunction function)
	{
		return GetClosureTupleFromContext(function.Context);
	}

	public static MutableTuple GetClosureTupleFromGenerator(PythonGenerator generator)
	{
		return GetClosureTupleFromContext(generator.Context);
	}

	public static MutableTuple GetClosureTupleFromContext(CodeContext context)
	{
		return (context.Dict._storage as RuntimeVariablesDictionaryStorage).Tuple;
	}

	public static CodeContext GetParentContextFromFunction(PythonFunction function)
	{
		return function.Context;
	}

	public static CodeContext GetParentContextFromGenerator(PythonGenerator generator)
	{
		return generator.Context;
	}

	public static object GetGlobal(CodeContext context, string name)
	{
		return GetVariable(context, name, isGlobal: true, lightThrow: false);
	}

	public static object GetLocal(CodeContext context, string name)
	{
		return GetVariable(context, name, isGlobal: false, lightThrow: false);
	}

	internal static object GetVariable(CodeContext context, string name, bool isGlobal, bool lightThrow)
	{
		object value;
		if (isGlobal)
		{
			if (context.TryGetGlobalVariable(name, out value))
			{
				return value;
			}
		}
		else if (context.TryLookupName(name, out value))
		{
			return value;
		}
		PythonDictionary builtinsDict = context.GetBuiltinsDict();
		if (builtinsDict != null && builtinsDict.TryGetValue(name, out value))
		{
			return value;
		}
		Exception ex = ((!isGlobal) ? NameError(name) : GlobalNameError(name));
		if (lightThrow)
		{
			return LightExceptions.Throw(ex);
		}
		throw ex;
	}

	public static object RawGetGlobal(CodeContext context, string name)
	{
		if (context.TryGetGlobalVariable(name, out var res))
		{
			return res;
		}
		return Uninitialized.Instance;
	}

	public static object RawGetLocal(CodeContext context, string name)
	{
		if (context.TryLookupName(name, out var value))
		{
			return value;
		}
		return Uninitialized.Instance;
	}

	public static void SetGlobal(CodeContext context, string name, object value)
	{
		context.SetGlobalVariable(name, value);
	}

	public static void SetLocal(CodeContext context, string name, object value)
	{
		context.SetVariable(name, value);
	}

	public static void DeleteGlobal(CodeContext context, string name)
	{
		if (context.TryRemoveGlobalVariable(name))
		{
			return;
		}
		throw NameError(name);
	}

	public static void DeleteLocal(CodeContext context, string name)
	{
		if (context.TryRemoveVariable(name))
		{
			return;
		}
		throw NameError(name);
	}

	public static PythonGlobal[] GetGlobalArrayFromContext(CodeContext context)
	{
		return context.GetGlobalArray();
	}

	public static Exception MultipleKeywordArgumentError(PythonFunction function, string name)
	{
		return TypeError("{0}() got multiple values for keyword argument '{1}'", function.__name__, name);
	}

	public static Exception UnexpectedKeywordArgumentError(PythonFunction function, string name)
	{
		return TypeError("{0}() got an unexpected keyword argument '{1}'", function.__name__, name);
	}

	public static Exception StaticAssignmentFromInstanceError(PropertyTracker tracker, bool isAssignment)
	{
		if (isAssignment)
		{
			if (DynamicHelpers.GetPythonTypeFromType(tracker.DeclaringType).IsPythonType)
			{
				return TypeError("can't set attributes of built-in/extension type '{0}'", NameConverter.GetTypeName(tracker.DeclaringType));
			}
			return new MissingMemberException(string.Format(Resources.StaticAssignmentFromInstanceError, tracker.Name, NameConverter.GetTypeName(tracker.DeclaringType)));
		}
		return new MissingMemberException(string.Format(Resources.StaticAccessFromInstanceError, tracker.Name, NameConverter.GetTypeName(tracker.DeclaringType)));
	}

	public static Exception FunctionBadArgumentError(PythonFunction func, int count)
	{
		return func.BadArgumentError(count);
	}

	public static Exception BadKeywordArgumentError(PythonFunction func, int count)
	{
		return func.BadKeywordArgumentError(count);
	}

	public static Exception AttributeErrorForMissingOrReadonly(CodeContext context, PythonType dt, string name)
	{
		if (dt.TryResolveSlot(context, name, out var _))
		{
			throw AttributeErrorForReadonlyAttribute(dt.Name, name);
		}
		throw AttributeErrorForMissingAttribute(dt.Name, name);
	}

	public static Exception AttributeErrorForMissingAttribute(object o, string name)
	{
		if (o is PythonType pythonType)
		{
			return AttributeErrorForMissingAttribute(pythonType.Name, name);
		}
		return AttributeErrorForReadonlyAttribute(PythonTypeOps.GetName(o), name);
	}

	public static Exception ValueError(string format, params object[] args)
	{
		return new ValueErrorException(string.Format(format, args));
	}

	public static Exception KeyError(object key)
	{
		return PythonExceptions.CreateThrowable(PythonExceptions.KeyError, key);
	}

	public static Exception KeyError(string format, params object[] args)
	{
		return new KeyNotFoundException(string.Format(format, args));
	}

	public static Exception UnicodeDecodeError(string format, params object[] args)
	{
		return new DecoderFallbackException(string.Format(format, args));
	}

	public static Exception UnicodeEncodeError(string format, params object[] args)
	{
		return new EncoderFallbackException(string.Format(format, args));
	}

	public static Exception IOError(Exception inner)
	{
		return new IOException(inner.Message, inner);
	}

	public static Exception IOError(string format, params object[] args)
	{
		return new IOException(string.Format(format, args));
	}

	public static Exception EofError(string format, params object[] args)
	{
		return new EndOfStreamException(string.Format(format, args));
	}

	public static Exception StandardError(string format, params object[] args)
	{
		return new SystemException(string.Format(format, args));
	}

	public static Exception ZeroDivisionError(string format, params object[] args)
	{
		return new DivideByZeroException(string.Format(format, args));
	}

	public static Exception SystemError(string format, params object[] args)
	{
		return new SystemException(string.Format(format, args));
	}

	public static Exception TypeError(string format, params object[] args)
	{
		return new TypeErrorException(string.Format(format, args));
	}

	public static Exception IndexError(string format, params object[] args)
	{
		return new IndexOutOfRangeException(string.Format(format, args));
	}

	public static Exception MemoryError(string format, params object[] args)
	{
		return new OutOfMemoryException(string.Format(format, args));
	}

	public static Exception ArithmeticError(string format, params object[] args)
	{
		return new ArithmeticException(string.Format(format, args));
	}

	public static Exception NotImplementedError(string format, params object[] args)
	{
		return new NotImplementedException(string.Format(format, args));
	}

	public static Exception AttributeError(string format, params object[] args)
	{
		return new MissingMemberException(string.Format(format, args));
	}

	public static Exception OverflowError(string format, params object[] args)
	{
		return new OverflowException(string.Format(format, args));
	}

	public static Exception WindowsError(string format, params object[] args)
	{
		return new Win32Exception(string.Format(format, args));
	}

	public static Exception SystemExit()
	{
		return new SystemExitException();
	}

	public static void SyntaxWarning(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode)
	{
		PythonContext pythonContext = (PythonContext)sourceUnit.LanguageContext;
		CodeContext sharedContext = pythonContext.SharedContext;
		ShowWarning(sharedContext, PythonExceptions.SyntaxWarning, message, sourceUnit.Path, span.Start.Line);
	}

	public static SyntaxErrorException SyntaxError(string message, SourceUnit sourceUnit, SourceSpan span, int errorCode)
	{
		switch (errorCode & 0x7FFFFFF0)
		{
		case 32:
			return new IndentationException(message, sourceUnit, span, errorCode, Severity.FatalError);
		case 48:
			return new TabException(message, sourceUnit, span, errorCode, Severity.FatalError);
		default:
		{
			SyntaxErrorException ex = new SyntaxErrorException(message, sourceUnit, span, errorCode, Severity.FatalError);
			if ((errorCode & 0x40) != 0)
			{
				ex.Data[PythonContext._syntaxErrorNoCaret] = ScriptingRuntimeHelpers.True;
			}
			return ex;
		}
		}
	}

	public static SyntaxErrorException BadSourceError(byte badByte, SourceSpan span, string path)
	{
		SyntaxErrorException ex = new SyntaxErrorException(string.Format("Non-ASCII character '\\x{0:x2}' in file {2} on line {1}, but no encoding declared; see http://www.python.org/peps/pep-0263.html for details", badByte, span.Start.Line, path), path, null, null, span, 16, Severity.FatalError);
		ex.Data[PythonContext._syntaxErrorNoCaret] = ScriptingRuntimeHelpers.True;
		return ex;
	}

	public static Exception StopIteration()
	{
		return StopIteration("");
	}

	public static Exception InvalidType(object o, RuntimeTypeHandle handle)
	{
		return TypeErrorForTypeMismatch(DynamicHelpers.GetPythonTypeFromType(Type.GetTypeFromHandle(handle)).Name, o);
	}

	public static Exception ZeroDivisionError()
	{
		return ZeroDivisionError("Attempted to divide by zero.");
	}

	public static Exception ValueErrorForUnpackMismatch(int left, int right)
	{
		if (left > right)
		{
			return ValueError("need more than {0} values to unpack", right);
		}
		return ValueError("too many values to unpack");
	}

	public static Exception NameError(string name)
	{
		return new UnboundNameException($"name '{name}' is not defined");
	}

	public static Exception GlobalNameError(string name)
	{
		return new UnboundNameException($"global name '{name}' is not defined");
	}

	public static Exception TypeErrorForUnboundMethodCall(string methodName, Type methodType, object instance)
	{
		return TypeErrorForUnboundMethodCall(methodName, DynamicHelpers.GetPythonTypeFromType(methodType), instance);
	}

	public static Exception TypeErrorForUnboundMethodCall(string methodName, PythonType methodType, object instance)
	{
		string format = $"unbound method {methodName}() must be called with {methodType.Name} instance as first argument (got {PythonTypeOps.GetName(instance)} instead)";
		return TypeError(format);
	}

	public static Exception TypeErrorForIllegalSend()
	{
		string format = "can't send non-None value to a just-started generator";
		return TypeError(format);
	}

	public static Exception TypeErrorForArgumentCountMismatch(string methodName, int expectedArgCount, int actualArgCount)
	{
		return TypeError("{0}() takes exactly {1} argument{2} ({3} given)", methodName, expectedArgCount, (expectedArgCount == 1) ? "" : "s", actualArgCount);
	}

	public static Exception TypeErrorForTypeMismatch(string expectedTypeName, object instance)
	{
		return TypeError("expected {0}, got {1}", expectedTypeName, GetPythonTypeName(instance));
	}

	public static Exception TypeErrorForUnhashableType(string typeName)
	{
		return TypeError(typeName + " objects are unhashable");
	}

	public static Exception TypeErrorForUnhashableObject(object obj)
	{
		return TypeErrorForUnhashableType(PythonTypeOps.GetName(obj));
	}

	internal static Exception TypeErrorForIncompatibleObjectLayout(string prefix, PythonType type, Type newType)
	{
		return TypeError("{0}: '{1}' object layout differs from '{2}'", prefix, type.Name, newType);
	}

	public static Exception TypeErrorForNonStringAttribute()
	{
		return TypeError("attribute name must be string");
	}

	internal static Exception TypeErrorForBadInstance(string template, object instance)
	{
		return TypeError(template, GetPythonTypeName(instance));
	}

	public static Exception TypeErrorForBinaryOp(string opSymbol, object x, object y)
	{
		throw TypeError("unsupported operand type(s) for {0}: '{1}' and '{2}'", opSymbol, GetPythonTypeName(x), GetPythonTypeName(y));
	}

	public static Exception TypeErrorForUnaryOp(string opSymbol, object x)
	{
		throw TypeError("unsupported operand type for {0}: '{1}'", opSymbol, GetPythonTypeName(x));
	}

	public static Exception TypeErrorForNonIterableObject(object o)
	{
		return TypeError("argument of type '{0}' is not iterable", PythonTypeOps.GetName(o));
	}

	public static Exception TypeErrorForDefaultArgument(string message)
	{
		return TypeError(message);
	}

	public static Exception AttributeErrorForReadonlyAttribute(string typeName, string attributeName)
	{
		if (attributeName == "__class__")
		{
			return TypeError("can't delete __class__ attribute");
		}
		return AttributeError("attribute '{0}' of '{1}' object is read-only", attributeName, typeName);
	}

	public static Exception AttributeErrorForBuiltinAttributeDeletion(string typeName, string attributeName)
	{
		return AttributeError("cannot delete attribute '{0}' of builtin type '{1}'", attributeName, typeName);
	}

	public static Exception MissingInvokeMethodException(object o, string name)
	{
		if (o is OldClass)
		{
			throw AttributeError("type object '{0}' has no attribute '{1}'", ((OldClass)o).Name, name);
		}
		throw AttributeError("'{0}' object has no attribute '{1}'", GetPythonTypeName(o), name);
	}

	internal static Exception MakeExceptionTypeError(object type)
	{
		return TypeError("exceptions must be classes, or instances, not {0}", PythonTypeOps.GetName(type));
	}

	public static Exception AttributeErrorForObjectMissingAttribute(object obj, string attributeName)
	{
		if (obj is OldInstance)
		{
			return AttributeErrorForOldInstanceMissingAttribute(((OldInstance)obj)._class.Name, attributeName);
		}
		if (obj is OldClass)
		{
			return AttributeErrorForOldClassMissingAttribute(((OldClass)obj).Name, attributeName);
		}
		return AttributeErrorForMissingAttribute(PythonTypeOps.GetName(obj), attributeName);
	}

	public static Exception AttributeErrorForMissingAttribute(string typeName, string attributeName)
	{
		return AttributeError("'{0}' object has no attribute '{1}'", typeName, attributeName);
	}

	public static Exception AttributeErrorForOldInstanceMissingAttribute(string typeName, string attributeName)
	{
		return AttributeError("{0} instance has no attribute '{1}'", typeName, attributeName);
	}

	public static Exception AttributeErrorForOldClassMissingAttribute(string typeName, string attributeName)
	{
		return AttributeError("class {0} has no attribute '{1}'", typeName, attributeName);
	}

	public static Exception UncallableError(object func)
	{
		return TypeError("{0} is not callable", PythonTypeOps.GetName(func));
	}

	public static Exception TypeErrorForProtectedMember(Type type, string name)
	{
		return TypeError("cannot access protected member {0} without a python subclass of {1}", name, NameConverter.GetTypeName(type));
	}

	public static Exception TypeErrorForGenericMethod(Type type, string name)
	{
		return TypeError("{0}.{1} is a generic method and must be indexed with types before calling", NameConverter.GetTypeName(type), name);
	}

	public static Exception TypeErrorForUnIndexableObject(object o)
	{
		if (o == null)
		{
			return TypeError("'NoneType' object cannot be interpreted as an index");
		}
		if (o is IPythonObject pythonObject)
		{
			return TypeError("'{0}' object cannot be interpreted as an index", pythonObject.PythonType.Name);
		}
		return TypeError("object cannot be interpreted as an index");
	}

	[Obsolete("no longer used anywhere")]
	public static Exception TypeErrorForBadDictionaryArgument(PythonFunction f)
	{
		return TypeError("{0}() argument after ** must be a dictionary", f.__name__);
	}

	public static T TypeErrorForBadEnumConversion<T>(object value)
	{
		throw TypeError("Cannot convert numeric value {0} to {1}.  The value must be zero.", value, NameConverter.GetTypeName(typeof(T)));
	}

	public static Exception UnreadableProperty()
	{
		return AttributeError("unreadable attribute");
	}

	public static Exception UnsetableProperty()
	{
		return AttributeError("readonly attribute");
	}

	public static Exception UndeletableProperty()
	{
		return AttributeError("undeletable attribute");
	}

	public static Exception Warning(string format, params object[] args)
	{
		return new WarningException(string.Format(format, args));
	}

	public static List<FunctionStack> GetFunctionStack()
	{
		return _funcStack ?? (_funcStack = new List<FunctionStack>());
	}

	public static List<FunctionStack> GetFunctionStackNoCreate()
	{
		return _funcStack;
	}

	public static List<FunctionStack> PushFrame(CodeContext context, FunctionCode function)
	{
		List<FunctionStack> functionStack = GetFunctionStack();
		functionStack.Add(new FunctionStack(context, function));
		return functionStack;
	}

	internal static LightLambdaExpression ToGenerator(this LightLambdaExpression code, bool shouldInterpret, bool debuggable, int compilationThreshold)
	{
		return Utils.LightLambda(typeof(object), code.Type, new GeneratorRewriter(code.Name, code.Body).Reduce(shouldInterpret, debuggable, compilationThreshold, code.Parameters, (Expression<Func<MutableTuple, object>> x) => x), code.Name, code.Parameters);
	}

	public static void UpdateStackTrace(Exception e, CodeContext context, FunctionCode funcCode, int line)
	{
		if (line != -1)
		{
			List<DynamicStackFrame> list = e.GetFrameList();
			if (list == null)
			{
				e.SetFrameList(list = new List<DynamicStackFrame>());
			}
			PythonDynamicStackFrame item = new PythonDynamicStackFrame(context, funcCode, line);
			funcCode.LightThrowCompile(context);
			list.Add(item);
		}
	}

	public static DynamicStackFrame[] GetDynamicStackFrames(Exception e)
	{
		return PythonExceptions.GetDynamicStackFrames(e);
	}

	public static byte[] ConvertBufferToByteArray(PythonBuffer buffer)
	{
		return buffer.ToString().MakeByteArray();
	}

	public static bool ModuleTryGetMember(CodeContext context, PythonModule module, string name, out object res)
	{
		object attributeNoThrow = module.GetAttributeNoThrow(context, name);
		if (attributeNoThrow != OperationFailed.Value)
		{
			res = attributeNoThrow;
			return true;
		}
		res = null;
		return false;
	}

	internal static void ScopeSetMember(CodeContext context, Scope scope, string name, object value)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			scopeStorage.SetValue(name, ignoreCase: false, value);
		}
		else
		{
			SetAttr(context, scope, name, value);
		}
	}

	internal static object ScopeGetMember(CodeContext context, Scope scope, string name)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			return scopeStorage.GetValue(name, ignoreCase: false);
		}
		return GetBoundAttr(context, scope, name);
	}

	internal static bool ScopeTryGetMember(CodeContext context, Scope scope, string name, out object value)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			return scopeStorage.TryGetValue(name, ignoreCase: false, out value);
		}
		return TryGetBoundAttr(context, scope, name, out value);
	}

	internal static bool ScopeContainsMember(CodeContext context, Scope scope, string name)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			return scopeStorage.HasValue(name, ignoreCase: false);
		}
		return HasAttr(context, scope, name);
	}

	internal static bool ScopeDeleteMember(CodeContext context, Scope scope, string name)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			return scopeStorage.DeleteValue(name, ignoreCase: false);
		}
		bool result = HasAttr(context, scope, name);
		DeleteAttr(context, scope, name);
		return result;
	}

	internal static IList<object> ScopeGetMemberNames(CodeContext context, Scope scope)
	{
		if (scope.Storage is ScopeStorage scopeStorage)
		{
			List<object> list = new List<object>();
			foreach (string memberName in scopeStorage.GetMemberNames())
			{
				list.Add(memberName);
			}
			Dictionary<object, object> objectKeys = ((PythonScopeExtension)context.LanguageContext.EnsureScopeExtension(scope)).ObjectKeys;
			if (objectKeys != null)
			{
				foreach (object key in objectKeys.Keys)
				{
					list.Add(key);
				}
			}
			return list;
		}
		return GetAttrNames(context, scope);
	}

	public static bool IsUnicode(object unicodeObj)
	{
		return unicodeObj == TypeCache.String;
	}

	public static BuiltinFunction GetUnicodeFuntion()
	{
		return UnicodeHelper.Function;
	}

	public static bool IsExtensionSet(CodeContext codeContext, int id)
	{
		return codeContext.ModuleContext.ExtensionMethods.Id == id;
	}

	public static object GetExtensionMethodSet(CodeContext context)
	{
		return context.ModuleContext.ExtensionMethods;
	}

	public static Exception ImportError(string format, params object[] args)
	{
		return new ImportException(string.Format(format, args));
	}

	public static Exception RuntimeError(string format, params object[] args)
	{
		return new RuntimeException(string.Format(format, args));
	}

	public static Exception UnicodeTranslateError(string format, params object[] args)
	{
		return new UnicodeTranslateException(string.Format(format, args));
	}

	public static Exception PendingDeprecationWarning(string format, params object[] args)
	{
		return new PendingDeprecationWarningException(string.Format(format, args));
	}

	public static Exception EnvironmentError(string format, params object[] args)
	{
		return new EnvironmentException(string.Format(format, args));
	}

	public static Exception LookupError(string format, params object[] args)
	{
		return new LookupException(string.Format(format, args));
	}

	public static Exception OSError(string format, params object[] args)
	{
		return new OSException(string.Format(format, args));
	}

	public static Exception DeprecationWarning(string format, params object[] args)
	{
		return new DeprecationWarningException(string.Format(format, args));
	}

	public static Exception UnicodeError(string format, params object[] args)
	{
		return new UnicodeException(string.Format(format, args));
	}

	public static Exception FloatingPointError(string format, params object[] args)
	{
		return new FloatingPointException(string.Format(format, args));
	}

	public static Exception ReferenceError(string format, params object[] args)
	{
		return new ReferenceException(string.Format(format, args));
	}

	public static Exception FutureWarning(string format, params object[] args)
	{
		return new FutureWarningException(string.Format(format, args));
	}

	public static Exception AssertionError(string format, params object[] args)
	{
		return new AssertionException(string.Format(format, args));
	}

	public static Exception RuntimeWarning(string format, params object[] args)
	{
		return new RuntimeWarningException(string.Format(format, args));
	}

	public static Exception ImportWarning(string format, params object[] args)
	{
		return new ImportWarningException(string.Format(format, args));
	}

	public static Exception UserWarning(string format, params object[] args)
	{
		return new UserWarningException(string.Format(format, args));
	}

	public static Exception SyntaxWarning(string format, params object[] args)
	{
		return new SyntaxWarningException(string.Format(format, args));
	}

	public static Exception UnicodeWarning(string format, params object[] args)
	{
		return new UnicodeWarningException(string.Format(format, args));
	}

	public static Exception StopIteration(string format, params object[] args)
	{
		return new StopIterationException(string.Format(format, args));
	}

	public static Exception BytesWarning(string format, params object[] args)
	{
		return new BytesWarningException(string.Format(format, args));
	}

	public static Exception BufferError(string format, params object[] args)
	{
		return new BufferException(string.Format(format, args));
	}
}
