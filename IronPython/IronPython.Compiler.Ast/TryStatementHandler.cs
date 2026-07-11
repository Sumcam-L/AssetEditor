using Microsoft.Scripting;

namespace IronPython.Compiler.Ast;

public class TryStatementHandler : Node
{
	private int _headerIndex;

	private readonly Expression _test;

	private readonly Expression _target;

	private readonly Statement _body;

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

	public Expression Target => _target;

	public Statement Body => _body;

	public TryStatementHandler(Expression test, Expression target, Statement body)
	{
		_test = test;
		_target = target;
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
			if (_target != null)
			{
				_target.Walk(walker);
			}
			if (_body != null)
			{
				_body.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
