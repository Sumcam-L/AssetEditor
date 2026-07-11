using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class ForStatement : Statement, ILoopStatement
{
	private int _headerIndex;

	private readonly Expression _left;

	private Expression _list;

	private Statement _body;

	private readonly Statement _else;

	private LabelTarget _break;

	private LabelTarget _continue;

	public int HeaderIndex
	{
		set
		{
			_headerIndex = value;
		}
	}

	public Expression Left => _left;

	public Statement Body
	{
		get
		{
			return _body;
		}
		set
		{
			_body = value;
		}
	}

	public Expression List
	{
		get
		{
			return _list;
		}
		set
		{
			_list = value;
		}
	}

	public Statement Else => _else;

	LabelTarget ILoopStatement.BreakLabel
	{
		get
		{
			return _break;
		}
		set
		{
			_break = value;
		}
	}

	LabelTarget ILoopStatement.ContinueLabel
	{
		get
		{
			return _continue;
		}
		set
		{
			_continue = value;
		}
	}

	internal override bool CanThrow
	{
		get
		{
			if (_left.CanThrow)
			{
				return true;
			}
			if (_list.CanThrow)
			{
				return true;
			}
			if (_list is ConstantExpression constantExpression)
			{
				if (constantExpression.Value is string)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public ForStatement(Expression left, Expression list, Statement body, Statement else_)
	{
		_left = left;
		_list = list;
		_body = body;
		_else = else_;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(KeyValuePair<IEnumerator, IDisposable>), "foreach_enumerator");
		return System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, TransformFor(base.Parent, parameterExpression, _list, _left, _body, _else, base.Span, base.GlobalParent.IndexToLocation(_headerIndex), _break, _continue, isStatement: true));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_left != null)
			{
				_left.Walk(walker);
			}
			if (_list != null)
			{
				_list.Walk(walker);
			}
			if (_body != null)
			{
				_body.Walk(walker);
			}
			if (_else != null)
			{
				_else.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}

	internal static System.Linq.Expressions.Expression TransformFor(ScopeStatement parent, ParameterExpression enumerator, Expression list, Expression left, System.Linq.Expressions.Expression body, Statement else_, SourceSpan span, SourceLocation header, LabelTarget breakLabel, LabelTarget continueLabel, bool isStatement)
	{
		System.Linq.Expressions.Expression arg = System.Linq.Expressions.Expression.Assign(enumerator, new PythonDynamicExpression1<KeyValuePair<IEnumerator, IDisposable>>(Binders.UnaryOperationBinder(parent.GlobalParent.PyContext, PythonOperationKind.GetEnumeratorForIteration), parent.GlobalParent.CompilationMode, Utils.Convert(list, typeof(object))));
		System.Linq.Expressions.Expression body2 = Utils.Loop(parent.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Property(enumerator, typeof(KeyValuePair<IEnumerator, IDisposable>).GetProperty("Key")), typeof(IEnumerator).GetMethod("MoveNext")), left.Span), null, System.Linq.Expressions.Expression.Block(left.TransformSet(SourceSpan.None, System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Property(enumerator, typeof(KeyValuePair<IEnumerator, IDisposable>).GetProperty("Key")), typeof(IEnumerator).GetProperty("Current").GetGetMethod()), PythonOperationKind.None), body, isStatement ? Node.UpdateLineNumber(parent.GlobalParent.IndexToLocation(list.StartIndex).Line) : Utils.Empty(), Utils.Empty()), else_, breakLabel, continueLabel);
		return System.Linq.Expressions.Expression.Block(arg, System.Linq.Expressions.Expression.TryFinally(body2, System.Linq.Expressions.Expression.Call(AstMethods.ForLoopDispose, enumerator)));
	}
}
