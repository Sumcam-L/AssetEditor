using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

public class NoneTypeOps
{
	internal const int NoneHashCode = 505032256;

	public static readonly string __doc__;

	public static int __hash__(DynamicNull self)
	{
		return 505032256;
	}

	public static string __repr__(DynamicNull self)
	{
		return "None";
	}
}
