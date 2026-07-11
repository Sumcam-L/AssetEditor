using System;
using System.Text;

namespace IronPython.Runtime;

[Serializable]
internal sealed class PythonAsciiEncoding : Encoding
{
	internal static readonly Encoding Instance = MakeNonThrowing();

	internal static readonly Encoding SourceEncoding = MakeSourceEncoding();

	public override string WebName => "ascii";

	public override string EncodingName => "ascii";

	internal PythonAsciiEncoding()
	{
	}

	internal static Encoding MakeNonThrowing()
	{
		Encoding encoding = new PythonAsciiEncoding();
		encoding = (Encoding)encoding.Clone();
		encoding.DecoderFallback = new NonStrictDecoderFallback();
		encoding.EncoderFallback = new NonStrictEncoderFallback();
		return encoding;
	}

	private static Encoding MakeSourceEncoding()
	{
		Encoding encoding = new PythonAsciiEncoding();
		encoding = (Encoding)encoding.Clone();
		encoding.DecoderFallback = new SourceNonStrictDecoderFallback();
		return encoding;
	}

	public override int GetByteCount(char[] chars, int index, int count)
	{
		int num = 0;
		int num2 = index + count;
		while (index < num2)
		{
			char c = chars[index];
			if (c > '\u007f')
			{
				EncoderFallbackBuffer encoderFallbackBuffer = base.EncoderFallback.CreateFallbackBuffer();
				if (encoderFallbackBuffer.Fallback(c, index))
				{
					num += encoderFallbackBuffer.Remaining;
				}
			}
			else
			{
				num++;
			}
			index++;
		}
		return num;
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		int num = charIndex + charCount;
		int num2 = 0;
		while (charIndex < num)
		{
			char c = chars[charIndex];
			if (c > '\u007f')
			{
				EncoderFallbackBuffer encoderFallbackBuffer = base.EncoderFallback.CreateFallbackBuffer();
				if (encoderFallbackBuffer.Fallback(c, charIndex))
				{
					while (encoderFallbackBuffer.Remaining != 0)
					{
						bytes[byteIndex++] = (byte)encoderFallbackBuffer.GetNextChar();
						num2++;
					}
				}
			}
			else
			{
				bytes[byteIndex++] = (byte)c;
				num2++;
			}
			charIndex++;
		}
		return num2;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		int num = index + count;
		int num2 = 0;
		while (index < num)
		{
			byte b = bytes[index];
			if (b > 127)
			{
				DecoderFallbackBuffer decoderFallbackBuffer = base.DecoderFallback.CreateFallbackBuffer();
				if (decoderFallbackBuffer.Fallback(new byte[1] { b }, 0))
				{
					num2 += decoderFallbackBuffer.Remaining;
				}
			}
			else
			{
				num2++;
			}
			index++;
		}
		return num2;
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		int num = byteIndex + byteCount;
		int num2 = 0;
		while (byteIndex < num)
		{
			byte b = bytes[byteIndex];
			if (b > 127)
			{
				DecoderFallbackBuffer decoderFallbackBuffer = base.DecoderFallback.CreateFallbackBuffer();
				if (decoderFallbackBuffer.Fallback(new byte[1] { b }, 0))
				{
					while (decoderFallbackBuffer.Remaining != 0)
					{
						chars[charIndex++] = decoderFallbackBuffer.GetNextChar();
						num2++;
					}
				}
			}
			else
			{
				chars[charIndex++] = (char)b;
				num2++;
			}
			byteIndex++;
		}
		return num2;
	}

	public override int GetMaxByteCount(int charCount)
	{
		return charCount * 4;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}
}
