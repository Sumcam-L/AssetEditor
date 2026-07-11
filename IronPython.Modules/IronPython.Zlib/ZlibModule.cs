using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ComponentAce.Compression.Libs.ZLib;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Zlib;

public static class ZlibModule
{
	public const string __doc__ = "The functions in this module allow compression and decompression using the\r\nzlib library, which is based on GNU zip.\r\n\r\nadler32(string[, start]) -- Compute an Adler-32 checksum.\r\ncompress(string[, level]) -- Compress string, with compression level in 1-9.\r\ncompressobj([level]) -- Return a compressor object.\r\ncrc32(string[, start]) -- Compute a CRC-32 checksum.\r\ndecompress(string,[wbits],[bufsize]) -- Decompresses a compressed string.\r\ndecompressobj([wbits]) -- Return a decompressor object.\r\n\r\n'wbits' is window buffer size.\r\nCompressor objects support compress() and flush() methods; decompressor\r\nobjects support decompress() and flush().";

	public const string ZLIB_VERSION = "1.2.3";

	internal const int Z_OK = 0;

	internal const int Z_STREAM_END = 1;

	internal const int Z_NEED_DICT = 2;

	internal const int Z_ERRNO = -1;

	internal const int Z_STREAM_ERROR = -2;

	internal const int Z_DATA_ERROR = -3;

	internal const int Z_MEM_ERROR = -4;

	internal const int Z_BUF_ERROR = -5;

	internal const int Z_VERSION_ERROR = -6;

	public const int Z_NO_FLUSH = 0;

	public const int Z_SYNC_FLUSH = 2;

	public const int Z_FULL_FLUSH = 3;

	public const int Z_FINISH = 4;

	public const int Z_BEST_SPEED = 1;

	public const int Z_BEST_COMPRESSION = 9;

	public const int Z_DEFAULT_COMPRESSION = -1;

	public const int Z_FILTERED = 1;

	public const int Z_HUFFMAN_ONLY = 2;

	public const int Z_DEFAULT_STRATEGY = 0;

	public const int DEFLATED = 8;

	public const int DEF_MEM_LEVEL = 8;

	public const int MAX_WBITS = 15;

	internal const int DEFAULTALLOC = 16384;

	public static PythonType error;

	[Documentation("adler32(string[, start]) -- Compute an Adler-32 checksum of string.\r\n\r\nAn optional starting value can be specified.  The returned checksum is\r\na signed integer.")]
	public static int adler32([BytesConversion] IList<byte> data, [DefaultParameterValue(1L)] long baseValue)
	{
		return (int)Adler32.GetAdler32Checksum(baseValue, data.ToArray(), 0, data.Count());
	}

	[Documentation("crc32(string[, start]) -- Compute a CRC-32 checksum of string.\r\n\r\nAn optional starting value can be specified.  The returned checksum is\r\na signed integer.")]
	public static int crc32([BytesConversion] IList<byte> data, [DefaultParameterValue(0L)] long baseValue)
	{
		if (baseValue < int.MinValue || baseValue > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("baseValue");
		}
		if (baseValue >= 0 && baseValue <= uint.MaxValue)
		{
			return PythonBinaryAscii.crc32(data.ToArray(), (uint)baseValue);
		}
		return PythonBinaryAscii.crc32(data.ToArray(), (int)baseValue);
	}

	[Documentation("compress(string[, level]) -- Returned compressed string.\r\n\r\nOptional arg level is the compression level, in 1-9.")]
	public static string compress([BytesConversion] IList<byte> data, [DefaultParameterValue(-1)] int level)
	{
		byte[] array = data.ToArray();
		byte[] array2 = new byte[array.Length + array.Length / 1000 + 12 + 1];
		ZStream zStream = new ZStream();
		zStream.next_in = array;
		zStream.avail_in = array.Length;
		zStream.next_out = array2;
		zStream.avail_out = array2.Length;
		int num = zStream.deflateInit(level);
		switch (num)
		{
		case -2:
			throw PythonOps.CreateThrowable(error, "Bad compression level");
		default:
			zStream.deflateEnd();
			zlib_error(zStream, num, "while compressing data");
			return null;
		case 0:
			num = zStream.deflate(FlushStrategy.Z_FINISH);
			if (num != 1)
			{
				zStream.deflateEnd();
				throw zlib_error(zStream, num, "while compressing data");
			}
			num = zStream.deflateEnd();
			if (num == 0)
			{
				return PythonAsciiEncoding.Instance.GetString(array2, 0, (int)zStream.total_out);
			}
			throw zlib_error(zStream, num, "while finishing compression");
		}
	}

