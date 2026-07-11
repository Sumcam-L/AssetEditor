using System;
using System.Collections.Generic;
using System.IO;

namespace SharpDX.Multimedia;

public class SoundStream : Stream
{
	private readonly bool isOwnerOfInput;

	private Stream input;

	private long startPositionOfData;

	private long length;

	public uint[] DecodedPacketsInfo { get; private set; }

	public WaveFormat Format { get; protected set; }

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Position
	{
		get
		{
			return input.Position - startPositionOfData;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	private string FileFormatName { get; set; }

	public override long Length => length;

	public SoundStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		input = stream;
		Initialize(stream);
	}

	private unsafe void Initialize(Stream stream)
	{
		RiffParser riffParser = new RiffParser(stream);
		FileFormatName = "Unknown";
		if (!riffParser.MoveNext() || riffParser.Current == null)
		{
			ThrowInvalidFileFormat();
			return;
		}
		FileFormatName = riffParser.Current.Type;
		if (FileFormatName != "WAVE" && FileFormatName != "XWMA")
		{
			throw new InvalidOperationException("Unsupported " + FileFormatName + " file format. Only WAVE or XWMA");
		}
		riffParser.Descend();
		IList<RiffChunk> allChunks = riffParser.GetAllChunks();
		RiffChunk riffChunk = Chunk(allChunks, "fmt ");
		if (riffChunk.Size < sizeof(WaveFormat.__PcmNative))
		{
			ThrowInvalidFileFormat();
		}
		try
		{
			Format = WaveFormat.MarshalFrom(riffChunk.GetData());
		}
		catch (InvalidOperationException nestedException)
		{
			ThrowInvalidFileFormat(nestedException);
		}
		if (FileFormatName == "XWMA")
		{
			if (Format.Encoding != WaveFormatEncoding.Wmaudio2 && Format.Encoding != WaveFormatEncoding.Wmaudio3)
			{
				ThrowInvalidFileFormat();
			}
			RiffChunk riffChunk2 = Chunk(allChunks, "dpds");
			DecodedPacketsInfo = riffChunk2.GetDataAsArray<uint>();
		}
		else
		{
			switch (Format.Encoding)
			{
			default:
				ThrowInvalidFileFormat();
				break;
			case WaveFormatEncoding.Extensible:
			case WaveFormatEncoding.Pcm:
			case WaveFormatEncoding.Adpcm:
			case WaveFormatEncoding.IeeeFloat:
				break;
			}
		}
		RiffChunk riffChunk3 = Chunk(allChunks, "data");
		startPositionOfData = riffChunk3.DataPosition;
		length = riffChunk3.Size;
		input.Position = startPositionOfData;
	}

	protected void ThrowInvalidFileFormat(Exception nestedException = null)
	{
		throw new InvalidOperationException("Invalid " + FileFormatName + " file format", nestedException);
	}

	public DataStream ToDataStream()
	{
		byte[] array = new byte[Length];
		if (Read(array, 0, (int)Length) != Length)
		{
			throw new InvalidOperationException("Unable to get a valid DataStream");
		}
		return DataStream.Create(array, canRead: true, canWrite: true);
	}

	public static implicit operator DataStream(SoundStream stream)
	{
		return stream.ToDataStream();
	}

	protected override void Dispose(bool disposing)
	{
		if (input != null)
		{
			input.Dispose();
			input = null;
		}
		base.Dispose(disposing);
	}

	protected RiffChunk Chunk(IEnumerable<RiffChunk> chunks, string id)
	{
		RiffChunk riffChunk = null;
		foreach (RiffChunk chunk in chunks)
		{
			if (chunk.Type == (FourCC)id)
			{
				riffChunk = chunk;
				break;
			}
		}
		if (riffChunk == null || riffChunk.Type != (FourCC)id)
		{
			throw new InvalidOperationException("Invalid " + FileFormatName + " file format");
		}
		return riffChunk;
	}

	public override void Flush()
	{
		throw new NotSupportedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long num = input.Position;
		switch (origin)
		{
		case SeekOrigin.Begin:
			num = startPositionOfData + offset;
			break;
		case SeekOrigin.Current:
			num = input.Position + offset;
			break;
		case SeekOrigin.End:
			num = startPositionOfData + length + offset;
			break;
		}
		if (num < startPositionOfData || num > startPositionOfData + length)
		{
			throw new InvalidOperationException("Cannot seek outside the range of this stream");
		}
		return input.Seek(num, SeekOrigin.Begin);
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return input.Read(buffer, offset, Math.Min(count, (int)Math.Max(startPositionOfData + length - input.Position, 0L)));
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
