using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

internal class UncollectableCompilationMode : CompilationMode
{
	private class CodeContextExpression : System.Linq.Expressions.Expression, IInstructionProvider
	{
		private readonly System.Linq.Expressions.Expression _expression;

		public CodeContext Context;

		public override bool CanReduce => true;

		public override Type Type => typeof(CodeContext);

		public override ExpressionType NodeType => ExpressionType.Extension;

		public CodeContextExpression(System.Linq.Expressions.Expression expression)
		{
			_expression = expression;
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return _expression;
		}

		public void AddInstructions(LightCompiler compiler)
		{
			compiler.Instructions.EmitLoad(Context);
		}
	}

	private sealed class DelegateCache
	{
		public Type DelegateType;

		public Type SiteType;

		public Func<DynamicMetaObjectBinder, SiteInfo> NextSite;

		public FieldInfo TargetField;

		public MethodInfo InvokeMethod;

		public Dictionary<Type, DelegateCache> TypeChain;

		public void MakeDelegateType(Type retType, params System.Linq.Expressions.Expression[] args)
		{
			DelegateType = GetDelegateType(retType, args);
			SiteType = typeof(CallSite<>).MakeGenericType(DelegateType);
			NextSite = (Func<DynamicMetaObjectBinder, SiteInfo>)ReflectionUtils.CreateDelegate(typeof(UncollectableCompilationMode).GetMethod("NextSite").MakeGenericMethod(DelegateType), typeof(Func<DynamicMetaObjectBinder, SiteInfo>));
			TargetField = SiteType.GetField("Target");
			InvokeMethod = DelegateType.GetMethod("Invoke");
		}

		public static DelegateCache FirstCacheNode(Type argType)
		{
			if (!_delegateCache.TryGetValue(argType, out var value))
			{
				value = new DelegateCache();
				_delegateCache[argType] = value;
			}
			return value;
		}

		public DelegateCache NextCacheNode(Type argType)
		{
			if (TypeChain == null)
			{
				TypeChain = new Dictionary<Type, DelegateCache>();
			}
			if (!TypeChain.TryGetValue(argType, out var value))
			{
				value = new DelegateCache();
				TypeChain[argType] = value;
			}
			return value;
		}
	}

	internal abstract class ReducibleExpression : System.Linq.Expressions.Expression
	{
		private readonly int _offset;

		private int _start = -1;

		private FieldInfo _fieldInfo;

		public abstract string Name { get; }

		public abstract int FieldCount { get; }

		public abstract override Type Type { get; }

		public FieldInfo FieldInfo => _fieldInfo;

		public int Start
		{
			get
			{
				return _start;
			}
			set
			{
				_start = value;
				int num = _offset + _start;
				Type storageType = GetStorageType(num);
				if (storageType != typeof(StorageData))
				{
					_fieldInfo = storageType.GetField(Name + $"{num % 50:000}");
				}
				else
				{
					_fieldInfo = typeof(StorageData).GetField(Name + "s");
				}
			}
		}

		public override ExpressionType NodeType => ExpressionType.Extension;

		public override bool CanReduce => true;

		public ReducibleExpression(int offset)
		{
			_offset = offset;
		}

		protected abstract Type GetStorageType(int index);

		public override System.Linq.Expressions.Expression Reduce()
		{
			int num = _offset + _start;
			int num2 = num - FieldCount;
			if (num2 < 0)
			{
				return System.Linq.Expressions.Expression.Field(null, _fieldInfo);
			}
			return System.Linq.Expressions.Expression.ArrayIndex(System.Linq.Expressions.Expression.Field(null, _fieldInfo), System.Linq.Expressions.Expression.Constant(num2, typeof(int)));
		}

		protected override System.Linq.Expressions.Expression Accept(ExpressionVisitor visitor)
		{
			return this;
		}

		protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
		{
			return this;
		}
	}

	internal sealed class ConstantExpression : ReducibleExpression
	{
		private object _value;

		public override string Name => "Constant";

		public override int FieldCount => 250;

		public override Type Type
		{
			get
			{
				Type type = _value.GetType();
				if (!type.IsValueType())
				{
					return type;
				}
				return typeof(object);
			}
		}

