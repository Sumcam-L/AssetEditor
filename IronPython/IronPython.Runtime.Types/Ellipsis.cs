using System.Threading;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

[Documentation(null)]
[PythonType("ellipsis")]
public class Ellipsis : ICodeFormattable
{
	private static Ellipsis _instance;

	internal static Ellipsis Value
	{
		get
		{
			if (_instance == null)
			{
				Interlocked.CompareExchange(ref _instance, new Ellipsis(), null);
			}
			return _instance;
		}
	}

	private Ellipsis()
	{
	}

	public string __repr__(CodeContext context)
	{
		return "Ellipsis";
	}

	public int __hash__()
	{
		return 505045512;
	}
}
