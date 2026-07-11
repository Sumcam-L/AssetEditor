using System.Collections.Generic;
using System.Text;

namespace IronPython.Runtime;

internal class NonStrictDecoderFallbackBuffer : DecoderFallbackBuffer
{
	private List<byte> _bytes = new List<byte>();

	private int _index;

	public override int Remaining => _bytes.Count - _index;

	public override bool Fallback(byte[] bytesUnknown, int index)
	{
		_bytes.AddRange(bytesUnknown);
		return true;
	}

	public override char GetNextChar()
	{
		if (_index == _bytes.Count)
		{
			return '\0';
		}
		return (char)_bytes[_index++];
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
