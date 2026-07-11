namespace IronPython.Compiler.Ast;

internal class PythonReference
{
	private readonly string _name;

	private PythonVariable _variable;

	public string Name => _name;

	internal PythonVariable PythonVariable
	{
		get
		{
			return _variable;
		}
		set
		{
			_variable = value;
		}
	}

	public PythonReference(string name)
	{
		_name = name;
	}
}
