using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonBinaryAscii
{
	private delegate char EncodeChar(int val);

	private delegate int DecodeByte(char val);

	public const string __doc__ = "Provides functions for converting between binary data encoded in various formats and ASCII.";

	private const int IgnoreByte = -1;

	private const int EmptyByte = -2;

	private const int PadByte = -3;

	private const int InvalidByte = -4;

	private const int NoMoreBytes = -5;

	private static readonly object _ErrorKey = new object();

	private static readonly object _IncompleteKey = new object();

	private static Exception Error(CodeContext context, params object[] args)
	{
		return PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState(_ErrorKey), args);
	}

	private static Exception Incomplete(CodeContext context, params object[] args)
	{
		return PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState(_IncompleteKey), args);
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException(_ErrorKey, dict, "Error", "binascii");
		context.EnsureModuleException(_IncompleteKey, dict, "Incomplete", "binascii");
	}

	private static int UuDecFunc(char val)
	{
		if (val > ' ' && val < '`')
		{
			return val - 32;
		}
		switch (val)
		{
		case '\n':
		case '\r':
		case ' ':
		case '`':
			return -2;
		default:
			return -4;
		}
	}

	public static string a2b_uu(CodeContext context, string data)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (data.Length < 1)
		{
			return new string('\0', 32);
		}
		int num = (data[0] + 32) % 64;
		int num2 = (num * 4 + 2) / 3;
		string text = null;
		if (data.Length - 1 > num2)
		{
			text = data.Substring(1 + num2);
			data = data.Substring(1, num2);
		}
		else
		{
			data = data.Substring(1);
		}
		StringBuilder stringBuilder = DecodeWorker(context, data, bounded: true, UuDecFunc);
		if (text == null)
		{
			stringBuilder.Append('\0', num - stringBuilder.Length);
		}
		else
		{
			ProcessSuffix(context, text, UuDecFunc);
		}
		return stringBuilder.ToString();
	}

	public static string b2a_uu(CodeContext context, string data)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (data.Length > 45)
		{
			throw Error(context, "At most 45 bytes at once");
		}
		StringBuilder stringBuilder = EncodeWorker(data, ' ', (int val) => (char)(32 + val % 64));
		stringBuilder.Insert(0, ((char)(32 + data.Length)).ToString());
		stringBuilder.Append('\n');
		return stringBuilder.ToString();
	}

	private static int Base64DecFunc(char val)
	{
		if (val >= 'A' && val <= 'Z')
		{
			return val - 65;
		}
		if (val >= 'a' && val <= 'z')
		{
			return val - 97 + 26;
		}
		if (val >= '0' && val <= '9')
		{
			return val - 48 + 52;
		}
		return val switch
		{
			'+' => 62, 
			'/' => 63, 
			'=' => -3, 
			_ => -1, 
		};
	}

	public static object a2b_base64(CodeContext context, string data)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		data = RemovePrefix(context, data, Base64DecFunc);
		if (data.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = DecodeWorker(context, data, bounded: false, Base64DecFunc);
		return stringBuilder.ToString();
	}

	public static object b2a_base64(string data)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (data.Length == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = EncodeWorker(data, '=', EncodeValue);
		stringBuilder.Append('\n');
		return stringBuilder.ToString();
	}

	private static char EncodeValue(int val)
	{
		if (val < 26)
		{
			return (char)(65 + val);
		}
		if (val < 52)
		{
			return (char)(97 + val - 26);
		}
		if (val < 62)
		{
			return (char)(48 + val - 52);
		}
		return val switch
		{
			62 => '+', 
			63 => '/', 
			_ => throw new InvalidOperationException($"Bad int val: {val}"), 
		};
	}

	public static object a2b_qp(object data)
	{
		throw new NotImplementedException();
	}

	[LightThrowing]
	public static object a2b_qp(object data, object header)
	{
		return LightExceptions.Throw(new NotImplementedException());
	}

	public static object b2a_qp(object data)
	{
		throw new NotImplementedException();
	}

	public static object b2a_qp(object data, object quotetabs)
	{
		throw new NotImplementedException();
	}

	public static object b2a_qp(object data, object quotetabs, object istext)
	{
		throw new NotImplementedException();
	}

	public static object b2a_qp(object data, object quotetabs, object istext, object header)
	{
		throw new NotImplementedException();
	}

	public static object a2b_hqx(object data)
	{
		throw new NotImplementedException();
	}

	public static object rledecode_hqx(object data)
	{
		throw new NotImplementedException();
	}

	public static object rlecode_hqx(object data)
	{
		throw new NotImplementedException();
	}

	public static object b2a_hqx(object data)
	{
		throw new NotImplementedException();
	}

	public static object crc_hqx(object data, object crc)
	{
		throw new NotImplementedException();
	}

	[Documentation("crc32(string[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of string.")]
	public static int crc32(string buffer, [DefaultParameterValue(0)] int baseValue)
	{
		byte[] array = buffer.MakeByteArray();
		return (int)crc32(array, 0, array.Length, (uint)baseValue);
	}

	[Documentation("crc32(string[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of string.")]
	public static int crc32(string buffer, uint baseValue)
	{
		byte[] array = buffer.MakeByteArray();
		return (int)crc32(array, 0, array.Length, baseValue);
	}

	[Documentation("crc32(byte_array[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of byte_array.")]
	public static int crc32(byte[] buffer, [DefaultParameterValue(0)] int baseValue)
	{
		return (int)crc32(buffer, 0, buffer.Length, (uint)baseValue);
	}

	[Documentation("crc32(byte_array[, value]) -> string\n\nComputes a CRC (Cyclic Redundancy Check) checksum of byte_array.")]
	public static int crc32(byte[] buffer, uint baseValue)
	{
		return (int)crc32(buffer, 0, buffer.Length, baseValue);
	}

	internal static uint crc32(byte[] buffer, int offset, int count, uint baseValue)
	{
		uint num = baseValue ^ 0xFFFFFFFFu;
		for (int i = offset; i < offset + count; i++)
		{
			num ^= buffer[i];
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) == 0) ? (num >> 1) : ((num >> 1) ^ 0xEDB88320u));
			}
		}
		return num ^ 0xFFFFFFFFu;
	}

	public static string b2a_hex(string data)
	{
		StringBuilder stringBuilder = new StringBuilder(data.Length * 2);
		for (int i = 0; i < data.Length; i++)
		{
			stringBuilder.AppendFormat("{0:x2}", (int)data[i]);
		}
		return stringBuilder.ToString();
	}

	public static string hexlify(string data)
	{
		return b2a_hex(data);
	}

	public static Bytes hexlify(MemoryView data)
	{
		return hexlify(data.tobytes());
	}

	public static Bytes hexlify(Bytes data)
	{
		byte[] array = new byte[data.Count * 2];
		for (int i = 0; i < data.Count; i++)
		{
			array[i * 2] = ToHex(data._bytes[i] >> 4);
			array[i * 2 + 1] = ToHex(data._bytes[i] & 0xF);
		}
		return Bytes.Make(array);
	}

	private static byte ToHex(int p)
	{
		if (p >= 10)
		{
			return (byte)(97 + p - 10);
		}
		return (byte)(48 + p);
	}

	public static string hexlify([NotNull] PythonBuffer data)
	{
		return hexlify(data.ToString());
	}

	public static object a2b_hex(CodeContext context, string data)
	{
		if (data == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if ((data.Length & 1) != 0)
		{
			throw Error(context, "string must be even lengthed");
		}
		StringBuilder stringBuilder = new StringBuilder(data.Length / 2);
		for (int i = 0; i < data.Length; i += 2)
		{
			byte b = ((!char.IsDigit(data[i])) ? ((byte)(char.ToUpper(data[i]) - 65 + 10)) : ((byte)(data[i] - 48)));
			byte b2 = ((!char.IsDigit(data[i + 1])) ? ((byte)(char.ToUpper(data[i + 1]) - 65 + 10)) : ((byte)(data[i + 1] - 48)));
			stringBuilder.Append((char)(b * 16 + b2));
		}
		return stringBuilder.ToString();
	}

	public static object unhexlify(CodeContext context, string hexstr)
	{
		return a2b_hex(context, hexstr);
	}

	private static StringBuilder EncodeWorker(string data, char empty, EncodeChar encFunc)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < data.Length; i += 3)
		{
			switch (data.Length - i)
			{
			case 1:
			{
				int num = (data[i] & 0xFF) << 16;
				stringBuilder.Append(encFunc((num >> 18) & 0x3F));
				stringBuilder.Append(encFunc((num >> 12) & 0x3F));
				stringBuilder.Append(empty);
				stringBuilder.Append(empty);
				break;
			}
			case 2:
			{
				int num = ((data[i] & 0xFF) << 16) | ((data[i + 1] & 0xFF) << 8);
				stringBuilder.Append(encFunc((num >> 18) & 0x3F));
				stringBuilder.Append(encFunc((num >> 12) & 0x3F));
				stringBuilder.Append(encFunc((num >> 6) & 0x3F));
				stringBuilder.Append(empty);
				break;
			}
			default:
			{
				int num = ((data[i] & 0xFF) << 16) | ((data[i + 1] & 0xFF) << 8) | (data[i + 2] & 0xFF);
				stringBuilder.Append(encFunc((num >> 18) & 0x3F));
				stringBuilder.Append(encFunc((num >> 12) & 0x3F));
				stringBuilder.Append(encFunc((num >> 6) & 0x3F));
				stringBuilder.Append(encFunc(num & 0x3F));
				break;
			}
			}
		}
		return stringBuilder;
	}

	private static int NextVal(CodeContext context, string data, ref int index, DecodeByte decFunc)
	{
		while (index < data.Length)
		{
			int num = decFunc(data[index++]);
			switch (num)
			{
			case -2:
				return 0;
			case -4:
				throw Error(context, "Illegal char");
			default:
				return num;
			case -1:
				break;
			}
		}
		return -5;
	}

	private static int CountPadBytes(CodeContext context, string data, int bound, ref int index, DecodeByte decFunc)
	{
		int num = -3;
		int i;
		for (i = 0; bound < 0 || i < bound; i++)
		{
			if ((num = NextVal(context, data, ref index, decFunc)) != -3)
			{
				break;
			}
		}
		if (num != -3 && num != -5)
		{
			index--;
		}
		return i;
	}

	private static int GetVal(CodeContext context, string data, int align, bool bounded, ref int index, DecodeByte decFunc)
	{
		while (true)
		{
			int num = NextVal(context, data, ref index, decFunc);
			switch (num)
			{
			case -3:
				switch (align)
				{
				case 0:
				case 1:
					CountPadBytes(context, data, -1, ref index, decFunc);
					break;
				case 2:
					if (CountPadBytes(context, data, 1, ref index, decFunc) > 0)
					{
						return -5;
					}
					break;
				default:
					return -5;
				}
				break;
			case -5:
				if (bounded || align == 0)
				{
					return -5;
				}
				throw Error(context, "Incorrect padding");
			case -2:
				return 0;
			default:
				return num;
			}
		}
	}

	private static StringBuilder DecodeWorker(CodeContext context, string data, bool bounded, DecodeByte decFunc)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int index = 0;
		while (index < data.Length)
		{
			int val = GetVal(context, data, 0, bounded, ref index, decFunc);
			if (val < 0)
			{
				break;
			}
			int val2 = GetVal(context, data, 1, bounded, ref index, decFunc);
			if (val2 < 0)
			{
				break;
			}
			int val3 = GetVal(context, data, 2, bounded, ref index, decFunc);
			int num;
			if (val3 < 0)
			{
				num = (val << 18) | (val2 << 12);
				stringBuilder.Append((char)((num >> 16) & 0xFF));
				break;
			}
			int val4 = GetVal(context, data, 3, bounded, ref index, decFunc);
			if (val4 < 0)
			{
				num = (val << 18) | (val2 << 12) | (val3 << 6);
				stringBuilder.Append((char)((num >> 16) & 0xFF));
				stringBuilder.Append((char)((num >> 8) & 0xFF));
				break;
			}
			num = (val << 18) | (val2 << 12) | (val3 << 6) | val4;
			stringBuilder.Append((char)((num >> 16) & 0xFF));
			stringBuilder.Append((char)((num >> 8) & 0xFF));
			stringBuilder.Append((char)(num & 0xFF));
		}
		return stringBuilder;
	}

	private static string RemovePrefix(CodeContext context, string data, DecodeByte decFunc)
	{
		int i;
		for (i = 0; i < data.Length; i++)
		{
			int num = decFunc(data[i]);
			if (num == -4)
			{
				throw Error(context, "Illegal char");
			}
			if (num >= 0)
			{
				break;
			}
		}
		if (i != 0)
		{
			return data.Substring(i);
		}
		return data;
	}

	private static void ProcessSuffix(CodeContext context, string data, DecodeByte decFunc)
	{
		for (int i = 0; i < data.Length; i++)
		{
			int num = decFunc(data[i]);
			if (num >= 0 || num == -4)
			{
				throw Error(context, "Trailing garbage");
			}
		}
	}
}
