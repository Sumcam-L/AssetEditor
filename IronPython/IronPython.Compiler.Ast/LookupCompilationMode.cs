using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

internal class LookupCompilationMode : CompilationMode
{
	public override ScriptCode MakeScriptCode(PythonAst ast)
	{
		return new PythonScriptCode(ast);
	}

	public override LightLambdaExpression ReduceAst(PythonAst instance, string name)
	{
		return Utils.LightLambda<LookupCompilationDelegate>(typeof(object), Utils.Convert(instance.ReduceWorker(), typeof(object)), name, PythonAst._arrayFuncParams);
	}

	public override System.Linq.Expressions.Expression GetGlobal(System.Linq.Expressions.Expression globalContext, int arrayIndex, PythonVariable variable, PythonGlobal global)
	{
		return new LookupGlobalVariable(globalContext, variable.Name, variable.Kind == VariableKind.Local);
	}
}
