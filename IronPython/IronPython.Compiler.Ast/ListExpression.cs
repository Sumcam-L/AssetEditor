using System.Linq.Expressions;

namespace IronPython.Compiler.Ast;

public class ListExpression : SequenceExpression
{
	public ListExpression(params Expression[] items)
		: base(items)
	{
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (base.Items.Count == 0)
		{
			return System.Linq.Expressions.Expression.Call(AstMethods.MakeEmptyListFromCode, Node.EmptyExpression);
		}
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeListNoCopy, System.Linq.Expressions.Expression.NewArrayInit(typeof(object), Node.ToObjectArray(base.Items)));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && base.Items != null)
		{
			foreach (Expression item in base.Items)
			{
				item.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
