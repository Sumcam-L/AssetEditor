using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime.Binding;

namespace IronPython.Compiler.Ast;

public class SublistParameter : Parameter
{
	private readonly TupleExpression _tuple;

	public TupleExpression Tuple => _tuple;

	public SublistParameter(int position, TupleExpression tuple)
		: base("." + position, ParameterKind.Normal)
	{
		_tuple = tuple;
	}

	internal override void Init(List<System.Linq.Expressions.Expression> init)
	{
		init.Add(_tuple.TransformSet(base.Span, base.ParameterExpression, PythonOperationKind.None));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_tuple != null)
			{
				_tuple.Walk(walker);
			}
			if (_defaultValue != null)
			{
				_defaultValue.Walk(walker);
			}
		}
		walker.PostWalk(this);
	}
}
