namespace IronPython.Compiler;

public class OperatorToken : Token
{
	private readonly int _precedence;

	private readonly string _image;

	public int Precedence => _precedence;

	public override object Value => _image;

	public override string Image => _image;

	public OperatorToken(TokenKind kind, string image, int precedence)
		: base(kind)
	{
		_image = image;
		_precedence = precedence;
	}
}
