using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

[Documentation("")]
public static class Builtin
{
	private enum SumVariantType
	{
		Double,
		Int,
		BigInt,
		Object
	}

	private struct SumState
	{
		public double DoubleVal;

		public int IntVal;

		public object ObjectVal;

		public BigInteger BigIntVal;

		public SumVariantType CurType;

		public CallSite<Func<CallSite, object, object, object>> AddSite;

		public object CurrentValue => CurType switch
		{
			SumVariantType.BigInt => BigIntVal, 
			SumVariantType.Double => DoubleVal, 
			SumVariantType.Int => IntVal, 
			SumVariantType.Object => ObjectVal, 
			_ => throw Assert.Unreachable, 
		};

		public SumState(PythonContext context, object start)
		{
			DoubleVal = 0.0;
			IntVal = 0;
			ObjectVal = start;
			BigIntVal = BigInteger.Zero;
			AddSite = context.EnsureAddSite();
			if (start != null)
			{
				if (start.GetType() == typeof(int))
				{
					CurType = SumVariantType.Int;
					IntVal = (int)start;
				}
				else if (start.GetType() == typeof(double))
				{
					CurType = SumVariantType.Double;
					DoubleVal = (double)start;
				}
				else if (start.GetType() == typeof(BigInteger))
				{
					CurType = SumVariantType.BigInt;
					BigIntVal = (BigInteger)start;
				}
				else
				{
					CurType = SumVariantType.Object;
				}
			}
			else
			{
				CurType = SumVariantType.Object;
			}
		}
	}

	public const string __doc__ = "Provides access to commonly used built-in functions, exception objects, etc...";

	public const object __package__ = null;

	public const string __name__ = "__builtin__";

	public static readonly object None;

	[ThreadStatic]
	private static List<PythonModule> _reloadStack;

	private static BigInteger MaxDouble = new BigInteger(double.MaxValue);

	private static BigInteger MinDouble = new BigInteger(double.MinValue);

	public static object True => ScriptingRuntimeHelpers.True;

	public static object False => ScriptingRuntimeHelpers.False;

	public static Ellipsis Ellipsis => Ellipsis.Value;

	public static NotImplementedType NotImplemented => NotImplementedType.Value;

	public static object exit => "Use Ctrl-Z plus Return to exit";

	public static object quit => "Use Ctrl-Z plus Return to exit";

	public static PythonType basestring => DynamicHelpers.GetPythonTypeFromType(typeof(string));

	public static PythonType @bool => DynamicHelpers.GetPythonTypeFromType(typeof(bool));

	public static PythonType buffer => DynamicHelpers.GetPythonTypeFromType(typeof(PythonBuffer));

	public static PythonType bytes => DynamicHelpers.GetPythonTypeFromType(typeof(Bytes));

	public static PythonType bytearray => DynamicHelpers.GetPythonTypeFromType(typeof(ByteArray));

	public static PythonType classmethod => DynamicHelpers.GetPythonTypeFromType(typeof(classmethod));

	public static PythonType complex => DynamicHelpers.GetPythonTypeFromType(typeof(Complex));

	public static PythonType dict => DynamicHelpers.GetPythonTypeFromType(typeof(PythonDictionary));

	public static PythonType enumerate => DynamicHelpers.GetPythonTypeFromType(typeof(Enumerate));

	public static PythonType file => DynamicHelpers.GetPythonTypeFromType(typeof(PythonFile));

	public static PythonType @float => DynamicHelpers.GetPythonTypeFromType(typeof(double));

	public static PythonType @int => DynamicHelpers.GetPythonTypeFromType(typeof(int));

	public static PythonType set => DynamicHelpers.GetPythonTypeFromType(typeof(SetCollection));

	public static PythonType frozenset => DynamicHelpers.GetPythonTypeFromType(typeof(FrozenSetCollection));

	public static PythonType list => DynamicHelpers.GetPythonTypeFromType(typeof(List));

	public static PythonType @long => TypeCache.BigInteger;

	public static PythonType memoryview => DynamicHelpers.GetPythonTypeFromType(typeof(MemoryView));

	public static PythonType @object => DynamicHelpers.GetPythonTypeFromType(typeof(object));

	public static PythonType property => DynamicHelpers.GetPythonTypeFromType(typeof(PythonProperty));

	public static PythonType reversed => DynamicHelpers.GetPythonTypeFromType(typeof(ReversedEnumerator));

	public static PythonType slice => DynamicHelpers.GetPythonTypeFromType(typeof(Slice));

	public static PythonType staticmethod => DynamicHelpers.GetPythonTypeFromType(typeof(staticmethod));

	public static PythonType super => DynamicHelpers.GetPythonTypeFromType(typeof(Super));

	public static PythonType str => DynamicHelpers.GetPythonTypeFromType(typeof(string));

	public static PythonType tuple => DynamicHelpers.GetPythonTypeFromType(typeof(PythonTuple));

	public static PythonType type => DynamicHelpers.GetPythonTypeFromType(typeof(PythonType));

	public static PythonType unicode => DynamicHelpers.GetPythonTypeFromType(typeof(string));

	public static PythonType xrange => DynamicHelpers.GetPythonTypeFromType(typeof(XRange));

	public static PythonType BaseException => DynamicHelpers.GetPythonTypeFromType(typeof(PythonExceptions.BaseException));

	public static PythonType GeneratorExit => PythonExceptions.GeneratorExit;

	public static PythonType SystemExit => PythonExceptions.SystemExit;

	public static PythonType KeyboardInterrupt => PythonExceptions.KeyboardInterrupt;

	public static PythonType Exception => PythonExceptions.Exception;

	public static PythonType StopIteration => PythonExceptions.StopIteration;

	public static PythonType StandardError => PythonExceptions.StandardError;

	public static PythonType BufferError => PythonExceptions.BufferError;

	public static PythonType ArithmeticError => PythonExceptions.ArithmeticError;

	public static PythonType FloatingPointError => PythonExceptions.FloatingPointError;

	public static PythonType OverflowError => PythonExceptions.OverflowError;

	public static PythonType ZeroDivisionError => PythonExceptions.ZeroDivisionError;

	public static PythonType AssertionError => PythonExceptions.AssertionError;

	public static PythonType AttributeError => PythonExceptions.AttributeError;

	public static PythonType EnvironmentError => PythonExceptions.EnvironmentError;

	public static PythonType IOError => PythonExceptions.IOError;

	public static PythonType OSError => PythonExceptions.OSError;

	public static PythonType WindowsError => PythonExceptions.WindowsError;

	public static PythonType EOFError => PythonExceptions.EOFError;

	public static PythonType ImportError => PythonExceptions.ImportError;

	public static PythonType LookupError => PythonExceptions.LookupError;

	public static PythonType IndexError => PythonExceptions.IndexError;

	public static PythonType KeyError => PythonExceptions.KeyError;

	public static PythonType MemoryError => PythonExceptions.MemoryError;

	public static PythonType NameError => PythonExceptions.NameError;

	public static PythonType UnboundLocalError => PythonExceptions.UnboundLocalError;

	public static PythonType ReferenceError => PythonExceptions.ReferenceError;

	public static PythonType RuntimeError => PythonExceptions.RuntimeError;

	public static PythonType NotImplementedError => PythonExceptions.NotImplementedError;

	public static PythonType SyntaxError => PythonExceptions.SyntaxError;

	public static PythonType IndentationError => PythonExceptions.IndentationError;

	public static PythonType TabError => PythonExceptions.TabError;

	public static PythonType SystemError => PythonExceptions.SystemError;

	public static PythonType TypeError => PythonExceptions.TypeError;

	public static PythonType ValueError => PythonExceptions.ValueError;

	public static PythonType UnicodeError => PythonExceptions.UnicodeError;

	public static PythonType UnicodeDecodeError => PythonExceptions.UnicodeDecodeError;

	public static PythonType UnicodeEncodeError => PythonExceptions.UnicodeEncodeError;

	public static PythonType UnicodeTranslateError => PythonExceptions.UnicodeTranslateError;

	public static PythonType Warning => PythonExceptions.Warning;

	public static PythonType DeprecationWarning => PythonExceptions.DeprecationWarning;

	public static PythonType PendingDeprecationWarning => PythonExceptions.PendingDeprecationWarning;

	public static PythonType RuntimeWarning => PythonExceptions.RuntimeWarning;

	public static PythonType SyntaxWarning => PythonExceptions.SyntaxWarning;

	public static PythonType UserWarning => PythonExceptions.UserWarning;

	public static PythonType FutureWarning => PythonExceptions.FutureWarning;

	public static PythonType ImportWarning => PythonExceptions.ImportWarning;

	public static PythonType UnicodeWarning => PythonExceptions.UnicodeWarning;

	public static PythonType BytesWarning => PythonExceptions.BytesWarning;

	[Documentation("__import__(name) -> module\n\nImport a module.")]
	[LightThrowing]
	public static object __import__(CodeContext context, string name)
	{
		return __import__(context, name, null, null, null, -1);
	}

