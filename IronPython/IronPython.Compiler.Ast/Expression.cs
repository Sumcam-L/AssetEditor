using System;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;

namespace IronPython.Compiler.Ast;

public abstract class Expression : Node
{
	internal static Expression[] EmptyArray = new Expression[0];

	internal virtual bool IsConstant => ConstantFold()?.IsConstant ?? false;

	public override Type Type => typeof(object);

	internal virtual System.Linq.Expressions.Expression TransformSet(SourceSpan span, System.Linq.Expressions.Expression right, PythonOperationKind op)
	{
		throw new InvalidOperationException();
	}

	internal virtual System.Linq.Expressions.Expression TransformDelete()
	{
		throw new InvalidOperationException();
	}

	internal virtual ConstantExpression ConstantFold()
	{
		return null;
	}

	internal virtual string CheckAssign()
	{
		return "can't assign to " + NodeName;
	}

	internal virtual string CheckAugmentedAssign()
	{
		if (CheckAssign() != null)
		{
			return "illegal expression for augmented assignment";
		}
		return null;
	}

	internal virtual string CheckDelete()
	{
		return "can't delete " + NodeName;
	}

	internal virtual object GetConstantValue()
	{
		ConstantExpression constantExpression = ConstantFold();
		if (constantExpression != null && constantExpression.IsConstant)
		{
			return constantExpression.GetConstantValue();
		}
		throw new InvalidOperationException(GetType().Name + " is not a constant");
	}
}
