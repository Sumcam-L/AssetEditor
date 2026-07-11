using System.IO;
using System.Text;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class PythonBinaryReader : PythonStreamReader
{
	private const int BufferSize = 4096;

	private readonly Stream _stream;

	private byte[] _buffer;

	public override TextReader TextReader => null;

	public override long Position
	{
		get
		{
			return _stream.Position;
		}
		internal set
		{
		}
	}

	public PythonBinaryReader(Stream stream)
		: base(null)
	{
		_stream = stream;
	}

	public override string Read(int size)
	{
		byte[] array;
		if (size <= 4096)
		{
			if (_buffer == null)
			{
				_buffer = new byte[4096];
			}
			array = _buffer;
		}
		else
		{
			array = new byte[size];
		}
		int num = size;
		int num2 = 0;
		while (true)
		{
			int num3 = _stream.Read(array, num2, num);
			if (num3 <= 0)
			{
				break;
			}
			num -= num3;
			if (num <= 0)
			{
				break;
			}
			num2 += num3;
		}
		return PackDataIntoString(array, size - num);
	}

	public override string ReadToEnd()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		if (_buffer == null)
		{
			_buffer = new byte[4096];
		}
		while (true)
		{
			int num2 = _stream.Read(_buffer, 0, 4096);
			if (num2 == 0)
			{
				break;
			}
			stringBuilder.Append(PackDataIntoString(_buffer, num2));
			num += num2;
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
			num = _stream.ReadByte();
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
			int num = _stream.ReadByte();
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

	public override void DiscardBufferedData()
	{
	}

	internal static string PackDataIntoString(byte[] data, int count)
	{
		if (count == 1)
		{
			return ScriptingRuntimeHelpers.CharToString((char)data[0]);
		}
		StringBuilder stringBuilder = new StringBuilder(count);
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append((char)data[i]);
		}
		return stringBuilder.ToString();
	}
}
