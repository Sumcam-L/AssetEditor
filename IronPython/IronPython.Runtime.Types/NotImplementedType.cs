using System.Threading;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

[Documentation(null)]
[PythonType("NotImplementedType")]
public class NotImplementedType : ICodeFormattable
{
	private static NotImplementedType _instance;

	internal static NotImplementedType Value
	{
		get
		{
			if (_instance == null)
			{
				Interlocked.CompareExchange(ref _instance, new NotImplementedType(), null);
			}
			return _instance;
		}
	}

	private NotImplementedType()
	{
	}

	public string __repr__(CodeContext context)
	{
		return "NotImplemented";
	}

	public int __hash__()
	{
		return 505028248;
	}
}
