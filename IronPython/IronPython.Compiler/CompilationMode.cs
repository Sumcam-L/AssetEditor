using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler;

[Serializable]
internal abstract class CompilationMode
{
	public class ConstantInfo
	{
		public readonly System.Linq.Expressions.Expression Expression;

		public readonly FieldInfo Field;

		public readonly int Offset;

		public ConstantInfo(System.Linq.Expressions.Expression expr, FieldInfo field, int offset)
		{
			Expression = expr;
			Field = field;
			Offset = offset;
		}
	}

	public abstract class SiteInfo : ConstantInfo
	{
		public readonly DynamicMetaObjectBinder Binder;

		public readonly Type DelegateType;

		protected Type _siteType;

		public Type SiteType
		{
			get
			{
				if (_siteType != null)
				{
					_siteType = typeof(CallSite<>).MakeGenericType(DelegateType);
				}
				return _siteType;
			}
		}

		public SiteInfo(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo field, int index, Type delegateType)
			: base(expr, field, index)
		{
			Binder = binder;
			DelegateType = delegateType;
		}

		public SiteInfo(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo field, int index, Type delegateType, Type siteType)
			: this(binder, expr, field, index, delegateType)
		{
			_siteType = siteType;
		}

		public abstract CallSite MakeSite();
	}

	public class SiteInfoLarge : SiteInfo
	{
		public SiteInfoLarge(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo field, int index, Type delegateType)
			: base(binder, expr, field, index, delegateType)
		{
		}

		public override CallSite MakeSite()
		{
			return CallSite.Create(DelegateType, Binder);
		}
	}

	public class SiteInfo<T> : SiteInfo where T : class
	{
		public SiteInfo(DynamicMetaObjectBinder binder, System.Linq.Expressions.Expression expr, FieldInfo field, int index)
			: base(binder, expr, field, index, typeof(T), typeof(CallSite<T>))
		{
		}

		public override CallSite MakeSite()
		{
			return CallSite<T>.Create(Binder);
		}
	}

	public static readonly CompilationMode ToDisk = new ToDiskCompilationMode();

	public static readonly CompilationMode Uncollectable = new UncollectableCompilationMode();

	public static readonly CompilationMode Collectable = new CollectableCompilationMode();

	public static readonly CompilationMode Lookup = new LookupCompilationMode();

	public virtual Type DelegateType => typeof(Expression<LookupCompilationDelegate>);

	public virtual ScriptCode MakeScriptCode(PythonAst ast)
	{
		return new RuntimeScriptCode(ast, ast.ModuleContext.GlobalContext);
	}

	public virtual System.Linq.Expressions.Expression GetConstant(object value)
	{
		return System.Linq.Expressions.Expression.Constant(value);
	}

	public virtual Type GetConstantType(object value)
	{
		if (value == null)
		{
			return typeof(object);
		}
		return value.GetType();
	}

	public virtual void PrepareScope(PythonAst ast, ReadOnlyCollectionBuilder<ParameterExpression> locals, List<System.Linq.Expressions.Expression> init)
	{
	}

	public virtual ConstantInfo GetContext()
	{
		return null;
	}

	public virtual void PublishContext(CodeContext codeContext, ConstantInfo _contextInfo)
	{
	}

	public System.Linq.Expressions.Expression Dynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0)
	{
		if (retType == typeof(object))
		{
			return new PythonDynamicExpression1(binder, this, arg0);
		}
		if (retType == typeof(bool))
		{
			return new PythonDynamicExpression1<bool>(binder, this, arg0);
		}
		return ReduceDynamic(binder, retType, arg0);
	}

	public System.Linq.Expressions.Expression Dynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1)
	{
		if (retType == typeof(object))
		{
			return new PythonDynamicExpression2(binder, this, arg0, arg1);
		}
		if (retType == typeof(bool))
		{
			return new PythonDynamicExpression2<bool>(binder, this, arg0, arg1);
		}
		return ReduceDynamic(binder, retType, arg0, arg1);
	}

	public System.Linq.Expressions.Expression Dynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2)
	{
		if (retType == typeof(object))
		{
			return new PythonDynamicExpression3(binder, this, arg0, arg1, arg2);
		}
		return ReduceDynamic(binder, retType, arg0, arg1, arg2);
	}

	public System.Linq.Expressions.Expression Dynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2, System.Linq.Expressions.Expression arg3)
	{
		if (retType == typeof(object))
		{
			return new PythonDynamicExpression4(binder, this, arg0, arg1, arg2, arg3);
		}
		return ReduceDynamic(binder, retType, arg0, arg1, arg2, arg3);
	}

	public System.Linq.Expressions.Expression Dynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression[] args)
	{
		switch (args.Length)
		{
		case 1:
			return Dynamic(binder, retType, args[0]);
		case 2:
			return Dynamic(binder, retType, args[0], args[1]);
		case 3:
			return Dynamic(binder, retType, args[0], args[1], args[2]);
		case 4:
			return Dynamic(binder, retType, args[0], args[1], args[2], args[3]);
		default:
			if (retType == typeof(object))
			{
				return new PythonDynamicExpressionN(binder, this, args);
			}
			return ReduceDynamic(binder, retType, args);
		}
	}

	public virtual System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0)
	{
		return System.Linq.Expressions.Expression.Dynamic(binder, retType, arg0);
	}

	public virtual System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1)
	{
		return System.Linq.Expressions.Expression.Dynamic(binder, retType, arg0, arg1);
	}

	public virtual System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2)
	{
		return System.Linq.Expressions.Expression.Dynamic(binder, retType, arg0, arg1, arg2);
	}

	public virtual System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression arg0, System.Linq.Expressions.Expression arg1, System.Linq.Expressions.Expression arg2, System.Linq.Expressions.Expression arg3)
	{
		return System.Linq.Expressions.Expression.Dynamic(binder, retType, arg0, arg1, arg2, arg3);
	}

	public virtual System.Linq.Expressions.Expression ReduceDynamic(DynamicMetaObjectBinder binder, Type retType, System.Linq.Expressions.Expression[] args)
	{
		return System.Linq.Expressions.Expression.Dynamic(binder, retType, args);
	}

	public abstract System.Linq.Expressions.Expression GetGlobal(System.Linq.Expressions.Expression globalContext, int arrayIndex, PythonVariable variable, PythonGlobal global);

	public abstract LightLambdaExpression ReduceAst(PythonAst instance, string name);
}
