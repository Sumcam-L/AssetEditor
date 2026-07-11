using System;
using Ionic.Crc;

namespace Ionic.BZip2;

internal class BZip2Compressor
{
	private class CompressionState
	{
		public readonly bool[] inUse = new bool[256];

		public readonly byte[] unseqToSeq = new byte[256];

		public readonly int[] mtfFreq = new int[BZip2.MaxAlphaSize];

		public readonly byte[] selector = new byte[BZip2.MaxSelectors];

		public readonly byte[] selectorMtf = new byte[BZip2.MaxSelectors];

		public readonly byte[] generateMTFValues_yy = new byte[256];

		public byte[][] sendMTFValues_len;

		public int[][] sendMTFValues_rfreq;

		public readonly int[] sendMTFValues_fave = new int[BZip2.NGroups];

		public readonly short[] sendMTFValues_cost = new short[BZip2.NGroups];

		public int[][] sendMTFValues_code;

		public readonly byte[] sendMTFValues2_pos = new byte[BZip2.NGroups];

		public readonly bool[] sentMTFValues4_inUse16 = new bool[16];

		public readonly int[] stack_ll = new int[BZip2.QSORT_STACK_SIZE];

		public readonly int[] stack_hh = new int[BZip2.QSORT_STACK_SIZE];

		public readonly int[] stack_dd = new int[BZip2.QSORT_STACK_SIZE];

		public readonly int[] mainSort_runningOrder = new int[256];

		public readonly int[] mainSort_copy = new int[256];

		public readonly bool[] mainSort_bigDone = new bool[256];

		public int[] heap = new int[BZip2.MaxAlphaSize + 2];

		public int[] weight = new int[BZip2.MaxAlphaSize * 2];

		public int[] parent = new int[BZip2.MaxAlphaSize * 2];

		public readonly int[] ftab = new int[65537];

		public byte[] block;

		public int[] fmap;

		public char[] sfmap;

		public char[] quadrant;

		public CompressionState(int blockSize100k)
		{
			int num = blockSize100k * BZip2.BlockSizeMultiple;
			block = new byte[num + 1 + BZip2.NUM_OVERSHOOT_BYTES];
			fmap = new int[num];
			sfmap = new char[2 * num];
			quadrant = sfmap;
			sendMTFValues_len = BZip2.InitRectangularArray<byte>(BZip2.NGroups, BZip2.MaxAlphaSize);
			sendMTFValues_rfreq = BZip2.InitRectangularArray<int>(BZip2.NGroups, BZip2.MaxAlphaSize);
			sendMTFValues_code = BZip2.InitRectangularArray<int>(BZip2.NGroups, BZip2.MaxAlphaSize);
		}
	}

	private int blockSize100k;

	private int currentByte = -1;

	private int runLength;

	private int last;

	private int outBlockFillThreshold;

	private CompressionState cstate;

	private readonly CRC32 crc = new CRC32(reverseBits: true);

	private BitWriter bw;

	private int runs;

	private int workDone;

	private int workLimit;

	private bool firstAttempt;

	private bool blockRandomised;

	private int origPtr;

	private int nInUse;

	private int nMTF;

	private static readonly int SETMASK = 2097152;

	private static readonly int CLEARMASK = ~SETMASK;

	private static readonly byte GREATER_ICOST = 15;

	private static readonly byte LESSER_ICOST = 0;

	private static readonly int SMALL_THRESH = 20;

	private static readonly int DEPTH_THRESH = 10;

	private static readonly int WORK_FACTOR = 30;

	private static readonly int[] increments = new int[14]
	{
		1, 4, 13, 40, 121, 364, 1093, 3280, 9841, 29524,
		88573, 265720, 797161, 2391484
	};

	public int BlockSize => blockSize100k;

	public uint Crc32 { get; private set; }

	public int AvailableBytesOut { get; private set; }

	public int UncompressedBytes => last + 1;

	public BZip2Compressor(BitWriter writer)
		: this(writer, BZip2.MaxBlockSize)
	{
	}

	public BZip2Compressor(BitWriter writer, int blockSize)
	{
		blockSize100k = blockSize;
		bw = writer;
		outBlockFillThreshold = blockSize * BZip2.BlockSizeMultiple - 20;
		cstate = new CompressionState(blockSize);
		Reset();
	}

	private void Reset()
	{
		crc.Reset();
		currentByte = -1;
		runLength = 0;
		last = -1;
		int num = 256;
		while (--num >= 0)
		{
			cstate.inUse[num] = false;
		}
	}

	public int Fill(byte[] buffer, int offset, int count)
	{
		if (last >= outBlockFillThreshold)
		{
			return 0;
		}
		int num = 0;
		int num2 = offset + count;
		int num3;
		do
		{
			num3 = write0(buffer[offset++]);
			if (num3 > 0)
			{
				num++;
			}
		}
		while (offset < num2 && num3 == 1);
		return num;
	}

