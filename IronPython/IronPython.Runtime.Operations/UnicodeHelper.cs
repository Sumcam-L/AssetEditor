using System.Reflection;
using System.Runtime.InteropServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public class UnicodeHelper
{
	internal static BuiltinFunction Function = BuiltinFunction.MakeFunction("unicode", ArrayUtils.ConvertAll(typeof(UnicodeHelper).GetMember("unicode"), (MemberInfo x) => (MethodInfo)x), typeof(string));

	public static object unicode(CodeContext context)
	{
		return string.Empty;
	}

	public static object unicode(CodeContext context, object @string)
	{
		return StringOps.FastNewUnicode(context, @string);
	}

	public static object unicode(CodeContext context, object @string, object encoding)
	{
		return StringOps.FastNewUnicode(context, @string, encoding);
	}

	public static object unicode(CodeContext context, object @string, [Optional] object encoding, object errors)
	{
		return StringOps.FastNewUnicode(context, @string, encoding, errors);
	}
}
