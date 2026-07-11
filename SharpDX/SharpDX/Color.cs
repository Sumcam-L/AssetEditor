using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Size = 4)]
[DynamicSerializer("TKC1")]
public struct Color : IEquatable<Color>, IFormattable, IDataSerializable
{
	private const string toStringFormat = "A:{0} R:{1} G:{2} B:{3}";

	public byte R;

	public byte G;

	public byte B;

	public byte A;

	public static readonly Color Zero;

	public static readonly Color Transparent;

	public static readonly Color AliceBlue;

	public static readonly Color AntiqueWhite;

	public static readonly Color Aqua;

	public static readonly Color Aquamarine;

	public static readonly Color Azure;

	public static readonly Color Beige;

	public static readonly Color Bisque;

	public static readonly Color Black;

	public static readonly Color BlanchedAlmond;

	public static readonly Color Blue;

	public static readonly Color BlueViolet;

	public static readonly Color Brown;

	public static readonly Color BurlyWood;

	public static readonly Color CadetBlue;

	public static readonly Color Chartreuse;

	public static readonly Color Chocolate;

	public static readonly Color Coral;

	public static readonly Color CornflowerBlue;

	public static readonly Color Cornsilk;

	public static readonly Color Crimson;

	public static readonly Color Cyan;

	public static readonly Color DarkBlue;

	public static readonly Color DarkCyan;

	public static readonly Color DarkGoldenrod;

	public static readonly Color DarkGray;

	public static readonly Color DarkGreen;

	public static readonly Color DarkKhaki;

	public static readonly Color DarkMagenta;

	public static readonly Color DarkOliveGreen;

	public static readonly Color DarkOrange;

	public static readonly Color DarkOrchid;

	public static readonly Color DarkRed;

	public static readonly Color DarkSalmon;

	public static readonly Color DarkSeaGreen;

	public static readonly Color DarkSlateBlue;

	public static readonly Color DarkSlateGray;

	public static readonly Color DarkTurquoise;

	public static readonly Color DarkViolet;

	public static readonly Color DeepPink;

	public static readonly Color DeepSkyBlue;

	public static readonly Color DimGray;

	public static readonly Color DodgerBlue;

	public static readonly Color Firebrick;

	public static readonly Color FloralWhite;

	public static readonly Color ForestGreen;

	public static readonly Color Fuchsia;

	public static readonly Color Gainsboro;

	public static readonly Color GhostWhite;

	public static readonly Color Gold;

	public static readonly Color Goldenrod;

	public static readonly Color Gray;

	public static readonly Color Green;

	public static readonly Color GreenYellow;

	public static readonly Color Honeydew;

	public static readonly Color HotPink;

	public static readonly Color IndianRed;

	public static readonly Color Indigo;

	public static readonly Color Ivory;

	public static readonly Color Khaki;

	public static readonly Color Lavender;

	public static readonly Color LavenderBlush;

	public static readonly Color LawnGreen;

	public static readonly Color LemonChiffon;

	public static readonly Color LightBlue;

	public static readonly Color LightCoral;

	public static readonly Color LightCyan;

	public static readonly Color LightGoldenrodYellow;

	public static readonly Color LightGray;

	public static readonly Color LightGreen;

	public static readonly Color LightPink;

	public static readonly Color LightSalmon;

	public static readonly Color LightSeaGreen;

	public static readonly Color LightSkyBlue;

	public static readonly Color LightSlateGray;

	public static readonly Color LightSteelBlue;

	public static readonly Color LightYellow;

	public static readonly Color Lime;

	public static readonly Color LimeGreen;

	public static readonly Color Linen;

	public static readonly Color Magenta;

	public static readonly Color Maroon;

	public static readonly Color MediumAquamarine;

	public static readonly Color MediumBlue;

	public static readonly Color MediumOrchid;

	public static readonly Color MediumPurple;

	public static readonly Color MediumSeaGreen;

	public static readonly Color MediumSlateBlue;

	public static readonly Color MediumSpringGreen;

	public static readonly Color MediumTurquoise;

	public static readonly Color MediumVioletRed;

	public static readonly Color MidnightBlue;

	public static readonly Color MintCream;

	public static readonly Color MistyRose;

	public static readonly Color Moccasin;

	public static readonly Color NavajoWhite;

	public static readonly Color Navy;

	public static readonly Color OldLace;

	public static readonly Color Olive;

	public static readonly Color OliveDrab;

	public static readonly Color Orange;

	public static readonly Color OrangeRed;

