using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonCodecs
{
	[PythonHidden]
	public class EncodingMap
	{
		internal Dictionary<int, char> Mapping = new Dictionary<int, char>();
	}

	public const string __doc__ = "Provides access to various codecs (ASCII, UTF7, UTF8, etc...)";

	internal const int EncoderIndex = 0;

	internal const int DecoderIndex = 1;

	internal const int StreamReaderIndex = 2;

	internal const int StreamWriterIndex = 3;

	public static object ascii_decode(object input)
	{
		return ascii_decode(input, "strict");
	}

	public static object ascii_decode(object input, string errors)
	{
		return DoDecode(PythonAsciiEncoding.Instance, input, errors, fAlwaysThrow: true);
	}

	public static object ascii_encode(object input)
	{
		return ascii_encode(input, "strict");
	}

	public static object ascii_encode(object input, string errors)
	{
		return DoEncode(PythonAsciiEncoding.Instance, input, errors);
	}

	public static EncodingMap charmap_build(string decoding_table)
	{
		if (decoding_table.Length != 256)
		{
			throw PythonOps.TypeError("charmap_build expected 256 character string");
		}
		EncodingMap encodingMap = new EncodingMap();
		for (int i = 0; i < decoding_table.Length; i++)
		{
			encodingMap.Mapping[decoding_table[i]] = (char)i;
		}
		return encodingMap;
	}

	public static PythonTuple charmap_decode([BytesConversion] string input, string errors, [NotNull] string map)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			int num = input[i];
			if (map.Length <= num)
			{
				if (errors == "strict")
				{
					throw PythonOps.UnicodeDecodeError("failed to find key in mapping");
				}
				stringBuilder.Append("\ufffd");
			}
			else
			{
				stringBuilder.Append(map[input[i]]);
			}
		}
		return PythonTuple.MakeTuple(stringBuilder.ToString(), stringBuilder.Length);
	}

	public static PythonTuple charmap_encode([BytesConversion] string input, string errors, [NotNull] EncodingMap map)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<int, char> mapping = map.Mapping;
		for (int i = 0; i < input.Length; i++)
		{
			if (!mapping.TryGetValue(input[i], out var value))
			{
				if (errors == "strict")
				{
					throw PythonOps.UnicodeEncodeError("failed to find key in mapping");
				}
				stringBuilder.Append("\ufffd");
			}
			else
			{
				stringBuilder.Append(value);
			}
		}
		return PythonTuple.MakeTuple(stringBuilder.ToString(), stringBuilder.Length);
	}

	public static object charbuffer_encode()
	{
		throw PythonOps.NotImplementedError("charbuffer_encode");
	}

	public static object charmap_decode([BytesConversion] string input, [Optional] string errors, [Optional] IDictionary<object, object> map)
	{
		return CharmapDecodeWorker(input, errors, map, isDecode: true);
	}

	private static object CharmapDecodeWorker(string input, string errors, IDictionary<object, object> map, bool isDecode)
	{
		if (input.Length == 0)
		{
			return PythonTuple.MakeTuple(string.Empty, 0);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			if (map == null)
			{
				stringBuilder.Append(input[i]);
				continue;
			}
			object key = ScriptingRuntimeHelpers.Int32ToObject(input[i]);
			if (!map.TryGetValue(key, out var value))
			{
				if (errors == "strict" && isDecode)
				{
					throw PythonOps.UnicodeDecodeError("failed to find key in mapping");
				}
				if (!isDecode)
				{
					throw PythonOps.UnicodeEncodeError("failed to find key in mapping");
				}
				stringBuilder.Append("\ufffd");
			}
			else if (value == null)
			{
				if (errors == "strict" && isDecode)
				{
					throw PythonOps.UnicodeDecodeError("'charmap' codec can't decode characters at index {0} because charmap maps to None", i);
				}
				if (!isDecode)
				{
					throw PythonOps.UnicodeEncodeError("'charmap' codec can't encode characters at index {0} because charmap maps to None", i);
				}
				stringBuilder.Append("\ufffd");
			}
			else if (value is string)
			{
				stringBuilder.Append((string)value);
			}
			else
			{
				if (!(value is int))
				{
					throw PythonOps.TypeError("charmap must be an int, str, or None");
				}
				stringBuilder.Append((char)(int)value);
			}
		}
		return PythonTuple.MakeTuple(stringBuilder.ToString(), stringBuilder.Length);
	}

	public static object charmap_encode([BytesConversion] string input, [DefaultParameterValue("strict")] string errors, [DefaultParameterValue(null)] IDictionary<object, object> map)
	{
		return CharmapDecodeWorker(input, errors, map, isDecode: false);
	}

	public static object decode(CodeContext context, object obj)
	{
		PythonTuple pythonTuple = lookup(context, PythonContext.GetContext(context).GetDefaultEncodingName());
		return PythonOps.GetIndex(context, PythonCalls.Call(context, pythonTuple[1], obj, null), 0);
	}

	public static object decode(CodeContext context, object obj, string encoding)
	{
		PythonTuple pythonTuple = lookup(context, encoding);
		return PythonOps.GetIndex(context, PythonCalls.Call(context, pythonTuple[1], obj, null), 0);
	}

	public static object decode(CodeContext context, object obj, string encoding, string errors)
	{
		PythonTuple pythonTuple = lookup(context, encoding);
		return PythonOps.GetIndex(context, PythonCalls.Call(context, pythonTuple[1], obj, errors), 0);
	}

	public static object encode(CodeContext context, object obj)
	{
		PythonTuple pythonTuple = lookup(context, PythonContext.GetContext(context).GetDefaultEncodingName());
		return PythonOps.GetIndex(context, PythonCalls.Call(context, pythonTuple[0], obj, null), 0);
	}

	public static object encode(CodeContext context, object obj, string encoding)
	{
		PythonTuple pythonTuple = lookup(context, encoding);
		return PythonOps.GetIndex(context, PythonCalls.Call(context, pythonTuple[0], obj, null), 0);
	}

	public static object encode(CodeContext context, object obj, string encoding, string errors)
	{
		PythonTuple pythonTuple = lookup(context, encoding);
		return PythonOps.GetIndex(context, PythonCalls.Call(context, pythonTuple[0], obj, errors), 0);
	}

	public static object escape_decode(string text, [DefaultParameterValue("strict")] string errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\\')
			{
				if (i == text.Length - 1)
				{
					throw PythonOps.ValueError("\\ at end of string");
				}
				switch (text[++i])
				{
				case 'a':
					stringBuilder.Append('\a');
					break;
				case 'b':
					stringBuilder.Append('\b');
					break;
				case 't':
					stringBuilder.Append('\t');
					break;
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case '\\':
					stringBuilder.Append('\\');
					break;
				case 'f':
					stringBuilder.Append('\f');
					break;
				case 'v':
					stringBuilder.Append('\v');
					break;
				case 'x':
				{
					if (i >= text.Length - 2 || !CharToInt(text[i], out var val) || !CharToInt(text[i + 1], out var val2))
					{
						switch (errors)
						{
						case "strict":
							if (i >= text.Length - 2)
							{
								throw PythonOps.ValueError("invalid character value");
							}
							throw PythonOps.ValueError("invalid hexadecimal digit");
						case "replace":
							stringBuilder.Append("?");
							i--;
							while (i < text.Length - 1)
							{
								stringBuilder.Append(text[i++]);
							}
							break;
						default:
							throw PythonOps.ValueError("decoding error; unknown error handling code: " + errors);
						}
					}
					else
					{
						stringBuilder.Append(val * 16 + val2);
						i += 2;
					}
					break;
				}
				default:
					stringBuilder.Append("\\" + text[i]);
					break;
				case '\n':
					break;
				}
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		return PythonTuple.MakeTuple(stringBuilder.ToString(), text.Length);
	}

	private static bool CharToInt(char ch, out int val)
	{
		if (char.IsDigit(ch))
		{
			val = ch - 48;
			return true;
		}
		ch = char.ToUpper(ch);
		if (ch >= 'A' && ch <= 'F')
		{
			val = ch - 65 + 10;
			return true;
		}
		val = 0;
		return false;
	}

	public static PythonTuple escape_encode(string text, [DefaultParameterValue("strict")] string errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case '\n':
				stringBuilder.Append("\\n");
				continue;
			case '\r':
				stringBuilder.Append("\\r");
				continue;
			case '\t':
				stringBuilder.Append("\\t");
				continue;
			case '\\':
				stringBuilder.Append("\\\\");
				continue;
			case '\'':
				stringBuilder.Append("\\'");
				continue;
			}
			if (text[i] < ' ' || text[i] >= '\u007f')
			{
				stringBuilder.AppendFormat("\\x{0:x2}", (int)text[i]);
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		return PythonTuple.MakeTuple(stringBuilder.ToString(), ScriptingRuntimeHelpers.Int32ToObject(stringBuilder.Length));
	}

	public static object latin_1_decode(object input)
	{
		return latin_1_decode(input, "strict");
	}

	public static object latin_1_decode(object input, string errors)
	{
		return DoDecode(Encoding.GetEncoding("iso-8859-1"), input, errors);
	}

	public static object latin_1_encode(object input)
	{
		return latin_1_encode(input, "strict");
	}

	public static object latin_1_encode(object input, string errors)
	{
		return DoEncode(Encoding.GetEncoding("iso-8859-1"), input, errors);
	}

	public static PythonTuple lookup(CodeContext context, string encoding)
	{
		return PythonOps.LookupEncoding(context, encoding);
	}

	[LightThrowing]
	public static object lookup_error(CodeContext context, string name)
	{
		return PythonOps.LookupEncodingError(context, name);
	}

	public static PythonTuple mbcs_decode(CodeContext context, string input, [DefaultParameterValue("strict")] string errors, [DefaultParameterValue(false)] bool ignored)
	{
		return PythonTuple.MakeTuple(StringOps.decode(context, input, Encoding.Default, "replace"), Builtin.len(input));
	}

	public static PythonTuple mbcs_encode(CodeContext context, string input, [DefaultParameterValue("strict")] string errors)
	{
		return PythonTuple.MakeTuple(StringOps.encode(context, input, Encoding.Default, "replace"), Builtin.len(input));
	}

	public static PythonTuple raw_unicode_escape_decode(CodeContext context, object input, [DefaultParameterValue("strict")] string errors)
	{
		return PythonTuple.MakeTuple(StringOps.decode(context, Converter.ConvertToString(input), "raw-unicode-escape", errors), Builtin.len(input));
	}

	public static PythonTuple raw_unicode_escape_encode(CodeContext context, object input, [DefaultParameterValue("strict")] string errors)
	{
		return PythonTuple.MakeTuple(StringOps.encode(context, Converter.ConvertToString(input), "raw-unicode-escape", errors), Builtin.len(input));
	}

	public static object readbuffer_encode()
	{
		throw PythonOps.NotImplementedError("readbuffer_encode");
	}

	public static void register(CodeContext context, object search_function)
	{
		PythonOps.RegisterEncoding(context, search_function);
	}

	public static void register_error(CodeContext context, string name, object handler)
	{
		PythonOps.RegisterEncodingError(context, name, handler);
	}

	public static PythonTuple unicode_escape_decode()
	{
		throw PythonOps.NotImplementedError("unicode_escape_decode");
	}

	public static PythonTuple unicode_escape_encode()
	{
		throw PythonOps.NotImplementedError("unicode_escape_encode");
	}

	public static PythonTuple unicode_internal_decode(object input, [Optional] string errors)
	{
		return utf_16_decode(input, errors, ignored: false);
	}

	public static PythonTuple unicode_internal_encode(object input, [Optional] string errors)
	{
		PythonTuple pythonTuple = DoEncode(Encoding.Unicode, input, errors, includePreamble: false);
		return PythonTuple.MakeTuple(pythonTuple[0], (int)pythonTuple[1] * 2);
	}

	public static PythonTuple utf_16_be_decode(object input)
	{
		return utf_16_be_decode(input, "strict", ignored: false);
	}

	public static PythonTuple utf_16_be_decode(object input, string errors, [Optional] bool ignored)
	{
		return DoDecode(Encoding.BigEndianUnicode, input, errors);
	}

	public static PythonTuple utf_16_be_encode(object input)
	{
		return utf_16_be_encode(input, "strict");
	}

	public static PythonTuple utf_16_be_encode(object input, string errors)
	{
		return DoEncode(Encoding.BigEndianUnicode, input, errors);
	}

	public static PythonTuple utf_16_decode(object input)
	{
		return utf_16_decode(input, "strict", ignored: false);
	}

	public static PythonTuple utf_16_decode(object input, string errors, [Optional] bool ignored)
	{
		return DoDecode(Encoding.Unicode, input, errors);
	}

	public static PythonTuple utf_16_encode(object input)
	{
		return utf_16_encode(input, "strict");
	}

	public static PythonTuple utf_16_encode(object input, string errors)
	{
		return DoEncode(Encoding.Unicode, input, errors, includePreamble: true);
	}

	public static PythonTuple utf_16_ex_decode(object input, [Optional] string errors)
	{
		return utf_16_ex_decode(input, errors, null, null);
	}

	public static PythonTuple utf_16_ex_decode(object input, string errors, object unknown1, object unknown2)
	{
		byte[] preamble = Encoding.Unicode.GetPreamble();
		byte[] preamble2 = Encoding.BigEndianUnicode.GetPreamble();
		string text = Converter.ConvertToString(input);
		bool flag = true;
		if (text.Length > preamble.Length)
		{
			for (int i = 0; i < preamble.Length; i++)
			{
				if ((byte)text[i] != preamble[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return PythonTuple.MakeTuple(string.Empty, preamble.Length, -1);
			}
			flag = true;
		}
		if (text.Length > preamble2.Length)
		{
			for (int j = 0; j < preamble2.Length; j++)
			{
				if ((byte)text[j] != preamble2[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return PythonTuple.MakeTuple(string.Empty, preamble2.Length, 1);
			}
		}
		PythonTuple pythonTuple = utf_16_decode(input, errors, ignored: false);
		return PythonTuple.MakeTuple(pythonTuple[0], pythonTuple[1], 0);
	}

	public static PythonTuple utf_16_le_decode(object input)
	{
		return utf_16_le_decode(input, "strict", ignored: false);
	}

	public static PythonTuple utf_16_le_decode(object input, string errors, [Optional] bool ignored)
	{
		return utf_16_decode(input, errors, ignored: false);
	}

	public static PythonTuple utf_16_le_encode(object input)
	{
		return utf_16_le_encode(input, "strict");
	}

	public static PythonTuple utf_16_le_encode(object input, string errors)
	{
		return DoEncode(Encoding.Unicode, input, errors);
	}

	public static PythonTuple utf_7_decode(object input)
	{
		return utf_7_decode(input, "strict", ignored: false);
	}

	public static PythonTuple utf_7_decode(object input, string errors, [Optional] bool ignored)
	{
		return DoDecode(Encoding.UTF7, input, errors);
	}

	public static PythonTuple utf_7_encode(object input)
	{
		return utf_7_encode(input, "strict");
	}

	public static PythonTuple utf_7_encode(object input, string errors)
	{
		return DoEncode(Encoding.UTF7, input, errors);
	}

	public static PythonTuple utf_8_decode(object input)
	{
		return utf_8_decode(input, "strict", ignored: false);
	}

	public static PythonTuple utf_8_decode(object input, string errors, [Optional] bool ignored)
	{
		return DoDecode(Encoding.UTF8, input, errors);
	}

	public static PythonTuple utf_8_encode(object input)
	{
		return utf_8_encode(input, "strict");
	}

	public static PythonTuple utf_8_encode(object input, string errors)
	{
		return DoEncode(Encoding.UTF8, input, errors);
	}

	public static PythonTuple utf_32_decode(object input)
	{
		return utf_32_decode(input, "strict");
	}

	public static PythonTuple utf_32_decode(object input, string errors)
	{
		return DoDecode(Encoding.UTF32, input, errors);
	}

	public static PythonTuple utf_32_encode(object input)
	{
		return utf_32_encode(input, "strict");
	}

	public static PythonTuple utf_32_encode(object input, string errors)
	{
		return DoEncode(Encoding.UTF32, input, errors, includePreamble: true);
	}

	public static PythonTuple utf_32_ex_decode(object input, [Optional] string errors)
	{
		return utf_32_ex_decode(input, errors, null, null);
	}

	public static PythonTuple utf_32_ex_decode(object input, string errors, object unknown1, object unknown2)
	{
		byte[] preamble = Encoding.UTF32.GetPreamble();
		string text = Converter.ConvertToString(input);
		bool flag = true;
		if (text.Length > preamble.Length)
		{
			for (int i = 0; i < preamble.Length; i++)
			{
				if ((byte)text[i] != preamble[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return PythonTuple.MakeTuple(string.Empty, preamble.Length, -1);
			}
		}
		PythonTuple pythonTuple = utf_32_decode(input, errors);
		return PythonTuple.MakeTuple(pythonTuple[0], pythonTuple[1], 0);
	}

	public static PythonTuple utf_32_le_decode(object input)
	{
		return utf_32_le_decode(input, "strict", ignored: false);
	}

	public static PythonTuple utf_32_le_decode(object input, string errors, [Optional] bool ignored)
	{
		return utf_32_decode(input, errors);
	}

	public static PythonTuple utf_32_le_encode(object input)
	{
		return utf_32_le_encode(input, "strict");
	}

	public static PythonTuple utf_32_le_encode(object input, string errors)
	{
		return DoEncode(Encoding.UTF32, input, errors);
	}

	private static PythonTuple DoDecode(Encoding encoding, object input, string errors)
	{
		return DoDecode(encoding, input, errors, fAlwaysThrow: false);
	}

	private static PythonTuple DoDecode(Encoding encoding, object input, string errors, bool fAlwaysThrow)
	{
		if (!Converter.TryConvertToString(input, out var result))
		{
			if (!(input is Bytes bytes))
			{
				throw PythonOps.TypeErrorForBadInstance("argument 1 must be string, got {0}", input);
			}
			result = bytes.ToString();
		}
		int num = CheckPreamble(encoding, result);
		byte[] array = new byte[result.Length - num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (byte)result[i + num];
		}
		encoding = (Encoding)encoding.Clone();
		ExceptionFallBack exceptionFallBack = null;
		if (!fAlwaysThrow)
		{
			exceptionFallBack = (ExceptionFallBack)(encoding.DecoderFallback = new ExceptionFallBack(array));
		}
		else
		{
			encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
		}
		string text = encoding.GetString(array, 0, array.Length);
		int num2 = 0;
		if (!fAlwaysThrow)
		{
			byte[] badBytes = exceptionFallBack.buffer.badBytes;
			if (badBytes != null)
			{
				num2 = badBytes.Length;
			}
		}
		return PythonTuple.MakeTuple(text, array.Length - num2);
	}

	private static int CheckPreamble(Encoding enc, string buffer)
	{
		byte[] preamble = enc.GetPreamble();
		if (preamble.Length != 0 && buffer.Length >= preamble.Length)
		{
			bool flag = true;
			for (int i = 0; i < preamble.Length; i++)
			{
				if (preamble[i] != (byte)buffer[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return preamble.Length;
			}
		}
		return 0;
	}

	private static PythonTuple DoEncode(Encoding encoding, object input, string errors)
	{
		return DoEncode(encoding, input, errors, includePreamble: false);
	}

	private static PythonTuple DoEncode(Encoding encoding, object input, string errors, bool includePreamble)
	{
		if (Converter.TryConvertToString(input, out var result))
		{
			StringBuilder stringBuilder = new StringBuilder();
			encoding = (Encoding)encoding.Clone();
			encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
			if (includePreamble)
			{
				byte[] preamble = encoding.GetPreamble();
				for (int i = 0; i < preamble.Length; i++)
				{
					stringBuilder.Append((char)preamble[i]);
				}
			}
			byte[] bytes = encoding.GetBytes(result);
			for (int j = 0; j < bytes.Length; j++)
			{
				stringBuilder.Append((char)bytes[j]);
			}
			return PythonTuple.MakeTuple(stringBuilder.ToString(), result.Length);
		}
		throw PythonOps.TypeErrorForBadInstance("cannot decode {0}", input);
	}
}
