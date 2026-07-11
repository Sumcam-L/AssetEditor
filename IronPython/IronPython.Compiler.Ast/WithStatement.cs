using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class WithStatement : Statement
{
	private int _headerIndex;

	private readonly Expression _contextManager;

	private readonly Expression _var;

	private Statement _body;

	public int HeaderIndex
	{
		set
		{
			_headerIndex = value;
		}
	}

	public new Expression Variable => _var;

	public Expression ContextManager => _contextManager;

	public Statement Body => _body;

	public WithStatement(Expression contextManager, Expression var, Statement body)
	{
		_contextManager = contextManager;
		_var = var;
		_body = body;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(6);
		ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder2 = new ReadOnlyCollectionBuilder<ParameterExpression>(6);
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Variable(typeof(bool), "$lineUpdated_with");
		readOnlyCollectionBuilder2.Add(parameterExpression);
		ParameterExpression parameterExpression2 = System.Linq.Expressions.Expression.Variable(typeof(object), "with_manager");
		readOnlyCollectionBuilder2.Add(parameterExpression2);
		readOnlyCollectionBuilder.Add(base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Assign(parameterExpression2, _contextManager), new SourceSpan(base.GlobalParent.IndexToLocation(base.StartIndex), base.GlobalParent.IndexToLocation(_headerIndex))));
		ParameterExpression parameterExpression3 = System.Linq.Expressions.Expression.Variable(typeof(object), "with_exit");
		readOnlyCollectionBuilder2.Add(parameterExpression3);
		readOnlyCollectionBuilder.Add(Node.MakeAssignment(parameterExpression3, base.GlobalParent.Get("__exit__", parameterExpression2)));
		ParameterExpression parameterExpression4 = System.Linq.Expressions.Expression.Variable(typeof(object), "with_value");
		readOnlyCollectionBuilder2.Add(parameterExpression4);
		readOnlyCollectionBuilder.Add(base.GlobalParent.AddDebugInfoAndVoid(Node.MakeAssignment(parameterExpression4, base.Parent.Invoke(new CallSignature(0), base.Parent.LocalContext, base.GlobalParent.Get("__enter__", parameterExpression2))), new SourceSpan(base.GlobalParent.IndexToLocation(base.StartIndex), base.GlobalParent.IndexToLocation(_headerIndex))));
		ParameterExpression parameterExpression5 = System.Linq.Expressions.Expression.Variable(typeof(bool), "with_exc");
		readOnlyCollectionBuilder2.Add(parameterExpression5);
		readOnlyCollectionBuilder.Add(Node.MakeAssignment(parameterExpression5, Utils.Constant(true)));
		ParameterExpression parameterExpression6;
		readOnlyCollectionBuilder.Add(Utils.Try(Utils.Try(Node.PushLineUpdated(updated: false, parameterExpression), (_var != null) ? ((System.Linq.Expressions.Expression)System.Linq.Expressions.Expression.Block(_var.TransformSet(SourceSpan.None, parameterExpression4, PythonOperationKind.None), _body, Utils.Empty())) : ((System.Linq.Expressions.Expression)_body)).Catch(parameterExpression6 = System.Linq.Expressions.Expression.Variable(typeof(Exception), "exception"), TryStatement.GetTracebackHeader(this, parameterExpression6, base.GlobalParent.AddDebugInfoAndVoid(System.Linq.Expressions.Expression.Block(Node.MakeAssignment(parameterExpression5, Utils.Constant(false)), Utils.IfThen(base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, base.GlobalParent.Operation(typeof(bool), PythonOperationKind.IsFalse, MakeExitCall(parameterExpression3, parameterExpression6))), Node.UpdateLineUpdated(updated: true), System.Linq.Expressions.Expression.Throw(System.Linq.Expressions.Expression.Call(AstMethods.MakeRethrowExceptionWorker, parameterExpression6)))), _body.Span)), Node.PopLineUpdated(parameterExpression), System.Linq.Expressions.Expression.Empty())).Finally(Utils.IfThen(parameterExpression5, base.GlobalParent.AddDebugInfoAndVoid(System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Dynamic(base.GlobalParent.PyContext.Invoke(new CallSignature(3)), typeof(object), base.Parent.LocalContext, parameterExpression3, Utils.Constant(null), Utils.Constant(null), Utils.Constant(null)), System.Linq.Expressions.Expression.Empty()), _contextManager.Span))));
		readOnlyCollectionBuilder.Add(Utils.Empty());
		return System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder2.ToReadOnlyCollection(), readOnlyCollectionBuilder.ToReadOnlyCollection());
	}

	private System.Linq.Expressions.Expression MakeExitCall(ParameterExpression exit, System.Linq.Expressions.Expression exception)
	{
		return base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, base.Parent.Invoke(new CallSignature(ArgumentType.List), base.Parent.LocalContext, exit, System.Linq.Expressions.Expression.Call(AstMethods.GetExceptionInfoLocal, base.Parent.LocalContext, exception)));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_contextManager != null)
			{
				_contextManager.Walk(walker);
			}
			if (_var != null)
			{
				_var.Walk(walker);
			}
			if (_body != null)
			{
				_body.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
