using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[DebuggerDisplay("function {__name__} in {__module__}")]
[PythonType("function")]
[DontMapGetMemberNamesToDir]
public sealed class PythonFunction : PythonTypeSlot, IWeakReferenceable, IPythonMembersList, IMembersList, IDynamicMetaObjectProvider, ICodeFormattable, IFastInvokable
{
	private class FunctionCallerKey : IEquatable<FunctionCallerKey>
	{
		public readonly Type CallerType;

		public readonly int FunctionCompat;

		public FunctionCallerKey(Type callerType, int compat)
		{
			CallerType = callerType;
			FunctionCompat = compat;
		}

		public bool Equals(FunctionCallerKey other)
		{
			if (CallerType == other.CallerType)
			{
				return FunctionCompat == other.FunctionCompat;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return CallerType.GetHashCode() ^ FunctionCompat;
		}

		public override bool Equals(object obj)
		{
			if (obj is FunctionCallerKey other)
			{
				return Equals(other);
			}
			return false;
		}
	}

	private static Dictionary<FunctionCallerKey, FunctionCaller> _functionCallers = new Dictionary<FunctionCallerKey, FunctionCaller>();

	private readonly CodeContext _context;

	[PythonHidden]
	public readonly MutableTuple Closure;

	private object[] _defaults;

	internal PythonDictionary _dict;

	private object _module;

	internal int _id;

	internal int _compat;

	private FunctionCode _code;

	private string _name;

	private object _doc;

	private static int[] _depth_fast = new int[20];

	[ThreadStatic]
	private static int DepthSlow;

	private static int _CurrentId = 1;

	public object __globals__
	{
		get
		{
			return func_globals;
		}
		set
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public object func_globals
	{
		get
		{
			return _context.GlobalDict;
		}
		set
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public PythonTuple __defaults__
	{
		get
		{
			return func_defaults;
		}
		set
		{
			func_defaults = value;
		}
	}

	public PythonTuple func_defaults
	{
		get
		{
			if (_defaults.Length == 0)
			{
				return null;
			}
			return new PythonTuple(_defaults);
		}
		set
		{
			if (value == null)
			{
				_defaults = ArrayUtils.EmptyObjects;
			}
			else
			{
				_defaults = value.ToArray();
			}
			_compat = CalculatedCachedCompat();
		}
	}

	public PythonTuple __closure__
	{
		get
		{
			return func_closure;
		}
		set
		{
			func_closure = value;
		}
	}

	public PythonTuple func_closure
	{
		get
		{
			if (Context.Dict._storage is RuntimeVariablesDictionaryStorage runtimeVariablesDictionaryStorage)
			{
				object[] array = new object[runtimeVariablesDictionaryStorage.Names.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = runtimeVariablesDictionaryStorage.GetCell(i);
				}
				return PythonTuple.MakeTuple(array);
			}
			return null;
		}
		set
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public string __name__
	{
		get
		{
			return func_name;
		}
		set
		{
			func_name = value;
		}
	}

	public string func_name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value == null)
			{
				throw PythonOps.TypeError("func_name must be set to a string object");
			}
			_name = value;
		}
	}

	public PythonDictionary __dict__
	{
		get
		{
			return func_dict;
		}
		set
		{
			func_dict = value;
		}
	}

	public PythonDictionary func_dict
	{
		get
		{
			return EnsureDict();
		}
		set
		{
			if (value == null)
			{
				throw PythonOps.TypeError("setting function's dictionary to non-dict");
			}
			_dict = value;
		}
	}

	public object __doc__
	{
		get
		{
			return _doc;
		}
		set
		{
			_doc = value;
		}
	}

	public object func_doc
	{
		get
		{
			return __doc__;
		}
		set
		{
			__doc__ = value;
		}
	}

	public object __module__
	{
		get
		{
			return _module;
		}
		set
		{
			_module = value;
		}
	}

	public FunctionCode __code__
	{
		get
		{
			return func_code;
		}
		set
		{
			func_code = value;
		}
	}

	public FunctionCode func_code
	{
		get
		{
			return _code;
		}
		set
		{
			if (value == null)
			{
				throw PythonOps.TypeError("func_code must be set to a code object");
			}
			_code = value;
			_compat = CalculatedCachedCompat();
		}
	}

	internal SourceSpan Span => func_code.Span;

	internal string[] ArgNames => func_code.ArgNames;

	internal CodeContext Context => _context;

	internal int FunctionCompatibility => _compat;

	internal bool IsGeneratorWithExceptionHandling => (_code.Flags & (FunctionAttributes.Generator | FunctionAttributes.CanSetSysExcInfo)) == (FunctionAttributes.Generator | FunctionAttributes.CanSetSysExcInfo);

	internal int FunctionID => _id;

	internal int ExpandListPosition
	{
		get
		{
			if ((_code.Flags & FunctionAttributes.ArgumentList) != FunctionAttributes.None)
			{
				return _code.co_argcount;
			}
			return -1;
		}
	}

