using System.Windows.Media;

namespace Firaxis.ATF;

internal static class ColorToWPFBrushConverter
{
	public static Brush Convert(string value)
	{
		return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));
	}
}
