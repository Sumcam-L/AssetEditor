using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class PythonLightThrowGlobalInstruction : PythonGlobalInstruction
{
	public PythonLightThrowGlobalInstruction(PythonGlobal global)
		: base(global)
	{
	}

	public override int Run(InterpretedFrame frame)
	{
		frame.Push(_global.CurrentValueLightThrow);
		return 1;
	}
}
