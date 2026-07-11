using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Ionic.BZip2;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules.Bz2;

public static class Bz2Module
{
	[PythonType]
	public class BZ2Compressor
	{
		public const string __doc__ = "BZ2Compressor([compresslevel=9]) -> compressor object\r\n\r\nCreate a new compressor object. This object may be used to compress\r\ndata sequentially. If you want to compress data in one shot, use the\r\ncompress() function instead. The compresslevel parameter, if given,\r\nmust be a number between 1 and 9.\r\n";

		private int compresslevel;

		private MemoryStream output;

		private BZip2OutputStream bz2Output;

		private long lastPosition;

		public BZ2Compressor([DefaultParameterValue(9)] int compresslevel)
		{
			this.compresslevel = compresslevel;
			output = new MemoryStream();
			bz2Output = new BZip2OutputStream(output, leaveOpen: true);
		}

		[Documentation("compress(data) -> string\r\n\r\nProvide more data to the compressor object. It will return chunks of\r\ncompressed data whenever possible. When you've finished providing data\r\nto compress, call the flush() method to finish the compression process,\r\nand return what is left in the internal buffers.\r\n")]
		public Bytes compress([BytesConversion] IList<byte> data)
		{
			byte[] array = data.ToArrayNoCopy();
			bz2Output.Write(array, 0, array.Length);
			return new Bytes(GetLatestData());
		}

		[Documentation("flush() -> string\r\n\r\nFinish the compression process and return what is left in internal buffers.\r\nYou must not use the compressor object after calling this method.\r\n")]
		public Bytes flush()
		{
			bz2Output.Close();
			return new Bytes(GetLatestData());
		}

		private byte[] GetLatestData()
		{
			long num = output.Position - lastPosition;
			byte[] array = new byte[num];
			if (num > 0)
			{
				Array.Copy(output.GetBuffer(), lastPosition, array, 0L, num);
				lastPosition = output.Position;
			}
			return array;
		}
	}

	[PythonType]
	public class BZ2Decompressor
	{
		public const string __doc__ = "BZ2Decompressor() -> decompressor object\r\n\r\nCreate a new decompressor object. This object may be used to decompress\r\ndata sequentially. If you want to decompress data in one shot, use the\r\ndecompress() function instead.\r\n";

		private MemoryStream input;

		private BZip2InputStream bz2Input;

		private long lastSuccessfulPosition;

		private bool _finished;

		public Bytes unused_data
		{
			get
			{
				byte[] buffer = input.GetBuffer();
				long num = input.Length - lastSuccessfulPosition;
				byte[] array = new byte[num];
				Array.Copy(buffer, lastSuccessfulPosition, array, 0L, num);
				return new Bytes(array);
			}
		}

		[Documentation("decompress(data) -> string\r\n\r\nProvide more data to the decompressor object. It will return chunks\r\nof decompressed data whenever possible. If you try to decompress data\r\nafter the end of stream is found, EOFError will be raised. If any data\r\nwas found after the end of stream, it'll be ignored and saved in\r\nunused_data attribute.\r\n")]
		public Bytes decompress([BytesConversion] IList<byte> data)
		{
			if (_finished)
			{
				throw PythonOps.EofError("End of stream was already found");
			}
			byte[] array = data.ToArrayNoCopy();
			if (!InitializeMemoryStream(array))
			{
				AddData(array);
			}
			List<byte> list = new List<byte>();
			if (InitializeBZ2Stream())
			{
				long position = input.Position;
				object o = bz2Input.DumpState();
				try
				{
					int num;
					while ((num = bz2Input.ReadByte()) != -1)
					{
						list.Add((byte)num);
						position = input.Position;
						o = bz2Input.DumpState();
					}
					lastSuccessfulPosition = input.Position;
					_finished = true;
				}
				catch (IOException)
				{
					input.Position = position;
					bz2Input.RestoreState(o);
				}
			}
			return new Bytes(list);
		}

		private bool InitializeMemoryStream(byte[] data)
		{
			if (input != null)
			{
				return false;
			}
			input = new MemoryStream();
			input.Write(data, 0, data.Length);
			input.Position = 0L;
			return true;
		}

		private bool InitializeBZ2Stream()
		{
			if (bz2Input != null)
			{
				return true;
			}
			try
			{
				bz2Input = new BZip2InputStream(input, leaveOpen: true);
				return true;
			}
			catch (IOException)
			{
				input.Position = lastSuccessfulPosition;
				return false;
			}
		}

