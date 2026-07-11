using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class PsdBinaryReader : IDisposable
{
	private BinaryReader reader;

	private Encoding encoding;

	private bool disposed = false;

	public Stream BaseStream => reader.BaseStream;

	public PsdBinaryReader(Stream stream, PsdBinaryReader reader)
		: this(stream, reader.encoding)
	{
	}

	public PsdBinaryReader(Stream stream, Encoding encoding)
	{
		this.encoding = encoding;
		reader = new BinaryReader(stream, Encoding.ASCII);
	}

	public byte ReadByte()
	{
		return reader.ReadByte();
	}

	public byte[] ReadBytes(int count)
	{
		return reader.ReadBytes(count);
	}

	public bool ReadBoolean()
	{
		return reader.ReadBoolean();
	}

	public unsafe short ReadInt16()
	{
		short result = reader.ReadInt16();
		Util.SwapBytes((byte*)(&result), 2);
		return result;
	}

	public unsafe int ReadInt32()
	{
		int result = reader.ReadInt32();
		Util.SwapBytes((byte*)(&result), 4);
		return result;
	}

	public unsafe long ReadInt64()
	{
		long result = reader.ReadInt64();
		Util.SwapBytes((byte*)(&result), 8);
		return result;
	}

	public unsafe ushort ReadUInt16()
	{
		ushort result = reader.ReadUInt16();
		Util.SwapBytes((byte*)(&result), 2);
		return result;
	}

	public unsafe uint ReadUInt32()
	{
		uint result = reader.ReadUInt32();
		Util.SwapBytes((byte*)(&result), 4);
		return result;
	}

	public unsafe ulong ReadUInt64()
	{
		ulong result = reader.ReadUInt64();
		Util.SwapBytes((byte*)(&result), 8);
		return result;
	}

	public void ReadPadding(long startPosition, int padMultiple)
	{
		long num = reader.BaseStream.Position - startPosition;
		int padding = Util.GetPadding((int)num, padMultiple);
		ReadBytes(padding);
	}

	public Rectangle ReadRectangle()
	{
		Rectangle result = default(Rectangle);
		result.Y = ReadInt32();
		result.X = ReadInt32();
		result.Height = ReadInt32() - result.Y;
		result.Width = ReadInt32() - result.X;
		return result;
	}

	public string ReadAsciiChars(int count)
	{
		byte[] bytes = reader.ReadBytes(count);
		return Encoding.ASCII.GetString(bytes);
	}

	public string ReadPascalString(int padMultiple)
	{
		long position = reader.BaseStream.Position;
		byte count = ReadByte();
		byte[] bytes = ReadBytes(count);
		ReadPadding(position, padMultiple);
		return encoding.GetString(bytes);
	}

	public string ReadUnicodeString()
	{
		int num = ReadInt32();
		int count = 2 * num;
		byte[] bytes = ReadBytes(count);
		return Encoding.BigEndianUnicode.GetString(bytes, 0, count);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing && reader != null)
			{
				reader.Close();
				reader = null;
			}
			disposed = true;
		}
	}
}
