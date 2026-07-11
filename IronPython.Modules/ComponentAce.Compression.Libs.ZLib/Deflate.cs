using System;

namespace ComponentAce.Compression.Libs.ZLib;

internal sealed class Deflate
{
	internal class Config
	{
		internal int good_length;

		internal int max_lazy;

		internal int nice_length;

		internal int max_chain;

		internal int func;

		internal Config(int good_length, int max_lazy, int nice_length, int max_chain, int func)
		{
			this.good_length = good_length;
			this.max_lazy = max_lazy;
			this.nice_length = nice_length;
			this.max_chain = max_chain;
			this.func = func;
		}
	}

	private const int MAX_MEM_LEVEL = 9;

	private const int Z_DEFAULT_COMPRESSION = -1;

	private const int DEF_MEM_LEVEL = 8;

	private const int STORED = 0;

	private const int FAST = 1;

	private const int SLOW = 2;

	private const int NeedMore = 0;

	private const int BlockDone = 1;

	private const int FinishStarted = 2;

	private const int FinishDone = 3;

	private const int PRESET_DICT = 32;

	private const int Z_DEFLATED = 8;

	private const int STORED_BLOCK = 0;

	private const int STATIC_TREES = 1;

	private const int DYN_TREES = 2;

	private const int Buf_size = 16;

	private const int REP_3_6 = 16;

	private const int REPZ_3_10 = 17;

	private const int REPZ_11_138 = 18;

	private const int MIN_MATCH = 3;

	private const int MAX_MATCH = 258;

	private const int MIN_LOOKAHEAD = 262;

	private const int MAX_BITS = 15;

	private const int D_CODES = 30;

	private const int BL_CODES = 19;

	private const int LENGTH_CODES = 29;

	private const int LITERALS = 256;

	private const int L_CODES = 286;

	private const int HEAP_SIZE = 573;

	private const int END_BLOCK = 256;

	private static Config[] config_table;

	private ZStream strm;

	private DeflateState status;

	private byte[] pending_buf;

	private int pending_buf_size;

	private int pending_out;

	private int pending;

	private int noheader;

	private BlockType data_type;

	private byte method;

	private int last_flush;

	private int w_size;

	private int w_bits;

	private int w_mask;

	private byte[] window;

	private int window_size;

	private short[] prev;

	private short[] head;

	private int ins_h;

	private int hash_size;

	private int hash_bits;

	private int hash_mask;

	private int hash_shift;

	private int block_start;

	private int match_length;

	private int prev_match;

	private int match_available;

	private int strstart;

	private int match_start;

	private int lookahead;

	private int prev_length;

	private int max_chain_length;

	private int max_lazy_match;

	private int _level;

	private CompressionStrategy strategy;

	private int good_match;

	private int nice_match;

	private short[] dyn_ltree;

	private short[] dyn_dtree;

	private short[] bl_tree;

	private Tree l_desc = new Tree();

	private Tree d_desc = new Tree();

	private Tree bl_desc = new Tree();

	internal short[] bl_count = new short[16];

	internal int[] heap = new int[573];

	internal int heap_len;

	internal int heap_max;

	internal byte[] depth = new byte[573];

	internal int l_buf;

	private int lit_bufsize;

	private int last_lit;

	private int d_buf;

	internal int opt_len;

	internal int static_len;

	internal int matches;

	internal int last_eob_len;

	private short bi_buf;

	private int bi_valid;

	public int level
	{
		get
		{
			return _level;
		}
		set
		{
			_level = value;
		}
	}

	public int Pending
	{
		get
		{
			return pending;
		}
		set
		{
			pending = value;
		}
	}

	public byte[] Pending_buf
	{
		get
		{
			return pending_buf;
		}
		set
		{
			pending_buf = value;
		}
	}

	public int Pending_out
	{
		get
		{
			return pending_out;
		}
		set
		{
			pending_out = value;
		}
	}

	public int NoHeader
	{
		get
		{
			return noheader;
		}
		set
		{
			noheader = value;
		}
	}

	internal Deflate()
	{
		dyn_ltree = new short[1146];
		dyn_dtree = new short[122];
		bl_tree = new short[78];
	}

	private void lm_init()
	{
		window_size = 2 * w_size;
		Array.Clear(head, 0, hash_size);
		max_lazy_match = config_table[level].max_lazy;
		good_match = config_table[level].good_length;
		nice_match = config_table[level].nice_length;
		max_chain_length = config_table[level].max_chain;
		strstart = 0;
		block_start = 0;
		lookahead = 0;
		match_length = (prev_length = 2);
		match_available = 0;
		ins_h = 0;
	}

