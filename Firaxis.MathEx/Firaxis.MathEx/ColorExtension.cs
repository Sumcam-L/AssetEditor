using System;
using System.Drawing;

namespace Firaxis.MathEx;

public static class ColorExtension
{
	private static Vec3 Luminance = new Vec3(0.3f, 0.59f, 0.11f);

	public static readonly int kHue = 0;

	public static readonly int kSaturation = 1;

	public static readonly int kVibrance = 2;

	public static Color Desaturate(this Color color)
	{
		return color.Desaturate(0f);
	}

	public static Color Desaturate(this Color color, float s)
	{
		Vec3 vec = new Vec3((float)(int)color.R / 255f, (float)(int)color.G / 255f, (float)(int)color.B / 255f);
		float num = Luminance.Dot(vec);
		Vec3 v = new Vec3(num, num, num);
		Vec3 vec2 = Vec3.Lerp(v, vec, s);
		return Color.FromArgb(color.A, (int)(vec2.X * 255f), (int)(vec2.Y * 255f), (int)(vec2.Z * 255f));
	}

	public static Color Blend(this Color A, Color B, float blendFactor)
	{
		float num = 1f - blendFactor;
		int red = MathExtension.Clamp((int)((float)(int)A.R * num + (float)(int)B.R * blendFactor), 0, 255);
		int green = MathExtension.Clamp((int)((float)(int)A.G * num + (float)(int)B.G * blendFactor), 0, 255);
		int blue = MathExtension.Clamp((int)((float)(int)A.B * num + (float)(int)B.B * blendFactor), 0, 255);
		int alpha = MathExtension.Clamp((int)((float)(int)A.A * num + (float)(int)B.A * blendFactor), 0, 255);
		return Color.FromArgb(alpha, red, green, blue);
	}

	public static Color Blend(this Color middle, Color left, Color right, float blendFactor)
	{
		if (blendFactor < 0f)
		{
			return left.Blend(middle, blendFactor + 1f);
		}
		if (blendFactor > 0f)
		{
			return middle.Blend(right, blendFactor);
		}
		return middle;
	}

	public static Color Colorize(this Color baseColor, Color addedHue, float saturation, float lightness)
	{
		return baseColor.Colorize(addedHue.ToHSV()[kHue], saturation, lightness);
	}

	public static Color Colorize(this Color color, float hue, float saturation, float lightness)
	{
		if (lightness <= -1f)
		{
			return Color.Black;
		}
		if (lightness >= 1f)
		{
			return Color.White;
		}
		hue = MathExtension.Clamp(hue, 0f, 359.9999f);
		saturation = MathExtension.Clamp(saturation, 0f, 1f);
		Vec3 hsv = new Vec3(hue, 1f, 1f);
		Vec3 vec = color.ToHSV();
		Color middle = color.Blend(hsv.ToColor(), saturation);
		if (lightness >= 0f)
		{
			return middle.Blend(Color.Black, Color.White, 2f * (1f - lightness) * (vec[kVibrance] - 1f) + 1f);
		}
		return middle.Blend(Color.Black, Color.White, 2f * (1f + lightness) * vec[kVibrance] - 1f);
	}

	public static Color ToColor(this Vec3 hsv)
	{
		float num = hsv[kHue];
		float num2 = hsv[kSaturation];
		float num3 = hsv[kVibrance];
		if (num < 0f || num >= 360f)
		{
			return Color.FromArgb(0, 0, 0);
		}
		if (num2 < 0f || num2 > 1f)
		{
			return Color.FromArgb(0, 0, 0);
		}
		if (num3 < 0f || num3 > 1f)
		{
			return Color.FromArgb(0, 0, 0);
		}
		float num4 = num / 60f;
		float num5 = (float)Math.Floor(num4);
		float num6 = num4 - num5;
		float num7 = (1f - num2) * num3;
		float num8 = (1f - num2 * num6) * num3;
		float num9 = (1f - num2 * (1f - num6)) * num3;
		int red = 0;
		int green = 0;
		int blue = 0;
		if (num5 == 0f)
		{
			red = (int)(num3 * 255f);
			green = (int)(num9 * 255f);
			blue = (int)(num7 * 255f);
		}
		else if (num5 == 1f)
		{
			red = (int)(num8 * 255f);
			green = (int)(num3 * 255f);
			blue = (int)(num7 * 255f);
		}
		else if (num5 == 2f)
		{
			red = (int)(num7 * 255f);
			green = (int)(num3 * 255f);
			blue = (int)(num9 * 255f);
		}
		else if (num5 == 3f)
		{
			red = (int)(num7 * 255f);
			green = (int)(num8 * 255f);
			blue = (int)(num3 * 255f);
		}
		else if (num5 == 4f)
		{
			red = (int)(num9 * 255f);
			green = (int)(num7 * 255f);
			blue = (int)(num3 * 255f);
		}
		else if (num5 == 5f)
		{
			red = (int)(num3 * 255f);
			green = (int)(num7 * 255f);
			blue = (int)(num8 * 255f);
		}
		return Color.FromArgb(red, green, blue);
	}

	public static Vec3 ToHSV(this Color color)
	{
		float num = (float)(int)color.R / 255f;
		float num2 = (float)(int)color.G / 255f;
		float num3 = (float)(int)color.B / 255f;
		float num4 = Math.Min(Math.Min(num, num2), num3);
		float num5 = Math.Max(Math.Max(num, num2), num3);
		if (num5 == 0f)
		{
			return new Vec3(0f, 0f, 0f);
		}
		float num6 = num5 - num4;
		float num7 = 0f;
		float z = num5;
		float y = num6 / num5;
		if (num6 == 0f)
		{
			return new Vec3(0f, y, z);
		}
		if (num == num5)
		{
			num7 = (num2 - num3) / num6 % 6f * 60f;
		}
		else if (num2 == num5)
		{
			num7 = ((num3 - num) / num6 + 2f) * 60f;
		}
		else if (num3 == num5)
		{
			num7 = ((num - num2) / num6 + 4f) * 60f;
		}
		if (num7 < 0f)
		{
			num7 += 360f;
		}
		return new Vec3(num7, y, z);
	}
}
