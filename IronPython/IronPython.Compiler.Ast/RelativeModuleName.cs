namespace IronPython.Compiler.Ast;

public class RelativeModuleName : ModuleName
{
	private readonly int _dotCount;

	public int DotCount => _dotCount;

	public RelativeModuleName(string[] names, int dotCount)
		: base(names)
	{
		_dotCount = dotCount;
	}
}
