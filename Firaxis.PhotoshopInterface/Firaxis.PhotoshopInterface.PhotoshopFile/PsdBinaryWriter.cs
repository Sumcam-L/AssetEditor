using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class PsdBinaryWriter : IDisposable
{
	private BinaryWriter writer;

	private Encoding encoding;

	private bool disposed = false;

	public Stream BaseStream => writer.BaseStream;

	public bool AutoFlush { get; set; }

	public PsdBinaryWriter(Stream stream, Encoding encoding)
	{
		this.encoding = encoding;
		writer = new BinaryWriter(stream, Encoding.ASCII);
	}

	public void Flush()
	{
		writer.Flush();
	}

	public void Write(Rectangle rect)
	{
		Write(rect.Top);
		Write(rect.Left);
		Write(rect.Bottom);
		Write(rect.Right);
	}

	public void WritePadding(long startPosition, int padMultiple)
	{
		long num = writer.BaseStream.Position - startPosition;
		int padding = Util.GetPadding((int)num, padMultiple);
		for (long num2 = 0L; num2 < padding; num2++)
		{
			writer.Write((byte)0);
		}
		if (AutoFlush)
		{
			Flush();
		}
	}

	public void WriteAsciiChars(string s)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		writer.Write(bytes);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public void WritePascalString(string s, int padMultiple, byte maxBytes = byte.MaxValue)
	{
		long position = writer.BaseStream.Position;
		byte[] array = encoding.GetBytes(s);
		if (array.Length > maxBytes)
		{
			byte[] array2 = new byte[maxBytes];
			Array.Copy(array, array2, maxBytes);
			array = array2;
		}
		writer.Write((byte)array.Length);
		writer.Write(array);
		WritePadding(position, padMultiple);
	}

	public void WriteUnicodeString(string s)
	{
		Write(s.Length);
		byte[] bytes = Encoding.BigEndianUnicode.GetBytes(s);
		Write(bytes);
	}

	public void Write(bool value)
	{
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public void Write(byte[] value)
	{
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public void Write(byte value)
	{
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public unsafe void Write(short value)
	{
		Util.SwapBytes2((byte*)(&value));
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public unsafe void Write(int value)
	{
		Util.SwapBytes4((byte*)(&value));
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public unsafe void Write(long value)
	{
		Util.SwapBytes((byte*)(&value), 8);
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public unsafe void Write(ushort value)
	{
		Util.SwapBytes2((byte*)(&value));
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public unsafe void Write(uint value)
	{
		Util.SwapBytes4((byte*)(&value));
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
	}

	public unsafe void Write(ulong value)
	{
		Util.SwapBytes((byte*)(&value), 8);
		writer.Write(value);
		if (AutoFlush)
		{
			Flush();
		}
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
			if (disposing && writer != null)
			{
				writer.Close();
				writer = null;
			}
			disposed = true;
		}
	}
}
