using System;
using System.IO;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class RleReader
{
	private Stream stream;

	public RleReader(Stream stream)
	{
		this.stream = stream;
	}

	public unsafe int Read(byte[] buffer, int offset, int count)
	{
		if (!Util.CheckBufferBounds(buffer, offset, count))
		{
			throw new ArgumentOutOfRangeException();
		}
		fixed (byte* ptr = &buffer[0])
		{
			int num = count;
			int num2 = offset;
			while (num > 0)
			{
				sbyte b = (sbyte)stream.ReadByte();
				if (b > 0)
				{
					int num3 = b + 1;
					if (num < num3)
					{
						throw new RleException("Raw packet overruns the decode window.");
					}
					stream.Read(buffer, num2, num3);
					num2 += num3;
					num -= num3;
				}
				else if (b > sbyte.MinValue)
				{
					int num4 = 1 - b;
					byte b2 = (byte)stream.ReadByte();
					if (num4 > num)
					{
						throw new RleException("RLE packet overruns the decode window.");
					}
					byte* ptr2 = ptr + num2;
					for (byte* ptr3 = ptr2 + num4; ptr2 < ptr3; ptr2++)
					{
						*ptr2 = b2;
					}
					num2 += num4;
					num -= num4;
				}
			}
			return count - num;
		}
	}
}