	[LightThrowing]
	[Documentation("__import__(name, globals, locals, fromlist, level) -> module\n\nImport a module.")]
	public static object __import__(CodeContext context, string name, [DefaultParameterValue(null)] object globals, [DefaultParameterValue(null)] object locals, [DefaultParameterValue(null)] object fromlist, [DefaultParameterValue(-1)] int level)
	{
		if (fromlist is string || fromlist is Extensible<string>)
		{
			List<object> list = new List<object>();
			list.Add(fromlist);
			fromlist = list;
		}
		IList list2 = fromlist as IList;
		PythonContext context2 = PythonContext.GetContext(context);
		object obj = Importer.ImportModule(context, globals, name, list2 != null && list2.Count > 0, level);
		if (obj == null)
		{
			return LightExceptions.Throw(PythonOps.ImportError("No module named {0}", name));
		}
		if (obj is PythonModule pythonModule && list2 != null)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				object obj2 = list2[i];
				if (context2.TryConvertToString(obj2, out var res) && !string.IsNullOrEmpty(res) && res != "*")
				{
					try
					{
						Importer.ImportFrom(context, pythonModule, res);
					}
					catch (ImportException)
					{
					}
				}
			}
		}
		return obj;
	}

	[Documentation("abs(number) -> number\n\nReturn the absolute value of the argument.")]
	public static object abs(CodeContext context, object o)
	{
		if (o is int)
		{
			return Int32Ops.Abs((int)o);
		}
		if (o is long)
		{
			return Int64Ops.Abs((long)o);
		}
		if (o is double)
		{
			return DoubleOps.Abs((double)o);
		}
		if (o is bool)
		{
			return ((bool)o) ? 1 : 0;
		}
		if (o is BigInteger)
		{
			return BigIntegerOps.__abs__((BigInteger)o);
		}
		if (o is Complex)
		{
			return ComplexOps.Abs((Complex)o);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(context, o, "__abs__", out var value))
		{
			return value;
		}
		throw PythonOps.TypeError("bad operand type for abs(): '{0}'", PythonTypeOps.GetName(o));
	}

	public static bool all(object x)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(x);
		while (enumerator.MoveNext())
		{
			if (!PythonOps.IsTrue(enumerator.Current))
			{
				return false;
			}
		}
		return true;
	}

	public static bool any(object x)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(x);
		while (enumerator.MoveNext())
		{
			if (PythonOps.IsTrue(enumerator.Current))
			{
				return true;
			}
		}
		return false;
	}

	[Documentation("apply(object[, args[, kwargs]]) -> value\n\nDeprecated.\nInstead, use:\n    function(*args, **keywords).")]
	public static object apply(CodeContext context, object func)
	{
		return PythonOps.CallWithContext(context, func);
	}

	public static object apply(CodeContext context, object func, object args)
	{
		return PythonOps.CallWithArgsTupleAndContext(context, func, ArrayUtils.EmptyObjects, args);
	}

	public static object apply(CodeContext context, object func, object args, object kws)
	{
		return context.LanguageContext.CallWithKeywords(func, args, kws);
	}

	public static string bin(int number)
	{
		return Int32Ops.ToBinary(number);
	}

	public static string bin(Index number)
	{
		return Int32Ops.ToBinary(Converter.ConvertToIndex(number));
	}

	public static string bin(BigInteger number)
	{
		return BigIntegerOps.ToBinary(number);
	}

	public static string bin(double number)
	{
		throw PythonOps.TypeError("'float' object cannot be interpreted as an index");
	}

	[Python3Warning("callable() is removed in 3.x. instead call hasattr(obj, '__call__')")]
	[Documentation("callable(object) -> bool\n\nReturn whether the object is callable (i.e., some kind of function).")]
	public static bool callable(CodeContext context, object o)
	{
		return PythonOps.IsCallable(context, o);
	}

	[LightThrowing]
	[Documentation("chr(i) -> character\n\nReturn a string of one character with ordinal i; 0 <= i< 256.")]
	public static object chr(int value)
	{
		if (value < 0 || value > 255)
		{
			return LightExceptions.Throw(PythonOps.ValueError("{0} is not in required range", value));
		}
		return ScriptingRuntimeHelpers.CharToString((char)value);
	}

	internal static object TryCoerce(CodeContext context, object x, object y)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(x);
		if (pythonType.TryResolveSlot(context, "__coerce__", out var slot) && slot.TryGetValue(context, x, pythonType, out var value))
		{
			return PythonCalls.Call(context, value, y);
		}
		return NotImplementedType.Value;
	}

	[Documentation("coerce(x, y) -> (x1, y1)\n\nReturn a tuple consisting of the two numeric arguments converted to\na common type. If coercion is not possible, raise TypeError.")]
	public static object coerce(CodeContext context, object x, object y)
	{
		if (x == null && y == null)
		{
			object[] items = new object[2];
			return PythonTuple.MakeTuple(items);
		}
		object obj = TryCoerce(context, x, y);
		if (obj != null && obj != NotImplementedType.Value)
		{
			return obj;
		}
		obj = TryCoerce(context, y, x);
		if (obj != null && obj != NotImplementedType.Value && obj is PythonTuple { Count: 2 } pythonTuple)
		{
			return PythonTuple.MakeTuple(pythonTuple[1], pythonTuple[0]);
		}
		throw PythonOps.TypeError("coercion failed");
	}

	[Documentation("compile a unit of source code.\n\nThe source can be compiled either as exec, eval, or single.\nexec compiles the code as if it were a file\neval compiles the code as if were an expression\nsingle compiles a single statement\n\n")]
	public static object compile(CodeContext context, string source, string filename, string mode, [DefaultParameterValue(null)] object flags, [DefaultParameterValue(null)] object dont_inherit)
	{
		if (source.IndexOf('\0') != -1)
		{
			throw PythonOps.TypeError("compile() expected string without null bytes");
		}
		bool flag = false;
		int num = ((flags != null) ? Converter.ConvertToInt32(flags) : 0);
		if ((num & 0x400) != 0)
		{
			flag = true;
			num &= -1025;
		}
		source = RemoveBom(source);
		bool compilerInheritance = GetCompilerInheritance(dont_inherit);
		CompileFlags compilerFlags = GetCompilerFlags(num);
		PythonCompilerOptions runtimeGeneratedCodeCompilerOptions = GetRuntimeGeneratedCodeCompilerOptions(context, compilerInheritance, compilerFlags);
		if ((compilerFlags & CompileFlags.CO_DONT_IMPLY_DEDENT) != 0)
		{
			runtimeGeneratedCodeCompilerOptions.DontImplyDedent = true;
		}
		SourceUnit sourceUnit = mode switch
		{
			"exec" => context.LanguageContext.CreateSnippet(source, filename, SourceCodeKind.Statements), 
			"eval" => context.LanguageContext.CreateSnippet(source, filename, SourceCodeKind.Expression), 
			"single" => context.LanguageContext.CreateSnippet(source, filename, SourceCodeKind.InteractiveCode), 
			_ => throw PythonOps.ValueError("compile() arg 3 must be 'exec' or 'eval' or 'single'"), 
		};
		if (flag)
		{
			return _ast.BuildAst(context, sourceUnit, runtimeGeneratedCodeCompilerOptions, mode);
		}
		return FunctionCode.FromSourceUnit(sourceUnit, runtimeGeneratedCodeCompilerOptions, register: true);
	}

	private static string RemoveBom(string source)
	{
		if (source.StartsWith("ï»¿", StringComparison.Ordinal))
		{
			source = source.Substring(3, source.Length - 3);
		}
		return source;
	}

	public static int cmp(CodeContext context, object x, object y)
	{
		return PythonOps.Compare(context, x, y);
	}

	public static int cmp(CodeContext context, int x, int y)
	{
		return Int32Ops.Compare(x, y);
	}

	public static int cmp(CodeContext context, [NotNull] BigInteger x, [NotNull] BigInteger y)
	{
		if ((object)x == (object)y)
		{
			return 0;
		}
		return BigIntegerOps.Compare(x, y);
	}

	public static int cmp(CodeContext context, double x, [NotNull] BigInteger y)
	{
		return -BigIntegerOps.Compare(y, x);
	}

	public static int cmp(CodeContext context, [NotNull] BigInteger x, double y)
	{
		return BigIntegerOps.Compare(x, y);
	}

	public static int cmp(CodeContext context, [NotNull] string x, [NotNull] string y)
	{
		if ((object)x != y)
		{
			int num = string.CompareOrdinal(x, y);
			if (num >= 1)
			{
				return 1;
			}
			if (num <= -1)
			{
				return -1;
			}
		}
		return 0;
	}

	public static int cmp(CodeContext context, [NotNull] PythonTuple x, [NotNull] PythonTuple y)
	{
		if (x == y)
		{
			return 0;
		}
		return x.CompareTo(y);
	}

	public static void delattr(CodeContext context, object o, string name)
	{
		PythonOps.DeleteAttr(context, o, name);
	}

	public static List dir(CodeContext context)
	{
		List list = PythonOps.MakeListFromSequence(context.Dict.Keys);
		list.sort(context);
		return list;
	}

	public static List dir(CodeContext context, object o)
	{
		IList<object> attrNames = PythonOps.GetAttrNames(context, o);
		List list = new List(attrNames);
		list.sort(context);
		return list;
	}

	public static object divmod(CodeContext context, object x, object y)
	{
		return PythonContext.GetContext(context).DivMod(x, y);
	}

	public static object eval(CodeContext context, FunctionCode code)
	{
		if (code == null)
		{
			throw PythonOps.TypeError("eval() argument 1 must be string or code object");
		}
		return eval(context, code, null);
	}

	public static object eval(CodeContext context, FunctionCode code, PythonDictionary globals)
	{
		if (code == null)
		{
			throw PythonOps.TypeError("eval() argument 1 must be string or code object");
		}
		return eval(context, code, globals, globals);
	}

	public static object eval(CodeContext context, FunctionCode code, PythonDictionary globals, object locals)
	{
		if (code == null)
		{
			throw PythonOps.TypeError("eval() argument 1 must be string or code object");
		}
		return code.Call(GetExecEvalScopeOptional(context, globals, locals, copyModule: false));
	}

	internal static PythonDictionary GetAttrLocals(CodeContext context, object locals)
	{
		PythonDictionary result = null;
		if (locals == null)
		{
			if (context.IsTopLevel)
			{
				result = context.Dict;
			}
		}
		else
		{
			result = (locals as PythonDictionary) ?? new PythonDictionary(new ObjectAttributesAdapter(context, locals));
		}
		return result;
	}

	[LightThrowing]
	public static object eval(CodeContext context, string expression)
	{
		if (expression == null)
		{
			throw PythonOps.TypeError("eval() argument 1 must be string or code object");
		}
		return eval(context, expression, globals(context), locals(context));
	}

	[LightThrowing]
	public static object eval(CodeContext context, string expression, PythonDictionary globals)
	{
		if (expression == null)
		{
			throw PythonOps.TypeError("eval() argument 1 must be string or code object");
		}
		return eval(context, expression, globals, globals);
	}

	[LightThrowing]
	public static object eval(CodeContext context, string expression, PythonDictionary globals, object locals)
	{
		if (expression == null)
		{
			throw PythonOps.TypeError("eval() argument 1 must be string or code object");
		}
		if (locals != null && PythonOps.IsMappingType(context, locals) == ScriptingRuntimeHelpers.False)
		{
			throw PythonOps.TypeError("locals must be mapping");
		}
		expression = RemoveBom(expression);
		CodeContext execEvalScopeOptional = GetExecEvalScopeOptional(context, globals, locals, copyModule: false);
		PythonContext context2 = PythonContext.GetContext(context);
		SourceUnit sourceUnit = context2.CreateSnippet(expression.TrimStart(' ', '\t'), SourceCodeKind.Expression);
		PythonCompilerOptions runtimeGeneratedCodeCompilerOptions = GetRuntimeGeneratedCodeCompilerOptions(context, inheritContext: true, (CompileFlags)0);
		runtimeGeneratedCodeCompilerOptions.Module |= ModuleOptions.LightThrow;
		runtimeGeneratedCodeCompilerOptions.Module &= ~ModuleOptions.ModuleBuiltins;
		FunctionCode functionCode = FunctionCode.FromSourceUnit(sourceUnit, runtimeGeneratedCodeCompilerOptions, register: false);
		return functionCode.Call(execEvalScopeOptional);
	}

	public static void execfile(CodeContext context, object filename)
	{
		execfile(context, filename, null, null);
	}

	public static void execfile(CodeContext context, object filename, object globals)
	{
		execfile(context, filename, globals, null);
	}

	public static void execfile(CodeContext context, object filename, object globals, object locals)
	{
		if (filename == null)
		{
			throw PythonOps.TypeError("execfile() argument 1 must be string, not None");
		}
		PythonDictionary pythonDictionary = globals as PythonDictionary;
		if (pythonDictionary == null && globals != null)
		{
			throw PythonOps.TypeError("execfile() arg 2 must be dictionary");
		}
		PythonDictionary pythonDictionary2 = locals as PythonDictionary;
		if (pythonDictionary2 == null && locals != null)
		{
			throw PythonOps.TypeError("execfile() arg 3 must be dictionary");
		}
		if (pythonDictionary2 == null)
		{
			pythonDictionary2 = pythonDictionary;
		}
		CodeContext execEvalScopeOptional = GetExecEvalScopeOptional(context, pythonDictionary, pythonDictionary2, copyModule: true);
		string path = Converter.ConvertToString(filename);
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.DomainManager.Platform.FileExists(path))
		{
			throw PythonOps.IOError("execfile: specified file doesn't exist");
		}
		SourceUnit sourceUnit = context2.CreateFileUnit(path, context2.DefaultEncoding, SourceCodeKind.Statements);
		PythonCompilerOptions runtimeGeneratedCodeCompilerOptions = GetRuntimeGeneratedCodeCompilerOptions(context, inheritContext: true, (CompileFlags)0);
		runtimeGeneratedCodeCompilerOptions.Module &= ~ModuleOptions.Optimized;
		FunctionCode functionCode;
		try
		{
			functionCode = FunctionCode.FromSourceUnit(sourceUnit, runtimeGeneratedCodeCompilerOptions, register: false);
		}
		catch (UnauthorizedAccessException inner)
		{
			throw PythonOps.IOError(inner);
		}
		functionCode.Call(execEvalScopeOptional);
	}

	public static string filter(CodeContext context, object function, [NotNull] string list)
	{
		if (function == null)
		{
			return list;
		}
		if (list == null)
		{
			throw PythonOps.TypeError("NoneType is not iterable");
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in list)
		{
			if (PythonOps.IsTrue(PythonCalls.Call(context, function, ScriptingRuntimeHelpers.CharToString(c))))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string filter(CodeContext context, object function, [NotNull] ExtensibleString list)
	{
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerator enumerator = PythonOps.GetEnumerator(list);
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			object o = ((function != null) ? PythonCalls.Call(context, function, current) : current);
			if (PythonOps.IsTrue(o))
			{
				stringBuilder.Append(Converter.ConvertToString(current));
			}
		}
		return stringBuilder.ToString();
	}

	public static PythonTuple filter(CodeContext context, object function, [NotNull] PythonTuple tuple)
	{
		List<object> list = new List<object>(tuple.__len__());
		for (int i = 0; i < tuple.__len__(); i++)
		{
			object obj = tuple[i];
			object o = ((function != null) ? PythonCalls.Call(context, function, obj) : obj);
			if (PythonOps.IsTrue(o))
			{
				list.Add(obj);
			}
		}
		return PythonTuple.MakeTuple(list.ToArray());
	}

	public static List filter(CodeContext context, object function, object list)
	{
		if (list == null)
		{
			throw PythonOps.TypeError("NoneType is not iterable");
		}
		List list2 = new List();
		IEnumerator enumerator = PythonOps.GetEnumerator(list);
		while (enumerator.MoveNext())
		{
			if (function == null)
			{
				if (PythonOps.IsTrue(enumerator.Current))
				{
					list2.AddNoLock(enumerator.Current);
				}
			}
			else if (PythonOps.IsTrue(PythonCalls.Call(context, function, enumerator.Current)))
			{
				list2.AddNoLock(enumerator.Current);
			}
		}
		return list2;
	}

	public static string format(CodeContext context, object argValue, [DefaultParameterValue("")] string formatSpec)
	{
		object value2;
		if (argValue is OldInstance oldInstance && oldInstance.TryGetBoundCustomMember(context, "__format__", out var value))
		{
			value2 = PythonOps.CallWithContext(context, value, formatSpec);
		}
		else
		{
			PythonTypeOps.TryInvokeBinaryOperator(context, argValue, formatSpec, "__format__", out value2);
		}
		if (!(value2 is string result))
		{
			throw PythonOps.TypeError("{0}.__format__ must return string or unicode, not {1}", PythonTypeOps.GetName(argValue), PythonTypeOps.GetName(value2));
		}
		return result;
	}

	public static object getattr(CodeContext context, object o, string name)
	{
		return PythonOps.GetBoundAttr(context, o, name);
	}

	public static object getattr(CodeContext context, object o, string name, object def)
	{
		if (PythonOps.TryGetBoundAttr(context, o, name, out var ret))
		{
			return ret;
		}
		return def;
	}

	public static PythonDictionary globals(CodeContext context)
	{
		return context.ModuleContext.Globals;
	}

	public static bool hasattr(CodeContext context, object o, string name)
	{
		return PythonOps.HasAttr(context, o, name);
	}

	public static int hash(CodeContext context, object o)
	{
		return PythonContext.Hash(o);
	}

	public static int hash(CodeContext context, [NotNull] PythonTuple o)
	{
		return ((IStructuralEquatable)o).GetHashCode(PythonContext.GetContext(context).EqualityComparerNonGeneric);
	}

	public static int hash(CodeContext context, char o)
	{
		return PythonContext.Hash(o);
	}

	public static int hash(CodeContext context, int o)
	{
		return o;
	}

	public static int hash(CodeContext context, [NotNull] string o)
	{
		return o.GetHashCode();
	}

	public static int hash(CodeContext context, [NotNull] ExtensibleString o)
	{
		return hash(context, (object)o);
	}

	public static int hash(CodeContext context, [NotNull] BigInteger o)
	{
		return BigIntegerOps.__hash__(o);
	}

	public static int hash(CodeContext context, [NotNull] Extensible<BigInteger> o)
	{
		return hash(context, (object)o);
	}

	public static int hash(CodeContext context, double o)
	{
		return DoubleOps.__hash__(o);
	}

	public static void help(CodeContext context, object o)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<object> doced = new List<object>();
		help(context, doced, stringBuilder, 0, o);
		if (stringBuilder.Length == 0)
		{
			if (!(o is string))
			{
				help(context, DynamicHelpers.GetPythonType(o));
				return;
			}
			stringBuilder.Append("no documentation found for ");
			stringBuilder.Append(PythonOps.Repr(context, o));
		}
		string[] array = stringBuilder.ToString().Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			PythonOps.Print(context, array[i]);
		}
	}

	private static void help(CodeContext context, List<object> doced, StringBuilder doc, int indent, object obj)
	{
		if (doced.Contains(obj))
		{
			return;
		}
		doced.Add(obj);
		if (obj is string text)
		{
			if (indent != 0)
			{
				return;
			}
			{
				foreach (object value4 in PythonContext.GetContext(context).SystemStateModules.Values)
				{
					IList<object> attrNames = PythonOps.GetAttrNames(context, value4);
					List list = new List();
					foreach (string item in attrNames)
					{
						if (item == text && PythonOps.TryGetBoundAttr(context, value4, text, out var ret))
						{
							list.append(ret);
						}
					}
					PythonType pythonType = null;
					BuiltinFunction builtinFunction = null;
					PythonFunction pythonFunction = null;
					for (int i = 0; i < list.__len__(); i++)
					{
						if ((pythonType = list[i] as PythonType) != null)
						{
							break;
						}
						if ((builtinFunction != null || (builtinFunction = list[i] as BuiltinFunction) == null) && pythonFunction == null)
						{
							pythonFunction = list[i] as PythonFunction;
						}
					}
					if (pythonType != null)
					{
						help(context, doced, doc, indent, pythonType);
					}
					else if (builtinFunction != null)
					{
						help(context, doced, doc, indent, builtinFunction);
					}
					else if (pythonFunction != null)
					{
						help(context, doced, doc, indent, pythonFunction);
					}
				}
				return;
			}
		}
		if (obj is PythonType pythonType2)
		{
			if (indent == 0)
			{
				doc.AppendFormat("Help on {0} in module {1}\n\n", pythonType2.Name, PythonOps.GetBoundAttr(context, pythonType2, "__module__"));
			}
			if (pythonType2.TryResolveSlot(context, "__doc__", out var slot))
			{
				if (slot.TryGetValue(context, null, pythonType2, out var value) && value != null)
				{
					AppendMultiLine(doc, value.ToString() + Environment.NewLine, indent);
				}
				AppendIndent(doc, indent);
				doc.AppendLine("Data and other attributes defined here:");
				AppendIndent(doc, indent);
				doc.AppendLine();
			}
			List memberNames = pythonType2.GetMemberNames(context);
			memberNames.sort(context);
			{
				foreach (string item2 in memberNames)
				{
					if (!(item2 == "__class__") && pythonType2.TryLookupSlot(context, item2, out var slot2) && slot2.TryGetValue(context, null, pythonType2, out var value2))
					{
						help(context, doced, doc, indent + 1, value2);
					}
				}
				return;
			}
		}
		if (obj is BuiltinMethodDescriptor builtinMethodDescriptor)
		{
			if (indent == 0)
			{
				doc.AppendFormat("Help on method-descriptor {0}\n\n", builtinMethodDescriptor.__name__);
			}
			AppendIndent(doc, indent);
			doc.Append(builtinMethodDescriptor.__name__);
			doc.Append("(...)\n");
			AppendMultiLine(doc, builtinMethodDescriptor.__doc__, indent + 1);
			return;
		}
		if (obj is BuiltinFunction builtinFunction2)
		{
			if (indent == 0)
			{
				doc.AppendFormat("Help on built-in function {0}\n\n", builtinFunction2.Name);
			}
			AppendIndent(doc, indent);
			doc.Append(builtinFunction2.Name);
			doc.Append("(...)\n");
			AppendMultiLine(doc, builtinFunction2.__doc__, indent + 1);
			return;
		}
		if (obj is PythonFunction pythonFunction2)
		{
			if (indent == 0)
			{
				doc.AppendFormat("Help on function {0} in module {1}:\n\n", pythonFunction2.__name__, pythonFunction2.__module__);
			}
			AppendIndent(doc, indent);
			doc.Append(pythonFunction2.GetSignatureString());
			string text4 = Converter.ConvertToString(pythonFunction2.__doc__);
			if (!string.IsNullOrEmpty(text4))
			{
				AppendMultiLine(doc, text4, indent);
			}
			return;
		}
		if (obj is Method { im_func: PythonFunction im_func } method)
		{
			if (indent == 0)
			{
				doc.AppendFormat("Help on method {0} in module {1}:\n\n", im_func.__name__, im_func.__module__);
			}
			AppendIndent(doc, indent);
			doc.Append(im_func.GetSignatureString());
			if (method.im_self == null)
			{
				doc.AppendFormat(" unbound {0} method\n", PythonOps.ToString(method.im_class));
			}
			else
			{
				doc.AppendFormat(" method of {0} instance\n", PythonOps.ToString(method.im_class));
			}
			string text5 = Converter.ConvertToString(im_func.__doc__);
			if (!string.IsNullOrEmpty(text5))
			{
				AppendMultiLine(doc, text5, indent);
			}
			return;
		}
		if (obj is PythonModule pythonModule)
		{
			{
				foreach (string key in pythonModule.__dict__.Keys)
				{
					if (!(key == "__class__") && !(key == "__builtins__") && pythonModule.__dict__.TryGetValue(key, out var value3))
					{
						help(context, doced, doc, indent + 1, value3);
					}
				}
				return;
			}
		}
		if (!(obj is OldClass oldClass))
		{
			return;
		}
		if (indent == 0)
		{
			doc.AppendFormat("Help on {0} in module {1}\n\n", oldClass.Name, PythonOps.GetBoundAttr(context, oldClass, "__module__"));
		}
		if (oldClass.TryLookupSlot("__doc__", out var ret2) && ret2 != null)
		{
			AppendMultiLine(doc, ret2.ToString() + Environment.NewLine, indent);
			AppendIndent(doc, indent);
			doc.AppendLine("Data and other attributes defined here:");
			AppendIndent(doc, indent);
			doc.AppendLine();
		}
		IList<object> memberNames2 = ((IPythonMembersList)oldClass).GetMemberNames(context);
		List list2 = new List(memberNames2);
		list2.sort(context);
		memberNames2 = list2;
		foreach (string item3 in memberNames2)
		{
			if (!(item3 == "__class__") && oldClass.TryLookupSlot(item3, out var ret3))
			{
				help(context, doced, doc, indent + 1, ret3);
			}
		}
	}

	private static void AppendMultiLine(StringBuilder doc, string multiline, int indent)
	{
		string[] array = multiline.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			AppendIndent(doc, indent + 1);
			doc.Append(array[i]);
			doc.Append('\n');
		}
	}

	private static void AppendIndent(StringBuilder doc, int indent)
	{
		doc.Append(" |  ");
		for (int i = 0; i < indent; i++)
		{
			doc.Append("    ");
		}
	}

	public static object hex(object o)
	{
		return PythonOps.Hex(o);
	}

	public static object id(object o)
	{
		long num = PythonOps.Id(o);
		if (PythonOps.Id(o) <= int.MaxValue)
		{
			return (int)num;
		}
		return (BigInteger)num;
	}

	[LightThrowing]
	public static object input(CodeContext context)
	{
		return input(context, null);
	}

	[LightThrowing]
	public static object input(CodeContext context, object prompt)
	{
		return eval(context, raw_input(context, prompt));
	}

	public static string intern(object o)
	{
		if (!(o is string text))
		{
			throw PythonOps.TypeError("intern: argument must be string");
		}
		return string.Intern(text);
	}

	public static bool isinstance(object o, [NotNull] PythonType typeinfo)
	{
		return PythonOps.IsInstance(o, typeinfo);
	}

	public static bool isinstance(CodeContext context, object o, [NotNull] PythonTuple typeinfo)
	{
		return PythonOps.IsInstance(context, o, typeinfo);
	}

	public static bool isinstance(CodeContext context, object o, object typeinfo)
	{
		return PythonOps.IsInstance(context, o, typeinfo);
	}

	public static bool issubclass(CodeContext context, [NotNull] OldClass c, object typeinfo)
	{
		return PythonOps.IsSubClass(context, c.TypeObject, typeinfo);
	}

	public static bool issubclass(CodeContext context, [NotNull] PythonType c, object typeinfo)
	{
		return PythonOps.IsSubClass(context, c, typeinfo);
	}

	public static bool issubclass(CodeContext context, [NotNull] PythonType c, [NotNull] PythonType typeinfo)
	{
		return PythonOps.IsSubClass(c, typeinfo);
	}

	[LightThrowing]
	public static object issubclass(CodeContext context, object o, object typeinfo)
	{
		if (typeinfo is PythonTuple pythonTuple)
		{
			foreach (object item in pythonTuple)
			{
				try
				{
					PythonOps.FunctionPushFrame(PythonContext.GetContext(context));
					object obj = issubclass(context, o, item);
					if (obj == ScriptingRuntimeHelpers.True)
					{
						return ScriptingRuntimeHelpers.True;
					}
					if (LightExceptions.IsLightException(obj))
					{
						return obj;
					}
				}
				finally
				{
					PythonOps.FunctionPopFrame();
				}
			}
			return ScriptingRuntimeHelpers.False;
		}
		if (!PythonOps.TryGetBoundAttr(o, "__bases__", out var ret) || !(ret is PythonTuple pythonTuple2))
		{
			return LightExceptions.Throw(PythonOps.TypeError("issubclass() arg 1 must be a class"));
		}
		if (o == typeinfo)
		{
			return ScriptingRuntimeHelpers.True;
		}
		foreach (object item2 in pythonTuple2)
		{
			if (item2 == typeinfo)
			{
				return ScriptingRuntimeHelpers.True;
			}
			if (item2 is PythonType c)
			{
				if (issubclass(context, c, typeinfo))
				{
					return ScriptingRuntimeHelpers.True;
				}
			}
			else if (item2 is OldClass c2)
			{
				if (issubclass(context, c2, typeinfo))
				{
					return ScriptingRuntimeHelpers.True;
				}
			}
			else if (hasattr(context, item2, "__bases__"))
			{
				object obj2 = issubclass(context, item2, typeinfo);
				if (obj2 == ScriptingRuntimeHelpers.True)
				{
					return ScriptingRuntimeHelpers.True;
				}
				if (LightExceptions.IsLightException(obj2))
				{
					return obj2;
				}
			}
		}
		return ScriptingRuntimeHelpers.False;
	}

	public static object iter(CodeContext context, object o)
	{
		return PythonOps.GetEnumeratorObject(context, o);
	}

	public static object iter(CodeContext context, object func, object sentinel)
	{
		if (!PythonOps.IsCallable(context, func))
		{
			throw PythonOps.TypeError("iter(v, w): v must be callable");
		}
		return new SentinelIterator(context, func, sentinel);
	}

	public static int len([NotNull] string str)
	{
		return str.Length;
	}

	public static int len([NotNull] ExtensibleString str)
	{
		return str.__len__();
	}

	public static int len([NotNull] List list)
	{
		return list.__len__();
	}

	public static int len([NotNull] PythonTuple tuple)
	{
		return tuple.__len__();
	}

	public static int len([NotNull] PythonDictionary dict)
	{
		return dict.__len__();
	}

	public static int len([NotNull] ICollection collection)
	{
		return collection.Count;
	}

	public static int len(object o)
	{
		return PythonOps.Length(o);
	}

	public static object locals(CodeContext context)
	{
		PythonDictionary pythonDictionary = context.Dict;
		if (pythonDictionary._storage is ObjectAttributesAdapter objectAttributesAdapter)
		{
			return objectAttributesAdapter.Backing;
		}
		return context.Dict;
	}

	private static CallSite<Func<CallSite, CodeContext, T, T1, object>> MakeMapSite<T, T1>(CodeContext context)
	{
		return CallSite<Func<CallSite, CodeContext, T, T1, object>>.Create(PythonContext.GetContext(context).InvokeOne);
	}

	public static List map(CodeContext context, object func, [NotNull] IEnumerable enumerator)
	{
		IEnumerator enumerator2 = PythonOps.GetEnumerator(enumerator);
		List list = new List();
		CallSite<Func<CallSite, CodeContext, object, object, object>> callSite = null;
		if (func != null)
		{
			callSite = MakeMapSite<object, object>(context);
		}
		while (enumerator2.MoveNext())
		{
			if (func == null)
			{
				list.AddNoLock(enumerator2.Current);
			}
			else
			{
				list.AddNoLock(callSite.Target(callSite, context, func, enumerator2.Current));
			}
		}
		return list;
	}

	public static List map(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object, object>>> storage, object func, [NotNull] string enumerator)
	{
		if (storage.Data == null && func != null)
		{
			storage.Data = MakeMapSite<object, object>(context);
		}
		CallSite<Func<CallSite, CodeContext, object, object, object>> data = storage.Data;
		List list = new List(enumerator.Length);
		foreach (char ch in enumerator)
		{
			if (func == null)
			{
				list.AddNoLock(ScriptingRuntimeHelpers.CharToString(ch));
			}
			else
			{
				list.AddNoLock(data.Target(data, context, func, ScriptingRuntimeHelpers.CharToString(ch)));
			}
		}
		return list;
	}

	public static List map(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, PythonType, object, object>>> storage, [NotNull] PythonType func, [NotNull] string enumerator)
	{
		CallSite<Func<CallSite, CodeContext, PythonType, string, object>> callSite = MakeMapSite<PythonType, string>(context);
		List list = new List(enumerator.Length);
		foreach (char ch in enumerator)
		{
			list.AddNoLock(callSite.Target(callSite, context, func, ScriptingRuntimeHelpers.CharToString(ch)));
		}
		return list;
	}

	public static List map(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, PythonType, object, object>>> storage, [NotNull] PythonType func, [NotNull] IEnumerable enumerator)
	{
		if (storage.Data == null)
		{
			storage.Data = MakeMapSite<PythonType, object>(context);
		}
		CallSite<Func<CallSite, CodeContext, PythonType, object, object>> data = storage.Data;
		IEnumerator enumerator2 = PythonOps.GetEnumerator(enumerator);
		List list = new List();
		while (enumerator2.MoveNext())
		{
			list.AddNoLock(data.Target(data, context, func, enumerator2.Current));
		}
		return list;
	}

	public static List map(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>>> storage, [NotNull] BuiltinFunction func, [NotNull] string enumerator)
	{
		if (storage.Data == null)
		{
			storage.Data = MakeMapSite<BuiltinFunction, object>(context);
		}
		CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>> data = storage.Data;
		List list = new List(enumerator.Length);
		foreach (char ch in enumerator)
		{
			list.AddNoLock(data.Target(data, context, func, ScriptingRuntimeHelpers.CharToString(ch)));
		}
		return list;
	}

	public static List map(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>>> storage, [NotNull] BuiltinFunction func, [NotNull] IEnumerable enumerator)
	{
		if (storage.Data == null)
		{
			storage.Data = MakeMapSite<BuiltinFunction, object>(context);
		}
		CallSite<Func<CallSite, CodeContext, BuiltinFunction, object, object>> data = storage.Data;
		IEnumerator enumerator2 = PythonOps.GetEnumerator(enumerator);
		List list = new List();
		while (enumerator2.MoveNext())
		{
			list.AddNoLock(data.Target(data, context, func, enumerator2.Current));
		}
		return list;
	}

	public static List map(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, PythonFunction, object, object>>> storage, [NotNull] PythonFunction func, [NotNull] IList enumerator)
	{
		if (storage.Data == null)
		{
			storage.Data = MakeMapSite<PythonFunction, object>(context);
		}
		CallSite<Func<CallSite, CodeContext, PythonFunction, object, object>> data = storage.Data;
		IEnumerator enumerator2 = PythonOps.GetEnumerator(enumerator);
		List list = new List(enumerator.Count);
		while (enumerator2.MoveNext())
		{
			list.AddNoLock(data.Target(data, context, func, enumerator2.Current));
		}
		return list;
	}

	public static List map(CodeContext context, params object[] param)
	{
		if (param == null || param.Length < 2)
		{
			throw PythonOps.TypeError("at least 2 arguments required to map");
		}
		List list = new List();
		object obj = param[0];
		IEnumerator[] array = new IEnumerator[param.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PythonOps.GetEnumerator(param[i + 1]);
		}
		object[] array2 = new object[array.Length];
		while (true)
		{
			bool flag = true;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].MoveNext())
				{
					array2[j] = array[j].Current;
					flag = false;
				}
				else
				{
					array2[j] = null;
				}
			}
			if (flag)
			{
				break;
			}
			if (obj != null)
			{
				list.AddNoLock(PythonCalls.Call(context, obj, array2));
				continue;
			}
			if (array2.Length == 1)
			{
				list.AddNoLock(array2[0]);
				continue;
			}
			list.AddNoLock(PythonTuple.MakeTuple(array2));
			array2 = new object[array.Length];
		}
		return list;
	}

	public static object max(CodeContext context, object x)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(x);
		if (!enumerator.MoveNext())
		{
			throw PythonOps.ValueError("max() arg is an empty sequence");
		}
		object current = enumerator.Current;
		PythonContext context2 = PythonContext.GetContext(context);
		while (enumerator.MoveNext())
		{
			if (context2.GreaterThan(enumerator.Current, current))
			{
				current = enumerator.Current;
			}
		}
		return current;
	}

	public static object max(CodeContext context, object x, object y)
	{
		if (!PythonContext.GetContext(context).GreaterThan(x, y))
		{
			return y;
		}
		return x;
	}

	public static object max(CodeContext context, params object[] args)
	{
		if (args.Length > 0)
		{
			object obj = args[0];
			if (args.Length == 1)
			{
				return max(context, obj);
			}
			PythonContext context2 = PythonContext.GetContext(context);
			for (int i = 1; i < args.Length; i++)
			{
				if (context2.GreaterThan(args[i], obj))
				{
					obj = args[i];
				}
			}
			return obj;
		}
		throw PythonOps.TypeError("max expecting 1 arguments, got 0");
	}

	public static object max(CodeContext context, object x, [ParamDictionary] IDictionary<object, object> dict)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(x);
		if (!enumerator.MoveNext())
		{
			throw PythonOps.ValueError(" max() arg is an empty sequence");
		}
		object maxKwArg = GetMaxKwArg(dict);
		object current = enumerator.Current;
		object other = PythonCalls.Call(context, maxKwArg, enumerator.Current);
		PythonContext context2 = PythonContext.GetContext(context);
		while (enumerator.MoveNext())
		{
			object obj = PythonCalls.Call(context, maxKwArg, enumerator.Current);
			if (context2.GreaterThan(obj, other))
			{
				current = enumerator.Current;
				other = obj;
			}
		}
		return current;
	}

	public static object max(CodeContext context, object x, object y, [ParamDictionary] IDictionary<object, object> dict)
	{
		object maxKwArg = GetMaxKwArg(dict);
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.GreaterThan(PythonCalls.Call(context, maxKwArg, x), PythonCalls.Call(context, maxKwArg, y)))
		{
			return y;
		}
		return x;
	}

	public static object max(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
		if (args.Length > 0)
		{
			int num = 0;
			if (args.Length == 1)
			{
				return max(context, args[num], dict);
			}
			object maxKwArg = GetMaxKwArg(dict);
			object other = PythonCalls.Call(context, maxKwArg, args[num]);
			PythonContext context2 = PythonContext.GetContext(context);
			for (int i = 1; i < args.Length; i++)
			{
				object obj = PythonCalls.Call(context, maxKwArg, args[i]);
				if (context2.GreaterThan(obj, other))
				{
					num = i;
					other = obj;
				}
			}
			return args[num];
		}
		throw PythonOps.TypeError("max expecting 1 arguments, got 0");
	}

	private static object GetMaxKwArg(IDictionary<object, object> dict)
	{
		if (dict.Count != 1)
		{
			throw PythonOps.TypeError(" max() should have only 1 keyword argument, but got {0} keyword arguments", dict.Count);
		}
		return VerifyKeys("max", dict);
	}

	public static object min(CodeContext context, object x)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(x);
		if (!enumerator.MoveNext())
		{
			throw PythonOps.ValueError("empty sequence");
		}
		object current = enumerator.Current;
		PythonContext context2 = PythonContext.GetContext(context);
		while (enumerator.MoveNext())
		{
			if (context2.LessThan(enumerator.Current, current))
			{
				current = enumerator.Current;
			}
		}
		return current;
	}

	public static object min(CodeContext context, object x, object y)
	{
		if (!PythonContext.GetContext(context).LessThan(x, y))
		{
			return y;
		}
		return x;
	}

	public static object min(CodeContext context, params object[] args)
	{
		if (args.Length > 0)
		{
			object obj = args[0];
			if (args.Length == 1)
			{
				return min(context, obj);
			}
			PythonContext context2 = PythonContext.GetContext(context);
			for (int i = 1; i < args.Length; i++)
			{
				if (context2.LessThan(args[i], obj))
				{
					obj = args[i];
				}
			}
			return obj;
		}
		throw PythonOps.TypeError("min expecting 1 arguments, got 0");
	}

	public static object min(CodeContext context, object x, [ParamDictionary] IDictionary<object, object> dict)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(x);
		if (!enumerator.MoveNext())
		{
			throw PythonOps.ValueError(" min() arg is an empty sequence");
		}
		object minKwArg = GetMinKwArg(dict);
		object current = enumerator.Current;
		object other = PythonCalls.Call(context, minKwArg, enumerator.Current);
		PythonContext context2 = PythonContext.GetContext(context);
		while (enumerator.MoveNext())
		{
			object obj = PythonCalls.Call(context, minKwArg, enumerator.Current);
			if (context2.LessThan(obj, other))
			{
				current = enumerator.Current;
				other = obj;
			}
		}
		return current;
	}

	public static object min(CodeContext context, object x, object y, [ParamDictionary] IDictionary<object, object> dict)
	{
		object minKwArg = GetMinKwArg(dict);
		if (!PythonContext.GetContext(context).LessThan(PythonCalls.Call(context, minKwArg, x), PythonCalls.Call(context, minKwArg, y)))
		{
			return y;
		}
		return x;
	}

	public static object min(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
		if (args.Length > 0)
		{
			int num = 0;
			if (args.Length == 1)
			{
				return min(context, args[num], dict);
			}
			object minKwArg = GetMinKwArg(dict);
			object other = PythonCalls.Call(context, minKwArg, args[num]);
			PythonContext context2 = PythonContext.GetContext(context);
			for (int i = 1; i < args.Length; i++)
			{
				object obj = PythonCalls.Call(context, minKwArg, args[i]);
				if (context2.LessThan(obj, other))
				{
					num = i;
					other = obj;
				}
			}
			return args[num];
		}
		throw PythonOps.TypeError("min expecting 1 arguments, got 0");
	}

	private static object GetMinKwArg([ParamDictionary] IDictionary<object, object> dict)
	{
		if (dict.Count != 1)
		{
			throw PythonOps.TypeError(" min() should have only 1 keyword argument, but got {0} keyword arguments", dict.Count);
		}
		return VerifyKeys("min", dict);
	}

	private static object VerifyKeys(string name, IDictionary<object, object> dict)
	{
		if (!dict.TryGetValue("key", out var value))
		{
			ICollection<object> keys = dict.Keys;
			IEnumerator<object> enumerator = keys.GetEnumerator();
			if (enumerator.MoveNext())
			{
				throw PythonOps.TypeError(" {1}() got an unexpected keyword argument ({0})", enumerator.Current, name);
			}
		}
		return value;
	}

	public static object next(IEnumerator iter)
	{
		if (iter.MoveNext())
		{
			return iter.Current;
		}
		throw PythonOps.StopIteration();
	}

	public static object next(IEnumerator iter, object defaultVal)
	{
		if (iter.MoveNext())
		{
			return iter.Current;
		}
		return defaultVal;
	}

	[LightThrowing]
	public static object next(PythonGenerator gen)
	{
		return gen.next();
	}

	[LightThrowing]
	public static object next(PythonGenerator gen, object defaultVal)
	{
		object obj = gen.next();
		Exception lightException = LightExceptions.GetLightException(obj);
		if (lightException != null && lightException is StopIterationException)
		{
			return defaultVal;
		}
		return obj;
	}

	public static object next(CodeContext context, object iter)
	{
		return PythonOps.Invoke(context, iter, "next");
	}

	public static object next(CodeContext context, object iter, object defaultVal)
	{
		try
		{
			return PythonOps.Invoke(context, iter, "next");
		}
		catch (StopIterationException)
		{
			return defaultVal;
		}
	}

	public static object oct(object o)
	{
		return PythonOps.Oct(o);
	}

	public static PythonFile open(CodeContext context, string name, [DefaultParameterValue("r")] string mode, [DefaultParameterValue(-1)] int buffering)
	{
		PythonFile pythonFile = new PythonFile(context);
		pythonFile.__init__(context, name, mode, buffering);
		return pythonFile;
	}

	public static PythonFile open(CodeContext context, [NotNull] Stream stream)
	{
		PythonFile pythonFile = new PythonFile(context);
		pythonFile.__init__(context, stream);
		return pythonFile;
	}

	public static int ord(object value)
	{
		if (value is char)
		{
			return (char)value;
		}
		string text = value as string;
		if (text == null && value is ExtensibleString extensibleString)
		{
			text = extensibleString.Value;
		}
		if (text != null)
		{
			if (text.Length != 1)
			{
				throw PythonOps.TypeError("expected a character, but string of length {0} found", text.Length);
			}
			return text[0];
		}
		if (value is IList<byte> list)
		{
			if (list.Count != 1)
			{
				throw PythonOps.TypeError("expected a character, but string of length {0} found", list.Count);
			}
			return list[0];
		}
		throw PythonOps.TypeError("expected a character, but {0} found", PythonTypeOps.GetName(value));
	}

	public static object pow(CodeContext context, object x, object y)
	{
		return PythonContext.GetContext(context).Operation(PythonOperationKind.Power, x, y);
	}

	public static object pow(CodeContext context, object x, object y, object z)
	{
		try
		{
			return PythonOps.PowerMod(context, x, y, z);
		}
		catch (DivideByZeroException)
		{
			throw PythonOps.ValueError("3rd argument cannot be 0");
		}
	}

	public static void print(CodeContext context, params object[] args)
	{
		print(context, " ", "\n", null, args);
	}

	public static void print(CodeContext context, [ParamDictionary] IDictionary<object, object> kwargs, params object[] args)
	{
		object obj = AttrCollectionPop(kwargs, "sep", " ");
		if (obj != null && !(obj is string))
		{
			throw PythonOps.TypeError("sep must be None or str, not {0}", PythonTypeOps.GetName(obj));
		}
		object obj2 = AttrCollectionPop(kwargs, "end", "\n");
		if (obj != null && !(obj is string))
		{
			throw PythonOps.TypeError("end must be None or str, not {0}", PythonTypeOps.GetName(obj2));
		}
		object obj3 = AttrCollectionPop(kwargs, "file", null);
		if (kwargs.Count != 0)
		{
			throw PythonOps.TypeError("'{0}' is an invalid keyword argument for this function", new List<object>(kwargs.Keys)[0]);
		}
		print(context, ((string)obj) ?? " ", ((string)obj2) ?? "\n", obj3, args);
	}

	private static object AttrCollectionPop(IDictionary<object, object> kwargs, string name, object defaultValue)
	{
		if (kwargs.TryGetValue(name, out var value))
		{
			kwargs.Remove(name);
			return value;
		}
		return defaultValue;
	}

	private static void print(CodeContext context, string sep, string end, object file, object[] args)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (file == null)
		{
			file = context2.SystemStandardOut;
		}
		if (file == null)
		{
			throw PythonOps.RuntimeError("lost sys.std_out");
		}
		if (args == null)
		{
			args = new object[1];
		}
		PythonFile pythonFile = file as PythonFile;
		for (int i = 0; i < args.Length; i++)
		{
			string text = PythonOps.ToString(context, args[i]);
			if (pythonFile != null)
			{
				pythonFile.write(text);
			}
			else
			{
				context2.WriteCallSite.Target(context2.WriteCallSite, context, PythonOps.GetBoundAttr(context, file, "write"), text);
			}
			if (i != args.Length - 1)
			{
				if (pythonFile != null)
				{
					pythonFile.write(sep);
				}
				else
				{
					context2.WriteCallSite.Target(context2.WriteCallSite, context, PythonOps.GetBoundAttr(context, file, "write"), sep);
				}
			}
		}
		if (pythonFile != null)
		{
			pythonFile.write(end);
		}
		else
		{
			context2.WriteCallSite.Target(context2.WriteCallSite, context, PythonOps.GetBoundAttr(context, file, "write"), end);
		}
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(int stop)
	{
		return rangeWorker(stop);
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(BigInteger stop)
	{
		return rangeWorker(stop);
	}

	private static List rangeWorker(int stop)
	{
		if (stop < 0)
		{
			stop = 0;
		}
		List list = PythonOps.MakeEmptyList(stop);
		for (int i = 0; i < stop; i++)
		{
			list.AddNoLock(ScriptingRuntimeHelpers.Int32ToObject(i));
		}
		return list;
	}

	private static List rangeWorker(BigInteger stop)
	{
		if (stop < BigInteger.Zero)
		{
			return range(0);
		}
		if (stop.AsInt32(out var ret))
		{
			return range(ret);
		}
		throw PythonOps.OverflowError("too many items in the range");
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(int start, int stop)
	{
		return rangeWorker(start, stop);
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(BigInteger start, BigInteger stop)
	{
		return rangeWorker(start, stop);
	}

	private static List rangeWorker(int start, int stop)
	{
		if (start > stop)
		{
			stop = start;
		}
		long num = (long)stop - (long)start;
		if (int.MinValue <= num && num <= int.MaxValue)
		{
			List list = PythonOps.MakeEmptyList(stop - start);
			for (int i = start; i < stop; i++)
			{
				list.AddNoLock(ScriptingRuntimeHelpers.Int32ToObject(i));
			}
			return list;
		}
		throw PythonOps.OverflowError("too many items in the list");
	}

	private static List rangeWorker(BigInteger start, BigInteger stop)
	{
		if (start > stop)
		{
			stop = start;
		}
		BigInteger self = stop - start;
		if (self.AsInt32(out var ret))
		{
			List list = PythonOps.MakeEmptyList(ret);
			for (int i = 0; i < ret; i++)
			{
				list.AddNoLock(start + i);
			}
			return list;
		}
		throw PythonOps.OverflowError("too many items in the range");
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(int start, int stop, int step)
	{
		return rangeWorker(start, stop, step);
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(BigInteger start, BigInteger stop, BigInteger step)
	{
		return rangeWorker(start, stop, step);
	}

	private static List rangeWorker(int start, int stop, int step)
	{
		if (step == 0)
		{
			throw PythonOps.ValueError("step of 0");
		}
		List list;
		if (step > 0)
		{
			if (start > stop)
			{
				stop = start;
			}
			list = PythonOps.MakeEmptyList((stop - start) / step);
			for (int i = start; i < stop; i += step)
			{
				list.AddNoLock(ScriptingRuntimeHelpers.Int32ToObject(i));
			}
		}
		else
		{
			if (start < stop)
			{
				stop = start;
			}
			list = PythonOps.MakeEmptyList((stop - start) / step);
			for (int j = start; j > stop; j += step)
			{
				list.AddNoLock(ScriptingRuntimeHelpers.Int32ToObject(j));
			}
		}
		return list;
	}

	private static List rangeWorker(BigInteger start, BigInteger stop, BigInteger step)
	{
		if (step == BigInteger.Zero)
		{
			throw PythonOps.ValueError("step of 0");
		}
		BigInteger self;
		if (step > BigInteger.Zero)
		{
			if (start > stop)
			{
				stop = start;
			}
			self = (stop - start + step - 1) / step;
		}
		else
		{
			if (start < stop)
			{
				stop = start;
			}
			self = (stop - start + step + 1) / step;
		}
		if (self.AsInt32(out var ret))
		{
			List list = PythonOps.MakeEmptyList(ret);
			for (int i = 0; i < ret; i++)
			{
				list.AddNoLock(start);
				start += step;
			}
			return list;
		}
		throw PythonOps.OverflowError("too many items for list");
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(CodeContext context, object stop)
	{
		return range(GetRangeInt(context, stop, "end"));
	}

	[return: SequenceTypeInfo(new Type[] { typeof(int) })]
	public static List range(CodeContext context, object start, object stop, [DefaultParameterValue(1)] object step)
	{
		BigInteger rangeInt = GetRangeInt(context, stop, "end");
		BigInteger rangeInt2 = GetRangeInt(context, start, "start");
		BigInteger rangeInt3 = GetRangeInt(context, step, "step");
		return range(rangeInt2, rangeInt, rangeInt3);
	}

	private static bool FastGetRangeInt(object arg, out BigInteger res)
	{
		if (arg is int)
		{
			res = (int)arg;
			return true;
		}
		if (arg is BigInteger)
		{
			res = (BigInteger)arg;
			return true;
		}
		if (arg is Extensible<int> extensible)
		{
			res = extensible.Value;
			return true;
		}
		if (arg is Extensible<BigInteger> extensible2)
		{
			res = extensible2.Value;
			return true;
		}
		res = BigInteger.Zero;
		return false;
	}

	private static BigInteger GetRangeInt(CodeContext context, object arg, string pos)
	{
		if (FastGetRangeInt(arg, out var res))
		{
			return res;
		}
		if (arg is double || arg is Extensible<double>)
		{
			throw PythonOps.TypeError("range() integer {0} argument expected, got {1}.", pos, PythonTypeOps.GetName(arg));
		}
		if (PythonOps.TryGetBoundAttr(context, arg, "__int__", out var ret))
		{
			if (!FastGetRangeInt(PythonOps.CallWithContext(context, ret), out res))
			{
				throw PythonOps.TypeError("__int__ should return int object");
			}
			return res;
		}
		if (arg is OldInstance)
		{
			if (PythonOps.TryGetBoundAttr(context, arg, "__trunc__", out ret))
			{
				if (!FastGetRangeInt(PythonOps.CallWithContext(context, ret), out res))
				{
					throw PythonOps.TypeError("__trunc__ returned non-Integral (type {0})", PythonTypeOps.GetOldName(arg));
				}
				return res;
			}
			throw PythonOps.AttributeError("{0} instance has no attribute __trunc__", PythonTypeOps.GetOldName(arg));
		}
		throw PythonOps.TypeError("range() integer {0} argument expected, got {1}.", pos, PythonTypeOps.GetName(arg));
	}

	public static string raw_input(CodeContext context)
	{
		return raw_input(context, null);
	}

	public static string raw_input(CodeContext context, object prompt)
	{
		if (prompt != null)
		{
			PythonOps.PrintNoNewline(context, prompt);
		}
		string text = PythonOps.ReadLineFromSrc(context, PythonContext.GetContext(context).SystemStandardIn) as string;
		if (text != null && text.EndsWith("\n"))
		{
			return text.Substring(0, text.Length - 1);
		}
		return text;
	}

	public static object reduce(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object, object, object>>> siteData, object func, object seq)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(seq);
		if (!enumerator.MoveNext())
		{
			throw PythonOps.TypeError("reduce() of empty sequence with no initial value");
		}
		EnsureReduceData(context, siteData);
		CallSite<Func<CallSite, CodeContext, object, object, object, object>> data = siteData.Data;
		object obj = enumerator.Current;
		while (enumerator.MoveNext())
		{
			obj = data.Target(data, context, func, obj, enumerator.Current);
		}
		return obj;
	}

	public static object reduce(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object, object, object>>> siteData, object func, object seq, object initializer)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(seq);
		EnsureReduceData(context, siteData);
		CallSite<Func<CallSite, CodeContext, object, object, object, object>> data = siteData.Data;
		object obj = initializer;
		while (enumerator.MoveNext())
		{
			obj = data.Target(data, context, func, obj, enumerator.Current);
		}
		return obj;
	}

	private static void EnsureReduceData(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object, object, object>>> siteData)
	{
		if (siteData.Data == null)
		{
			siteData.Data = CallSite<Func<CallSite, CodeContext, object, object, object, object>>.Create(PythonContext.GetContext(context).Invoke(new CallSignature(2)));
		}
	}

	public static object reload(CodeContext context, PythonModule module)
	{
		if (module == null)
		{
			throw PythonOps.TypeError("unexpected type: NoneType");
		}
		if (_reloadStack == null)
		{
			Interlocked.CompareExchange(ref _reloadStack, new List<PythonModule>(), null);
		}
		if (_reloadStack.Contains(module))
		{
			return module;
		}
		_reloadStack.Add(module);
		try
		{
			return Importer.ReloadModule(context, module);
		}
		finally
		{
			_reloadStack.RemoveAt(_reloadStack.Count - 1);
		}
	}

	public static object repr(CodeContext context, object o)
	{
		object obj = PythonOps.Repr(context, o);
		if (!(obj is string) && !(obj is ExtensibleString))
		{
			throw PythonOps.TypeError("__repr__ returned non-string (type {0})", PythonOps.GetPythonTypeName(o));
		}
		return obj;
	}

	public static double round(double number)
	{
		return MathUtils.RoundAwayFromZero(number);
	}

	public static double round(double number, int ndigits)
	{
		return PythonOps.CheckMath(number, MathUtils.RoundAwayFromZero(number, ndigits));
	}

	public static double round(double number, BigInteger ndigits)
	{
		if (ndigits.AsInt32(out var ret))
		{
			return round(number, ret);
		}
		if (!(ndigits > 0L))
		{
			return 0.0;
		}
		return number;
	}

	public static double round(double number, double ndigits)
	{
		throw PythonOps.TypeError("'float' object cannot be interpreted as an index");
	}

	public static void setattr(CodeContext context, object o, string name, object val)
	{
		PythonOps.SetAttr(context, o, name, val);
	}

	public static List sorted(CodeContext context, object iterable)
	{
		return sorted(context, iterable, null, null, reverse: false);
	}

	public static List sorted(CodeContext context, object iterable, object cmp)
	{
		return sorted(context, iterable, cmp, null, reverse: false);
	}

	public static List sorted(CodeContext context, object iterable, object cmp, object key)
	{
		return sorted(context, iterable, cmp, key, reverse: false);
	}

	public static List sorted(CodeContext context, [DefaultParameterValue(null)] object iterable, [DefaultParameterValue(null)] object cmp, [DefaultParameterValue(null)] object key, [DefaultParameterValue(false)] bool reverse)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
		List list = PythonOps.MakeEmptyList(10);
		while (enumerator.MoveNext())
		{
			list.AddNoLock(enumerator.Current);
		}
		list.sort(context, cmp, key, reverse);
		return list;
	}

	public static object sum(CodeContext context, object sequence)
	{
		return sum(context, sequence, 0);
	}

	public static object sum(CodeContext context, [NotNull] List sequence)
	{
		return sum(context, sequence, 0);
	}

	public static object sum(CodeContext context, [NotNull] PythonTuple sequence)
	{
		return sum(context, sequence, 0);
	}

	public static object sum(CodeContext context, object sequence, object start)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(sequence);
		if (start is string)
		{
			throw PythonOps.TypeError("Cannot sum strings, use '{0}'.join(seq)", start);
		}
		SumState state = new SumState(context.LanguageContext, start);
		while (enumerator.MoveNext())
		{
			SumOne(ref state, enumerator.Current);
		}
		return state.CurrentValue;
	}

	public static object sum(CodeContext context, [NotNull] List sequence, object start)
	{
		if (start is string)
		{
			throw PythonOps.TypeError("Cannot sum strings, use '{0}'.join(seq)", start);
		}
		SumState state = new SumState(context.LanguageContext, start);
		for (int i = 0; i < sequence._size; i++)
		{
			SumOne(ref state, sequence._data[i]);
		}
		return state.CurrentValue;
	}

	public static object sum(CodeContext context, [NotNull] PythonTuple sequence, object start)
	{
		if (start is string)
		{
			throw PythonOps.TypeError("Cannot sum strings, use '{0}'.join(seq)", start);
		}
		SumState state = new SumState(context.LanguageContext, start);
		object[] data = sequence._data;
		for (int i = 0; i < data.Length; i++)
		{
			SumOne(ref state, data[i]);
		}
		return state.CurrentValue;
	}

	private static void SumOne(ref SumState state, object current)
	{
		checked
		{
			if (current != null)
			{
				if (state.CurType == SumVariantType.Int)
				{
					if (current.GetType() == typeof(int))
					{
						try
						{
							state.IntVal += (int)current;
							return;
						}
						catch (OverflowException)
						{
							state.BigIntVal = (BigInteger)state.IntVal + (BigInteger)(int)current;
							state.CurType = SumVariantType.BigInt;
							return;
						}
					}
					if (current.GetType() == typeof(double))
					{
						state.DoubleVal = (double)state.IntVal + (double)current;
						state.CurType = SumVariantType.Double;
					}
					else if (current.GetType() == typeof(BigInteger))
					{
						state.BigIntVal = state.IntVal + (BigInteger)current;
						state.CurType = SumVariantType.BigInt;
					}
					else
					{
						SumObject(ref state, state.IntVal, current);
					}
				}
				else if (state.CurType == SumVariantType.Double)
				{
					if (current.GetType() == typeof(double))
					{
						state.DoubleVal += (double)current;
					}
					else if (current.GetType() == typeof(int))
					{
						state.DoubleVal += (int)current;
					}
					else if (current.GetType() == typeof(BigInteger))
					{
						SumBigIntAndDouble(ref state, (BigInteger)current, state.DoubleVal);
					}
					else
					{
						SumObject(ref state, state.DoubleVal, current);
					}
				}
				else if (state.CurType == SumVariantType.BigInt)
				{
					if (current.GetType() == typeof(BigInteger))
					{
						state.BigIntVal += (BigInteger)current;
					}
					else if (current.GetType() == typeof(int))
					{
						state.BigIntVal += (BigInteger)(int)current;
					}
					else if (current.GetType() == typeof(double))
					{
						SumBigIntAndDouble(ref state, state.BigIntVal, (double)current);
					}
					else
					{
						SumObject(ref state, state.BigIntVal, current);
					}
				}
				else if (state.CurType == SumVariantType.Object)
				{
					state.ObjectVal = state.AddSite.Target(state.AddSite, state.ObjectVal, current);
				}
			}
			else
			{
				SumObject(ref state, state.BigIntVal, current);
			}
		}
	}

	private static void SumBigIntAndDouble(ref SumState state, BigInteger bigInt, double dbl)
	{
		if (bigInt <= MaxDouble && bigInt >= MinDouble)
		{
			state.DoubleVal = (double)bigInt + dbl;
			state.CurType = SumVariantType.Double;
		}
		else
		{
			SumObject(ref state, dbl, bigInt);
		}
	}

	private static void SumObject(ref SumState state, object value, object current)
	{
		state.ObjectVal = state.AddSite.Target(state.AddSite, value, current);
		state.CurType = SumVariantType.Object;
	}

	public static string unichr(int i)
	{
		if (i < 0 || i > 65535)
		{
			throw PythonOps.ValueError("{0} is not in required range", i);
		}
		return ScriptingRuntimeHelpers.CharToString((char)i);
	}

	[Documentation("vars([object]) -> dictionary\n\nWithout arguments, equivalent to locals().\nWith an argument, equivalent to object.__dict__.")]
	public static object vars(CodeContext context)
	{
		return locals(context);
	}

	public static object vars(CodeContext context, object @object)
	{
		if (!PythonOps.TryGetBoundAttr(@object, "__dict__", out var ret))
		{
			throw PythonOps.TypeError("vars() argument must have __dict__ attribute");
		}
		return ret;
	}

	public static List zip(object s0, object s1)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(s0);
		IEnumerator enumerator2 = PythonOps.GetEnumerator(s1);
		List list = new List();
		while (enumerator.MoveNext())
		{
			object current = enumerator.Current;
			if (!enumerator2.MoveNext())
			{
				break;
			}
			object current2 = enumerator2.Current;
			list.AddNoLock(PythonTuple.MakeTuple(current, current2));
		}
		return list;
	}

	public static List zip(params object[] seqs)
	{
		if (seqs == null)
		{
			throw PythonOps.TypeError("zip argument must support iteration, got None");
		}
		int num = seqs.Length;
		switch (num)
		{
		case 2:
			return zip(seqs[0], seqs[1]);
		case 0:
			return PythonOps.MakeList();
		default:
		{
			IEnumerator[] array = new IEnumerator[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = PythonOps.GetEnumerator(seqs[i]);
			}
			List list = new List();
			while (true)
			{
				object[] array2 = new object[num];
				for (int j = 0; j < num; j++)
				{
					if (!array[j].MoveNext())
					{
						return list;
					}
					array2[j] = array[j].Current;
				}
				list.AddNoLock(PythonTuple.MakeTuple(array2));
			}
		}
		}
	}

	internal static PythonCompilerOptions GetRuntimeGeneratedCodeCompilerOptions(CodeContext context, bool inheritContext, CompileFlags cflags)
	{
		PythonCompilerOptions pythonCompilerOptions = ((!inheritContext) ? DefaultContext.DefaultPythonContext.GetPythonCompilerOptions() : new PythonCompilerOptions(context.ModuleContext.Features));
		if ((cflags & (CompileFlags.CO_FUTURE_DIVISION | CompileFlags.CO_FUTURE_ABSOLUTE_IMPORT | CompileFlags.CO_FUTURE_WITH_STATEMENT)) != 0)
		{
			ModuleOptions moduleOptions = ModuleOptions.None;
			if ((cflags & CompileFlags.CO_FUTURE_DIVISION) != 0)
			{
				moduleOptions |= ModuleOptions.TrueDivision;
			}
			if ((cflags & CompileFlags.CO_FUTURE_WITH_STATEMENT) != 0)
			{
				moduleOptions |= ModuleOptions.WithStatement;
			}
			if ((cflags & CompileFlags.CO_FUTURE_ABSOLUTE_IMPORT) != 0)
			{
				moduleOptions |= ModuleOptions.AbsoluteImports;
			}
			pythonCompilerOptions.Module |= moduleOptions;
		}
		pythonCompilerOptions.Module &= ~(ModuleOptions.Optimized | ModuleOptions.Initialize);
		pythonCompilerOptions.Module |= ModuleOptions.ExecOrEvalCode | ModuleOptions.Interpret;
		pythonCompilerOptions.CompilationMode = CompilationMode.Lookup;
		return pythonCompilerOptions;
	}

	private static bool GetCompilerInheritance(object dontInherit)
	{
		if (dontInherit != null)
		{
			return Converter.ConvertToInt32(dontInherit) == 0;
		}
		return true;
	}

	private static CompileFlags GetCompilerFlags(int flags)
	{
		if ((flags & -61969) != 0)
		{
			throw PythonOps.ValueError("unrecognized flags");
		}
		return (CompileFlags)flags;
	}

	private static CodeContext GetExecEvalScopeOptional(CodeContext context, PythonDictionary globals, object localsDict, bool copyModule)
	{
		if (globals == null)
		{
			globals = Builtin.globals(context);
		}
		if (localsDict == null)
		{
			localsDict = locals(context);
		}
		return GetExecEvalScope(context, globals, GetAttrLocals(context, localsDict), copyModule, setBuiltinsToModule: true);
	}

	internal static CodeContext GetExecEvalScope(CodeContext context, PythonDictionary globals, PythonDictionary locals, bool copyModule, bool setBuiltinsToModule)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		ModuleContext moduleContext = new ModuleContext(PythonDictionary.FromIAC(context, globals), context.LanguageContext);
		CodeContext result = ((locals != null) ? new CodeContext(PythonDictionary.FromIAC(context, locals), moduleContext) : moduleContext.GlobalContext);
		if (!globals.ContainsKey("__builtins__"))
		{
			if (setBuiltinsToModule)
			{
				globals["__builtins__"] = context2.SystemStateModules["__builtin__"];
			}
			else
			{
				globals["__builtins__"] = context2.BuiltinModuleDict;
			}
		}
		return result;
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		dict["__debug__"] = ScriptingRuntimeHelpers.BooleanToObject(!context.PythonOptions.Optimize);
	}
}