	private void tr_init()
	{
		l_desc.DynTree = dyn_ltree;
		l_desc.StatDesc = StaticTree.static_l_desc;
		d_desc.DynTree = dyn_dtree;
		d_desc.StatDesc = StaticTree.static_d_desc;
		bl_desc.DynTree = bl_tree;
		bl_desc.StatDesc = StaticTree.static_bl_desc;
		bi_buf = 0;
		bi_valid = 0;
		last_eob_len = 8;
		init_block();
	}

	private void init_block()
	{
		for (int i = 0; i < 286; i++)
		{
			dyn_ltree[i * 2] = 0;
		}
		for (int j = 0; j < 30; j++)
		{
			dyn_dtree[j * 2] = 0;
		}
		for (int k = 0; k < 19; k++)
		{
			bl_tree[k * 2] = 0;
		}
		dyn_ltree[512] = 1;
		opt_len = (static_len = 0);
		last_lit = (matches = 0);
	}

	internal void pqdownheap(short[] tree, int k)
	{
		int num = heap[k];
		for (int num2 = k << 1; num2 <= heap_len; num2 <<= 1)
		{
			if (num2 < heap_len && smaller(tree, heap[num2 + 1], heap[num2], depth))
			{
				num2++;
			}
			if (smaller(tree, num, heap[num2], depth))
			{
				break;
			}
			heap[k] = heap[num2];
			k = num2;
		}
		heap[k] = num;
	}

	internal static bool smaller(short[] tree, int n, int m, byte[] depth)
	{
		if (tree[n * 2] >= tree[m * 2])
		{
			if (tree[n * 2] == tree[m * 2])
			{
				return depth[n] <= depth[m];
			}
			return false;
		}
		return true;
	}

	private void scan_tree(short[] tree, int max_code)
	{
		int num = -1;
		int num2 = tree[1];
		int num3 = 0;
		int num4 = 7;
		int num5 = 4;
		if (num2 == 0)
		{
			num4 = 138;
			num5 = 3;
		}
		tree[(max_code + 1) * 2 + 1] = (short)ZLibUtil.Identity(65535L);
		for (int i = 0; i <= max_code; i++)
		{
			int num6 = num2;
			num2 = tree[(i + 1) * 2 + 1];
			if (++num3 < num4 && num6 == num2)
			{
				continue;
			}
			if (num3 < num5)
			{
				bl_tree[num6 * 2] = (short)(bl_tree[num6 * 2] + num3);
			}
			else if (num6 != 0)
			{
				if (num6 != num)
				{
					bl_tree[num6 * 2]++;
				}
				bl_tree[32]++;
			}
			else if (num3 <= 10)
			{
				bl_tree[34]++;
			}
			else
			{
				bl_tree[36]++;
			}
			num3 = 0;
			num = num6;
			if (num2 == 0)
			{
				num4 = 138;
				num5 = 3;
			}
			else if (num6 == num2)
			{
				num4 = 6;
				num5 = 3;
			}
			else
			{
				num4 = 7;
				num5 = 4;
			}
		}
	}

	private int build_bl_tree()
	{
		scan_tree(dyn_ltree, l_desc.MaxCode);
		scan_tree(dyn_dtree, d_desc.MaxCode);
		bl_desc.build_tree(this);
		int num = 18;
		while (num >= 3 && bl_tree[ZLibUtil.bl_order[num] * 2 + 1] == 0)
		{
			num--;
		}
		opt_len += 3 * (num + 1) + 5 + 5 + 4;
		return num;
	}

	private void send_all_trees(int lcodes, int dcodes, int blcodes)
	{
		send_bits(lcodes - 257, 5);
		send_bits(dcodes - 1, 5);
		send_bits(blcodes - 4, 4);
		for (int i = 0; i < blcodes; i++)
		{
			send_bits(bl_tree[ZLibUtil.bl_order[i] * 2 + 1], 3);
		}
		send_tree(dyn_ltree, lcodes - 1);
		send_tree(dyn_dtree, dcodes - 1);
	}

