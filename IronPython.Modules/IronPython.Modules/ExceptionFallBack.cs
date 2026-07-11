using System.Text;

namespace IronPython.Modules;

internal class ExceptionFallBack : DecoderFallback
{
	internal ExceptionFallbackBuffer buffer;

	public override int MaxCharCount => 100;

	public ExceptionFallBack(byte[] bytes)
	{
		buffer = new ExceptionFallbackBuffer(bytes);
	}

	public override DecoderFallbackBuffer CreateFallbackBuffer()
	{
		return buffer;
	}
}
