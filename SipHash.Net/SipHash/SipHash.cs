using System;

namespace SipHash;

public sealed class SipHash
{
	private readonly ulong initialState0;

	private readonly ulong initialState1;

	public unsafe byte[] Key
	{
		get
		{
			byte[] array = new byte[16];
			fixed (byte* ptr = array)
			{
				ulong* ptr2 = (ulong*)ptr;
				*ptr2 = initialState0 ^ 0x736F6D6570736575L;
				ptr2[1] = initialState1 ^ 0x646F72616E646F6DL;
			}
			return array;
		}
	}

	public SipHash(byte[] key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (key.Length != 16)
		{
			throw new ArgumentException("SipHash key must be exactly 128-bit long (16 bytes).", "key");
		}
		initialState0 = 0x736F6D6570736575L ^ BitConverter.ToUInt64(key, 0);
		initialState1 = 0x646F72616E646F6DL ^ BitConverter.ToUInt64(key, 8);
	}

	public SipHash(byte[] key, int offset)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Array offset cannot be negative.");
		}
		if (key.Length - offset < 16)
		{
			throw new ArgumentException("The specified 'offset' parameter does not specify a valid 16-byte range in 'key'.");
		}
		initialState0 = 0x736F6D6570736575L ^ BitConverter.ToUInt64(key, offset);
		initialState1 = 0x646F72616E646F6DL ^ BitConverter.ToUInt64(key, offset + 8);
	}

	public long Compute(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return Compute(data, 0, data.Length);
	}

	public long Compute(ArraySegment<byte> data)
	{
		return Compute(data.Array, data.Offset, data.Count);
	}

	public unsafe long Compute(byte[] data, int offset, int count)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Array offset cannot be negative.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Number of array elements cannot be negative.");
		}
		if (data.Length - offset < count)
		{
			throw new ArgumentException("The specified 'offset' and 'count' parameters do not specify a valid range in 'data'.");
		}
		ulong num = initialState0;
		ulong num2 = initialState1;
		ulong num3 = 0x1F160A001E161714L ^ initialState0;
		ulong num4 = 0x100A160317100A1EL ^ initialState1;
		ulong num5;
		fixed (byte* ptr = &data[offset])
		{
			byte* ptr2 = ptr + (count & -8);
			ulong* ptr3 = (ulong*)ptr;
			while (ptr3 < ptr2)
			{
				num5 = *(ptr3++);
				num4 ^= num5;
				num += num2;
				num3 += num4;
				num2 = (num2 << 13) | (num2 >> 51);
				num4 = (num4 << 16) | (num4 >> 48);
				num2 ^= num;
				num4 ^= num3;
				num = (num << 32) | (num >> 32);
				num3 += num2;
				num += num4;
				num2 = (num2 << 17) | (num2 >> 47);
				num4 = (num4 << 21) | (num4 >> 43);
				num2 ^= num3;
				num4 ^= num;
				num3 = (num3 << 32) | (num3 >> 32);
				num += num2;
				num3 += num4;
				num2 = (num2 << 13) | (num2 >> 51);
				num4 = (num4 << 16) | (num4 >> 48);
				num2 ^= num;
				num4 ^= num3;
				num = (num << 32) | (num >> 32);
				num3 += num2;
				num += num4;
				num2 = (num2 << 17) | (num2 >> 47);
				num4 = (num4 << 21) | (num4 >> 43);
				num2 ^= num3;
				num4 ^= num;
				num3 = (num3 << 32) | (num3 >> 32);
				num ^= num5;
			}
			num5 = (ulong)((long)count << 56);
			switch (count & 7)
			{
			case 7:
				num5 |= (uint)(*(int*)ptr2) | ((ulong)((ushort*)ptr2)[2] << 32) | ((ulong)ptr2[6] << 48);
				break;
			case 6:
				num5 |= (uint)(*(int*)ptr2) | ((ulong)((ushort*)ptr2)[2] << 32);
				break;
			case 5:
				num5 |= (uint)(*(int*)ptr2) | ((ulong)ptr2[4] << 32);
				break;
			case 4:
				num5 |= (uint)(*(int*)ptr2);
				break;
			case 3:
				num5 |= *(ushort*)ptr2 | ((ulong)ptr2[2] << 16);
				break;
			case 2:
				num5 |= *(ushort*)ptr2;
				break;
			case 1:
				num5 |= *ptr2;
				break;
			}
		}
		num4 ^= num5;
		num += num2;
		num3 += num4;
		num2 = (num2 << 13) | (num2 >> 51);
		num4 = (num4 << 16) | (num4 >> 48);
		num2 ^= num;
		num4 ^= num3;
		num = (num << 32) | (num >> 32);
		num3 += num2;
		num += num4;
		num2 = (num2 << 17) | (num2 >> 47);
		num4 = (num4 << 21) | (num4 >> 43);
		num2 ^= num3;
		num4 ^= num;
		num3 = (num3 << 32) | (num3 >> 32);
		num += num2;
		num3 += num4;
		num2 = (num2 << 13) | (num2 >> 51);
		num4 = (num4 << 16) | (num4 >> 48);
		num2 ^= num;
		num4 ^= num3;
		num = (num << 32) | (num >> 32);
		num3 += num2;
		num += num4;
		num2 = (num2 << 17) | (num2 >> 47);
		num4 = (num4 << 21) | (num4 >> 43);
		num2 ^= num3;
		num4 ^= num;
		num3 = (num3 << 32) | (num3 >> 32);
		num ^= num5;
		num3 ^= 0xFF;
		num += num2;
		num3 += num4;
		num2 = (num2 << 13) | (num2 >> 51);
		num4 = (num4 << 16) | (num4 >> 48);
		num2 ^= num;
		num4 ^= num3;
		num = (num << 32) | (num >> 32);
		num3 += num2;
		num += num4;
		num2 = (num2 << 17) | (num2 >> 47);
		num4 = (num4 << 21) | (num4 >> 43);
		num2 ^= num3;
		num4 ^= num;
		num3 = (num3 << 32) | (num3 >> 32);
		num += num2;
		num3 += num4;
		num2 = (num2 << 13) | (num2 >> 51);
		num4 = (num4 << 16) | (num4 >> 48);
		num2 ^= num;
		num4 ^= num3;
		num = (num << 32) | (num >> 32);
		num3 += num2;
		num += num4;
		num2 = (num2 << 17) | (num2 >> 47);
		num4 = (num4 << 21) | (num4 >> 43);
		num2 ^= num3;
		num4 ^= num;
		num3 = (num3 << 32) | (num3 >> 32);
		num += num2;
		num3 += num4;
		num2 = (num2 << 13) | (num2 >> 51);
		num4 = (num4 << 16) | (num4 >> 48);
		num2 ^= num;
		num4 ^= num3;
		num = (num << 32) | (num >> 32);
		num3 += num2;
		num += num4;
		num2 = (num2 << 17) | (num2 >> 47);
		num4 = (num4 << 21) | (num4 >> 43);
		num2 ^= num3;
		num4 ^= num;
		num3 = (num3 << 32) | (num3 >> 32);
		num += num2;
		num3 += num4;
		num2 = (num2 << 13) | (num2 >> 51);
		num4 = (num4 << 16) | (num4 >> 48);
		num2 ^= num;
		num4 ^= num3;
		num = (num << 32) | (num >> 32);
		num3 += num2;
		num += num4;
		num2 = (num2 << 17) | (num2 >> 47);
		num4 = (num4 << 21) | (num4 >> 43);
		num2 ^= num3;
		num4 ^= num;
		num3 = (num3 << 32) | (num3 >> 32);
		return (long)(num ^ num2 ^ (num3 ^ num4));
	}
}
