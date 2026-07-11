using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

public class WhileStatement : Statement, ILoopStatement, IInstructionProvider
{
	private int _indexHeader;

	private readonly Expression _test;

	private readonly Statement _body;

	private readonly Statement _else;

	private LabelTarget _break;

	private LabelTarget _continue;

	public Expression Test => _test;

	public Statement Body => _body;

	public Statement ElseStatement => _else;

	private SourceSpan Header => new SourceSpan(base.GlobalParent.IndexToLocation(base.StartIndex), base.GlobalParent.IndexToLocation(_indexHeader));

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

	public WhileStatement(Expression test, Statement body, Statement else_)
	{
		_test = test;
		_body = body;
		_else = else_;
	}

	public void SetLoc(PythonAst globalParent, int start, int header, int end)
	{
		SetLoc(globalParent, start, end);
		_indexHeader = header;
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
		if (_test is ConstantExpression constantExpression && constantExpression.Value is int)
		{
			if ((int)constantExpression.Value == 0)
			{
				if (_else == null)
				{
					return System.Linq.Expressions.Expression.Empty();
				}
				return _else;
			}
			System.Linq.Expressions.Expression test = System.Linq.Expressions.Expression.Constant(true);
			System.Linq.Expressions.Expression expression = Utils.While(test, _body, _else, _break, _continue);
			if (base.GlobalParent.IndexToLocation(_test.StartIndex).Line != base.GlobalParent.IndexToLocation(_body.StartIndex).Line)
			{
				expression = base.GlobalParent.AddDebugInfoAndVoid(expression, _test.Span);
			}
			return expression;
		}
		return Utils.While(base.GlobalParent.AddDebugInfo(optimizeDynamicConvert ? TransformAndDynamicConvert(_test, typeof(bool)) : base.GlobalParent.Convert(typeof(bool), ConversionResultKind.ExplicitCast, _test), Header), _body, _else, _break, _continue);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_test != null)
			{
				_test.Walk(walker);
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
}
