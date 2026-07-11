namespace IronPython.Compiler;

public class ConstantValueToken : Token
{
	private readonly object _value;

	public object Constant => _value;

	public override object Value => _value;

	public override string Image
	{
		get
		{
			if (_value != null)
			{
				return _value.ToString();
			}
			return "None";
		}
	}

	public ConstantValueToken(object value)
		: base(TokenKind.Constant)
	{
		_value = value;
	}
}
