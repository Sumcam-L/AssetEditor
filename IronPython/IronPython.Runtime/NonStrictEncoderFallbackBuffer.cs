using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

internal class NonStrictEncoderFallbackBuffer : EncoderFallbackBuffer
{
	private List<char> _buffer = new List<char>();

	private int _index;

	public override int Remaining => _buffer.Count - _index;

	public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
	{
		throw PythonOps.UnicodeEncodeError("'ascii' codec can't encode character '\\u{0:X}{1:04X}' in position {1}", (int)charUnknownHigh, (int)charUnknownLow, index);
	}

	public override bool Fallback(char charUnknown, int index)
	{
		if (charUnknown > 'ÿ')
		{
			throw PythonOps.UnicodeEncodeError("'ascii' codec can't encode character '\\u{0:X}' in position {1}", (int)charUnknown, index);
		}
		_buffer.Add(charUnknown);
		return true;
	}

	public override char GetNextChar()
	{
		return _buffer[_index++];
	}

	public override bool MovePrevious()
	{
		if (_index > 0)
		{
			_index--;
			return true;
		}
		return false;
	}
}
