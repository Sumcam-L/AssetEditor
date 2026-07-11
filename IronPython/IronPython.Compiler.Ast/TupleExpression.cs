using System.Linq.Expressions;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Compiler.Ast;

public class TupleExpression : SequenceExpression
{
	private bool _expandable;

	public bool IsExpandable => _expandable;

	internal override bool IsConstant
	{
		get
		{
			foreach (Expression item in base.Items)
			{
				if (!item.IsConstant)
				{
					return false;
				}
			}
			return true;
		}
	}

	public TupleExpression(bool expandable, params Expression[] items)
		: base(items)
	{
		_expandable = expandable;
	}

	internal override string CheckAssign()
	{
		if (base.Items.Count == 0)
		{
			return "can't assign to ()";
		}
		for (int i = 0; i < base.Items.Count; i++)
		{
			Expression expression = base.Items[i];
			if (expression.CheckAssign() != null)
			{
				return "can't assign to " + expression.NodeName;
			}
		}
		return null;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (_expandable)
		{
			return System.Linq.Expressions.Expression.NewArrayInit(typeof(object), Node.ToObjectArray(base.Items));
		}
		if (base.Items.Count == 0)
		{
			return System.Linq.Expressions.Expression.Field(null, typeof(PythonOps).GetField("EmptyTuple"));
		}
		return System.Linq.Expressions.Expression.Call(AstMethods.MakeTuple, System.Linq.Expressions.Expression.NewArrayInit(typeof(object), Node.ToObjectArray(base.Items)));
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

	internal override object GetConstantValue()
	{
		if (base.Items.Count == 0)
		{
			return PythonTuple.EMPTY;
		}
		object[] array = new object[base.Items.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = base.Items[i].GetConstantValue();
		}
		return PythonOps.MakeTuple(array);
	}
}
