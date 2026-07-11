using System.IO;

namespace Ionic.BZip2;

internal class BitWriter
{
	private uint accumulator;

	private int nAccumulatedBits;

	private Stream output;

	private int totalBytesWrittenOut;

	public byte RemainingBits => (byte)((accumulator >> 32 - nAccumulatedBits) & 0xFF);

	public int NumRemainingBits => nAccumulatedBits;

	public int TotalBytesWrittenOut => totalBytesWrittenOut;

	public BitWriter(Stream s)
	{
		output = s;
	}

	public void Reset()
	{
		accumulator = 0u;
		nAccumulatedBits = 0;
		totalBytesWrittenOut = 0;
		output.Seek(0L, SeekOrigin.Begin);
		output.SetLength(0L);
	}

	public void WriteBits(int nbits, uint value)
	{
		int num = nAccumulatedBits;
		uint num2 = accumulator;
		while (num >= 8)
		{
			output.WriteByte((byte)((num2 >> 24) & 0xFF));
			totalBytesWrittenOut++;
			num2 <<= 8;
			num -= 8;
		}
		accumulator = num2 | (value << 32 - num - nbits);
		nAccumulatedBits = num + nbits;
	}

	public void WriteByte(byte b)
	{
		WriteBits(8, b);
	}

	public void WriteInt(uint u)
	{
		WriteBits(8, (u >> 24) & 0xFF);
		WriteBits(8, (u >> 16) & 0xFF);
		WriteBits(8, (u >> 8) & 0xFF);
		WriteBits(8, u & 0xFF);
	}

	public void Flush()
	{
		WriteBits(0, 0u);
	}

	public void FinishAndPad()
	{
		Flush();
		if (NumRemainingBits > 0)
		{
			byte value = (byte)((accumulator >> 24) & 0xFF);
			output.WriteByte(value);
			totalBytesWrittenOut++;
		}
	}
}
