using System.Collections.Generic;
using System.IO;

namespace IronPython.Runtime;

internal class PythonBinaryWriter : PythonStreamWriter
{
	private Stream _stream;

	public override TextWriter TextWriter => null;

	public PythonBinaryWriter(Stream stream)
		: base(null)
	{
		_stream = stream;
	}

	public override int Write(string data)
	{
		byte[] bytes = PythonAsciiEncoding.Instance.GetBytes(data);
		_stream.Write(bytes, 0, bytes.Length);
		return bytes.Length;
	}

	public override int WriteBytes(IList<byte> data)
	{
		int count = data.Count;
		for (int i = 0; i < count; i++)
		{
			_stream.WriteByte(data[i]);
		}
		return count;
	}

	public override void Flush()
	{
		_stream.Flush();
	}
}
