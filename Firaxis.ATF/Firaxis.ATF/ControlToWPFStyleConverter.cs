using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Firaxis.MVVMBase;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class ControlToWPFStyleConverter
{
	public Type TargetType { get; } = typeof(Control);

	public void ConvertStyle(SkinStyle style, ResourceDictionary targetDictionary)
	{
		if (style.TargetType != TargetType)
		{
			throw new ArgumentException("TargetType of the style to be converted does not match the TargetType of this converter.", "style");
		}
		if (targetDictionary == null)
		{
			throw new ArgumentNullException("targetDictionary");
		}
		Sce.Atf.Applications.Setter setter = style.Setters.FirstOrDefault((Sce.Atf.Applications.Setter x) => x.PropertyName.Equals("BackColor", StringComparison.CurrentCultureIgnoreCase));
		if (setter != null && setter.ValueInfo.Type == typeof(System.Drawing.Color))
		{
			System.Windows.Media.Brush value = ColorToWPFBrushConverter.Convert(setter.ValueInfo.Value);
			targetDictionary[System.Windows.SystemColors.ControlBrushKey] = value;
			targetDictionary[System.Windows.SystemColors.WindowBrushKey] = value;
		}
		Sce.Atf.Applications.Setter setter2 = style.Setters.FirstOrDefault((Sce.Atf.Applications.Setter x) => x.PropertyName.Equals("ForeColor", StringComparison.CurrentCultureIgnoreCase));
		if (setter2 != null && setter2.ValueInfo.Type == typeof(System.Drawing.Color))
		{
			System.Windows.Media.Brush value2 = ColorToWPFBrushConverter.Convert(setter2.ValueInfo.Value);
			targetDictionary[System.Windows.SystemColors.ControlTextBrushKey] = value2;
			targetDictionary[System.Windows.SystemColors.WindowTextBrushKey] = value2;
		}
		Sce.Atf.Applications.Setter setter3 = style.Setters.FirstOrDefault((Sce.Atf.Applications.Setter x) => x.PropertyName.Equals("Font", StringComparison.CurrentCultureIgnoreCase));
		if (setter3 != null && setter3.ValueInfo.Type == typeof(Font))
		{
			using (Font font = StyleToFontConverter.Convert(setter3.ValueInfo))
			{
				System.Windows.Media.FontFamily value3 = FontToWPFFontConverter.ConvertFontFamily(font);
				double num = FontToWPFFontConverter.ConvertFontSize(font);
				System.Windows.FontStyle fontStyle = FontToWPFFontConverter.ConvertFontStyle(font);
				FontWeight fontWeight = FontToWPFFontConverter.ConvertFontWeight(font);
				targetDictionary[WpfStyleProperties.ControlFontFamilyKey] = value3;
				targetDictionary[WpfStyleProperties.ControlFontSizeKey] = num;
				targetDictionary[WpfStyleProperties.ControlFontStyleKey] = fontStyle;
				targetDictionary[WpfStyleProperties.ControlFontWeightKey] = fontWeight;
				targetDictionary[WpfStyleProperties.WindowFontFamilyKey] = value3;
				targetDictionary[WpfStyleProperties.WindowFontSizeKey] = num;
				targetDictionary[WpfStyleProperties.WindowFontStyleKey] = fontStyle;
				targetDictionary[WpfStyleProperties.WindowFontWeightKey] = fontWeight;
			}
		}
	}
}
