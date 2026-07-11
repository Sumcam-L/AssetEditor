using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

internal class StringFormatSpec
{
	internal readonly char? Fill;

	internal readonly char? Alignment;

	internal readonly char? Sign;

	internal readonly char? Type;

	internal readonly int? Width;

	internal readonly int? Precision;

	internal readonly bool IncludeType;

	internal readonly bool ThousandsComma;

	private StringFormatSpec(char? fill, char? alignment, char? sign, int? width, bool thousandsComma, int? precision, char? type, bool includeType)
	{
		Fill = fill;
		Alignment = alignment;
		Sign = sign;
		Width = width;
		ThousandsComma = thousandsComma;
		Precision = precision;
		Type = type;
		IncludeType = includeType;
	}

	internal static StringFormatSpec FromString(string formatSpec)
	{
		char? c = null;
		char? sign = null;
		char? c2 = null;
		char? c3 = null;
		int? width = null;
		int? precision = null;
		bool includeType = false;
		bool flag = false;
		int curOffset = 0;
		if (formatSpec.Length >= 2)
		{
			switch (formatSpec[1])
			{
			case '<':
			case '=':
			case '>':
			case '^':
			{
				c = formatSpec[0];
				char? c4 = c;
				if (c4.GetValueOrDefault() == '\0' && c4.HasValue)
				{
					c = ' ';
				}
				c2 = formatSpec[1];
				curOffset = 2;
				break;
			}
			default:
				switch (formatSpec[0])
				{
				case '<':
				case '=':
				case '>':
				case '^':
					c2 = formatSpec[0];
					curOffset = 1;
					break;
				}
				break;
			}
		}
		if (curOffset != formatSpec.Length && (formatSpec[curOffset] == '+' || formatSpec[curOffset] == '-' || formatSpec[curOffset] == ' '))
		{
			sign = formatSpec[curOffset++];
		}
		if (curOffset != formatSpec.Length && formatSpec[curOffset] == '#')
		{
			includeType = true;
			curOffset++;
		}
		if (curOffset != formatSpec.Length && formatSpec[curOffset] == '0')
		{
			if (!((int?)c2).HasValue)
			{
				c2 = '=';
			}
			if (!((int?)c).HasValue)
			{
				c = '0';
			}
			curOffset++;
		}
		if (curOffset != formatSpec.Length && char.IsDigit(formatSpec[curOffset]))
		{
			width = ParseInt(formatSpec, ref curOffset);
		}
		if (curOffset != formatSpec.Length && formatSpec[curOffset] == ',')
		{
			curOffset++;
			flag = true;
		}
		if (curOffset != formatSpec.Length && formatSpec[curOffset] == '.')
		{
			curOffset++;
			if (curOffset == formatSpec.Length || !char.IsDigit(formatSpec[curOffset]))
			{
				throw PythonOps.ValueError("Format specifier missing precision");
			}
			precision = ParseInt(formatSpec, ref curOffset);
		}
		if (curOffset != formatSpec.Length)
		{
			c3 = formatSpec[curOffset++];
			if (flag)
			{
				switch (c3)
				{
				case 'X':
				case 'b':
				case 'c':
				case 'n':
				case 'o':
				case 'x':
					throw PythonOps.ValueError("Cannot specify ',' with '{0}'", c3);
				}
			}
		}
		return new StringFormatSpec(c, c2, sign, width, flag, precision, c3, includeType);
	}

	internal string AlignText(string text)
	{
		if (Width.HasValue)
		{
			int value = Width.Value;
			if (text.Length < value)
			{
				char c = Fill ?? ' ';
				int num = 0;
				int num2 = 0;
				int num3 = value - text.Length;
				switch (Alignment)
				{
				case null:
				case '<':
					num2 = num3;
					break;
				case '=':
				case '>':
					num = num3;
					break;
				case '^':
					num = (num2 = num3 / 2);
					if ((num3 & 1) != 0)
					{
						num2++;
					}
					break;
				}
				if (num != 0)
				{
					text = new string(c, num) + text;
				}
				if (num2 != 0)
				{
					text += new string(c, num2);
				}
			}
		}
		return text;
	}

	internal string AlignNumericText(string text, bool isZero, bool isPos)
	{
		char? c = GetSign(isZero, isPos);
		string text2 = GetTypeString();
		if (Width.HasValue)
		{
			int value = Width.Value;
			if (text.Length < value)
			{
				char c2 = Fill ?? ' ';
				int num = 0;
				int num2 = 0;
				int num3 = value - text.Length;
				if (((int?)c).HasValue)
				{
					num3--;
				}
				if (text2 != null)
				{
					num3 -= text2.Length;
				}
				if (Alignment != '=' && ((int?)c).HasValue)
				{
					text = c.Value + text2 + text;
					c = null;
					text2 = null;
				}
				switch (Alignment)
				{
				case '<':
					num2 = num3;
					break;
				case '=':
					num = num3;
					break;
				case null:
				case '>':
					num = num3;
					break;
				case '^':
					num = (num2 = num3 / 2);
					if ((num3 & 1) != 0)
					{
						num2++;
					}
					break;
				}
				if (num != 0)
				{
					string text3 = (c.HasValue ? c.Value.ToString() : "");
					text = text3 + text2 + new string(c2, num) + text;
				}
				else
				{
					if (((int?)c).HasValue)
					{
						text = c.Value + text;
					}
					if (text2 != null)
					{
						text = text2 + text;
					}
				}
				if (num2 != 0)
				{
					text += new string(c2, num2);
				}
			}
			else
			{
				text = FinishText(text, c, text2);
			}
		}
		else
		{
			text = FinishText(text, c, text2);
		}
		return text;
	}

	private static string FinishText(string text, char? sign, string type)
	{
		if (((int?)sign).HasValue)
		{
			text = sign.Value + type + text;
		}
		else if (type != null)
		{
			text = type + text;
		}
		return text;
	}

	private string GetTypeString()
	{
		string result = null;
		if (IncludeType)
		{
			switch (Type)
			{
			case 'X':
				result = "0X";
				break;
			case 'x':
				result = "0x";
				break;
			case 'o':
				result = "0o";
				break;
			case 'b':
				result = "0b";
				break;
			}
		}
		return result;
	}

	private char? GetSign(bool isZero, bool isPos)
	{
		switch (Sign)
		{
		case ' ':
			if (isPos || isZero)
			{
				return ' ';
			}
			return '-';
		case '+':
			if (isPos || isZero)
			{
				return '+';
			}
			return '-';
		default:
			if (!isPos && !isZero)
			{
				return '-';
			}
			return null;
		}
	}

	private static int? ParseInt(string formatSpec, ref int curOffset)
	{
		int? result = null;
		int num = curOffset;
		do
		{
			curOffset++;
		}
		while (curOffset < formatSpec.Length && char.IsDigit(formatSpec[curOffset]));
		if (num != curOffset)
		{
			result = int.Parse(formatSpec.Substring(num, curOffset - num));
		}
		return result;
	}
}
