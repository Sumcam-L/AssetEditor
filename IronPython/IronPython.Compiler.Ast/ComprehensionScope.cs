using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

internal class ComprehensionScope : ScopeStatement
{
	private readonly Expression _comprehension;

	internal static readonly ParameterExpression _compContext = System.Linq.Expressions.Expression.Parameter(typeof(CodeContext), "$compContext");

	internal override System.Linq.Expressions.Expression LocalContext
	{
		get
		{
			if (base.NeedsLocalContext)
			{
				return _compContext;
			}
			return _comprehension.Parent.LocalContext;
		}
	}

	public ComprehensionScope(Expression comprehension)
	{
		_comprehension = comprehension;
	}

	internal override bool ExposesLocalVariable(PythonVariable variable)
	{
		if (base.NeedsLocalsDictionary)
		{
			return true;
		}
		if (variable.Scope == this)
		{
			return false;
		}
		return _comprehension.Parent.ExposesLocalVariable(variable);
	}

	internal override PythonVariable BindReference(PythonNameBinder binder, PythonReference reference)
	{
		if (TryGetVariable(reference.Name, out var variable))
		{
			if (variable.Kind == VariableKind.Global)
			{
				AddReferencedGlobal(reference.Name);
			}
			return variable;
		}
		return _comprehension.Parent.BindReference(binder, reference);
	}

	internal override System.Linq.Expressions.Expression GetVariableExpression(PythonVariable variable)
	{
		if (variable.IsGlobal)
		{
			return base.GlobalParent.ModuleVariables[variable];
		}
		if (_variableMapping.TryGetValue(variable, out var value))
		{
			return value;
		}
		return _comprehension.Parent.GetVariableExpression(variable);
	}

	internal override LightLambdaExpression GetLambda()
	{
		throw new NotImplementedException();
	}

	public override void Walk(PythonWalker walker)
	{
		_comprehension.Walk(walker);
	}

	internal System.Linq.Expressions.Expression AddVariables(System.Linq.Expressions.Expression expression)
	{
		ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>();
		ParameterExpression parameterExpression = null;
		if (base.NeedsLocalContext)
		{
			parameterExpression = _compContext;
			readOnlyCollectionBuilder.Add(_compContext);
		}
		List<System.Linq.Expressions.Expression> list = new List<System.Linq.Expressions.Expression>();
		CreateVariables(readOnlyCollectionBuilder, list);
		if (parameterExpression != null)
		{
			MethodCallExpression right = CreateLocalContext(_comprehension.Parent.LocalContext);
			list.Add(System.Linq.Expressions.Expression.Assign(_compContext, right));
			list.Add(expression);
		}
		else
		{
			list.Add(expression);
		}
		return System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder, list);
	}
}
