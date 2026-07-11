using System.Text;

namespace IronPython.Runtime;

internal class NonStrictEncoderFallback : EncoderFallback
{
	public override int MaxCharCount => 1;

	public override EncoderFallbackBuffer CreateFallbackBuffer()
	{
		return new NonStrictEncoderFallbackBuffer();
	}
}
