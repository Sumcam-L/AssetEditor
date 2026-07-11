using System;
using System.Text;

namespace IronPython.Runtime;

internal class SourceNonStrictDecoderFallbackBuffer : DecoderFallbackBuffer
{
	public override int Remaining
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override bool Fallback(byte[] bytesUnknown, int index)
	{
		throw new BadSourceException(bytesUnknown[index]);
	}

	public override char GetNextChar()
	{
		throw new NotImplementedException();
	}

	public override bool MovePrevious()
	{
		throw new NotImplementedException();
	}
}
