using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Size = 4)]
[DynamicSerializer("TKC0")]
public struct ColorBGRA : IEquatable<ColorBGRA>, IFormattable, IDataSerializable
{
	private const string toStringFormat = "A:{0} R:{1} G:{2} B:{3}";

	public byte B;

	public byte G;

	public byte R;

	public byte A;

	public byte this[int index]
	{
		get
		{
			return index switch
			{
				0 => B, 
				1 => G, 
				2 => R, 
				3 => A, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for ColorBGRA run from 0 to 3, inclusive."), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				B = value;
				break;
			case 1:
				G = value;
				break;
			case 2:
				R = value;
				break;
			case 3:
				A = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for ColorBGRA run from 0 to 3, inclusive.");
			}
		}
	}

	public ColorBGRA(byte value)
	{
		A = (R = (G = (B = value)));
	}

	public ColorBGRA(float value)
	{
		A = (R = (G = (B = ToByte(value))));
	}

	public ColorBGRA(byte red, byte green, byte blue, byte alpha)
	{
		R = red;
		G = green;
		B = blue;
		A = alpha;
	}

	public ColorBGRA(float red, float green, float blue, float alpha)
	{
		R = ToByte(red);
		G = ToByte(green);
		B = ToByte(blue);
		A = ToByte(alpha);
	}

	public ColorBGRA(Vector4 value)
	{
		R = ToByte(value.X);
		G = ToByte(value.Y);
		B = ToByte(value.Z);
		A = ToByte(value.W);
	}

	public ColorBGRA(Vector3 value, float alpha)
	{
		R = ToByte(value.X);
		G = ToByte(value.Y);
		B = ToByte(value.Z);
		A = ToByte(alpha);
	}

	public ColorBGRA(uint bgra)
	{
		A = (byte)((bgra >> 24) & 0xFF);
		R = (byte)((bgra >> 16) & 0xFF);
		G = (byte)((bgra >> 8) & 0xFF);
		B = (byte)(bgra & 0xFF);
	}

	public ColorBGRA(int bgra)
	{
		A = (byte)((bgra >> 24) & 0xFF);
		R = (byte)((bgra >> 16) & 0xFF);
		G = (byte)((bgra >> 8) & 0xFF);
		B = (byte)(bgra & 0xFF);
	}

