using System.IO;
using System.Text;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class PythonUniversalReader : PythonTextReader
{
	public enum TerminatorStyles
	{
		None = 0,
		CrLf = 1,
		Cr = 2,
		Lf = 4
	}

	private int _lastChar = -1;

	private TerminatorStyles _terminators;

	public TerminatorStyles Terminators => _terminators;

	public PythonUniversalReader(TextReader reader, Encoding encoding, long position)
		: base(reader, encoding, position)
	{
		_terminators = TerminatorStyles.None;
	}

	private int ReadOne()
	{
		if (_lastChar != -1)
		{
			int lastChar = _lastChar;
			_lastChar = -1;
			return lastChar;
		}
		return _reader.Read();
	}

	private int ReadChar()
	{
		int num = ReadOne();
		if (num != -1)
		{
			_position++;
		}
		switch (num)
		{
		case 13:
		{
			int num2 = _reader.Read();
			if (num2 == 10)
			{
				_position++;
				_terminators |= TerminatorStyles.CrLf;
			}
			else
			{
				_lastChar = num2;
				_terminators |= TerminatorStyles.Cr;
			}
			num = 10;
			break;
		}
		case 10:
			_terminators |= TerminatorStyles.Lf;
			break;
		}
		return num;
	}

	public override string Read(int size)
	{
		if (size == 1)
		{
			int num = ReadChar();
			if (num == -1)
			{
				return string.Empty;
			}
			return ScriptingRuntimeHelpers.CharToString((char)num);
		}
		StringBuilder stringBuilder = new StringBuilder(size);
		while (size-- > 0)
		{
			int num2 = ReadChar();
			if (num2 == -1)
			{
				break;
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
			int num = ReadChar();
			if (num == -1)
			{
				break;
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
			num = ReadChar();
			if (num == -1)
			{
				break;
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
			int num = ReadChar();
			if (num == -1)
			{
				break;
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
