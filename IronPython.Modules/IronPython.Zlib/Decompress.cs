using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ComponentAce.Compression.Libs.ZLib;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Zlib;

[PythonType]
public class Decompress
{
	private const int Z_OK = 0;

	private const int Z_STREAM_END = 1;

	private const int Z_BUF_ERROR = -5;

	private const int Z_SYNC_FLUSH = 2;

	private const int Z_FINISH = 4;

	private string _unused_data;

	private string _unconsumed_tail;

	private ZStream zst;

	public string unused_data => _unused_data;

	public string unconsumed_tail => _unconsumed_tail;

	internal Decompress(int wbits)
	{
		zst = new ZStream();
		int num = zst.inflateInit(wbits);
		switch (num)
		{
		case -2:
			throw PythonOps.ValueError("Invalid initialization option");
		default:
			throw ZlibModule.zlib_error(zst, num, "while creating decompression object");
		case 0:
			_unused_data = string.Empty;
			_unconsumed_tail = string.Empty;
			break;
		}
	}

	[Documentation("decompress(data, max_length) -- Return a string containing the decompressed\r\nversion of the data.\r\n\r\nAfter calling this function, some of the input data may still be stored in\r\ninternal buffers for later processing.\r\nCall the flush() method to clear these buffers.\r\nIf the max_length parameter is specified then the return value will be\r\nno longer than max_length.  Unconsumed input data will be stored in\r\nthe unconsumed_tail attribute.")]
	public string decompress([BytesConversion] IList<byte> value, [DefaultParameterValue(0)] int max_length)
	{
		if (max_length < 0)
		{
			throw new ArgumentException("max_length must be greater than zero");
		}
		byte[] array = value.ToArray();
		byte[] array2 = new byte[(max_length > 0 && 16384 > max_length) ? max_length : 16384];
		long total_out = zst.total_out;
		zst.next_in = array;
		zst.next_in_index = 0;
		zst.avail_in = array.Length;
		zst.next_out = array2;
		zst.next_out_index = 0;
		zst.avail_out = array2.Length;
		int num = zst.inflate(FlushStrategy.Z_SYNC_FLUSH);
		while (num == 0 && zst.avail_out == 0 && (max_length <= 0 || array2.Length < max_length))
		{
			int avail_out = array2.Length;
			Array.Resize(ref array2, array2.Length * 2);
			zst.next_out = array2;
			zst.avail_out = avail_out;
			num = zst.inflate(FlushStrategy.Z_SYNC_FLUSH);
		}
		if (max_length > 0)
		{
			_unconsumed_tail = PythonAsciiEncoding.Instance.GetString(zst.next_in, zst.next_in_index, zst.avail_in);
		}
		switch (num)
		{
		case 1:
			_unused_data += PythonAsciiEncoding.Instance.GetString(zst.next_in, zst.next_in_index, zst.avail_in);
			break;
		default:
			throw ZlibModule.zlib_error(zst, num, "while decompressing");
		case -5:
		case 0:
			break;
		}
		return PythonAsciiEncoding.Instance.GetString(array2, 0, (int)(zst.total_out - total_out));
	}

	[Documentation("flush( [length] ) -- Return a string containing any remaining\r\ndecompressed data. length, if given, is the initial size of the\r\noutput buffer.\r\n\r\nThe decompressor object can no longer be used after this call.")]
	public string flush([DefaultParameterValue(16384)] int length)
	{
		if (length < 1)
		{
			throw PythonOps.ValueError("length must be greater than 0.");
		}
		byte[] array = new byte[length];
		long total_out = zst.total_out;
		zst.next_out = array;
		zst.next_out_index = 0;
		zst.avail_out = array.Length;
		int num = zst.inflate(FlushStrategy.Z_FINISH);
		while ((num == 0 || num == -5) && zst.avail_out == 0)
		{
			int avail_out = array.Length;
			Array.Resize(ref array, array.Length * 2);
			zst.next_out = array;
			zst.avail_out = avail_out;
			num = zst.inflate(FlushStrategy.Z_FINISH);
		}
		if (num == 1)
		{
			num = zst.inflateEnd();
			if (num != 0)
			{
				throw ZlibModule.zlib_error(zst, num, "from inflateEnd()");
			}
		}
		return PythonAsciiEncoding.Instance.GetString(array, 0, (int)(zst.total_out - total_out));
	}
}
