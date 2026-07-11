namespace ComponentAce.Compression.Libs.ZLib;

internal sealed class Inflate
{
	public InflateMode mode;

	private int method;

	private long[] was = new long[1];

	private long need;

	private int marker;

	private int nowrap;

	private int wbits;

	private InfBlocks blocks;

	internal int inflateReset(ZStream z)
	{
		if (z == null || z.istate == null)
		{
			return -2;
		}
		long total_in = (z.total_out = 0L);
		z.total_in = total_in;
		z.msg = null;
		z.istate.mode = ((z.istate.nowrap != 0) ? InflateMode.BLOCKS : InflateMode.METHOD);
		z.istate.blocks.reset(z, null);
		return 0;
	}

	internal int inflateEnd(ZStream z)
	{
		if (blocks != null)
		{
			blocks.free(z);
		}
		blocks = null;
		return 0;
	}

	internal int inflateInit(ZStream z, int windowBits)
	{
		z.msg = null;
		blocks = null;
		nowrap = 0;
		if (windowBits < 0)
		{
			windowBits = -windowBits;
			nowrap = 1;
		}
		if (windowBits < 8 || windowBits > 15)
		{
			inflateEnd(z);
			return -2;
		}
		wbits = windowBits;
		z.istate.blocks = new InfBlocks(z, z.istate.nowrap == 0, 1 << windowBits);
		inflateReset(z);
		return 0;
	}

	internal int inflate(ZStream z, FlushStrategy flush)
	{
		if (z == null || z.istate == null || z.next_in == null)
		{
			return -2;
		}
		int num = ((flush == FlushStrategy.Z_FINISH) ? (-5) : 0);
		int num2 = -5;
		while (true)
		{
			switch (z.istate.mode)
			{
			case InflateMode.METHOD:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				if (((z.istate.method = z.next_in[z.next_in_index++]) & 0xF) != 8)
				{
					z.istate.mode = InflateMode.BAD;
					z.msg = "unknown compression method";
					z.istate.marker = 5;
					break;
				}
				if ((z.istate.method >> 4) + 8 > z.istate.wbits)
				{
					z.istate.mode = InflateMode.BAD;
					z.msg = "invalid Window size";
					z.istate.marker = 5;
					break;
				}
				z.istate.mode = InflateMode.FLAG;
				goto case InflateMode.FLAG;
			case InflateMode.FLAG:
			{
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				int num3 = z.next_in[z.next_in_index++] & 0xFF;
				if (((z.istate.method << 8) + num3) % 31 != 0)
				{
					z.istate.mode = InflateMode.BAD;
					z.msg = "incorrect header check";
					z.istate.marker = 5;
					break;
				}
				if ((num3 & 0x20) == 0)
				{
					z.istate.mode = InflateMode.BLOCKS;
					break;
				}
				z.istate.mode = InflateMode.DICT4;
				goto case InflateMode.DICT4;
			}
			case InflateMode.DICT4:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need = ((long)(z.next_in[z.next_in_index++] & 0xFF) << 24) & -16777216;
				z.istate.mode = InflateMode.DICT3;
				goto case InflateMode.DICT3;
			case InflateMode.DICT3:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need += ((long)(z.next_in[z.next_in_index++] & 0xFF) << 16) & 0xFF0000;
				z.istate.mode = InflateMode.DICT2;
				goto case InflateMode.DICT2;
			case InflateMode.DICT2:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need += ((long)(z.next_in[z.next_in_index++] & 0xFF) << 8) & 0xFF00;
				z.istate.mode = InflateMode.DICT1;
				goto case InflateMode.DICT1;
			case InflateMode.DICT1:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need += (long)((ulong)z.next_in[z.next_in_index++] & 0xFFuL);
				z.adler = z.istate.need;
				z.istate.mode = InflateMode.DICT0;
				return 2;
			case InflateMode.DICT0:
				z.istate.mode = InflateMode.BAD;
				z.msg = "need dictionary";
				z.istate.marker = 0;
				return -2;
			case InflateMode.BLOCKS:
				num2 = z.istate.blocks.proc(z, num2);
				switch (num2)
				{
				case -3:
					z.istate.mode = InflateMode.BAD;
					z.istate.marker = 0;
					goto end_IL_0034;
				case 0:
					num2 = num;
					break;
				}
				if (num2 != 1)
				{
					return num2;
				}
				num2 = num;
				z.istate.blocks.reset(z, z.istate.was);
				if (z.istate.nowrap != 0)
				{
					z.istate.mode = InflateMode.DONE;
					break;
				}
				z.istate.mode = InflateMode.CHECK4;
				goto case InflateMode.CHECK4;
			case InflateMode.CHECK4:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need = ((z.next_in[z.next_in_index++] & 0xFF) << 24) & -16777216;
				z.istate.mode = InflateMode.CHECK3;
				goto case InflateMode.CHECK3;
			case InflateMode.CHECK3:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need += (long)((ulong)((z.next_in[z.next_in_index++] & 0xFF) << 16) & 0xFF0000uL);
				z.istate.mode = InflateMode.CHECK2;
				goto case InflateMode.CHECK2;
			case InflateMode.CHECK2:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need += (long)((ulong)((z.next_in[z.next_in_index++] & 0xFF) << 8) & 0xFF00uL);
				z.istate.mode = InflateMode.CHECK1;
				goto case InflateMode.CHECK1;
			case InflateMode.CHECK1:
				if (z.avail_in == 0)
				{
					return num2;
				}
				num2 = num;
				z.avail_in--;
				z.total_in++;
				z.istate.need += (long)((ulong)z.next_in[z.next_in_index++] & 0xFFuL);
				if ((int)z.istate.was[0] != (int)z.istate.need)
				{
					z.istate.mode = InflateMode.BAD;
					z.msg = "incorrect data check";
					z.istate.marker = 5;
					break;
				}
				z.istate.mode = InflateMode.DONE;
				goto case InflateMode.DONE;
			case InflateMode.DONE:
				return 1;
			case InflateMode.BAD:
				return -3;
			default:
				{
					return -2;
				}
				end_IL_0034:
				break;
			}
		}
	}

