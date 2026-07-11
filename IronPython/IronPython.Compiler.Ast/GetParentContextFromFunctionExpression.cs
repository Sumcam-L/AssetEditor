using System;
using System.Linq.Expressions;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

internal class GetParentContextFromFunctionExpression : System.Linq.Expressions.Expression, IInstructionProvider
{
	private class GetParentContextFromFunctionInstruction : Instruction
	{
		public static readonly GetParentContextFromFunctionInstruction Instance = new GetParentContextFromFunctionInstruction();

		public override int ProducedStack => 1;

		public override int ConsumedStack => 1;

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(PythonOps.GetParentContextFromFunction((PythonFunction)frame.Pop()));
			return 1;
		}
	}

	private static System.Linq.Expressions.Expression _parentContext = System.Linq.Expressions.Expression.Call(AstMethods.GetParentContextFromFunction, FunctionDefinition._functionParam);

	public override bool CanReduce => true;

	public override ExpressionType NodeType => ExpressionType.Extension;

	public override Type Type => typeof(CodeContext);

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _parentContext;
	}

	public void AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(FunctionDefinition._functionParam);
		compiler.Instructions.Emit(GetParentContextFromFunctionInstruction.Instance);
	}
}
