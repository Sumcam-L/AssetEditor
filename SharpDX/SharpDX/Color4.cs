using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DynamicSerializer("TKC4")]
public struct Color4 : IEquatable<Color4>, IFormattable, IDataSerializable
{
	private const string toStringFormat = "Alpha:{0} Red:{1} Green:{2} Blue:{3}";

	public static readonly Color4 Black;

	public static readonly Color4 White;

	public float Red;

	public float Green;

	public float Blue;

	public float Alpha;

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => Red, 
				1 => Green, 
				2 => Blue, 
				3 => Alpha, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Color4 run from 0 to 3, inclusive."), 
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
			case 3:
				Alpha = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Color4 run from 0 to 3, inclusive.");
			}
		}
	}

	public Color4(float value)
	{
		Alpha = (Red = (Green = (Blue = value)));
	}

	public Color4(float red, float green, float blue, float alpha)
	{
		Red = red;
		Green = green;
		Blue = blue;
		Alpha = alpha;
	}

	public Color4(Vector4 value)
	{
		Red = value.X;
		Green = value.Y;
		Blue = value.Z;
		Alpha = value.W;
	}

	public Color4(Vector3 value, float alpha)
	{
		Red = value.X;
		Green = value.Y;
		Blue = value.Z;
		Alpha = alpha;
	}

	public Color4(uint rgba)
	{
		Alpha = (float)((rgba >> 24) & 0xFF) / 255f;
		Blue = (float)((rgba >> 16) & 0xFF) / 255f;
		Green = (float)((rgba >> 8) & 0xFF) / 255f;
		Red = (float)(rgba & 0xFF) / 255f;
	}

	public Color4(int rgba)
	{
		Alpha = (float)((rgba >> 24) & 0xFF) / 255f;
		Blue = (float)((rgba >> 16) & 0xFF) / 255f;
		Green = (float)((rgba >> 8) & 0xFF) / 255f;
		Red = (float)(rgba & 0xFF) / 255f;
	}

	public Color4(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Color4.");
		}
		Red = values[0];
		Green = values[1];
		Blue = values[2];
		Alpha = values[3];
	}

	public Color4(Color3 color)
	{
		Red = color.Red;
		Green = color.Green;
		Blue = color.Blue;
		Alpha = 1f;
	}

	public Color4(Color3 color, float alpha)
	{
		Red = color.Red;
		Green = color.Green;
		Blue = color.Blue;
		Alpha = alpha;
	}

	public int ToBgra()
	{
		uint num = (uint)(Alpha * 255f) & 0xFF;
		uint num2 = (uint)(Red * 255f) & 0xFF;
		uint num3 = (uint)(Green * 255f) & 0xFF;
		uint num4 = (uint)(Blue * 255f) & 0xFF;
		uint num5 = num4;
		num5 |= num3 << 8;
		num5 |= num2 << 16;
		return (int)(num5 | (num << 24));
	}

	public void ToBgra(out byte r, out byte g, out byte b, out byte a)
	{
		a = (byte)(Alpha * 255f);
		r = (byte)(Red * 255f);
		g = (byte)(Green * 255f);
		b = (byte)(Blue * 255f);
	}

	public int ToRgba()
	{
		uint num = (uint)(Alpha * 255f) & 0xFF;
		uint num2 = (uint)(Red * 255f) & 0xFF;
		uint num3 = (uint)(Green * 255f) & 0xFF;
		uint num4 = (uint)(Blue * 255f) & 0xFF;
		uint num5 = num2;
		num5 |= num3 << 8;
		num5 |= num4 << 16;
		return (int)(num5 | (num << 24));
	}

	public Vector3 ToVector3()
	{
		return new Vector3(Red, Green, Blue);
	}

	public Vector4 ToVector4()
	{
		return new Vector4(Red, Green, Blue, Alpha);
	}

	public float[] ToArray()
	{
		return new float[4] { Red, Green, Blue, Alpha };
	}

	public static void Add(ref Color4 left, ref Color4 right, out Color4 result)
	{
		result.Alpha = left.Alpha + right.Alpha;
		result.Red = left.Red + right.Red;
		result.Green = left.Green + right.Green;
		result.Blue = left.Blue + right.Blue;
	}

	public static Color4 Add(Color4 left, Color4 right)
	{
		return new Color4(left.Red + right.Red, left.Green + right.Green, left.Blue + right.Blue, left.Alpha + right.Alpha);
	}

	public static void Subtract(ref Color4 left, ref Color4 right, out Color4 result)
	{
		result.Alpha = left.Alpha - right.Alpha;
		result.Red = left.Red - right.Red;
		result.Green = left.Green - right.Green;
		result.Blue = left.Blue - right.Blue;
	}

	public static Color4 Subtract(Color4 left, Color4 right)
	{
		return new Color4(left.Red - right.Red, left.Green - right.Green, left.Blue - right.Blue, left.Alpha - right.Alpha);
	}

	public static void Modulate(ref Color4 left, ref Color4 right, out Color4 result)
	{
		result.Alpha = left.Alpha * right.Alpha;
		result.Red = left.Red * right.Red;
		result.Green = left.Green * right.Green;
		result.Blue = left.Blue * right.Blue;
	}

	public static Color4 Modulate(Color4 left, Color4 right)
	{
		return new Color4(left.Red * right.Red, left.Green * right.Green, left.Blue * right.Blue, left.Alpha * right.Alpha);
	}

	public static void Scale(ref Color4 value, float scale, out Color4 result)
	{
		result.Alpha = value.Alpha * scale;
		result.Red = value.Red * scale;
		result.Green = value.Green * scale;
		result.Blue = value.Blue * scale;
	}

	public static Color4 Scale(Color4 value, float scale)
	{
		return new Color4(value.Red * scale, value.Green * scale, value.Blue * scale, value.Alpha * scale);
	}

	public static void Negate(ref Color4 value, out Color4 result)
	{
		result.Alpha = 1f - value.Alpha;
		result.Red = 1f - value.Red;
		result.Green = 1f - value.Green;
		result.Blue = 1f - value.Blue;
	}

	public static Color4 Negate(Color4 value)
	{
		return new Color4(1f - value.Red, 1f - value.Green, 1f - value.Blue, 1f - value.Alpha);
	}

	public static void Clamp(ref Color4 value, ref Color4 min, ref Color4 max, out Color4 result)
	{
		float alpha = value.Alpha;
		alpha = ((alpha > max.Alpha) ? max.Alpha : alpha);
		alpha = ((alpha < min.Alpha) ? min.Alpha : alpha);
		float red = value.Red;
		red = ((red > max.Red) ? max.Red : red);
		red = ((red < min.Red) ? min.Red : red);
		float green = value.Green;
		green = ((green > max.Green) ? max.Green : green);
		green = ((green < min.Green) ? min.Green : green);
		float blue = value.Blue;
		blue = ((blue > max.Blue) ? max.Blue : blue);
		blue = ((blue < min.Blue) ? min.Blue : blue);
		result = new Color4(red, green, blue, alpha);
	}

	public static Color4 Clamp(Color4 value, Color4 min, Color4 max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Lerp(ref Color4 start, ref Color4 end, float amount, out Color4 result)
	{
		result.Red = MathUtil.Lerp(start.Red, end.Red, amount);
		result.Green = MathUtil.Lerp(start.Green, end.Green, amount);
		result.Blue = MathUtil.Lerp(start.Blue, end.Blue, amount);
		result.Alpha = MathUtil.Lerp(start.Alpha, end.Alpha, amount);
	}

	public static Color4 Lerp(Color4 start, Color4 end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Color4 start, ref Color4 end, float amount, out Color4 result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Color4 SmoothStep(Color4 start, Color4 end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Max(ref Color4 left, ref Color4 right, out Color4 result)
	{
		result.Alpha = ((left.Alpha > right.Alpha) ? left.Alpha : right.Alpha);
		result.Red = ((left.Red > right.Red) ? left.Red : right.Red);
		result.Green = ((left.Green > right.Green) ? left.Green : right.Green);
		result.Blue = ((left.Blue > right.Blue) ? left.Blue : right.Blue);
	}

	public static Color4 Max(Color4 left, Color4 right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Color4 left, ref Color4 right, out Color4 result)
	{
		result.Alpha = ((left.Alpha < right.Alpha) ? left.Alpha : right.Alpha);
		result.Red = ((left.Red < right.Red) ? left.Red : right.Red);
		result.Green = ((left.Green < right.Green) ? left.Green : right.Green);
		result.Blue = ((left.Blue < right.Blue) ? left.Blue : right.Blue);
	}

	public static Color4 Min(Color4 left, Color4 right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static void AdjustContrast(ref Color4 value, float contrast, out Color4 result)
	{
		result.Alpha = value.Alpha;
		result.Red = 0.5f + contrast * (value.Red - 0.5f);
		result.Green = 0.5f + contrast * (value.Green - 0.5f);
		result.Blue = 0.5f + contrast * (value.Blue - 0.5f);
	}

	public static Color4 AdjustContrast(Color4 value, float contrast)
	{
		return new Color4(0.5f + contrast * (value.Red - 0.5f), 0.5f + contrast * (value.Green - 0.5f), 0.5f + contrast * (value.Blue - 0.5f), value.Alpha);
	}

	public static void AdjustSaturation(ref Color4 value, float saturation, out Color4 result)
	{
		float num = value.Red * 0.2125f + value.Green * 0.7154f + value.Blue * 0.0721f;
		result.Alpha = value.Alpha;
		result.Red = num + saturation * (value.Red - num);
		result.Green = num + saturation * (value.Green - num);
		result.Blue = num + saturation * (value.Blue - num);
	}

	public static Color4 AdjustSaturation(Color4 value, float saturation)
	{
		float num = value.Red * 0.2125f + value.Green * 0.7154f + value.Blue * 0.0721f;
		return new Color4(num + saturation * (value.Red - num), num + saturation * (value.Green - num), num + saturation * (value.Blue - num), value.Alpha);
	}

	public static void Premultiply(ref Color4 value, out Color4 result)
	{
		result.Alpha = value.Alpha;
		result.Red = value.Red * value.Alpha;
		result.Green = value.Green * value.Alpha;
		result.Blue = value.Blue * value.Alpha;
	}

	public static Color4 Premultiply(Color4 value)
	{
		Premultiply(ref value, out var result);
		return result;
	}

	public static Color4 operator +(Color4 left, Color4 right)
	{
		return new Color4(left.Red + right.Red, left.Green + right.Green, left.Blue + right.Blue, left.Alpha + right.Alpha);
	}

	public static Color4 operator +(Color4 value)
	{
		return value;
	}

	public static Color4 operator -(Color4 left, Color4 right)
	{
		return new Color4(left.Red - right.Red, left.Green - right.Green, left.Blue - right.Blue, left.Alpha - right.Alpha);
	}

	public static Color4 operator -(Color4 value)
	{
		return new Color4(0f - value.Red, 0f - value.Green, 0f - value.Blue, 0f - value.Alpha);
	}

	public static Color4 operator *(float scale, Color4 value)
	{
		return new Color4(value.Red * scale, value.Green * scale, value.Blue * scale, value.Alpha * scale);
	}

	public static Color4 operator *(Color4 value, float scale)
	{
		return new Color4(value.Red * scale, value.Green * scale, value.Blue * scale, value.Alpha * scale);
	}

	public static Color4 operator *(Color4 left, Color4 right)
	{
		return new Color4(left.Red * right.Red, left.Green * right.Green, left.Blue * right.Blue, left.Alpha * right.Alpha);
	}

	public static bool operator ==(Color4 left, Color4 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Color4 left, Color4 right)
	{
		return !left.Equals(right);
	}

	public static explicit operator Color3(Color4 value)
	{
		return new Color3(value.Red, value.Green, value.Blue);
	}

	public static explicit operator Vector3(Color4 value)
	{
		return new Vector3(value.Red, value.Green, value.Blue);
	}

	public static implicit operator Vector4(Color4 value)
	{
		return new Vector4(value.Red, value.Green, value.Blue, value.Alpha);
	}

	public static explicit operator Color4(Vector3 value)
	{
		return new Color4(value.X, value.Y, value.Z, 1f);
	}

	public static explicit operator Color4(Vector4 value)
	{
		return new Color4(value.X, value.Y, value.Z, value.W);
	}

	public static explicit operator Color4(ColorBGRA value)
	{
		return new Color4((int)value.R, (int)value.G, (int)value.B, (int)value.A);
	}

	public static explicit operator ColorBGRA(Color4 value)
	{
		return new ColorBGRA(value.Red, value.Green, value.Blue, value.Alpha);
	}

	public static explicit operator int(Color4 value)
	{
		return value.ToRgba();
	}

	public static explicit operator Color4(int value)
	{
		return new Color4(value);
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
		return string.Format(formatProvider, "Alpha:{0} Red:{1} Green:{2} Blue:{3}", Alpha, Red, Green, Blue);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "Alpha:{0} Red:{1} Green:{2} Blue:{3}", Alpha.ToString(format, formatProvider), Red.ToString(format, formatProvider), Green.ToString(format, formatProvider), Blue.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		int hashCode = Red.GetHashCode();
		hashCode = (hashCode * 397) ^ Green.GetHashCode();
		hashCode = (hashCode * 397) ^ Blue.GetHashCode();
		return (hashCode * 397) ^ Alpha.GetHashCode();
	}

	public bool Equals(Color4 other)
	{
		if (Alpha == other.Alpha && Red == other.Red && Green == other.Green)
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
		if (!object.ReferenceEquals(value.GetType(), typeof(Color4)))
		{
			return false;
		}
		return Equals((Color4)value);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(Red);
			serializer.Writer.Write(Green);
			serializer.Writer.Write(Blue);
			serializer.Writer.Write(Alpha);
		}
		else
		{
			Red = serializer.Reader.ReadSingle();
			Green = serializer.Reader.ReadSingle();
			Blue = serializer.Reader.ReadSingle();
			Alpha = serializer.Reader.ReadSingle();
		}
	}

	static Color4()
	{
		Black = new Color4(0f, 0f, 0f, 1f);
		White = new Color4(1f, 1f, 1f, 1f);
	}
}
