namespace IronPython.Compiler;

public sealed class CommentToken : Token
{
	private readonly string _comment;

	public string Comment => _comment;

	public override string Image => _comment;

	public override object Value => _comment;

	public CommentToken(string comment)
		: base(TokenKind.Comment)
	{
		_comment = comment;
	}
}