	private void send_tree(short[] tree, int max_code)
	{
		int num = -1;
		int num2 = tree[1];
		int num3 = 0;
		int num4 = 7;
		int num5 = 4;
		if (num2 == 0)
		{
			num4 = 138;
			num5 = 3;
		}
		for (int i = 0; i <= max_code; i++)
		{
			int num6 = num2;
			num2 = tree[(i + 1) * 2 + 1];
			if (++num3 < num4 && num6 == num2)
			{
				continue;
			}
			if (num3 < num5)
			{
				do
				{
					send_code(num6, bl_tree);
				}
				while (--num3 != 0);
			}
			else if (num6 != 0)
			{
				if (num6 != num)
				{
					send_code(num6, bl_tree);
					num3--;
				}
				send_code(16, bl_tree);
				send_bits(num3 - 3, 2);
			}
			else if (num3 <= 10)
			{
				send_code(17, bl_tree);
				send_bits(num3 - 3, 3);
			}
			else
			{
				send_code(18, bl_tree);
				send_bits(num3 - 11, 7);
			}
			num3 = 0;
			num = num6;
			if (num2 == 0)
			{
				num4 = 138;
				num5 = 3;
			}
			else if (num6 == num2)
			{
				num4 = 6;
				num5 = 3;
			}
			else
			{
				num4 = 7;
				num5 = 4;
			}
		}
	}

	private void put_byte(byte[] p, int start, int len)
	{
		Array.Copy(p, start, Pending_buf, pending, len);
		pending += len;
	}

	private void put_byte(byte c)
	{
		Pending_buf[pending++] = c;
	}

	private void put_short(int w)
	{
		put_byte((byte)w);
		put_byte((byte)ZLibUtil.URShift(w, 8));
	}

	private void putShortMSB(int b)
	{
		put_byte((byte)(b >> 8));
		put_byte((byte)b);
	}

	private void send_code(int c, short[] tree)
	{
		send_bits(tree[c * 2] & 0xFFFF, tree[c * 2 + 1] & 0xFFFF);
	}

	private void send_bits(int value_Renamed, int length)
	{
		if (bi_valid > 16 - length)
		{
			bi_buf = (short)((ushort)bi_buf | (ushort)((value_Renamed << bi_valid) & 0xFFFF));
			put_short(bi_buf);
			bi_buf = (short)ZLibUtil.URShift(value_Renamed, 16 - bi_valid);
			bi_valid += length - 16;
		}
		else
		{
			bi_buf = (short)((ushort)bi_buf | (ushort)((value_Renamed << bi_valid) & 0xFFFF));
			bi_valid += length;
		}
	}

	private void _tr_align()
	{
		send_bits(2, 3);
		send_code(256, StaticTree.static_ltree);
		bi_flush();
		if (1 + last_eob_len + 10 - bi_valid < 9)
		{
			send_bits(2, 3);
			send_code(256, StaticTree.static_ltree);
			bi_flush();
		}
		last_eob_len = 7;
	}

	private bool _tr_tally(int dist, int lc)
	{
		Pending_buf[d_buf + last_lit * 2] = (byte)ZLibUtil.URShift(dist, 8);
		Pending_buf[d_buf + last_lit * 2 + 1] = (byte)dist;
		Pending_buf[l_buf + last_lit] = (byte)lc;
		last_lit++;
		if (dist == 0)
		{
			dyn_ltree[lc * 2]++;
		}
		else
		{
			matches++;
			dist--;
			dyn_ltree[(ZLibUtil._length_code[lc] + 256 + 1) * 2]++;
			dyn_dtree[Tree.d_code(dist) * 2]++;
		}
		if ((last_lit & 0x1FFF) == 0 && level > 2)
		{
			int num = last_lit * 8;
			int num2 = strstart - block_start;
			for (int i = 0; i < 30; i++)
			{
				num = (int)(num + dyn_dtree[i * 2] * (5L + (long)ZLibUtil.extra_dbits[i]));
			}
			num = ZLibUtil.URShift(num, 3);
			if (matches < last_lit / 2 && num < num2 / 2)
			{
				return true;
			}
		}
		return last_lit == lit_bufsize - 1;
	}

