using System;
using System.Linq.Expressions;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

internal class PythonConstantExpression : System.Linq.Expressions.Expression, IInstructionProvider
{
	private readonly CompilationMode _mode;

	private readonly object _value;

	public override bool CanReduce => true;

	public override ExpressionType NodeType => ExpressionType.Extension;

	public override Type Type => _mode.GetConstantType(_value);

	public object Value => _value;

	public CompilationMode Mode => _mode;

	public PythonConstantExpression(CompilationMode mode, object value)
	{
		_mode = mode;
		_value = value;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _mode.GetConstant(_value);
	}

	public void AddInstructions(LightCompiler compiler)
	{
		compiler.Instructions.EmitLoad(_value);
	}
}
