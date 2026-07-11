using System;
using System.Numerics;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class ModuleOps
{
	public static IntPtr StringToHGlobalAnsi(string str)
	{
		if (str == null)
		{
			return IntPtr.Zero;
		}
		return Marshal.StringToHGlobalAnsi(str);
	}

	public static IntPtr StringToHGlobalUni(string str)
	{
		if (str == null)
		{
			return IntPtr.Zero;
		}
		return Marshal.StringToHGlobalUni(str);
	}

	public static object DoErrorCheck(object errCheckFunc, object result, object func, object[] arguments)
	{
		return PythonCalls.Call(errCheckFunc, result, func, PythonTuple.Make(arguments));
	}

	public static object CreateMemoryHolder(IntPtr data, int size)
	{
		MemoryHolder memoryHolder = new MemoryHolder(size);
		memoryHolder.CopyFrom(data, new IntPtr(size));
		return memoryHolder;
	}

	public static object CreateNativeWrapper(PythonType type, object holder)
	{
		CTypes.CData cData = (CTypes.CData)type.CreateInstance(type.Context.SharedContext);
		cData._memHolder = (MemoryHolder)holder;
		return cData;
	}

	public static object CreateCData(IntPtr dataAddress, PythonType type)
	{
		CTypes.INativeType nativeType = (CTypes.INativeType)type;
		CTypes.CData cData = (CTypes.CData)type.CreateInstance(type.Context.SharedContext);
		cData._memHolder = new MemoryHolder(nativeType.Size);
		cData._memHolder.CopyFrom(dataAddress, new IntPtr(nativeType.Size));
		return cData;
	}

	public static object CreateCFunction(IntPtr address, PythonType type)
	{
		return type.CreateInstance(type.Context.SharedContext, address);
	}

	public static CTypes.CData CheckSimpleCDataType(object o, object type)
	{
		CTypes.SimpleCData simpleCData = o as CTypes.SimpleCData;
		if (simpleCData == null && PythonOps.TryGetBoundAttr(o, "_as_parameter_", out var ret))
		{
			simpleCData = ret as CTypes.SimpleCData;
		}
		if (simpleCData != null && simpleCData.NativeType != type)
		{
			throw PythonOps.TypeErrorForTypeMismatch(((PythonType)type).Name, o);
		}
		return simpleCData;
	}

	public static CTypes.CData CheckCDataType(object o, object type)
	{
		CTypes.CData cData = o as CTypes.CData;
		if (cData == null && PythonOps.TryGetBoundAttr(o, "_as_parameter_", out var ret))
		{
			cData = ret as CTypes.CData;
		}
		if (cData == null || cData.NativeType != type)
		{
			throw ArgumentError(type, ((PythonType)type).Name, o);
		}
		return cData;
	}

	public static IntPtr GetFunctionPointerValue(object o, object type)
	{
		CTypes._CFuncPtr cFuncPtr = o as CTypes._CFuncPtr;
		if (cFuncPtr == null && PythonOps.TryGetBoundAttr(o, "_as_parameter_", out var ret))
		{
			cFuncPtr = ret as CTypes._CFuncPtr;
		}
		if (cFuncPtr == null || cFuncPtr.NativeType != type)
		{
			throw ArgumentError(type, ((PythonType)type).Name, o);
		}
		return cFuncPtr.addr;
	}

	public static CTypes.CData TryCheckCDataPointerType(object o, object type)
	{
		CTypes.CData cData = o as CTypes.CData;
		if (cData == null && PythonOps.TryGetBoundAttr(o, "_as_parameter_", out var ret))
		{
			cData = ret as CTypes.CData;
		}
		if (cData != null && cData.NativeType != ((CTypes.PointerType)type)._type)
		{
			if (!(cData is CTypes.Pointer))
			{
				throw ArgumentError(type, ((PythonType)((CTypes.PointerType)type)._type).Name, o);
			}
			return null;
		}
		return cData;
	}

	public static CTypes._Array TryCheckCharArray(object o)
	{
		if (o is CTypes._Array array && ((CTypes.ArrayType)array.NativeType).ElementType is CTypes.SimpleType { _type: CTypes.SimpleTypeKind.Char })
		{
			return array;
		}
		return null;
	}

	public static byte[] TryCheckBytes(object o)
	{
		if (o is Bytes bytes)
		{
			return bytes._bytes;
		}
		return null;
	}

	public static byte[] GetBytes(Bytes bytes)
	{
		return bytes._bytes;
	}

	public static CTypes._Array TryCheckWCharArray(object o)
	{
		if (o is CTypes._Array array && ((CTypes.ArrayType)array.NativeType).ElementType is CTypes.SimpleType { _type: CTypes.SimpleTypeKind.WChar })
		{
			return array;
		}
		return null;
	}

	public static object CreateSubclassInstance(object type, object instance)
	{
		return PythonCalls.Call(type, instance);
	}

	public static void CallbackException(Exception e, CodeContext context)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		object systemStandardError = context2.SystemStandardError;
		PythonOps.PrintWithDest(context, systemStandardError, context2.FormatException(e));
	}

	private static Exception ArgumentError(object type, string expected, object got)
	{
		PythonContext context = ((PythonType)type).Context;
		return PythonExceptions.CreateThrowable((PythonType)context.GetModuleState("ArgumentError"), $"expected {expected}, got {DynamicHelpers.GetPythonType(got).Name}");
	}

	public static CTypes.CData CheckNativeArgument(object o, object type)
	{
		if (o is CTypes.NativeArgument nativeArgument)
		{
			if (((CTypes.PointerType)type)._type != DynamicHelpers.GetPythonType(nativeArgument._obj))
			{
				throw ArgumentError(type, ((PythonType)type).Name, o);
			}
			return nativeArgument._obj;
		}
		return null;
	}

	public static string CharToString(byte c)
	{
		return new string((char)c, 1);
	}

	public static string WCharToString(char c)
	{
		return new string(c, 1);
	}

	public static char StringToChar(string s)
	{
		return s[0];
	}

	public static string EnsureString(object o)
	{
		if (!(o is string result))
		{
			throw PythonOps.TypeErrorForTypeMismatch("str", o);
		}
		return result;
	}

	public static bool CheckFunctionId(CTypes._CFuncPtr func, int id)
	{
		return func.Id == id;
	}

	public static IntPtr GetWCharPointer(object value)
	{
		if (value is string s)
		{
			return Marshal.StringToCoTaskMemUni(s);
		}
		if (value == null)
		{
			return IntPtr.Zero;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetWCharPointer(ret);
		}
		throw PythonOps.TypeErrorForTypeMismatch("wchar pointer", value);
	}

	public static IntPtr GetBSTR(object value)
	{
		if (value is string s)
		{
			return Marshal.StringToBSTR(s);
		}
		if (value == null)
		{
			return IntPtr.Zero;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetBSTR(ret);
		}
		throw PythonOps.TypeErrorForTypeMismatch("BSTR", value);
	}

	public static IntPtr GetCharPointer(object value)
	{
		if (value is string s)
		{
			return Marshal.StringToCoTaskMemAnsi(s);
		}
		if (value == null)
		{
			return IntPtr.Zero;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetCharPointer(ret);
		}
		throw PythonOps.TypeErrorForTypeMismatch("char pointer", value);
	}

	public static IntPtr GetPointer(object value)
	{
		if (value is int num && num >= 0)
		{
			return new IntPtr(num);
		}
		if (value is BigInteger)
		{
			return new IntPtr((long)(BigInteger)value);
		}
		if (value == null)
		{
			return IntPtr.Zero;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetPointer(ret);
		}
		if (value is CTypes.SimpleCData simpleCData)
		{
			CTypes.SimpleType simpleType = (CTypes.SimpleType)simpleCData.NativeType;
			if (simpleType._type == CTypes.SimpleTypeKind.WCharPointer || simpleType._type == CTypes.SimpleTypeKind.CharPointer)
			{
				return simpleCData.UnsafeAddress;
			}
			if (simpleType._type == CTypes.SimpleTypeKind.Pointer)
			{
				return simpleCData._memHolder.ReadIntPtr(0);
			}
		}
		if (value is CTypes._Array array)
		{
			return array.UnsafeAddress;
		}
		if (value is CTypes.Pointer pointer)
		{
			return pointer.UnsafeAddress;
		}
		throw PythonOps.TypeErrorForTypeMismatch("pointer", value);
	}

	public static IntPtr GetInterfacePointer(IntPtr self, int offset)
	{
		IntPtr ptr = Marshal.ReadIntPtr(self);
		return Marshal.ReadIntPtr(ptr, offset * IntPtr.Size);
	}

	public static IntPtr GetObject(object value)
	{
		GCHandle value2 = GCHandle.Alloc(value);
		return GCHandle.ToIntPtr(value2);
	}

	public static long GetSignedLongLong(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			return num.Value;
		}
		if (value is BigInteger)
		{
			return (long)(BigInteger)value;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetSignedLongLong(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("signed long long ", value);
	}

	public static long GetUnsignedLongLong(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue && num.Value >= 0)
		{
			return num.Value;
		}
		if (value is BigInteger)
		{
			return (long)(ulong)(BigInteger)value;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetUnsignedLongLong(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("unsigned long long", value);
	}

	public static double GetDouble(object value, object type)
	{
		if (value is double)
		{
			return (double)value;
		}
		if (value is float)
		{
			return (float)value;
		}
		if (value is int)
		{
			return (int)value;
		}
		if (value is BigInteger)
		{
			return ((BigInteger)value).ToFloat64();
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetDouble(ret, type);
		}
		return Converter.ConvertToDouble(value);
	}

	public static float GetSingle(object value, object type)
	{
		if (value is double)
		{
			return (float)(double)value;
		}
		if (value is float)
		{
			return (float)value;
		}
		if (value is int)
		{
			return (int)value;
		}
		if (value is BigInteger)
		{
			return (float)((BigInteger)value).ToFloat64();
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetSingle(ret, type);
		}
		return (float)Converter.ConvertToDouble(value);
	}

	public static long GetDoubleBits(object value)
	{
		if (value is double)
		{
			return BitConverter.ToInt64(BitConverter.GetBytes((double)value), 0);
		}
		if (value is float)
		{
			return BitConverter.ToInt64(BitConverter.GetBytes((float)value), 0);
		}
		if (value is int)
		{
			return BitConverter.ToInt64(BitConverter.GetBytes((double)(int)value), 0);
		}
		if (value is BigInteger)
		{
			return BitConverter.ToInt64(BitConverter.GetBytes(((BigInteger)value).ToFloat64()), 0);
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetDoubleBits(ret);
		}
		return BitConverter.ToInt64(BitConverter.GetBytes(Converter.ConvertToDouble(value)), 0);
	}

	public static int GetSingleBits(object value)
	{
		if (value is double)
		{
			return BitConverter.ToInt32(BitConverter.GetBytes((float)(double)value), 0);
		}
		if (value is float)
		{
			return BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0);
		}
		if (value is int)
		{
			return BitConverter.ToInt32(BitConverter.GetBytes((float)(int)value), 0);
		}
		if (value is BigInteger)
		{
			return BitConverter.ToInt32(BitConverter.GetBytes((float)((BigInteger)value).ToFloat64()), 0);
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetSingleBits(ret);
		}
		return BitConverter.ToInt32(BitConverter.GetBytes((float)Converter.ConvertToDouble(value)), 0);
	}

	public static int GetSignedLong(object value, object type)
	{
		if (value is int)
		{
			return (int)value;
		}
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			return num.Value;
		}
		if (value is BigInteger && ((BigInteger)value).AsUInt32(out var ret))
		{
			return (int)ret;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret2))
		{
			return GetSignedLong(ret2, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("signed long", value);
	}

	public static int GetUnsignedLong(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue && num.Value >= 0)
		{
			return num.Value;
		}
		if (value is BigInteger && ((BigInteger)value).AsUInt32(out var ret))
		{
			return (int)ret;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret2))
		{
			return GetUnsignedLong(ret2, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("unsigned long", value);
	}

	public static int GetUnsignedInt(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue && num.Value >= 0)
		{
			return num.Value;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetUnsignedInt(type, ret);
		}
		throw PythonOps.TypeErrorForTypeMismatch("unsigned int", value);
	}

	public static int GetSignedInt(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			return num.Value;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetSignedInt(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("signed int", value);
	}

	public static short GetUnsignedShort(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			int value2 = num.Value;
			if (value2 >= 0 && value2 <= 65535)
			{
				return (short)(ushort)value2;
			}
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetUnsignedShort(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("unsigned short", value);
	}

	public static short GetSignedShort(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			int value2 = num.Value;
			return (short)value2;
		}
		if (value is BigInteger bigInteger)
		{
			return (short)(int)(bigInteger & 65535);
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetSignedShort(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("signed short", value);
	}

	public static int GetVariantBool(object value, object type)
	{
		if (!Converter.ConvertToBoolean(value))
		{
			return 0;
		}
		return 1;
	}

	public static byte GetUnsignedByte(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			return (byte)num.Value;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetUnsignedByte(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("unsigned byte", value);
	}

	public static byte GetSignedByte(object value, object type)
	{
		int? num = Converter.ImplicitConvertToInt32(value);
		if (num.HasValue)
		{
			int value2 = num.Value;
			if (value2 >= -128 && value2 <= 127)
			{
				return (byte)(sbyte)value2;
			}
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetSignedByte(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("signed byte", value);
	}

	public static byte GetBoolean(object value, object type)
	{
		if (value is bool)
		{
			if (!(bool)value)
			{
				return 0;
			}
			return 1;
		}
		if (value is int)
		{
			if ((int)value == 0)
			{
				return 0;
			}
			return 1;
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetBoolean(ret, type);
		}
		throw PythonOps.TypeErrorForTypeMismatch("bool", value);
	}

	public static byte GetChar(object value, object type)
	{
		if (value is string { Length: 1 } text)
		{
			return (byte)text[0];
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetChar(ret, type);
		}
		throw ArgumentError(type, "char", value);
	}

	public static char GetWChar(object value, object type)
	{
		if (value is string { Length: 1 } text)
		{
			return text[0];
		}
		if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var ret))
		{
			return GetWChar(ret, type);
		}
		throw ArgumentError(type, "wchar", value);
	}

	public static object IntPtrToObject(IntPtr address)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(address);
		object target = gCHandle.Target;
		gCHandle.Free();
		return target;
	}
}
