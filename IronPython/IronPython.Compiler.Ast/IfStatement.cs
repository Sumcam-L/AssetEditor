using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

public class IfStatement : Statement, IInstructionProvider
{
	private readonly IfStatementTest[] _tests;

	private readonly Statement _else;

	public IList<IfStatementTest> Tests => _tests;

	public Statement ElseStatement => _else;

	public IfStatement(IfStatementTest[] tests, Statement else_)
	{
		_tests = tests;
		_else = else_;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return ReduceWorker(optimizeDynamicConvert: true);
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(ReduceWorker(optimizeDynamicConvert: false));
	}

	private System.Linq.Expressions.Expression ReduceWorker(bool optimizeDynamicConvert)
	{
		System.Linq.Expressions.Expression expression;
		if (_tests.Length > 100)
		{
			BlockBuilder blockBuilder = new BlockBuilder();
			LabelTarget target = System.Linq.Expressions.Expression.Label();
			for (int i = 0; i < _tests.Length; i++)
			{
				IfStatementTest ifStatementTest = _tests[i];
				blockBuilder.Add(System.Linq.Expressions.Expression.Condition(optimizeDynamicConvert ? TransformAndDynamicConvert(ifStatementTest.Test, typeof(bool)) : base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, ifStatementTest.Test), System.Linq.Expressions.Expression.Block(Node.TransformMaybeSingleLineSuite(ifStatementTest.Body, base.GlobalParent.IndexToLocation(ifStatementTest.Test.StartIndex)), System.Linq.Expressions.Expression.Goto(target)), Utils.Empty()));
			}
			if (_else != null)
			{
				blockBuilder.Add(_else);
			}
			blockBuilder.Add(System.Linq.Expressions.Expression.Label(target));
			expression = blockBuilder.ToExpression();
		}
		else
		{
			expression = ((_else == null) ? ((System.Linq.Expressions.Expression)Utils.Empty()) : ((System.Linq.Expressions.Expression)_else));
			int num = _tests.Length;
			while (num-- > 0)
			{
				IfStatementTest ifStatementTest2 = _tests[num];
				expression = base.GlobalParent.AddDebugInfoAndVoid(System.Linq.Expressions.Expression.Condition(optimizeDynamicConvert ? TransformAndDynamicConvert(ifStatementTest2.Test, typeof(bool)) : base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, ifStatementTest2.Test), Node.TransformMaybeSingleLineSuite(ifStatementTest2.Body, base.GlobalParent.IndexToLocation(ifStatementTest2.Test.StartIndex)), expression), new SourceSpan(base.GlobalParent.IndexToLocation(ifStatementTest2.StartIndex), base.GlobalParent.IndexToLocation(ifStatementTest2.HeaderIndex)));
			}
		}
		return expression;
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_tests != null)
			{
				IfStatementTest[] tests = _tests;
				foreach (IfStatementTest ifStatementTest in tests)
				{
					ifStatementTest.Walk(walker);
				}
			}
			if (_else != null)
			{
				_else.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