	public static readonly Color Orchid;

	public static readonly Color PaleGoldenrod;

	public static readonly Color PaleGreen;

	public static readonly Color PaleTurquoise;

	public static readonly Color PaleVioletRed;

	public static readonly Color PapayaWhip;

	public static readonly Color PeachPuff;

	public static readonly Color Peru;

	public static readonly Color Pink;

	public static readonly Color Plum;

	public static readonly Color PowderBlue;

	public static readonly Color Purple;

	public static readonly Color Red;

	public static readonly Color RosyBrown;

	public static readonly Color RoyalBlue;

	public static readonly Color SaddleBrown;

	public static readonly Color Salmon;

	public static readonly Color SandyBrown;

	public static readonly Color SeaGreen;

	public static readonly Color SeaShell;

	public static readonly Color Sienna;

	public static readonly Color Silver;

	public static readonly Color SkyBlue;

	public static readonly Color SlateBlue;

	public static readonly Color SlateGray;

	public static readonly Color Snow;

	public static readonly Color SpringGreen;

	public static readonly Color SteelBlue;

	public static readonly Color Tan;

	public static readonly Color Teal;

	public static readonly Color Thistle;

	public static readonly Color Tomato;

	public static readonly Color Turquoise;

	public static readonly Color Violet;

	public static readonly Color Wheat;

	public static readonly Color White;

	public static readonly Color WhiteSmoke;

	public static readonly Color Yellow;

	public static readonly Color YellowGreen;

	public byte this[int index]
	{
		get
		{
			return index switch
			{
				0 => R, 
				1 => G, 
				2 => B, 
				3 => A, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Color run from 0 to 3, inclusive."), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				R = value;
				break;
			case 1:
				G = value;
				break;
			case 2:
				B = value;
				break;
			case 3:
				A = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Color run from 0 to 3, inclusive.");
			}
		}
	}

	public Color(byte value)
	{
		A = (R = (G = (B = value)));
	}

	public Color(float value)
	{
		A = (R = (G = (B = ToByte(value))));
	}

	public Color(byte red, byte green, byte blue, byte alpha)
	{
		R = red;
		G = green;
		B = blue;
		A = alpha;
	}

	public Color(byte red, byte green, byte blue)
	{
		R = red;
		G = green;
		B = blue;
		A = byte.MaxValue;
	}

	public Color(int red, int green, int blue, int alpha)
	{
		R = ToByte(red);
		G = ToByte(green);
		B = ToByte(blue);
		A = ToByte(alpha);
	}

	public Color(int red, int green, int blue)
	{
		this = new Color(red, green, blue, 255);
	}

	public Color(float red, float green, float blue, float alpha)
	{
		R = ToByte(red);
		G = ToByte(green);
		B = ToByte(blue);
		A = ToByte(alpha);
	}

	public Color(float red, float green, float blue)
	{
		R = ToByte(red);
		G = ToByte(green);
		B = ToByte(blue);
		A = byte.MaxValue;
	}

	public Color(Vector4 value)
	{
		R = ToByte(value.X);
		G = ToByte(value.Y);
		B = ToByte(value.Z);
		A = ToByte(value.W);
	}

	public Color(Vector3 value, float alpha)
	{
		R = ToByte(value.X);
		G = ToByte(value.Y);
		B = ToByte(value.Z);
		A = ToByte(alpha);
	}

	public Color(Vector3 value)
	{
		R = ToByte(value.X);
		G = ToByte(value.Y);
		B = ToByte(value.Z);
		A = byte.MaxValue;
	}

	public Color(uint rgba)
	{
		A = (byte)((rgba >> 24) & 0xFF);
		B = (byte)((rgba >> 16) & 0xFF);
		G = (byte)((rgba >> 8) & 0xFF);
		R = (byte)(rgba & 0xFF);
	}

	public Color(int rgba)
	{
		A = (byte)((rgba >> 24) & 0xFF);
		B = (byte)((rgba >> 16) & 0xFF);
		G = (byte)((rgba >> 8) & 0xFF);
		R = (byte)(rgba & 0xFF);
	}

