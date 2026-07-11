using System;
using System.Collections.Generic;
using System.Threading;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal abstract class RunnableScriptCode : ScriptCode
{
	internal FunctionCode _code;

	private readonly PythonAst _ast;

	public PythonAst Ast => _ast;

	public FunctionCode Code => _code;

	public RunnableScriptCode(PythonAst ast)
		: base(ast.SourceUnit)
	{
		_ast = ast;
	}

	public override object Run()
	{
		return base.Run();
	}

	public override object Run(Scope scope)
	{
		throw new NotImplementedException();
	}

	protected static CodeContext CreateTopLevelCodeContext(PythonDictionary dict, LanguageContext context)
	{
		ModuleContext moduleContext = new ModuleContext(dict, (PythonContext)context);
		return moduleContext.GlobalContext;
	}

	protected static CodeContext GetContextForScope(Scope scope, SourceUnit sourceUnit)
	{
		PythonScopeExtension pythonScopeExtension = scope.GetExtension(sourceUnit.LanguageContext.ContextId) as PythonScopeExtension;
		if (pythonScopeExtension == null)
		{
			pythonScopeExtension = sourceUnit.LanguageContext.EnsureScopeExtension(scope) as PythonScopeExtension;
		}
		return pythonScopeExtension.ModuleContext.GlobalContext;
	}

	protected FunctionCode EnsureFunctionCode(Delegate dlg)
	{
		return EnsureFunctionCode(dlg, tracing: false, register: true);
	}

	protected FunctionCode EnsureFunctionCode(Delegate dlg, bool tracing, bool register)
	{
		if (_code == null)
		{
			Interlocked.CompareExchange(ref _code, new FunctionCode((PythonContext)base.SourceUnit.LanguageContext, dlg, _ast, _ast.GetDocumentation(_ast), tracing, register), null);
		}
		return _code;
	}

	public abstract FunctionCode GetFunctionCode(bool register);

	protected void PushFrame(CodeContext context, FunctionCode code)
	{
		if (((PythonContext)base.SourceUnit.LanguageContext).PythonOptions.Frames)
		{
			PythonOps.PushFrame(context, code);
		}
	}

	protected void PopFrame()
	{
		if (((PythonContext)base.SourceUnit.LanguageContext).PythonOptions.Frames)
		{
			List<FunctionStack> functionStack = PythonOps.GetFunctionStack();
			functionStack.RemoveAt(functionStack.Count - 1);
		}
	}
}
