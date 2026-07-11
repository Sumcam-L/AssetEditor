using System;

namespace IronPython.Compiler;

public abstract class Token
{
	private readonly TokenKind _kind;

	public TokenKind Kind => _kind;

	public virtual object Value
	{
		get
		{
			throw new NotSupportedException(Resources.TokenHasNoValue);
		}
	}

	public abstract string Image { get; }

	protected Token(TokenKind kind)
	{
		_kind = kind;
	}

	public override string ToString()
	{
		return string.Concat(base.ToString(), "(", _kind, ")");
	}
}
