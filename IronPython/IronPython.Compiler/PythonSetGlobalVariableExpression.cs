using System;
using System.Linq.Expressions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class PythonSetGlobalVariableExpression : Expression, IInstructionProvider
{
	private readonly PythonGlobalVariableExpression _global;

	private readonly Expression _value;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => typeof(object);

	public Expression Value => _value;

	public override bool CanReduce => true;

	public PythonGlobalVariableExpression Global => _global;

	public PythonSetGlobalVariableExpression(PythonGlobalVariableExpression global, Expression value)
	{
		_global = global;
		_value = value;
	}

	public override Expression Reduce()
	{
		return Expression.Assign(Expression.Property(_global.Target, typeof(PythonGlobal).GetProperty("CurrentValue")), Utils.Convert(_value, typeof(object)));
	}

	protected override Expression VisitChildren(ExpressionVisitor visitor)
	{
		Expression expression = visitor.Visit(_value);
		if (expression == _value)
		{
			return this;
		}
		return new PythonSetGlobalVariableExpression(_global, expression);
	}

	public void AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(_value);
		compiler.Instructions.Emit(new PythonSetGlobalInstruction(_global.Global));
	}
}