	private int write0(byte b)
	{
		if (currentByte == -1)
		{
			currentByte = b;
			runLength++;
			return 1;
		}
		if (currentByte == b)
		{
			if (++runLength > 254)
			{
				bool flag = AddRunToOutputBlock(final: false);
				currentByte = -1;
				runLength = 0;
				if (!flag)
				{
					return 1;
				}
				return 2;
			}
			return 1;
		}
		if (AddRunToOutputBlock(final: false))
		{
			currentByte = -1;
			runLength = 0;
			return 0;
		}
		runLength = 1;
		currentByte = b;
		return 1;
	}

	private bool AddRunToOutputBlock(bool final)
	{
		runs++;
		int num = last;
		if (num >= outBlockFillThreshold && !final)
		{
			string message = string.Format("block overrun(final={2}): {0} >= threshold ({1})", num, outBlockFillThreshold, final);
			throw new Exception(message);
		}
		byte b = (byte)currentByte;
		byte[] block = cstate.block;
		cstate.inUse[b] = true;
		int num2 = runLength;
		crc.UpdateCRC(b, num2);
		switch (num2)
		{
		case 1:
			block[num + 2] = b;
			last = num + 1;
			break;
		case 2:
			block[num + 2] = b;
			block[num + 3] = b;
			last = num + 2;
			break;
		case 3:
			block[num + 2] = b;
			block[num + 3] = b;
			block[num + 4] = b;
			last = num + 3;
			break;
		default:
			num2 -= 4;
			cstate.inUse[num2] = true;
			block[num + 2] = b;
			block[num + 3] = b;
			block[num + 4] = b;
			block[num + 5] = b;
			block[num + 6] = (byte)num2;
			last = num + 5;
			break;
		}
		return last >= outBlockFillThreshold;
	}

	public void CompressAndWrite()
	{
		if (runLength > 0)
		{
			AddRunToOutputBlock(final: true);
		}
		currentByte = -1;
		if (last != -1)
		{
			blockSort();
			bw.WriteByte(49);
			bw.WriteByte(65);
			bw.WriteByte(89);
			bw.WriteByte(38);
			bw.WriteByte(83);
			bw.WriteByte(89);
			Crc32 = (uint)crc.Crc32Result;
			bw.WriteInt(Crc32);
			bw.WriteBits(1, blockRandomised ? 1u : 0u);
			moveToFrontCodeAndSend();
			Reset();
		}
	}

	private void randomiseBlock()
	{
		bool[] inUse = cstate.inUse;
		byte[] block = cstate.block;
		int num = last;
		int num2 = 256;
		while (--num2 >= 0)
		{
			inUse[num2] = false;
		}
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 1;
		while (num5 <= num)
		{
			if (num3 == 0)
			{
				num3 = (ushort)Rand.Rnums(num4);
				if (++num4 == 512)
				{
					num4 = 0;
				}
			}
			num3--;
			block[num6] ^= ((num3 == 1) ? ((byte)1) : ((byte)0));
			inUse[block[num6] & 0xFF] = true;
			num5 = num6;
			num6++;
		}
		blockRandomised = true;
	}

