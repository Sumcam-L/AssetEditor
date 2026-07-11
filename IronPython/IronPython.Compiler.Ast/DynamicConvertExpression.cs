using System;
using System.Linq.Expressions;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler.Ast;

internal class DynamicConvertExpression : System.Linq.Expressions.Expression, IInstructionProvider
{
	private abstract class ConversionInstruction : Instruction
	{
		public override int ConsumedStack => 1;

		public override int ProducedStack => 1;
	}

	private class BooleanConversionInstruction : ConversionInstruction
	{
		public static BooleanConversionInstruction Instance = new BooleanConversionInstruction();

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(Converter.ConvertToBoolean(frame.Pop()));
			return 1;
		}
	}

	private class TypedConversionInstruction : ConversionInstruction
	{
		private readonly Type _type;

		public TypedConversionInstruction(Type type)
		{
			_type = type;
		}

		public override int Run(InterpretedFrame frame)
		{
			frame.Push(Converter.Convert(frame.Pop(), _type));
			return 1;
		}
	}

	private readonly PythonConversionBinder _binder;

	private readonly CompilationMode _mode;

	private readonly System.Linq.Expressions.Expression _target;

	public override bool CanReduce => true;

	public override ExpressionType NodeType => ExpressionType.Extension;

	public override Type Type => _binder.Type;

	public DynamicConvertExpression(PythonConversionBinder binder, CompilationMode mode, System.Linq.Expressions.Expression target)
	{
		_binder = binder;
		_mode = mode;
		_target = target;
	}

	public override System.Linq.Expressions.Expression Reduce()
	{
		return _mode.ReduceDynamic(_binder, _binder.Type, _target);
	}

	public void AddInstructions(LightCompiler compiler)
	{
		compiler.Compile(_target);
		TypeCode typeCode = Type.GetTypeCode(_binder.Type);
		if (typeCode == TypeCode.Boolean)
		{
			compiler.Instructions.Emit(BooleanConversionInstruction.Instance);
		}
		else
		{
			compiler.Instructions.Emit(new TypedConversionInstruction(_binder.Type));
		}
	}
}
