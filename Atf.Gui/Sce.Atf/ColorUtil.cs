using System;
using System.Drawing;

namespace Sce.Atf;

public static class ColorUtil
{
	public static Color GetShade(Color color, float amount)
	{
		amount = Math.Max(0f, amount);
		int red = (int)Math.Min(255f, (float)(int)color.R * amount);
		int green = (int)Math.Min(255f, (float)(int)color.G * amount);
		int blue = (int)Math.Min(255f, (float)(int)color.B * amount);
		return Color.FromArgb(color.A, red, green, blue);
	}

	public static Color Lerp(Color c0, Color c1, float amount)
	{
		int red = (int)((float)(int)c0.R + (float)(c1.R - c0.R) * amount);
		int green = (int)((float)(int)c0.G + (float)(c1.G - c0.G) * amount);
		int blue = (int)((float)(int)c0.B + (float)(c1.B - c0.B) * amount);
		int alpha = (int)((float)(int)c0.A + (float)(c1.A - c0.A) * amount);
		return Color.FromArgb(alpha, red, green, blue);
	}

	public static Color FromAhsb(int a, float h, float s, float b)
	{
		if (Math.Abs(s) < float.Epsilon)
		{
			int num = Convert.ToInt32(b * 255f);
			return Color.FromArgb(a, num, num, num);
		}
		float num2;
		float num3;
		if (0.5 < (double)b)
		{
			num2 = b - b * s + s;
			num3 = b + b * s - s;
		}
		else
		{
			num2 = b + b * s;
			num3 = b - b * s;
		}
		int num4 = (int)Math.Floor(h / 60f);
		if (300f <= h)
		{
			h -= 360f;
		}
		h /= 60f;
		h -= 2f * (float)Math.Floor(((float)num4 + 1f) % 6f / 2f);
		float num5 = ((num4 % 2 != 0) ? (num3 - h * (num2 - num3)) : (h * (num2 - num3) + num3));
		int num6 = Convert.ToInt32(num2 * 255f);
		int num7 = Convert.ToInt32(num5 * 255f);
		int num8 = Convert.ToInt32(num3 * 255f);
		return num4 switch
		{
			1 => Color.FromArgb(a, num7, num6, num8), 
			2 => Color.FromArgb(a, num8, num6, num7), 
			3 => Color.FromArgb(a, num8, num7, num6), 
			4 => Color.FromArgb(a, num7, num8, num6), 
			5 => Color.FromArgb(a, num6, num8, num7), 
			_ => Color.FromArgb(a, num6, num7, num8), 
		};
	}
}
