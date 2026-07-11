using System;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class AndExpression : Expression
{
	private readonly Expression _left;

	private readonly Expression _right;

	public Expression Left => _left;

	public Expression Right => _right;

	public override Type Type
	{
		get
		{
			if (!(_left.Type == _right.Type))
			{
				return typeof(object);
			}
			return _left.Type;
		}
	}

	internal override bool CanThrow
	{
		get
		{
			if (!_left.CanThrow)
			{
				return _right.CanThrow;
			}
			return true;
		}
	}

	public AndExpression(Expression left, Expression right)
	{
		ContractUtils.RequiresNotNull(left, "left");
		ContractUtils.RequiresNotNull(right, "right");
		_left = left;
		_right = right;
		base.StartIndex = left.StartIndex;
		base.EndIndex = right.EndIndex;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression left = _left;
		System.Linq.Expressions.Expression right = _right;
		Type type = Type;
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(type, "__all__");
		return System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, System.Linq.Expressions.Expression.Condition(base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, System.Linq.Expressions.Expression.Assign(parameterExpression, Utils.Convert(left, type))), Utils.Convert(right, type), parameterExpression));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_left != null)
			{
				_left.Walk(walker);
			}
			if (_right != null)
			{
				_right.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
