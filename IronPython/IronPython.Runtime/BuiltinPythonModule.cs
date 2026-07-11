using System.Collections.Generic;
using IronPython.Compiler;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public class BuiltinPythonModule
{
	private readonly PythonContext _context;

	private CodeContext _codeContext;

	protected PythonContext Context => _context;

	protected CodeContext Globals => _codeContext;

	protected BuiltinPythonModule(PythonContext context)
	{
		ContractUtils.RequiresNotNull(context, "context");
		_context = context;
	}

	protected internal virtual void Initialize(CodeContext codeContext, Dictionary<string, PythonGlobal> optimizedGlobals)
	{
		ContractUtils.RequiresNotNull(codeContext, "codeContext");
		ContractUtils.RequiresNotNull(optimizedGlobals, "globals");
		_codeContext = codeContext;
		PerformModuleReload();
	}

	protected internal virtual IEnumerable<string> GetGlobalVariableNames()
	{
		return ArrayUtils.EmptyStrings;
	}

	protected internal virtual void PerformModuleReload()
	{
	}
}
