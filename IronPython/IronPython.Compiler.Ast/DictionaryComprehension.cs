using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

public sealed class DictionaryComprehension : Comprehension
{
	private readonly ComprehensionIterator[] _iterators;

	private readonly Expression _key;

	private readonly Expression _value;

	private readonly ComprehensionScope _scope;

	public Expression Key => _key;

	public Expression Value => _value;

	public override IList<ComprehensionIterator> Iterators => _iterators;

	public override string NodeName => "dict comprehension";

	internal ComprehensionScope Scope => _scope;

	public DictionaryComprehension(Expression key, Expression value, ComprehensionIterator[] iterators)
	{
		_key = key;
		_value = value;
		_iterators = iterators;
		_scope = new ComprehensionScope(this);
	}

	protected override ParameterExpression MakeParameter()
	{
		return System.Linq.Expressions.Expression.Parameter(typeof(PythonDictionary), "dict_comprehension_dict");
	}

	protected override MethodInfo Factory()
	{
		return AstMethods.MakeEmptyDict;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _scope.AddVariables(base.Reduce());
	}

	protected override System.Linq.Expressions.Expression Body(ParameterExpression res)
	{
		return base.GlobalParent.AddDebugInfo(System.Linq.Expressions.Expression.Call(AstMethods.DictAddForComprehension, res, Utils.Convert(_key, typeof(object)), Utils.Convert(_value, typeof(object))), new SourceSpan(_key.Span.Start, _value.Span.End));
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			if (_key != null)
			{
				_key.Walk(walker);
			}
			if (_value != null)
			{
				_value.Walk(walker);
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