	public Color(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Color.");
		}
		R = ToByte(values[0]);
		G = ToByte(values[1]);
		B = ToByte(values[2]);
		A = ToByte(values[3]);
	}

	public Color(byte[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Color.");
		}
		R = values[0];
		G = values[1];
		B = values[2];
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

	public int ToAbgr()
	{
		int a = A;
		a |= B << 8;
		a |= G << 16;
		return a | (R << 24);
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
		return new byte[4] { R, G, B, A };
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

	public static void Add(ref Color left, ref Color right, out Color result)
	{
		result.A = (byte)(left.A + right.A);
		result.R = (byte)(left.R + right.R);
		result.G = (byte)(left.G + right.G);
		result.B = (byte)(left.B + right.B);
	}

	public static Color Add(Color left, Color right)
	{
		return new Color(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
	}

	public static void Subtract(ref Color left, ref Color right, out Color result)
	{
		result.A = (byte)(left.A - right.A);
		result.R = (byte)(left.R - right.R);
		result.G = (byte)(left.G - right.G);
		result.B = (byte)(left.B - right.B);
	}

	public static Color Subtract(Color left, Color right)
	{
		return new Color(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
	}

	public static void Modulate(ref Color left, ref Color right, out Color result)
	{
		result.A = (byte)((float)(left.A * right.A) / 255f);
		result.R = (byte)((float)(left.R * right.R) / 255f);
		result.G = (byte)((float)(left.G * right.G) / 255f);
		result.B = (byte)((float)(left.B * right.B) / 255f);
	}

	public static Color Modulate(Color left, Color right)
	{
		return new Color(left.R * right.R, left.G * right.G, left.B * right.B, left.A * right.A);
	}

	public static void Scale(ref Color value, float scale, out Color result)
	{
		result.A = (byte)((float)(int)value.A * scale);
		result.R = (byte)((float)(int)value.R * scale);
		result.G = (byte)((float)(int)value.G * scale);
		result.B = (byte)((float)(int)value.B * scale);
	}

	public static Color Scale(Color value, float scale)
	{
		return new Color((byte)((float)(int)value.R * scale), (byte)((float)(int)value.G * scale), (byte)((float)(int)value.B * scale), (byte)((float)(int)value.A * scale));
	}

	public static void Negate(ref Color value, out Color result)
	{
		result.A = (byte)(255 - value.A);
		result.R = (byte)(255 - value.R);
		result.G = (byte)(255 - value.G);
		result.B = (byte)(255 - value.B);
	}

	public static Color Negate(Color value)
	{
		return new Color(255 - value.R, 255 - value.G, 255 - value.B, 255 - value.A);
	}

	public static void Clamp(ref Color value, ref Color min, ref Color max, out Color result)
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
		result = new Color(r, g, b, a);
	}

	public static void Premultiply(ref Color value, out Color result)
	{
		float num = (float)(int)value.A / 65025f;
		result.A = value.A;
		result.R = ToByte((float)(int)value.R * num);
		result.G = ToByte((float)(int)value.G * num);
		result.B = ToByte((float)(int)value.B * num);
	}

	public static Color Premultiply(Color value)
	{
		Premultiply(ref value, out var result);
		return result;
	}

	public static Color FromBgra(int color)
	{
		return new Color((byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)(color & 0xFF), (byte)((color >> 24) & 0xFF));
	}

	public static Color FromBgra(uint color)
	{
		return FromBgra((int)color);
	}

	public static Color FromAbgr(int color)
	{
		return new Color((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color);
	}

	public static Color FromAbgr(uint color)
	{
		return FromAbgr((int)color);
	}

	public static Color FromRgba(int color)
	{
		return new Color(color);
	}

	public static Color FromRgba(uint color)
	{
		return new Color(color);
	}

	public static Color Clamp(Color value, Color min, Color max)
	{
		Clamp(ref value, ref min, ref max, out var result);
		return result;
	}

	public static void Lerp(ref Color start, ref Color end, float amount, out Color result)
	{
		result.R = MathUtil.Lerp(start.R, end.R, amount);
		result.G = MathUtil.Lerp(start.G, end.G, amount);
		result.B = MathUtil.Lerp(start.B, end.B, amount);
		result.A = MathUtil.Lerp(start.A, end.A, amount);
	}

	public static Color Lerp(Color start, Color end, float amount)
	{
		Lerp(ref start, ref end, amount, out var result);
		return result;
	}

	public static void SmoothStep(ref Color start, ref Color end, float amount, out Color result)
	{
		amount = MathUtil.SmoothStep(amount);
		Lerp(ref start, ref end, amount, out result);
	}

	public static Color SmoothStep(Color start, Color end, float amount)
	{
		SmoothStep(ref start, ref end, amount, out var result);
		return result;
	}

	public static void Max(ref Color left, ref Color right, out Color result)
	{
		result.A = ((left.A > right.A) ? left.A : right.A);
		result.R = ((left.R > right.R) ? left.R : right.R);
		result.G = ((left.G > right.G) ? left.G : right.G);
		result.B = ((left.B > right.B) ? left.B : right.B);
	}

	public static Color Max(Color left, Color right)
	{
		Max(ref left, ref right, out var result);
		return result;
	}

	public static void Min(ref Color left, ref Color right, out Color result)
	{
		result.A = ((left.A < right.A) ? left.A : right.A);
		result.R = ((left.R < right.R) ? left.R : right.R);
		result.G = ((left.G < right.G) ? left.G : right.G);
		result.B = ((left.B < right.B) ? left.B : right.B);
	}

	public static Color Min(Color left, Color right)
	{
		Min(ref left, ref right, out var result);
		return result;
	}

	public static void AdjustContrast(ref Color value, float contrast, out Color result)
	{
		result.A = value.A;
		result.R = ToByte(0.5f + contrast * ((float)(int)value.R / 255f - 0.5f));
		result.G = ToByte(0.5f + contrast * ((float)(int)value.G / 255f - 0.5f));
		result.B = ToByte(0.5f + contrast * ((float)(int)value.B / 255f - 0.5f));
	}

	public static Color AdjustContrast(Color value, float contrast)
	{
		return new Color(ToByte(0.5f + contrast * ((float)(int)value.R / 255f - 0.5f)), ToByte(0.5f + contrast * ((float)(int)value.G / 255f - 0.5f)), ToByte(0.5f + contrast * ((float)(int)value.B / 255f - 0.5f)), value.A);
	}

	public static void AdjustSaturation(ref Color value, float saturation, out Color result)
	{
		float num = (float)(int)value.R / 255f * 0.2125f + (float)(int)value.G / 255f * 0.7154f + (float)(int)value.B / 255f * 0.0721f;
		result.A = value.A;
		result.R = ToByte(num + saturation * ((float)(int)value.R / 255f - num));
		result.G = ToByte(num + saturation * ((float)(int)value.G / 255f - num));
		result.B = ToByte(num + saturation * ((float)(int)value.B / 255f - num));
	}

	public static Color AdjustSaturation(Color value, float saturation)
	{
		float num = (float)(int)value.R / 255f * 0.2125f + (float)(int)value.G / 255f * 0.7154f + (float)(int)value.B / 255f * 0.0721f;
		return new Color(ToByte(num + saturation * ((float)(int)value.R / 255f - num)), ToByte(num + saturation * ((float)(int)value.G / 255f - num)), ToByte(num + saturation * ((float)(int)value.B / 255f - num)), value.A);
	}

	public static Color operator +(Color left, Color right)
	{
		return new Color(left.R + right.R, left.G + right.G, left.B + right.B, left.A + right.A);
	}

	public static Color operator +(Color value)
	{
		return value;
	}

	public static Color operator -(Color left, Color right)
	{
		return new Color(left.R - right.R, left.G - right.G, left.B - right.B, left.A - right.A);
	}

	public static Color operator -(Color value)
	{
		return new Color(-value.R, -value.G, -value.B, -value.A);
	}

	public static Color operator *(float scale, Color value)
	{
		return new Color((byte)((float)(int)value.R * scale), (byte)((float)(int)value.G * scale), (byte)((float)(int)value.B * scale), (byte)((float)(int)value.A * scale));
	}

	public static Color operator *(Color value, float scale)
	{
		return new Color((byte)((float)(int)value.R * scale), (byte)((float)(int)value.G * scale), (byte)((float)(int)value.B * scale), (byte)((float)(int)value.A * scale));
	}

	public static Color operator *(Color left, Color right)
	{
		return new Color((byte)((float)(left.R * right.R) / 255f), (byte)((float)(left.G * right.G) / 255f), (byte)((float)(left.B * right.B) / 255f), (byte)((float)(left.A * right.A) / 255f));
	}

	public static bool operator ==(Color left, Color right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Color left, Color right)
	{
		return !left.Equals(right);
	}

	public static explicit operator Color3(Color value)
	{
		return value.ToColor3();
	}

	public static explicit operator Vector3(Color value)
	{
		return new Vector3((float)(int)value.R / 255f, (float)(int)value.G / 255f, (float)(int)value.B / 255f);
	}

	public static explicit operator Vector4(Color value)
	{
		return new Vector4((float)(int)value.R / 255f, (float)(int)value.G / 255f, (float)(int)value.B / 255f, (float)(int)value.A / 255f);
	}

	public Color4 ToColor4()
	{
		return new Color4((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f, (float)(int)A / 255f);
	}

	public static implicit operator Color4(Color value)
	{
		return value.ToColor4();
	}

	public static explicit operator Color(Vector3 value)
	{
		return new Color(value.X, value.Y, value.Z, 1f);
	}

	public static explicit operator Color(Color3 value)
	{
		return new Color(value.Red, value.Green, value.Blue, 1f);
	}

	public static explicit operator Color(Vector4 value)
	{
		return new Color(value.X, value.Y, value.Z, value.W);
	}

	public static explicit operator Color(Color4 value)
	{
		return new Color(value.Red, value.Green, value.Blue, value.Alpha);
	}

	public static explicit operator int(Color value)
	{
		return value.ToRgba();
	}

	public static explicit operator Color(int value)
	{
		return new Color(value);
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
		int hashCode = R.GetHashCode();
		hashCode = (hashCode * 397) ^ G.GetHashCode();
		hashCode = (hashCode * 397) ^ B.GetHashCode();
		return (hashCode * 397) ^ A.GetHashCode();
	}

	public bool Equals(Color other)
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
		if (!object.ReferenceEquals(value.GetType(), typeof(Color)))
		{
			return false;
		}
		return Equals((Color)value);
	}

	private static byte ToByte(float component)
	{
		int value = (int)(component * 255f);
		return ToByte(value);
	}

	public static byte ToByte(int value)
	{
		return (byte)((value >= 0) ? ((value > 255) ? 255u : ((uint)value)) : 0u);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(ToRgba());
		}
		else
		{
			this = FromRgba(serializer.Reader.ReadInt32());
		}
	}

	static Color()
	{
		Zero = FromBgra(0);
		Transparent = FromBgra(0);
		AliceBlue = FromBgra(4293982463u);
		AntiqueWhite = FromBgra(4294634455u);
		Aqua = FromBgra(4278255615u);
		Aquamarine = FromBgra(4286578644u);
		Azure = FromBgra(4293984255u);
		Beige = FromBgra(4294309340u);
		Bisque = FromBgra(4294960324u);
		Black = FromBgra(4278190080u);
		BlanchedAlmond = FromBgra(4294962125u);
		Blue = FromBgra(4278190335u);
		BlueViolet = FromBgra(4287245282u);
		Brown = FromBgra(4289014314u);
		BurlyWood = FromBgra(4292786311u);
		CadetBlue = FromBgra(4284456608u);
		Chartreuse = FromBgra(4286578432u);
		Chocolate = FromBgra(4291979550u);
		Coral = FromBgra(4294934352u);
		CornflowerBlue = FromBgra(4284782061u);
		Cornsilk = FromBgra(4294965468u);
		Crimson = FromBgra(4292613180u);
		Cyan = FromBgra(4278255615u);
		DarkBlue = FromBgra(4278190219u);
		DarkCyan = FromBgra(4278225803u);
		DarkGoldenrod = FromBgra(4290283019u);
		DarkGray = FromBgra(4289309097u);
		DarkGreen = FromBgra(4278215680u);
		DarkKhaki = FromBgra(4290623339u);
		DarkMagenta = FromBgra(4287299723u);
		DarkOliveGreen = FromBgra(4283788079u);
		DarkOrange = FromBgra(4294937600u);
		DarkOrchid = FromBgra(4288230092u);
		DarkRed = FromBgra(4287299584u);
		DarkSalmon = FromBgra(4293498490u);
		DarkSeaGreen = FromBgra(4287609995u);
		DarkSlateBlue = FromBgra(4282924427u);
		DarkSlateGray = FromBgra(4281290575u);
		DarkTurquoise = FromBgra(4278243025u);
		DarkViolet = FromBgra(4287889619u);
		DeepPink = FromBgra(4294907027u);
		DeepSkyBlue = FromBgra(4278239231u);
		DimGray = FromBgra(4285098345u);
		DodgerBlue = FromBgra(4280193279u);
		Firebrick = FromBgra(4289864226u);
		FloralWhite = FromBgra(4294966000u);
		ForestGreen = FromBgra(4280453922u);
		Fuchsia = FromBgra(4294902015u);
		Gainsboro = FromBgra(4292664540u);
		GhostWhite = FromBgra(4294506751u);
		Gold = FromBgra(4294956800u);
		Goldenrod = FromBgra(4292519200u);
		Gray = FromBgra(4286611584u);
		Green = FromBgra(4278222848u);
		GreenYellow = FromBgra(4289593135u);
		Honeydew = FromBgra(4293984240u);
		HotPink = FromBgra(4294928820u);
		IndianRed = FromBgra(4291648604u);
		Indigo = FromBgra(4283105410u);
		Ivory = FromBgra(4294967280u);
		Khaki = FromBgra(4293977740u);
		Lavender = FromBgra(4293322490u);
		LavenderBlush = FromBgra(4294963445u);
		LawnGreen = FromBgra(4286381056u);
		LemonChiffon = FromBgra(4294965965u);
		LightBlue = FromBgra(4289583334u);
		LightCoral = FromBgra(4293951616u);
		LightCyan = FromBgra(4292935679u);
		LightGoldenrodYellow = FromBgra(4294638290u);
		LightGray = FromBgra(4292072403u);
		LightGreen = FromBgra(4287688336u);
		LightPink = FromBgra(4294948545u);
		LightSalmon = FromBgra(4294942842u);
		LightSeaGreen = FromBgra(4280332970u);
		LightSkyBlue = FromBgra(4287090426u);
		LightSlateGray = FromBgra(4286023833u);
		LightSteelBlue = FromBgra(4289774814u);
		LightYellow = FromBgra(4294967264u);
		Lime = FromBgra(4278255360u);
		LimeGreen = FromBgra(4281519410u);
		Linen = FromBgra(4294635750u);
		Magenta = FromBgra(4294902015u);
		Maroon = FromBgra(4286578688u);
		MediumAquamarine = FromBgra(4284927402u);
		MediumBlue = FromBgra(4278190285u);
		MediumOrchid = FromBgra(4290401747u);
		MediumPurple = FromBgra(4287852763u);
		MediumSeaGreen = FromBgra(4282168177u);
		MediumSlateBlue = FromBgra(4286277870u);
		MediumSpringGreen = FromBgra(4278254234u);
		MediumTurquoise = FromBgra(4282962380u);
		MediumVioletRed = FromBgra(4291237253u);
		MidnightBlue = FromBgra(4279834992u);
		MintCream = FromBgra(4294311930u);
		MistyRose = FromBgra(4294960353u);
		Moccasin = FromBgra(4294960309u);
		NavajoWhite = FromBgra(4294958765u);
		Navy = FromBgra(4278190208u);
		OldLace = FromBgra(4294833638u);
		Olive = FromBgra(4286611456u);
		OliveDrab = FromBgra(4285238819u);
		Orange = FromBgra(4294944000u);
		OrangeRed = FromBgra(4294919424u);
		Orchid = FromBgra(4292505814u);
		PaleGoldenrod = FromBgra(4293847210u);
		PaleGreen = FromBgra(4288215960u);
		PaleTurquoise = FromBgra(4289720046u);
		PaleVioletRed = FromBgra(4292571283u);
		PapayaWhip = FromBgra(4294963157u);
		PeachPuff = FromBgra(4294957753u);
		Peru = FromBgra(4291659071u);
		Pink = FromBgra(4294951115u);
		Plum = FromBgra(4292714717u);
		PowderBlue = FromBgra(4289781990u);
		Purple = FromBgra(4286578816u);
		Red = FromBgra(4294901760u);
		RosyBrown = FromBgra(4290547599u);
		RoyalBlue = FromBgra(4282477025u);
		SaddleBrown = FromBgra(4287317267u);
		Salmon = FromBgra(4294606962u);
		SandyBrown = FromBgra(4294222944u);
		SeaGreen = FromBgra(4281240407u);
		SeaShell = FromBgra(4294964718u);
		Sienna = FromBgra(4288696877u);
		Silver = FromBgra(4290822336u);
		SkyBlue = FromBgra(4287090411u);
		SlateBlue = FromBgra(4285160141u);
		SlateGray = FromBgra(4285563024u);
		Snow = FromBgra(4294966010u);
		SpringGreen = FromBgra(4278255487u);
		SteelBlue = FromBgra(4282811060u);
		Tan = FromBgra(4291998860u);
		Teal = FromBgra(4278222976u);
		Thistle = FromBgra(4292394968u);
		Tomato = FromBgra(4294927175u);
		Turquoise = FromBgra(4282441936u);
		Violet = FromBgra(4293821166u);
		Wheat = FromBgra(4294303411u);
		White = FromBgra(uint.MaxValue);
		WhiteSmoke = FromBgra(4294309365u);
		Yellow = FromBgra(4294967040u);
		YellowGreen = FromBgra(4288335154u);
	}
}
