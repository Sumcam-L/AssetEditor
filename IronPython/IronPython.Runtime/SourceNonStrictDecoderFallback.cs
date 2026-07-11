using System.Text;

namespace IronPython.Runtime;

internal class SourceNonStrictDecoderFallback : DecoderFallback
{
	public override int MaxCharCount => 1;

	public override DecoderFallbackBuffer CreateFallbackBuffer()
	{
		return new SourceNonStrictDecoderFallbackBuffer();
	}
}
