using System.Text;
using IronPython.Runtime.Operations;

namespace IronPython.Modules;

internal class ExceptionFallbackBuffer : DecoderFallbackBuffer
{
	internal byte[] badBytes;

	private byte[] inputBytes;

	public override int Remaining => 0;

	public ExceptionFallbackBuffer(byte[] bytes)
	{
		inputBytes = bytes;
	}

	public override bool Fallback(byte[] bytesUnknown, int index)
	{
		if (index > 0 && index + bytesUnknown.Length != inputBytes.Length)
		{
			throw PythonOps.UnicodeEncodeError("failed to decode bytes at index {0}", index);
		}
		badBytes = bytesUnknown;
		return false;
	}

	public override char GetNextChar()
	{
		return ' ';
	}

	public override bool MovePrevious()
	{
		return false;
	}
}
