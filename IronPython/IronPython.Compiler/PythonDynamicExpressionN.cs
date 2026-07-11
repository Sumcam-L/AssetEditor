using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler;

internal sealed class PythonDynamicExpressionN : LightTypedDynamicExpressionN
{
	private readonly CompilationMode _mode;

	public PythonDynamicExpressionN(CallSiteBinder binder, CompilationMode mode, IList<Expression> args)
		: base(binder, typeof(object), args)
	{
		_mode = mode;
	}

	protected override Expression Rewrite(CallSiteBinder binder, IList<Expression> args)
	{
		return new PythonDynamicExpressionN(binder, _mode, args);
	}

	public override Expression Reduce()
	{
		return _mode.ReduceDynamic((DynamicMetaObjectBinder)base.Binder, Type, ArrayUtils.ToArray(base.Arguments));
	}

	public override void AddInstructions(LightCompiler compiler)
	{
		if (ArgumentCount > 15)
		{
			compiler.Compile(Reduce());
		}
		else if (GetArgument(0).Type == typeof(CodeContext))
		{
			for (int i = 0; i < ArgumentCount; i++)
			{
				compiler.Compile(GetArgument(i));
			}
			switch (ArgumentCount)
			{
			case 1:
				compiler.Instructions.EmitDynamic<CodeContext, object>(base.Binder);
				break;
			case 2:
				compiler.Instructions.EmitDynamic<CodeContext, object, object>(base.Binder);
				break;
			case 3:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object>(base.Binder);
				break;
			case 4:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object>(base.Binder);
				break;
			case 5:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object>(base.Binder);
				break;
			case 6:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object>(base.Binder);
				break;
			case 7:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 8:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 9:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 10:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 11:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 12:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 13:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 14:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			case 15:
				compiler.Instructions.EmitDynamic<CodeContext, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>(base.Binder);
				break;
			}
		}
		else
		{
			base.AddInstructions(compiler);
		}
	}
}
