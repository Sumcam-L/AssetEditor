using System;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class UFixed16_16
{
	public ushort Integer { get; set; }

	public ushort Fraction { get; set; }

	public UFixed16_16(ushort integer, ushort fraction)
	{
		Integer = integer;
		Fraction = fraction;
	}

	public UFixed16_16(uint value)
	{
		Integer = (ushort)(value >> 16);
		Fraction = (ushort)(value & 0xFFFF);
	}

	public UFixed16_16(double value)
	{
		if (value >= 65536.0)
		{
			throw new OverflowException();
		}
		if (value < 0.0)
		{
			throw new OverflowException();
		}
		Integer = (ushort)value;
		Fraction = (ushort)((value - (double)(int)Integer) * 65536.0 + 0.5);
	}

	public static implicit operator double(UFixed16_16 value)
	{
		return (double)(int)value.Integer + (double)(int)value.Fraction / 65536.0;
	}
}
