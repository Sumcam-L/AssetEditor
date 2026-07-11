using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Firaxis.MVVMBase;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public static class SkinToWpfSkinConverter
{
	private static IDictionary<string, object> PropertyToResourceMap { get; }

	static SkinToWpfSkinConverter()
	{
		PropertyToResourceMap = new Dictionary<string, object>();
		PropertyToResourceMap.Add("ControlBackColor", SystemColors.ControlBrushKey);
		PropertyToResourceMap.Add("ControlDarkBackColor", SystemColors.ControlDarkBrushKey);
		PropertyToResourceMap.Add("ControlDarkDarkBackColor", SystemColors.ControlDarkDarkBrushKey);
		PropertyToResourceMap.Add("ControlLightBackColor", SystemColors.ControlLightBrushKey);
		PropertyToResourceMap.Add("ControlLightLightBackColor", SystemColors.ControlLightLightBrushKey);
		PropertyToResourceMap.Add("ControlTextColor", SystemColors.ControlTextBrushKey);
		PropertyToResourceMap.Add("InfoTextColor", SystemColors.InfoTextBrushKey);
		PropertyToResourceMap.Add("GrayTextBrush", SystemColors.GrayTextBrushKey);
		PropertyToResourceMap.Add("MenuTextBrush", SystemColors.MenuTextBrushKey);
		PropertyToResourceMap.Add("HighlightBrush", SystemColors.HighlightBrushKey);
		PropertyToResourceMap.Add("HighlightTextBrush", SystemColors.HighlightTextBrushKey);
		PropertyToResourceMap.Add("WindorBackColor", SystemColors.WindowBrushKey);
		PropertyToResourceMap.Add("WindowTextColor", SystemColors.WindowTextBrushKey);
		PropertyToResourceMap.Add("ScrollBarColor", SystemColors.ScrollBarBrushKey);
	}

	public static void ConvertFrom(ISkin atfSkin, ResourceDictionary targetResources)
	{
		if (atfSkin == null || targetResources == null)
		{
			return;
		}
		SkinStyle skinStyle = atfSkin.Styles.FirstOrDefault((SkinStyle x) => x.TargetType == typeof(Control));
		if (skinStyle != null)
		{
			new ControlToWPFStyleConverter().ConvertStyle(skinStyle, targetResources);
		}
		SkinStyle skinStyle2 = atfSkin.Styles.FirstOrDefault((SkinStyle x) => x.TargetType == typeof(SkinStub));
		if (skinStyle2 == null)
		{
			return;
		}
		foreach (Sce.Atf.Applications.Setter setter in skinStyle2.Setters)
		{
			object value = null;
			if (PropertyToResourceMap.TryGetValue(setter.PropertyName, out value))
			{
				Brush value2 = ColorToWPFBrushConverter.Convert(setter.ValueInfo.Value);
				targetResources[value] = value2;
			}
		}
	}
}
