using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Ionic.BZip2;

public class BZip2OutputStream : Stream
{
	[Flags]
	private enum TraceBits : uint
	{
		None = 0u,
		Crc = 1u,
		Write = 2u,
		All = uint.MaxValue
	}

	private int totalBytesWrittenIn;

	private bool leaveOpen;

	private BZip2Compressor compressor;

	private uint combinedCRC;

	private Stream output;

	private BitWriter bw;

	private int blockSize100k;

	private TraceBits desiredTrace = TraceBits.Crc | TraceBits.Write;

	public int BlockSize => blockSize100k;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite
	{
		get
		{
			if (output == null)
			{
				throw new ObjectDisposedException("BZip2Stream");
			}
			return output.CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Position
	{
		get
		{
			return totalBytesWrittenIn;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public BZip2OutputStream(Stream output)
		: this(output, BZip2.MaxBlockSize, leaveOpen: false)
	{
	}

	public BZip2OutputStream(Stream output, int blockSize)
		: this(output, blockSize, leaveOpen: false)
	{
	}

	public BZip2OutputStream(Stream output, bool leaveOpen)
		: this(output, BZip2.MaxBlockSize, leaveOpen)
	{
	}

	public BZip2OutputStream(Stream output, int blockSize, bool leaveOpen)
	{
		if (blockSize < BZip2.MinBlockSize || blockSize > BZip2.MaxBlockSize)
		{
			string message = $"blockSize={blockSize} is out of range; must be between {BZip2.MinBlockSize} and {BZip2.MaxBlockSize}";
			throw new ArgumentException(message, "blockSize");
		}
		this.output = output;
		if (!this.output.CanWrite)
		{
			throw new ArgumentException("The stream is not writable.", "output");
		}
		bw = new BitWriter(this.output);
		blockSize100k = blockSize;
		compressor = new BZip2Compressor(bw, blockSize);
		this.leaveOpen = leaveOpen;
		combinedCRC = 0u;
		EmitHeader();
	}

	public override void Close()
	{
		if (output != null)
		{
			Stream stream = output;
			Finish();
			if (!leaveOpen)
			{
				stream.Close();
			}
		}
	}

	public override void Flush()
	{
		if (output != null)
		{
			bw.Flush();
			output.Flush();
		}
	}

	private void EmitHeader()
	{
		byte[] array = new byte[4] { 66, 90, 104, 0 };
		array[3] = (byte)(48 + blockSize100k);
		byte[] array2 = array;
		output.Write(array2, 0, array2.Length);
	}

	private void EmitTrailer()
	{
		bw.WriteByte(23);
		bw.WriteByte(114);
		bw.WriteByte(69);
		bw.WriteByte(56);
		bw.WriteByte(80);
		bw.WriteByte(144);
		bw.WriteInt(combinedCRC);
		bw.FinishAndPad();
	}

	private void Finish()
	{
		try
		{
			_ = bw.TotalBytesWrittenOut;
			compressor.CompressAndWrite();
			combinedCRC = (combinedCRC << 1) | (combinedCRC >> 31);
			combinedCRC ^= compressor.Crc32;
			EmitTrailer();
		}
		finally
		{
			output = null;
			compressor = null;
			bw = null;
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (offset < 0)
		{
			throw new IndexOutOfRangeException($"offset ({offset}) must be > 0");
		}
		if (count < 0)
		{
			throw new IndexOutOfRangeException($"count ({count}) must be > 0");
		}
		if (offset + count > buffer.Length)
		{
			throw new IndexOutOfRangeException($"offset({offset}) count({count}) bLength({buffer.Length})");
		}
		if (output == null)
		{
			throw new IOException("the stream is not open");
		}
		if (count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = count;
		do
		{
			int num3 = compressor.Fill(buffer, offset, num2);
			if (num3 != num2)
			{
				_ = bw.TotalBytesWrittenOut;
				compressor.CompressAndWrite();
				combinedCRC = (combinedCRC << 1) | (combinedCRC >> 31);
				combinedCRC ^= compressor.Crc32;
				offset += num3;
			}
			num2 -= num3;
			num += num3;
		}
		while (num2 > 0);
		totalBytesWrittenIn += num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

	[Conditional("Trace")]
	private void TraceOutput(TraceBits bits, string format, params object[] varParams)
	{
		if ((bits & desiredTrace) != TraceBits.None)
		{
			int hashCode = Thread.CurrentThread.GetHashCode();
			Console.ForegroundColor = (ConsoleColor)(hashCode % 8 + 10);
			Console.Write("{0:000} PBOS ", hashCode);
			Console.WriteLine(format, varParams);
			Console.ResetColor();
		}
	}
}