	private void compress_block(short[] ltree, short[] dtree)
	{
		int num = 0;
		if (last_lit != 0)
		{
			do
			{
				int num2 = ((Pending_buf[d_buf + num * 2] << 8) & 0xFF00) | (Pending_buf[d_buf + num * 2 + 1] & 0xFF);
				int num3 = Pending_buf[l_buf + num] & 0xFF;
				num++;
				if (num2 == 0)
				{
					send_code(num3, ltree);
					continue;
				}
				int num4 = ZLibUtil._length_code[num3];
				send_code(num4 + 256 + 1, ltree);
				int num5 = ZLibUtil.extra_lbits[num4];
				if (num5 != 0)
				{
					num3 -= ZLibUtil.base_length[num4];
					send_bits(num3, num5);
				}
				num2--;
				num4 = Tree.d_code(num2);
				send_code(num4, dtree);
				num5 = ZLibUtil.extra_dbits[num4];
				if (num5 != 0)
				{
					num2 -= ZLibUtil.base_dist[num4];
					send_bits(num2, num5);
				}
			}
			while (num < last_lit);
		}
		send_code(256, ltree);
		last_eob_len = ltree[513];
	}

	private void set_data_type()
	{
		int i = 0;
		int num = 0;
		int num2 = 0;
		for (; i < 7; i++)
		{
			num2 += dyn_ltree[i * 2];
		}
		for (; i < 128; i++)
		{
			num += dyn_ltree[i * 2];
		}
		for (; i < 256; i++)
		{
			num2 += dyn_ltree[i * 2];
		}
		data_type = ((num2 <= ZLibUtil.URShift(num, 2)) ? BlockType.Z_ASCII : BlockType.Z_BINARY);
	}

	private void bi_flush()
	{
		if (bi_valid == 16)
		{
			put_short(bi_buf);
			bi_buf = 0;
			bi_valid = 0;
		}
		else if (bi_valid >= 8)
		{
			put_byte((byte)bi_buf);
			bi_buf = (short)ZLibUtil.URShift(bi_buf, 8);
			bi_valid -= 8;
		}
	}

	private void bi_windup()
	{
		if (bi_valid > 8)
		{
			put_short(bi_buf);
		}
		else if (bi_valid > 0)
		{
			put_byte((byte)bi_buf);
		}
		bi_buf = 0;
		bi_valid = 0;
	}

	private void copy_block(int buf, int len, bool header)
	{
		bi_windup();
		last_eob_len = 8;
		if (header)
		{
			put_short((short)len);
			put_short((short)(~len));
		}
		put_byte(window, buf, len);
	}

	private void flush_block_only(bool eof)
	{
		_tr_flush_block((block_start >= 0) ? block_start : (-1), strstart - block_start, eof);
		block_start = strstart;
		strm.flush_pending();
	}

	private int deflate_stored(int flush)
	{
		int num = 65535;
		if (num > pending_buf_size - 5)
		{
			num = pending_buf_size - 5;
		}
		while (true)
		{
			if (lookahead <= 1)
			{
				fill_window();
				if (lookahead == 0 && flush == 0)
				{
					return 0;
				}
				if (lookahead == 0)
				{
					break;
				}
			}
			strstart += lookahead;
			lookahead = 0;
			int num2 = block_start + num;
			if (strstart == 0 || strstart >= num2)
			{
				lookahead = strstart - num2;
				strstart = num2;
				flush_block_only(eof: false);
				if (strm.avail_out == 0)
				{
					return 0;
				}
			}
			if (strstart - block_start >= w_size - 262)
			{
				flush_block_only(eof: false);
				if (strm.avail_out == 0)
				{
					return 0;
				}
			}
		}
		flush_block_only(flush == 4);
		if (strm.avail_out == 0)
		{
			if (flush != 4)
			{
				return 0;
			}
			return 2;
		}
		if (flush != 4)
		{
			return 1;
		}
		return 3;
	}

	private void _tr_stored_block(int buf, int stored_len, bool eof)
	{
		send_bits(eof ? 1 : 0, 3);
		copy_block(buf, stored_len, header: true);
	}

	private void _tr_flush_block(int buf, int stored_len, bool eof)
	{
		int num = 0;
		int num2;
		int num3;
		if (level > 0)
		{
			if (data_type == BlockType.Z_UNKNOWN)
			{
				set_data_type();
			}
			l_desc.build_tree(this);
			d_desc.build_tree(this);
			num = build_bl_tree();
			num2 = ZLibUtil.URShift(opt_len + 3 + 7, 3);
			num3 = ZLibUtil.URShift(static_len + 3 + 7, 3);
			if (num3 <= num2)
			{
				num2 = num3;
			}
		}
		else
		{
			num2 = (num3 = stored_len + 5);
		}
		if (stored_len + 4 <= num2 && buf != -1)
		{
			_tr_stored_block(buf, stored_len, eof);
		}
		else if (num3 == num2)
		{
			send_bits(2 + (eof ? 1 : 0), 3);
			compress_block(StaticTree.static_ltree, StaticTree.static_dtree);
		}
		else
		{
			send_bits(4 + (eof ? 1 : 0), 3);
			send_all_trees(l_desc.MaxCode + 1, d_desc.MaxCode + 1, num + 1);
			compress_block(dyn_ltree, dyn_dtree);
		}
		init_block();
		if (eof)
		{
			bi_windup();
		}
	}

