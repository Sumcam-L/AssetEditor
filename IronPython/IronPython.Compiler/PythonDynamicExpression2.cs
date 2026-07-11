using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class PythonDynamicExpression2 : LightDynamicExpression2
{
	private readonly CompilationMode _mode;

	public PythonDynamicExpression2(CallSiteBinder binder, CompilationMode mode, Expression arg0, Expression arg1)
		: base(binder, arg0, arg1)
	{
		_mode = mode;
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, base.Argument0, base.Argument1);
	}

	protected override Expression Rewrite(CallSiteBinder binder, Expression arg0, Expression arg1)
	{
		return new PythonDynamicExpression2(binder, _mode, arg0, arg1);
	}

	public override void AddInstructions(LightCompiler compiler)
	{
		if (base.Argument0.Type == typeof(CodeContext))
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Instructions.EmitDynamic<CodeContext, object, object>(base.Binder);
		}
		else if (base.Argument1.Type == typeof(CodeContext))
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Instructions.EmitDynamic<object, CodeContext, object>(base.Binder);
		}
		else
		{
			base.AddInstructions(compiler);
		}
	}
}
internal sealed class PythonDynamicExpression2<T> : PythonDynamicExpression2
{
	private readonly CompilationMode _mode;

	public override Type Type => typeof(T);

	public PythonDynamicExpression2(CallSiteBinder binder, CompilationMode mode, Expression arg0, Expression arg1)
		: base(binder, mode, arg0, arg1)
	{
		_mode = mode;
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, base.Argument0, base.Argument1);
	}

	protected override Expression Rewrite(CallSiteBinder binder, Expression arg0, Expression arg1)
	{
		return new PythonDynamicExpression2<T>(binder, _mode, arg0, arg1);
	}

	public override void AddInstructions(LightCompiler compiler)
	{
		if (base.Argument0.Type == typeof(CodeContext))
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Instructions.EmitDynamic<CodeContext, object, T>(base.Binder);
		}
		else if (base.Argument1.Type == typeof(CodeContext))
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Instructions.EmitDynamic<object, CodeContext, T>(base.Binder);
		}
		else
		{
			compiler.Compile(base.Argument0);
			compiler.Compile(base.Argument1);
			compiler.Instructions.EmitDynamic<object, object, T>(base.Binder);
		}
	}
}
