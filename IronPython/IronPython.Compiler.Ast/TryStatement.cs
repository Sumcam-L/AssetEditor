using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

public class TryStatement : Statement
{
	private int _headerIndex;

	private Statement _body;

	private readonly TryStatementHandler[] _handlers;

	private Statement _else;

	private Statement _finally;

	public int HeaderIndex
	{
		set
		{
			_headerIndex = value;
		}
	}

	public Statement Body => _body;

	public Statement Else => _else;

	public Statement Finally => _finally;

	public IList<TryStatementHandler> Handlers => _handlers;

	public TryStatement(Statement body, TryStatementHandler[] handlers, Statement else_, Statement finally_)
	{
		_body = body;
		_handlers = handlers;
		_else = else_;
		_finally = finally_;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		ParameterExpression parameterExpression = null;
		ParameterExpression parameterExpression2 = null;
		if (_else != null || (_handlers != null && _handlers.Length > 0))
		{
			parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(bool), "$lineUpdated_try");
			if (_else != null)
			{
				parameterExpression2 = System.Linq.Expressions.Expression.Variable(typeof(bool), "run_else");
			}
		}
		System.Linq.Expressions.Expression body = _body;
		System.Linq.Expressions.Expression expression = _else;
		ParameterExpression parameterExpression3;
		System.Linq.Expressions.Expression expression2;
		if (_handlers != null && _handlers.Length > 0)
		{
			parameterExpression3 = System.Linq.Expressions.Expression.Variable(typeof(Exception), "$exception");
			expression2 = TransformHandlers(parameterExpression3);
		}
		else
		{
			parameterExpression3 = null;
			expression2 = null;
		}
		System.Linq.Expressions.Expression body2 = ((expression != null) ? System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(parameterExpression2, Utils.Constant(true)), Node.PushLineUpdated(updated: false, parameterExpression), LightExceptions.RewriteExternal(Utils.Try(base.Parent.AddDebugInfo(Utils.Empty(), new SourceSpan(base.Span.Start, base.GlobalParent.IndexToLocation(_headerIndex))), body, Utils.Constant(null)).Catch(parameterExpression3, System.Linq.Expressions.Expression.Assign(parameterExpression2, Utils.Constant(false)), expression2, Node.PopLineUpdated(parameterExpression), System.Linq.Expressions.Expression.Assign(parameterExpression3, System.Linq.Expressions.Expression.Constant(null, typeof(Exception))), Utils.Constant(null))), Utils.IfThen(parameterExpression2, expression), Utils.Empty()) : ((expression2 == null) ? body : LightExceptions.RewriteExternal(Utils.Try(base.GlobalParent.AddDebugInfo(Utils.Empty(), new SourceSpan(base.Span.Start, base.GlobalParent.IndexToLocation(_headerIndex))), Node.PushLineUpdated(updated: false, parameterExpression), body, Utils.Constant(null)).Catch(parameterExpression3, expression2, Node.PopLineUpdated(parameterExpression), System.Linq.Expressions.Expression.Call(AstMethods.ExceptionHandled, base.Parent.LocalContext), System.Linq.Expressions.Expression.Assign(parameterExpression3, System.Linq.Expressions.Expression.Constant(null, typeof(Exception))), Utils.Constant(null)))));
		return System.Linq.Expressions.Expression.Block(GetVariables(parameterExpression, parameterExpression2), AddFinally(body2), Utils.Default(typeof(void)));
	}

	private static ReadOnlyCollectionBuilder<ParameterExpression> GetVariables(ParameterExpression lineUpdated, ParameterExpression runElse)
	{
		ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>();
		if (lineUpdated != null)
		{
			readOnlyCollectionBuilder.Add(lineUpdated);
		}
		if (runElse != null)
		{
			readOnlyCollectionBuilder.Add(runElse);
		}
		return readOnlyCollectionBuilder;
	}

	private System.Linq.Expressions.Expression AddFinally(System.Linq.Expressions.Expression body)
	{
		if (_finally != null)
		{
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(Exception), "$tryThrows");
			ParameterExpression parameterExpression2 = System.Linq.Expressions.Expression.Variable(typeof(Exception), "$localException");
			System.Linq.Expressions.Expression expression = _finally;
			body = Utils.Try(Utils.Try(base.Parent.AddDebugInfo(Utils.Empty(), new SourceSpan(base.Span.Start, base.GlobalParent.IndexToLocation(_headerIndex))), System.Linq.Expressions.Expression.Assign(parameterExpression, Utils.Constant(null, typeof(Exception))), body, Utils.Empty()).Catch(parameterExpression2, System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(parameterExpression, parameterExpression2), System.Linq.Expressions.Expression.Rethrow()))).FinallyWithJumps(Utils.If(System.Linq.Expressions.Expression.NotEqual(parameterExpression, System.Linq.Expressions.Expression.Default(typeof(Exception))), base.Parent.GetSaveLineNumberExpression(parameterExpression, preventAdditionalAdds: false)), Node.UpdateLineUpdated(updated: false), expression, Utils.If(System.Linq.Expressions.Expression.NotEqual(parameterExpression, System.Linq.Expressions.Expression.Default(typeof(Exception))), Node.UpdateLineUpdated(updated: true)));
			body = System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { parameterExpression }, body);
		}
		return body;
	}

	private System.Linq.Expressions.Expression TransformHandlers(ParameterExpression exception)
	{
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(object), "$extracted");
		List<Microsoft.Scripting.Ast.IfStatementTest> list = new List<Microsoft.Scripting.Ast.IfStatementTest>(_handlers.Length);
		ParameterExpression parameterExpression2 = null;
		System.Linq.Expressions.Expression expression = null;
		for (int i = 0; i < _handlers.Length; i++)
		{
			TryStatementHandler tryStatementHandler = _handlers[i];
			if (tryStatementHandler.Test != null)
			{
				System.Linq.Expressions.Expression expression2 = System.Linq.Expressions.Expression.Call(AstMethods.CheckException, base.Parent.LocalContext, parameterExpression, Utils.Convert(tryStatementHandler.Test, typeof(object)));
				Microsoft.Scripting.Ast.IfStatementTest item;
				if (tryStatementHandler.Target != null)
				{
					if (parameterExpression2 == null)
					{
						parameterExpression2 = System.Linq.Expressions.Expression.Variable(typeof(object), "$converted");
					}
					item = Utils.IfCondition(System.Linq.Expressions.Expression.NotEqual(System.Linq.Expressions.Expression.Assign(parameterExpression2, expression2), Utils.Constant(null)), System.Linq.Expressions.Expression.Block(tryStatementHandler.Target.TransformSet(SourceSpan.None, parameterExpression2, PythonOperationKind.None), base.GlobalParent.AddDebugInfo(GetTracebackHeader(this, exception, tryStatementHandler.Body), new SourceSpan(base.GlobalParent.IndexToLocation(tryStatementHandler.StartIndex), base.GlobalParent.IndexToLocation(tryStatementHandler.HeaderIndex))), Utils.Empty()));
				}
				else
				{
					item = Utils.IfCondition(System.Linq.Expressions.Expression.NotEqual(expression2, Utils.Constant(null)), base.GlobalParent.AddDebugInfo(GetTracebackHeader(this, exception, tryStatementHandler.Body), new SourceSpan(base.GlobalParent.IndexToLocation(tryStatementHandler.StartIndex), base.GlobalParent.IndexToLocation(tryStatementHandler.HeaderIndex))));
				}
				list.Add(item);
			}
			else
			{
				expression = base.GlobalParent.AddDebugInfo(GetTracebackHeader(this, exception, tryStatementHandler.Body), new SourceSpan(base.GlobalParent.IndexToLocation(tryStatementHandler.StartIndex), base.GlobalParent.IndexToLocation(tryStatementHandler.HeaderIndex)));
			}
		}
		System.Linq.Expressions.Expression expression3 = null;
		if (list.Count > 0)
		{
			if (expression == null)
			{
				expression = System.Linq.Expressions.Expression.Block(base.Parent.GetSaveLineNumberExpression(exception, preventAdditionalAdds: true), System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.Call(typeof(ExceptionHelpers).GetMethod("UpdateForRethrow"), exception)));
			}
			expression3 = Utils.If(list.ToArray(), expression);
		}
		else
		{
			expression3 = expression;
		}
		IList<ParameterExpression> variables;
		if (parameterExpression2 != null)
		{
			ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>();
			readOnlyCollectionBuilder.Add(parameterExpression2);
			readOnlyCollectionBuilder.Add(parameterExpression);
			variables = readOnlyCollectionBuilder;
		}
		else
		{
			ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder2 = new ReadOnlyCollectionBuilder<ParameterExpression>();
			readOnlyCollectionBuilder2.Add(parameterExpression);
			variables = readOnlyCollectionBuilder2;
		}
		return System.Linq.Expressions.Expression.Block(variables, System.Linq.Expressions.Expression.Assign(parameterExpression, System.Linq.Expressions.Expression.Call(AstMethods.SetCurrentException, base.Parent.LocalContext, exception)), expression3, System.Linq.Expressions.Expression.Assign(parameterExpression, System.Linq.Expressions.Expression.Constant(null)), Utils.Empty());
	}

	internal static System.Linq.Expressions.Expression GetTracebackHeader(Statement node, ParameterExpression exception, System.Linq.Expressions.Expression body)
	{
		return System.Linq.Expressions.Expression.Block(node.Parent.GetSaveLineNumberExpression(exception, preventAdditionalAdds: false), System.Linq.Expressions.Expression.Call(AstMethods.BuildExceptionInfo, node.Parent.LocalContext, exception), body, Utils.Empty());
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_body != null)
			{
				_body.Walk(walker);
			}
			if (_handlers != null)
			{
				TryStatementHandler[] handlers = _handlers;
				foreach (TryStatementHandler tryStatementHandler in handlers)
				{
					tryStatementHandler.Walk(walker);
				}
			}
			if (_else != null)
			{
				_else.Walk(walker);
			}
			if (_finally != null)
			{
				_finally.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
