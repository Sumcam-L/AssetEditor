using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class CharOps
{
	public static bool __eq__(char self, char other)
	{
		return self == other;
	}

	[SpecialName]
	public static bool __ne__(char self, char other)
	{
		return self != other;
	}

	public static int __hash__(char self)
	{
		return new string(self, 1).GetHashCode();
	}

	[return: MaybeNotImplemented]
	public static object __cmp__(char self, object other)
	{
		if (other is char)
		{
			int num = self - (char)other;
			return (num > 0) ? 1 : ((num < 0) ? (-1) : 0);
		}
		if (other is string { Length: 1 } text)
		{
			int num2 = self - text[0];
			return (num2 > 0) ? 1 : ((num2 < 0) ? (-1) : 0);
		}
		return NotImplementedType.Value;
	}

	public static bool __contains__(char self, char other)
	{
		return self == other;
	}

	public static bool __contains__(char self, string other)
	{
		if (other.Length == 1)
		{
			return other[0] == self;
		}
		return false;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static string ConvertToString(char self)
	{
		return new string(self, 1);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static char ConvertToChar(int value)
	{
		if (value < 0 || value > 65535)
		{
			throw new OverflowException();
		}
		return (char)value;
	}
}
