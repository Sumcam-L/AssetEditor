namespace IronPython.Compiler;

public class NameToken : Token
{
	private readonly string _name;

	public string Name => _name;

	public override object Value => _name;

	public override string Image => _name;

	public NameToken(string name)
		: base(TokenKind.Name)
	{
		_name = name;
	}
}
