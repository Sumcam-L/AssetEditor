namespace IronPython.Compiler;

public class IncompleteStringErrorToken : ErrorToken
{
	private readonly string _value;

	public override string Image => _value;

	public override object Value => _value;

	public IncompleteStringErrorToken(string message, string value)
		: base(message)
	{
		_value = value;
	}
}
