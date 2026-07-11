namespace IronPython.Compiler;

internal sealed class UnicodeStringToken : ConstantValueToken
{
	public UnicodeStringToken(object value)
		: base(value)
	{
	}
}
