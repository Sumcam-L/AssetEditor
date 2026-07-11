using System;

namespace ComponentAce.Compression.Libs.ZLib;

internal sealed class InfCodes
{
	private InflateCodesMode mode;

	private int count;

	private int[] tree;

	internal int tree_index;

	internal int need;

	internal int lit;

	internal int get_Renamed;

	internal int dist;

	private byte lbits;

	private byte dbits;

	private int[] ltree;

	private int ltree_index;

	private int[] dtree;

	private int dtree_index;

	internal InfCodes(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, ZStream z)
	{
		mode = InflateCodesMode.START;
		lbits = (byte)bl;
		dbits = (byte)bd;
		ltree = tl;
		ltree_index = tl_index;
		dtree = td;
		dtree_index = td_index;
	}

	internal InfCodes(int bl, int bd, int[] tl, int[] td, ZStream z)
	{
		mode = InflateCodesMode.START;
		lbits = (byte)bl;
		dbits = (byte)bd;
		ltree = tl;
		ltree_index = 0;
		dtree = td;
		dtree_index = 0;
	}

	internal int proc(InfBlocks s, ZStream z, int r)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		num3 = z.next_in_index;
		int num4 = z.avail_in;
		num = s.BitB;
		num2 = s.BitK;
		int num5 = s.WritePos;
		int num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
		while (true)
		{
			switch (mode)
			{
			case InflateCodesMode.START:
				if (num6 >= 258 && num4 >= 10)
				{
					s.BitB = num;
					s.BitK = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.WritePos = num5;
					r = inflate_fast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, s, z);
					num3 = z.next_in_index;
					num4 = z.avail_in;
					num = s.BitB;
					num2 = s.BitK;
					num5 = s.WritePos;
					num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
					if (r != 0)
					{
						mode = ((r == 1) ? InflateCodesMode.WASH : InflateCodesMode.BADCODE);
						break;
					}
				}
				need = lbits;
				tree = ltree;
				tree_index = ltree_index;
				mode = InflateCodesMode.LEN;
				goto case InflateCodesMode.LEN;
			case InflateCodesMode.LEN:
			{
				int num7;
				for (num7 = need; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.BitB = num;
					s.BitK = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.WritePos = num5;
					return s.inflate_flush(z, r);
				}
				int num8 = (tree_index + (num & ZLibUtil.inflate_mask[num7])) * 3;
				num = ZLibUtil.URShift(num, tree[num8 + 1]);
				num2 -= tree[num8 + 1];
				int num9 = tree[num8];
				if (num9 == 0)
				{
					lit = tree[num8 + 2];
					mode = InflateCodesMode.LIT;
					break;
				}
				if ((num9 & 0x10) != 0)
				{
					get_Renamed = num9 & 0xF;
					count = tree[num8 + 2];
					mode = InflateCodesMode.LENEXT;
					break;
				}
				if ((num9 & 0x40) == 0)
				{
					need = num9;
					tree_index = num8 / 3 + tree[num8 + 2];
					break;
				}
				if ((num9 & 0x20) != 0)
				{
					mode = InflateCodesMode.WASH;
					break;
				}
				mode = InflateCodesMode.BADCODE;
				z.msg = "invalid literal/length code";
				r = -3;
				s.BitB = num;
				s.BitK = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.WritePos = num5;
				return s.inflate_flush(z, r);
			}
			case InflateCodesMode.LENEXT:
			{
				int num7;
				for (num7 = get_Renamed; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.BitB = num;
					s.BitK = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.WritePos = num5;
					return s.inflate_flush(z, r);
				}
				count += num & ZLibUtil.inflate_mask[num7];
				num >>= num7;
				num2 -= num7;
				need = dbits;
				tree = dtree;
				tree_index = dtree_index;
				mode = InflateCodesMode.DIST;
				goto case InflateCodesMode.DIST;
			}
			case InflateCodesMode.DIST:
			{
				int num7;
				for (num7 = need; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.BitB = num;
					s.BitK = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.WritePos = num5;
					return s.inflate_flush(z, r);
				}
				int num8 = (tree_index + (num & ZLibUtil.inflate_mask[num7])) * 3;
				num >>= tree[num8 + 1];
				num2 -= tree[num8 + 1];
				int num9 = tree[num8];
				if ((num9 & 0x10) != 0)
				{
					get_Renamed = num9 & 0xF;
					dist = tree[num8 + 2];
					mode = InflateCodesMode.DISTEXT;
					break;
				}
				if ((num9 & 0x40) == 0)
				{
					need = num9;
					tree_index = num8 / 3 + tree[num8 + 2];
					break;
				}
				mode = InflateCodesMode.BADCODE;
				z.msg = "invalid distance code";
				r = -3;
				s.BitB = num;
				s.BitK = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.WritePos = num5;
				return s.inflate_flush(z, r);
			}
			case InflateCodesMode.DISTEXT:
			{
				int num7;
				for (num7 = get_Renamed; num2 < num7; num2 += 8)
				{
					if (num4 != 0)
					{
						r = 0;
						num4--;
						num |= (z.next_in[num3++] & 0xFF) << num2;
						continue;
					}
					s.BitB = num;
					s.BitK = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.WritePos = num5;
					return s.inflate_flush(z, r);
				}
				dist += num & ZLibUtil.inflate_mask[num7];
				num >>= num7;
				num2 -= num7;
				mode = InflateCodesMode.COPY;
				goto case InflateCodesMode.COPY;
			}
			case InflateCodesMode.COPY:
			{
				int i;
				for (i = num5 - dist; i < 0; i += s.End)
				{
				}
				while (count != 0)
				{
					if (num6 == 0)
					{
						if (num5 == s.End && s.ReadPos != 0)
						{
							num5 = 0;
							num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
						}
						if (num6 == 0)
						{
							s.WritePos = num5;
							r = s.inflate_flush(z, r);
							num5 = s.WritePos;
							num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
							if (num5 == s.End && s.ReadPos != 0)
							{
								num5 = 0;
								num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
							}
							if (num6 == 0)
							{
								s.BitB = num;
								s.BitK = num2;
								z.avail_in = num4;
								z.total_in += num3 - z.next_in_index;
								z.next_in_index = num3;
								s.WritePos = num5;
								return s.inflate_flush(z, r);
							}
						}
					}
					s.Window[num5++] = s.Window[i++];
					num6--;
					if (i == s.End)
					{
						i = 0;
					}
					count--;
				}
				mode = InflateCodesMode.START;
				break;
			}
			case InflateCodesMode.LIT:
				if (num6 == 0)
				{
					if (num5 == s.End && s.ReadPos != 0)
					{
						num5 = 0;
						num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
					}
					if (num6 == 0)
					{
						s.WritePos = num5;
						r = s.inflate_flush(z, r);
						num5 = s.WritePos;
						num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
						if (num5 == s.End && s.ReadPos != 0)
						{
							num5 = 0;
							num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
						}
						if (num6 == 0)
						{
							s.BitB = num;
							s.BitK = num2;
							z.avail_in = num4;
							z.total_in += num3 - z.next_in_index;
							z.next_in_index = num3;
							s.WritePos = num5;
							return s.inflate_flush(z, r);
						}
					}
				}
				r = 0;
				s.Window[num5++] = (byte)lit;
				num6--;
				mode = InflateCodesMode.START;
				break;
			case InflateCodesMode.WASH:
				if (num2 > 7)
				{
					num2 -= 8;
					num4++;
					num3--;
				}
				s.WritePos = num5;
				r = s.inflate_flush(z, r);
				num5 = s.WritePos;
				num6 = ((num5 < s.ReadPos) ? (s.ReadPos - num5 - 1) : (s.End - num5));
				if (s.ReadPos != s.WritePos)
				{
					s.BitB = num;
					s.BitK = num2;
					z.avail_in = num4;
					z.total_in += num3 - z.next_in_index;
					z.next_in_index = num3;
					s.WritePos = num5;
					return s.inflate_flush(z, r);
				}
				mode = InflateCodesMode.END;
				goto case InflateCodesMode.END;
			case InflateCodesMode.END:
				r = 1;
				s.BitB = num;
				s.BitK = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.WritePos = num5;
				return s.inflate_flush(z, r);
			case InflateCodesMode.BADCODE:
				r = -3;
				s.BitB = num;
				s.BitK = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.WritePos = num5;
				return s.inflate_flush(z, r);
			default:
				r = -2;
				s.BitB = num;
				s.BitK = num2;
				z.avail_in = num4;
				z.total_in += num3 - z.next_in_index;
				z.next_in_index = num3;
				s.WritePos = num5;
				return s.inflate_flush(z, r);
			}
		}
	}

	internal void free(ZStream z)
	{
	}

	internal int inflate_fast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InfBlocks s, ZStream z)
	{
		int next_in_index = z.next_in_index;
		int num = z.avail_in;
		int num2 = s.BitB;
		int num3 = s.BitK;
		int num4 = s.WritePos;
		int num5 = ((num4 < s.ReadPos) ? (s.ReadPos - num4 - 1) : (s.End - num4));
		int num6 = ZLibUtil.inflate_mask[bl];
		int num7 = ZLibUtil.inflate_mask[bd];
		int num11;
		while (true)
		{
			if (num3 < 20)
			{
				num--;
				num2 |= (z.next_in[next_in_index++] & 0xFF) << num3;
				num3 += 8;
				continue;
			}
			int num8 = num2 & num6;
			int[] array = tl;
			int num9 = tl_index;
			int num10;
			if ((num10 = array[(num9 + num8) * 3]) == 0)
			{
				num2 >>= array[(num9 + num8) * 3 + 1];
				num3 -= array[(num9 + num8) * 3 + 1];
				s.Window[num4++] = (byte)array[(num9 + num8) * 3 + 2];
				num5--;
			}
			else
			{
				while (true)
				{
					num2 >>= array[(num9 + num8) * 3 + 1];
					num3 -= array[(num9 + num8) * 3 + 1];
					if ((num10 & 0x10) != 0)
					{
						num10 &= 0xF;
						num11 = array[(num9 + num8) * 3 + 2] + (num2 & ZLibUtil.inflate_mask[num10]);
						num2 >>= num10;
						for (num3 -= num10; num3 < 15; num3 += 8)
						{
							num--;
							num2 |= (z.next_in[next_in_index++] & 0xFF) << num3;
						}
						num8 = num2 & num7;
						array = td;
						num9 = td_index;
						num10 = array[(num9 + num8) * 3];
						while (true)
						{
							num2 >>= array[(num9 + num8) * 3 + 1];
							num3 -= array[(num9 + num8) * 3 + 1];
							if ((num10 & 0x10) != 0)
							{
								break;
							}
							if ((num10 & 0x40) == 0)
							{
								num8 += array[(num9 + num8) * 3 + 2];
								num8 += num2 & ZLibUtil.inflate_mask[num10];
								num10 = array[(num9 + num8) * 3];
								continue;
							}
							z.msg = "invalid distance code";
							num11 = z.avail_in - num;
							num11 = ((num3 >> 3 < num11) ? (num3 >> 3) : num11);
							num += num11;
							next_in_index -= num11;
							num3 -= num11 << 3;
							s.BitB = num2;
							s.BitK = num3;
							z.avail_in = num;
							z.total_in += next_in_index - z.next_in_index;
							z.next_in_index = next_in_index;
							s.WritePos = num4;
							return -3;
						}
						for (num10 &= 0xF; num3 < num10; num3 += 8)
						{
							num--;
							num2 |= (z.next_in[next_in_index++] & 0xFF) << num3;
						}
						int num12 = array[(num9 + num8) * 3 + 2] + (num2 & ZLibUtil.inflate_mask[num10]);
						num2 >>= num10;
						num3 -= num10;
						num5 -= num11;
						int num13;
						if (num4 >= num12)
						{
							num13 = num4 - num12;
							if (num4 - num13 > 0 && 2 > num4 - num13)
							{
								s.Window[num4++] = s.Window[num13++];
								num11--;
								s.Window[num4++] = s.Window[num13++];
								num11--;
							}
							else
							{
								Array.Copy(s.Window, num13, s.Window, num4, 2);
								num4 += 2;
								num13 += 2;
								num11 -= 2;
							}
						}
						else
						{
							num13 = num4 - num12;
							do
							{
								num13 += s.End;
							}
							while (num13 < 0);
							num10 = s.End - num13;
							if (num11 > num10)
							{
								num11 -= num10;
								if (num4 - num13 > 0 && num10 > num4 - num13)
								{
									do
									{
										s.Window[num4++] = s.Window[num13++];
									}
									while (--num10 != 0);
								}
								else
								{
									Array.Copy(s.Window, num13, s.Window, num4, num10);
									num4 += num10;
									num13 += num10;
									num10 = 0;
								}
								num13 = 0;
							}
						}
						if (num4 - num13 > 0 && num11 > num4 - num13)
						{
							do
							{
								s.Window[num4++] = s.Window[num13++];
							}
							while (--num11 != 0);
							break;
						}
						Array.Copy(s.Window, num13, s.Window, num4, num11);
						num4 += num11;
						num13 += num11;
						num11 = 0;
						break;
					}
					if ((num10 & 0x40) == 0)
					{
						num8 += array[(num9 + num8) * 3 + 2];
						num8 += num2 & ZLibUtil.inflate_mask[num10];
						if ((num10 = array[(num9 + num8) * 3]) == 0)
						{
							num2 >>= array[(num9 + num8) * 3 + 1];
							num3 -= array[(num9 + num8) * 3 + 1];
							s.Window[num4++] = (byte)array[(num9 + num8) * 3 + 2];
							num5--;
							break;
						}
						continue;
					}
					if ((num10 & 0x20) != 0)
					{
						num11 = z.avail_in - num;
						num11 = ((num3 >> 3 < num11) ? (num3 >> 3) : num11);
						num += num11;
						next_in_index -= num11;
						num3 -= num11 << 3;
						s.BitB = num2;
						s.BitK = num3;
						z.avail_in = num;
						z.total_in += next_in_index - z.next_in_index;
						z.next_in_index = next_in_index;
						s.WritePos = num4;
						return 1;
					}
					z.msg = "invalid literal/length code";
					num11 = z.avail_in - num;
					num11 = ((num3 >> 3 < num11) ? (num3 >> 3) : num11);
					num += num11;
					next_in_index -= num11;
					num3 -= num11 << 3;
					s.BitB = num2;
					s.BitK = num3;
					z.avail_in = num;
					z.total_in += next_in_index - z.next_in_index;
					z.next_in_index = next_in_index;
					s.WritePos = num4;
					return -3;
				}
			}
			if (num5 < 258 || num < 10)
			{
				break;
			}
		}
		num11 = z.avail_in - num;
		num11 = ((num3 >> 3 < num11) ? (num3 >> 3) : num11);
		num += num11;
		next_in_index -= num11;
		num3 -= num11 << 3;
		s.BitB = num2;
		s.BitK = num3;
		z.avail_in = num;
		z.total_in += next_in_index - z.next_in_index;
		z.next_in_index = next_in_index;
		s.WritePos = num4;
		return 0;
	}
}
