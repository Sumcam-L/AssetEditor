using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public sealed class ListComprehension : Comprehension
{
	private readonly ComprehensionIterator[] _iterators;

	private readonly Expression _item;

	public Expression Item => _item;

	public override IList<ComprehensionIterator> Iterators => _iterators;

	public override string NodeName => "list comprehension";

	public ListComprehension(Expression item, ComprehensionIterator[] iterators)
	{
		_item = item;
		_iterators = iterators;
	}

	protected override ParameterExpression MakeParameter()
	{
		return System.Linq.Expressions.Expression.Parameter(typeof(List), "list_comprehension_list");
	}

	protected override MethodInfo Factory()
	{
		return AstMethods.MakeList;
	}

	protected override System.Linq.Expressions.Expression Body(ParameterExpression res)
	{
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Call(AstMethods.ListAddForComprehension, res, Utils.Convert(_item, typeof(object))), _item.Span);
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_item != null)
			{
				_item.Walk(walker);
			}
			if (_iterators != null)
			{
				ComprehensionIterator[] iterators = _iterators;
				foreach (ComprehensionIterator comprehensionIterator in iterators)
				{
					comprehensionIterator.Walk(walker);
				}
			}
		}
		walker.PostWalk(this);
	}
}
