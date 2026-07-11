using System.Numerics;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public static class FutureBuiltins
{
	public const string __doc__ = "Provides access to built-ins which will be defined differently in Python 3.0.";

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		if (Importer.ImportModule(context.SharedContext, context.SharedContext.GlobalDict, "itertools", bottom: false, -1) is PythonModule pythonModule)
		{
			dict["map"] = pythonModule.__dict__["imap"];
			dict["filter"] = pythonModule.__dict__["ifilter"];
			dict["zip"] = pythonModule.__dict__["izip"];
		}
	}

	public static string ascii(CodeContext context, object @object)
	{
		return PythonOps.Repr(context, @object);
	}

	public static string hex(CodeContext context, object number)
	{
		if (number is int)
		{
			return Int32Ops.__hex__((int)number);
		}
		if (number is BigInteger bigInteger)
		{
			if (bigInteger < 0L)
			{
				return "-0x" + (-bigInteger).ToString(16).ToLower();
			}
			return "0x" + bigInteger.ToString(16).ToLower();
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(context, number, "__index__", out var value))
		{
			if (!(value is int) && !(value is BigInteger))
			{
				throw PythonOps.TypeError("index returned non-(int, long), got '{0}'", PythonTypeOps.GetName(value));
			}
			return hex(context, value);
		}
		throw PythonOps.TypeError("hex() argument cannot be interpreted as an index");
	}

	public static string oct(CodeContext context, object number)
	{
		if (number is int)
		{
			number = (BigInteger)(int)number;
		}
		if (number is BigInteger bigInteger)
		{
			if (bigInteger == 0L)
			{
				return "0o0";
			}
			if (bigInteger > 0L)
			{
				return "0o" + bigInteger.ToString(8);
			}
			return "-0o" + (-bigInteger).ToString(8);
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(context, number, "__index__", out var value))
		{
			if (!(value is int) && !(value is BigInteger))
			{
				throw PythonOps.TypeError("index returned non-(int, long), got '{0}'", PythonTypeOps.GetName(value));
			}
			return oct(context, value);
		}
		throw PythonOps.TypeError("oct() argument cannot be interpreted as an index");
	}
}
