using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

public abstract class SequenceExpression : Expression
{
	private readonly Expression[] _items;

	public IList<Expression> Items => _items;

	internal override bool CanThrow
	{
		get
		{
			Expression[] items = _items;
			foreach (Expression expression in items)
			{
				if (expression.CanThrow)
				{
					return true;
				}
			}
			return false;
		}
	}

	protected SequenceExpression(Expression[] items)
	{
		_items = items;
	}

	internal override System.Linq.Expressions.Expression TransformSet(SourceSpan span, System.Linq.Expressions.Expression right, PythonOperationKind op)
	{
		bool flag = false;
		Expression[] items = _items;
		foreach (Expression expr in items)
		{
			if (IsComplexAssignment(expr))
			{
				flag = true;
				break;
			}
		}
		SourceSpan span2 = SourceSpan.None;
		SourceSpan location = ((base.Span.Start.IsValid && span.IsValid) ? new SourceSpan(base.Span.Start, span.End) : SourceSpan.None);
		SourceSpan location2 = SourceSpan.None;
		if (flag)
		{
			span2 = span;
			location = SourceSpan.None;
			location2 = ((base.Span.Start.IsValid && span.IsValid) ? new SourceSpan(base.Span.Start, span.End) : SourceSpan.None);
		}
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "unpacking");
		System.Linq.Expressions.Expression expression = Node.MakeAssignment(parameterExpression, right);
		System.Linq.Expressions.Expression right2 = System.Linq.Expressions.Expression.Convert(LightExceptions.CheckAndThrow(System.Linq.Expressions.Expression.Call(flag ? AstMethods.GetEnumeratorValues : AstMethods.GetEnumeratorValuesNoComplexSets, base.Parent.LocalContext, parameterExpression, Utils.Constant(_items.Length))), typeof(object[]));
		ParameterExpression parameterExpression2 = System.Linq.Expressions.Expression.Variable(typeof(object[]), "array");
		System.Linq.Expressions.Expression expression2 = MakeAssignment(parameterExpression2, right2, span2);
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(_items.Length + 1);
		for (int j = 0; j < _items.Length; j++)
		{
			Expression expression3 = _items[j];
			if (expression3 != null)
			{
				System.Linq.Expressions.Expression right3 = System.Linq.Expressions.Expression.ArrayAccess(parameterExpression2, Utils.Constant(j));
				System.Linq.Expressions.Expression item = expression3.TransformSet(flag ? expression3.Span : SourceSpan.None, right3, PythonOperationKind.None);
				readOnlyCollectionBuilder.Add(item);
			}
		}
		readOnlyCollectionBuilder.Add(Utils.Empty());
		System.Linq.Expressions.Expression expression4 = base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder.ToReadOnlyCollection()), location);
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(new ParameterExpression[2] { parameterExpression2, parameterExpression }, expression, expression2, expression4, Utils.Empty()), location2);
	}

	internal override string CheckAssign()
	{
		return null;
	}

	internal override string CheckDelete()
	{
		return null;
	}

	internal override string CheckAugmentedAssign()
	{
		return "illegal expression for augmented assignment";
	}

	private static bool IsComplexAssignment(Expression expr)
	{
		return !(expr is NameExpression);
	}

	internal override System.Linq.Expressions.Expression TransformDelete()
	{
		System.Linq.Expressions.Expression[] array = new System.Linq.Expressions.Expression[_items.Length + 1];
		for (int i = 0; i < _items.Length; i++)
		{
			array[i] = _items[i].TransformDelete();
		}
		array[_items.Length] = Utils.Empty();
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(array), base.Span);
	}
}
