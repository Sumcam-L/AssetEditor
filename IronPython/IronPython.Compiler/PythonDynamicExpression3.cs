using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal sealed class PythonDynamicExpression3 : LightDynamicExpression3
{
	private readonly CompilationMode _mode;

	public PythonDynamicExpression3(CallSiteBinder binder, CompilationMode mode, Expression arg0, Expression arg1, Expression arg2)
		: base(binder, arg0, arg1, arg2)
	{
		_mode = mode;
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, base.Argument0, base.Argument1, base.Argument2);
	}

	protected override Expression Rewrite(CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
	{
		return new PythonDynamicExpression3(binder, _mode, arg0, arg1, arg2);
	}

	public override void AddInstructions(LightCompiler compiler)
	{
		if (base.Argument0.Type == typeof(CodeContext))
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Compile(base.Argument2);
			compiler.Instructions.EmitDynamic<CodeContext, object, object, object>(base.Binder);
		}
		else
		{
			base.AddInstructions(compiler);
		}
	}
}
