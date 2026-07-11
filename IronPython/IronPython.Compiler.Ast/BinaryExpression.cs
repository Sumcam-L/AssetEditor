using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Compiler.Ast;

public class BinaryExpression : Expression, IInstructionProvider
{
	private abstract class BinaryInstruction : Instruction
	{
		public override int ConsumedStack => 2;

		public override int ProducedStack => 1;
	}

	private class IsInstruction : BinaryInstruction
	{
		public static readonly IsInstruction Instance = new IsInstruction();

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(PythonOps.Is(frame.Pop(), frame.Pop()));
			return 1;
		}
	}

	private class IsNotInstruction : BinaryInstruction
	{
		public static readonly IsNotInstruction Instance = new IsNotInstruction();

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(PythonOps.IsNot(frame.Pop(), frame.Pop()));
			return 1;
		}
	}

	private const int MaximumInlineStringLength = 1048576;

	private readonly Expression _left;

	private readonly Expression _right;

	private readonly PythonOperator _op;

	public Expression Left => _left;

	public Expression Right => _right;

	public PythonOperator Operator => _op;

	internal override bool CanThrow
	{
		get
		{
			if (_op == PythonOperator.Is || _op == PythonOperator.IsNot)
			{
				if (!_left.CanThrow)
				{
					return _right.CanThrow;
				}
				return true;
			}
			return true;
		}
	}

	internal override ConstantExpression ConstantFold()
	{
		Expression expression = _left.ConstantFold();
		Expression expression2 = _right.ConstantFold();
		ConstantExpression constantExpression = expression as ConstantExpression;
		ConstantExpression constantExpression2 = expression2 as ConstantExpression;
		try
		{
			if (constantExpression != null && constantExpression2 != null && constantExpression.Value != null && constantExpression2.Value != null && constantExpression.Value.GetType() == constantExpression2.Value.GetType())
			{
				if (constantExpression.Value.GetType() == typeof(int))
				{
					switch (_op)
					{
					case PythonOperator.Add:
						return new ConstantExpression(Int32Ops.Add((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.Subtract:
						return new ConstantExpression(Int32Ops.Subtract((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.Power:
						return new ConstantExpression(Int32Ops.Power((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.Multiply:
						return new ConstantExpression(Int32Ops.Multiply((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.FloorDivide:
						return new ConstantExpression(Int32Ops.FloorDivide((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.Divide:
						return new ConstantExpression(Int32Ops.Divide((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.TrueDivide:
						return new ConstantExpression(Int32Ops.TrueDivide((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.Mod:
						return new ConstantExpression(Int32Ops.Mod((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.LeftShift:
						return new ConstantExpression(Int32Ops.LeftShift((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.RightShift:
						return new ConstantExpression(Int32Ops.RightShift((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.BitwiseAnd:
						return new ConstantExpression(Int32Ops.BitwiseAnd((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.BitwiseOr:
						return new ConstantExpression(Int32Ops.BitwiseOr((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.Xor:
						return new ConstantExpression(Int32Ops.ExclusiveOr((int)constantExpression.Value, (int)constantExpression2.Value));
					case PythonOperator.LessThan:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(Int32Ops.Compare((int)constantExpression.Value, (int)constantExpression2.Value) < 0));
					case PythonOperator.GreaterThan:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(Int32Ops.Compare((int)constantExpression.Value, (int)constantExpression2.Value) > 0));
					case PythonOperator.LessThanOrEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(Int32Ops.Compare((int)constantExpression.Value, (int)constantExpression2.Value) <= 0));
					case PythonOperator.GreaterThanOrEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(Int32Ops.Compare((int)constantExpression.Value, (int)constantExpression2.Value) >= 0));
					case PythonOperator.Equal:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(Int32Ops.Compare((int)constantExpression.Value, (int)constantExpression2.Value) == 0));
					case PythonOperator.NotEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(Int32Ops.Compare((int)constantExpression.Value, (int)constantExpression2.Value) != 0));
					}
				}
				if (constantExpression.Value.GetType() == typeof(double))
				{
					switch (_op)
					{
					case PythonOperator.Add:
						return new ConstantExpression(DoubleOps.Add((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.Subtract:
						return new ConstantExpression(DoubleOps.Subtract((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.Power:
						return new ConstantExpression(DoubleOps.Power((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.Multiply:
						return new ConstantExpression(DoubleOps.Multiply((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.FloorDivide:
						return new ConstantExpression(DoubleOps.FloorDivide((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.Divide:
						return new ConstantExpression(DoubleOps.Divide((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.TrueDivide:
						return new ConstantExpression(DoubleOps.TrueDivide((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.Mod:
						return new ConstantExpression(DoubleOps.Mod((double)constantExpression.Value, (double)constantExpression2.Value));
					case PythonOperator.LessThan:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(DoubleOps.Compare((double)constantExpression.Value, (double)constantExpression2.Value) < 0));
					case PythonOperator.GreaterThan:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(DoubleOps.Compare((double)constantExpression.Value, (double)constantExpression2.Value) > 0));
					case PythonOperator.LessThanOrEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(DoubleOps.Compare((double)constantExpression.Value, (double)constantExpression2.Value) <= 0));
					case PythonOperator.GreaterThanOrEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(DoubleOps.Compare((double)constantExpression.Value, (double)constantExpression2.Value) >= 0));
					case PythonOperator.Equal:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(DoubleOps.Compare((double)constantExpression.Value, (double)constantExpression2.Value) == 0));
					case PythonOperator.NotEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(DoubleOps.Compare((double)constantExpression.Value, (double)constantExpression2.Value) != 0));
					}
				}
				if (constantExpression.Value.GetType() == typeof(BigInteger))
				{
					switch (_op)
					{
					case PythonOperator.Add:
						return new ConstantExpression(BigIntegerOps.Add((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.Subtract:
						return new ConstantExpression(BigIntegerOps.Subtract((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.Power:
						return new ConstantExpression(BigIntegerOps.Power((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.Multiply:
						return new ConstantExpression(BigIntegerOps.Multiply((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.FloorDivide:
						return new ConstantExpression(BigIntegerOps.FloorDivide((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.Divide:
						return new ConstantExpression(BigIntegerOps.Divide((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.TrueDivide:
						return new ConstantExpression(BigIntegerOps.TrueDivide((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.Mod:
						return new ConstantExpression(BigIntegerOps.Mod((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.LeftShift:
						return new ConstantExpression(BigIntegerOps.LeftShift((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.RightShift:
						return new ConstantExpression(BigIntegerOps.RightShift((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.BitwiseAnd:
						return new ConstantExpression(BigIntegerOps.BitwiseAnd((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.BitwiseOr:
						return new ConstantExpression(BigIntegerOps.BitwiseOr((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.Xor:
						return new ConstantExpression(BigIntegerOps.ExclusiveOr((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value));
					case PythonOperator.LessThan:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(BigIntegerOps.Compare((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value) < 0));
					case PythonOperator.GreaterThan:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(BigIntegerOps.Compare((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value) > 0));
					case PythonOperator.LessThanOrEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(BigIntegerOps.Compare((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value) <= 0));
					case PythonOperator.GreaterThanOrEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(BigIntegerOps.Compare((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value) >= 0));
					case PythonOperator.Equal:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(BigIntegerOps.Compare((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value) == 0));
					case PythonOperator.NotEqual:
						return new ConstantExpression(ScriptingRuntimeHelpers.BooleanToObject(BigIntegerOps.Compare((BigInteger)constantExpression.Value, (BigInteger)constantExpression2.Value) != 0));
					}
				}
				if (constantExpression.Value.GetType() == typeof(Complex))
				{
					switch (_op)
					{
					case PythonOperator.Add:
						return new ConstantExpression(ComplexOps.Add((Complex)constantExpression.Value, (Complex)constantExpression2.Value));
					case PythonOperator.Subtract:
						return new ConstantExpression(ComplexOps.Subtract((Complex)constantExpression.Value, (Complex)constantExpression2.Value));
					case PythonOperator.Power:
						return new ConstantExpression(ComplexOps.Power((Complex)constantExpression.Value, (Complex)constantExpression2.Value));
					case PythonOperator.Multiply:
						return new ConstantExpression(ComplexOps.Multiply((Complex)constantExpression.Value, (Complex)constantExpression2.Value));
					case PythonOperator.Divide:
						return new ConstantExpression(ComplexOps.Divide((Complex)constantExpression.Value, (Complex)constantExpression2.Value));
					case PythonOperator.TrueDivide:
						return new ConstantExpression(ComplexOps.TrueDivide((Complex)constantExpression.Value, (Complex)constantExpression2.Value));
					}
				}
				if (constantExpression.Value.GetType() == typeof(string) && _op == PythonOperator.Add)
				{
					return new ConstantExpression((string)constantExpression.Value + (string)constantExpression2.Value);
				}
			}
			else if (_op == PythonOperator.Multiply && constantExpression != null && constantExpression2 != null)
			{
				if (constantExpression.Value.GetType() == typeof(string) && constantExpression2.Value.GetType() == typeof(int))
				{
					string text = StringOps.Multiply((string)constantExpression.Value, (int)constantExpression2.Value);
					if (text.Length < 1048576)
					{
						return new ConstantExpression(text);
					}
				}
				else if (constantExpression.Value.GetType() == typeof(int) && constantExpression2.Value.GetType() == typeof(string))
				{
					string text2 = StringOps.Multiply((string)constantExpression2.Value, (int)constantExpression.Value);
					if (text2.Length < 1048576)
					{
						return new ConstantExpression(text2);
					}
				}
			}
		}
		catch (ArithmeticException)
		{
		}
		return null;
	}

	public BinaryExpression(PythonOperator op, Expression left, Expression right)
	{
		ContractUtils.RequiresNotNull(left, "left");
		ContractUtils.RequiresNotNull(right, "right");
		if (op == PythonOperator.None)
		{
			throw new ValueErrorException("bad operator");
		}
		_op = op;
		_left = left;
		_right = right;
		base.StartIndex = left.StartIndex;
		base.EndIndex = right.EndIndex;
	}

	private bool IsComparison()
	{
		switch (_op)
		{
		case PythonOperator.LessThan:
		case PythonOperator.LessThanOrEqual:
		case PythonOperator.GreaterThan:
		case PythonOperator.GreaterThanOrEqual:
		case PythonOperator.Equal:
		case PythonOperator.NotEqual:
		case PythonOperator.In:
		case PythonOperator.NotIn:
		case PythonOperator.IsNot:
		case PythonOperator.Is:
			return true;
		default:
			return false;
		}
	}

	private bool NeedComparisonTransformation()
	{
		if (IsComparison())
		{
			return IsComparison(_right);
		}
		return false;
	}

	public static bool IsComparison(Expression expression)
	{
		if (expression is BinaryExpression binaryExpression)
		{
			return binaryExpression.IsComparison();
		}
		return false;
	}

	private System.Linq.Expressions.Expression FinishCompare(System.Linq.Expressions.Expression left)
	{
		BinaryExpression binaryExpression = (BinaryExpression)_right;
		System.Linq.Expressions.Expression left2 = binaryExpression.Left;
		ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(object), "chained_comparison");
		System.Linq.Expressions.Expression left3 = MakeBinaryOperation(_op, left, System.Linq.Expressions.Expression.Assign(parameterExpression, Utils.Convert(left2, parameterExpression.Type)), base.Span);
		System.Linq.Expressions.Expression right;
		if (IsComparison(binaryExpression._right))
		{
			right = binaryExpression.FinishCompare(parameterExpression);
		}
		else
		{
			System.Linq.Expressions.Expression right2 = binaryExpression.Right;
			right = MakeBinaryOperation(binaryExpression.Operator, parameterExpression, right2, binaryExpression.Span);
		}
		ParameterExpression temp;
		System.Linq.Expressions.Expression expression = Utils.CoalesceTrue(left3, right, AstMethods.IsTrue, out temp);
		return System.Linq.Expressions.Expression.Block(new ParameterExpression[2] { parameterExpression, temp }, expression);
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		if (!CanEmitWarning(_op))
		{
			ConstantExpression constantExpression = ConstantFold();
			if (constantExpression != null)
			{
				constantExpression.Parent = base.Parent;
				return Utils.Convert(constantExpression.Reduce(), typeof(object));
			}
		}
		if (_op == PythonOperator.Mod && _left is ConstantExpression constantExpression2 && constantExpression2.Value is string)
		{
			MethodInfo method = ((!constantExpression2.IsUnicodeString) ? AstMethods.FormatString : AstMethods.FormatUnicode);
			return System.Linq.Expressions.Expression.Call(method, base.Parent.LocalContext, _left, Utils.Convert(_right, typeof(object)));
		}
		if (NeedComparisonTransformation())
		{
			return FinishCompare(_left);
		}
		return MakeBinaryOperation(_op, _left, _right, base.Span);
	}

	void IInstructionProvider.AddInstructions(LightCompiler compiler)
	{
		if (NeedComparisonTransformation())
		{
			compiler.Compile(Reduce());
			return;
		}
		switch (_op)
		{
		case PythonOperator.Is:
			compiler.Compile(_left);
			compiler.Compile(_right);
			compiler.Instructions.Emit(IsInstruction.Instance);
			break;
		case PythonOperator.IsNot:
			compiler.Compile(_left);
			compiler.Compile(_right);
			compiler.Instructions.Emit(IsNotInstruction.Instance);
			break;
		default:
			compiler.Compile(Reduce());
			break;
		}
	}

	internal override string CheckAssign()
	{
		return "can't assign to operator";
	}

	private System.Linq.Expressions.Expression MakeBinaryOperation(PythonOperator op, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right, SourceSpan span)
	{
		switch (op)
		{
		case PythonOperator.NotIn:
			return Utils.Convert(System.Linq.Expressions.Expression.Not(base.GlobalParent.Operation(typeof(bool), PythonOperationKind.Contains, left, right)), typeof(object));
		case PythonOperator.In:
			return Utils.Convert(base.GlobalParent.Operation(typeof(bool), PythonOperationKind.Contains, left, right), typeof(object));
		default:
		{
			PythonOperationKind pythonOperationKind = PythonOperatorToAction(op);
			if (pythonOperationKind != PythonOperationKind.None)
			{
				if (CanEmitWarning(op))
				{
					ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(left.Type, "left");
					ParameterExpression parameterExpression2 = System.Linq.Expressions.Expression.Parameter(right.Type, "right");
					return System.Linq.Expressions.Expression.Block(new ParameterExpression[2] { parameterExpression, parameterExpression2 }, System.Linq.Expressions.Expression.Call(AstMethods.WarnDivision, base.Parent.LocalContext, Utils.Constant(base.GlobalParent.DivisionOptions), Utils.Convert(System.Linq.Expressions.Expression.Assign(parameterExpression, left), typeof(object)), Utils.Convert(System.Linq.Expressions.Expression.Assign(parameterExpression2, right), typeof(object))), base.GlobalParent.Operation(typeof(object), pythonOperationKind, parameterExpression, parameterExpression2));
				}
				return base.GlobalParent.Operation(typeof(object), pythonOperationKind, left, right);
			}
			return System.Linq.Expressions.Expression.Call(GetHelperMethod(op), Node.ConvertIfNeeded(left, typeof(object)), Node.ConvertIfNeeded(right, typeof(object)));
		}
		}
	}

	private bool CanEmitWarning(PythonOperator op)
	{
		if (op == PythonOperator.Divide)
		{
			if (base.GlobalParent.DivisionOptions != PythonDivisionOptions.Warn)
			{
				return base.GlobalParent.DivisionOptions == PythonDivisionOptions.WarnAll;
			}
			return true;
		}
		return false;
	}

	public override void Walk(PythonWalker walker)
	{
		if (walker.Walk(this))
		{
			_left.Walk(walker);
			_right.Walk(walker);
		}
		walker.PostWalk(this);
	}

	private static PythonOperationKind PythonOperatorToAction(PythonOperator op)
	{
		switch (op)
		{
		case PythonOperator.Add:
			return PythonOperationKind.Add;
		case PythonOperator.Subtract:
			return PythonOperationKind.Subtract;
		case PythonOperator.Multiply:
			return PythonOperationKind.Multiply;
		case PythonOperator.Divide:
			return PythonOperationKind.Divide;
		case PythonOperator.TrueDivide:
			return PythonOperationKind.TrueDivide;
		case PythonOperator.Mod:
			return PythonOperationKind.Mod;
		case PythonOperator.BitwiseAnd:
			return PythonOperationKind.BitwiseAnd;
		case PythonOperator.BitwiseOr:
			return PythonOperationKind.BitwiseOr;
		case PythonOperator.Xor:
			return PythonOperationKind.ExclusiveOr;
		case PythonOperator.LeftShift:
			return PythonOperationKind.LeftShift;
		case PythonOperator.RightShift:
			return PythonOperationKind.RightShift;
		case PythonOperator.Power:
			return PythonOperationKind.Power;
		case PythonOperator.FloorDivide:
			return PythonOperationKind.FloorDivide;
		case PythonOperator.LessThan:
			return PythonOperationKind.LessThan;
		case PythonOperator.LessThanOrEqual:
			return PythonOperationKind.LessThanOrEqual;
		case PythonOperator.GreaterThan:
			return PythonOperationKind.GreaterThan;
		case PythonOperator.GreaterThanOrEqual:
			return PythonOperationKind.GreaterThanOrEqual;
		case PythonOperator.Equal:
			return PythonOperationKind.Equal;
		case PythonOperator.NotEqual:
			return PythonOperationKind.NotEqual;
		case PythonOperator.In:
			return PythonOperationKind.Contains;
		case PythonOperator.NotIn:
		case PythonOperator.IsNot:
		case PythonOperator.Is:
			return PythonOperationKind.None;
		default:
			return PythonOperationKind.None;
		}
	}

	private static MethodInfo GetHelperMethod(PythonOperator op)
	{
		return op switch
		{
			PythonOperator.IsNot => AstMethods.IsNot, 
			PythonOperator.Is => AstMethods.Is, 
			_ => null, 
		};
	}
}
