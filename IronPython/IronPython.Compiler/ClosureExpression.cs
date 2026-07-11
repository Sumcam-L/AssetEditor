using System;
using System.Linq.Expressions;
using System.Reflection;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

internal class ClosureExpression : System.Linq.Expressions.Expression, IPythonVariableExpression
{
	private class MakeClosureCellExpression : System.Linq.Expressions.Expression, IInstructionProvider
	{
		private class MakeClosureCellInstruction : Instruction
		{
			public static readonly MakeClosureCellInstruction Instance = new MakeClosureCellInstruction();

			public override int ProducedStack => 1;

			public override int ConsumedStack => 0;

			public override int Run(InterpretedFrame frame)
			{
				frame.Push(PythonOps.MakeClosureCell());
				return 1;
			}
		}

		private static readonly System.Linq.Expressions.Expression _call = System.Linq.Expressions.Expression.Call(AstMethods.MakeClosureCell);

		public static readonly MakeClosureCellExpression Instance = new MakeClosureCellExpression();

		public override bool CanReduce => true;

		public override ExpressionType NodeType => ExpressionType.Extension;

		public override Type Type => typeof(ClosureCell);

		public override System.Linq.Expressions.Expression Reduce()
		{
			return _call;
		}

		public void AddInstructions(LightCompiler compiler)
		{
			compiler.Instructions.Emit(MakeClosureCellInstruction.Instance);
		}
	}

	private readonly System.Linq.Expressions.Expression _closureCell;

	private readonly ParameterExpression _parameter;

	private readonly PythonVariable _variable;

	internal static readonly FieldInfo _cellField = typeof(ClosureCell).GetField("Value");

	public System.Linq.Expressions.Expression ClosureCell => _closureCell;

	public ParameterExpression OriginalParameter => _parameter;

	public PythonVariable PythonVariable => _variable;

	public sealed override ExpressionType NodeType => ExpressionType.Extension;

	public sealed override Type Type => typeof(object);

	public override bool CanReduce => true;

	public string Name => _variable.Name;

	public ClosureExpression(PythonVariable variable, System.Linq.Expressions.Expression closureCell, ParameterExpression parameter)
	{
		_variable = variable;
		_closureCell = closureCell;
		_parameter = parameter;
	}

	public System.Linq.Expressions.Expression Create()
	{
		if (OriginalParameter != null)
		{
			return System.Linq.Expressions.Expression.Assign(_closureCell, System.Linq.Expressions.Expression.Call(AstMethods.MakeClosureCellWithValue, OriginalParameter));
		}
		return System.Linq.Expressions.Expression.Assign(_closureCell, MakeClosureCellExpression.Instance);
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Field(_closureCell, _cellField);
	}

	public System.Linq.Expressions.Expression Assign(System.Linq.Expressions.Expression value)
	{
		return System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(_closureCell, _cellField), value);
	}

	public System.Linq.Expressions.Expression Delete()
	{
		return System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(_closureCell, _cellField), System.Linq.Expressions.Expression.Field(null, typeof(Uninitialized).GetField("Instance")));
	}
}
