namespace IronPython.Compiler;

public class SymbolToken : Token
{
	private readonly string _image;

	public string Symbol => _image;

	public override object Value => _image;

	public override string Image => _image;

	public SymbolToken(TokenKind kind, string image)
		: base(kind)
	{
		_image = image;
	}
}
