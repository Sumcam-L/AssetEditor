using System.Linq.Expressions;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public class ExecStatement : Statement
{
	private readonly Expression _code;

	private readonly Expression _locals;

	private readonly Expression _globals;

	public Expression Code => _code;

	public Expression Locals => _locals;

	public Expression Globals => _globals;

	public ExecStatement(Expression code, Expression locals, Expression globals)
	{
		_code = code;
		_locals = locals;
		_globals = globals;
	}

	public bool NeedsLocalsDictionary()
	{
		if (_globals == null)
		{
			return _locals == null;
		}
		return false;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		MethodCallExpression expression = ((_locals != null || _globals != null) ? System.Linq.Expressions.Expression.Call(AstMethods.QualifiedExec, base.Parent.LocalContext, Utils.Convert(_code, typeof(object)), TransformAndDynamicConvert(_globals, typeof(PythonDictionary)), Node.TransformOrConstantNull(_locals, typeof(object))) : System.Linq.Expressions.Expression.Call(AstMethods.UnqualifiedExec, base.Parent.LocalContext, Utils.Convert(_code, typeof(object))));
		return base.GlobalParent.AddDebugInfo(expression, base.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_code != null)
			{
				_code.Walk(walker);
			}
			if (_locals != null)
			{
				_locals.Walk(walker);
			}
			if (_globals != null)
			{
				_globals.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
