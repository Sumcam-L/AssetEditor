using System;
using System.Linq.Expressions;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Debugging.CompilerServices;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal class PythonScriptCode : RunnableScriptCode
{
	private CodeContext _defaultContext;

	private LookupCompilationDelegate _target;

	private LookupCompilationDelegate _tracingTarget;

	private CodeContext DefaultContext
	{
		get
		{
			if (_defaultContext == null)
			{
				_defaultContext = RunnableScriptCode.CreateTopLevelCodeContext(new PythonDictionary(), base.Ast.CompilerContext.SourceUnit.LanguageContext);
			}
			return _defaultContext;
		}
	}

	public PythonScriptCode(PythonAst ast)
		: base(ast)
	{
	}

	public override object Run()
	{
		if (base.SourceUnit.Kind == SourceCodeKind.Expression)
		{
			return EvalWrapper(DefaultContext);
		}
		return RunWorker(DefaultContext);
	}

	public override object Run(Scope scope)
	{
		CodeContext contextForScope = RunnableScriptCode.GetContextForScope(scope, base.SourceUnit);
		if (base.SourceUnit.Kind == SourceCodeKind.Expression)
		{
			return EvalWrapper(contextForScope);
		}
		return RunWorker(contextForScope);
	}

	private object RunWorker(CodeContext ctx)
	{
		LookupCompilationDelegate target = GetTarget(register: true);
		Exception clrException = PythonOps.SaveCurrentException();
		PushFrame(ctx, _code);
		try
		{
			return target(ctx, _code);
		}
		finally
		{
			PythonOps.RestoreCurrentException(clrException);
			PopFrame();
		}
	}

	private LookupCompilationDelegate GetTarget(bool register)
	{
		PythonContext pythonContext = (PythonContext)base.Ast.CompilerContext.SourceUnit.LanguageContext;
		if (!pythonContext.EnableTracing)
		{
			EnsureTarget(register);
			return _target;
		}
		EnsureTracingTarget();
		return _tracingTarget;
	}

	public override FunctionCode GetFunctionCode(bool register)
	{
		GetTarget(register);
		return _code;
	}

	public override Scope CreateScope()
	{
		return new Scope();
	}

	private object EvalWrapper(CodeContext ctx)
	{
		try
		{
			return RunWorker(ctx);
		}
		catch (Exception e)
		{
			PythonOps.UpdateStackTrace(e, ctx, base.Code, 0);
			throw;
		}
	}

	private LookupCompilationDelegate CompileBody(LightExpression<LookupCompilationDelegate> lambda)
	{
		PythonConstantExpression pythonConstantExpression = ExtractConstant(lambda);
		if (pythonConstantExpression != null)
		{
			object value = pythonConstantExpression.Value;
			return (CodeContext codeCtx, FunctionCode functionCode) => value;
		}
		PythonContext pythonContext = (PythonContext)base.Ast.CompilerContext.SourceUnit.LanguageContext;
		if (ShouldInterpret(pythonContext))
		{
			return lambda.Compile(pythonContext.Options.CompilationThreshold);
		}
		return CompilerHelpers.Compile(lambda.ReduceToLambda(), pythonContext.EmitDebugSymbols(base.Ast.CompilerContext.SourceUnit));
	}

	private bool ShouldInterpret(PythonContext pc)
	{
		return pc.ShouldInterpret((PythonCompilerOptions)base.Ast.CompilerContext.Options, base.Ast.CompilerContext.SourceUnit);
	}

	private static PythonConstantExpression ExtractConstant(LightExpression<LookupCompilationDelegate> lambda)
	{
		if (!(lambda.Body is BlockExpression blockExpression) || blockExpression.Expressions.Count != 2 || !(blockExpression.Expressions[0] is DebugInfoExpression) || blockExpression.Expressions[1].NodeType != ExpressionType.Convert || !(((System.Linq.Expressions.UnaryExpression)blockExpression.Expressions[1]).Operand is PythonConstantExpression))
		{
			return null;
		}
		return (PythonConstantExpression)((System.Linq.Expressions.UnaryExpression)blockExpression.Expressions[1]).Operand;
	}

	private void EnsureTarget(bool register)
	{
		if (_target == null)
		{
			_target = CompileBody((LightExpression<LookupCompilationDelegate>)base.Ast.GetLambda());
			EnsureFunctionCode(_target, tracing: false, register);
		}
	}

	private void EnsureTracingTarget()
	{
		if (_tracingTarget == null)
		{
			PythonContext pythonContext = (PythonContext)base.Ast.CompilerContext.SourceUnit.LanguageContext;
			PythonDebuggingPayload pythonDebuggingPayload = new PythonDebuggingPayload(null);
			DebugLambdaInfo lambdaInfo = new DebugLambdaInfo(null, null, optimizeForLeafFrames: false, null, null, pythonDebuggingPayload);
			Expression<LookupCompilationDelegate> lambda = (Expression<LookupCompilationDelegate>)pythonContext.DebugContext.TransformLambda((System.Linq.Expressions.LambdaExpression)base.Ast.GetLambda().Reduce(), lambdaInfo);
			LookupCompilationDelegate tracingTarget = ((!ShouldInterpret(pythonContext)) ? CompilerHelpers.Compile(lambda, pythonContext.EmitDebugSymbols(base.Ast.CompilerContext.SourceUnit)) : lambda.LightCompile(pythonContext.Options.CompilationThreshold));
			_tracingTarget = tracingTarget;
			pythonDebuggingPayload.Code = EnsureFunctionCode(_tracingTarget, tracing: true, register: true);
		}
	}
}
