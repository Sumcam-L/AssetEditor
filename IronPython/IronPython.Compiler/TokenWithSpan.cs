using Microsoft.Scripting;

namespace IronPython.Compiler;

internal struct TokenWithSpan
{
	private readonly Token _token;

	private readonly IndexSpan _span;

	public IndexSpan Span => _span;

	public Token Token => _token;

	public TokenWithSpan(Token token, IndexSpan span)
	{
		_token = token;
		_span = span;
	}
}
