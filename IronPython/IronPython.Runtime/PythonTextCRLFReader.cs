using System.IO;
using System.Text;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class PythonTextCRLFReader : PythonTextReader
{
	private char[] _buffer = new char[160];

	private int _bufPos;

	private int _bufLen;

	public PythonTextCRLFReader(TextReader reader, Encoding encoding, long position)
		: base(reader, encoding, position)
	{
	}

	private int Read()
	{
		if (_bufPos >= _bufLen && ReadBuffer() == 0)
		{
			return -1;
		}
		_position++;
		return _buffer[_bufPos++];
	}

	private int Peek()
	{
		if (_bufPos >= _bufLen && ReadBuffer() == 0)
		{
			return -1;
		}
		return _buffer[_bufPos];
	}

	private int ReadBuffer()
	{
		_bufLen = _reader.Read(_buffer, 0, _buffer.Length);
		_bufPos = 0;
		return _bufLen;
	}

	public override string Read(int size)
	{
		if (size == 1)
		{
			int num = Read();
			switch (num)
			{
			case -1:
				return string.Empty;
			case 13:
				if (Peek() == 10)
				{
					num = Read();
				}
				break;
			}
			return ScriptingRuntimeHelpers.CharToString((char)num);
		}
		StringBuilder stringBuilder;
		int num2;
		for (stringBuilder = new StringBuilder(size); size-- > 0; stringBuilder.Append((char)num2))
		{
			num2 = Read();
			switch (num2)
			{
			case 13:
				if (Peek() == 10)
				{
					num2 = Read();
				}
				continue;
			default:
				continue;
			case -1:
				break;
			}
			break;
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
			int num = Read();
			switch (num)
			{
			case 13:
				if (Peek() == 10)
				{
					num = Read();
				}
				break;
			case -1:
				if (stringBuilder.Length == 0)
				{
					return string.Empty;
				}
				return stringBuilder.ToString();
			}
			stringBuilder.Append((char)num);
		}
	}

	public override string ReadLine()
	{
		return ReadLine(int.MaxValue);
	}

	public override string ReadLine(int size)
	{
		StringBuilder stringBuilder = null;
		if (_bufPos >= _bufLen)
		{
			ReadBuffer();
		}
		if (_bufLen == 0)
		{
			return string.Empty;
		}
		int num = _bufPos;
		int num2 = 0;
		int lenAdj = 0;
		char c;
		do
		{
			if (num >= _bufLen)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder((num - _bufPos) * 2);
				}
				stringBuilder.Append(_buffer, _bufPos, num - _bufPos);
				if (ReadBuffer() == 0)
				{
					return stringBuilder.ToString();
				}
				num = 0;
			}
			c = _buffer[num++];
			if (c == '\r')
			{
				if (num < _bufLen)
				{
					if (_buffer[num] == '\n')
					{
						_position++;
						c = _buffer[num++];
						lenAdj = 2;
					}
				}
				else if (_reader.Peek() == 10)
				{
					c = (char)_reader.Read();
					lenAdj = 1;
				}
			}
			_position++;
		}
		while (c != '\n' && ++num2 < size);
		return FinishString(stringBuilder, num, lenAdj);
	}

	private string FinishString(StringBuilder sb, int curIndex, int lenAdj)
	{
		int num = curIndex - _bufPos;
		int bufPos = _bufPos;
		_bufPos = curIndex;
		if (sb != null)
		{
			if (lenAdj != 0)
			{
				sb.Append(_buffer, bufPos, num - lenAdj);
				sb.Append('\n');
			}
			else
			{
				sb.Append(_buffer, bufPos, num);
			}
			return sb.ToString();
		}
		if (lenAdj != 0)
		{
			return new string(_buffer, bufPos, num - lenAdj) + "\n";
		}
		return new string(_buffer, bufPos, num);
	}

	public override void DiscardBufferedData()
	{
		_bufPos = (_bufLen = 0);
		base.DiscardBufferedData();
	}
}
