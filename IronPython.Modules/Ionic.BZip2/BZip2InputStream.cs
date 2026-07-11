using System;
using System.IO;
using Ionic.Crc;

namespace Ionic.BZip2;

public class BZip2InputStream : Stream
{
	private enum CState
	{
		EOF,
		START_BLOCK,
		RAND_PART_A,
		RAND_PART_B,
		RAND_PART_C,
		NO_RAND_PART_A,
		NO_RAND_PART_B,
		NO_RAND_PART_C
	}

	private sealed class DecompressionState
	{
		public readonly bool[] inUse = new bool[256];

		public readonly byte[] seqToUnseq = new byte[256];

		public readonly byte[] selector = new byte[BZip2.MaxSelectors];

		public readonly byte[] selectorMtf = new byte[BZip2.MaxSelectors];

		public readonly int[] unzftab;

		public readonly int[][] gLimit;

		public readonly int[][] gBase;

		public readonly int[][] gPerm;

		public readonly int[] gMinlen;

		public readonly int[] cftab;

		public readonly byte[] getAndMoveToFrontDecode_yy;

		public readonly char[][] temp_charArray2d;

		public readonly byte[] recvDecodingTables_pos;

		public int[] tt;

		public byte[] ll8;

		public DecompressionState(int blockSize100k)
		{
			unzftab = new int[256];
			gLimit = BZip2.InitRectangularArray<int>(BZip2.NGroups, BZip2.MaxAlphaSize);
			gBase = BZip2.InitRectangularArray<int>(BZip2.NGroups, BZip2.MaxAlphaSize);
			gPerm = BZip2.InitRectangularArray<int>(BZip2.NGroups, BZip2.MaxAlphaSize);
			gMinlen = new int[BZip2.NGroups];
			cftab = new int[257];
			getAndMoveToFrontDecode_yy = new byte[256];
			temp_charArray2d = BZip2.InitRectangularArray<char>(BZip2.NGroups, BZip2.MaxAlphaSize);
			recvDecodingTables_pos = new byte[BZip2.NGroups];
			ll8 = new byte[blockSize100k * BZip2.BlockSizeMultiple];
		}

		public int[] initTT(int length)
		{
			int[] array = tt;
			if (array == null || array.Length < length)
			{
				array = (tt = new int[length]);
			}
			return array;
		}
	}

	private struct StateDump
	{
		public bool _disposed;

		public long totalBytesRead;

		public int last;

		public int origPtr;

		public bool blockRandomised;

		public int bsBuff;

		public int bsLive;

		public int nInUse;

		public int currentChar;

		public CState currentState;

		public uint storedBlockCRC;

		public uint storedCombinedCRC;

		public uint computedBlockCRC;

		public uint computedCombinedCRC;

		public int su_count;

		public int su_ch2;

		public int su_chPrev;

		public int su_i2;

		public int su_j2;

		public int su_rNToGo;

		public int su_rTPos;

		public int su_tPos;

		public char su_z;
	}

	private bool _disposed;

	private bool _leaveOpen;

	private long totalBytesRead;

	private int last;

	private int origPtr;

	private int blockSize100k;

	private bool blockRandomised;

	private int bsBuff;

	private int bsLive;

	private readonly CRC32 crc = new CRC32(reverseBits: true);

	private int nInUse;

	private Stream input;

	private int currentChar = -1;

	private CState currentState = CState.START_BLOCK;

	private uint storedBlockCRC;

	private uint storedCombinedCRC;

	private uint computedBlockCRC;

	private uint computedCombinedCRC;

	private int su_count;

	private int su_ch2;

	private int su_chPrev;

	private int su_i2;

	private int su_j2;

	private int su_rNToGo;

	private int su_rTPos;

	private int su_tPos;

	private char su_z;

	private DecompressionState data;

