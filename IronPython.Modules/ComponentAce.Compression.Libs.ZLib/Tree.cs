using System;

namespace ComponentAce.Compression.Libs.ZLib;

internal sealed class Tree
{
	private short[] dyn_tree;

	private int max_code;

	private StaticTree stat_desc;

	public short[] DynTree
	{
		get
		{
			return dyn_tree;
		}
		set
		{
			dyn_tree = value;
		}
	}

	public int MaxCode
	{
		get
		{
			return max_code;
		}
		set
		{
			max_code = value;
		}
	}

	internal StaticTree StatDesc
	{
		get
		{
			return stat_desc;
		}
		set
		{
			stat_desc = value;
		}
	}

	internal static int d_code(int dist)
	{
		if (dist >= 256)
		{
			return ZLibUtil._dist_code[256 + ZLibUtil.URShift(dist, 7)];
		}
		return ZLibUtil._dist_code[dist];
	}

	private void gen_bitlen(Deflate s)
	{
		short[] array = dyn_tree;
		short[] static_tree = stat_desc.static_tree;
		int[] extra_bits = stat_desc.extra_bits;
		int extra_base = stat_desc.extra_base;
		int max_length = stat_desc.max_length;
		int num = 0;
		for (int i = 0; i <= 15; i++)
		{
			s.bl_count[i] = 0;
		}
		array[s.heap[s.heap_max] * 2 + 1] = 0;
		int j;
		for (j = s.heap_max + 1; j < 573; j++)
		{
			int num2 = s.heap[j];
			int i = array[array[num2 * 2 + 1] * 2 + 1] + 1;
			if (i > max_length)
			{
				i = max_length;
				num++;
			}
			array[num2 * 2 + 1] = (short)i;
			if (num2 <= max_code)
			{
				s.bl_count[i]++;
				int num3 = 0;
				if (num2 >= extra_base)
				{
					num3 = extra_bits[num2 - extra_base];
				}
				short num4 = array[num2 * 2];
				s.opt_len += num4 * (i + num3);
				if (static_tree != null)
				{
					s.static_len += num4 * (static_tree[num2 * 2 + 1] + num3);
				}
			}
		}
		if (num == 0)
		{
			return;
		}
		do
		{
			int i = max_length - 1;
			while (s.bl_count[i] == 0)
			{
				i--;
			}
			s.bl_count[i]--;
			s.bl_count[i + 1] = (short)(s.bl_count[i + 1] + 2);
			s.bl_count[max_length]--;
			num -= 2;
		}
		while (num > 0);
		for (int i = max_length; i != 0; i--)
		{
			int num2 = s.bl_count[i];
			while (num2 != 0)
			{
				int num5 = s.heap[--j];
				if (num5 <= max_code)
				{
					if (array[num5 * 2 + 1] != i)
					{
						s.opt_len = (int)(s.opt_len + ((long)i - (long)array[num5 * 2 + 1]) * array[num5 * 2]);
						array[num5 * 2 + 1] = (short)i;
					}
					num2--;
				}
			}
		}
	}

	internal void build_tree(Deflate s)
	{
		short[] array = dyn_tree;
		short[] static_tree = stat_desc.static_tree;
		int elems = stat_desc.elems;
		int num = -1;
		s.heap_len = 0;
		s.heap_max = 573;
		for (int i = 0; i < elems; i++)
		{
			if (array[i * 2] != 0)
			{
				num = (s.heap[++s.heap_len] = i);
				s.depth[i] = 0;
			}
			else
			{
				array[i * 2 + 1] = 0;
			}
		}
		int num2;
		while (s.heap_len < 2)
		{
			num2 = (s.heap[++s.heap_len] = ((num < 2) ? (++num) : 0));
			array[num2 * 2] = 1;
			s.depth[num2] = 0;
			s.opt_len--;
			if (static_tree != null)
			{
				s.static_len -= static_tree[num2 * 2 + 1];
			}
		}
		max_code = num;
		for (int i = s.heap_len / 2; i >= 1; i--)
		{
			s.pqdownheap(array, i);
		}
		num2 = elems;
		do
		{
			int i = s.heap[1];
			s.heap[1] = s.heap[s.heap_len--];
			s.pqdownheap(array, 1);
			int num3 = s.heap[1];
			s.heap[--s.heap_max] = i;
			s.heap[--s.heap_max] = num3;
			array[num2 * 2] = (short)(array[i * 2] + array[num3 * 2]);
			s.depth[num2] = (byte)(Math.Max(s.depth[i], s.depth[num3]) + 1);
			array[i * 2 + 1] = (array[num3 * 2 + 1] = (short)num2);
			s.heap[1] = num2++;
			s.pqdownheap(array, 1);
		}
		while (s.heap_len >= 2);
		s.heap[--s.heap_max] = s.heap[1];
		gen_bitlen(s);
		gen_codes(array, num, s.bl_count);
	}

	private static void gen_codes(short[] tree, int max_code, short[] bl_count)
	{
		short[] array = new short[16];
		short num = 0;
		for (int i = 1; i <= 15; i++)
		{
			num = (array[i] = (short)(num + bl_count[i - 1] << 1));
		}
		for (int j = 0; j <= max_code; j++)
		{
			int num2 = tree[j * 2 + 1];
			if (num2 != 0)
			{
				tree[j * 2] = (short)bi_reverse(array[num2]++, num2);
			}
		}
	}

	private static int bi_reverse(int code, int len)
	{
		int num = 0;
		do
		{
			num |= code & 1;
			code = ZLibUtil.URShift(code, 1);
			num <<= 1;
		}
		while (--len > 0);
		return ZLibUtil.URShift(num, 1);
	}
}
