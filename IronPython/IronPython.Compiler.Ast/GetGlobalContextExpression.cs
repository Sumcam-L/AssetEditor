using System;
using System.Linq.Expressions;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

internal class GetGlobalContextExpression : System.Linq.Expressions.Expression, IInstructionProvider
{
	private class GetGlobalContextInstruction : Instruction
	{
		public static readonly GetGlobalContextInstruction Instance = new GetGlobalContextInstruction();

		public override int ConsumedStack => 1;

		public override int ProducedStack => 1;

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(PythonOps.GetGlobalContext((CodeContext)frame.Pop()));
			return 1;
		}
	}

	private readonly System.Linq.Expressions.Expression _parentContext;

	public override bool CanReduce => true;

	public override ExpressionType NodeType => ExpressionType.Extension;

	public override Type Type => typeof(CodeContext);

	public GetGlobalContextExpression(System.Linq.Expressions.Expression parentContext)
	{
		_parentContext = parentContext;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return System.Linq.Expressions.Expression.Call(AstMethods.GetGlobalContext, _parentContext);
	}

	public void AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(_parentContext);
		compiler.Instructions.Emit(GetGlobalContextInstruction.Instance);
	}
}