	private void mainSort()
	{
		CompressionState compressionState = cstate;
		int[] mainSort_runningOrder = compressionState.mainSort_runningOrder;
		int[] mainSort_copy = compressionState.mainSort_copy;
		bool[] mainSort_bigDone = compressionState.mainSort_bigDone;
		int[] ftab = compressionState.ftab;
		byte[] block = compressionState.block;
		int[] fmap = compressionState.fmap;
		char[] quadrant = compressionState.quadrant;
		int num = last;
		int num2 = workLimit;
		bool flag = firstAttempt;
		int num3 = 65537;
		while (--num3 >= 0)
		{
			ftab[num3] = 0;
		}
		for (int i = 0; i < BZip2.NUM_OVERSHOOT_BYTES; i++)
		{
			block[num + i + 2] = block[i % (num + 1) + 1];
		}
		int num4 = num + BZip2.NUM_OVERSHOOT_BYTES + 1;
		while (--num4 >= 0)
		{
			quadrant[num4] = '\0';
		}
		block[0] = block[num + 1];
		int num5 = block[0] & 0xFF;
		for (int j = 0; j <= num; j++)
		{
			int num6 = block[j + 1] & 0xFF;
			ftab[(num5 << 8) + num6]++;
			num5 = num6;
		}
		for (int k = 1; k <= 65536; k++)
		{
			ftab[k] += ftab[k - 1];
		}
		num5 = block[1] & 0xFF;
		for (int l = 0; l < num; l++)
		{
			int num7 = block[l + 2] & 0xFF;
			fmap[--ftab[(num5 << 8) + num7]] = l;
			num5 = num7;
		}
		fmap[--ftab[((block[num + 1] & 0xFF) << 8) + (block[1] & 0xFF)]] = num;
		int num8 = 256;
		while (--num8 >= 0)
		{
			mainSort_bigDone[num8] = false;
			mainSort_runningOrder[num8] = num8;
		}
		int num9 = 364;
		while (num9 != 1)
		{
			num9 /= 3;
			for (int m = num9; m <= 255; m++)
			{
				int num10 = mainSort_runningOrder[m];
				int num11 = ftab[num10 + 1 << 8] - ftab[num10 << 8];
				int num12 = num9 - 1;
				int num13 = m;
				int num14 = mainSort_runningOrder[num13 - num9];
				while (ftab[num14 + 1 << 8] - ftab[num14 << 8] > num11)
				{
					mainSort_runningOrder[num13] = num14;
					num13 -= num9;
					if (num13 <= num12)
					{
						break;
					}
					num14 = mainSort_runningOrder[num13 - num9];
				}
				mainSort_runningOrder[num13] = num10;
			}
		}
		for (int n = 0; n <= 255; n++)
		{
			int num15 = mainSort_runningOrder[n];
			for (int num16 = 0; num16 <= 255; num16++)
			{
				int num17 = (num15 << 8) + num16;
				int num18 = ftab[num17];
				if ((num18 & SETMASK) == SETMASK)
				{
					continue;
				}
				int num19 = num18 & CLEARMASK;
				int num20 = (ftab[num17 + 1] & CLEARMASK) - 1;
				if (num20 > num19)
				{
					mainQSort3(compressionState, num19, num20, 2);
					if (flag && workDone > num2)
					{
						return;
					}
				}
				ftab[num17] = num18 | SETMASK;
			}
			for (int num21 = 0; num21 <= 255; num21++)
			{
				mainSort_copy[num21] = ftab[(num21 << 8) + num15] & CLEARMASK;
			}
			int num22 = ftab[num15 << 8] & CLEARMASK;
			for (int num23 = ftab[num15 + 1 << 8] & CLEARMASK; num22 < num23; num22++)
			{
				int num24 = fmap[num22];
				num5 = block[num24] & 0xFF;
				if (!mainSort_bigDone[num5])
				{
					fmap[mainSort_copy[num5]] = ((num24 == 0) ? num : (num24 - 1));
					mainSort_copy[num5]++;
				}
			}
			int num25 = 256;
			while (--num25 >= 0)
			{
				ftab[(num25 << 8) + num15] |= SETMASK;
			}
			mainSort_bigDone[num15] = true;
			if (n >= 255)
			{
				continue;
			}
			int num26 = ftab[num15 << 8] & CLEARMASK;
			int num27 = (ftab[num15 + 1 << 8] & CLEARMASK) - num26;
			int num28;
			for (num28 = 0; num27 >> num28 > 65534; num28++)
			{
			}
			for (int num29 = 0; num29 < num27; num29++)
			{
				int num30 = fmap[num26 + num29];
				char c = (quadrant[num30] = (char)(num29 >> num28));
				if (num30 < BZip2.NUM_OVERSHOOT_BYTES)
				{
					quadrant[num30 + num + 1] = c;
				}
			}
		}
	}

	private void blockSort()
	{
		workLimit = WORK_FACTOR * last;
		workDone = 0;
		blockRandomised = false;
		firstAttempt = true;
		mainSort();
		if (firstAttempt && workDone > workLimit)
		{
			randomiseBlock();
			workLimit = (workDone = 0);
			firstAttempt = false;
			mainSort();
		}
		int[] fmap = cstate.fmap;
		origPtr = -1;
		int i = 0;
		for (int num = last; i <= num; i++)
		{
			if (fmap[i] == 0)
			{
				origPtr = i;
				break;
			}
		}
	}

