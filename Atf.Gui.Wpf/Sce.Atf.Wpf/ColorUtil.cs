using System;
using System.Windows.Media;

namespace Sce.Atf.Wpf;

public class ColorUtil
{
	public static Color GetShade(Color color, float amount, byte alpha)
	{
		amount = Math.Max(0f, amount);
		byte r = (byte)Math.Min(255f, (float)(int)color.R * amount);
		byte g = (byte)Math.Min(255f, (float)(int)color.G * amount);
		byte b = (byte)Math.Min(255f, (float)(int)color.B * amount);
		return Color.FromArgb(alpha, r, g, b);
	}

	public static Color ConvertFromString(string color)
	{
		object obj = ColorConverter.ConvertFromString(color);
		if (obj != null)
		{
			return (Color)obj;
		}
		return Colors.White;
	}
}
