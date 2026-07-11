using System;
using System.Collections.Generic;

namespace Bespoke.Common.Osc;

public class OscTimeTag
{
	public static readonly DateTime Epoch = new DateTime(1900, 1, 1, 0, 0, 0, 0);

	public static readonly OscTimeTag MinValue = new OscTimeTag(Epoch + TimeSpan.FromMilliseconds(1.0));

	private DateTime mTimeStamp;

	public uint SecondsSinceEpoch => (uint)(mTimeStamp - Epoch).TotalSeconds;

	public uint FractionalSecond => (uint)(mTimeStamp - Epoch).Milliseconds;

	public DateTime DateTime => mTimeStamp;

	public OscTimeTag()
		: this(DateTime.Now)
	{
	}

	public OscTimeTag(DateTime timeStamp)
	{
		Set(timeStamp);
	}

	public OscTimeTag(byte[] data)
	{
		byte[] array = data.CopySubArray(0, 4);
		byte[] array2 = data.CopySubArray(4, 4);
		if (BitConverter.IsLittleEndian != OscPacket.LittleEndianByteOrder)
		{
			array = Utility.SwapEndian(array);
			array2 = Utility.SwapEndian(array2);
		}
		uint num = BitConverter.ToUInt32(array, 0);
		uint num2 = BitConverter.ToUInt32(array2, 0);
		DateTime timeStamp = Epoch.AddSeconds(num).AddMilliseconds(num2);
		Assert.IsTrue(IsValidTime(timeStamp));
		mTimeStamp = timeStamp;
	}

	public static bool Equals(OscTimeTag lhs, OscTimeTag rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator ==(OscTimeTag lhs, OscTimeTag rhs)
	{
		if (object.ReferenceEquals(lhs, rhs))
		{
			return true;
		}
		if ((object)lhs == null || (object)rhs == null)
		{
			return false;
		}
		return lhs.DateTime == rhs.DateTime;
	}

	public static bool operator !=(OscTimeTag lhs, OscTimeTag rhs)
	{
		return !(lhs == rhs);
	}

	public static bool operator <(OscTimeTag lhs, OscTimeTag rhs)
	{
		return lhs.DateTime < rhs.DateTime;
	}

	public static bool operator <=(OscTimeTag lhs, OscTimeTag rhs)
	{
		return lhs.DateTime <= rhs.DateTime;
	}

	public static bool operator >(OscTimeTag lhs, OscTimeTag rhs)
	{
		return lhs.DateTime > rhs.DateTime;
	}

	public static bool operator >=(OscTimeTag lhs, OscTimeTag rhs)
	{
		return lhs.DateTime >= rhs.DateTime;
	}

	public static bool IsValidTime(DateTime timeStamp)
	{
		return timeStamp >= Epoch + TimeSpan.FromMilliseconds(1.0);
	}

	public void Set(DateTime timeStamp)
	{
		timeStamp = new DateTime(timeStamp.Ticks - timeStamp.Ticks % 10000, timeStamp.Kind);
		Assert.IsTrue(IsValidTime(timeStamp));
		mTimeStamp = timeStamp;
	}

	public byte[] ToByteArray()
	{
		List<byte> list = new List<byte>();
		byte[] array = BitConverter.GetBytes(SecondsSinceEpoch);
		byte[] array2 = BitConverter.GetBytes(FractionalSecond);
		if (BitConverter.IsLittleEndian != OscPacket.LittleEndianByteOrder)
		{
			array = Utility.SwapEndian(array);
			array2 = Utility.SwapEndian(array2);
		}
		list.AddRange(array);
		list.AddRange(array2);
		return list.ToArray();
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		OscTimeTag oscTimeTag = value as OscTimeTag;
		if (oscTimeTag == null)
		{
			return false;
		}
		return mTimeStamp.Equals(oscTimeTag.mTimeStamp);
	}

	public bool Equals(OscTimeTag value)
	{
		if ((object)value == null)
		{
			return false;
		}
		return mTimeStamp.Equals(value.mTimeStamp);
	}

	public override int GetHashCode()
	{
		return mTimeStamp.GetHashCode();
	}

	public override string ToString()
	{
		return mTimeStamp.ToString();
	}
}
