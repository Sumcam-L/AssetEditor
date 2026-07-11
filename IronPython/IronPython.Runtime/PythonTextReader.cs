using System.IO;
using System.Text;

namespace IronPython.Runtime;

internal abstract class PythonTextReader : PythonStreamReader
{
	protected readonly TextReader _reader;

	protected long _position;

	public override TextReader TextReader => _reader;

	public override long Position
	{
		get
		{
			return _position;
		}
		internal set
		{
			_position = value;
		}
	}

	public PythonTextReader(TextReader reader, Encoding encoding, long position)
		: base(encoding)
	{
		_reader = reader;
		_position = position;
	}

	public override void DiscardBufferedData()
	{
		if (_reader is StreamReader streamReader)
		{
			streamReader.DiscardBufferedData();
		}
	}
}
