using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public sealed class SetComprehension : Comprehension
{
	private readonly ComprehensionIterator[] _iterators;

	private readonly Expression _item;

	private readonly ComprehensionScope _scope;

	public Expression Item => _item;

	public override IList<ComprehensionIterator> Iterators => _iterators;

	public override string NodeName => "set comprehension";

	internal ComprehensionScope Scope => _scope;

	public SetComprehension(Expression item, ComprehensionIterator[] iterators)
	{
		_item = item;
		_iterators = iterators;
		_scope = new ComprehensionScope(this);
	}

	protected override ParameterExpression MakeParameter()
	{
		return System.Linq.Expressions.Expression.Parameter(typeof(SetCollection), "set_comprehension_set");
	}

	protected override MethodInfo Factory()
	{
		return AstMethods.MakeEmptySet;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _scope.AddVariables(base.Reduce());
	}

	protected override System.Linq.Expressions.Expression Body(ParameterExpression res)
	{
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Call(AstMethods.SetAddForComprehension, res, Utils.Convert(_item, typeof(object))), _item.Span);
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
