using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Explicit)]
public struct AngleSingle : IComparable, IComparable<AngleSingle>, IEquatable<AngleSingle>, IFormattable, IDataSerializable
{
	public const float Degree = 0.0027777778f;

	public const float Minute = 4.6296296E-05f;

	public const float Second = 7.7160496E-07f;

	public const float Radian = 1f / (2f * (float)Math.PI);

	public const float Milliradian = 0.00015915494f;

	public const float Gradian = 0.0025f;

	[FieldOffset(0)]
	private float radians;

	[FieldOffset(0)]
	private int radiansInt = 0;

	public float Revolutions
	{
		get
		{
			return MathUtil.RadiansToRevolutions(radians);
		}
		set
		{
			radians = MathUtil.RevolutionsToRadians(value);
		}
	}

	public float Degrees
	{
		get
		{
			return MathUtil.RadiansToDegrees(radians);
		}
		set
		{
			radians = MathUtil.DegreesToRadians(value);
		}
	}

	public float Minutes
	{
		get
		{
			float num = MathUtil.RadiansToDegrees(radians);
			if (num < 0f)
			{
				float num2 = (float)Math.Ceiling(num);
				return (num - num2) * 60f;
			}
			float num3 = (float)Math.Floor(num);
			return (num - num3) * 60f;
		}
		set
		{
			float num = MathUtil.RadiansToDegrees(radians);
			float num2 = (float)Math.Floor(num);
			num2 += value / 60f;
			radians = MathUtil.DegreesToRadians(num2);
		}
	}

	public float Seconds
	{
		get
		{
			float num = MathUtil.RadiansToDegrees(radians);
			if (num < 0f)
			{
				float num2 = (float)Math.Ceiling(num);
				float num3 = (num - num2) * 60f;
				float num4 = (float)Math.Ceiling(num3);
				return (num3 - num4) * 60f;
			}
			float num5 = (float)Math.Floor(num);
			float num6 = (num - num5) * 60f;
			float num7 = (float)Math.Floor(num6);
			return (num6 - num7) * 60f;
		}
		set
		{
			float num = MathUtil.RadiansToDegrees(radians);
			float num2 = (float)Math.Floor(num);
			float num3 = (num - num2) * 60f;
			float num4 = (float)Math.Floor(num3);
			num4 += value / 60f;
			num2 += num4 / 60f;
			radians = MathUtil.DegreesToRadians(num2);
		}
	}

	public float Radians
	{
		get
		{
			return radians;
		}
		set
		{
			radians = value;
		}
	}

	public float Milliradians
	{
		get
		{
			return radians / 0.001f;
		}
		set
		{
			radians = value * 0.001f;
		}
	}

	public float Gradians
	{
		get
		{
			return MathUtil.RadiansToGradians(radians);
		}
		set
		{
			radians = MathUtil.RadiansToGradians(value);
		}
	}

	public bool IsRight => radians == (float)Math.PI / 2f;

	public bool IsStraight => radians == (float)Math.PI;

	public bool IsFullRotation => radians == (float)Math.PI * 2f;

	public bool IsOblique => WrapPositive(this).radians != (float)Math.PI / 2f;

	public bool IsAcute
	{
		get
		{
			if ((double)radians > 0.0)
			{
				return radians < (float)Math.PI / 2f;
			}
			return false;
		}
	}

	public bool IsObtuse
	{
		get
		{
			if (radians > (float)Math.PI / 2f)
			{
				return radians < (float)Math.PI;
			}
			return false;
		}
	}

	public bool IsReflex
	{
		get
		{
			if (radians > (float)Math.PI)
			{
				return radians < (float)Math.PI * 2f;
			}
			return false;
		}
	}

	public AngleSingle Complement => new AngleSingle((float)Math.PI / 2f - radians, AngleType.Radian);

	public AngleSingle Supplement => new AngleSingle((float)Math.PI - radians, AngleType.Radian);

	public static AngleSingle ZeroAngle => new AngleSingle(0f, AngleType.Radian);

	public static AngleSingle RightAngle => new AngleSingle((float)Math.PI / 2f, AngleType.Radian);

	public static AngleSingle StraightAngle => new AngleSingle((float)Math.PI, AngleType.Radian);

	public static AngleSingle FullRotationAngle => new AngleSingle((float)Math.PI * 2f, AngleType.Radian);

