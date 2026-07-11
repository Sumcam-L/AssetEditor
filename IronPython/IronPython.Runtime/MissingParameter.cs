namespace IronPython.Runtime;

public sealed class MissingParameter
{
	public static readonly MissingParameter Value = new MissingParameter();

	private MissingParameter()
	{
	}
}