	private bool mainSimpleSort(CompressionState dataShadow, int lo, int hi, int d)
	{
		int num = hi - lo + 1;
		if (num < 2)
		{
			if (firstAttempt)
			{
				return workDone > workLimit;
			}
			return false;
		}
		int i;
		for (i = 0; increments[i] < num; i++)
		{
		}
		int[] fmap = dataShadow.fmap;
		char[] quadrant = dataShadow.quadrant;
		byte[] block = dataShadow.block;
		int num2 = last;
		int num3 = num2 + 1;
		bool flag = firstAttempt;
		int num4 = workLimit;
		int num5 = workDone;
		while (--i >= 0)
		{
			int num6 = increments[i];
			int num7 = lo + num6 - 1;
			int j = lo + num6;
			while (j <= hi)
			{
				int num8 = 3;
				for (; j <= hi; j++)
				{
					if (--num8 < 0)
					{
						break;
					}
					int num9 = fmap[j];
					int num10 = num9 + d;
					int num11 = j;
					bool flag2 = false;
					int num12 = 0;
					int num13;
					int num14;
					do
					{
						IL_00b1:
						if (flag2)
						{
							fmap[num11] = num12;
							if ((num11 -= num6) <= num7)
							{
								break;
							}
						}
						else
						{
							flag2 = true;
						}
						num12 = fmap[num11 - num6];
						num13 = num12 + d;
						num14 = num10;
						if (block[num13 + 1] == block[num14 + 1])
						{
							if (block[num13 + 2] == block[num14 + 2])
							{
								if (block[num13 + 3] == block[num14 + 3])
								{
									if (block[num13 + 4] == block[num14 + 4])
									{
										if (block[num13 + 5] == block[num14 + 5])
										{
											if (block[num13 += 6] == block[num14 += 6])
											{
												int num15 = num2;
												while (num15 > 0)
												{
													num15 -= 4;
													if (block[num13 + 1] == block[num14 + 1])
													{
														if (quadrant[num13] == quadrant[num14])
														{
															if (block[num13 + 2] == block[num14 + 2])
															{
																if (quadrant[num13 + 1] == quadrant[num14 + 1])
																{
																	if (block[num13 + 3] == block[num14 + 3])
																	{
																		if (quadrant[num13 + 2] == quadrant[num14 + 2])
																		{
																			if (block[num13 + 4] == block[num14 + 4])
																			{
																				if (quadrant[num13 + 3] == quadrant[num14 + 3])
																				{
																					if ((num13 += 4) >= num3)
																					{
																						num13 -= num3;
																					}
																					if ((num14 += 4) >= num3)
																					{
																						num14 -= num3;
																					}
																					num5++;
																					continue;
																				}
																				goto IL_021f;
																			}
																			goto IL_0235;
																		}
																		goto IL_0259;
																	}
																	goto IL_026f;
																}
																goto IL_0293;
															}
															goto IL_02a9;
														}
														goto IL_02cd;
													}
													goto IL_02df;
												}
												break;
											}
											if ((block[num13] & 0xFF) <= (block[num14] & 0xFF))
											{
												break;
											}
										}
										else if ((block[num13 + 5] & 0xFF) <= (block[num14 + 5] & 0xFF))
										{
											break;
										}
									}
									else if ((block[num13 + 4] & 0xFF) <= (block[num14 + 4] & 0xFF))
									{
										break;
									}
								}
								else if ((block[num13 + 3] & 0xFF) <= (block[num14 + 3] & 0xFF))
								{
									break;
								}
							}
							else if ((block[num13 + 2] & 0xFF) <= (block[num14 + 2] & 0xFF))
							{
								break;
							}
						}
						else if ((block[num13 + 1] & 0xFF) <= (block[num14 + 1] & 0xFF))
						{
							break;
						}
						goto IL_00b1;
						IL_0259:
						if (quadrant[num13 + 2] <= quadrant[num14 + 2])
						{
							break;
						}
						goto IL_00b1;
						IL_021f:
						if (quadrant[num13 + 3] <= quadrant[num14 + 3])
						{
							break;
						}
						goto IL_00b1;
						IL_02cd:
						if (quadrant[num13] <= quadrant[num14])
						{
							break;
						}
						goto IL_00b1;
						IL_0235:
						if ((block[num13 + 4] & 0xFF) <= (block[num14 + 4] & 0xFF))
						{
							break;
						}
						goto IL_00b1;
						IL_026f:
						if ((block[num13 + 3] & 0xFF) <= (block[num14 + 3] & 0xFF))
						{
							break;
						}
						goto IL_00b1;
						IL_02a9:
						if ((block[num13 + 2] & 0xFF) <= (block[num14 + 2] & 0xFF))
						{
							break;
						}
						goto IL_00b1;
						IL_02df:
						if ((block[num13 + 1] & 0xFF) <= (block[num14 + 1] & 0xFF))
						{
							break;
						}
						goto IL_00b1;
						IL_0293:;
					}
					while (quadrant[num13 + 1] > quadrant[num14 + 1]);
					fmap[num11] = num9;
				}
				if (flag && j <= hi && num5 > num4)
				{
					goto end_IL_040b;
				}
			}
			continue;
			end_IL_040b:
			break;
		}
		workDone = num5;
		if (flag)
		{
			return num5 > num4;
		}
		return false;
	}

	private static void vswap(int[] fmap, int p1, int p2, int n)
	{
		n += p1;
		while (p1 < n)
		{
			int num = fmap[p1];
			fmap[p1++] = fmap[p2];
			fmap[p2++] = num;
		}
	}

	private static byte med3(byte a, byte b, byte c)
	{
		if (a >= b)
		{
			if (b <= c)
			{
				if (a <= c)
				{
					return a;
				}
				return c;
			}
			return b;
		}
		if (b >= c)
		{
			if (a >= c)
			{
				return a;
			}
			return c;
		}
		return b;
	}

