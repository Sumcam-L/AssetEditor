using System;
using IronPython.Runtime;
using Microsoft.Scripting.Ast;

namespace IronPython.Compiler.Ast;

internal class SerializedScopeStatement : ScopeStatement
{
	private readonly string _name;

	private readonly string _filename;

	private readonly FunctionAttributes _flags;

	private readonly string[] _parameterNames;

	public override string Name => _name;

	internal override string Filename => _filename;

	internal override FunctionAttributes Flags => _flags;

	internal override string[] ParameterNames => _parameterNames;

	internal override int ArgCount => _parameterNames.Length;

	internal SerializedScopeStatement(string name, string[] argNames, FunctionAttributes flags, int startIndex, int endIndex, string path, string[] freeVars, string[] names, string[] cellVars, string[] varNames)
	{
		_name = name;
		_filename = path;
		_flags = flags;
		SetLoc(null, startIndex, endIndex);
		_parameterNames = argNames;
		if (freeVars != null)
		{
			foreach (string name2 in freeVars)
			{
				AddFreeVariable(new PythonVariable(name2, VariableKind.Local, this), accessedInScope: false);
			}
		}
		if (names != null)
		{
			foreach (string name3 in names)
			{
				AddGlobalVariable(new PythonVariable(name3, VariableKind.Global, this));
			}
		}
		if (varNames != null)
		{
			foreach (string name4 in varNames)
			{
				EnsureVariable(name4);
			}
		}
		if (cellVars != null)
		{
			foreach (string name5 in cellVars)
			{
				AddCellVariable(new PythonVariable(name5, VariableKind.Local, this));
			}
		}
	}

	internal override LightLambdaExpression GetLambda()
	{
		throw new InvalidOperationException();
	}

	internal override bool ExposesLocalVariable(PythonVariable variable)
	{
		throw new InvalidOperationException();
	}

	internal override PythonVariable BindReference(PythonNameBinder binder, PythonReference reference)
	{
		throw new InvalidOperationException();
	}

	public override void Walk(PythonWalker walker)
	{
		throw new InvalidOperationException();
	}
}