	public ColorBGRA(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for ColorBGRA.");
		}
		B = ToByte(values[0]);
		G = ToByte(values[1]);
		R = ToByte(values[2]);
		A = ToByte(values[3]);
	}

	public ColorBGRA(byte[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for ColorBGRA.");
		}
		B = values[0];
		G = values[1];
		R = values[2];
		A = values[3];
	}

	public int ToBgra()
	{
		int b = B;
		b |= G << 8;
		b |= R << 16;
		return b | (A << 24);
	}

	public int ToRgba()
	{
		int r = R;
		r |= G << 8;
		r |= B << 16;
		return r | (A << 24);
	}

	public Vector3 ToVector3()
	{
		return new Vector3((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f);
	}

	public Color3 ToColor3()
	{
		return new Color3((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f);
	}

	public Vector4 ToVector4()
	{
		return new Vector4((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f, (float)(int)A / 255f);
	}

	public byte[] ToArray()
	{
		return new byte[4] { B, G, R, A };
	}

	public float GetBrightness()
	{
		float num = (float)(int)R / 255f;
		float num2 = (float)(int)G / 255f;
		float num3 = (float)(int)B / 255f;
		float num4 = num;
		float num5 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		if (num2 < num5)
		{
			num5 = num2;
		}
		if (num3 < num5)
		{
			num5 = num3;
		}
		return (num4 + num5) / 2f;
	}

	public float GetHue()
	{
		if (R == G && G == B)
		{
			return 0f;
		}
		float num = (float)(int)R / 255f;
		float num2 = (float)(int)G / 255f;
		float num3 = (float)(int)B / 255f;
		float num4 = 0f;
		float num5 = num;
		float num6 = num;
		if (num2 > num5)
		{
			num5 = num2;
		}
		if (num3 > num5)
		{
			num5 = num3;
		}
		if (num2 < num6)
		{
			num6 = num2;
		}
		if (num3 < num6)
		{
			num6 = num3;
		}
		float num7 = num5 - num6;
		if (num == num5)
		{
			num4 = (num2 - num3) / num7;
		}
		else if (num2 == num5)
		{
			num4 = 2f + (num3 - num) / num7;
		}
		else if (num3 == num5)
		{
			num4 = 4f + (num - num2) / num7;
		}
		num4 *= 60f;
		if (num4 < 0f)
		{
			num4 += 360f;
		}
		return num4;
	}

	public float GetSaturation()
	{
		float num = (float)(int)R / 255f;
		float num2 = (float)(int)G / 255f;
		float num3 = (float)(int)B / 255f;
		float result = 0f;
		float num4 = num;
		float num5 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		if (num2 < num5)
		{
			num5 = num2;
		}
		if (num3 < num5)
		{
			num5 = num3;
		}
		if (num4 != num5)
		{
			float num6 = (num4 + num5) / 2f;
			result = ((!((double)num6 <= 0.5)) ? ((num4 - num5) / (2f - num4 - num5)) : ((num4 - num5) / (num4 + num5)));
		}
		return result;
	}

	public static ColorBGRA FromBgra(int color)
	{
		return new ColorBGRA(color);
	}

	public static ColorBGRA FromBgra(uint color)
	{
		return new ColorBGRA(color);
	}

	public static ColorBGRA FromRgba(int color)
	{
		return new ColorBGRA((byte)(color & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF), (byte)((color >> 24) & 0xFF));
	}

	public static ColorBGRA FromRgba(uint color)
	{
		return FromRgba((int)color);
	}

	public static void Add(ref ColorBGRA left, ref ColorBGRA right, out ColorBGRA result)
	{
		result.A = (byte)(left.A + right.A);
		result.R = (byte)(left.R + right.R);
		result.G = (byte)(left.G + right.G);
		result.B = (byte)(left.B + right.B);
	}

	public static ColorBGRA Add(ColorBGRA left, ColorBGRA right)
	{
		return new ColorBGRA(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
	}

	public static void Subtract(ref ColorBGRA left, ref ColorBGRA right, out ColorBGRA result)
	{
		result.A = (byte)(left.A - right.A);
		result.R = (byte)(left.R - right.R);
		result.G = (byte)(left.G - right.G);
		result.B = (byte)(left.B - right.B);
	}

	public static ColorBGRA Subtract(ColorBGRA left, ColorBGRA right)
	{
		return new ColorBGRA(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
	}

	public static void Modulate(ref ColorBGRA left, ref ColorBGRA right, out ColorBGRA result)
	{
		result.A = (byte)((float)(left.A * right.A) / 255f);
		result.R = (byte)((float)(left.R * right.R) / 255f);
		result.G = (byte)((float)(left.G * right.G) / 255f);
		result.B = (byte)((float)(left.B * right.B) / 255f);
	}

	public static ColorBGRA Modulate(ColorBGRA left, ColorBGRA right)
	{
		return new ColorBGRA(left.R * right.R >> 8, left.G * right.G >> 8, left.B * right.B >> 8, left.A * right.A >> 8);
	}

	public static void Scale(ref ColorBGRA value, float scale, out ColorBGRA result)
	{
		result.A = (byte)((float)(int)value.A * scale);
		result.R = (byte)((float)(int)value.R * scale);
		result.G = (byte)((float)(int)value.G * scale);
		result.B = (byte)((float)(int)value.B * scale);
	}

	public static ColorBGRA Scale(ColorBGRA value, float scale)
	{
		return new ColorBGRA((byte)((float)(int)value.R * scale), (byte)((float)(int)value.G * scale), (byte)((float)(int)value.B * scale), (byte)((float)(int)value.A * scale));
	}

	public static void Negate(ref ColorBGRA value, out ColorBGRA result)
	{
		result.A = (byte)(255 - value.A);
		result.R = (byte)(255 - value.R);
		result.G = (byte)(255 - value.G);
		result.B = (byte)(255 - value.B);
	}

	public static ColorBGRA Negate(ColorBGRA value)
	{
		return new ColorBGRA(255 - value.R, 255 - value.G, 255 - value.B, 255 - value.A);
	}

	public static void Clamp(ref ColorBGRA value, ref ColorBGRA min, ref ColorBGRA max, out ColorBGRA result)
	{
		byte a = value.A;
		a = ((a > max.A) ? max.A : a);
		a = ((a < min.A) ? min.A : a);
		byte r = value.R;
		r = ((r > max.R) ? max.R : r);
		r = ((r < min.R) ? min.R : r);
		byte g = value.G;
		g = ((g > max.G) ? max.G : g);
		g = ((g < min.G) ? min.G : g);
		byte b = value.B;
		b = ((b > max.B) ? max.B : b);
		b = ((b < min.B) ? min.B : b);
		result = new ColorBGRA(r, g, b, a);
	}

	public static ColorBGRA Clamp(ColorBGRA value, ColorBGRA min, ColorBGRA max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Lerp(ref ColorBGRA start, ref ColorBGRA end, float amount, out ColorBGRA result)
	{
		result.B = MathUtil.Lerp(start.B, end.B, amount);
		result.G = MathUtil.Lerp(start.G, end.G, amount);
		result.R = MathUtil.Lerp(start.R, end.R, amount);
		result.A = MathUtil.Lerp(start.A, end.A, amount);
	}

	public static ColorBGRA Lerp(ColorBGRA start, ColorBGRA end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref ColorBGRA start, ref ColorBGRA end, float amount, out ColorBGRA result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static ColorBGRA SmoothStep(ColorBGRA start, ColorBGRA end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Max(ref ColorBGRA left, ref ColorBGRA right, out ColorBGRA result)
	{
		result.A = ((left.A > right.A) ? left.A : right.A);
		result.R = ((left.R > right.R) ? left.R : right.R);
		result.G = ((left.G > right.G) ? left.G : right.G);
		result.B = ((left.B > right.B) ? left.B : right.B);
	}

	public static ColorBGRA Max(ColorBGRA left, ColorBGRA right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref ColorBGRA left, ref ColorBGRA right, out ColorBGRA result)
	{
		result.A = ((left.A < right.A) ? left.A : right.A);
		result.R = ((left.R < right.R) ? left.R : right.R);
		result.G = ((left.G < right.G) ? left.G : right.G);
		result.B = ((left.B < right.B) ? left.B : right.B);
	}

	public static ColorBGRA Min(ColorBGRA left, ColorBGRA right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static void AdjustContrast(ref ColorBGRA value, float contrast, out ColorBGRA result)
	{
		result.A = value.A;
		result.R = ToByte(0.5f + contrast * ((float)(int)value.R / 255f - 0.5f));
		result.G = ToByte(0.5f + contrast * ((float)(int)value.G / 255f - 0.5f));
		result.B = ToByte(0.5f + contrast * ((float)(int)value.B / 255f - 0.5f));
	}

	public static ColorBGRA AdjustContrast(ColorBGRA value, float contrast)
	{
		return new ColorBGRA(ToByte(0.5f + contrast * ((float)(int)value.R / 255f - 0.5f)), ToByte(0.5f + contrast * ((float)(int)value.G / 255f - 0.5f)), ToByte(0.5f + contrast * ((float)(int)value.B / 255f - 0.5f)), value.A);
	}

	public static void AdjustSaturation(ref ColorBGRA value, float saturation, out ColorBGRA result)
	{
		float num = (float)(int)value.R / 255f * 0.2125f + (float)(int)value.G / 255f * 0.7154f + (float)(int)value.B / 255f * 0.0721f;
		result.A = value.A;
		result.R = ToByte(num + saturation * ((float)(int)value.R / 255f - num));
		result.G = ToByte(num + saturation * ((float)(int)value.G / 255f - num));
		result.B = ToByte(num + saturation * ((float)(int)value.B / 255f - num));
	}

	public static ColorBGRA AdjustSaturation(ColorBGRA value, float saturation)
	{
		float num = (float)(int)value.R / 255f * 0.2125f + (float)(int)value.G / 255f * 0.7154f + (float)(int)value.B / 255f * 0.0721f;
		return new ColorBGRA(ToByte(num + saturation * ((float)(int)value.R / 255f - num)), ToByte(num + saturation * ((float)(int)value.G / 255f - num)), ToByte(num + saturation * ((float)(int)value.B / 255f - num)), value.A);
	}

	public static void Premultiply(ref ColorBGRA value, out ColorBGRA result)
	{
		float num = (float)(int)value.A / 65025f;
		result.A = value.A;
		result.R = ToByte((float)(int)value.R * num);
		result.G = ToByte((float)(int)value.G * num);
		result.B = ToByte((float)(int)value.B * num);
	}

	public static ColorBGRA Premultiply(ColorBGRA value)
	{
		Premultiply(ref value, out var result);
		return result;
	}

	public static ColorBGRA operator +(ColorBGRA left, ColorBGRA right)
	{
		return new ColorBGRA(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
	}

	public static ColorBGRA operator +(ColorBGRA value)
	{
		return value;
	}

	public static ColorBGRA operator -(ColorBGRA left, ColorBGRA right)
	{
		return new ColorBGRA(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
	}

	public static ColorBGRA operator -(ColorBGRA value)
	{
		return new ColorBGRA(-value.R, -value.G, -value.B, -value.A);
	}

	public static ColorBGRA operator *(float scale, ColorBGRA value)
	{
		return new ColorBGRA((byte)((float)(int)value.R * scale), (byte)((float)(int)value.G * scale), (byte)((float)(int)value.B * scale), (byte)((float)(int)value.A * scale));
	}

	public static ColorBGRA operator *(ColorBGRA value, float scale)
	{
		return new ColorBGRA((byte)((float)(int)value.R * scale), (byte)((float)(int)value.G * scale), (byte)((float)(int)value.B * scale), (byte)((float)(int)value.A * scale));
	}

	public static ColorBGRA operator *(ColorBGRA left, ColorBGRA right)
	{
		return new ColorBGRA((byte)((float)(left.R * right.R) / 255f), (byte)((float)(left.G * right.G) / 255f), (byte)((float)(left.B * right.B) / 255f), (byte)((float)(left.A * right.A) / 255f));
	}

	public static bool operator ==(ColorBGRA left, ColorBGRA right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ColorBGRA left, ColorBGRA right)
	{
		return !left.Equals(right);
	}

	public static explicit operator Color3(ColorBGRA value)
	{
		return new Color3((int)value.R, (int)value.G, (int)value.B);
	}

	public static explicit operator Vector3(ColorBGRA value)
	{
		return new Vector3((float)(int)value.R / 255f, (float)(int)value.G / 255f, (float)(int)value.B / 255f);
	}

	public static explicit operator Vector4(ColorBGRA value)
	{
		return new Vector4((float)(int)value.R / 255f, (float)(int)value.G / 255f, (float)(int)value.B / 255f, (float)(int)value.A / 255f);
	}

	public static explicit operator Color4(ColorBGRA value)
	{
		return new Color4((float)(int)value.R / 255f, (float)(int)value.G / 255f, (float)(int)value.B / 255f, (float)(int)value.A / 255f);
	}

	public static explicit operator ColorBGRA(Vector3 value)
	{
		return new ColorBGRA(value.X / 255f, value.Y / 255f, value.Z / 255f, 1f);
	}

	public static explicit operator ColorBGRA(Color3 value)
	{
		return new ColorBGRA(value.Red, value.Green, value.Blue, 1f);
	}

	public static explicit operator ColorBGRA(Vector4 value)
	{
		return new ColorBGRA(value.X, value.Y, value.Z, value.W);
	}

	public static explicit operator ColorBGRA(Color4 value)
	{
		return new ColorBGRA(value.Red, value.Green, value.Blue, value.Alpha);
	}

	public static implicit operator ColorBGRA(Color value)
	{
		return new ColorBGRA(value.R, value.G, value.B, value.A);
	}

	public static implicit operator Color(ColorBGRA value)
	{
		return new Color(value.R, value.G, value.B, value.A);
	}

	public static explicit operator int(ColorBGRA value)
	{
		return value.ToBgra();
	}

	public static explicit operator ColorBGRA(int value)
	{
		return new ColorBGRA(value);
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
		return string.Format(formatProvider, "A:{0} R:{1} G:{2} B:{3}", A, R, G, B);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "A:{0} R:{1} G:{2} B:{3}", A.ToString(format, formatProvider), R.ToString(format, formatProvider), G.ToString(format, formatProvider), B.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		int hashCode = B.GetHashCode();
		hashCode = (hashCode * 397) ^ G.GetHashCode();
		hashCode = (hashCode * 397) ^ R.GetHashCode();
		return (hashCode * 397) ^ A.GetHashCode();
	}

	public bool Equals(ColorBGRA other)
	{
		if (R == other.R && G == other.G && B == other.B)
		{
			return A == other.A;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(ColorBGRA)))
		{
			return false;
		}
		return Equals((ColorBGRA)value);
	}

	private static byte ToByte(float component)
	{
		int num = (int)(component * 255f);
		return (byte)((num >= 0) ? ((num > 255) ? 255u : ((uint)num)) : 0u);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(ToBgra());
		}
		else
		{
			this = FromBgra(serializer.Reader.ReadInt32());
		}
	}
}
