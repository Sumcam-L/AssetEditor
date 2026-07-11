using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class SetExpression : Expression
{
	private readonly Expression[] _items;

	public IList<Expression> Items => _items;

	public SetExpression(params Expression[] items)
	{
		ContractUtils.RequiresNotNull(items, "items");
		_items = items;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeSet, System.Linq.Expressions.Expression.NewArrayInit(typeof(object), ArrayUtils.ConvertAll(_items, (Expression x) => Utils.Convert(x, typeof(object)))));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			Expression[] items = _items;
			foreach (Expression expression in items)
			{
				expression.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