		private void AddData(byte[] bytes)
		{
			long position = input.Position;
			input.Position = input.Length;
			input.Write(bytes.ToArray(), 0, bytes.Length);
			input.Position = position;
		}
	}

	[PythonType]
	public class BZ2File : PythonFile
	{
		public const string __doc__ = "BZ2File(name [, mode='r', buffering=0, compresslevel=9]) -> file object\r\n\r\nOpen a bz2 file. The mode can be 'r' or 'w', for reading (default) or\r\nwriting. When opened for writing, the file will be created if it doesn't\r\nexist, and truncated otherwise. If the buffering argument is given, 0 means\r\nunbuffered, and larger numbers specify the buffer size. If compresslevel\r\nis given, must be a number between 1 and 9.\r\n";

		private Stream bz2Stream;

		public int buffering { get; private set; }

		public int compresslevel { get; private set; }

		public BZ2File(CodeContext context)
			: base(context)
		{
		}

		public void __init__(CodeContext context, string filename, [DefaultParameterValue("r")] string mode, [DefaultParameterValue(0)] int buffering, [DefaultParameterValue(9)] int compresslevel)
		{
			PythonContext context2 = PythonContext.GetContext(context);
			this.buffering = buffering;
			this.compresslevel = compresslevel;
			if (!mode.Contains("b") && !mode.Contains("U"))
			{
				mode += 'b';
			}
			if (mode.Contains("w"))
			{
				FileStream output = File.Open(filename, FileMode.Create, FileAccess.Write);
				if (mode.Contains("p"))
				{
					bz2Stream = new ParallelBZip2OutputStream(output);
				}
				else
				{
					bz2Stream = new BZip2OutputStream(output);
				}
			}
			else
			{
				bz2Stream = new BZip2InputStream(File.OpenRead(filename));
			}
			__init__(bz2Stream, context2.DefaultEncoding, filename, mode);
		}

		[Documentation("close() -> None or (perhaps) an integer\r\n\r\nClose the file. Sets data attribute .closed to true. A closed file\r\ncannot be used for further I/O operations. close() may be called more\r\nthan once without error.\r\n")]
		public new void close()
		{
			base.close();
		}

		[Documentation("read([size]) -> string\r\n\r\nRead at most size uncompressed bytes, returned as a string. If the size\r\nargument is negative or omitted, read until EOF is reached.\r\n")]
		public new string read()
		{
			ThrowIfClosed();
			return base.read();
		}

		public new string read(int size)
		{
			ThrowIfClosed();
			return base.read(size);
		}

		[Documentation("readline([size]) -> string\r\n\r\nReturn the next line from the file, as a string, retaining newline.\r\nA non-negative size argument will limit the maximum number of bytes to\r\nreturn (an incomplete line may be returned then). Return an empty\r\nstring at EOF.\r\n")]
		public new string readline()
		{
			ThrowIfClosed();
			return base.readline();
		}

		public new string readline(int sizehint)
		{
			ThrowIfClosed();
			return base.readline(sizehint);
		}

		[Documentation("readlines([size]) -> list\r\n\r\nCall readline() repeatedly and return a list of lines read.\r\nThe optional size argument, if given, is an approximate bound on the\r\ntotal number of bytes in the lines returned.\r\n")]
		public new List readlines()
		{
			if (base.closed)
			{
				throw PythonOps.ValueError("I/O operation on closed file");
			}
			return base.readlines();
		}

		public new List readlines(int sizehint)
		{
			if (base.closed)
			{
				throw PythonOps.ValueError("I/O operation on closed file");
			}
			return base.readlines(sizehint);
		}

		[Documentation("xreadlines() -> self\r\n\r\nFor backward compatibility. BZ2File objects now include the performance\r\noptimizations previously implemented in the xreadlines module.\r\n")]
		public new BZ2File xreadlines()
		{
			return this;
		}

		[Documentation("seek(offset [, whence]) -> None\r\n\r\nMove to new file position. Argument offset is a byte count. Optional\r\nargument whence defaults to 0 (offset from start of file, offset\r\nshould be >= 0); other values are 1 (move relative to current position,\r\npositive or negative), and 2 (move relative to end of file, usually\r\nnegative, although many platforms allow seeking beyond the end of a file).\r\n\r\nNote that seeking of bz2 files is emulated, and depending on the parameters\r\nthe operation may be extremely slow.\r\n")]
		public new void seek(long offset, [DefaultParameterValue(0)] int whence)
		{
			throw new NotImplementedException();
		}

