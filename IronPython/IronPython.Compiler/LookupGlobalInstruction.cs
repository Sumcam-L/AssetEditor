using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Interpreter;

namespace IronPython.Compiler;

internal class LookupGlobalInstruction : Instruction
{
	private readonly string _name;

	private readonly bool _isLocal;

	private readonly bool _lightThrow;

	public override int ConsumedStack => 1;

	public override int ProducedStack => 1;

	public LookupGlobalInstruction(string name, bool isLocal, bool lightThrow)
	{
		_name = name;
		_isLocal = isLocal;
		_lightThrow = lightThrow;
	}

	public override int Run(InterpretedFrame frame)
	{
		frame.Push(PythonOps.GetVariable((CodeContext)frame.Pop(), _name, !_isLocal, _lightThrow));
		return 1;
	}

	public override string ToString()
	{
		return "LookupGlobal(" + _name + ", isLocal=" + _isLocal + ")";
	}
}
