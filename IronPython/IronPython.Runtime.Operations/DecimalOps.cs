using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class DecimalOps
{
	public static int __cmp__(CodeContext context, decimal x, decimal other)
	{
		return x.CompareTo(other);
	}

	public static bool __nonzero__(decimal x)
	{
		return x != 0m;
	}

	[SpecialName]
	public static bool LessThan(decimal x, decimal y)
	{
		return x < y;
	}

	[SpecialName]
	public static bool LessThanOrEqual(decimal x, decimal y)
	{
		return x <= y;
	}

	[SpecialName]
	public static bool GreaterThan(decimal x, decimal y)
	{
		return x > y;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(decimal x, decimal y)
	{
		return x >= y;
	}

	[SpecialName]
	public static bool Equals(decimal x, decimal y)
	{
		return x == y;
	}

	[SpecialName]
	public static bool NotEquals(decimal x, decimal y)
	{
		return x != y;
	}

	internal static int __cmp__(BigInteger x, decimal y)
	{
		return -__cmp__(y, x);
	}

	internal static int __cmp__(decimal x, BigInteger y)
	{
		BigInteger bigInteger = (BigInteger)x;
		if (bigInteger == y)
		{
			decimal num = x % 1m;
			if (num == 0m)
			{
				return 0;
			}
			if (num > 0m)
			{
				return 1;
			}
			return -1;
		}
		if (!(bigInteger > y))
		{
			return -1;
		}
		return 1;
	}

	[return: MaybeNotImplemented]
	internal static object __cmp__(object x, decimal y)
	{
		return __cmp__(y, x);
	}

	[return: MaybeNotImplemented]
	internal static object __cmp__(decimal x, object y)
	{
		if (object.ReferenceEquals(y, null))
		{
			return ScriptingRuntimeHelpers.Int32ToObject(1);
		}
		return PythonOps.NotImplemented;
	}

	public static int __hash__(decimal x)
	{
		return ((BigInteger)x).GetHashCode();
	}
}
