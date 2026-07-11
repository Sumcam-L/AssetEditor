using System.Collections;
using System.Globalization;
using System.Text;

namespace Firaxis.MayaInterface.Utilities;

public class Json
{
	private enum Token
	{
		None,
		CurlyOpen,
		CurlyClose,
		SquareOpen,
		SquareClose,
		Colon,
		Comma,
		String,
		Number,
		True,
		False,
		Null
	}

	private const string whiteSpaceChars = " \t\n\r";

	private const string numberChars = "0123456789+-.eE";

	private const int builderCapacity = 2000;

	public object Decode(string json)
	{
		bool success;
		return Decode(json, out success);
	}

	public object Decode(string json, out bool success)
	{
		success = false;
		if (!string.IsNullOrWhiteSpace(json))
		{
			int index = 0;
			return ParseValue(json.ToCharArray(), ref index, ref success);
		}
		return null;
	}

	private string ParseString(char[] json, ref int index, ref bool success)
	{
		EatWhitespace(json, ref index);
		StringBuilder stringBuilder = new StringBuilder(2000);
		char c = json[index++];
		bool flag = false;
		while (!flag && index != json.Length)
		{
			c = json[index++];
			switch (c)
			{
			case '"':
				flag = true;
				break;
			case '\\':
			{
				if (index == json.Length)
				{
					break;
				}
				switch (json[index++])
				{
				case '"':
					stringBuilder.Append('"');
					continue;
				case '\\':
					stringBuilder.Append('\\');
					continue;
				case '/':
					stringBuilder.Append('/');
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
				case 'u':
					break;
				default:
					continue;
				}
				int num = json.Length - index;
				if (num < 4)
				{
					break;
				}
				if (!(success = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result)))
				{
					return string.Empty;
				}
				stringBuilder.Append(char.ConvertFromUtf32((int)result));
				index += 4;
				continue;
			}
			default:
				stringBuilder.Append(c);
				continue;
			}
			break;
		}
		if (!flag)
		{
			success = false;
			return null;
		}
		success = true;
		return stringBuilder.ToString();
	}

	private int GetLastIndexOfNumber(char[] json, int index)
	{
		int i;
		for (i = index; i < json.Length && "0123456789+-.eE".IndexOf(json[i]) != -1; i++)
		{
		}
		return i - 1;
	}

	private object ParseNumber(char[] json, ref int index, ref bool success)
	{
		EatWhitespace(json, ref index);
		int lastIndexOfNumber = GetLastIndexOfNumber(json, index);
		int length = lastIndexOfNumber - index + 1;
		string s = new string(json, index, length);
		index = lastIndexOfNumber + 1;
		if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			success = true;
			return result;
		}
		success = double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result2);
		return result2;
	}

	private object ParseObject(char[] json, ref int index, ref bool success)
	{
		NextToken(json, ref index);
		Hashtable hashtable = new Hashtable();
		bool flag = false;
		while (!flag)
		{
			switch (LookAhead(json, index))
			{
			case Token.None:
				success = false;
				return null;
			case Token.Comma:
				NextToken(json, ref index);
				continue;
			case Token.CurlyClose:
				NextToken(json, ref index);
				return hashtable;
			}
			string key = ParseString(json, ref index, ref success);
			if (!success)
			{
				return hashtable;
			}
			if (NextToken(json, ref index) != Token.Colon)
			{
				success = false;
				return null;
			}
			object value = ParseValue(json, ref index, ref success);
			if (!success)
			{
				return null;
			}
			hashtable[key] = value;
		}
		return hashtable;
	}

	private object ParseArray(char[] json, ref int index, ref bool success)
	{
		ArrayList arrayList = new ArrayList();
		NextToken(json, ref index);
		bool flag = false;
		while (!flag)
		{
			switch (LookAhead(json, index))
			{
			case Token.None:
				success = false;
				return null;
			case Token.Comma:
				NextToken(json, ref index);
				continue;
			case Token.SquareClose:
				break;
			default:
			{
				object value = ParseValue(json, ref index, ref success);
				if (!success)
				{
					return null;
				}
				arrayList.Add(value);
				continue;
			}
			}
			NextToken(json, ref index);
			break;
		}
		return arrayList;
	}

	protected object ParseValue(char[] json, ref int index, ref bool success)
	{
		switch (LookAhead(json, index))
		{
		case Token.String:
			return ParseString(json, ref index, ref success);
		case Token.Number:
			return ParseNumber(json, ref index, ref success);
		case Token.CurlyOpen:
			return ParseObject(json, ref index, ref success);
		case Token.SquareOpen:
			return ParseArray(json, ref index, ref success);
		case Token.True:
			NextToken(json, ref index);
			return true;
		case Token.False:
			NextToken(json, ref index);
			return false;
		case Token.Null:
			NextToken(json, ref index);
			return null;
		default:
			success = false;
			return null;
		}
	}

	private Token LookAhead(char[] json, int index)
	{
		int index2 = index;
		return NextToken(json, ref index2);
	}

	protected void EatWhitespace(char[] json, ref int index)
	{
		while (index < json.Length && " \t\n\r".IndexOf(json[index]) != -1)
		{
			index++;
		}
	}

	private Token NextToken(char[] json, ref int index)
	{
		EatWhitespace(json, ref index);
		if (index == json.Length)
		{
			return Token.None;
		}
		char c = json[index];
		index++;
		switch (c)
		{
		case '{':
			return Token.CurlyOpen;
		case '}':
			return Token.CurlyClose;
		case '[':
			return Token.SquareOpen;
		case ']':
			return Token.SquareClose;
		case ',':
			return Token.Comma;
		case '"':
			return Token.String;
		case '-':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			return Token.Number;
		case ':':
			return Token.Colon;
		default:
		{
			index--;
			int num = json.Length - index;
			if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
			{
				index += 5;
				return Token.False;
			}
			if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
			{
				index += 4;
				return Token.True;
			}
			if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
			{
				index += 4;
				return Token.Null;
			}
			return Token.None;
		}
		}
	}
}
