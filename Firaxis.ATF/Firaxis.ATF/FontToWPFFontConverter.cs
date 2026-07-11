using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace Firaxis.ATF;

internal static class FontToWPFFontConverter
{
	private static FontFamilyConverter FamilyConverter { get; } = new FontFamilyConverter();

	private static FontStyleConverter StyleConverter { get; } = new FontStyleConverter();

	public static double ConvertFontSize(Font font)
	{
		return (double)font.Size * 1.33;
	}

	public static System.Windows.Media.FontFamily ConvertFontFamily(Font font)
	{
		return (System.Windows.Media.FontFamily)FamilyConverter.ConvertFromString(font.FontFamily.Name);
	}

	public static FontWeight ConvertFontWeight(Font font)
	{
		if (font.Bold)
		{
			return FontWeights.Bold;
		}
		return FontWeights.Normal;
	}

	public static System.Windows.FontStyle ConvertFontStyle(Font font)
	{
		if (font.Italic)
		{
			return FontStyles.Italic;
		}
		return FontStyles.Normal;
	}
}