	private void fill_window()
	{
		do
		{
			int num = window_size - lookahead - strstart;
			int num2;
			if (num == 0 && strstart == 0 && lookahead == 0)
			{
				num = w_size;
			}
			else if (num == -1)
			{
				num--;
			}
			else if (strstart >= w_size + w_size - 262)
			{
				Array.Copy(window, w_size, window, 0, w_size);
				match_start -= w_size;
				strstart -= w_size;
				block_start -= w_size;
				num2 = hash_size;
				int num3 = num2;
				do
				{
					int num4 = head[--num3] & 0xFFFF;
					head[num3] = (short)((num4 >= w_size) ? (num4 - w_size) : 0);
				}
				while (--num2 != 0);
				num2 = w_size;
				num3 = num2;
				do
				{
					int num4 = prev[--num3] & 0xFFFF;
					prev[num3] = (short)((num4 >= w_size) ? (num4 - w_size) : 0);
				}
				while (--num2 != 0);
				num += w_size;
			}
			if (strm.avail_in == 0)
			{
				break;
			}
			num2 = strm.read_buf(window, strstart + lookahead, num);
			lookahead += num2;
			if (lookahead >= 3)
			{
				ins_h = window[strstart] & 0xFF;
				ins_h = ((ins_h << hash_shift) ^ (window[strstart + 1] & 0xFF)) & hash_mask;
			}
		}
		while (lookahead < 262 && strm.avail_in != 0);
	}

	private int deflate_fast(int flush)
	{
		int num = 0;
		while (true)
		{
			if (lookahead < 262)
			{
				fill_window();
				if (lookahead < 262 && flush == 0)
				{
					return 0;
				}
				if (lookahead == 0)
				{
					break;
				}
			}
			if (lookahead >= 3)
			{
				ins_h = ((ins_h << hash_shift) ^ (window[strstart + 2] & 0xFF)) & hash_mask;
				num = head[ins_h] & 0xFFFF;
				prev[strstart & w_mask] = head[ins_h];
				head[ins_h] = (short)strstart;
			}
			if ((long)num != 0 && ((strstart - num) & 0xFFFF) <= w_size - 262 && strategy != CompressionStrategy.Z_HUFFMAN_ONLY)
			{
				match_length = longest_match(num);
			}
			bool flag;
			if (match_length >= 3)
			{
				flag = _tr_tally(strstart - match_start, match_length - 3);
				lookahead -= match_length;
				if (match_length <= max_lazy_match && lookahead >= 3)
				{
					match_length--;
					do
					{
						strstart++;
						ins_h = ((ins_h << hash_shift) ^ (window[strstart + 2] & 0xFF)) & hash_mask;
						num = head[ins_h] & 0xFFFF;
						prev[strstart & w_mask] = head[ins_h];
						head[ins_h] = (short)strstart;
					}
					while (--match_length != 0);
					strstart++;
				}
				else
				{
					strstart += match_length;
					match_length = 0;
					ins_h = window[strstart] & 0xFF;
					ins_h = ((ins_h << hash_shift) ^ (window[strstart + 1] & 0xFF)) & hash_mask;
				}
			}
			else
			{
				flag = _tr_tally(0, window[strstart] & 0xFF);
				lookahead--;
				strstart++;
			}
			if (flag)
			{
				flush_block_only(eof: false);
				if (strm.avail_out == 0)
				{
					return 0;
				}
			}
		}
		flush_block_only(flush == 4);
		if (strm.avail_out == 0)
		{
			if (flush == 4)
			{
				return 2;
			}
			return 0;
		}
		if (flush != 4)
		{
			return 1;
		}
		return 3;
	}

