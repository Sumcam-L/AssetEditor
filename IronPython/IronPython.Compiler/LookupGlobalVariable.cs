using System;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class LookupGlobalVariable : Expression, IInstructionProvider, IPythonGlobalExpression, IPythonVariableExpression, ILightExceptionAwareExpression
{
	private readonly string _name;

	private readonly bool _isLocal;

	private readonly bool _lightThrow;

	private readonly Expression _codeContextExpr;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => typeof(object);

	public override bool CanReduce => true;

	public bool IsLocal => _isLocal;

	public Expression CodeContext => _codeContextExpr;

	public string Name => _name;

	public LookupGlobalVariable(Expression codeContextExpr, string name, bool isLocal)
		: this(codeContextExpr, name, isLocal, lightThrow: false)
	{
	}

	public LookupGlobalVariable(Expression codeContextExpr, string name, bool isLocal, bool lightThrow)
	{
		_name = name;
		_isLocal = isLocal;
		_codeContextExpr = codeContextExpr;
		_lightThrow = lightThrow;
	}

	protected override Expression VisitChildren(ExpressionVisitor visitor)
	{
		return this;
	}

	public Expression RawValue()
	{
		return Expression.Call(typeof(PythonOps).GetMethod(_isLocal ? "RawGetLocal" : "RawGetGlobal"), _codeContextExpr, Utils.Constant(_name));
	}

	public override Expression Reduce()
	{
		return Expression.Call(typeof(PythonOps).GetMethod(_isLocal ? "GetLocal" : "GetGlobal"), _codeContextExpr, Utils.Constant(_name));
	}

	public Expression Assign(Expression value)
	{
		return Expression.Call(typeof(PythonOps).GetMethod(_isLocal ? "SetLocal" : "SetGlobal"), _codeContextExpr, Utils.Constant(_name), value);
	}

	public Expression Create()
	{
		return null;
	}

	public Expression Delete()
	{
		return Expression.Call(typeof(PythonOps).GetMethod(_isLocal ? "DeleteLocal" : "DeleteGlobal"), _codeContextExpr, Utils.Constant(_name));
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(_codeContextExpr);
		compiler.Instructions.Emit(new LookupGlobalInstruction(_name, _isLocal, _lightThrow));
	}

	Expression ILightExceptionAwareExpression.ReduceForLightExceptions()
	{
		if (_lightThrow)
		{
			return this;
		}
		return new LookupGlobalVariable(_codeContextExpr, _name, _isLocal, lightThrow: true);
	}
}
