namespace SharpDX.Text;

public class ASCIIEncoding : Encoding
{
	public override int GetByteCount(char[] chars, int index, int count)
	{
		return count;
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		for (int i = 0; i < charCount; i++)
		{
			bytes[byteIndex + i] = (byte)(chars[charIndex + i] & 0x7F);
		}
		return charCount;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		return count;
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		for (int i = 0; i < byteCount; i++)
		{
			chars[charIndex + i] = (char)(bytes[byteIndex + i] & 0x7F);
		}
		return byteCount;
	}

	public override int GetMaxByteCount(int charCount)
	{
		return charCount;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}

	public new string GetString(byte[] bytes)
	{
		return base.GetString(bytes, 0, bytes.Length);
	}
}