	private int deflate_slow(int flush)
	{
		int num = 0;
		while (true)
		{
			if (lookahead < 262)
			{
				fill_window();
				if (lookahead < 262 && flush == 0)
				{
					return 0;
				}
				if (lookahead == 0)
				{
					break;
				}
			}
			if (lookahead >= 3)
			{
				ins_h = ((ins_h << hash_shift) ^ (window[strstart + 2] & 0xFF)) & hash_mask;
				num = head[ins_h] & 0xFFFF;
				prev[strstart & w_mask] = head[ins_h];
				head[ins_h] = (short)strstart;
			}
			prev_length = match_length;
			prev_match = match_start;
			match_length = 2;
			if (num != 0 && prev_length < max_lazy_match && ((strstart - num) & 0xFFFF) <= w_size - 262)
			{
				if (strategy != CompressionStrategy.Z_HUFFMAN_ONLY)
				{
					match_length = longest_match(num);
				}
				if (match_length <= 5 && (strategy == CompressionStrategy.Z_FILTERED || (match_length == 3 && strstart - match_start > 4096)))
				{
					match_length = 2;
				}
			}
			if (prev_length >= 3 && match_length <= prev_length)
			{
				int num2 = strstart + lookahead - 3;
				bool flag = _tr_tally(strstart - 1 - prev_match, prev_length - 3);
				lookahead -= prev_length - 1;
				prev_length -= 2;
				do
				{
					if (++strstart <= num2)
					{
						ins_h = ((ins_h << hash_shift) ^ (window[strstart + 2] & 0xFF)) & hash_mask;
						num = head[ins_h] & 0xFFFF;
						prev[strstart & w_mask] = head[ins_h];
						head[ins_h] = (short)strstart;
					}
				}
				while (--prev_length != 0);
				match_available = 0;
				match_length = 2;
				strstart++;
				if (flag)
				{
					flush_block_only(eof: false);
					if (strm.avail_out == 0)
					{
						return 0;
					}
				}
			}
			else if (match_available != 0)
			{
				if (_tr_tally(0, window[strstart - 1] & 0xFF))
				{
					flush_block_only(eof: false);
				}
				strstart++;
				lookahead--;
				if (strm.avail_out == 0)
				{
					return 0;
				}
			}
			else
			{
				match_available = 1;
				strstart++;
				lookahead--;
			}
		}
		if (match_available != 0)
		{
			bool flag = _tr_tally(0, window[strstart - 1] & 0xFF);
			match_available = 0;
		}
		flush_block_only(flush == 4);
		if (strm.avail_out == 0)
		{
			if (flush == 4)
			{
				return 2;
			}
			return 0;
		}
		if (flush != 4)
		{
			return 1;
		}
		return 3;
	}

	private int longest_match(int cur_match)
	{
		int num = max_chain_length;
		int num2 = strstart;
		int num3 = prev_length;
		int num4 = ((strstart > w_size - 262) ? (strstart - (w_size - 262)) : 0);
		int num5 = nice_match;
		int num6 = w_mask;
		int num7 = strstart + 258;
		byte b = window[num2 + num3 - 1];
		byte b2 = window[num2 + num3];
		if (prev_length >= good_match)
		{
			num >>= 2;
		}
		if (num5 > lookahead)
		{
			num5 = lookahead;
		}
		do
		{
			int num8 = cur_match;
			if (window[num8 + num3] != b2 || window[num8 + num3 - 1] != b || window[num8] != window[num2] || window[++num8] != window[num2 + 1])
			{
				continue;
			}
			num2 += 2;
			num8++;
			while (window[++num2] == window[++num8] && window[++num2] == window[++num8] && window[++num2] == window[++num8] && window[++num2] == window[++num8] && window[++num2] == window[++num8] && window[++num2] == window[++num8] && window[++num2] == window[++num8] && window[++num2] == window[++num8] && num2 < num7)
			{
			}
			int num9 = 258 - (num7 - num2);
			num2 = num7 - 258;
			if (num9 > num3)
			{
				match_start = cur_match;
				num3 = num9;
				if (num9 >= num5)
				{
					break;
				}
				b = window[num2 + num3 - 1];
				b2 = window[num2 + num3];
			}
		}
		while ((cur_match = prev[cur_match & num6] & 0xFFFF) > num4 && --num != 0);
		if (num3 <= lookahead)
		{
			return num3;
		}
		return lookahead;
	}

	internal int deflateInit(ZStream strm, int level, int bits)
	{
		return deflateInit2(strm, level, 8, bits, 8, CompressionStrategy.Z_DEFAULT_STRATEGY);
	}

	internal int deflateInit(ZStream strm, int level)
	{
		return deflateInit(strm, level, 15);
	}

