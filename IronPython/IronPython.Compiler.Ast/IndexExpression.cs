using System.Linq.Expressions;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class IndexExpression : Expression
{
	private readonly Expression _target;

	private readonly Expression _index;

	public Expression Target => _target;

	public Expression Index => _index;

	private bool IsSlice => _index is SliceExpression;

	public IndexExpression(Expression target, Expression index)
	{
		_target = target;
		_index = index;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (IsSlice)
		{
			return base.GlobalParent.GetSlice(GetActionArgumentsForGetOrDelete());
		}
		return base.GlobalParent.GetIndex(GetActionArgumentsForGetOrDelete());
	}

	private System.Linq.Expressions.Expression[] GetActionArgumentsForGetOrDelete()
	{
		if (_index is TupleExpression { IsExpandable: not false } tupleExpression)
		{
			return ArrayUtils.Insert(_target, tupleExpression.Items);
		}
		if (_index is SliceExpression sliceExpression)
		{
			if (sliceExpression.StepProvided)
			{
				return new System.Linq.Expressions.Expression[4]
				{
					_target,
					GetSliceValue(sliceExpression.SliceStart),
					GetSliceValue(sliceExpression.SliceStop),
					GetSliceValue(sliceExpression.SliceStep)
				};
			}
			return new System.Linq.Expressions.Expression[3]
			{
				_target,
				GetSliceValue(sliceExpression.SliceStart),
				GetSliceValue(sliceExpression.SliceStop)
			};
		}
		return new Expression[2] { _target, _index };
	}

	private static System.Linq.Expressions.Expression GetSliceValue(Expression expr)
	{
		if (expr != null)
		{
			return expr;
		}
		return System.Linq.Expressions.Expression.Field(null, typeof(MissingParameter).GetField("Value"));
	}

	private System.Linq.Expressions.Expression[] GetActionArgumentsForSet(System.Linq.Expressions.Expression right)
	{
		return ArrayUtils.Append(GetActionArgumentsForGetOrDelete(), right);
	}

	internal override System.Linq.Expressions.Expression TransformSet(SourceSpan span, System.Linq.Expressions.Expression right, PythonOperationKind op)
	{
		if (op != PythonOperationKind.None)
		{
			right = base.GlobalParent.Operation(typeof(object), op, this, right);
		}
		System.Linq.Expressions.Expression expression = ((!IsSlice) ? base.GlobalParent.SetIndex(GetActionArgumentsForSet(right)) : base.GlobalParent.SetSlice(GetActionArgumentsForSet(right)));
		return base.GlobalParent.AddDebugInfoAndVoid(expression, base.Span);
	}

	internal override System.Linq.Expressions.Expression TransformDelete()
	{
		System.Linq.Expressions.Expression expression = ((!IsSlice) ? base.GlobalParent.DeleteIndex(GetActionArgumentsForGetOrDelete()) : base.GlobalParent.DeleteSlice(GetActionArgumentsForGetOrDelete()));
		return base.GlobalParent.AddDebugInfoAndVoid(expression, base.Span);
	}

	internal override string CheckAssign()
	{
		return null;
	}

	internal override string CheckDelete()
	{
		return null;
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_target != null)
			{
				_target.Walk(walker);
			}
			if (_index != null)
			{
				_index.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
