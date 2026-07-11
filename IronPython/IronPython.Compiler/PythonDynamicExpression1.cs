using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal sealed class PythonDynamicExpression1 : LightDynamicExpression1
{
	private readonly CompilationMode _mode;

	public PythonDynamicExpression1(CallSiteBinder binder, CompilationMode mode, Expression arg0)
		: base(binder, arg0)
	{
		_mode = mode;
	}

	protected override Expression Rewrite(CallSiteBinder binder, Expression arg0)
	{
		return new PythonDynamicExpression1(binder, _mode, arg0);
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, base.Argument0);
	}
}
internal sealed class PythonDynamicExpression1<T> : LightDynamicExpression1
{
	private readonly CompilationMode _mode;

	public override Type Type => typeof(T);

	public PythonDynamicExpression1(CallSiteBinder binder, CompilationMode mode, Expression arg0)
		: base(binder, arg0)
	{
		_mode = mode;
	}

	protected override Expression Rewrite(CallSiteBinder binder, Expression arg0)
	{
		return new PythonDynamicExpression1<T>(binder, _mode, arg0);
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, base.Argument0);
	}

	public override void AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(base.Argument0);
		compiler.Instructions.EmitDynamic<object, T>(base.Binder);
	}
}
