using System;
using System.Diagnostics;
using System.Drawing;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public static class Util
{
	[DebuggerDisplay("Top = {Top}, Bottom = {Bottom}, Left = {Left}, Right = {Right}")]
	public struct RectanglePosition
	{
		public int Top { get; set; }

		public int Bottom { get; set; }

		public int Left { get; set; }

		public int Right { get; set; }
	}

	public unsafe static void Fill(byte* ptr, byte* ptrEnd, byte value)
	{
		while (ptr < ptrEnd)
		{
			*ptr = value;
			ptr++;
		}
	}

	public unsafe static void SwapBytes2(byte* ptr)
	{
		byte b = *ptr;
		*ptr = ptr[1];
		ptr[1] = b;
	}

	public unsafe static void SwapBytes4(byte* ptr)
	{
		byte b = *ptr;
		byte b2 = ptr[1];
		*ptr = ptr[3];
		ptr[1] = ptr[2];
		ptr[2] = b2;
		ptr[3] = b;
	}

	public unsafe static void SwapBytes(byte* ptr, int nLength)
	{
		for (long num = 0L; num < nLength / 2; num++)
		{
			byte b = ptr[num];
			ptr[num] = *(ptr + nLength - num - 1);
			*(ptr + nLength - num - 1) = b;
		}
	}

	public unsafe static void SwapByteArray2(byte[] byteArray, int startIdx, int count)
	{
		int num = startIdx + count * 2;
		if (byteArray.Length < num)
		{
			throw new IndexOutOfRangeException();
		}
		fixed (byte* ptr = &byteArray[0])
		{
			byte* ptr2 = ptr + startIdx;
			for (byte* ptr3 = ptr + num; ptr2 < ptr3; ptr2 += 2)
			{
				SwapBytes2(ptr2);
			}
		}
	}

	public unsafe static void SwapByteArray4(byte[] byteArray, int startIdx, int count)
	{
		int num = startIdx + count * 4;
		if (byteArray.Length < num)
		{
			throw new IndexOutOfRangeException();
		}
		fixed (byte* ptr = &byteArray[0])
		{
			byte* ptr2 = ptr + startIdx;
			for (byte* ptr3 = ptr + num; ptr2 < ptr3; ptr2 += 4)
			{
				SwapBytes4(ptr2);
			}
		}
	}

	public static int BytesPerRow(Rectangle rect, int depth)
	{
		if (depth == 1)
		{
			return (rect.Width + 7) / 8;
		}
		return rect.Width * BytesFromBitDepth(depth);
	}

	public static int RoundUp(int value, int multiple)
	{
		if (value == 0)
		{
			return 0;
		}
		if (Math.Sign(value) != Math.Sign(multiple))
		{
			throw new ArgumentException("value and multiple cannot have opposite signs.");
		}
		int num = value % multiple;
		if (num > 0)
		{
			value += multiple - num;
		}
		return value;
	}

	public static int GetPadding(int length, int padMultiple)
	{
		if (length < 0 || padMultiple < 0)
		{
			throw new ArgumentException();
		}
		int num = length % padMultiple;
		if (num == 0)
		{
			return 0;
		}
		return padMultiple - num;
	}

	public static int BytesFromBitDepth(int depth)
	{
		switch (depth)
		{
		case 1:
		case 8:
			return 1;
		case 16:
			return 2;
		case 32:
			return 4;
		default:
			throw new ArgumentException("Invalid bit depth.");
		}
	}

	public static short ChannelCount(this PsdColorMode colorMode)
	{
		switch (colorMode)
		{
		case PsdColorMode.Bitmap:
		case PsdColorMode.Grayscale:
		case PsdColorMode.Indexed:
		case PsdColorMode.Duotone:
			return 1;
		case PsdColorMode.RGB:
		case PsdColorMode.Multichannel:
		case PsdColorMode.Lab:
			return 3;
		case PsdColorMode.CMYK:
			return 4;
		default:
			throw new ArgumentException("Unknown color mode.");
		}
	}

	public static bool CheckBufferBounds(byte[] data, int offset, int count)
	{
		if (offset < 0)
		{
			return false;
		}
		if (count < 0)
		{
			return false;
		}
		if (offset + count > data.Length)
		{
			return false;
		}
		return true;
	}
}
