using Microsoft.Scripting;

namespace IronPython.Compiler.Ast;

public class IfStatementTest : Node
{
	private int _headerIndex;

	private readonly Expression _test;

	private Statement _body;

	public SourceLocation Header => base.GlobalParent.IndexToLocation(_headerIndex);

	public int HeaderIndex
	{
		get
		{
			return _headerIndex;
		}
		set
		{
			_headerIndex = value;
		}
	}

	public Expression Test => _test;

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

	public IfStatementTest(Expression test, Statement body)
	{
		_test = test;
		_body = body;
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
		}
		walker.PostWalk(this);
	}
}
