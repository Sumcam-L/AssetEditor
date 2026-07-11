using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Modules;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

[DontMapGetMemberNamesToDir]
[PythonType("builtin_function_or_method")]
public class BuiltinFunction : PythonTypeSlot, ICodeFormattable, IDynamicMetaObjectProvider, IDelegateConvertible, IFastInvokable
{
	internal class BindingResult
	{
		public readonly BindingTarget Target;

		public readonly DynamicMetaObject MetaObject;

		public BindingResult(BindingTarget target, DynamicMetaObject meta)
		{
			Target = target;
			MetaObject = meta;
		}
	}

	internal class TypeList
	{
		private Type[] _types;

		public TypeList(Type[] types)
		{
			_types = types;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TypeList typeList) || _types.Length != typeList._types.Length)
			{
				return false;
			}
			for (int i = 0; i < _types.Length; i++)
			{
				if (_types[i] != typeList._types[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = 6551;
			Type[] types = _types;
			foreach (Type type in types)
			{
				num = (num << 5) ^ type.GetHashCode();
			}
			return num;
		}
	}

	internal sealed class BuiltinFunctionData
	{
		public string Name;

		public MethodBase[] Targets;

		public readonly Type DeclaringType;

		public FunctionType Type;

		public Dictionary<TypeList, BuiltinFunction> BoundGenerics;

		public Dictionary<TypeList, BuiltinFunction> OverloadDictionary;

		public BuiltinFunctionData(string name, MethodBase[] targets, Type declType, FunctionType functionType)
		{
			Name = name;
			Targets = targets;
			DeclaringType = declType;
			Type = functionType;
		}

		internal void AddMethod(MethodBase info)
		{
			MethodBase[] array = new MethodBase[Targets.Length + 1];
			Targets.CopyTo(array, 0);
			array[Targets.Length] = info;
			Targets = array;
		}
	}

	internal readonly BuiltinFunctionData _data;

	internal readonly object _instance;

	private static readonly object _noInstance = new object();

	internal bool IsUnbound => _instance == _noInstance;

	internal string Name
	{
		get
		{
			return _data.Name;
		}
		set
		{
			_data.Name = value;
		}
	}

	public Type DeclaringType
	{
		[PythonHidden]
		get
		{
			return _data.DeclaringType;
		}
	}

	public IList<MethodBase> Targets
	{
		[PythonHidden]
		get
		{
			return _data.Targets;
		}
	}

	internal override bool IsAlwaysVisible => (_data.Type & FunctionType.AlwaysVisible) != 0;

	internal bool IsReversedOperator => (FunctionType & FunctionType.ReversedOperator) != 0;

	internal bool IsBinaryOperator => (FunctionType & FunctionType.BinaryOperator) != 0;

	internal FunctionType FunctionType
	{
		get
		{
			return _data.Type;
		}
		set
		{
			_data.Type = value;
		}
	}

	internal override bool GetAlwaysSucceeds => true;

	public virtual BuiltinFunctionOverloadMapper Overloads
	{
		[PythonHidden]
		get
		{
			return new BuiltinFunctionOverloadMapper(this, IsUnbound ? null : _instance);
		}
	}

	internal Dictionary<TypeList, BuiltinFunction> OverloadDictionary
	{
		get
		{
			if (_data.OverloadDictionary == null)
			{
				Interlocked.CompareExchange(ref _data.OverloadDictionary, new Dictionary<TypeList, BuiltinFunction>(), null);
			}
			return _data.OverloadDictionary;
		}
	}

	public string __name__ => Name;

	public virtual string __doc__
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			IList<MethodBase> targets = Targets;
			for (int i = 0; i < targets.Count; i++)
			{
				if (targets[i] != null)
				{
					if (IsBuiltinModuleMethod)
					{
						stringBuilder.Append(DocBuilder.DocOneInfo(targets[i], Name, includeSelf: false));
					}
					else
					{
						stringBuilder.Append(DocBuilder.DocOneInfo(targets[i], Name));
					}
				}
			}
			return stringBuilder.ToString();
		}
	}

	public object __self__
	{
		get
		{
			if (IsUnbound || IsBuiltinModuleMethod)
			{
				return null;
			}
			return _instance;
		}
	}

	internal object BindingSelf
	{
		get
		{
			if (IsUnbound)
			{
				return null;
			}
			return _instance;
		}
	}

	private bool IsBuiltinModuleMethod => (FunctionType & FunctionType.ModuleMethod) != 0;

	internal virtual bool IsOnlyGeneric => false;

	private BinderType BinderType
	{
		get
		{
			if (!IsBinaryOperator)
			{
				return BinderType.Normal;
			}
			return BinderType.BinaryOperator;
		}
	}

	internal static BuiltinFunction MakeFunction(string name, MethodBase[] infos, Type declaringType)
	{
		return new BuiltinFunction(name, infos, declaringType, FunctionType.Function | FunctionType.AlwaysVisible);
	}

	internal static BuiltinFunction MakeMethod(string name, MethodBase[] infos, Type declaringType, FunctionType ft)
	{
		foreach (MethodBase methodBase in infos)
		{
			if (methodBase.ContainsGenericParameters)
			{
				return new GenericBuiltinFunction(name, infos, declaringType, ft);
			}
		}
		return new BuiltinFunction(name, infos, declaringType, ft);
	}

	internal virtual BuiltinFunction BindToInstance(object instance)
	{
		return new BuiltinFunction(instance, _data);
	}

	internal BuiltinFunction(string name, MethodBase[] originalTargets, Type declaringType, FunctionType functionType)
	{
		_data = new BuiltinFunctionData(name, originalTargets, declaringType, functionType);
		_instance = _noInstance;
	}

	internal BuiltinFunction(object instance, BuiltinFunctionData data)
	{
		_instance = instance;
		_data = data;
	}

	internal void AddMethod(MethodInfo mi)
	{
		_data.AddMethod(mi);
	}

	internal bool TestData(object data)
	{
		return _data == data;
	}

	internal object Call(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage, object instance, object[] args)
	{
		storage = GetInitializedStorage(context, storage);
		if (!GetDescriptor().TryGetValue(context, instance, DynamicHelpers.GetPythonTypeFromType(DeclaringType), out var value))
		{
			value = this;
		}
		return storage.Data.Target(storage.Data, context, value, args);
	}

	internal object Call0(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object>>> storage, object instance)
	{
		storage = GetInitializedStorage(context, storage);
		if (!GetDescriptor().TryGetValue(context, instance, DynamicHelpers.GetPythonTypeFromType(DeclaringType), out var value))
		{
			value = this;
		}
		return storage.Data.Target(storage.Data, context, value);
	}

	private static SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> GetInitializedStorage(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], object>>> storage)
	{
		if (storage == null)
		{
			storage = PythonContext.GetContext(context).GetGenericCallSiteStorage();
		}
		if (storage.Data == null)
		{
			storage.Data = PythonContext.GetContext(context).MakeSplatSite();
		}
		return storage;
	}

	private static SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object>>> GetInitializedStorage(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object>>> storage)
	{
		if (storage.Data == null)
		{
			storage.Data = CallSite<Func<CallSite, CodeContext, object, object>>.Create(PythonContext.GetContext(context).InvokeNone);
		}
		return storage;
	}

	internal object Call(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>> storage, object instance, object[] args, IDictionary<object, object> keywordArgs)
	{
		if (storage == null)
		{
			storage = PythonContext.GetContext(context).GetGenericKeywordCallSiteStorage();
		}
		if (storage.Data == null)
		{
			storage.Data = PythonContext.GetContext(context).MakeKeywordSplatSite();
		}
		if (instance != null)
		{
			return storage.Data.Target(storage.Data, context, this, ArrayUtils.Insert(instance, args), keywordArgs);
		}
		return storage.Data.Target(storage.Data, context, this, args, keywordArgs);
	}

	internal BuiltinFunction MakeGenericMethod(Type[] types)
	{
		TypeList key = new TypeList(types);
		BuiltinFunction value;
		if (_data.BoundGenerics != null)
		{
			lock (_data.BoundGenerics)
			{
				if (_data.BoundGenerics.TryGetValue(key, out value))
				{
					return value;
				}
			}
		}
		List<MethodBase> list = new List<MethodBase>(Targets.Count);
		foreach (MethodBase target in Targets)
		{
			MethodInfo methodInfo = target as MethodInfo;
			if (!(methodInfo == null) && methodInfo.ContainsGenericParameters && methodInfo.GetGenericArguments().Length == types.Length)
			{
				list.Add(methodInfo.MakeGenericMethod(types));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		value = new BuiltinFunction(Name, list.ToArray(), DeclaringType, FunctionType);
		EnsureBoundGenericDict();
		lock (_data.BoundGenerics)
		{
			_data.BoundGenerics[key] = value;
			return value;
		}
	}

	internal PythonTypeSlot GetDescriptor()
	{
		if ((FunctionType & FunctionType.Method) != FunctionType.None)
		{
			return new BuiltinMethodDescriptor(this);
		}
		return this;
	}

	internal Expression MakeBoundFunctionTest(Expression functionTarget)
	{
		return Expression.Call(typeof(PythonOps).GetMethod("TestBoundBuiltinFunction"), functionTarget, Utils.Constant(_data, typeof(object)));
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = this;
		return true;
	}

	internal override void MakeGetExpression(PythonBinder binder, Expression codeContext, DynamicMetaObject instance, DynamicMetaObject owner, ConditionalBuilder builder)
	{
		builder.FinishCondition(Expression.Constant(this));
	}

	public string __repr__(CodeContext context)
	{
		if (IsUnbound || IsBuiltinModuleMethod)
		{
			return $"<built-in function {Name}>";
		}
		return $"<built-in method {__name__} of {PythonOps.GetPythonTypeName(__self__)} object at {PythonOps.HexId(__self__)}>";
	}

	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return new MetaBuiltinFunction(parameter, BindingRestrictions.Empty, this);
	}

	internal DynamicMetaObject MakeBuiltinFunctionCall(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject function, DynamicMetaObject[] args, bool hasSelf, BindingRestrictions functionRestriction, Func<DynamicMetaObject[], BindingResult> bind)
	{
		DynamicMetaObject dynamicMetaObject = null;
		DynamicMetaObject dynamicMetaObject2 = TranslateArguments(call, codeContext, new DynamicMetaObject(function.Expression, functionRestriction, function.Value), args, hasSelf, Name);
		if (dynamicMetaObject2 != null)
		{
			return dynamicMetaObject2;
		}
		if (IsReversedOperator)
		{
			ArrayUtils.SwapLastTwo(args);
		}
		BindingResult bindingResult = bind(args);
		BindingTarget target = bindingResult.Target;
		dynamicMetaObject = bindingResult.MetaObject;
		if (target.Overload != null && target.Overload.IsProtected)
		{
			dynamicMetaObject = new DynamicMetaObject(BindingHelpers.TypeErrorForProtectedMember(target.Overload.DeclaringType, target.Overload.Name), dynamicMetaObject.Restrictions);
		}
		else if (IsBinaryOperator && args.Length == 2 && IsThrowException(dynamicMetaObject.Expression))
		{
			dynamicMetaObject = new DynamicMetaObject(Expression.Property(null, typeof(PythonOps), "NotImplemented"), dynamicMetaObject.Restrictions);
		}
		else if (target.Overload != null && call is IPythonSite { Context: var context } && context.Options is PythonOptions { EnableProfiler: not false })
		{
			Profiler profiler = Profiler.GetProfiler(context);
			dynamicMetaObject = new DynamicMetaObject(profiler.AddProfiling(dynamicMetaObject.Expression, target.Overload.ReflectionInfo), dynamicMetaObject.Restrictions);
		}
		if (target.Overload != null && BindingWarnings.ShouldWarn(PythonContext.GetPythonContext(call), target.Overload, out var info))
		{
			dynamicMetaObject = info.AddWarning(codeContext, dynamicMetaObject);
		}
		dynamicMetaObject = new DynamicMetaObject(dynamicMetaObject.Expression, functionRestriction.Merge(dynamicMetaObject.Restrictions));
		if (dynamicMetaObject.Expression.Type.IsValueType())
		{
			dynamicMetaObject = BindingHelpers.AddPythonBoxing(dynamicMetaObject);
		}
		else if (dynamicMetaObject.Expression.Type == typeof(void))
		{
			dynamicMetaObject = new DynamicMetaObject(Expression.Block(dynamicMetaObject.Expression, Expression.Constant(null)), dynamicMetaObject.Restrictions);
		}
		return dynamicMetaObject;
	}

	internal static DynamicMetaObject TranslateArguments(DynamicMetaObjectBinder call, Expression codeContext, DynamicMetaObject function, DynamicMetaObject[] args, bool hasSelf, string name)
	{
		if (hasSelf)
		{
			args = ArrayUtils.RemoveFirst(args);
		}
		CallSignature callSignature = BindingHelpers.GetCallSignature(call);
		if (callSignature.HasDictionaryArgument())
		{
			int num = callSignature.IndexOf(ArgumentType.Dictionary);
			DynamicMetaObject dynamicMetaObject = args[num];
			if (!(dynamicMetaObject.Value is IDictionary) && dynamicMetaObject.Value != null)
			{
				DynamicMetaObject[] array = ArrayUtils.Insert(function, args);
				array[num + 1] = new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("UserMappingToPythonDictionary"), codeContext, args[num].Expression, Utils.Constant(name)), BindingRestrictionsHelpers.GetRuntimeTypeRestriction(dynamicMetaObject.Expression, dynamicMetaObject.GetLimitType()), PythonOps.UserMappingToPythonDictionary(PythonContext.GetPythonContext(call).SharedContext, dynamicMetaObject.Value, name));
				if (call is IPythonSite)
				{
					array = ArrayUtils.Insert(new DynamicMetaObject(codeContext, BindingRestrictions.Empty), array);
				}
				return new DynamicMetaObject(Expression.Dynamic(call, typeof(object), DynamicUtils.GetExpressions(array)), BindingRestrictions.Combine(array).Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(dynamicMetaObject.Expression, dynamicMetaObject.GetLimitType())));
			}
		}
		if (callSignature.HasListArgument())
		{
			int num2 = callSignature.IndexOf(ArgumentType.List);
			DynamicMetaObject dynamicMetaObject2 = args[num2];
			if (!(dynamicMetaObject2.Value is IList<object>) && dynamicMetaObject2.Value is IEnumerable)
			{
				DynamicMetaObject[] array2 = ArrayUtils.Insert(function, args);
				array2[num2 + 1] = new DynamicMetaObject(Expression.Call(typeof(PythonOps).GetMethod("MakeTupleFromSequence"), Expression.Convert(args[num2].Expression, typeof(object))), BindingRestrictions.Empty);
				if (call is IPythonSite)
				{
					array2 = ArrayUtils.Insert(new DynamicMetaObject(codeContext, BindingRestrictions.Empty), array2);
				}
				return new DynamicMetaObject(Expression.Dynamic(call, typeof(object), DynamicUtils.GetExpressions(array2)), function.Restrictions.Merge(BindingRestrictions.Combine(args).Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(dynamicMetaObject2.Expression, dynamicMetaObject2.GetLimitType()))));
			}
		}
		return null;
	}

	private static bool IsThrowException(Expression expr)
	{
		if (expr.NodeType == ExpressionType.Throw)
		{
			return true;
		}
		if (expr.NodeType == ExpressionType.Convert)
		{
			return IsThrowException(((UnaryExpression)expr).Operand);
		}
		if (expr.NodeType == ExpressionType.Block)
		{
			foreach (Expression expression in ((BlockExpression)expr).Expressions)
			{
				if (IsThrowException(expression))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static object __new__(object cls, object newFunction, object inst)
	{
		return new Method(newFunction, inst, null);
	}

	public int __cmp__(CodeContext context, [NotNull] BuiltinFunction other)
	{
		if (other == this)
		{
			return 0;
		}
		if (!IsUnbound && !other.IsUnbound)
		{
			int num = PythonOps.Compare(__self__, other.__self__);
			if (num != 0)
			{
				return num;
			}
			if (_data == other._data)
			{
				return 0;
			}
		}
		int num2 = string.CompareOrdinal(__name__, other.__name__);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = string.CompareOrdinal(Get__module__(context), other.Get__module__(context));
		if (num2 != 0)
		{
			return num2;
		}
		long num3 = IdDispenser.GetId(this) - IdDispenser.GetId(other);
		if (num3 <= 0)
		{
			return -1;
		}
		return 1;
	}

	[Python3Warning("builtin_function_or_method order comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator >(BuiltinFunction self, BuiltinFunction other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("builtin_function_or_method order comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator <(BuiltinFunction self, BuiltinFunction other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("builtin_function_or_method order comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator >=(BuiltinFunction self, BuiltinFunction other)
	{
		return PythonOps.NotImplemented;
	}

	[Python3Warning("builtin_function_or_method order comparisons not supported in 3.x")]
	[return: MaybeNotImplemented]
	public static NotImplementedType operator <=(BuiltinFunction self, BuiltinFunction other)
	{
		return PythonOps.NotImplemented;
	}

	public int __hash__(CodeContext context)
	{
		return PythonOps.Hash(context, _instance) ^ PythonOps.Hash(context, _data);
	}

	[SpecialName]
	[PropertyMethod]
	public string Get__module__(CodeContext context)
	{
		if (Targets.Count > 0)
		{
			PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(DeclaringType);
			string moduleName = PythonTypeOps.GetModuleName(context, pythonTypeFromType.UnderlyingSystemType);
			if (moduleName != "__builtin__" || DeclaringType == typeof(Builtin))
			{
				return moduleName;
			}
		}
		return null;
	}

	[SpecialName]
	[PropertyMethod]
	public void Set__module__(string value)
	{
	}

	public object __call__(CodeContext context, SiteLocalStorage<CallSite<Func<CallSite, CodeContext, object, object[], IDictionary<object, object>, object>>> storage, [ParamDictionary] IDictionary<object, object> dictArgs, params object[] args)
	{
		return Call(context, storage, null, args, dictArgs);
	}

	private void EnsureBoundGenericDict()
	{
		if (_data.BoundGenerics == null)
		{
			Interlocked.CompareExchange(ref _data.BoundGenerics, new Dictionary<TypeList, BuiltinFunction>(1), null);
		}
	}

	Delegate IDelegateConvertible.ConvertToDelegate(Type type)
	{
		ParameterInfo[] parameters = type.GetMethod("Invoke").GetParameters();
		if (Targets.Count == 1)
		{
			MethodInfo methodInfo = Targets[0] as MethodInfo;
			if (methodInfo != null)
			{
				ParameterInfo[] parameters2 = methodInfo.GetParameters();
				if (parameters2.Length == parameters.Length)
				{
					bool flag = true;
					for (int i = 0; i < parameters2.Length; i++)
					{
						if (parameters[i].ParameterType != parameters2[i].ParameterType)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						if (IsUnbound)
						{
							return Delegate.CreateDelegate(type, methodInfo);
						}
						return Delegate.CreateDelegate(type, _instance, methodInfo);
					}
				}
			}
		}
		return null;
	}

	FastBindResult<T> IFastInvokable.MakeInvokeBinding<T>(CallSite<T> site, PythonInvokeBinder binder, CodeContext state, object[] args)
	{
		return new FastBindResult<T>(binder.LightBind<T>(ArrayUtils.Insert(state, this, args), 100), shouldCache: true);
	}
}