		[Documentation("tell() -> int\r\n\r\nReturn the current file position, an integer (may be a long integer).\r\n")]
		public new object tell()
		{
			throw new NotImplementedException();
		}

		[Documentation("write(data) -> None\r\n\r\nWrite the 'data' string to file. Note that due to buffering, close() may\r\nbe needed before the file on disk reflects the data written.\r\n")]
		public new void write([BytesConversion] IList<byte> data)
		{
			ThrowIfClosed();
			base.write(data);
		}

		public new void write(object data)
		{
			ThrowIfClosed();
			base.write(data);
		}

		public new void write(string data)
		{
			ThrowIfClosed();
			base.write(data);
		}

		public new void write(PythonBuffer data)
		{
			ThrowIfClosed();
			base.write(data);
		}

		[Documentation("writelines(sequence_of_strings) -> None\r\n\r\nWrite the sequence of strings to the file. Note that newlines are not\r\nadded. The sequence can be any iterable object producing strings. This is\r\nequivalent to calling write() for each string.\r\n")]
		public new void writelines(object sequence_of_strings)
		{
			ThrowIfClosed();
			base.writelines(sequence_of_strings);
		}

		public void __del__()
		{
			close();
		}

		[Documentation("__enter__() -> self.")]
		public new object __enter__()
		{
			ThrowIfClosed();
			return this;
		}

		[Documentation("__exit__(*excinfo) -> None.  Closes the file.")]
		public new void __exit__(params object[] excinfo)
		{
			close();
		}
	}

	public const string __doc__ = "The python bz2 module provides a comprehensive interface for\r\nthe bz2 compression library. It implements a complete file\r\ninterface, one shot (de)compression functions, and types for\r\nsequential (de)compression.";

	internal const int DEFAULT_COMPRESSLEVEL = 9;

	private const int PARALLEL_THRESHOLD = 10485760;

	[Documentation("compress(data [, compresslevel=9]) -> string\r\n\r\nCompress data in one shot. If you want to compress data sequentially,\r\nuse an instance of BZ2Compressor instead. The compresslevel parameter, if\r\ngiven, must be a number between 1 and 9.\r\n")]
	public static Bytes compress([BytesConversion] IList<byte> data, [DefaultParameterValue(9)] int compresslevel)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (Stream stream = ((data.Count > 10485760) ? ((Stream)new ParallelBZip2OutputStream(memoryStream, leaveOpen: true)) : ((Stream)new BZip2OutputStream(memoryStream, leaveOpen: true))))
		{
			byte[] buffer = data.ToArrayNoCopy();
			stream.Write(buffer, 0, data.Count);
		}
		return Bytes.Make(memoryStream.ToArray());
	}

	[Documentation("decompress(data) -> decompressed data\r\n\r\nDecompress data in one shot. If you want to decompress data sequentially,\r\nuse an instance of BZ2Decompressor instead.\r\n")]
	public static Bytes decompress([BytesConversion] IList<byte> data)
	{
		if (data.Count == 0)
		{
			return new Bytes();
		}
		byte[] array = new byte[1024];
		using MemoryStream memoryStream = new MemoryStream();
		using (MemoryStream input = new MemoryStream(data.ToArrayNoCopy(), writable: false))
		{
			using BZip2InputStream bZip2InputStream = new BZip2InputStream(input);
			int num = 0;
			while (true)
			{
				try
				{
					num = bZip2InputStream.Read(array, 0, array.Length);
				}
				catch (IOException ex)
				{
					throw PythonOps.ValueError(ex.Message);
				}
				if (num > 0)
				{
					memoryStream.Write(array, 0, num);
					continue;
				}
				break;
			}
		}
		return Bytes.Make(memoryStream.ToArray());
	}

	private static byte[] ToArrayNoCopy(this IList<byte> bytes)
	{
		if (bytes is byte[] result)
		{
			return result;
		}
		if (bytes is Bytes bytes2)
		{
			return bytes2.GetUnsafeByteArray();
		}
		return bytes.ToArray();
	}
}
