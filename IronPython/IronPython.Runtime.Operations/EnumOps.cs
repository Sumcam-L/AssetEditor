using System;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class EnumOps
{
	[SpecialName]
	public static object BitwiseOr(object self, object other)
	{
		object obj = EnumUtils.BitwiseOr(self, other);
		if (obj != null)
		{
			return obj;
		}
		throw PythonOps.ValueError("bitwise or cannot be applied to {0} and {1}", self.GetType(), other.GetType());
	}

	[SpecialName]
	public static object BitwiseAnd(object self, object other)
	{
		object obj = EnumUtils.BitwiseAnd(self, other);
		if (obj != null)
		{
			return obj;
		}
		throw PythonOps.ValueError("bitwise and cannot be applied to {0} and {1}", self.GetType(), other.GetType());
	}

	[SpecialName]
	public static object ExclusiveOr(object self, object other)
	{
		object obj = EnumUtils.ExclusiveOr(self, other);
		if (obj != null)
		{
			return obj;
		}
		throw PythonOps.ValueError("bitwise xor cannot be applied to {0} and {1}", self.GetType(), other.GetType());
	}

	[SpecialName]
	public static object OnesComplement(object self)
	{
		object obj = EnumUtils.OnesComplement(self);
		if (obj != null)
		{
			return obj;
		}
		throw PythonOps.ValueError("one's complement cannot be applied to {0}", self.GetType());
	}

	public static bool __nonzero__(object self)
	{
		if (self is Enum)
		{
			Type type = self.GetType();
			Type underlyingType = Enum.GetUnderlyingType(type);
			switch (underlyingType.GetTypeCode())
			{
			case TypeCode.Int16:
				return (short)self != 0;
			case TypeCode.Int32:
				return (int)self != 0;
			case TypeCode.Int64:
				return (long)self != 0;
			case TypeCode.UInt16:
				return (ushort)self != 0;
			case TypeCode.UInt32:
				return (uint)self != 0;
			case TypeCode.UInt64:
				return ~(ulong)self != 0;
			case TypeCode.Byte:
				return (byte)self != 0;
			case TypeCode.SByte:
				return (sbyte)self != 0;
			}
		}
		throw PythonOps.ValueError("__nonzero__ cannot be applied to {0}", self.GetType());
	}

	public static string __repr__(object self)
	{
		if (Enum.IsDefined(self.GetType(), self))
		{
			string name = Enum.GetName(self.GetType(), self);
			return self.GetType().FullName + "." + name;
		}
		return $"<enum {self.GetType().FullName}: {self.ToString()}>";
	}
}
