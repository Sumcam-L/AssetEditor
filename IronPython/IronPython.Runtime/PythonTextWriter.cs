using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IronPython.Runtime;

internal class PythonTextWriter : PythonStreamWriter
{
	private TextWriter _writer;

	private readonly string _eoln;

	public override TextWriter TextWriter => _writer;

	public PythonTextWriter(TextWriter writer, string eoln)
		: base(writer.Encoding)
	{
		_writer = writer;
		_eoln = eoln;
	}

	public override int Write(string data)
	{
		if (_eoln != null)
		{
			data = data.Replace("\n", _eoln);
		}
		_writer.Write(data);
		return data.Length;
	}

	public override int WriteBytes(IList<byte> data)
	{
		int count = data.Count;
		StringBuilder stringBuilder = new StringBuilder((_eoln.Length > 1) ? ((int)((double)count * 1.2)) : count);
		for (int i = 0; i < count; i++)
		{
			char c = (char)data[i];
			if (c == '\n')
			{
				stringBuilder.Append(_eoln);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		_writer.Write(stringBuilder.ToString());
		return count;
	}

	public override void Flush()
	{
		_writer.Flush();
	}
}