	internal int deflateInit2(ZStream strm, int level, int method, int windowBits, int memLevel, CompressionStrategy strategy)
	{
		int noHeader = 0;
		strm.msg = null;
		if (level == -1)
		{
			level = 6;
		}
		if (windowBits < 0)
		{
			noHeader = 1;
			windowBits = -windowBits;
		}
		if (memLevel < 1 || memLevel > 9 || method != 8 || windowBits < 9 || windowBits > 15 || level < 0 || level > 9 || strategy < CompressionStrategy.Z_DEFAULT_STRATEGY || strategy > CompressionStrategy.Z_HUFFMAN_ONLY)
		{
			return -2;
		}
		strm.dstate = this;
		NoHeader = noHeader;
		w_bits = windowBits;
		w_size = 1 << w_bits;
		w_mask = w_size - 1;
		hash_bits = memLevel + 7;
		hash_size = 1 << hash_bits;
		hash_mask = hash_size - 1;
		hash_shift = (hash_bits + 3 - 1) / 3;
		window = new byte[w_size * 2];
		prev = new short[w_size];
		head = new short[hash_size];
		lit_bufsize = 1 << memLevel + 6;
		Pending_buf = new byte[lit_bufsize * 4];
		pending_buf_size = lit_bufsize * 4;
		d_buf = lit_bufsize;
		l_buf = 3 * lit_bufsize;
		this.level = level;
		this.strategy = strategy;
		this.method = (byte)method;
		return deflateReset(strm);
	}

	internal int deflateReset(ZStream strm)
	{
		long total_in = (strm.total_out = 0L);
		strm.total_in = total_in;
		strm.msg = null;
		strm.Data_type = BlockType.Z_UNKNOWN;
		pending = 0;
		Pending_out = 0;
		if (NoHeader < 0)
		{
			NoHeader = 0;
		}
		status = ((NoHeader != 0) ? DeflateState.BUSY_STATE : DeflateState.INIT_STATE);
		strm.adler = Adler32.GetAdler32Checksum(0L, null, 0, 0);
		last_flush = 0;
		tr_init();
		lm_init();
		return 0;
	}

	internal int deflateEnd()
	{
		if (status != DeflateState.INIT_STATE && status != DeflateState.BUSY_STATE && status != DeflateState.FINISH_STATE)
		{
			return -2;
		}
		Pending_buf = null;
		head = null;
		prev = null;
		window = null;
		if (status != DeflateState.BUSY_STATE)
		{
			return 0;
		}
		return -3;
	}

	internal int deflateParams(ZStream strm, int level, CompressionStrategy strategy)
	{
		int result = 0;
		if (level == -1)
		{
			level = 6;
		}
		if (level < 0 || level > 9 || strategy < CompressionStrategy.Z_DEFAULT_STRATEGY || strategy > CompressionStrategy.Z_HUFFMAN_ONLY)
		{
			return -2;
		}
		if (config_table[_level].func != config_table[level].func && strm.total_in != 0)
		{
			result = strm.deflate(FlushStrategy.Z_PARTIAL_FLUSH);
		}
		if (_level != level)
		{
			_level = level;
			max_lazy_match = config_table[_level].max_lazy;
			good_match = config_table[_level].good_length;
			nice_match = config_table[_level].nice_length;
			max_chain_length = config_table[_level].max_chain;
		}
		this.strategy = strategy;
		return result;
	}

	internal int deflateSetDictionary(ZStream strm, byte[] dictionary, int dictLength)
	{
		int num = dictLength;
		int sourceIndex = 0;
		if (dictionary == null || status != DeflateState.INIT_STATE)
		{
			return -2;
		}
		strm.adler = Adler32.GetAdler32Checksum(strm.adler, dictionary, 0, dictLength);
		if (num < 3)
		{
			return 0;
		}
		if (num > w_size - 262)
		{
			num = w_size - 262;
			sourceIndex = dictLength - num;
		}
		Array.Copy(dictionary, sourceIndex, window, 0, num);
		strstart = num;
		block_start = num;
		ins_h = window[0] & 0xFF;
		ins_h = ((ins_h << hash_shift) ^ (window[1] & 0xFF)) & hash_mask;
		for (int i = 0; i <= num - 3; i++)
		{
			ins_h = ((ins_h << hash_shift) ^ (window[i + 2] & 0xFF)) & hash_mask;
			prev[i & w_mask] = head[ins_h];
			head[ins_h] = (short)i;
		}
		return 0;
	}

