using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class AssignmentStatement : Statement
{
	private readonly Expression[] _left;

	private readonly Expression _right;

	public IList<Expression> Left => _left;

	public Expression Right => _right;

	public AssignmentStatement(Expression[] left, Expression right)
	{
		_left = left;
		_right = right;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (_left.Length == 1)
		{
			return AssignOne();
		}
		return AssignComplex(_right);
	}

	private System.Linq.Expressions.Expression AssignComplex(System.Linq.Expressions.Expression right)
	{
		List<System.Linq.Expressions.Expression> list = new List<System.Linq.Expressions.Expression>();
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "assignment");
		list.Add(Node.MakeAssignment(parameterExpression, right));
		Expression[] left = _left;
		foreach (Expression expression in left)
		{
			if (expression != null)
			{
				System.Linq.Expressions.Expression item = expression.TransformSet(base.Span, parameterExpression, PythonOperationKind.None);
				list.Add(item);
			}
		}
		list.Add(Utils.Empty());
		return base.GlobalParent.AddDebugInfoAndVoid(System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, list.ToArray()), base.Span);
	}

	private System.Linq.Expressions.Expression AssignOne()
	{
		SequenceExpression sequenceExpression = _left[0] as SequenceExpression;
		SequenceExpression sequenceExpression2 = _right as SequenceExpression;
		if (sequenceExpression != null && sequenceExpression2 != null && sequenceExpression.Items.Count == sequenceExpression2.Items.Count)
		{
			int count = sequenceExpression.Items.Count;
			ParameterExpression[] array = new ParameterExpression[count];
			System.Linq.Expressions.Expression[] array2 = new System.Linq.Expressions.Expression[count * 2 + 1];
			for (int i = 0; i < count; i++)
			{
				System.Linq.Expressions.Expression expression = sequenceExpression2.Items[i];
				array[i] = System.Linq.Expressions.Expression.Variable(expression.Type, "parallelAssign");
				array2[i] = System.Linq.Expressions.Expression.Assign(array[i], expression);
			}
			for (int j = 0; j < count; j++)
			{
				array2[j + count] = sequenceExpression.Items[j].TransformSet(SourceSpan.None, array[j], PythonOperationKind.None);
			}
			array2[count * 2] = Utils.Empty();
			return base.GlobalParent.AddDebugInfoAndVoid(System.Linq.Expressions.Expression.Block(array, array2), base.Span);
		}
		return _left[0].TransformSet(base.Span, _right, PythonOperationKind.None);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			Expression[] left = _left;
			foreach (Expression expression in left)
			{
				expression.Walk(walker);
			}
			_right.Walk(walker);
		}
		walker.PostWalk(this);
	}
}