	public override bool CanRead
	{
		get
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("BZip2Stream");
			}
			return input.CanRead;
		}
	}

	public override bool CanSeek => false;

	public override bool CanWrite
	{
		get
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("BZip2Stream");
			}
			return input.CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Position
	{
		get
		{
			return totalBytesRead;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public BZip2InputStream(Stream input)
		: this(input, leaveOpen: false)
	{
	}

	public BZip2InputStream(Stream input, bool leaveOpen)
	{
		this.input = input;
		_leaveOpen = leaveOpen;
		init();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (offset < 0)
		{
			throw new IndexOutOfRangeException($"offset ({offset}) must be > 0");
		}
		if (count < 0)
		{
			throw new IndexOutOfRangeException($"count ({count}) must be > 0");
		}
		if (offset + count > buffer.Length)
		{
			throw new IndexOutOfRangeException($"offset({offset}) count({count}) bLength({buffer.Length})");
		}
		if (input == null)
		{
			throw new IOException("the stream is not open");
		}
		int num = offset + count;
		int num2 = offset;
		int num3;
		while (num2 < num && (num3 = ReadByte()) >= 0)
		{
			buffer[num2++] = (byte)num3;
		}
		return num2 - offset;
	}

	private void MakeMaps()
	{
		bool[] inUse = data.inUse;
		byte[] seqToUnseq = data.seqToUnseq;
		int num = 0;
		for (int i = 0; i < 256; i++)
		{
			if (inUse[i])
			{
				seqToUnseq[num++] = (byte)i;
			}
		}
		nInUse = num;
	}

	public override int ReadByte()
	{
		int result = currentChar;
		totalBytesRead++;
		switch (currentState)
		{
		case CState.EOF:
			return -1;
		case CState.START_BLOCK:
			throw new IOException("bad state");
		case CState.RAND_PART_A:
			throw new IOException("bad state");
		case CState.RAND_PART_B:
			SetupRandPartB();
			break;
		case CState.RAND_PART_C:
			SetupRandPartC();
			break;
		case CState.NO_RAND_PART_A:
			throw new IOException("bad state");
		case CState.NO_RAND_PART_B:
			SetupNoRandPartB();
			break;
		case CState.NO_RAND_PART_C:
			SetupNoRandPartC();
			break;
		default:
			throw new IOException("bad state");
		}
		return result;
	}

	public override void Flush()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("BZip2Stream");
		}
		input.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!_disposed)
			{
				if (disposing && input != null)
				{
					input.Close();
				}
				_disposed = true;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void init()
	{
		if (input == null)
		{
			throw new IOException("No input Stream");
		}
		if (!input.CanRead)
		{
			throw new IOException("Unreadable input Stream");
		}
		CheckMagicChar('B', 0);
		CheckMagicChar('Z', 1);
		CheckMagicChar('h', 2);
		int num = input.ReadByte();
		if (num < 49 || num > 57)
		{
			throw new IOException("Stream is not BZip2 formatted: illegal blocksize " + (char)num);
		}
		blockSize100k = num - 48;
		InitBlock();
		SetupBlock();
	}

	private void CheckMagicChar(char expected, int position)
	{
		int num = input.ReadByte();
		if (num != expected)
		{
			string message = $"Not a valid BZip2 stream. byte {position}, expected '{(int)expected}', got '{num}'";
			throw new IOException(message);
		}
	}

	private void InitBlock()
	{
		char c = bsGetUByte();
		char c2 = bsGetUByte();
		char c3 = bsGetUByte();
		char c4 = bsGetUByte();
		char c5 = bsGetUByte();
		char c6 = bsGetUByte();
		if (c == '\u0017' && c2 == 'r' && c3 == 'E' && c4 == '8' && c5 == 'P' && c6 == '\u0090')
		{
			complete();
			return;
		}
		if (c != '1' || c2 != 'A' || c3 != 'Y' || c4 != '&' || c5 != 'S' || c6 != 'Y')
		{
			currentState = CState.EOF;
			string message = $"bad block header at offset 0x{input.Position:X}";
			throw new IOException(message);
		}
		storedBlockCRC = bsGetInt();
		blockRandomised = GetBits(1) == 1;
		if (data == null)
		{
			data = new DecompressionState(blockSize100k);
		}
		getAndMoveToFrontDecode();
		crc.Reset();
		currentState = CState.START_BLOCK;
	}

	private void EndBlock()
	{
		computedBlockCRC = (uint)crc.Crc32Result;
		if (storedBlockCRC != computedBlockCRC)
		{
			string message = $"BZip2 CRC error (expected {storedBlockCRC:X8}, computed {computedBlockCRC:X8})";
			throw new IOException(message);
		}
		computedCombinedCRC = (computedCombinedCRC << 1) | (computedCombinedCRC >> 31);
		computedCombinedCRC ^= computedBlockCRC;
	}

	private void complete()
	{
		storedCombinedCRC = bsGetInt();
		currentState = CState.EOF;
		data = null;
		if (storedCombinedCRC != computedCombinedCRC)
		{
			string message = $"BZip2 CRC error (expected {storedCombinedCRC:X8}, computed {computedCombinedCRC:X8})";
			throw new IOException(message);
		}
	}

	public override void Close()
	{
		Stream stream = input;
		if (stream == null)
		{
			return;
		}
		try
		{
			if (!_leaveOpen)
			{
				stream.Close();
			}
		}
		finally
		{
			data = null;
			input = null;
		}
	}

	private int GetBits(int n)
	{
		int num = bsLive;
		int num2 = bsBuff;
		if (num < n)
		{
			do
			{
				int num3 = input.ReadByte();
				if (num3 < 0)
				{
					throw new IOException("unexpected end of stream");
				}
				num2 = (num2 << 8) | num3;
				num += 8;
			}
			while (num < n);
			bsBuff = num2;
		}
		bsLive = num - n;
		return (num2 >> num - n) & ((1 << n) - 1);
	}

	private bool bsGetBit()
	{
		int bits = GetBits(1);
		return bits != 0;
	}

	private char bsGetUByte()
	{
		return (char)GetBits(8);
	}

	private uint bsGetInt()
	{
		return (uint)((((((GetBits(8) << 8) | GetBits(8)) << 8) | GetBits(8)) << 8) | GetBits(8));
	}

	private static void hbCreateDecodeTables(int[] limit, int[] bbase, int[] perm, char[] length, int minLen, int maxLen, int alphaSize)
	{
		int i = minLen;
		int num = 0;
		for (; i <= maxLen; i++)
		{
			for (int j = 0; j < alphaSize; j++)
			{
				if (length[j] == i)
				{
					perm[num++] = j;
				}
			}
		}
		int num2 = BZip2.MaxCodeLength;
		while (--num2 > 0)
		{
			bbase[num2] = 0;
			limit[num2] = 0;
		}
		for (int k = 0; k < alphaSize; k++)
		{
			bbase[length[k] + 1]++;
		}
		int l = 1;
		int num3 = bbase[0];
		for (; l < BZip2.MaxCodeLength; l++)
		{
			num3 = (bbase[l] = num3 + bbase[l]);
		}
		int m = minLen;
		int num4 = 0;
		int num5 = bbase[m];
		for (; m <= maxLen; m++)
		{
			int num6 = bbase[m + 1];
			num4 += num6 - num5;
			num5 = num6;
			limit[m] = num4 - 1;
			num4 <<= 1;
		}
		for (int n = minLen + 1; n <= maxLen; n++)
		{
			bbase[n] = (limit[n - 1] + 1 << 1) - bbase[n];
		}
	}

	private void recvDecodingTables()
	{
		DecompressionState decompressionState = data;
		bool[] inUse = decompressionState.inUse;
		byte[] recvDecodingTables_pos = decompressionState.recvDecodingTables_pos;
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (bsGetBit())
			{
				num |= 1 << i;
			}
		}
		int num2 = 256;
		while (--num2 >= 0)
		{
			inUse[num2] = false;
		}
		for (int j = 0; j < 16; j++)
		{
			if ((num & (1 << j)) == 0)
			{
				continue;
			}
			int num3 = j << 4;
			for (int k = 0; k < 16; k++)
			{
				if (bsGetBit())
				{
					inUse[num3 + k] = true;
				}
			}
		}
		MakeMaps();
		int num4 = nInUse + 2;
		int bits = GetBits(3);
		int bits2 = GetBits(15);
		for (int l = 0; l < bits2; l++)
		{
			int num5 = 0;
			while (bsGetBit())
			{
				num5++;
			}
			decompressionState.selectorMtf[l] = (byte)num5;
		}
		int num6 = bits;
		while (--num6 >= 0)
		{
			recvDecodingTables_pos[num6] = (byte)num6;
		}
		for (int m = 0; m < bits2; m++)
		{
			int num7 = decompressionState.selectorMtf[m];
			byte b = recvDecodingTables_pos[num7];
			while (num7 > 0)
			{
				recvDecodingTables_pos[num7] = recvDecodingTables_pos[num7 - 1];
				num7--;
			}
			recvDecodingTables_pos[0] = b;
			decompressionState.selector[m] = b;
		}
		char[][] temp_charArray2d = decompressionState.temp_charArray2d;
		for (int n = 0; n < bits; n++)
		{
			int num8 = GetBits(5);
			char[] array = temp_charArray2d[n];
			for (int num9 = 0; num9 < num4; num9++)
			{
				while (bsGetBit())
				{
					num8 += ((!bsGetBit()) ? 1 : (-1));
				}
				array[num9] = (char)num8;
			}
		}
		createHuffmanDecodingTables(num4, bits);
	}

	private void createHuffmanDecodingTables(int alphaSize, int nGroups)
	{
		DecompressionState decompressionState = data;
		char[][] temp_charArray2d = decompressionState.temp_charArray2d;
		for (int i = 0; i < nGroups; i++)
		{
			int num = 32;
			int num2 = 0;
			char[] array = temp_charArray2d[i];
			int num3 = alphaSize;
			while (--num3 >= 0)
			{
				char c = array[num3];
				if (c > num2)
				{
					num2 = c;
				}
				if (c < num)
				{
					num = c;
				}
			}
			hbCreateDecodeTables(decompressionState.gLimit[i], decompressionState.gBase[i], decompressionState.gPerm[i], temp_charArray2d[i], num, num2, alphaSize);
			decompressionState.gMinlen[i] = num;
		}
	}

	private void getAndMoveToFrontDecode()
	{
		DecompressionState decompressionState = data;
		origPtr = GetBits(24);
		if (origPtr < 0)
		{
			throw new IOException("BZ_DATA_ERROR");
		}
		if (origPtr > 10 + BZip2.BlockSizeMultiple * blockSize100k)
		{
			throw new IOException("BZ_DATA_ERROR");
		}
		recvDecodingTables();
		byte[] getAndMoveToFrontDecode_yy = decompressionState.getAndMoveToFrontDecode_yy;
		int num = blockSize100k * BZip2.BlockSizeMultiple;
		int num2 = 256;
		while (--num2 >= 0)
		{
			getAndMoveToFrontDecode_yy[num2] = (byte)num2;
			decompressionState.unzftab[num2] = 0;
		}
		int num3 = 0;
		int num4 = BZip2.G_SIZE - 1;
		int num5 = nInUse + 1;
		int num6 = getAndMoveToFrontDecode0(0);
		int num7 = bsBuff;
		int i = bsLive;
		int num8 = -1;
		int num9 = decompressionState.selector[num3] & 0xFF;
		int[] array = decompressionState.gBase[num9];
		int[] array2 = decompressionState.gLimit[num9];
		int[] array3 = decompressionState.gPerm[num9];
		int num10 = decompressionState.gMinlen[num9];
		while (num6 != num5)
		{
			if (num6 == BZip2.RUNA || num6 == BZip2.RUNB)
			{
				int num11 = -1;
				int num12 = 1;
				while (true)
				{
					if (num6 == BZip2.RUNA)
					{
						num11 += num12;
					}
					else
					{
						if (num6 != BZip2.RUNB)
						{
							break;
						}
						num11 += num12 << 1;
					}
					if (num4 == 0)
					{
						num4 = BZip2.G_SIZE - 1;
						num9 = decompressionState.selector[++num3] & 0xFF;
						array = decompressionState.gBase[num9];
						array2 = decompressionState.gLimit[num9];
						array3 = decompressionState.gPerm[num9];
						num10 = decompressionState.gMinlen[num9];
					}
					else
					{
						num4--;
					}
					int num13;
					for (num13 = num10; i < num13; i += 8)
					{
						int num14 = input.ReadByte();
						if (num14 >= 0)
						{
							num7 = (num7 << 8) | num14;
							continue;
						}
						throw new IOException("unexpected end of stream");
					}
					int num15 = (num7 >> i - num13) & ((1 << num13) - 1);
					i -= num13;
					while (num15 > array2[num13])
					{
						num13++;
						for (; i < 1; i += 8)
						{
							int num16 = input.ReadByte();
							if (num16 >= 0)
							{
								num7 = (num7 << 8) | num16;
								continue;
							}
							throw new IOException("unexpected end of stream");
						}
						i--;
						num15 = (num15 << 1) | ((num7 >> i) & 1);
					}
					num6 = array3[num15 - array[num13]];
					num12 <<= 1;
				}
				byte b = decompressionState.seqToUnseq[getAndMoveToFrontDecode_yy[0]];
				decompressionState.unzftab[b & 0xFF] += num11 + 1;
				while (num11-- >= 0)
				{
					decompressionState.ll8[++num8] = b;
				}
				if (num8 >= num)
				{
					throw new IOException("block overrun");
				}
				continue;
			}
			if (++num8 >= num)
			{
				throw new IOException("block overrun");
			}
			byte b2 = getAndMoveToFrontDecode_yy[num6 - 1];
			decompressionState.unzftab[decompressionState.seqToUnseq[b2] & 0xFF]++;
			decompressionState.ll8[num8] = decompressionState.seqToUnseq[b2];
			if (num6 <= 16)
			{
				int num17 = num6 - 1;
				while (num17 > 0)
				{
					getAndMoveToFrontDecode_yy[num17] = getAndMoveToFrontDecode_yy[--num17];
				}
			}
			else
			{
				Buffer.BlockCopy(getAndMoveToFrontDecode_yy, 0, getAndMoveToFrontDecode_yy, 1, num6 - 1);
			}
			getAndMoveToFrontDecode_yy[0] = b2;
			if (num4 == 0)
			{
				num4 = BZip2.G_SIZE - 1;
				num9 = decompressionState.selector[++num3] & 0xFF;
				array = decompressionState.gBase[num9];
				array2 = decompressionState.gLimit[num9];
				array3 = decompressionState.gPerm[num9];
				num10 = decompressionState.gMinlen[num9];
			}
			else
			{
				num4--;
			}
			int num18;
			for (num18 = num10; i < num18; i += 8)
			{
				int num19 = input.ReadByte();
				if (num19 >= 0)
				{
					num7 = (num7 << 8) | num19;
					continue;
				}
				throw new IOException("unexpected end of stream");
			}
			int num20 = (num7 >> i - num18) & ((1 << num18) - 1);
			i -= num18;
			while (num20 > array2[num18])
			{
				num18++;
				for (; i < 1; i += 8)
				{
					int num21 = input.ReadByte();
					if (num21 >= 0)
					{
						num7 = (num7 << 8) | num21;
						continue;
					}
					throw new IOException("unexpected end of stream");
				}
				i--;
				num20 = (num20 << 1) | ((num7 >> i) & 1);
			}
			num6 = array3[num20 - array[num18]];
		}
		last = num8;
		bsLive = i;
		bsBuff = num7;
	}

	private int getAndMoveToFrontDecode0(int groupNo)
	{
		DecompressionState decompressionState = data;
		int num = decompressionState.selector[groupNo] & 0xFF;
		int[] array = decompressionState.gLimit[num];
		int num2 = decompressionState.gMinlen[num];
		int num3 = GetBits(num2);
		int i = bsLive;
		int num4 = bsBuff;
		while (num3 > array[num2])
		{
			num2++;
			for (; i < 1; i += 8)
			{
				int num5 = input.ReadByte();
				if (num5 >= 0)
				{
					num4 = (num4 << 8) | num5;
					continue;
				}
				throw new IOException("unexpected end of stream");
			}
			i--;
			num3 = (num3 << 1) | ((num4 >> i) & 1);
		}
		bsLive = i;
		bsBuff = num4;
		return decompressionState.gPerm[num][num3 - decompressionState.gBase[num][num2]];
	}

	private void SetupBlock()
	{
		if (data == null)
		{
			return;
		}
		DecompressionState decompressionState = data;
		int[] array = decompressionState.initTT(last + 1);
		int i;
		for (i = 0; i <= 255; i++)
		{
			if (decompressionState.unzftab[i] < 0 || decompressionState.unzftab[i] > last)
			{
				throw new Exception("BZ_DATA_ERROR");
			}
		}
		decompressionState.cftab[0] = 0;
		for (i = 1; i <= 256; i++)
		{
			decompressionState.cftab[i] = decompressionState.unzftab[i - 1];
		}
		for (i = 1; i <= 256; i++)
		{
			decompressionState.cftab[i] += decompressionState.cftab[i - 1];
		}
		for (i = 0; i <= 256; i++)
		{
			if (decompressionState.cftab[i] < 0 || decompressionState.cftab[i] > last + 1)
			{
				string message = $"BZ_DATA_ERROR: cftab[{i}]={decompressionState.cftab[i]} last={last}";
				throw new Exception(message);
			}
		}
		for (i = 1; i <= 256; i++)
		{
			if (decompressionState.cftab[i - 1] > decompressionState.cftab[i])
			{
				throw new Exception("BZ_DATA_ERROR");
			}
		}
		i = 0;
		for (int num = last; i <= num; i++)
		{
			array[decompressionState.cftab[decompressionState.ll8[i] & 0xFF]++] = i;
		}
		if (origPtr < 0 || origPtr >= array.Length)
		{
			throw new IOException("stream corrupted");
		}
		su_tPos = array[origPtr];
		su_count = 0;
		su_i2 = 0;
		su_ch2 = 256;
		if (blockRandomised)
		{
			su_rNToGo = 0;
			su_rTPos = 0;
			SetupRandPartA();
		}
		else
		{
			SetupNoRandPartA();
		}
	}

	private void SetupRandPartA()
	{
		if (su_i2 <= last)
		{
			su_chPrev = su_ch2;
			int num = data.ll8[su_tPos] & 0xFF;
			su_tPos = data.tt[su_tPos];
			if (su_rNToGo == 0)
			{
				su_rNToGo = Rand.Rnums(su_rTPos) - 1;
				if (++su_rTPos == 512)
				{
					su_rTPos = 0;
				}
			}
			else
			{
				su_rNToGo--;
			}
			num = (su_ch2 = num ^ ((su_rNToGo == 1) ? 1 : 0));
			su_i2++;
			currentChar = num;
			currentState = CState.RAND_PART_B;
			crc.UpdateCRC((byte)num);
		}
		else
		{
			EndBlock();
			InitBlock();
			SetupBlock();
		}
	}

	private void SetupNoRandPartA()
	{
		if (su_i2 <= last)
		{
			su_chPrev = su_ch2;
			int num = (su_ch2 = data.ll8[su_tPos] & 0xFF);
			su_tPos = data.tt[su_tPos];
			su_i2++;
			currentChar = num;
			currentState = CState.NO_RAND_PART_B;
			crc.UpdateCRC((byte)num);
		}
		else
		{
			currentState = CState.NO_RAND_PART_A;
			EndBlock();
			InitBlock();
			SetupBlock();
		}
	}

	private void SetupRandPartB()
	{
		if (su_ch2 != su_chPrev)
		{
			currentState = CState.RAND_PART_A;
			su_count = 1;
			SetupRandPartA();
		}
		else if (++su_count >= 4)
		{
			su_z = (char)(data.ll8[su_tPos] & 0xFF);
			su_tPos = data.tt[su_tPos];
			if (su_rNToGo == 0)
			{
				su_rNToGo = Rand.Rnums(su_rTPos) - 1;
				if (++su_rTPos == 512)
				{
					su_rTPos = 0;
				}
			}
			else
			{
				su_rNToGo--;
			}
			su_j2 = 0;
			currentState = CState.RAND_PART_C;
			if (su_rNToGo == 1)
			{
				su_z ^= '\u0001';
			}
			SetupRandPartC();
		}
		else
		{
			currentState = CState.RAND_PART_A;
			SetupRandPartA();
		}
	}

	private void SetupRandPartC()
	{
		if (su_j2 < su_z)
		{
			currentChar = su_ch2;
			crc.UpdateCRC((byte)su_ch2);
			su_j2++;
		}
		else
		{
			currentState = CState.RAND_PART_A;
			su_i2++;
			su_count = 0;
			SetupRandPartA();
		}
	}

	private void SetupNoRandPartB()
	{
		if (su_ch2 != su_chPrev)
		{
			su_count = 1;
			SetupNoRandPartA();
		}
		else if (++su_count >= 4)
		{
			su_z = (char)(data.ll8[su_tPos] & 0xFF);
			su_tPos = data.tt[su_tPos];
			su_j2 = 0;
			SetupNoRandPartC();
		}
		else
		{
			SetupNoRandPartA();
		}
	}

	private void SetupNoRandPartC()
	{
		if (su_j2 < su_z)
		{
			int num = (currentChar = su_ch2);
			crc.UpdateCRC((byte)num);
			su_j2++;
			currentState = CState.NO_RAND_PART_C;
		}
		else
		{
			su_i2++;
			su_count = 0;
			SetupNoRandPartA();
		}
	}

	internal object DumpState()
	{
		return new StateDump
		{
			_disposed = _disposed,
			totalBytesRead = totalBytesRead,
			last = last,
			origPtr = origPtr,
			blockRandomised = blockRandomised,
			bsBuff = bsBuff,
			bsLive = bsLive,
			nInUse = nInUse,
			currentChar = currentChar,
			currentState = currentState,
			storedBlockCRC = storedBlockCRC,
			storedCombinedCRC = storedCombinedCRC,
			computedBlockCRC = computedBlockCRC,
			computedCombinedCRC = computedCombinedCRC,
			su_count = su_count,
			su_ch2 = su_ch2,
			su_chPrev = su_chPrev,
			su_i2 = su_i2,
			su_j2 = su_j2,
			su_rNToGo = su_rNToGo,
			su_rTPos = su_rTPos,
			su_tPos = su_tPos,
			su_z = su_z
		};
	}

	internal void RestoreState(object o)
	{
		StateDump stateDump = (StateDump)o;
		_disposed = stateDump._disposed;
		totalBytesRead = stateDump.totalBytesRead;
		last = stateDump.last;
		origPtr = stateDump.origPtr;
		bsBuff = stateDump.bsBuff;
		bsLive = stateDump.bsLive;
		nInUse = stateDump.nInUse;
		currentChar = stateDump.currentChar;
		currentState = stateDump.currentState;
		storedBlockCRC = stateDump.storedBlockCRC;
		storedCombinedCRC = stateDump.storedCombinedCRC;
		computedBlockCRC = stateDump.computedBlockCRC;
		computedCombinedCRC = stateDump.computedCombinedCRC;
		su_count = stateDump.su_count;
		su_ch2 = stateDump.su_ch2;
		su_chPrev = stateDump.su_chPrev;
		su_i2 = stateDump.su_i2;
		su_j2 = stateDump.su_j2;
		su_rNToGo = stateDump.su_rNToGo;
		su_rTPos = stateDump.su_rTPos;
		su_tPos = stateDump.su_tPos;
		su_z = stateDump.su_z;
	}
}
