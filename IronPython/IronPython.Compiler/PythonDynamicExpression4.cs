using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal sealed class PythonDynamicExpression4 : LightDynamicExpression4
{
	private readonly CompilationMode _mode;

	public PythonDynamicExpression4(CallSiteBinder binder, CompilationMode mode, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
		: base(binder, arg0, arg1, arg2, arg3)
	{
		_mode = mode;
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, base.Argument0, base.Argument1, base.Argument2, base.Argument3);
	}

	protected override Expression Rewrite(CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
	{
		return new PythonDynamicExpression4(binder, _mode, arg0, arg1, arg2, arg3);
	}

	public override void AddInstructions(LightCompiler compiler)
	{
		if (base.Argument0.Type == typeof(CodeContext))
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Compile(base.Argument2);
			compiler.Compile(base.Argument3);
			compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object>(base.Binder);
		}
		else
		{
			base.AddInstructions(compiler);
		}
	}
}
