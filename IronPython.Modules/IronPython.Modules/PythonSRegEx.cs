using System.Globalization;
using IronPython.Runtime;

namespace IronPython.Modules;

public static class PythonSRegEx
{
	public const string __doc__ = "non-functional _sre module.  Included only for completeness.";

	public const int MAGIC = 20031017;

	public const int CODESIZE = 2;

	public static object getlower(CodeContext context, object val, object encoding)
	{
		int num = PythonContext.GetContext(context).ConvertToInt32(val);
		int num2 = PythonContext.GetContext(context).ConvertToInt32(val);
		if (num == 32)
		{
			return (int)char.ToLower((char)num2);
		}
		return (int)char.ToLower((char)num2, CultureInfo.InvariantCulture);
	}

	public static object compile(object a, object b, object c, object d, object e, object f)
	{
		return null;
	}
}
