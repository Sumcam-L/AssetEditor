using System;
using System.IO;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class RleWriter
{
	private int maxPacketLength = 128;

	private object rleLock;

	private Stream stream;

	private byte[] data;

	private int offset;

	private bool rlePacket;

	private int packetLength;

	private int idxDataRawPacket;

	private byte lastValue;

	public RleWriter(Stream stream)
	{
		rleLock = new object();
		this.stream = stream;
	}

	public unsafe int Write(byte[] data, int offset, int count)
	{
		if (!Util.CheckBufferBounds(data, offset, count))
		{
			throw new ArgumentOutOfRangeException();
		}
		if (count == 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		lock (rleLock)
		{
			long position = stream.Position;
			this.data = data;
			this.offset = offset;
			fixed (byte* ptr = &data[0])
			{
				byte* ptr2 = ptr + offset;
				byte* ptrEnd = ptr2 + count;
				int num = EncodeToStream(ptr2, ptrEnd);
			}
			return (int)(stream.Position - position);
		}
	}

	private void ClearPacket()
	{
		rlePacket = false;
		packetLength = 0;
	}

	private void WriteRlePacket()
	{
		byte value = (byte)(1 - packetLength);
		stream.WriteByte(value);
		stream.WriteByte(lastValue);
	}

	private void WriteRawPacket()
	{
		byte value = (byte)(packetLength - 1);
		stream.WriteByte(value);
		stream.Write(data, idxDataRawPacket, packetLength);
	}

	private void WritePacket()
	{
		if (rlePacket)
		{
			WriteRlePacket();
		}
		else
		{
			WriteRawPacket();
		}
	}

	private unsafe int EncodeToStream(byte* ptr, byte* ptrEnd)
	{
		idxDataRawPacket = offset;
		rlePacket = false;
		lastValue = *ptr;
		packetLength = 1;
		ptr++;
		int num = 1;
		while (ptr < ptrEnd)
		{
			byte b = *ptr;
			if (packetLength == 1)
			{
				rlePacket = b == lastValue;
				lastValue = b;
				packetLength = 2;
			}
			else if (packetLength == maxPacketLength)
			{
				WritePacket();
				rlePacket = false;
				lastValue = b;
				idxDataRawPacket = offset + num;
				packetLength = 1;
			}
			else if (rlePacket)
			{
				if (b == lastValue)
				{
					packetLength++;
				}
				else
				{
					WriteRlePacket();
					rlePacket = false;
					lastValue = b;
					idxDataRawPacket = offset + num;
					packetLength = 1;
				}
			}
			else if (b == lastValue)
			{
				packetLength--;
				WriteRawPacket();
				rlePacket = true;
				packetLength = 2;
			}
			else
			{
				lastValue = b;
				packetLength++;
			}
			ptr++;
			num++;
		}
		WritePacket();
		ClearPacket();
		return num;
	}
}
