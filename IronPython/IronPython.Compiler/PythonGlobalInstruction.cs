using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class PythonGlobalInstruction : Instruction
{
	protected readonly PythonGlobal _global;

	public override int ProducedStack => 1;

	public PythonGlobalInstruction(PythonGlobal global)
	{
		_global = global;
	}

	public override int Run(InterpretedFrame frame)
	{
		frame.Push(_global.CurrentValue);
		return 1;
	}

	public override string ToString()
	{
		return string.Concat("GetGlobal(", _global, ")");
	}
}