	internal int inflateSetDictionary(ZStream z, byte[] dictionary, int dictLength)
	{
		int start = 0;
		int num = dictLength;
		if (z == null || z.istate == null || z.istate.mode != InflateMode.DICT0)
		{
			return -2;
		}
		if (Adler32.GetAdler32Checksum(1L, dictionary, 0, dictLength) != z.adler)
		{
			return -3;
		}
		z.adler = Adler32.GetAdler32Checksum(0L, null, 0, 0);
		if (num >= 1 << z.istate.wbits)
		{
			num = (1 << z.istate.wbits) - 1;
			start = dictLength - num;
		}
		z.istate.blocks.set_dictionary(dictionary, start, num);
		z.istate.mode = InflateMode.BLOCKS;
		return 0;
	}

	internal int inflateSync(ZStream z)
	{
		if (z == null || z.istate == null)
		{
			return -2;
		}
		if (z.istate.mode != InflateMode.BAD)
		{
			z.istate.mode = InflateMode.BAD;
			z.istate.marker = 0;
		}
		int num;
		if ((num = z.avail_in) == 0)
		{
			return -5;
		}
		int num2 = z.next_in_index;
		int num3 = z.istate.marker;
		while (num != 0 && num3 < 4)
		{
			num3 = ((z.next_in[num2] != ZLibUtil.mark[num3]) ? ((z.next_in[num2] == 0) ? (4 - num3) : 0) : (num3 + 1));
			num2++;
			num--;
		}
		z.total_in += num2 - z.next_in_index;
		z.next_in_index = num2;
		z.avail_in = num;
		z.istate.marker = num3;
		if (num3 != 4)
		{
			return -3;
		}
		long total_in = z.total_in;
		long total_out = z.total_out;
		inflateReset(z);
		z.total_in = total_in;
		z.total_out = total_out;
		z.istate.mode = InflateMode.BLOCKS;
		return 0;
	}

	internal int inflateSyncPoint(ZStream z)
	{
		if (z == null || z.istate == null || z.istate.blocks == null)
		{
			return -2;
		}
		return z.istate.blocks.sync_point();
	}
}
