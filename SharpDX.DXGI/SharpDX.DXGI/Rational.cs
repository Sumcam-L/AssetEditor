using System;

namespace SharpDX.DXGI;

public struct Rational : IEquatable<Rational>
{
	public int Numerator;

	public int Denominator;

	public static readonly Rational Empty = default(Rational);

	public Rational(int numerator, int denominator)
	{
		Numerator = numerator;
		Denominator = denominator;
	}

	public bool Equals(Rational other)
	{
		if (Numerator == other.Numerator)
		{
			return Denominator == other.Denominator;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is Rational)
		{
			return Equals((Rational)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Numerator * 397) ^ Denominator;
	}

	public static bool operator ==(Rational left, Rational right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Rational left, Rational right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"{Numerator}/{Denominator}";
	}
}
