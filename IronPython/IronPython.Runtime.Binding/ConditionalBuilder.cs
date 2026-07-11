using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;

namespace IronPython.Runtime.Binding;

internal class ConditionalBuilder
{
	private readonly DynamicMetaObjectBinder _action;

	private readonly List<Expression> _conditions = new List<Expression>();

	private readonly List<Expression> _bodies = new List<Expression>();

	private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

	private Expression _body;

	private BindingRestrictions _restrictions = BindingRestrictions.Empty;

	private ParameterExpression _compareRetBool;

	private Type _retType;

	public ParameterExpression CompareRetBool
	{
		get
		{
			if (_compareRetBool == null)
			{
				_compareRetBool = Expression.Variable(typeof(bool), "compareRetBool");
				AddVariable(_compareRetBool);
			}
			return _compareRetBool;
		}
	}

	public BindingRestrictions Restrictions
	{
		get
		{
			return _restrictions;
		}
		set
		{
			_restrictions = value;
		}
	}

	public DynamicMetaObjectBinder Action => _action;

	public bool NoConditions => _conditions.Count == 0;

	public bool IsFinal => _body != null;

	public ConditionalBuilder(DynamicMetaObjectBinder action)
	{
		_action = action;
	}

	public ConditionalBuilder()
	{
	}

	public void AddCondition(Expression condition, Expression body)
	{
		_conditions.Add(condition);
		_bodies.Add(body);
	}

	public void AddCondition(Expression condition)
	{
		if (_body != null)
		{
			AddCondition(condition, _body);
			_body = null;
		}
		else
		{
			_conditions[_conditions.Count - 1] = Expression.AndAlso(_conditions[_conditions.Count - 1], condition);
		}
	}

	public void FinishCondition(Expression body)
	{
		FinishCondition(body, typeof(object));
	}

	public void FinishCondition(Expression body, Type retType)
	{
		if (_body != null)
		{
			throw new InvalidOperationException();
		}
		_body = body;
		_retType = retType;
	}

	public DynamicMetaObject GetMetaObject(params DynamicMetaObject[] types)
	{
		if (_body == null)
		{
			throw new InvalidOperationException("FinishCondition not called before GetMetaObject");
		}
		Expression expression = _body;
		for (int num = _bodies.Count - 1; num >= 0; num--)
		{
			expression = Expression.Condition(_conditions[num], Utils.Convert(_bodies[num], _retType), Utils.Convert(expression, _retType));
		}
		expression = Expression.Block(_variables, expression);
		return new DynamicMetaObject(expression, BindingRestrictions.Combine(types));
	}

	public void AddVariable(ParameterExpression var)
	{
		if (_body != null)
		{
			throw new InvalidOperationException("Variables must be added before calling FinishCondition");
		}
		_variables.Add(var);
	}
}
