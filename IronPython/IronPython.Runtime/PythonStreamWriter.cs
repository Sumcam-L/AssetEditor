using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IronPython.Runtime;

internal abstract class PythonStreamWriter
{
	protected Encoding _encoding;

	public Encoding Encoding => _encoding;

	public abstract TextWriter TextWriter { get; }

	public PythonStreamWriter(Encoding encoding)
	{
		_encoding = encoding;
	}

	public abstract int Write(string data);

	public abstract int WriteBytes(IList<byte> data);

	public abstract void Flush();
}
