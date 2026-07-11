using System;
using System.Linq.Expressions;
using IronPython.Compiler.Ast;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal class PythonGlobalVariableExpression : System.Linq.Expressions.Expression, IInstructionProvider, IPythonGlobalExpression, IPythonVariableExpression, ILightExceptionAwareExpression
{
	private readonly System.Linq.Expressions.Expression _target;

	private readonly PythonGlobal _global;

	private readonly PythonVariable _variable;

	private readonly bool _lightEh;

	internal static System.Linq.Expressions.Expression Uninitialized = System.Linq.Expressions.Expression.Field(null, typeof(Uninitialized).GetField("Instance"));

	public System.Linq.Expressions.Expression Target => _target;

	public new PythonVariable Variable => _variable;

	public PythonGlobal Global => _global;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => typeof(object);

	public override bool CanReduce => true;

	public PythonGlobalVariableExpression(System.Linq.Expressions.Expression globalExpr, PythonVariable variable, PythonGlobal global)
		: this(globalExpr, variable, global, lightEh: false)
	{
	}

	internal PythonGlobalVariableExpression(System.Linq.Expressions.Expression globalExpr, PythonVariable variable, PythonGlobal global, bool lightEh)
	{
		_target = globalExpr;
		_global = global;
		_variable = variable;
		_lightEh = lightEh;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Property(_target, PythonGlobal.CurrentValueProperty);
	}

	public System.Linq.Expressions.Expression RawValue()
	{
		return new PythonRawGlobalValueExpression(this);
	}

	public System.Linq.Expressions.Expression Assign(System.Linq.Expressions.Expression value)
	{
		return new PythonSetGlobalVariableExpression(this, value);
	}

	public System.Linq.Expressions.Expression Delete()
	{
		return new PythonSetGlobalVariableExpression(this, Uninitialized);
	}

	public System.Linq.Expressions.Expression Create()
	{
		return null;
	}

	protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
	{
		System.Linq.Expressions.Expression expression = visitor.Visit(_target);
		if (expression == _target)
		{
			return this;
		}
		return new PythonGlobalVariableExpression(expression, _variable, _global, _lightEh);
	}

	public void AddInstructions(LightCompiler compiler)
	{
		if (_lightEh)
		{
			compiler.Instructions.Emit(new PythonLightThrowGlobalInstruction(_global));
		}
		else
		{
			compiler.Instructions.Emit(new PythonGlobalInstruction(_global));
		}
	}

	System.Linq.Expressions.Expression ILightExceptionAwareExpression.ReduceForLightExceptions()
	{
		if (_lightEh)
		{
			return this;
		}
		return new PythonGlobalVariableExpression(_target, _variable, _global, lightEh: true);
	}
}
