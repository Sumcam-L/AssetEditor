using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public sealed class SuiteStatement : Statement
{
	internal class DebugInfoRemovalExpression : System.Linq.Expressions.Expression
	{
		private System.Linq.Expressions.Expression _inner;

		private int _start;

		public override ExpressionType NodeType => ExpressionType.Extension;

		public override Type Type => _inner.Type;

		public override bool CanReduce => true;

		public DebugInfoRemovalExpression(System.Linq.Expressions.Expression expression, int line)
		{
			_inner = expression;
			_start = line;
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return Node.RemoveDebugInfo(_start, _inner.Reduce());
		}
	}

	private readonly Statement[] _statements;

	public IList<Statement> Statements => _statements;

	public override string Documentation
	{
		get
		{
			if (_statements.Length > 0)
			{
				return _statements[0].Documentation;
			}
			return null;
		}
	}

	internal override bool CanThrow
	{
		get
		{
			Statement[] statements = _statements;
			foreach (Statement statement in statements)
			{
				if (statement.CanThrow)
				{
					return true;
				}
			}
			return false;
		}
	}

	public SuiteStatement(Statement[] statements)
	{
		_statements = statements;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (_statements.Length == 0)
		{
			return base.GlobalParent.AddDebugInfoAndVoid(Utils.Empty(), base.Span);
		}
		ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
		int num = -1;
		Statement[] statements = _statements;
		foreach (Statement statement in statements)
		{
			int line = base.GlobalParent.IndexToLocation(statement.StartIndex).Line;
			if (line == num)
			{
				readOnlyCollectionBuilder.Add(new DebugInfoRemovalExpression(statement, num));
			}
			else
			{
				if (statement.CanThrow && line != -1)
				{
					readOnlyCollectionBuilder.Add(Node.UpdateLineNumber(line));
				}
				readOnlyCollectionBuilder.Add(statement);
			}
			num = line;
		}
		return System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder.ToReadOnlyCollection());
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _statements != null)
		{
			Statement[] statements = _statements;
			foreach (Statement statement in statements)
			{
				statement.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