	private void mainQSort3(CompressionState dataShadow, int loSt, int hiSt, int dSt)
	{
		int[] stack_ll = dataShadow.stack_ll;
		int[] stack_hh = dataShadow.stack_hh;
		int[] stack_dd = dataShadow.stack_dd;
		int[] fmap = dataShadow.fmap;
		byte[] block = dataShadow.block;
		stack_ll[0] = loSt;
		stack_hh[0] = hiSt;
		stack_dd[0] = dSt;
		int num = 1;
		while (--num >= 0)
		{
			int num2 = stack_ll[num];
			int num3 = stack_hh[num];
			int num4 = stack_dd[num];
			if (num3 - num2 < SMALL_THRESH || num4 > DEPTH_THRESH)
			{
				if (mainSimpleSort(dataShadow, num2, num3, num4))
				{
					break;
				}
				continue;
			}
			int num5 = num4 + 1;
			int num6 = med3(block[fmap[num2] + num5], block[fmap[num3] + num5], block[fmap[num2 + num3 >> 1] + num5]) & 0xFF;
			int num7 = num2;
			int num8 = num3;
			int num9 = num2;
			int num10 = num3;
			while (true)
			{
				if (num7 <= num8)
				{
					int num11 = (block[fmap[num7] + num5] & 0xFF) - num6;
					if (num11 == 0)
					{
						int num12 = fmap[num7];
						fmap[num7++] = fmap[num9];
						fmap[num9++] = num12;
						continue;
					}
					if (num11 < 0)
					{
						num7++;
						continue;
					}
				}
				while (num7 <= num8)
				{
					int num13 = (block[fmap[num8] + num5] & 0xFF) - num6;
					if (num13 == 0)
					{
						int num14 = fmap[num8];
						fmap[num8--] = fmap[num10];
						fmap[num10--] = num14;
						continue;
					}
					if (num13 <= 0)
					{
						break;
					}
					num8--;
				}
				if (num7 > num8)
				{
					break;
				}
				int num15 = fmap[num7];
				fmap[num7++] = fmap[num8];
				fmap[num8--] = num15;
			}
			if (num10 < num9)
			{
				stack_ll[num] = num2;
				stack_hh[num] = num3;
				stack_dd[num] = num5;
				num++;
				continue;
			}
			int num16 = ((num9 - num2 < num7 - num9) ? (num9 - num2) : (num7 - num9));
			vswap(fmap, num2, num7 - num16, num16);
			int num17 = ((num3 - num10 < num10 - num8) ? (num3 - num10) : (num10 - num8));
			vswap(fmap, num7, num3 - num17 + 1, num17);
			num16 = num2 + num7 - num9 - 1;
			num17 = num3 - (num10 - num8) + 1;
			stack_ll[num] = num2;
			stack_hh[num] = num16;
			stack_dd[num] = num4;
			num++;
			stack_ll[num] = num16 + 1;
			stack_hh[num] = num17 - 1;
			stack_dd[num] = num5;
			num++;
			stack_ll[num] = num17;
			stack_hh[num] = num3;
			stack_dd[num] = num4;
			num++;
		}
	}

	private void generateMTFValues()
	{
		int num = last;
		CompressionState compressionState = cstate;
		bool[] inUse = compressionState.inUse;
		byte[] block = compressionState.block;
		int[] fmap = compressionState.fmap;
		char[] sfmap = compressionState.sfmap;
		int[] mtfFreq = compressionState.mtfFreq;
		byte[] unseqToSeq = compressionState.unseqToSeq;
		byte[] generateMTFValues_yy = compressionState.generateMTFValues_yy;
		int num2 = 0;
		for (int i = 0; i < 256; i++)
		{
			if (inUse[i])
			{
				unseqToSeq[i] = (byte)num2;
				num2++;
			}
		}
		nInUse = num2;
		int num3 = num2 + 1;
		for (int num4 = num3; num4 >= 0; num4--)
		{
			mtfFreq[num4] = 0;
		}
		int num5 = num2;
		while (--num5 >= 0)
		{
			generateMTFValues_yy[num5] = (byte)num5;
		}
		int num6 = 0;
		int num7 = 0;
		for (int j = 0; j <= num; j++)
		{
			byte b = unseqToSeq[block[fmap[j]] & 0xFF];
			byte b2 = generateMTFValues_yy[0];
			int num8 = 0;
			while (b != b2)
			{
				num8++;
				byte b3 = b2;
				b2 = generateMTFValues_yy[num8];
				generateMTFValues_yy[num8] = b3;
			}
			generateMTFValues_yy[0] = b2;
			if (num8 == 0)
			{
				num7++;
				continue;
			}
			if (num7 > 0)
			{
				num7--;
				while (true)
				{
					if ((num7 & 1) == 0)
					{
						sfmap[num6] = BZip2.RUNA;
						num6++;
						mtfFreq[(uint)BZip2.RUNA]++;
					}
					else
					{
						sfmap[num6] = BZip2.RUNB;
						num6++;
						mtfFreq[(uint)BZip2.RUNB]++;
					}
					if (num7 < 2)
					{
						break;
					}
					num7 = num7 - 2 >> 1;
				}
				num7 = 0;
			}
			sfmap[num6] = (char)(num8 + 1);
			num6++;
			mtfFreq[num8 + 1]++;
		}
		if (num7 > 0)
		{
			num7--;
			while (true)
			{
				if ((num7 & 1) == 0)
				{
					sfmap[num6] = BZip2.RUNA;
					num6++;
					mtfFreq[(uint)BZip2.RUNA]++;
				}
				else
				{
					sfmap[num6] = BZip2.RUNB;
					num6++;
					mtfFreq[(uint)BZip2.RUNB]++;
				}
				if (num7 < 2)
				{
					break;
				}
				num7 = num7 - 2 >> 1;
			}
		}
		sfmap[num6] = (char)num3;
		mtfFreq[num3]++;
		nMTF = num6 + 1;
	}

