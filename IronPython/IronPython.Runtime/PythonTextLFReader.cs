using System.IO;
using System.Text;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class PythonTextLFReader : PythonTextReader
{
	public PythonTextLFReader(TextReader reader, Encoding encoding, long position)
		: base(reader, encoding, position)
	{
	}

	public override string Read(int size)
	{
		if (size == 1)
		{
			int num = _reader.Read();
			if (num == -1)
			{
				return string.Empty;
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
		return _reader.ReadToEnd();
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