	[Documentation("compressobj([level]) -- Return a compressor object.\r\n\r\nOptional arg level is the compression level, in 1-9.")]
	public static Compress compressobj([DefaultParameterValue(-1)] int level, [DefaultParameterValue(8)] int method, [DefaultParameterValue(15)] int wbits, [DefaultParameterValue(8)] int memlevel, [DefaultParameterValue(0)] int strategy)
	{
		return new Compress(level, method, wbits, memlevel, strategy);
	}

	[Documentation("decompress(string[, wbits[, bufsize]]) -- Return decompressed string.\r\n\r\nOptional arg wbits is the window buffer size.  Optional arg bufsize is\r\nthe initial output buffer size.")]
	public static string decompress([BytesConversion] IList<byte> data, [DefaultParameterValue(15)] int wbits, [DefaultParameterValue(16384)] int bufsize)
	{
		byte[] array = Decompress(data.ToArray(), wbits, bufsize);
		return PythonAsciiEncoding.Instance.GetString(array, 0, array.Length);
	}

	[Documentation("decompressobj([wbits]) -- Return a decompressor object.\r\n\r\nOptional arg wbits is the window buffer size.")]
	public static Decompress decompressobj([DefaultParameterValue(15)] int wbits)
	{
		return new Decompress(wbits);
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		error = context.EnsureModuleException("zlib.error", PythonExceptions.Exception, dict, "error", "zlib");
	}

	internal static Exception MakeError(params object[] args)
	{
		return PythonOps.CreateThrowable(error, args);
	}

	internal static Exception zlib_error(ZStream zst, int err, string msg)
	{
		string text = zst.msg;
		if (text == null)
		{
			switch (err)
			{
			case -5:
				text = "incomplete or truncated stream";
				break;
			case -2:
				text = "inconsistent stream state";
				break;
			case -3:
				text = "invalid input data";
				break;
			}
		}
		if (text == null)
		{
			return MakeError($"Error {err} {msg}");
		}
		return MakeError($"Error {err} {msg}: {text}");
	}

	[PythonHidden]
	internal static byte[] Decompress(byte[] input, int wbits = 15, int bufsize = 16384)
	{
		byte[] array = new byte[bufsize];
		byte[] array2 = new byte[bufsize];
		int num = 0;
		ZStream zStream = new ZStream();
		zStream.next_in = input;
		zStream.avail_in = input.Length;
		zStream.next_out = array;
		zStream.avail_out = array.Length;
		int num2 = zStream.inflateInit(wbits);
		if (num2 != 0)
		{
			zStream.inflateEnd();
			throw zlib_error(zStream, num2, "while preparing to decompress data");
		}
		do
		{
			num2 = zStream.inflate(FlushStrategy.Z_FINISH);
			if (num2 != 1)
			{
				if (num2 == -5 && zStream.avail_out > 0)
				{
					zStream.inflateEnd();
					throw zlib_error(zStream, num2, "while decompressing data");
				}
				if (num2 != 0 && (num2 != -5 || zStream.avail_out != 0))
				{
					zStream.inflateEnd();
					throw zlib_error(zStream, num2, "while decompressing data");
				}
				if (num + array.Length > array2.Length)
				{
					Array.Resize(ref array2, array2.Length * 2);
				}
				Array.Copy(array, 0, array2, num, array.Length);
				num += array.Length;
				zStream.next_out = array;
				zStream.avail_out = array.Length;
				zStream.next_out_index = 0;
			}
		}
		while (num2 != 1);
		num2 = zStream.inflateEnd();
		if (num2 != 0)
		{
			throw zlib_error(zStream, num2, "while finishing data decompression");
		}
		if (num + array.Length - zStream.avail_out > array2.Length)
		{
			Array.Resize(ref array2, array2.Length * 2);
		}
		Array.Copy(array, 0, array2, num, array.Length - zStream.avail_out);
		num += array.Length - zStream.avail_out;
		return array2.Take(num).ToArray();
	}
}