	internal int deflate(ZStream strm, FlushStrategy f)
	{
		if (f > FlushStrategy.Z_FINISH || f < FlushStrategy.Z_NO_FLUSH)
		{
			return -2;
		}
		if (strm.next_out == null || (strm.next_in == null && strm.avail_in != 0) || (status == DeflateState.FINISH_STATE && f != FlushStrategy.Z_FINISH))
		{
			strm.msg = ZLibUtil.z_errmsg[4];
			return -2;
		}
		if (strm.avail_out == 0)
		{
			strm.msg = ZLibUtil.z_errmsg[7];
			return -5;
		}
		this.strm = strm;
		int num = last_flush;
		last_flush = (int)f;
		if (status == DeflateState.INIT_STATE)
		{
			int num2 = 8 + (w_bits - 8 << 4) << 8;
			int num3 = ((level - 1) & 0xFF) >> 1;
			if (num3 > 3)
			{
				num3 = 3;
			}
			num2 |= num3 << 6;
			if (strstart != 0)
			{
				num2 |= 0x20;
			}
			num2 += 31 - num2 % 31;
			status = DeflateState.BUSY_STATE;
			putShortMSB(num2);
			if (strstart != 0)
			{
				putShortMSB((int)ZLibUtil.URShift(strm.adler, 16));
				putShortMSB((int)(strm.adler & 0xFFFF));
			}
			strm.adler = Adler32.GetAdler32Checksum(0L, null, 0, 0);
		}
		if (pending != 0)
		{
			strm.flush_pending();
			if (strm.avail_out == 0)
			{
				last_flush = -1;
				return 0;
			}
		}
		else if (strm.avail_in == 0 && (int)f <= num && f != FlushStrategy.Z_FINISH)
		{
			strm.msg = ZLibUtil.z_errmsg[7];
			return -5;
		}
		if (status == DeflateState.FINISH_STATE && strm.avail_in != 0)
		{
			strm.msg = ZLibUtil.z_errmsg[7];
			return -5;
		}
		if (strm.avail_in != 0 || lookahead != 0 || (f != FlushStrategy.Z_NO_FLUSH && status != DeflateState.FINISH_STATE))
		{
			int num4 = -1;
			switch (config_table[level].func)
			{
			case 0:
				num4 = deflate_stored((int)f);
				break;
			case 1:
				num4 = deflate_fast((int)f);
				break;
			case 2:
				num4 = deflate_slow((int)f);
				break;
			}
			if (num4 == 2 || num4 == 3)
			{
				status = DeflateState.FINISH_STATE;
			}
			switch (num4)
			{
			case 0:
			case 2:
				if (strm.avail_out == 0)
				{
					last_flush = -1;
				}
				return 0;
			case 1:
				if (f == FlushStrategy.Z_PARTIAL_FLUSH)
				{
					_tr_align();
				}
				else
				{
					_tr_stored_block(0, 0, eof: false);
					if (f == FlushStrategy.Z_FULL_FLUSH)
					{
						for (int i = 0; i < hash_size; i++)
						{
							head[i] = 0;
						}
					}
				}
				strm.flush_pending();
				if (strm.avail_out == 0)
				{
					last_flush = -1;
					return 0;
				}
				break;
			}
		}
		if (f != FlushStrategy.Z_FINISH)
		{
			return 0;
		}
		if (NoHeader != 0)
		{
			return 1;
		}
		putShortMSB((int)ZLibUtil.URShift(strm.adler, 16));
		putShortMSB((int)(strm.adler & 0xFFFF));
		strm.flush_pending();
		NoHeader = -1;
		if (pending == 0)
		{
			return 1;
		}
		return 0;
	}

	static Deflate()
	{
		config_table = new Config[10];
		config_table[0] = new Config(0, 0, 0, 0, 0);
		config_table[1] = new Config(4, 4, 8, 4, 1);
		config_table[2] = new Config(4, 5, 16, 8, 1);
		config_table[3] = new Config(4, 6, 32, 32, 1);
		config_table[4] = new Config(4, 4, 16, 16, 2);
		config_table[5] = new Config(8, 16, 32, 32, 2);
		config_table[6] = new Config(8, 16, 128, 128, 2);
		config_table[7] = new Config(8, 32, 128, 256, 2);
		config_table[8] = new Config(32, 128, 258, 1024, 2);
		config_table[9] = new Config(32, 258, 258, 4096, 2);
	}
}