		public object Value => _value;

		public ConstantExpression(int offset, object value)
			: base(offset)
		{
			_value = value;
		}

		protected override Type GetStorageType(int index)
		{
			return StorageData.ConstantStorageType(index);
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			if (_value.GetType().IsValueType())
			{
				return base.Reduce();
			}
			return System.Linq.Expressions.Expression.Convert(base.Reduce(), _value.GetType());
		}
	}

	internal sealed class GlobalExpression : ReducibleExpression
	{
		public override string Name => "Global";

		public override int FieldCount => 1000;

		public override Type Type => typeof(PythonGlobal);

		public GlobalExpression(int offset)
			: base(offset)
		{
		}

		protected override Type GetStorageType(int index)
		{
			return StorageData.GlobalStorageType(index);
		}
	}

	private static readonly Dictionary<object, ConstantInfo> _allConstants = new Dictionary<object, ConstantInfo>();

	private static readonly Dictionary<Type, DelegateCache> _delegateCache = new Dictionary<Type, DelegateCache>();

	public override Type DelegateType => typeof(Expression<Func<FunctionCode, object>>);

	public override System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0)
	{
		DelegateCache delegateCache;
		lock (_delegateCache)
		{
			delegateCache = DelegateCache.FirstCacheNode(arg0.Type).NextCacheNode(retType);
			if (delegateCache.DelegateType == null)
			{
				delegateCache.MakeDelegateType(retType, arg0);
			}
		}
		SiteInfo siteInfo = delegateCache.NextSite(binder);
		return MakeDynamicExpression(binder, siteInfo.Expression, delegateCache.TargetField, delegateCache.InvokeMethod, arg0);
	}

	private static System.Linq.Expressions.Expression MakeDynamicExpression(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo targetField, MethodInfo invokeMethod, System.Linq.Expressions.Expression arg0)
	{
		return new ReducableDynamicExpression(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Field(expr, targetField), invokeMethod, expr, arg0), binder, new System.Linq.Expressions.Expression[1] { arg0 });
	}

	public override System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1)
	{
		DelegateCache delegateCache;
		lock (_delegateCache)
		{
			delegateCache = DelegateCache.FirstCacheNode(arg0.Type).NextCacheNode(arg1.Type).NextCacheNode(retType);
			if (delegateCache.DelegateType == null)
			{
				delegateCache.MakeDelegateType(retType, arg0, arg1);
			}
		}
		SiteInfo siteInfo = delegateCache.NextSite(binder);
		return MakeDynamicExpression(binder, siteInfo.Expression, delegateCache.TargetField, delegateCache.InvokeMethod, arg0, arg1);
	}

	private static System.Linq.Expressions.Expression MakeDynamicExpression(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo targetField, MethodInfo invokeMethod, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1)
	{
		return new ReducableDynamicExpression(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Field(expr, targetField), invokeMethod, expr, arg0, arg1), binder, new System.Linq.Expressions.Expression[2] { arg0, arg1 });
	}

	public override System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2)
	{
		DelegateCache delegateCache;
		lock (_delegateCache)
		{
			delegateCache = DelegateCache.FirstCacheNode(arg0.Type).NextCacheNode(arg1.Type).NextCacheNode(arg2.Type)
				.NextCacheNode(retType);
			if (delegateCache.DelegateType == null)
			{
				delegateCache.MakeDelegateType(retType, arg0, arg1, arg2);
			}
		}
		SiteInfo siteInfo = delegateCache.NextSite(binder);
		return MakeDynamicExpression(binder, siteInfo.Expression, delegateCache.TargetField, delegateCache.InvokeMethod, arg0, arg1, arg2);
	}

	private static System.Linq.Expressions.Expression MakeDynamicExpression(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo targetField, MethodInfo invokeMethod, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2)
	{
		return new ReducableDynamicExpression(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Field(expr, targetField), invokeMethod, expr, arg0, arg1, arg2), binder, new System.Linq.Expressions.Expression[3] { arg0, arg1, arg2 });
	}

	public override System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2, System.Linq.Expressions.Expression arg3)
	{
		DelegateCache delegateCache;
		lock (_delegateCache)
		{
			delegateCache = DelegateCache.FirstCacheNode(arg0.Type).NextCacheNode(arg1.Type).NextCacheNode(arg2.Type)
				.NextCacheNode(arg3.Type)
				.NextCacheNode(retType);
			if (delegateCache.DelegateType == null)
			{
				delegateCache.MakeDelegateType(retType, arg0, arg1, arg2, arg3);
			}
		}
		SiteInfo siteInfo = delegateCache.NextSite(binder);
		return MakeDynamicExpression(binder, siteInfo.Expression, delegateCache.TargetField, delegateCache.InvokeMethod, arg0, arg1, arg2, arg3);
	}

	private static System.Linq.Expressions.Expression MakeDynamicExpression(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo targetField, MethodInfo invokeMethod, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2, System.Linq.Expressions.Expression arg3)
	{
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(5);
		readOnlyCollectionBuilder.Add(expr);
		readOnlyCollectionBuilder.Add(arg0);
		readOnlyCollectionBuilder.Add(arg1);
		readOnlyCollectionBuilder.Add(arg2);
		readOnlyCollectionBuilder.Add(arg3);
		return new ReducableDynamicExpression(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Field(expr, targetField), invokeMethod, readOnlyCollectionBuilder.ToReadOnlyCollection()), binder, new System.Linq.Expressions.Expression[4] { arg0, arg1, arg2, arg3 });
	}

	public override System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression[] args)
	{
		Type delegateType = GetDelegateType(retType, args);
		Type type = typeof(CallSite<>).MakeGenericType(delegateType);
		SiteInfo siteInfo = NextSite(binder, delegateType);
		return MakeDynamicExpression(binder, siteInfo.Expression, type.GetField("Target"), delegateType.GetMethod("Invoke"), args);
	}

	private static System.Linq.Expressions.Expression MakeDynamicExpression(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo targetField, MethodInfo invokeMethod, IList<System.Linq.Expressions.Expression> args)
	{
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(args.Count + 1);
		readOnlyCollectionBuilder.Add(expr);
		foreach (System.Linq.Expressions.Expression arg in args)
		{
			readOnlyCollectionBuilder.Add(arg);
		}
		return new ReducableDynamicExpression(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Field(expr, targetField), invokeMethod, readOnlyCollectionBuilder.ToReadOnlyCollection()), binder, args);
	}

	private static Type GetDelegateType(Type retType, IList<System.Linq.Expressions.Expression> args)
	{
		if (retType != typeof(void))
		{
			Type[] array = new Type[args.Count + 2];
			array[0] = typeof(CallSite);
			for (int i = 0; i < args.Count; i++)
			{
				array[i + 1] = args[i].Type;
			}
			array[array.Length - 1] = retType;
			return GetFuncType(array) ?? PythonOps.MakeNewCustomDelegate(array);
		}
		Type[] array2 = new Type[args.Count + 1];
		array2[0] = typeof(CallSite);
		for (int j = 0; j < args.Count; j++)
		{
			array2[j + 1] = args[j].Type;
		}
		return GetActionType(array2) ?? PythonOps.MakeNewCustomDelegate(ArrayUtils.Append(array2, typeof(void)));
	}

	private static Type GetFuncType(Type[] types)
	{
		return types.Length switch
		{
			1 => typeof(Func<>).MakeGenericType(types), 
			2 => typeof(Func<, >).MakeGenericType(types), 
			3 => typeof(Func<, , >).MakeGenericType(types), 
			4 => typeof(Func<, , , >).MakeGenericType(types), 
			5 => typeof(Func<, , , , >).MakeGenericType(types), 
			6 => typeof(Func<, , , , , >).MakeGenericType(types), 
			7 => typeof(Func<, , , , , , >).MakeGenericType(types), 
			8 => typeof(Func<, , , , , , , >).MakeGenericType(types), 
			9 => typeof(Func<, , , , , , , , >).MakeGenericType(types), 
			10 => typeof(Func<, , , , , , , , , >).MakeGenericType(types), 
			11 => typeof(Func<, , , , , , , , , , >).MakeGenericType(types), 
			12 => typeof(Func<, , , , , , , , , , , >).MakeGenericType(types), 
			13 => typeof(Func<, , , , , , , , , , , , >).MakeGenericType(types), 
			14 => typeof(Func<, , , , , , , , , , , , , >).MakeGenericType(types), 
			15 => typeof(Func<, , , , , , , , , , , , , , >).MakeGenericType(types), 
			16 => typeof(Func<, , , , , , , , , , , , , , , >).MakeGenericType(types), 
			17 => typeof(Func<, , , , , , , , , , , , , , , , >).MakeGenericType(types), 
			_ => null, 
		};
	}

	private static Type GetActionType(Type[] types)
	{
		return types.Length switch
		{
			0 => typeof(Action), 
			1 => typeof(Action<>).MakeGenericType(types), 
			2 => typeof(Action<, >).MakeGenericType(types), 
			3 => typeof(Action<, , >).MakeGenericType(types), 
			4 => typeof(Action<, , , >).MakeGenericType(types), 
			5 => typeof(Action<, , , , >).MakeGenericType(types), 
			6 => typeof(Action<, , , , , >).MakeGenericType(types), 
			7 => typeof(Action<, , , , , , >).MakeGenericType(types), 
			8 => typeof(Action<, , , , , , , >).MakeGenericType(types), 
			9 => typeof(Action<, , , , , , , , >).MakeGenericType(types), 
			10 => typeof(Action<, , , , , , , , , >).MakeGenericType(types), 
			11 => typeof(Action<, , , , , , , , , , >).MakeGenericType(types), 
			12 => typeof(Action<, , , , , , , , , , , >).MakeGenericType(types), 
			13 => typeof(Action<, , , , , , , , , , , , >).MakeGenericType(types), 
			14 => typeof(Action<, , , , , , , , , , , , , >).MakeGenericType(types), 
			15 => typeof(Action<, , , , , , , , , , , , , , >).MakeGenericType(types), 
			16 => typeof(Action<, , , , , , , , , , , , , , , >).MakeGenericType(types), 
			_ => null, 
		};
	}

	public override LightLambdaExpression ReduceAst(PythonAst instance, string name)
	{
		return Utils.LightLambda<Func<FunctionCode, object>>(typeof(object), Utils.Convert(instance.ReduceWorker(), typeof(object)), name, new ParameterExpression[1] { PythonAst._functionCode });
	}

	public override System.Linq.Expressions.Expression GetConstant(object value)
	{
		if (CompilerHelpers.CanEmitConstant(value, CompilerHelpers.GetType(value)) && !CompilerHelpers.GetType(value).IsValueType())
		{
			return Utils.Constant(value);
		}
		ConstantInfo value2;
		lock (_allConstants)
		{
			if (!_allConstants.TryGetValue(value, out value2))
			{
				value2 = (_allConstants[value] = NextConstant(_allConstants.Count, value));
				PublishConstant(value, value2);
			}
		}
		return value2.Expression;
	}

	public override Type GetConstantType(object value)
	{
		if (value == null || value.GetType().IsValueType())
		{
			return typeof(object);
		}
		return value.GetType();
	}

	public override System.Linq.Expressions.Expression GetGlobal(System.Linq.Expressions.Expression globalContext, int arrayIndex, PythonVariable variable, PythonGlobal global)
	{
		lock (StorageData.Globals)
		{
			ConstantInfo constantInfo = NextGlobal(0);
			StorageData.GlobalStorageType(StorageData.GlobalCount + 1);
			PublishWorker(StorageData.GlobalCount, 20, constantInfo, global, StorageData.Globals);
			StorageData.GlobalCount++;
			return new PythonGlobalVariableExpression(constantInfo.Expression, variable, global);
		}
	}

	public override ConstantInfo GetContext()
	{
		lock (StorageData.Contexts)
		{
			int num = StorageData.ContextCount++;
			int num2 = num - 50;
			Type type = StorageData.ContextStorageType(num);
			FieldInfo field;
			System.Linq.Expressions.Expression expression;
			if (num2 < 0)
			{
				field = type.GetField($"Context{num % 50:000}");
				expression = System.Linq.Expressions.Expression.Field(null, field);
			}
			else
			{
				field = typeof(StorageData).GetField("Contexts");
				expression = System.Linq.Expressions.Expression.ArrayIndex(System.Linq.Expressions.Expression.Field(null, field), System.Linq.Expressions.Expression.Constant(num2, typeof(int)));
			}
			return new ConstantInfo(new CodeContextExpression(expression), field, num);
		}
	}

	private static ConstantInfo NextConstant(int offset, object value)
	{
		return new ConstantInfo(new ConstantExpression(offset, value), null, offset);
	}

	private static ConstantInfo NextGlobal(int offset)
	{
		return new ConstantInfo(new GlobalExpression(offset), null, offset);
	}

	public static SiteInfo NextSite<T>(DynamicMetaObjectBinder binder) where T : class
	{
		lock (StorageData.SiteLockObj)
		{
			int num = SiteStorage<T>.SiteCount++;
			int num2 = num - 1500;
			Type type = SiteStorage<T>.SiteStorageType(num);
			FieldInfo field;
			System.Linq.Expressions.Expression expr;
			if (num2 < 0)
			{
				field = type.GetField($"Site{num % 50:000}");
				expr = System.Linq.Expressions.Expression.Field(null, field);
			}
			else
			{
				field = typeof(SiteStorage<T>).GetField("Sites");
				expr = System.Linq.Expressions.Expression.ArrayIndex(System.Linq.Expressions.Expression.Field(null, field), System.Linq.Expressions.Expression.Constant(num2, typeof(int)));
			}
			return PublishSite(new SiteInfo<T>(binder, expr, field, num));
		}
	}

	private static SiteInfo NextSite(DynamicMetaObjectBinder binder, Type delegateType)
	{
		Type type = typeof(SiteStorage<>).MakeGenericType(delegateType);
		lock (StorageData.SiteLockObj)
		{
			int num = (int)type.GetField("SiteCount").GetValue(null);
			type.GetField("SiteCount").SetValue(null, num + 1);
			int num2 = num - 1500;
			Type type2 = (Type)type.GetMethod("SiteStorageType").Invoke(null, new object[1] { num });
			FieldInfo field;
			System.Linq.Expressions.Expression expr;
			if (num2 < 0)
			{
				field = type2.GetField($"Site{num % 50:000}");
				expr = System.Linq.Expressions.Expression.Field(null, field);
			}
			else
			{
				field = type.GetField("Sites");
				expr = System.Linq.Expressions.Expression.ArrayIndex(System.Linq.Expressions.Expression.Field(null, field), System.Linq.Expressions.Expression.Constant(num2, typeof(int)));
			}
			return PublishSite(new SiteInfoLarge(binder, expr, field, num, delegateType));
		}
	}

	public override void PublishContext(CodeContext context, ConstantInfo codeContextInfo)
	{
		int num = codeContextInfo.Offset - 50;
		if (num >= 0)
		{
			lock (StorageData.Contexts)
			{
				StorageData.Contexts[num] = context;
			}
		}
		else
		{
			codeContextInfo.Field.SetValue(null, context);
		}
		((CodeContextExpression)codeContextInfo.Expression).Context = context;
	}

	private static void PublishConstant(object constant, ConstantInfo info)
	{
		StorageData.ConstantStorageType(info.Offset);
		PublishWorker(0, 5, info, constant, StorageData.Constants);
	}

	private static SiteInfo PublishSite(SiteInfo si)
	{
		int num = si.Offset - 1500;
		CallSite callSite = si.MakeSite();
		if (num >= 0)
		{
			lock (StorageData.SiteLockObj)
			{
				((CallSite[])si.Field.GetValue(null))[num] = callSite;
			}
		}
		else
		{
			si.Field.SetValue(null, callSite);
		}
		return si;
	}

	private static void PublishWorker<T>(int start, int nTypes, ConstantInfo info, T value, T[] fallbackArray)
	{
		int num = start + info.Offset - nTypes * 50;
		((ReducibleExpression)info.Expression).Start = start;
		if (num < 0)
		{
			((ReducibleExpression)info.Expression).FieldInfo.SetValue(null, value);
		}
		else
		{
			fallbackArray[num] = value;
		}
	}
}
