using System;

namespace ComponentAce.Compression.Libs.ZLib;

internal sealed class InfBlocks
{
	private const int MANY = 1440;

	private InflateBlockMode mode;

	private int left;

	private int table;

	private int index;

	private int[] blens;

	private int[] bb = new int[1];

	private int[] tb = new int[1];

	private InfCodes codes;

	private int last;

	private int bitk;

	private int bitb;

	private int[] hufts;

	private byte[] window;

	private int end;

	private int read;

	private int write;

	private bool needCheck;

	private long check;

	public byte[] Window
	{
		get
		{
			return window;
		}
		set
		{
			window = value;
		}
	}

	public int End
	{
		get
		{
			return end;
		}
		set
		{
			end = value;
		}
	}

	public int ReadPos
	{
		get
		{
			return read;
		}
		set
		{
			read = value;
		}
	}

	public int WritePos
	{
		get
		{
			return write;
		}
		set
		{
			write = value;
		}
	}

	public int BitK
	{
		get
		{
			return bitk;
		}
		set
		{
			bitk = value;
		}
	}

	public int BitB
	{
		get
		{
			return bitb;
		}
		set
		{
			bitb = value;
		}
	}

	internal InfBlocks(ZStream z, bool needCheck, int w)
	{
		hufts = new int[4320];
		window = new byte[w];
		end = w;
		this.needCheck = needCheck;
		mode = InflateBlockMode.TYPE;
		reset(z, null);
	}

	internal void reset(ZStream z, long[] c)
	{
		if (c != null)
		{
			c[0] = check;
		}
		if (mode == InflateBlockMode.BTREE || mode == InflateBlockMode.DTREE)
		{
			blens = null;
		}
		if (mode == InflateBlockMode.CODES)
		{
			codes.free(z);
		}
		mode = InflateBlockMode.TYPE;
		BitK = 0;
		BitB = 0;
		int readPos = (WritePos = 0);
		ReadPos = readPos;
		if (needCheck)
		{
			z.adler = (check = Adler32.GetAdler32Checksum(0L, null, 0, 0));
		}
	}

