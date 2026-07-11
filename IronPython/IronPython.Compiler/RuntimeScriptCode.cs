using System;
using System.Threading;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal class RuntimeScriptCode : RunnableScriptCode
{
	private readonly CodeContext _optimizedContext;

	private Func<FunctionCode, object> _optimizedTarget;

	private ScriptCode _unoptimizedCode;

	public RuntimeScriptCode(PythonAst ast, CodeContext codeContext)
		: base(ast)
	{
		_optimizedContext = codeContext;
	}

	public override object Run()
	{
		return InvokeTarget(CreateScope());
	}

	public override object Run(Scope scope)
	{
		return InvokeTarget(scope);
	}

	public override FunctionCode GetFunctionCode(bool register)
	{
		EnsureCompiled();
		return EnsureFunctionCode(_optimizedTarget, tracing: false, register);
	}

	private object InvokeTarget(Scope scope)
	{
		if (scope == _optimizedContext.GlobalScope && !_optimizedContext.LanguageContext.EnableTracing)
		{
			EnsureCompiled();
			Exception clrException = PythonOps.SaveCurrentException();
			FunctionCode functionCode = EnsureFunctionCode(_optimizedTarget, tracing: false, register: true);
			PushFrame(_optimizedContext, functionCode);
			try
			{
				if (base.Ast.CompilerContext.SourceUnit.Kind == SourceCodeKind.Expression)
				{
					return OptimizedEvalWrapper(functionCode);
				}
				return _optimizedTarget(functionCode);
			}
			finally
			{
				PythonOps.RestoreCurrentException(clrException);
				PopFrame();
			}
		}
		if (_unoptimizedCode == null)
		{
			((PythonCompilerOptions)base.Ast.CompilerContext.Options).Optimized = false;
			Interlocked.CompareExchange(ref _unoptimizedCode, base.Ast.MakeLookupCode().ToScriptCode(), null);
		}
		return _unoptimizedCode.Run(scope);
	}

	private object OptimizedEvalWrapper(FunctionCode funcCode)
	{
		try
		{
			return _optimizedTarget(funcCode);
		}
		catch (Exception e)
		{
			PythonOps.UpdateStackTrace(e, _optimizedContext, base.Code, 0);
			throw;
		}
	}

	public override Scope CreateScope()
	{
		return _optimizedContext.GlobalScope;
	}

	private void EnsureCompiled()
	{
		if (_optimizedTarget == null)
		{
			Interlocked.CompareExchange(ref _optimizedTarget, Compile(), null);
		}
	}

	private Func<FunctionCode, object> Compile()
	{
		PythonCompilerOptions options = (PythonCompilerOptions)base.Ast.CompilerContext.Options;
		PythonContext pythonContext = (PythonContext)base.SourceUnit.LanguageContext;
		if (pythonContext.ShouldInterpret(options, base.SourceUnit))
		{
			return ((LightExpression<Func<FunctionCode, object>>)base.Ast.GetLambda()).Compile(pythonContext.Options.CompilationThreshold);
		}
		return CompilerHelpers.Compile(((LightExpression<Func<FunctionCode, object>>)base.Ast.GetLambda()).ReduceToLambda(), pythonContext.EmitDebugSymbols(base.SourceUnit));
	}
}
