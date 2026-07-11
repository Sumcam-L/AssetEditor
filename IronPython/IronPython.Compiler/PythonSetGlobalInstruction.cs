using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class PythonSetGlobalInstruction : Instruction
{
	private readonly PythonGlobal _global;

	public override int ProducedStack => 1;

	public override int ConsumedStack => 1;

	public PythonSetGlobalInstruction(PythonGlobal global)
	{
		_global = global;
	}

	public override int Run(InterpretedFrame frame)
	{
		_global.CurrentValue = frame.Peek();
		return 1;
	}

	public override string ToString()
	{
		return string.Concat("SetGlobal(", _global, ")");
	}
}
