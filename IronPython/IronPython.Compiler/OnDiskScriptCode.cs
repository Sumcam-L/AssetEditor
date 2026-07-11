using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal class OnDiskScriptCode : RunnableScriptCode
{
	private readonly LookupCompilationDelegate _target;

	private CodeContext _optimizedContext;

	private readonly string _moduleName;

	public string ModuleName => _moduleName;

	public OnDiskScriptCode(LookupCompilationDelegate code, SourceUnit sourceUnit, string moduleName)
		: base(MakeAstFromSourceUnit(sourceUnit))
	{
		_target = code;
		_moduleName = moduleName;
	}

	private static PythonAst MakeAstFromSourceUnit(SourceUnit sourceUnit)
	{
		CompilerContext context = new CompilerContext(sourceUnit, new PythonCompilerOptions(), ErrorSink.Null);
		return new PythonAst(context);
	}

	public override object Run()
	{
		CodeContext context = CreateContext();
		try
		{
			FunctionCode code = EnsureFunctionCode(_target, tracing: false, register: true);
			PushFrame(context, code);
			return _target(context, code);
		}
		finally
		{
			PopFrame();
		}
	}

	public override object Run(Scope scope)
	{
		if (scope == CreateScope())
		{
			return Run();
		}
		throw new NotSupportedException();
	}

	public override FunctionCode GetFunctionCode(bool register)
	{
		return EnsureFunctionCode(_target, tracing: false, register);
	}

	public override Scope CreateScope()
	{
		return CreateContext().GlobalScope;
	}

	internal CodeContext CreateContext()
	{
		if (_optimizedContext == null)
		{
			CachedOptimizedCodeAttribute[] array = (CachedOptimizedCodeAttribute[])_target.Method.GetCustomAttributes(typeof(CachedOptimizedCodeAttribute), inherit: false);
			CachedOptimizedCodeAttribute cachedOptimizedCodeAttribute = array[0];
			Dictionary<string, PythonGlobal> dictionary = new Dictionary<string, PythonGlobal>(StringComparer.Ordinal);
			PythonGlobal[] array2 = new PythonGlobal[cachedOptimizedCodeAttribute.Names.Length];
			PythonDictionary pythonDictionary = new PythonDictionary(new GlobalDictionaryStorage(dictionary, array2));
			ModuleContext moduleContext = new ModuleContext(pythonDictionary, (PythonContext)base.SourceUnit.LanguageContext);
			CodeContext globalContext = moduleContext.GlobalContext;
			for (int i = 0; i < cachedOptimizedCodeAttribute.Names.Length; i++)
			{
				string text = cachedOptimizedCodeAttribute.Names[i];
				int num = i;
				PythonGlobal pythonGlobal = (dictionary[text] = new PythonGlobal(globalContext, text));
				array2[num] = pythonGlobal;
			}
			_optimizedContext = RunnableScriptCode.CreateTopLevelCodeContext(pythonDictionary, (PythonContext)base.SourceUnit.LanguageContext);
		}
		return _optimizedContext;
	}
}
