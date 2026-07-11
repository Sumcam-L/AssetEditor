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
public class Compress
{
	private const int Z_OK = 0;

	private const int Z_BUF_ERROR = -5;

	private const int Z_STREAM_END = 1;

	private const int Z_NO_FLUSH = 0;

	private const int Z_FINISH = 4;

	private ZStream zst;

	internal Compress(int level, int method, int wbits, int memlevel, int strategy)
	{
		zst = new ZStream();
		int num = zst.deflateInit(level, wbits);
		switch (num)
		{
		case -2:
			throw PythonOps.ValueError("Invalid initialization option");
		default:
			throw ZlibModule.zlib_error(zst, num, "while creating compression object");
		case 0:
			break;
		}
	}

	[Documentation("compress(data) -- Return a string containing data compressed.\r\n\r\nAfter calling this function, some of the input data may still\r\nbe stored in internal buffers for later processing.\r\nCall the flush() method to clear these buffers.")]
	public string compress([BytesConversion] IList<byte> data)
	{
		byte[] array = data.ToArray();
		byte[] array2 = new byte[16384];
		long total_out = zst.total_out;
		zst.next_in = array;
		zst.next_in_index = 0;
		zst.avail_in = array.Length;
		zst.next_out = array2;
		zst.next_out_index = 0;
		zst.avail_out = array2.Length;
		int num = zst.deflate(FlushStrategy.Z_NO_FLUSH);
		while (num == 0 && zst.avail_out == 0)
		{
			int avail_out = array2.Length;
			Array.Resize(ref array2, array2.Length * 2);
			zst.next_out = array2;
			zst.avail_out = avail_out;
			num = zst.deflate(FlushStrategy.Z_NO_FLUSH);
		}
		if (num != 0 && num != -5)
		{
			throw ZlibModule.zlib_error(zst, num, "while compressing");
		}
		return PythonAsciiEncoding.Instance.GetString(array2, 0, (int)(zst.total_out - total_out));
	}

	[Documentation("flush( [mode] ) -- Return a string containing any remaining compressed data.\r\n\r\nmode can be one of the constants Z_SYNC_FLUSH, Z_FULL_FLUSH, Z_FINISH; the\r\ndefault value used when mode is not specified is Z_FINISH.\r\nIf mode == Z_FINISH, the compressor object can no longer be used after\r\ncalling the flush() method.  Otherwise, more data can still be compressed.")]
	public string flush([DefaultParameterValue(4)] int mode)
	{
		byte[] array = new byte[16384];
		if (mode == 0)
		{
			return string.Empty;
		}
		long total_out = zst.total_out;
		zst.avail_in = 0;
		zst.next_out = array;
		zst.next_out_index = 0;
		zst.avail_out = array.Length;
		int num = zst.deflate((FlushStrategy)mode);
		while (num == 0 && zst.avail_out == 0)
		{
			int avail_out = array.Length;
			Array.Resize(ref array, array.Length * 2);
			zst.next_out = array;
			zst.avail_out = avail_out;
			num = zst.deflate((FlushStrategy)mode);
		}
		if (num == 1 && mode == 4)
		{
			num = zst.deflateEnd();
			if (num != 0)
			{
				throw ZlibModule.zlib_error(zst, num, "from deflateEnd()");
			}
		}
		else if (num != 0 && num != -5)
		{
			throw ZlibModule.zlib_error(zst, num, "while flushing");
		}
		return PythonAsciiEncoding.Instance.GetString(array, 0, (int)(zst.total_out - total_out));
	}
}
