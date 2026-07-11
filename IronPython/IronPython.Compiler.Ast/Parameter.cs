using System.Collections.Generic;
using System.Linq.Expressions;
using IronPython.Runtime;

namespace IronPython.Compiler.Ast;

public class Parameter : Node
{
	private readonly string _name;

	protected readonly ParameterKind _kind;

	protected Expression _defaultValue;

	private PythonVariable _variable;

	private ParameterExpression _parameter;

	public string Name => _name;

	public Expression DefaultValue
	{
		get
		{
			return _defaultValue;
		}
		set
		{
			_defaultValue = value;
		}
	}

	public bool IsList => _kind == ParameterKind.List;

	public bool IsDictionary => _kind == ParameterKind.Dictionary;

	internal ParameterKind Kind => _kind;

	internal PythonVariable PythonVariable
	{
		get
		{
			return _variable;
		}
		set
		{
			_variable = value;
		}
	}

	internal ParameterExpression ParameterExpression => _parameter;

	public Parameter(string name)
		: this(name, ParameterKind.Normal)
	{
	}

	public Parameter(string name, ParameterKind kind)
	{
		_name = name;
		_kind = kind;
	}

	internal System.Linq.Expressions.Expression FinishBind(bool needsLocalsDictionary)
	{
		if (_variable.AccessedInNestedScope || needsLocalsDictionary)
		{
			_parameter = System.Linq.Expressions.Expression.Parameter(typeof(object), Name);
			ParameterExpression closureCell = System.Linq.Expressions.Expression.Parameter(typeof(ClosureCell), Name);
			return new ClosureExpression(_variable, closureCell, _parameter);
		}
		return _parameter = System.Linq.Expressions.Expression.Parameter(typeof(object), Name);
	}

	internal virtual void Init(List<System.Linq.Expressions.Expression> init)
	{
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this) && _defaultValue != null)
		{
			_defaultValue.Walk(walker);
		}
		walker.PostWalk(this);
	}
}