	internal int ExpandDictPosition
	{
		get
		{
			if ((_code.Flags & FunctionAttributes.KeywordDictionary) != FunctionAttributes.None)
			{
				if ((_code.Flags & FunctionAttributes.ArgumentList) != FunctionAttributes.None)
				{
					return _code.co_argcount + 1;
				}
				return _code.co_argcount;
			}
			return -1;
		}
	}

	internal int NormalArgumentCount => _code.co_argcount;

	internal int ExtraArguments
	{
		get
		{
			if ((_code.Flags & FunctionAttributes.ArgumentList) != FunctionAttributes.None)
			{
				if ((_code.Flags & FunctionAttributes.KeywordDictionary) != FunctionAttributes.None)
				{
					return 2;
				}
				return 1;
			}
			if ((_code.Flags & FunctionAttributes.KeywordDictionary) != FunctionAttributes.None)
			{
				return 1;
			}
			return 0;
		}
	}

	internal FunctionAttributes Flags => _code.Flags;

	internal object[] Defaults => _defaults;

	internal override bool GetAlwaysSucceeds => true;

	FastBindResult<T> IFastInvokable.MakeInvokeBinding<T>(CallSite<T> site, PythonInvokeBinder binder, CodeContext state, object[] args)
	{
		if (CanOptimizeCall(binder, args))
		{
			int functionCompatibility = FunctionCompatibility;
			ParameterInfo[] parameters = typeof(T).GetMethod("Invoke").GetParameters();
			Type[] array = ArrayUtils.ConvertAll(parameters, (ParameterInfo inp) => inp.ParameterType);
			if (array[2] != typeof(object))
			{
				return default(FastBindResult<T>);
			}
			array = ArrayUtils.Append(array, typeof(object));
			Type[] array2 = new Type[parameters.Length - 3];
			for (int num = 3; num < parameters.Length; num++)
			{
				array2[num - 3] = parameters[num].ParameterType;
			}
			string text = "";
			if (args.Length != NormalArgumentCount)
			{
				text = "Default" + (NormalArgumentCount - args.Length);
			}
			switch (args.Length)
			{
			case 0:
				if (!string.IsNullOrEmpty(text))
				{
					FunctionCaller functionCaller = new FunctionCaller(functionCompatibility);
					MethodInfo method = typeof(FunctionCaller).GetMethod(text + "Call0");
					return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, typeof(Func<CallSite, CodeContext, object, object>), functionCaller), shouldCache: true);
				}
				return new FastBindResult<T>((T)(object)new Func<CallSite, CodeContext, object, object>(new FunctionCaller(functionCompatibility).Call0), shouldCache: true);
			case 1:
			{
				Type type = typeof(FunctionCaller<>).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call1");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 2:
			{
				Type type = typeof(FunctionCaller<, >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call2");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 3:
			{
				Type type = typeof(FunctionCaller<, , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call3");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 4:
			{
				Type type = typeof(FunctionCaller<, , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call4");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 5:
			{
				Type type = typeof(FunctionCaller<, , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call5");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 6:
			{
				Type type = typeof(FunctionCaller<, , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call6");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 7:
			{
				Type type = typeof(FunctionCaller<, , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call7");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 8:
			{
				Type type = typeof(FunctionCaller<, , , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call8");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 9:
			{
				Type type = typeof(FunctionCaller<, , , , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call9");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 10:
			{
				Type type = typeof(FunctionCaller<, , , , , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call10");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 11:
			{
				Type type = typeof(FunctionCaller<, , , , , , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call11");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 12:
			{
				Type type = typeof(FunctionCaller<, , , , , , , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call12");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			case 13:
			{
				Type type = typeof(FunctionCaller<, , , , , , , , , , , , >).MakeGenericType(array2);
				MethodInfo method = type.GetMethod(text + "Call13");
				FunctionCaller functionCaller = GetFunctionCaller(type, functionCompatibility);
				Type delegateType = typeof(Func<, , , , , , , , , , , , , , , , >).MakeGenericType(array);
				return new FastBindResult<T>((T)(object)ReflectionUtils.CreateDelegate(method, delegateType, functionCaller), shouldCache: true);
			}
			}
		}
		return default(FastBindResult<T>);
	}

	private bool CanOptimizeCall(PythonInvokeBinder binder, object[] args)
	{
		if (args.Length >= NormalArgumentCount - _defaults.Length && args.Length <= NormalArgumentCount && ArgNames.Length < 14 && !binder.Signature.HasDictionaryArgument() && !binder.Signature.HasKeywordArgument() && !binder.Signature.HasListArgument() && (Flags & (FunctionAttributes.ArgumentList | FunctionAttributes.KeywordDictionary)) == 0)
		{
			return !binder.SupportsLightThrow();
		}
		return false;
	}

	private static FunctionCaller GetFunctionCaller(Type callerType, int funcCompat)
	{
		FunctionCaller value;
		lock (_functionCallers)
		{
			FunctionCallerKey key = new FunctionCallerKey(callerType, funcCompat);
			if (!_functionCallers.TryGetValue(key, out value))
			{
				value = (_functionCallers[key] = (FunctionCaller)Activator.CreateInstance(callerType, funcCompat));
			}
		}
		return value;
	}

	public PythonFunction(CodeContext context, FunctionCode code, PythonDictionary globals, string name, PythonTuple defaults, PythonTuple closure)
	{
		throw new NotImplementedException();
	}

	internal PythonFunction(CodeContext context, FunctionCode funcInfo, object modName, object[] defaults, MutableTuple closure)
	{
		_context = context;
		_defaults = defaults ?? ArrayUtils.EmptyObjects;
		_code = funcInfo;
		_doc = funcInfo._initialDoc;
		_name = funcInfo.co_name;
		if (modName != Uninitialized.Instance)
		{
			_module = modName;
		}
		Closure = closure;
		_compat = CalculatedCachedCompat();
	}

	[SpecialName]
	[PropertyMethod]
	public void Deletefunc_globals()
	{
		throw PythonOps.TypeError("readonly attribute");
	}

	public object __call__(CodeContext context, params object[] args)
	{
		return PythonCalls.Call(context, this, args);
	}

	public object __call__(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
		return PythonCalls.CallWithKeywordArgs(context, this, args, dict);
	}

	internal string GetSignatureString()
	{
		StringBuilder stringBuilder = new StringBuilder(__name__);
		stringBuilder.Append('(');
		for (int i = 0; i < _code.ArgNames.Length; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(", ");
			}
			if (i == ExpandDictPosition)
			{
				stringBuilder.Append("**");
			}
			else if (i == ExpandListPosition)
			{
				stringBuilder.Append("*");
			}
			stringBuilder.Append(ArgNames[i]);
			if (i < NormalArgumentCount)
			{
				int num = NormalArgumentCount - Defaults.Length;
				if (i - num >= 0)
				{
					stringBuilder.Append('=');
					stringBuilder.Append(PythonOps.Repr(Context, Defaults[i - num]));
				}
			}
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}

	private int CalculatedCachedCompat()
	{
		return NormalArgumentCount | (Defaults.Length << 14) | ((ExpandDictPosition != -1) ? 1073741824 : 0) | ((ExpandListPosition != -1) ? 536870912 : 0);
	}

	internal Exception BadArgumentError(int count)
	{
		return BinderOps.TypeErrorForIncorrectArgumentCount(__name__, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, keywordArgumentsProvided: false);
	}

	internal Exception BadKeywordArgumentError(int count)
	{
		return BinderOps.TypeErrorForIncorrectArgumentCount(__name__, NormalArgumentCount, Defaults.Length, count, ExpandListPosition != -1, keywordArgumentsProvided: true);
	}

	IList<string> IMembersList.GetMemberNames()
	{
		return PythonOps.GetStringMemberList(this);
	}

	IList<object> IPythonMembersList.GetMemberNames(CodeContext context)
	{
		List list = ((_dict != null) ? PythonOps.MakeListFromSequence(_dict) : PythonOps.MakeList());
		list.AddNoLock("__module__");
		list.extend(TypeCache.Function.GetMemberNames(context, this));
		return list;
	}

	WeakRefTracker IWeakReferenceable.GetWeakRef()
	{
		if (_dict != null && _dict.TryGetValue("__weakref__", out var value))
		{
			return value as WeakRefTracker;
		}
		return null;
	}

	bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
	{
		EnsureDict();
		_dict["__weakref__"] = value;
		return true;
	}

	void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
	{
		((IWeakReferenceable)this).SetWeakRef(value);
	}

	internal PythonDictionary EnsureDict()
	{
		if (_dict == null)
		{
			Interlocked.CompareExchange(ref _dict, PythonDictionary.MakeSymbolDictionary(), null);
		}
		return _dict;
	}

	internal static int AddRecursionDepth(int change)
	{
		uint managedThreadId = (uint)Thread.CurrentThread.ManagedThreadId;
		if (managedThreadId < _depth_fast.Length)
		{
			return _depth_fast[managedThreadId] += change;
		}
		return DepthSlow += change;
	}

	internal void EnsureID()
	{
		if (_id == 0)
		{
			Interlocked.CompareExchange(ref _id, Interlocked.Increment(ref _CurrentId), 0);
		}
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = new Method(this, instance, owner);
		return true;
	}

	public string __repr__(CodeContext context)
	{
		return $"<function {func_name} at {PythonOps.HexId(this)}>";
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new MetaPythonFunction(parameter, BindingRestrictions.Empty, this);
	}
}