	public AngleSingle(float angle, AngleType type)
	{
		switch (type)
		{
		case AngleType.Revolution:
			radians = MathUtil.RevolutionsToRadians(angle);
			break;
		case AngleType.Degree:
			radians = MathUtil.DegreesToRadians(angle);
			break;
		case AngleType.Radian:
			radians = angle;
			break;
		case AngleType.Gradian:
			radians = MathUtil.GradiansToRadians(angle);
			break;
		default:
			radians = 0f;
			break;
		}
	}

	public AngleSingle(float arcLength, float radius)
	{
		radians = arcLength / radius;
	}

	public void Wrap()
	{
		float num = (float)Math.IEEERemainder(radians, 6.2831854820251465);
		if (num <= -(float)Math.PI)
		{
			num += (float)Math.PI * 2f;
		}
		else if (num > (float)Math.PI)
		{
			num -= (float)Math.PI * 2f;
		}
		radians = num;
	}

	public void WrapPositive()
	{
		float num = radians % ((float)Math.PI * 2f);
		if ((double)num < 0.0)
		{
			num += (float)Math.PI * 2f;
		}
		radians = num;
	}

	public static AngleSingle Wrap(AngleSingle value)
	{
		value.Wrap();
		return value;
	}

	public static AngleSingle WrapPositive(AngleSingle value)
	{
		value.WrapPositive();
		return value;
	}

	public static AngleSingle Min(AngleSingle left, AngleSingle right)
	{
		if (left.radians < right.radians)
		{
			return left;
		}
		return right;
	}

	public static AngleSingle Max(AngleSingle left, AngleSingle right)
	{
		if (left.radians > right.radians)
		{
			return left;
		}
		return right;
	}

	public static AngleSingle Add(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians + right.radians, AngleType.Radian);
	}

	public static AngleSingle Subtract(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians - right.radians, AngleType.Radian);
	}

	public static AngleSingle Multiply(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians * right.radians, AngleType.Radian);
	}

	public static AngleSingle Divide(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians / right.radians, AngleType.Radian);
	}

	public static bool operator ==(AngleSingle left, AngleSingle right)
	{
		return left.radians == right.radians;
	}

	public static bool operator !=(AngleSingle left, AngleSingle right)
	{
		return left.radians != right.radians;
	}

	public static bool operator <(AngleSingle left, AngleSingle right)
	{
		return left.radians < right.radians;
	}

	public static bool operator >(AngleSingle left, AngleSingle right)
	{
		return left.radians > right.radians;
	}

	public static bool operator <=(AngleSingle left, AngleSingle right)
	{
		return left.radians <= right.radians;
	}

	public static bool operator >=(AngleSingle left, AngleSingle right)
	{
		return left.radians >= right.radians;
	}

	public static AngleSingle operator +(AngleSingle value)
	{
		return value;
	}

	public static AngleSingle operator -(AngleSingle value)
	{
		return new AngleSingle(0f - value.radians, AngleType.Radian);
	}

	public static AngleSingle operator +(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians + right.radians, AngleType.Radian);
	}

	public static AngleSingle operator -(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians - right.radians, AngleType.Radian);
	}

	public static AngleSingle operator *(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians * right.radians, AngleType.Radian);
	}

	public static AngleSingle operator /(AngleSingle left, AngleSingle right)
	{
		return new AngleSingle(left.radians / right.radians, AngleType.Radian);
	}

	public int CompareTo(object other)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is AngleSingle))
		{
			throw new ArgumentException("Argument must be of type Angle.", "other");
		}
		float num = ((AngleSingle)other).radians;
		if (radians > num)
		{
			return 1;
		}
		if (radians < num)
		{
			return -1;
		}
		return 0;
	}

	public int CompareTo(AngleSingle other)
	{
		if (radians > other.radians)
		{
			return 1;
		}
		if (radians < other.radians)
		{
			return -1;
		}
		return 0;
	}

	public bool Equals(AngleSingle other)
	{
		return this == other;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, MathUtil.RadiansToDegrees(radians).ToString("0.##°"));
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "{0}°", new object[1] { MathUtil.RadiansToDegrees(radians).ToString(format, CultureInfo.CurrentCulture) });
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, MathUtil.RadiansToDegrees(radians).ToString("0.##°"));
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "{0}°", new object[1] { MathUtil.RadiansToDegrees(radians).ToString(format, CultureInfo.CurrentCulture) });
	}

	public override int GetHashCode()
	{
		return radiansInt;
	}

	public override bool Equals(object obj)
	{
		if (obj is AngleSingle)
		{
			return this == (AngleSingle)obj;
		}
		return false;
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(radiansInt);
		}
		else
		{
			radiansInt = serializer.Reader.ReadInt32();
		}
	}
}
