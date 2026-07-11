using System.IO;
using System.Text;

namespace IronPython.Runtime;

internal abstract class PythonStreamReader
{
	protected Encoding _encoding;

	public Encoding Encoding => _encoding;

	public abstract TextReader TextReader { get; }

	public abstract long Position { get; internal set; }

	public PythonStreamReader(Encoding encoding)
	{
		_encoding = encoding;
	}

	public abstract string Read(int size);

	public abstract string ReadToEnd();

	public abstract string ReadLine();

	public abstract string ReadLine(int size);

	public abstract void DiscardBufferedData();
}