	private static void hbAssignCodes(int[] code, byte[] length, int minLen, int maxLen, int alphaSize)
	{
		int num = 0;
		for (int i = minLen; i <= maxLen; i++)
		{
			for (int j = 0; j < alphaSize; j++)
			{
				if ((length[j] & 0xFF) == i)
				{
					code[j] = num;
					num++;
				}
			}
			num <<= 1;
		}
	}

	private void sendMTFValues()
	{
		byte[][] sendMTFValues_len = cstate.sendMTFValues_len;
		int num = nInUse + 2;
		int num2 = BZip2.NGroups;
		while (--num2 >= 0)
		{
			byte[] array = sendMTFValues_len[num2];
			int num3 = num;
			while (--num3 >= 0)
			{
				array[num3] = GREATER_ICOST;
			}
		}
		int nGroups = ((nMTF < 200) ? 2 : ((nMTF < 600) ? 3 : ((nMTF < 1200) ? 4 : ((nMTF < 2400) ? 5 : 6))));
		sendMTFValues0(nGroups, num);
		int nSelectors = sendMTFValues1(nGroups, num);
		sendMTFValues2(nGroups, nSelectors);
		sendMTFValues3(nGroups, num);
		sendMTFValues4();
		sendMTFValues5(nGroups, nSelectors);
		sendMTFValues6(nGroups, num);
		sendMTFValues7(nSelectors);
	}

	private void sendMTFValues0(int nGroups, int alphaSize)
	{
		byte[][] sendMTFValues_len = cstate.sendMTFValues_len;
		int[] mtfFreq = cstate.mtfFreq;
		int num = nMTF;
		int num2 = 0;
		for (int num3 = nGroups; num3 > 0; num3--)
		{
			int num4 = num / num3;
			int num5 = num2 - 1;
			int i = 0;
			int num6 = alphaSize - 1;
			for (; i < num4; i += mtfFreq[++num5])
			{
				if (num5 >= num6)
				{
					break;
				}
			}
			if (num5 > num2 && num3 != nGroups && num3 != 1 && ((nGroups - num3) & 1) != 0)
			{
				i -= mtfFreq[num5--];
			}
			byte[] array = sendMTFValues_len[num3 - 1];
			int num7 = alphaSize;
			while (--num7 >= 0)
			{
				if (num7 >= num2 && num7 <= num5)
				{
					array[num7] = LESSER_ICOST;
				}
				else
				{
					array[num7] = GREATER_ICOST;
				}
			}
			num2 = num5 + 1;
			num -= i;
		}
	}

	private static void hbMakeCodeLengths(byte[] len, int[] freq, CompressionState state1, int alphaSize, int maxLen)
	{
		int[] heap = state1.heap;
		int[] weight = state1.weight;
		int[] parent = state1.parent;
		int num = alphaSize;
		while (--num >= 0)
		{
			weight[num + 1] = ((freq[num] == 0) ? 1 : freq[num]) << 8;
		}
		bool flag = true;
		while (flag)
		{
			flag = false;
			int num2 = alphaSize;
			int num3 = 0;
			heap[0] = 0;
			weight[0] = 0;
			parent[0] = -2;
			for (int i = 1; i <= alphaSize; i++)
			{
				parent[i] = -1;
				num3++;
				heap[num3] = i;
				int num4 = num3;
				int num5 = heap[num4];
				while (weight[num5] < weight[heap[num4 >> 1]])
				{
					heap[num4] = heap[num4 >> 1];
					num4 >>= 1;
				}
				heap[num4] = num5;
			}
			while (num3 > 1)
			{
				int num6 = heap[1];
				heap[1] = heap[num3];
				num3--;
				int num7 = 0;
				int num8 = 1;
				int num9 = heap[1];
				while (true)
				{
					num7 = num8 << 1;
					if (num7 > num3)
					{
						break;
					}
					if (num7 < num3 && weight[heap[num7 + 1]] < weight[heap[num7]])
					{
						num7++;
					}
					if (weight[num9] < weight[heap[num7]])
					{
						break;
					}
					heap[num8] = heap[num7];
					num8 = num7;
				}
				heap[num8] = num9;
				int num10 = heap[1];
				heap[1] = heap[num3];
				num3--;
				num7 = 0;
				num8 = 1;
				num9 = heap[1];
				while (true)
				{
					num7 = num8 << 1;
					if (num7 > num3)
					{
						break;
					}
					if (num7 < num3 && weight[heap[num7 + 1]] < weight[heap[num7]])
					{
						num7++;
					}
					if (weight[num9] < weight[heap[num7]])
					{
						break;
					}
					heap[num8] = heap[num7];
					num8 = num7;
				}
				heap[num8] = num9;
				num2++;
				parent[num6] = (parent[num10] = num2);
				int num11 = weight[num6];
				int num12 = weight[num10];
				weight[num2] = ((num11 & -256) + (num12 & -256)) | (1 + (((num11 & 0xFF) > (num12 & 0xFF)) ? (num11 & 0xFF) : (num12 & 0xFF)));
				parent[num2] = -1;
				num3++;
				heap[num3] = num2;
				num9 = 0;
				num8 = num3;
				num9 = heap[num8];
				int num13 = weight[num9];
				while (num13 < weight[heap[num8 >> 1]])
				{
					heap[num8] = heap[num8 >> 1];
					num8 >>= 1;
				}
				heap[num8] = num9;
			}
			for (int j = 1; j <= alphaSize; j++)
			{
				int num14 = 0;
				int num15 = j;
				int num16;
				while ((num16 = parent[num15]) >= 0)
				{
					num15 = num16;
					num14++;
				}
				len[j - 1] = (byte)num14;
				if (num14 > maxLen)
				{
					flag = true;
				}
			}
			if (flag)
			{
				for (int k = 1; k < alphaSize; k++)
				{
					int num17 = weight[k] >> 8;
					num17 = 1 + (num17 >> 1);
					weight[k] = num17 << 8;
				}
			}
		}
	}

