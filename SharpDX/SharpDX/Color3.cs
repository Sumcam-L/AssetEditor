using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKC3")]
public struct Color3 : IEquatable<Color3>, IFormattable, IDataSerializable
{
	private const string toStringFormat = "Red:{0} Green:{1} Blue:{2}";

	public static readonly Color3 Black;

	public static readonly Color3 White;

	public float Red;

	public float Green;

	public float Blue;

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => Red, 
				1 => Green, 
				2 => Blue, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Color3 run from 0 to 2, inclusive."), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				Red = value;
				break;
			case 1:
				Green = value;
				break;
			case 2:
				Blue = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Color3 run from 0 to 2, inclusive.");
			}
		}
	}

	public Color3(float value)
	{
		Red = (Green = (Blue = value));
	}

	public Color3(float red, float green, float blue)
	{
		Red = red;
		Green = green;
		Blue = blue;
	}

	public Color3(Vector3 value)
	{
		Red = value.X;
		Green = value.Y;
		Blue = value.Z;
	}

	public Color3(int rgb)
	{
		Blue = (float)((rgb >> 16) & 0xFF) / 255f;
		Green = (float)((rgb >> 8) & 0xFF) / 255f;
		Red = (float)(rgb & 0xFF) / 255f;
	}

	public Color3(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 3)
		{
			throw new ArgumentOutOfRangeException("values", "There must be three and only three input values for Color3.");
		}
		Red = values[0];
		Green = values[1];
		Blue = values[2];
	}

	public int ToRgba()
	{
		uint num = 255u;
		uint num2 = (uint)(Red * 255f) & 0xFF;
		uint num3 = (uint)(Green * 255f) & 0xFF;
		uint num4 = (uint)(Blue * 255f) & 0xFF;
		uint num5 = num2;
		num5 |= num3 << 8;
		num5 |= num4 << 16;
		return (int)(num5 | (num << 24));
	}

	public int ToBgra()
	{
		uint num = 255u;
		uint num2 = (uint)(Red * 255f) & 0xFF;
		uint num3 = (uint)(Green * 255f) & 0xFF;
		uint num4 = (uint)(Blue * 255f) & 0xFF;
		uint num5 = num4;
		num5 |= num3 << 8;
		num5 |= num2 << 16;
		return (int)(num5 | (num << 24));
	}

	public Vector3 ToVector3()
	{
		return new Vector3(Red, Green, Blue);
	}

	public float[] ToArray()
	{
		return new float[3] { Red, Green, Blue };
	}

	public static void Add(ref Color3 left, ref Color3 right, out Color3 result)
	{
		result.Red = left.Red + right.Red;
		result.Green = left.Green + right.Green;
		result.Blue = left.Blue + right.Blue;
	}

	public static Color3 Add(Color3 left, Color3 right)
	{
		return new Color3(left.Red + right.Red, left.Green + right.Green, left.Blue + right.Blue);
	}

	public static void Subtract(ref Color3 left, ref Color3 right, out Color3 result)
	{
		result.Red = left.Red - right.Red;
		result.Green = left.Green - right.Green;
		result.Blue = left.Blue - right.Blue;
	}

	public static Color3 Subtract(Color3 left, Color3 right)
	{
		return new Color3(left.Red - right.Red, left.Green - right.Green, left.Blue - right.Blue);
	}

	public static void Modulate(ref Color3 left, ref Color3 right, out Color3 result)
	{
		result.Red = left.Red * right.Red;
		result.Green = left.Green * right.Green;
		result.Blue = left.Blue * right.Blue;
	}

	public static Color3 Modulate(Color3 left, Color3 right)
	{
		return new Color3(left.Red * right.Red, left.Green * right.Green, left.Blue * right.Blue);
	}

	public static void Scale(ref Color3 value, float scale, out Color3 result)
	{
		result.Red = value.Red * scale;
		result.Green = value.Green * scale;
		result.Blue = value.Blue * scale;
	}

	public static Color3 Scale(Color3 value, float scale)
	{
		return new Color3(value.Red * scale, value.Green * scale, value.Blue * scale);
	}

	public static void Negate(ref Color3 value, out Color3 result)
	{
		result.Red = 1f - value.Red;
		result.Green = 1f - value.Green;
		result.Blue = 1f - value.Blue;
	}

	public static Color3 Negate(Color3 value)
	{
		return new Color3(1f - value.Red, 1f - value.Green, 1f - value.Blue);
	}

	public static void Clamp(ref Color3 value, ref Color3 min, ref Color3 max, out Color3 result)
	{
		float red = value.Red;
		red = ((red > max.Red) ? max.Red : red);
		red = ((red < min.Red) ? min.Red : red);
		float green = value.Green;
		green = ((green > max.Green) ? max.Green : green);
		green = ((green < min.Green) ? min.Green : green);
		float blue = value.Blue;
		blue = ((blue > max.Blue) ? max.Blue : blue);
		blue = ((blue < min.Blue) ? min.Blue : blue);
		result = new Color3(red, green, blue);
	}

	public static Color3 Clamp(Color3 value, Color3 min, Color3 max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Lerp(ref Color3 start, ref Color3 end, float amount, out Color3 result)
	{
		result.Red = MathUtil.Lerp(start.Red, end.Red, amount);
		result.Green = MathUtil.Lerp(start.Green, end.Green, amount);
		result.Blue = MathUtil.Lerp(start.Blue, end.Blue, amount);
	}

	public static Color3 Lerp(Color3 start, Color3 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Color3 start, ref Color3 end, float amount, out Color3 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Color3 SmoothStep(Color3 start, Color3 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Max(ref Color3 left, ref Color3 right, out Color3 result)
	{
		result.Red = ((left.Red > right.Red) ? left.Red : right.Red);
		result.Green = ((left.Green > right.Green) ? left.Green : right.Green);
		result.Blue = ((left.Blue > right.Blue) ? left.Blue : right.Blue);
	}

	public static Color3 Max(Color3 left, Color3 right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Color3 left, ref Color3 right, out Color3 result)
	{
		result.Red = ((left.Red < right.Red) ? left.Red : right.Red);
		result.Green = ((left.Green < right.Green) ? left.Green : right.Green);
		result.Blue = ((left.Blue < right.Blue) ? left.Blue : right.Blue);
	}

	public static Color3 Min(Color3 left, Color3 right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static void AdjustContrast(ref Color3 value, float contrast, out Color3 result)
	{
		result.Red = 0.5f + contrast * (value.Red - 0.5f);
		result.Green = 0.5f + contrast * (value.Green - 0.5f);
		result.Blue = 0.5f + contrast * (value.Blue - 0.5f);
	}

	public static Color3 AdjustContrast(Color3 value, float contrast)
	{
		return new Color3(0.5f + contrast * (value.Red - 0.5f), 0.5f + contrast * (value.Green - 0.5f), 0.5f + contrast * (value.Blue - 0.5f));
	}

	public static void AdjustSaturation(ref Color3 value, float saturation, out Color3 result)
	{
		float num = value.Red * 0.2125f + value.Green * 0.7154f + value.Blue * 0.0721f;
		result.Red = num + saturation * (value.Red - num);
		result.Green = num + saturation * (value.Green - num);
		result.Blue = num + saturation * (value.Blue - num);
	}

	public static Color3 AdjustSaturation(Color3 value, float saturation)
	{
		float num = value.Red * 0.2125f + value.Green * 0.7154f + value.Blue * 0.0721f;
		return new Color3(num + saturation * (value.Red - num), num + saturation * (value.Green - num), num + saturation * (value.Blue - num));
	}

	public static void Premultiply(ref Color3 value, float alpha, out Color3 result)
	{
		result.Red = value.Red * alpha;
		result.Green = value.Green * alpha;
		result.Blue = value.Blue * alpha;
	}

	public static Color3 Premultiply(Color3 value, float alpha)
	{
		Premultiply(ref value, alpha, out var result);
		return result;
	}

	public static Color3 operator +(Color3 left, Color3 right)
	{
		return new Color3(left.Red + right.Red, left.Green + right.Green, left.Blue + right.Blue);
	}

	public static Color3 operator +(Color3 value)
	{
		return value;
	}

	public static Color3 operator -(Color3 left, Color3 right)
	{
		return new Color3(left.Red - right.Red, left.Green - right.Green, left.Blue - right.Blue);
	}

	public static Color3 operator -(Color3 value)
	{
		return new Color3(0f - value.Red, 0f - value.Green, 0f - value.Blue);
	}

	public static Color3 operator *(float scale, Color3 value)
	{
		return new Color3(value.Red * scale, value.Green * scale, value.Blue * scale);
	}

	public static Color3 operator *(Color3 value, float scale)
	{
		return new Color3(value.Red * scale, value.Green * scale, value.Blue * scale);
	}

	public static Color3 operator *(Color3 left, Color3 right)
	{
		return new Color3(left.Red * right.Red, left.Green * right.Green, left.Blue * right.Blue);
	}

	public static bool operator ==(Color3 left, Color3 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Color3 left, Color3 right)
	{
		return !left.Equals(right);
	}

	public static explicit operator Color4(Color3 value)
	{
		return new Color4(value.Red, value.Green, value.Blue, 1f);
	}

	public static implicit operator Vector3(Color3 value)
	{
		return new Vector3(value.Red, value.Green, value.Blue);
	}

	public static implicit operator Color3(Vector3 value)
	{
		return new Color3(value.X, value.Y, value.Z);
	}

	public static explicit operator Color3(int value)
	{
		return new Color3(value);
	}

	public override string ToString()
	{
		return ToString(CultureInfo.CurrentCulture);
	}

	public string ToString(string format)
	{
		return ToString(format, CultureInfo.CurrentCulture);
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "Red:{0} Green:{1} Blue:{2}", new object[3] { Red, Green, Blue });
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "Red:{0} Green:{1} Blue:{2}", new object[3]
		{
			Red.ToString(format, formatProvider),
			Green.ToString(format, formatProvider),
			Blue.ToString(format, formatProvider)
		});
	}

	public override int GetHashCode()
	{
		int hashCode = Red.GetHashCode();
		hashCode = (hashCode * 397) ^ Green.GetHashCode();
		return (hashCode * 397) ^ Blue.GetHashCode();
	}

	public bool Equals(Color3 other)
	{
		if (Red == other.Red && Green == other.Green)
		{
			return Blue == other.Blue;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Color3)))
		{
			return false;
		}
		return Equals((Color3)value);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(Red);
			serializer.Writer.Write(Green);
			serializer.Writer.Write(Blue);
		}
		else
		{
			Red = serializer.Reader.ReadSingle();
			Green = serializer.Reader.ReadSingle();
			Blue = serializer.Reader.ReadSingle();
		}
	}

	static Color3()
	{
		Black = new Color3(0f, 0f, 0f);
		White = new Color3(1f, 1f, 1f);
	}
}
