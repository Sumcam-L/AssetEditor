using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler;

internal sealed class GeneratorRewriter : DynamicExpressionVisitor
{
	private struct GotoRewriteInfo
	{
		public readonly Expression Variable;

		public readonly LabelTarget VoidTarget;

		public GotoRewriteInfo(Expression variable, LabelTarget voidTarget)
		{
			Variable = variable;
			VoidTarget = voidTarget;
		}
	}

	private class GotoRewriter : ExpressionVisitor
	{
		private readonly GotoRewriteInfo _gotoInfo;

		private readonly LabelTarget _target;

		private readonly GeneratorRewriter _rewriter;

		public GotoRewriter(GeneratorRewriter rewriter, GotoRewriteInfo gotoInfo, LabelTarget target)
		{
			_gotoInfo = gotoInfo;
			_target = target;
			_rewriter = rewriter;
		}

		protected override Expression VisitGoto(GotoExpression node)
		{
			if (node.Target == _target)
			{
				return Expression.Goto(_gotoInfo.VoidTarget, Expression.Block(_rewriter.MakeAssign(_gotoInfo.Variable, node.Value), Expression.Default(typeof(void))), node.Type);
			}
			return base.VisitGoto(node);
		}
	}

	private class RethrowRewriter : ExpressionVisitor
	{
		internal Expression Exception;

		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.NodeType == ExpressionType.Throw && node.Operand == null)
			{
				return Expression.Throw(Exception, node.Type);
			}
			return base.VisitUnary(node);
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return node;
		}

		protected override Expression VisitTry(TryExpression node)
		{
			return node;
		}

		protected override Expression VisitExtension(Expression node)
		{
			if (node is DelayedTupleExpression)
			{
				return node;
			}
			return base.VisitExtension(node);
		}
	}

	private sealed class YieldMarker
	{
		internal LabelTarget Label = Expression.Label("yieldMarker");

		internal readonly int State;

		internal YieldMarker(int state)
		{
			State = state;
		}
	}

	private const int GotoRouterYielding = 0;

	private const int GotoRouterNone = -1;

	internal const int NotStarted = -1;

	internal const int Finished = 0;

	private readonly Expression _body;

	private readonly string _name;

	private readonly StrongBox<Type> _tupleType = new StrongBox<Type>(null);

	private readonly StrongBox<ParameterExpression> _tupleExpr = new StrongBox<ParameterExpression>(null);

	private readonly Stack<LabelTarget> _returnLabels = new Stack<LabelTarget>();

	private ParameterExpression _gotoRouter;

	private bool _inTryWithFinally;

	private readonly List<YieldMarker> _yields = new List<YieldMarker>();

	private readonly Dictionary<ParameterExpression, DelayedTupleExpression> _vars = new Dictionary<ParameterExpression, DelayedTupleExpression>();

	private readonly List<KeyValuePair<ParameterExpression, DelayedTupleExpression>> _orderedVars = new List<KeyValuePair<ParameterExpression, DelayedTupleExpression>>();

	private readonly List<ParameterExpression> _temps = new List<ParameterExpression>();

	private Expression _state;

	private Expression _current;

	internal static ParameterExpression _generatorParam = Expression.Parameter(typeof(PythonGenerator), "$generator");

	internal GeneratorRewriter(string name, Expression body)
	{
		_body = body;
		_name = name;
		_returnLabels.Push(Expression.Label("retLabel"));
		_gotoRouter = Expression.Variable(typeof(int), "$gotoRouter");
	}

	internal Expression Reduce(bool shouldInterpret, bool emitDebugSymbols, int compilationThreshold, IList<ParameterExpression> parameters, Func<Expression<Func<MutableTuple, object>>, Expression<Func<MutableTuple, object>>> bodyConverter)
	{
		_state = LiftVariable(Expression.Parameter(typeof(int), "state"));
		_current = LiftVariable(Expression.Parameter(typeof(object), "current"));
		foreach (ParameterExpression parameter in parameters)
		{
			LiftVariable(parameter);
		}
		DelayedTupleExpression delayedTupleExpression = LiftVariable(_generatorParam);
		Expression expression = Visit(_body);
		int count = _yields.Count;
		SwitchCase[] array = new SwitchCase[count + 1];
		for (int i = 0; i < count; i++)
		{
			array[i] = Expression.SwitchCase(Expression.Goto(_yields[i].Label), Utils.Constant(_yields[i].State));
		}
		array[count] = Expression.SwitchCase(Expression.Goto(_returnLabels.Peek()), Utils.Constant(0));
		Expression[] array2 = new Expression[_vars.Count];
		foreach (KeyValuePair<ParameterExpression, DelayedTupleExpression> orderedVar in _orderedVars)
		{
			if (orderedVar.Value.Index >= 2 && orderedVar.Value.Index < parameters.Count + 2)
			{
				array2[orderedVar.Value.Index] = parameters[orderedVar.Value.Index - 2];
			}
			else
			{
				array2[orderedVar.Value.Index] = Expression.Default(orderedVar.Key.Type);
			}
		}
		Expression expression2 = MutableTuple.Create(array2);
		Type type = (_tupleType.Value = expression2.Type);
		ParameterExpression parameterExpression = (_tupleExpr.Value = Expression.Parameter(type, "tuple"));
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(MutableTuple), "tupleArg");
		_temps.Add(_gotoRouter);
		_temps.Add(parameterExpression);
		ParameterExpression parameterExpression3 = Expression.Parameter(type, "tuple");
		ParameterExpression parameterExpression4 = Expression.Parameter(typeof(PythonGenerator), "ret");
		Expression<Func<MutableTuple, object>> arg = Expression.Lambda<Func<MutableTuple, object>>(Expression.Block(_temps.ToArray(), Expression.Assign(parameterExpression, Expression.Convert(parameterExpression2, type)), Expression.Switch(Expression.Assign(_gotoRouter, _state), array), expression, MakeAssign(_state, Utils.Constant(0)), Expression.Label(_returnLabels.Peek()), _current), _name, new ParameterExpression[1] { parameterExpression2 });
		return Expression.Block(new ParameterExpression[2] { parameterExpression3, parameterExpression4 }, Expression.Assign(parameterExpression4, Expression.Call(typeof(PythonOps).GetMethod("MakeGenerator"), parameters[0], Expression.Assign(parameterExpression3, expression2), emitDebugSymbols ? ((Expression)bodyConverter(arg)) : ((Expression)Expression.Constant(new LazyCode<Func<MutableTuple, object>>(bodyConverter(arg), shouldInterpret, compilationThreshold), typeof(object))))), new DelayedTupleAssign(new DelayedTupleExpression(delayedTupleExpression.Index, new StrongBox<ParameterExpression>(parameterExpression3), _tupleType, typeof(PythonGenerator)), parameterExpression4), parameterExpression4);
	}

	private YieldMarker GetYieldMarker(YieldExpression node)
	{
		YieldMarker yieldMarker = new YieldMarker(_yields.Count + 1);
		_yields.Add(yieldMarker);
		return yieldMarker;
	}

	private Expression ToTemp(ref Expression e)
	{
		DelayedTupleExpression delayedTupleExpression = LiftVariable(Expression.Variable(e.Type, "generatorTemp" + _temps.Count));
		Expression result = MakeAssign(delayedTupleExpression, e);
		e = delayedTupleExpression;
		return result;
	}

	private Expression MakeAssign(Expression variable, Expression value)
	{
		return value.NodeType switch
		{
			ExpressionType.Block => MakeAssignBlock(variable, value), 
			ExpressionType.Conditional => MakeAssignConditional(variable, value), 
			ExpressionType.Label => MakeAssignLabel(variable, (LabelExpression)value), 
			_ => DelayedAssign(variable, value), 
		};
	}

	private Expression MakeAssignLabel(Expression variable, LabelExpression value)
	{
		GotoRewriteInfo gotoRewriteInfo = new GotoRewriteInfo(variable, Expression.Label(value.Target.Name + "_voided"));
		Expression defaultValue = new GotoRewriter(this, gotoRewriteInfo, value.Target).Visit(value.DefaultValue);
		return MakeAssignLabel(variable, gotoRewriteInfo, value.Target, defaultValue);
	}

	private Expression MakeAssignLabel(Expression variable, GotoRewriteInfo curVariable, LabelTarget target, Expression defaultValue)
	{
		return Expression.Label(curVariable.VoidTarget, MakeAssign(variable, defaultValue));
	}

	private Expression MakeAssignBlock(Expression variable, Expression value)
	{
		BlockExpression blockExpression = (BlockExpression)value;
		ReadOnlyCollectionBuilder<Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<Expression>(blockExpression.Expressions);
		Expression expression = readOnlyCollectionBuilder[readOnlyCollectionBuilder.Count - 1];
		if (expression.NodeType == ExpressionType.Label)
		{
			LabelExpression labelExpression = (LabelExpression)expression;
			GotoRewriteInfo gotoRewriteInfo = new GotoRewriteInfo(variable, Expression.Label(labelExpression.Target.Name + "_voided"));
			GotoRewriter gotoRewriter = new GotoRewriter(this, gotoRewriteInfo, labelExpression.Target);
			for (int i = 0; i < readOnlyCollectionBuilder.Count - 1; i++)
			{
				readOnlyCollectionBuilder[i] = gotoRewriter.Visit(readOnlyCollectionBuilder[i]);
			}
			readOnlyCollectionBuilder[readOnlyCollectionBuilder.Count - 1] = MakeAssignLabel(variable, gotoRewriteInfo, labelExpression.Target, gotoRewriter.Visit(labelExpression.DefaultValue));
		}
		else
		{
			readOnlyCollectionBuilder[readOnlyCollectionBuilder.Count - 1] = MakeAssign(variable, readOnlyCollectionBuilder[readOnlyCollectionBuilder.Count - 1]);
		}
		return Expression.Block(blockExpression.Variables, readOnlyCollectionBuilder);
	}

	private Expression MakeAssignConditional(Expression variable, Expression value)
	{
		ConditionalExpression conditionalExpression = (ConditionalExpression)value;
		return Expression.Condition(conditionalExpression.Test, MakeAssign(variable, conditionalExpression.IfTrue), MakeAssign(variable, conditionalExpression.IfFalse));
	}

	private BlockExpression ToTemp(ref ReadOnlyCollection<Expression> args)
	{
		int count = args.Count;
		Expression[] array = new Expression[count];
		Expression[] array2 = new Expression[count];
		args.CopyTo(array2, 0);
		for (int i = 0; i < count; i++)
		{
			array[i] = ToTemp(ref array2[i]);
		}
		args = new ReadOnlyCollection<Expression>(array2);
		return Expression.Block(array);
	}

	protected override Expression VisitTry(TryExpression node)
	{
		int count = _yields.Count;
		bool inTryWithFinally = _inTryWithFinally;
		if (node.Finally != null || node.Fault != null)
		{
			_inTryWithFinally = true;
		}
		Expression expression = Visit(node.Body);
		int count2 = _yields.Count;
		IList<CatchBlock> list = ExpressionVisitor.Visit(node.Handlers, VisitCatchBlock);
		int count3 = _yields.Count;
		_returnLabels.Push(Expression.Label("tryLabel"));
		Expression expression2 = Visit(node.Finally);
		Expression expression3 = Visit(node.Fault);
		LabelTarget target = _returnLabels.Pop();
		int count4 = _yields.Count;
		_inTryWithFinally = inTryWithFinally;
		if (expression == node.Body && list == node.Handlers && expression2 == node.Finally && expression3 == node.Fault)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.MakeTry(null, expression, expression2, expression3, list);
		}
		if (expression3 != null && count4 != count3)
		{
			throw new NotSupportedException("yield in fault block is not supported");
		}
		LabelTarget labelTarget = Expression.Label("tryStart");
		if (count2 != count)
		{
			expression = Expression.Block(MakeYieldRouter(node.Body.Type, count, count2, labelTarget), expression);
		}
		if (count3 != count2)
		{
			List<Expression> list2 = new List<Expression>();
			list2.Add(MakeYieldRouter(node.Body.Type, count2, count3, labelTarget));
			list2.Add(null);
			int i = 0;
			for (int count5 = list.Count; i < count5; i++)
			{
				CatchBlock catchBlock = list[i];
				if (catchBlock != node.Handlers[i])
				{
					if (list.IsReadOnly)
					{
						list = ArrayUtils.ToArray(list);
					}
					ParameterExpression parameterExpression = Expression.Variable(catchBlock.Test, null);
					ParameterExpression parameterExpression2 = catchBlock.Variable ?? Expression.Variable(catchBlock.Test, null);
					LiftVariable(parameterExpression2);
					Expression expression4 = catchBlock.Filter;
					if (expression4 != null && catchBlock.Variable != null)
					{
						expression4 = Expression.Block(new ParameterExpression[1] { catchBlock.Variable }, Expression.Assign(catchBlock.Variable, parameterExpression), expression4);
					}
					list[i] = Expression.Catch(parameterExpression, Expression.Block(DelayedAssign(Visit(parameterExpression2), parameterExpression), Expression.Default(node.Body.Type)), expression4);
					RethrowRewriter rethrowRewriter = new RethrowRewriter();
					rethrowRewriter.Exception = parameterExpression2;
					Expression ifTrue = rethrowRewriter.Visit(catchBlock.Body);
					list2.Add(Expression.Condition(Expression.NotEqual(Visit(parameterExpression2), Utils.Constant(null, parameterExpression2.Type)), ifTrue, Expression.Default(node.Body.Type)));
				}
			}
			list2[1] = Expression.MakeTry(null, expression, null, null, new ReadOnlyCollection<CatchBlock>(list));
			expression = Expression.Block(list2);
			list = new CatchBlock[0];
		}
		if (count4 != count3)
		{
			if (list.Count > 0)
			{
				expression = Expression.MakeTry(null, expression, null, null, list);
				list = new CatchBlock[0];
			}
			LabelTarget labelTarget2 = Expression.Label("tryEnd");
			Expression arg = MakeYieldRouter(node.Body.Type, count3, count4, labelTarget2);
			Expression arg2 = MakeYieldRouter(node.Body.Type, count3, count4, labelTarget);
			ParameterExpression parameterExpression3 = Expression.Variable(typeof(Exception), "e");
			ParameterExpression parameterExpression4 = Expression.Variable(typeof(Exception), "$saved$" + _temps.Count);
			LiftVariable(parameterExpression4);
			expression = Expression.Block(Expression.TryCatchFinally(Expression.Block(arg2, expression, DelayedAssign(Visit(parameterExpression4), Utils.Constant(null, parameterExpression4.Type)), Expression.Label(labelTarget2)), Expression.Block(MakeSkipFinallyBlock(target), arg, expression2, Expression.Condition(Expression.NotEqual(Visit(parameterExpression4), Utils.Constant(null, parameterExpression4.Type)), Expression.Throw(Visit(parameterExpression4)), Utils.Empty()), Expression.Label(target)), Expression.Catch(parameterExpression3, Utils.Void(DelayedAssign(Visit(parameterExpression4), parameterExpression3)))), Expression.Condition(Expression.Equal(_gotoRouter, Utils.Constant(0)), Expression.Goto(_returnLabels.Peek()), Utils.Empty()));
			expression2 = null;
		}
		else if (expression2 != null)
		{
			expression2 = Expression.Block(MakeSkipFinallyBlock(target), expression2, Expression.Label(target));
		}
		if (list.Count > 0 || expression2 != null || expression3 != null)
		{
			expression = Expression.MakeTry(null, expression, expression2, expression3, list);
		}
		return Expression.Block(Expression.Label(labelTarget), expression);
	}

	private Expression MakeSkipFinallyBlock(LabelTarget target)
	{
		return Expression.Condition(Expression.AndAlso(Expression.Equal(_gotoRouter, Utils.Constant(0)), Expression.NotEqual(_state, Utils.Constant(0))), Expression.Goto(target), Utils.Empty());
	}

	protected override CatchBlock VisitCatchBlock(CatchBlock node)
	{
		if (node.Variable != null)
		{
			LiftVariable(node.Variable);
		}
		Expression expression = Visit(node.Variable);
		int count = _yields.Count;
		Expression expression2 = Visit(node.Filter);
		if (count != _yields.Count)
		{
			throw new NotSupportedException("yield in filter is not allowed");
		}
		Expression expression3 = Visit(node.Body);
		if (expression == node.Variable && expression3 == node.Body && expression2 == node.Filter)
		{
			return node;
		}
		if (expression != node.Variable && count == _yields.Count)
		{
			return Expression.MakeCatchBlock(node.Test, node.Variable, Expression.Block(new DelayedTupleAssign(expression, node.Variable), expression3), expression2);
		}
		return Expression.MakeCatchBlock(node.Test, node.Variable, expression3, expression2);
	}

	private SwitchExpression MakeYieldRouter(Type type, int start, int end, LabelTarget newTarget)
	{
		SwitchCase[] array = new SwitchCase[end - start];
		for (int i = start; i < end; i++)
		{
			YieldMarker yieldMarker = _yields[i];
			array[i - start] = Expression.SwitchCase(Expression.Goto(yieldMarker.Label, type), Utils.Constant(yieldMarker.State));
			yieldMarker.Label = newTarget;
		}
		return Expression.Switch(_gotoRouter, Expression.Default(type), array);
	}

	protected override Expression VisitExtension(Expression node)
	{
		if (node is YieldExpression node2)
		{
			return VisitYield(node2);
		}
		if (node is FinallyFlowControlExpression)
		{
			return Visit(node.ReduceExtensions());
		}
		return Visit(node.ReduceExtensions());
	}

	private Expression VisitYield(YieldExpression node)
	{
		Expression expression = Visit(node.Value);
		List<Expression> list = new List<Expression>();
		if (expression == null)
		{
			list.Add(MakeAssign(_state, Utils.Constant(0)));
			if (_inTryWithFinally)
			{
				list.Add(Expression.Assign(_gotoRouter, Utils.Constant(0)));
			}
			list.Add(Expression.Goto(_returnLabels.Peek()));
			return Expression.Block(list);
		}
		list.Add(MakeAssign(_current, expression));
		YieldMarker yieldMarker = GetYieldMarker(node);
		list.Add(MakeAssign(_state, Utils.Constant(yieldMarker.State)));
		if (_inTryWithFinally)
		{
			list.Add(Expression.Assign(_gotoRouter, Utils.Constant(0)));
		}
		list.Add(Expression.Goto(_returnLabels.Peek()));
		list.Add(Expression.Label(yieldMarker.Label));
		list.Add(Expression.Assign(_gotoRouter, Utils.Constant(-1)));
		list.Add(Utils.Empty());
		return Expression.Block(list);
	}

	protected override Expression VisitBlock(BlockExpression node)
	{
		foreach (ParameterExpression variable in node.Variables)
		{
			LiftVariable(variable);
		}
		int count = _yields.Count;
		ReadOnlyCollection<Expression> readOnlyCollection = Visit(node.Expressions);
		if (readOnlyCollection == node.Expressions)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.Block(node.Type, node.Variables, readOnlyCollection);
		}
		return Expression.Block(node.Type, readOnlyCollection);
	}

	private DelayedTupleExpression LiftVariable(ParameterExpression param)
	{
		if (!_vars.TryGetValue(param, out var value))
		{
			value = (_vars[param] = new DelayedTupleExpression(_vars.Count, _tupleExpr, _tupleType, param.Type));
			_orderedVars.Add(new KeyValuePair<ParameterExpression, DelayedTupleExpression>(param, value));
		}
		return value;
	}

	protected override Expression VisitParameter(ParameterExpression node)
	{
		return _vars[node];
	}

	protected override Expression VisitLambda<T>(Expression<T> node)
	{
		return node;
	}

	private Expression VisitAssign(BinaryExpression node)
	{
		int count = _yields.Count;
		Expression expression = Visit(node.Left);
		Expression e = Visit(node.Right);
		if (expression == node.Left && e == node.Right)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			if (expression is DelayedTupleExpression)
			{
				return new DelayedTupleAssign(expression, e);
			}
			return Expression.Assign(expression, e);
		}
		List<Expression> list = new List<Expression>();
		if (expression == node.Left)
		{
			switch (expression.NodeType)
			{
			case ExpressionType.MemberAccess:
			{
				MemberExpression memberExpression = (MemberExpression)node.Left;
				Expression e3 = Visit(memberExpression.Expression);
				list.Add(ToTemp(ref e3));
				expression = Expression.MakeMemberAccess(e3, memberExpression.Member);
				break;
			}
			case ExpressionType.Index:
			{
				IndexExpression indexExpression = (IndexExpression)node.Left;
				Expression e2 = Visit(indexExpression.Object);
				ReadOnlyCollection<Expression> args = Visit(indexExpression.Arguments);
				if (e2 == indexExpression.Object && args == indexExpression.Arguments)
				{
					return indexExpression;
				}
				list.Add(ToTemp(ref e2));
				list.Add(ToTemp(ref args));
				expression = Expression.MakeIndex(e2, indexExpression.Indexer, args);
				break;
			}
			default:
				throw Assert.Unreachable;
			case ExpressionType.Parameter:
				break;
			}
		}
		else if (expression is BlockExpression)
		{
			BlockExpression blockExpression = (BlockExpression)expression;
			expression = blockExpression.Expressions[blockExpression.Expressions.Count - 1];
			list.AddRange(blockExpression.Expressions);
			list.RemoveAt(list.Count - 1);
		}
		if (e != node.Right)
		{
			list.Add(ToTemp(ref e));
		}
		if (expression is DelayedTupleExpression)
		{
			list.Add(DelayedAssign(expression, e));
		}
		else
		{
			list.Add(Expression.Assign(expression, e));
		}
		return Expression.Block(list);
	}

	protected override Expression VisitDynamic(DynamicExpression node)
	{
		int count = _yields.Count;
		ReadOnlyCollection<Expression> args = Visit(node.Arguments);
		if (args == node.Arguments)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.MakeDynamic(node.DelegateType, node.Binder, args);
		}
		return Expression.Block(ToTemp(ref args), Expression.MakeDynamic(node.DelegateType, node.Binder, args));
	}

	protected override Expression VisitIndex(IndexExpression node)
	{
		int count = _yields.Count;
		Expression e = Visit(node.Object);
		ReadOnlyCollection<Expression> args = Visit(node.Arguments);
		if (e == node.Object && args == node.Arguments)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.MakeIndex(e, node.Indexer, args);
		}
		return Expression.Block(ToTemp(ref e), ToTemp(ref args), Expression.MakeIndex(e, node.Indexer, args));
	}

	protected override Expression VisitInvocation(InvocationExpression node)
	{
		int count = _yields.Count;
		Expression e = Visit(node.Expression);
		ReadOnlyCollection<Expression> args = Visit(node.Arguments);
		if (e == node.Expression && args == node.Arguments)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.Invoke(e, args);
		}
		return Expression.Block(ToTemp(ref e), ToTemp(ref args), Expression.Invoke(e, args));
	}

	protected override Expression VisitMethodCall(MethodCallExpression node)
	{
		int count = _yields.Count;
		Expression e = Visit(node.Object);
		ReadOnlyCollection<Expression> args = Visit(node.Arguments);
		if (e == node.Object && args == node.Arguments)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.Call(e, node.Method, args);
		}
		if (e == null)
		{
			return Expression.Block(ToTemp(ref args), Expression.Call(null, node.Method, args));
		}
		return Expression.Block(ToTemp(ref e), ToTemp(ref args), Expression.Call(e, node.Method, args));
	}

	protected override Expression VisitNew(NewExpression node)
	{
		int count = _yields.Count;
		ReadOnlyCollection<Expression> args = Visit(node.Arguments);
		if (args == node.Arguments)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			if (node.Members == null)
			{
				return Expression.New(node.Constructor, args);
			}
			return Expression.New(node.Constructor, args, node.Members);
		}
		return Expression.Block(ToTemp(ref args), (node.Members != null) ? Expression.New(node.Constructor, args, node.Members) : Expression.New(node.Constructor, args));
	}

	protected override Expression VisitNewArray(NewArrayExpression node)
	{
		int count = _yields.Count;
		ReadOnlyCollection<Expression> args = Visit(node.Expressions);
		if (args == node.Expressions)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			if (node.NodeType != ExpressionType.NewArrayInit)
			{
				return Expression.NewArrayBounds(node.Type.GetElementType(), args);
			}
			return Expression.NewArrayInit(node.Type.GetElementType(), args);
		}
		return Expression.Block(ToTemp(ref args), (node.NodeType == ExpressionType.NewArrayInit) ? Expression.NewArrayInit(node.Type.GetElementType(), args) : Expression.NewArrayBounds(node.Type.GetElementType(), args));
	}

	protected override Expression VisitMember(MemberExpression node)
	{
		int count = _yields.Count;
		Expression e = Visit(node.Expression);
		if (e == node.Expression)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.MakeMemberAccess(e, node.Member);
		}
		return Expression.Block(ToTemp(ref e), Expression.MakeMemberAccess(e, node.Member));
	}

	protected override Expression VisitBinary(BinaryExpression node)
	{
		if (node.NodeType == ExpressionType.Assign)
		{
			return VisitAssign(node);
		}
		if (node.CanReduce)
		{
			return Visit(node.Reduce());
		}
		int count = _yields.Count;
		Expression e = Visit(node.Left);
		Expression e2 = Visit(node.Right);
		if (e == node.Left && e2 == node.Right)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			return Expression.MakeBinary(node.NodeType, e, e2, node.IsLiftedToNull, node.Method, node.Conversion);
		}
		return Expression.Block(ToTemp(ref e), ToTemp(ref e2), Expression.MakeBinary(node.NodeType, e, e2, node.IsLiftedToNull, node.Method, node.Conversion));
	}

	protected override Expression VisitTypeBinary(TypeBinaryExpression node)
	{
		int count = _yields.Count;
		Expression e = Visit(node.Expression);
		if (e == node.Expression)
		{
			return node;
		}
		if (count == _yields.Count)
		{
			if (node.NodeType != ExpressionType.TypeIs)
			{
				return Expression.TypeEqual(e, node.TypeOperand);
			}
			return Expression.TypeIs(e, node.TypeOperand);
		}
		return Expression.Block(ToTemp(ref e), (node.NodeType == ExpressionType.TypeIs) ? Expression.TypeIs(e, node.TypeOperand) : Expression.TypeEqual(e, node.TypeOperand));
	}

	protected override Expression VisitUnary(UnaryExpression node)
	{
		if (node.CanReduce)
		{
			return Visit(node.Reduce());
		}
		int count = _yields.Count;
		Expression e = Visit(node.Operand);
		if (e == node.Operand)
		{
			return node;
		}
		if (count == _yields.Count || (node.NodeType == ExpressionType.Convert && node.Type == typeof(void)))
		{
			return Expression.MakeUnary(node.NodeType, e, node.Type, node.Method);
		}
		return Expression.Block(ToTemp(ref e), Expression.MakeUnary(node.NodeType, e, node.Type, node.Method));
	}

	protected override Expression VisitMemberInit(MemberInitExpression node)
	{
		int count = _yields.Count;
		Expression expression = base.VisitMemberInit(node);
		if (count == _yields.Count)
		{
			return expression;
		}
		return expression.Reduce();
	}

	protected override Expression VisitListInit(ListInitExpression node)
	{
		int count = _yields.Count;
		Expression expression = base.VisitListInit(node);
		if (count == _yields.Count)
		{
			return expression;
		}
		return expression.Reduce();
	}

	private static Expression DelayedAssign(Expression lhs, Expression rhs)
	{
		return new DelayedTupleAssign(lhs, rhs);
	}
}
