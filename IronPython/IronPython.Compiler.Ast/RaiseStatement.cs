using System;
using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class RaiseStatement : Statement
{
	private readonly Expression _type;

	private readonly Expression _value;

	private readonly Expression _traceback;

	private bool _inFinally;

	[Obsolete("Type is obsolete due to direct inheritance from DLR Expression.  Use ExceptType instead")]
	public new Expression Type => _type;

	public Expression ExceptType => _type;

	public Expression Value => _value;

	public Expression Traceback => _traceback;

	internal bool InFinally
	{
		get
		{
			return _inFinally;
		}
		set
		{
			_inFinally = value;
		}
	}

	public RaiseStatement(Expression exceptionType, Expression exceptionValue, Expression traceBack)
	{
		_type = exceptionType;
		_value = exceptionValue;
		_traceback = traceBack;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		System.Linq.Expressions.Expression expression;
		if (_type == null && _value == null && _traceback == null)
		{
			expression = System.Linq.Expressions.Expression.Call(AstMethods.MakeRethrownException, base.Parent.LocalContext);
			if (!InFinally)
			{
				expression = System.Linq.Expressions.Expression.Block(Node.UpdateLineUpdated(updated: true), expression);
			}
		}
		else
		{
			expression = System.Linq.Expressions.Expression.Call(AstMethods.MakeException, base.Parent.LocalContext, Node.TransformOrConstantNull(_type, typeof(object)), Node.TransformOrConstantNull(_value, typeof(object)), Node.TransformOrConstantNull(_traceback, typeof(object)));
		}
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Throw(expression), base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_type != null)
			{
				_type.Walk(walker);
			}
			if (_value != null)
			{
				_value.Walk(walker);
			}
			if (_traceback != null)
			{
				_traceback.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
