using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class BoolOps
{
	[StaticExtensionMethod]
	public static object __new__(object cls)
	{
		return ScriptingRuntimeHelpers.False;
	}

	[StaticExtensionMethod]
	public static bool __new__(object cls, object o)
	{
		return PythonOps.IsTrue(o);
	}

	[SpecialName]
	public static bool BitwiseAnd(bool x, bool y)
	{
		return x && y;
	}

	[SpecialName]
	public static bool BitwiseOr(bool x, bool y)
	{
		return x || y;
	}

	[SpecialName]
	public static bool ExclusiveOr(bool x, bool y)
	{
		return x ^ y;
	}

	[SpecialName]
	public static int BitwiseAnd(int x, bool y)
	{
		return Int32Ops.BitwiseAnd(y ? 1 : 0, x);
	}

	[SpecialName]
	public static int BitwiseAnd(bool x, int y)
	{
		return Int32Ops.BitwiseAnd(x ? 1 : 0, y);
	}

	[SpecialName]
	public static int BitwiseOr(int x, bool y)
	{
		return Int32Ops.BitwiseOr(y ? 1 : 0, x);
	}

	[SpecialName]
	public static int BitwiseOr(bool x, int y)
	{
		return Int32Ops.BitwiseOr(x ? 1 : 0, y);
	}

	[SpecialName]
	public static int ExclusiveOr(int x, bool y)
	{
		return Int32Ops.ExclusiveOr(y ? 1 : 0, x);
	}

	[SpecialName]
	public static int ExclusiveOr(bool x, int y)
	{
		return Int32Ops.ExclusiveOr(x ? 1 : 0, y);
	}

	public static string __repr__(bool self)
	{
		if (!self)
		{
			return "False";
		}
		return "True";
	}

	public static string __format__(CodeContext context, bool self, [NotNull] string formatSpec)
	{
		return __repr__(self);
	}

	[SpecialName]
	public static bool Equals(bool x, bool y)
	{
		return x == y;
	}

	[SpecialName]
	public static bool NotEquals(bool x, bool y)
	{
		return x != y;
	}

	[SpecialName]
	public static bool Equals(bool x, int y)
	{
		return (x ? 1 : 0) == y;
	}

	[SpecialName]
	public static bool NotEquals(bool x, int y)
	{
		return (x ? 1 : 0) != y;
	}

	[SpecialName]
	public static bool Equals(int x, bool y)
	{
		return Equals(y, x);
	}

	[SpecialName]
	public static bool NotEquals(int x, bool y)
	{
		return NotEquals(y, x);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static sbyte ConvertToSByte(bool x)
	{
		return (sbyte)(x ? 1 : 0);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static byte ConvertToByte(bool x)
	{
		return (byte)(x ? 1u : 0u);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static short ConvertToInt16(bool x)
	{
		return (short)(x ? 1 : 0);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static ushort ConvertToUInt16(bool x)
	{
		return (ushort)(x ? 1u : 0u);
	}

	public static int __int__(bool x)
	{
		if (!x)
		{
			return 0;
		}
		return 1;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static int ConvertToInt32(bool x)
	{
		if (!x)
		{
			return 0;
		}
		return 1;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static uint ConvertToUInt32(bool x)
	{
		if (!x)
		{
			return 0u;
		}
		return 1u;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static long ConvertToInt64(bool x)
	{
		return x ? 1 : 0;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static ulong ConvertToUInt64(bool x)
	{
		return (ulong)(int)(x ? 1u : 0u);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static float ConvertToSingle(bool x)
	{
		return x ? 1 : 0;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static double ConvertToDouble(bool x)
	{
		return x ? 1 : 0;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static Complex ConvertToComplex(bool x)
	{
		if (!x)
		{
			return Complex.Zero;
		}
		return Complex.One;
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static decimal ConvertToDecimal(bool x)
	{
		if (x)
		{
			return 1m;
		}
		return 0m;
	}
}
