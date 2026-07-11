using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler;

internal class ReducableDynamicExpression : Expression, ILightExceptionAwareExpression
{
	private readonly Expression _reduction;

	private readonly DynamicMetaObjectBinder _binder;

	private readonly IList<Expression> _args;

	public DynamicMetaObjectBinder Binder => _binder;

	public IList<Expression> Args => _args;

	public override bool CanReduce => true;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => _reduction.Type;

	public ReducableDynamicExpression(Expression reduction, DynamicMetaObjectBinder binder, IList<Expression> args)
	{
		_reduction = reduction;
		_binder = binder;
		_args = args;
	}

	public override Expression Reduce()
	{
		return _reduction;
	}

	Expression ILightExceptionAwareExpression.ReduceForLightExceptions()
	{
		if (Binder is ILightExceptionBinder lightExceptionBinder)
		{
			DynamicMetaObjectBinder dynamicMetaObjectBinder = lightExceptionBinder.GetLightExceptionBinder() as DynamicMetaObjectBinder;
			if (dynamicMetaObjectBinder != lightExceptionBinder)
			{
				return Expression.Dynamic(dynamicMetaObjectBinder, Type, _args);
			}
		}
		return this;
	}
}