	private int sendMTFValues1(int nGroups, int alphaSize)
	{
		CompressionState compressionState = cstate;
		int[][] sendMTFValues_rfreq = compressionState.sendMTFValues_rfreq;
		int[] sendMTFValues_fave = compressionState.sendMTFValues_fave;
		short[] sendMTFValues_cost = compressionState.sendMTFValues_cost;
		char[] sfmap = compressionState.sfmap;
		byte[] selector = compressionState.selector;
		byte[][] sendMTFValues_len = compressionState.sendMTFValues_len;
		byte[] array = sendMTFValues_len[0];
		byte[] array2 = sendMTFValues_len[1];
		byte[] array3 = sendMTFValues_len[2];
		byte[] array4 = sendMTFValues_len[3];
		byte[] array5 = sendMTFValues_len[4];
		byte[] array6 = sendMTFValues_len[5];
		int num = nMTF;
		int num2 = 0;
		for (int i = 0; i < BZip2.N_ITERS; i++)
		{
			int num3 = nGroups;
			while (--num3 >= 0)
			{
				sendMTFValues_fave[num3] = 0;
				int[] array7 = sendMTFValues_rfreq[num3];
				int num4 = alphaSize;
				while (--num4 >= 0)
				{
					array7[num4] = 0;
				}
			}
			num2 = 0;
			int num5 = 0;
			while (num5 < nMTF)
			{
				int num6 = Math.Min(num5 + BZip2.G_SIZE - 1, num - 1);
				if (nGroups == BZip2.NGroups)
				{
					int[] array8 = new int[6];
					for (int j = num5; j <= num6; j++)
					{
						int num7 = sfmap[j];
						array8[0] += array[num7] & 0xFF;
						array8[1] += array2[num7] & 0xFF;
						array8[2] += array3[num7] & 0xFF;
						array8[3] += array4[num7] & 0xFF;
						array8[4] += array5[num7] & 0xFF;
						array8[5] += array6[num7] & 0xFF;
					}
					sendMTFValues_cost[0] = (short)array8[0];
					sendMTFValues_cost[1] = (short)array8[1];
					sendMTFValues_cost[2] = (short)array8[2];
					sendMTFValues_cost[3] = (short)array8[3];
					sendMTFValues_cost[4] = (short)array8[4];
					sendMTFValues_cost[5] = (short)array8[5];
				}
				else
				{
					int num8 = nGroups;
					while (--num8 >= 0)
					{
						sendMTFValues_cost[num8] = 0;
					}
					for (int k = num5; k <= num6; k++)
					{
						int num9 = sfmap[k];
						int num10 = nGroups;
						while (--num10 >= 0)
						{
							sendMTFValues_cost[num10] += (short)(sendMTFValues_len[num10][num9] & 0xFF);
						}
					}
				}
				int num11 = -1;
				int num12 = nGroups;
				int num13 = 999999999;
				while (--num12 >= 0)
				{
					int num14 = sendMTFValues_cost[num12];
					if (num14 < num13)
					{
						num13 = num14;
						num11 = num12;
					}
				}
				sendMTFValues_fave[num11]++;
				selector[num2] = (byte)num11;
				num2++;
				int[] array9 = sendMTFValues_rfreq[num11];
				for (int l = num5; l <= num6; l++)
				{
					array9[(uint)sfmap[l]]++;
				}
				num5 = num6 + 1;
			}
			for (int m = 0; m < nGroups; m++)
			{
				hbMakeCodeLengths(sendMTFValues_len[m], sendMTFValues_rfreq[m], cstate, alphaSize, 20);
			}
		}
		return num2;
	}

