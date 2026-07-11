namespace IronPython.Compiler;

public class ErrorToken : Token
{
	private readonly string _message;

	public string Message => _message;

	public override string Image => _message;

	public override object Value => _message;

	public ErrorToken(string message)
		: base(TokenKind.Error)
	{
		_message = message;
	}
}