	internal int proc(ZStream z, int r)
	{
		int num = z.next_in_index;
		int num2 = z.avail_in;
		int num3 = BitB;
		int i = BitK;
		int num4 = WritePos;
		int num5 = ((num4 < ReadPos) ? (ReadPos - num4 - 1) : (End - num4));
		while (true)
		{
			switch (mode)
			{
			case InflateBlockMode.TYPE:
			{
				for (; i < 3; i += 8)
				{
					if (num2 != 0)
					{
						r = 0;
						num2--;
						num3 |= (z.next_in[num++] & 0xFF) << i;
						continue;
					}
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				int num6 = num3 & 7;
				last = num6 & 1;
				switch (ZLibUtil.URShift(num6, 1))
				{
				case 0:
					num3 = ZLibUtil.URShift(num3, 3);
					i -= 3;
					num6 = i & 7;
					num3 = ZLibUtil.URShift(num3, num6);
					i -= num6;
					mode = InflateBlockMode.LENS;
					break;
				case 1:
				{
					int[] array5 = new int[1];
					int[] array6 = new int[1];
					int[][] array7 = new int[1][];
					int[][] array8 = new int[1][];
					InfTree.inflate_trees_fixed(array5, array6, array7, array8, z);
					codes = new InfCodes(array5[0], array6[0], array7[0], array8[0], z);
					num3 = ZLibUtil.URShift(num3, 3);
					i -= 3;
					mode = InflateBlockMode.CODES;
					break;
				}
				case 2:
					num3 = ZLibUtil.URShift(num3, 3);
					i -= 3;
					mode = InflateBlockMode.TABLE;
					break;
				case 3:
					num3 = ZLibUtil.URShift(num3, 3);
					i -= 3;
					mode = InflateBlockMode.BAD;
					z.msg = "invalid block type";
					r = -3;
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				break;
			}
			case InflateBlockMode.LENS:
				for (; i < 32; i += 8)
				{
					if (num2 != 0)
					{
						r = 0;
						num2--;
						num3 |= (z.next_in[num++] & 0xFF) << i;
						continue;
					}
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				if ((ZLibUtil.URShift(~num3, 16) & 0xFFFF) != (num3 & 0xFFFF))
				{
					mode = InflateBlockMode.BAD;
					z.msg = "invalid stored block lengths";
					r = -3;
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				left = num3 & 0xFFFF;
				num3 = (i = 0);
				mode = ((left != 0) ? InflateBlockMode.STORED : ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE));
				break;
			case InflateBlockMode.STORED:
			{
				if (num2 == 0)
				{
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				if (num5 == 0)
				{
					if (num4 == End && ReadPos != 0)
					{
						num4 = 0;
						num5 = ((num4 < ReadPos) ? (ReadPos - num4 - 1) : (End - num4));
					}
					if (num5 == 0)
					{
						WritePos = num4;
						r = inflate_flush(z, r);
						num4 = WritePos;
						num5 = ((num4 < ReadPos) ? (ReadPos - num4 - 1) : (End - num4));
						if (num4 == End && ReadPos != 0)
						{
							num4 = 0;
							num5 = ((num4 < ReadPos) ? (ReadPos - num4 - 1) : (End - num4));
						}
						if (num5 == 0)
						{
							BitB = num3;
							BitK = i;
							z.avail_in = num2;
							z.total_in += num - z.next_in_index;
							z.next_in_index = num;
							WritePos = num4;
							return inflate_flush(z, r);
						}
					}
				}
				r = 0;
				int num6 = left;
				if (num6 > num2)
				{
					num6 = num2;
				}
				if (num6 > num5)
				{
					num6 = num5;
				}
				Array.Copy(z.next_in, num, Window, num4, num6);
				num += num6;
				num2 -= num6;
				num4 += num6;
				num5 -= num6;
				if ((left -= num6) == 0)
				{
					mode = ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
				}
				break;
			}
			case InflateBlockMode.TABLE:
			{
				for (; i < 14; i += 8)
				{
					if (num2 != 0)
					{
						r = 0;
						num2--;
						num3 |= (z.next_in[num++] & 0xFF) << i;
						continue;
					}
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				int num6 = (table = num3 & 0x3FFF);
				if ((num6 & 0x1F) > 29 || ((num6 >> 5) & 0x1F) > 29)
				{
					mode = InflateBlockMode.BAD;
					z.msg = "too many length or distance symbols";
					r = -3;
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				num6 = 258 + (num6 & 0x1F) + ((num6 >> 5) & 0x1F);
				blens = new int[num6];
				num3 = ZLibUtil.URShift(num3, 14);
				i -= 14;
				index = 0;
				mode = InflateBlockMode.BTREE;
				goto case InflateBlockMode.BTREE;
			}
			case InflateBlockMode.BTREE:
			{
				while (index < 4 + ZLibUtil.URShift(table, 10))
				{
					for (; i < 3; i += 8)
					{
						if (num2 != 0)
						{
							r = 0;
							num2--;
							num3 |= (z.next_in[num++] & 0xFF) << i;
							continue;
						}
						BitB = num3;
						BitK = i;
						z.avail_in = num2;
						z.total_in += num - z.next_in_index;
						z.next_in_index = num;
						WritePos = num4;
						return inflate_flush(z, r);
					}
					blens[ZLibUtil.border[index++]] = num3 & 7;
					num3 = ZLibUtil.URShift(num3, 3);
					i -= 3;
				}
				while (index < 19)
				{
					blens[ZLibUtil.border[index++]] = 0;
				}
				bb[0] = 7;
				int num6 = InfTree.inflate_trees_bits(blens, bb, tb, hufts, z);
				if (num6 != 0)
				{
					r = num6;
					if (r == -3)
					{
						blens = null;
						mode = InflateBlockMode.BAD;
					}
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				index = 0;
				mode = InflateBlockMode.DTREE;
				goto case InflateBlockMode.DTREE;
			}
			case InflateBlockMode.DTREE:
			{
				int num6;
				while (true)
				{
					num6 = table;
					if (index >= 258 + (num6 & 0x1F) + ((num6 >> 5) & 0x1F))
					{
						break;
					}
					for (num6 = bb[0]; i < num6; i += 8)
					{
						if (num2 != 0)
						{
							r = 0;
							num2--;
							num3 |= (z.next_in[num++] & 0xFF) << i;
							continue;
						}
						BitB = num3;
						BitK = i;
						z.avail_in = num2;
						z.total_in += num - z.next_in_index;
						z.next_in_index = num;
						WritePos = num4;
						return inflate_flush(z, r);
					}
					num6 = hufts[(tb[0] + (num3 & ZLibUtil.inflate_mask[num6])) * 3 + 1];
					int num7 = hufts[(tb[0] + (num3 & ZLibUtil.inflate_mask[num6])) * 3 + 2];
					if (num7 < 16)
					{
						num3 = ZLibUtil.URShift(num3, num6);
						i -= num6;
						blens[index++] = num7;
						continue;
					}
					int num8 = ((num7 == 18) ? 7 : (num7 - 14));
					int num9 = ((num7 == 18) ? 11 : 3);
					for (; i < num6 + num8; i += 8)
					{
						if (num2 != 0)
						{
							r = 0;
							num2--;
							num3 |= (z.next_in[num++] & 0xFF) << i;
							continue;
						}
						BitB = num3;
						BitK = i;
						z.avail_in = num2;
						z.total_in += num - z.next_in_index;
						z.next_in_index = num;
						WritePos = num4;
						return inflate_flush(z, r);
					}
					num3 = ZLibUtil.URShift(num3, num6);
					i -= num6;
					num9 += num3 & ZLibUtil.inflate_mask[num8];
					num3 = ZLibUtil.URShift(num3, num8);
					i -= num8;
					num8 = index;
					num6 = table;
					if (num8 + num9 > 258 + (num6 & 0x1F) + ((num6 >> 5) & 0x1F) || (num7 == 16 && num8 < 1))
					{
						blens = null;
						mode = InflateBlockMode.BAD;
						z.msg = "invalid bit length repeat";
						r = -3;
						BitB = num3;
						BitK = i;
						z.avail_in = num2;
						z.total_in += num - z.next_in_index;
						z.next_in_index = num;
						WritePos = num4;
						return inflate_flush(z, r);
					}
					num7 = ((num7 == 16) ? blens[num8 - 1] : 0);
					do
					{
						blens[num8++] = num7;
					}
					while (--num9 != 0);
					index = num8;
				}
				tb[0] = -1;
				int[] array = new int[1];
				int[] array2 = new int[1];
				int[] array3 = new int[1];
				int[] array4 = new int[1];
				array[0] = 9;
				array2[0] = 6;
				num6 = table;
				num6 = InfTree.inflate_trees_dynamic(257 + (num6 & 0x1F), 1 + ((num6 >> 5) & 0x1F), blens, array, array2, array3, array4, hufts, z);
				if (num6 != 0)
				{
					if (num6 == -3)
					{
						blens = null;
						mode = InflateBlockMode.BAD;
					}
					r = num6;
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				codes = new InfCodes(array[0], array2[0], hufts, array3[0], hufts, array4[0], z);
				blens = null;
				mode = InflateBlockMode.CODES;
				goto case InflateBlockMode.CODES;
			}
			case InflateBlockMode.CODES:
				BitB = num3;
				BitK = i;
				z.avail_in = num2;
				z.total_in += num - z.next_in_index;
				z.next_in_index = num;
				WritePos = num4;
				if ((r = codes.proc(this, z, r)) != 1)
				{
					return inflate_flush(z, r);
				}
				r = 0;
				codes.free(z);
				num = z.next_in_index;
				num2 = z.avail_in;
				num3 = BitB;
				i = BitK;
				num4 = WritePos;
				num5 = ((num4 < ReadPos) ? (ReadPos - num4 - 1) : (End - num4));
				if (last == 0)
				{
					mode = InflateBlockMode.TYPE;
					break;
				}
				mode = InflateBlockMode.DRY;
				goto case InflateBlockMode.DRY;
			case InflateBlockMode.DRY:
				WritePos = num4;
				r = inflate_flush(z, r);
				num4 = WritePos;
				num5 = ((num4 < ReadPos) ? (ReadPos - num4 - 1) : (End - num4));
				if (ReadPos != WritePos)
				{
					BitB = num3;
					BitK = i;
					z.avail_in = num2;
					z.total_in += num - z.next_in_index;
					z.next_in_index = num;
					WritePos = num4;
					return inflate_flush(z, r);
				}
				mode = InflateBlockMode.DONE;
				goto case InflateBlockMode.DONE;
			case InflateBlockMode.DONE:
				r = 1;
				BitB = num3;
				BitK = i;
				z.avail_in = num2;
				z.total_in += num - z.next_in_index;
				z.next_in_index = num;
				WritePos = num4;
				return inflate_flush(z, r);
			case InflateBlockMode.BAD:
				r = -3;
				BitB = num3;
				BitK = i;
				z.avail_in = num2;
				z.total_in += num - z.next_in_index;
				z.next_in_index = num;
				WritePos = num4;
				return inflate_flush(z, r);
			default:
				r = -2;
				BitB = num3;
				BitK = i;
				z.avail_in = num2;
				z.total_in += num - z.next_in_index;
				z.next_in_index = num;
				WritePos = num4;
				return inflate_flush(z, r);
			}
		}
	}

	internal void free(ZStream z)
	{
		reset(z, null);
		Window = null;
		hufts = null;
	}

	internal void set_dictionary(byte[] d, int start, int n)
	{
		Array.Copy(d, start, Window, 0, n);
		int readPos = (WritePos = n);
		ReadPos = readPos;
	}

	internal int sync_point()
	{
		if (mode != InflateBlockMode.LENS)
		{
			return 0;
		}
		return 1;
	}

	internal int inflate_flush(ZStream z, int r)
	{
		int next_out_index = z.next_out_index;
		int readPos = ReadPos;
		int num = ((readPos <= WritePos) ? WritePos : End) - readPos;
		if (num > z.avail_out)
		{
			num = z.avail_out;
		}
		if (num != 0 && r == -5)
		{
			r = 0;
		}
		z.avail_out -= num;
		z.total_out += num;
		if (needCheck)
		{
			z.adler = (check = Adler32.GetAdler32Checksum(check, Window, readPos, num));
		}
		Array.Copy(Window, readPos, z.next_out, next_out_index, num);
		next_out_index += num;
		readPos += num;
		if (readPos == End)
		{
			readPos = 0;
			if (WritePos == End)
			{
				WritePos = 0;
			}
			num = WritePos - readPos;
			if (num > z.avail_out)
			{
				num = z.avail_out;
			}
			if (num != 0 && r == -5)
			{
				r = 0;
			}
			z.avail_out -= num;
			z.total_out += num;
			if (needCheck)
			{
				z.adler = (check = Adler32.GetAdler32Checksum(check, Window, readPos, num));
			}
			Array.Copy(Window, readPos, z.next_out, next_out_index, num);
			next_out_index += num;
			readPos += num;
		}
		z.next_out_index = next_out_index;
		ReadPos = readPos;
		return r;
	}
}
