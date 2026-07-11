using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

internal class ToDiskCompilationMode : CollectableCompilationMode
{
	public override System.Linq.Expressions.Expression GetConstant(object value)
	{
		return Utils.Constant(value);
	}

	public override void PrepareScope(PythonAst ast, ReadOnlyCollectionBuilder<ParameterExpression> locals, List<System.Linq.Expressions.Expression> init)
	{
		locals.Add(PythonAst._globalArray);
		init.Add(System.Linq.Expressions.Expression.Assign(PythonAst._globalArray, System.Linq.Expressions.Expression.Call(typeof(PythonOps).GetMethod("GetGlobalArrayFromContext"), PythonAst._globalContext)));
	}

	public override LightLambdaExpression ReduceAst(PythonAst instance, string name)
	{
		return Utils.LightLambda<LookupCompilationDelegate>(typeof(object), System.Linq.Expressions.Expression.Block(new ParameterExpression[1] { PythonAst._globalArray }, System.Linq.Expressions.Expression.Assign(PythonAst._globalArray, System.Linq.Expressions.Expression.Call(null, typeof(PythonOps).GetMethod("GetGlobalArrayFromContext"), PythonAst._globalContext)), Utils.Convert(instance.ReduceWorker(), typeof(object))), name, PythonAst._arrayFuncParams);
	}

	public override ScriptCode MakeScriptCode(PythonAst ast)
	{
		PythonCompilerOptions pythonCompilerOptions = ast.CompilerContext.Options as PythonCompilerOptions;
		Expression<LookupCompilationDelegate> code = (Expression<LookupCompilationDelegate>)ast.Reduce().Reduce();
		return new PythonSavableScriptCode(code, ast.SourceUnit, ast.GetNames(), pythonCompilerOptions.ModuleName);
	}
}
