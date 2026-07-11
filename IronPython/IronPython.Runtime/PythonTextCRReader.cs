using System.IO;
using System.Text;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class PythonTextCRReader : PythonTextReader
{
	public PythonTextCRReader(TextReader reader, Encoding encoding, long position)
		: base(reader, encoding, position)
	{
	}

	public override string Read(int size)
	{
		if (size == 1)
		{
			int num = _reader.Read();
			switch (num)
			{
			case -1:
				return string.Empty;
			case 13:
				num = 10;
				break;
			}
			return ScriptingRuntimeHelpers.CharToString((char)num);
		}
		StringBuilder stringBuilder = new StringBuilder(size);
		while (size-- > 0)
		{
			int num2 = _reader.Read();
			if (num2 == -1)
			{
				break;
			}
			_position++;
			if (num2 == 13)
			{
				num2 = 10;
			}
			stringBuilder.Append((char)num2);
		}
		if (stringBuilder.Length == 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	public override string ReadToEnd()
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			int num = _reader.Read();
			if (num == -1)
			{
				break;
			}
			_position++;
			if (num == 13)
			{
				num = 10;
			}
			stringBuilder.Append((char)num);
		}
		if (stringBuilder.Length == 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	public override string ReadLine()
	{
		StringBuilder stringBuilder = new StringBuilder(80);
		int num;
		do
		{
			num = _reader.Read();
			if (num == -1)
			{
				break;
			}
			_position++;
			if (num == 13)
			{
				num = 10;
			}
			stringBuilder.Append((char)num);
		}
		while (num != 10);
		if (stringBuilder.Length == 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	public override string ReadLine(int size)
	{
		StringBuilder stringBuilder = new StringBuilder(80);
		while (size-- > 0)
		{
			int num = _reader.Read();
			if (num == -1)
			{
				break;
			}
			_position++;
			if (num == 13)
			{
				num = 10;
			}
			stringBuilder.Append((char)num);
			if (num == 10)
			{
				break;
			}
		}
		if (stringBuilder.Length == 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}
}
