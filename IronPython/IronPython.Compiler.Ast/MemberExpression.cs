using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class MemberExpression : Expression
{
	private readonly Expression _target;

	private readonly string _name;

	public Expression Target => _target;

	public string Name => _name;

	public MemberExpression(Expression target, string name)
	{
		_target = target;
		_name = name;
	}

	public override string ToString()
	{
		return base.ToString() + ":" + _name;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return base.GlobalParent.Get(_name, _target);
	}

	internal override System.Linq.Expressions.Expression TransformSet(SourceSpan span, System.Linq.Expressions.Expression right, PythonOperationKind op)
	{
		if (op == PythonOperationKind.None)
		{
			return base.GlobalParent.AddDebugInfoAndVoid(base.GlobalParent.Set(_name, _target, right), span);
		}
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "inplace");
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, System.Linq.Expressions.Expression.Assign(parameterExpression, _target), SetMemberOperator(right, op, parameterExpression), Utils.Empty()), base.Span.Start, span.End);
	}

	internal override string CheckAssign()
	{
		return null;
	}

	internal override string CheckDelete()
	{
		return null;
	}

	private System.Linq.Expressions.Expression SetMemberOperator(System.Linq.Expressions.Expression right, PythonOperationKind op, ParameterExpression temp)
	{
		return base.GlobalParent.Set(_name, temp, base.GlobalParent.Operation(typeof(object), op, base.GlobalParent.Get(_name, temp), right));
	}

	internal override System.Linq.Expressions.Expression TransformDelete()
	{
		return base.GlobalParent.Delete(typeof(void), _name, _target);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _target != null)
		{
			_target.Walk(walker);
		}
		walker.PostWalk(this);
	}
}
