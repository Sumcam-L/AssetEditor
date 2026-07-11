using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

internal class CollectableCompilationMode : CompilationMode
{
	public override Type DelegateType => typeof(Expression<Func<FunctionCode, object>>);

	public override LightLambdaExpression ReduceAst(PythonAst instance, string name)
	{
		return Utils.LightLambda<Func<FunctionCode, object>>(typeof(object), System.Linq.Expressions.Expression.Block(new ParameterExpression[2]
		{
			PythonAst._globalArray,
			PythonAst._globalContext
		}, System.Linq.Expressions.Expression.Assign(PythonAst._globalArray, instance.GlobalArrayInstance), System.Linq.Expressions.Expression.Assign(PythonAst._globalContext, System.Linq.Expressions.Expression.Constant(instance.ModuleContext.GlobalContext)), Utils.Convert(instance.ReduceWorker(), typeof(object))), name, new ParameterExpression[1] { PythonAst._functionCode });
	}

	public override void PrepareScope(PythonAst ast, ReadOnlyCollectionBuilder<ParameterExpression> locals, List<System.Linq.Expressions.Expression> init)
	{
		locals.Add(PythonAst._globalArray);
		init.Add(System.Linq.Expressions.Expression.Assign(PythonAst._globalArray, ast._arrayExpression));
	}

	public override System.Linq.Expressions.Expression GetGlobal(System.Linq.Expressions.Expression globalContext, int arrayIndex, PythonVariable variable, PythonGlobal global)
	{
		return new PythonGlobalVariableExpression(System.Linq.Expressions.Expression.ArrayIndex(PythonAst._globalArray, System.Linq.Expressions.Expression.Constant(arrayIndex)), variable, global);
	}
}
