using System;
using System.Linq.Expressions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler.Ast;

public class ConstantExpression : Expression, IInstructionProvider
{
	private class UnicodeWrapper
	{
		public readonly object Value;

		public UnicodeWrapper(string value)
		{
			Value = value;
		}
	}

	private readonly object _value;

	private static readonly System.Linq.Expressions.Expression EllipsisExpr = System.Linq.Expressions.Expression.Property(null, typeof(PythonOps).GetProperty("Ellipsis"));

	private static readonly System.Linq.Expressions.Expression TrueExpr = System.Linq.Expressions.Expression.Field(null, typeof(ScriptingRuntimeHelpers).GetField("True"));

	private static readonly System.Linq.Expressions.Expression FalseExpr = System.Linq.Expressions.Expression.Field(null, typeof(ScriptingRuntimeHelpers).GetField("False"));

	public object Value
	{
		get
		{
			if (_value is UnicodeWrapper unicodeWrapper)
			{
				return unicodeWrapper.Value;
			}
			return _value;
		}
	}

	internal bool IsUnicodeString => _value is UnicodeWrapper;

	public override Type Type => base.GlobalParent.CompilationMode.GetConstantType(Value);

	public override string NodeName => "literal";

	internal override bool CanThrow => false;

	internal override bool IsConstant => true;

	public ConstantExpression(object value)
	{
		_value = value;
	}

	internal static ConstantExpression MakeUnicode(string value)
	{
		return new ConstantExpression(new UnicodeWrapper(value));
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (_value == Ellipsis.Value)
		{
			return EllipsisExpr;
		}
		if (_value is bool)
		{
			if ((bool)_value)
			{
				return TrueExpr;
			}
			return FalseExpr;
		}
		if (_value is UnicodeWrapper unicodeWrapper)
		{
			return base.GlobalParent.Constant(unicodeWrapper.Value);
		}
		return base.GlobalParent.Constant(_value);
	}

	internal override ConstantExpression ConstantFold()
	{
		return this;
	}

	internal override string CheckAssign()
	{
		if (_value == null)
		{
			return "cannot assign to None";
		}
		return "can't assign to literal";
	}

	public override void Walk(PythonWalker walker)
	{
		walker.Walk(this);
		walker.PostWalk(this);
	}

	internal override object GetConstantValue()
	{
		return Value;
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		if (_value is bool)
		{
			compiler.Instructions.EmitLoad((bool)_value);
		}
		else if (_value is UnicodeWrapper)
		{
			compiler.Instructions.EmitLoad(((UnicodeWrapper)_value).Value);
		}
		else
		{
			compiler.Instructions.EmitLoad(_value);
		}
	}
}
