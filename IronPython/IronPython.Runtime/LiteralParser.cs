using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using IronPython.Modules;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public static class LiteralParser
{
	private static char[] signs = new char[2] { '+', '-' };

	public static string ParseString(string text, bool isRaw, bool isUni)
	{
		return ParseString(text.ToCharArray(), 0, text.Length, isRaw, isUni, normalizeLineEndings: false);
	}

	public static string ParseString(char[] text, int start, int length, bool isRaw, bool isUni, bool normalizeLineEndings)
	{
		if (isRaw && !isUni && !normalizeLineEndings)
		{
			return new string(text, start, length);
		}
		StringBuilder stringBuilder = null;
		int num = start;
		int num2 = start + length;
		while (num < num2)
		{
			char c = text[num++];
			if ((!isRaw || isUni) && c == '\\')
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(length);
					stringBuilder.Append(text, start, num - start - 1);
				}
				if (num >= num2)
				{
					if (isRaw)
					{
						stringBuilder.Append('\\');
						break;
					}
					throw PythonOps.ValueError("Trailing \\ in string");
				}
				c = text[num++];
				int value;
				if (c == 'u' || c == 'U')
				{
					int num3 = ((c == 'u') ? 4 : 8);
					int b = 16;
					if (isUni)
					{
						if (!TryParseInt(text, num, num3, b, out value))
						{
							throw PythonOps.UnicodeEncodeError("'unicodeescape' codec can't decode bytes in position {0}: truncated \\uXXXX escape", num);
						}
						stringBuilder.Append((char)value);
						num += num3;
					}
					else
					{
						stringBuilder.Append('\\');
						stringBuilder.Append(c);
					}
					continue;
				}
				if (isRaw)
				{
					stringBuilder.Append('\\');
					stringBuilder.Append(c);
					continue;
				}
				switch (c)
				{
				case 'a':
					stringBuilder.Append('\a');
					continue;
				case 'b':
					stringBuilder.Append('\b');
					continue;
				case 'f':
					stringBuilder.Append('\f');
					continue;
				case 'n':
					stringBuilder.Append('\n');
					continue;
				case 'r':
					stringBuilder.Append('\r');
					continue;
				case 't':
					stringBuilder.Append('\t');
					continue;
				case 'v':
					stringBuilder.Append('\v');
					continue;
				case '\\':
					stringBuilder.Append('\\');
					continue;
				case '\'':
					stringBuilder.Append('\'');
					continue;
				case '"':
					stringBuilder.Append('"');
					continue;
				case '\r':
					if (num < num2 && text[num] == '\n')
					{
						num++;
					}
					continue;
				case 'N':
					if (num < num2 && text[num] == '{')
					{
						num++;
						StringBuilder stringBuilder2 = new StringBuilder();
						bool flag = false;
						while (num < num2)
						{
							char c2 = text[num++];
							if (c2 != '}')
							{
								stringBuilder2.Append(c2);
								continue;
							}
							flag = true;
							break;
						}
						if (!flag || stringBuilder2.Length == 0)
						{
							throw PythonOps.StandardError("'unicodeescape' codec can't decode bytes in position {0}: malformed \\N character escape", num);
						}
						try
						{
							string value3 = unicodedata.lookup(stringBuilder2.ToString());
							stringBuilder.Append(value3);
						}
						catch (KeyNotFoundException)
						{
							throw PythonOps.StandardError("'unicodeescape' codec can't decode bytes in position {0}: unknown Unicode character name", num);
						}
						continue;
					}
					throw PythonOps.StandardError("'unicodeescape' codec can't decode bytes in position {0}: malformed \\N character escape", num);
				case 'x':
					if (TryParseInt(text, num, 2, 16, out value))
					{
						stringBuilder.Append((char)value);
						num += 2;
						continue;
					}
					break;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				{
					value = c - 48;
					if (num < num2 && HexValue(text[num], out var value2) && value2 < 8)
					{
						value = value * 8 + value2;
						num++;
						if (num < num2 && HexValue(text[num], out value2) && value2 < 8)
						{
							value = value * 8 + value2;
							num++;
						}
					}
					stringBuilder.Append((char)value);
					continue;
				}
				case '\n':
					continue;
				}
				stringBuilder.Append("\\");
				stringBuilder.Append(c);
			}
			else if (c == '\r' && normalizeLineEndings)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(length);
					stringBuilder.Append(text, start, num - start - 1);
				}
				if (num < text.Length && text[num] == '\n')
				{
					num++;
				}
				stringBuilder.Append('\n');
			}
			else
			{
				stringBuilder?.Append(c);
			}
		}
		if (stringBuilder != null)
		{
			return stringBuilder.ToString();
		}
		return new string(text, start, length);
	}

	internal static List<byte> ParseBytes(char[] text, int start, int length, bool isRaw, bool normalizeLineEndings)
	{
		List<byte> list = new List<byte>(length);
		int num = start;
		int num2 = start + length;
		while (num < num2)
		{
			char c = text[num++];
			if (!isRaw && c == '\\')
			{
				if (num >= num2)
				{
					throw PythonOps.ValueError("Trailing \\ in string");
				}
				c = text[num++];
				int value;
				switch (c)
				{
				case 'a':
					list.Add(7);
					continue;
				case 'b':
					list.Add(8);
					continue;
				case 'f':
					list.Add(12);
					continue;
				case 'n':
					list.Add(10);
					continue;
				case 'r':
					list.Add(13);
					continue;
				case 't':
					list.Add(9);
					continue;
				case 'v':
					list.Add(11);
					continue;
				case '\\':
					list.Add(92);
					continue;
				case '\'':
					list.Add(39);
					continue;
				case '"':
					list.Add(34);
					continue;
				case '\r':
					if (num < num2 && text[num] == '\n')
					{
						num++;
					}
					continue;
				case 'x':
					if (TryParseInt(text, num, 2, 16, out value))
					{
						list.Add((byte)value);
						num += 2;
						continue;
					}
					break;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				{
					value = c - 48;
					if (num < num2 && HexValue(text[num], out var value2) && value2 < 8)
					{
						value = value * 8 + value2;
						num++;
						if (num < num2 && HexValue(text[num], out value2) && value2 < 8)
						{
							value = value * 8 + value2;
							num++;
						}
					}
					list.Add((byte)value);
					continue;
				}
				case '\n':
					continue;
				}
				list.Add(92);
				list.Add((byte)c);
			}
			else if (c == '\r' && normalizeLineEndings)
			{
				if (num < text.Length && text[num] == '\n')
				{
					num++;
				}
				list.Add(10);
			}
			else
			{
				list.Add((byte)c);
			}
		}
		return list;
	}

	private static bool HexValue(char ch, out int value)
	{
		switch (ch)
		{
		case '0':
		case '٠':
			value = 0;
			break;
		case '1':
		case '١':
			value = 1;
			break;
		case '2':
		case '٢':
			value = 2;
			break;
		case '3':
		case '٣':
			value = 3;
			break;
		case '4':
		case '٤':
			value = 4;
			break;
		case '5':
		case '٥':
			value = 5;
			break;
		case '6':
		case '٦':
			value = 6;
			break;
		case '7':
		case '٧':
			value = 7;
			break;
		case '8':
		case '٨':
			value = 8;
			break;
		case '9':
		case '٩':
			value = 9;
			break;
		default:
			if (ch >= 'a' && ch <= 'z')
			{
				value = ch - 97 + 10;
				break;
			}
			if (ch >= 'A' && ch <= 'Z')
			{
				value = ch - 65 + 10;
				break;
			}
			value = -1;
			return false;
		}
		return true;
	}

	private static int HexValue(char ch)
	{
		if (!HexValue(ch, out var value))
		{
			throw new ValueErrorException("bad char for integer value: " + ch);
		}
		return value;
	}

	private static int CharValue(char ch, int b)
	{
		int num = HexValue(ch);
		if (num >= b)
		{
			throw new ValueErrorException($"bad char for the integer value: '{ch}' (base {b})");
		}
		return num;
	}

	private static bool ParseInt(string text, int b, out int ret)
	{
		ret = 0;
		long num = 1L;
		int num2 = text.Length - 1;
		while (num2 >= 0)
		{
			long num3 = ret + num * CharValue(text[num2], b);
			if (int.MinValue <= num3 && num3 <= int.MaxValue)
			{
				ret = (int)num3;
				num *= b;
				if (int.MinValue > num || num > int.MaxValue)
				{
					return false;
				}
				num2--;
				continue;
			}
			return false;
		}
		return true;
	}

	private static bool TryParseInt(char[] text, int start, int length, int b, out int value)
	{
		value = 0;
		if (start + length > text.Length)
		{
			return false;
		}
		int i = start;
		for (int num = start + length; i < num; i++)
		{
			if (HexValue(text[i], out var value2) && value2 < b)
			{
				value = value * b + value2;
				continue;
			}
			return false;
		}
		return true;
	}

	public static object ParseInteger(string text, int b)
	{
		if (!ParseInt(text, b, out var ret))
		{
			BigInteger bigInteger = ParseBigInteger(text, b);
			if (!bigInteger.AsInt32(out ret))
			{
				return bigInteger;
			}
		}
		return ScriptingRuntimeHelpers.Int32ToObject(ret);
	}

	public static object ParseIntegerSign(string text, int b)
	{
		int start = 0;
		int length = text.Length;
		int b2 = b;
		short sign = 1;
		if (b < 0 || b == 1 || b > 36)
		{
			throw new ValueErrorException("base must be >= 2 and <= 36");
		}
		ParseIntegerStart(text, ref b, ref start, length, ref sign);
		int num = 0;
		try
		{
			int num2 = start;
			while (true)
			{
				if (start >= length)
				{
					if (num2 == start)
					{
						throw new ValueErrorException("Invalid integer literal");
					}
					break;
				}
				if (!HexValue(text[start], out var value))
				{
					break;
				}
				if (value >= b)
				{
					if (text[start] != 'l' && text[start] != 'L')
					{
						throw new ValueErrorException("Invalid integer literal");
					}
					break;
				}
				num = checked(num * b + sign * value);
				start++;
			}
		}
		catch (OverflowException)
		{
			return ParseBigIntegerSign(text, b2);
		}
		ParseIntegerEnd(text, start, length);
		return ScriptingRuntimeHelpers.Int32ToObject(num);
	}

	private static void ParseIntegerStart(string text, ref int b, ref int start, int end, ref short sign)
	{
		while (start < end && char.IsWhiteSpace(text, start))
		{
			start++;
		}
		if (start < end)
		{
			switch (text[start])
			{
			case '-':
				sign = -1;
				goto case '+';
			case '+':
				start++;
				break;
			}
		}
		while (start < end && char.IsWhiteSpace(text, start))
		{
			start++;
		}
		if (b != 0)
		{
			return;
		}
		if (start < end && text[start] == '0')
		{
			if (++start < end)
			{
				switch (text[start])
				{
				case 'X':
				case 'x':
					start++;
					b = 16;
					break;
				case 'O':
				case 'o':
					b = 8;
					start++;
					break;
				case 'B':
				case 'b':
					start++;
					b = 2;
					break;
				}
			}
			if (b == 0)
			{
				start--;
				b = 8;
			}
		}
		else
		{
			b = 10;
		}
	}

	private static void ParseIntegerEnd(string text, int start, int end)
	{
		while (start < end && char.IsWhiteSpace(text, start))
		{
			start++;
		}
		if (start < end)
		{
			throw new ValueErrorException("invalid integer number literal");
		}
	}

	public static BigInteger ParseBigInteger(string text, int b)
	{
		BigInteger zero = BigInteger.Zero;
		BigInteger one = BigInteger.One;
		int num = text.Length - 1;
		if (text[num] == 'l' || text[num] == 'L')
		{
			num--;
		}
		int num2 = 7;
		if (b <= 10)
		{
			num2 = 9;
		}
		while (num >= 0)
		{
			int num3 = 1;
			uint num4 = 0u;
			for (int i = 0; i < num2; i++)
			{
				if (num < 0)
				{
					break;
				}
				num4 = (uint)(CharValue(text[num--], b) * num3 + num4);
				num3 *= b;
			}
			zero += one * num4;
			if (num >= 0)
			{
				one *= (BigInteger)num3;
			}
		}
		return zero;
	}

	public static BigInteger ParseBigIntegerSign(string text, int b)
	{
		int start = 0;
		int length = text.Length;
		short sign = 1;
		if (b < 0 || b == 1 || b > 36)
		{
			throw new ValueErrorException("base must be >= 2 and <= 36");
		}
		ParseIntegerStart(text, ref b, ref start, length, ref sign);
		BigInteger bigInteger = BigInteger.Zero;
		int num = start;
		while (true)
		{
			if (start >= length)
			{
				if (start != num)
				{
					break;
				}
				throw new ValueErrorException("Invalid integer literal");
			}
			if (!HexValue(text[start], out var value))
			{
				break;
			}
			if (value >= b)
			{
				if (text[start] == 'l' || text[start] == 'L')
				{
					break;
				}
				throw new ValueErrorException("Invalid integer literal");
			}
			bigInteger = bigInteger * b + value;
			start++;
		}
		if (start < length && (text[start] == 'l' || text[start] == 'L'))
		{
			start++;
		}
		ParseIntegerEnd(text, start, length);
		if (sign >= 0)
		{
			return bigInteger;
		}
		return -bigInteger;
	}

	public static double ParseFloat(string text)
	{
		try
		{
			if (text != null && text.Length > 0 && text[text.Length - 1] == '\0')
			{
				throw PythonOps.ValueError("null byte in float literal");
			}
			return ParseFloatNoCatch(text);
		}
		catch (OverflowException)
		{
			return text.lstrip().StartsWith("-") ? double.NegativeInfinity : double.PositiveInfinity;
		}
	}

	private static double ParseFloatNoCatch(string text)
	{
		string text2 = ReplaceUnicodeDigits(text);
		switch (text2.lower().lstrip())
		{
		case "nan":
		case "+nan":
		case "-nan":
			return double.NaN;
		case "inf":
		case "+inf":
			return double.PositiveInfinity;
		case "-inf":
			return double.NegativeInfinity;
		default:
		{
			double num = double.Parse(text2, NumberStyles.Float, CultureInfo.InvariantCulture);
			if (num != 0.0 || !text.lstrip().StartsWith("-"))
			{
				return num;
			}
			return -0.0;
		}
		}
	}

	private static string ReplaceUnicodeDigits(string text)
	{
		StringBuilder stringBuilder = null;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] >= '٠' && text[i] <= '٩')
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder(text);
				}
				stringBuilder[i] = (char)(text[i] - 1632 + 48);
			}
		}
		if (stringBuilder != null)
		{
			text = stringBuilder.ToString();
		}
		return text;
	}

	private static Exception ExnMalformed()
	{
		return PythonOps.ValueError("complex() arg is a malformed string");
	}

	public static Complex ParseComplex(string s)
	{
		string text = s.Trim().ToLower();
		if (string.IsNullOrEmpty(text))
		{
			throw PythonOps.ValueError("complex() arg is an empty string");
		}
		if (text.IndexOf(' ') != -1)
		{
			throw ExnMalformed();
		}
		if (text.StartsWith("(") && text.EndsWith(")"))
		{
			text = text.Substring(1, text.Length - 2);
		}
		try
		{
			int length = text.Length;
			string text2;
			string text3;
			if (text[length - 1] == 'j')
			{
				int num = text.LastIndexOfAny(signs);
				int num2 = 0;
				while (num > 0 && text[num - 1] == 'e')
				{
					if (num2 == 2)
					{
						throw ExnMalformed();
					}
					num = text.Substring(0, num - 1).LastIndexOfAny(signs);
					num2++;
				}
				if (num < 0)
				{
					return MathUtils.MakeImaginary((length == 1) ? 1.0 : ParseFloatNoCatch(text.Substring(0, length - 1)));
				}
				text2 = text.Substring(0, num);
				text3 = text.Substring(num, length - num - 1);
				if (text3.Length == 1)
				{
					text3 += "1";
				}
			}
			else
			{
				string[] array = text.Split('j');
				if (array.Length == 1)
				{
					return MathUtils.MakeReal(ParseFloatNoCatch(text));
				}
				if (array.Length != 2)
				{
					throw ExnMalformed();
				}
				text2 = array[1];
				text3 = array[0];
				if (!text2.StartsWith("+") && !text2.StartsWith("-"))
				{
					throw ExnMalformed();
				}
			}
			return new Complex(string.IsNullOrEmpty(text2) ? 0.0 : ParseFloatNoCatch(text2), ParseFloatNoCatch(text3));
		}
		catch (OverflowException)
		{
			throw PythonOps.ValueError("complex() literal too large to convert");
		}
		catch
		{
			throw ExnMalformed();
		}
	}

	public static Complex ParseImaginary(string text)
	{
		try
		{
			return MathUtils.MakeImaginary(double.Parse(text.Substring(0, text.Length - 1), CultureInfo.InvariantCulture.NumberFormat));
		}
		catch (OverflowException)
		{
			return new Complex(0.0, double.PositiveInfinity);
		}
	}
}
