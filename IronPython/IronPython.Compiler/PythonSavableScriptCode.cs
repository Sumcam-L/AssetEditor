using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal class PythonSavableScriptCode : SavableScriptCode, ICustomScriptCodeData
{
	private readonly Expression<LookupCompilationDelegate> _code;

	private readonly string[] _names;

	private readonly string _moduleName;

	public PythonSavableScriptCode(Expression<LookupCompilationDelegate> code, SourceUnit sourceUnit, string[] names, string moduleName)
		: base(sourceUnit)
	{
		_code = code;
		_names = names;
		_moduleName = moduleName;
	}

	protected override KeyValuePair<MethodBuilder, Type> CompileForSave(TypeGen typeGen)
	{
		LambdaExpression lambdaExpression = RewriteForSave(typeGen, _code);
		MethodBuilder methodBuilder = typeGen.TypeBuilder.DefineMethod(lambdaExpression.Name ?? "lambda_method", CompilerHelpers.PublicStatic | MethodAttributes.SpecialName);
		lambdaExpression.CompileToMethod(methodBuilder);
		methodBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(CachedOptimizedCodeAttribute).GetConstructor(new Type[1] { typeof(string[]) }), new object[1] { _names }));
		return new KeyValuePair<MethodBuilder, Type>(methodBuilder, typeof(LookupCompilationDelegate));
	}

	public override object Run()
	{
		throw new NotSupportedException();
	}

	public override object Run(Scope scope)
	{
		throw new NotSupportedException();
	}

	public override Scope CreateScope()
	{
		throw new NotSupportedException();
	}

	string ICustomScriptCodeData.GetCustomScriptCodeData()
	{
		return _moduleName;
	}
}
