namespace IronPython.Runtime;

public sealed class KwCallInfo
{
	private readonly object[] _args;

	private readonly string[] _names;

	public object[] Arguments => _args;

	public string[] Names => _names;

	public KwCallInfo(object[] args, string[] names)
	{
		_args = args;
		_names = names;
	}
}