	private void sendMTFValues2(int nGroups, int nSelectors)
	{
		CompressionState compressionState = cstate;
		byte[] sendMTFValues2_pos = compressionState.sendMTFValues2_pos;
		int num = nGroups;
		while (--num >= 0)
		{
			sendMTFValues2_pos[num] = (byte)num;
		}
		for (int i = 0; i < nSelectors; i++)
		{
			byte b = compressionState.selector[i];
			byte b2 = sendMTFValues2_pos[0];
			int num2 = 0;
			while (b != b2)
			{
				num2++;
				byte b3 = b2;
				b2 = sendMTFValues2_pos[num2];
				sendMTFValues2_pos[num2] = b3;
			}
			sendMTFValues2_pos[0] = b2;
			compressionState.selectorMtf[i] = (byte)num2;
		}
	}

	private void sendMTFValues3(int nGroups, int alphaSize)
	{
		int[][] sendMTFValues_code = cstate.sendMTFValues_code;
		byte[][] sendMTFValues_len = cstate.sendMTFValues_len;
		for (int i = 0; i < nGroups; i++)
		{
			int num = 32;
			int num2 = 0;
			byte[] array = sendMTFValues_len[i];
			int num3 = alphaSize;
			while (--num3 >= 0)
			{
				int num4 = array[num3] & 0xFF;
				if (num4 > num2)
				{
					num2 = num4;
				}
				if (num4 < num)
				{
					num = num4;
				}
			}
			hbAssignCodes(sendMTFValues_code[i], sendMTFValues_len[i], num, num2, alphaSize);
		}
	}

	private void sendMTFValues4()
	{
		bool[] inUse = cstate.inUse;
		bool[] sentMTFValues4_inUse = cstate.sentMTFValues4_inUse16;
		int num = 16;
		while (--num >= 0)
		{
			sentMTFValues4_inUse[num] = false;
			int num2 = num * 16;
			int num3 = 16;
			while (--num3 >= 0)
			{
				if (inUse[num2 + num3])
				{
					sentMTFValues4_inUse[num] = true;
				}
			}
		}
		uint num4 = 0u;
		for (int i = 0; i < 16; i++)
		{
			if (sentMTFValues4_inUse[i])
			{
				num4 |= (uint)(1 << 16 - i - 1);
			}
		}
		bw.WriteBits(16, num4);
		for (int j = 0; j < 16; j++)
		{
			if (!sentMTFValues4_inUse[j])
			{
				continue;
			}
			int num5 = j * 16;
			num4 = 0u;
			for (int k = 0; k < 16; k++)
			{
				if (inUse[num5 + k])
				{
					num4 |= (uint)(1 << 16 - k - 1);
				}
			}
			bw.WriteBits(16, num4);
		}
	}

	private void sendMTFValues5(int nGroups, int nSelectors)
	{
		bw.WriteBits(3, (uint)nGroups);
		bw.WriteBits(15, (uint)nSelectors);
		byte[] selectorMtf = cstate.selectorMtf;
		for (int i = 0; i < nSelectors; i++)
		{
			int j = 0;
			for (int num = selectorMtf[i] & 0xFF; j < num; j++)
			{
				bw.WriteBits(1, 1u);
			}
			bw.WriteBits(1, 0u);
		}
	}

	private void sendMTFValues6(int nGroups, int alphaSize)
	{
		byte[][] sendMTFValues_len = cstate.sendMTFValues_len;
		for (int i = 0; i < nGroups; i++)
		{
			byte[] array = sendMTFValues_len[i];
			uint num = (uint)(array[0] & 0xFF);
			bw.WriteBits(5, num);
			for (int j = 0; j < alphaSize; j++)
			{
				int num2;
				for (num2 = array[j] & 0xFF; num < num2; num++)
				{
					bw.WriteBits(2, 2u);
				}
				while (num > num2)
				{
					bw.WriteBits(2, 3u);
					num--;
				}
				bw.WriteBits(1, 0u);
			}
		}
	}

	private void sendMTFValues7(int nSelectors)
	{
		byte[][] sendMTFValues_len = cstate.sendMTFValues_len;
		int[][] sendMTFValues_code = cstate.sendMTFValues_code;
		byte[] selector = cstate.selector;
		char[] sfmap = cstate.sfmap;
		int num = nMTF;
		int num2 = 0;
		int i = 0;
		while (i < num)
		{
			int num3 = Math.Min(i + BZip2.G_SIZE - 1, num - 1);
			int num4 = selector[num2] & 0xFF;
			int[] array = sendMTFValues_code[num4];
			byte[] array2 = sendMTFValues_len[num4];
			for (; i <= num3; i++)
			{
				int num5 = sfmap[i];
				int nbits = array2[num5] & 0xFF;
				bw.WriteBits(nbits, (uint)array[num5]);
			}
			i = num3 + 1;
			num2++;
		}
	}

	private void moveToFrontCodeAndSend()
	{
		bw.WriteBits(24, (uint)origPtr);
		generateMTFValues();
		sendMTFValues();
	}
}
