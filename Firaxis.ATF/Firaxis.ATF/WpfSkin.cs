using System;
using System.Collections.Generic;
using System.Windows;
using Sce.Atf;

namespace Firaxis.ATF;

public class WpfSkin
{
	private IDictionary<Type, WpfSkinStyle> TypeStyleDictionary { get; set; } = new Dictionary<Type, WpfSkinStyle>();

	private IDictionary<WeakKey<DependencyObject>, ICollection<IWpfSkinStyleSetter>> PreviousValuesMap { get; set; } = new Dictionary<WeakKey<DependencyObject>, ICollection<IWpfSkinStyleSetter>>();

	public ICollection<WpfSkinStyle> SkinStyles { get; private set; } = new List<WpfSkinStyle>();

	public void ApplySkin(DependencyObject target)
	{
		WpfSkinStyle wpfSkinStyle = FindBestSkinStyle(target.GetType());
		if (wpfSkinStyle == null)
		{
			return;
		}
		WeakKey<DependencyObject> key = new WeakKey<DependencyObject>(target);
		List<IWpfSkinStyleSetter> list = new List<IWpfSkinStyleSetter>();
		foreach (IWpfSkinStyleSetter setter in wpfSkinStyle.Setters)
		{
			DependencyProperty targetProperty = setter.GetTargetProperty();
			list.Add(new WpfSkinStyleSetter(targetProperty, target.GetValue(targetProperty)));
			setter.ApplyStyle(target);
		}
		PreviousValuesMap[key] = list;
	}

	public void RemoveSkin(DependencyObject target)
	{
		WeakKey<DependencyObject> key = new WeakKey<DependencyObject>(target);
		if (!PreviousValuesMap.TryGetValue(key, out var value))
		{
			return;
		}
		foreach (IWpfSkinStyleSetter item in value)
		{
			item.ApplyStyle(target);
		}
		PreviousValuesMap.Remove(key);
	}

	private WpfSkinStyle FindBestSkinStyle(Type targetType)
	{
		WpfSkinStyle value = null;
		if (!TypeStyleDictionary.TryGetValue(targetType, out value))
		{
			WpfSkinStyle wpfSkinStyle = null;
			foreach (WpfSkinStyle skinStyle in SkinStyles)
			{
				if (skinStyle.TargetType == targetType)
				{
					wpfSkinStyle = skinStyle;
					break;
				}
				if (targetType.IsSubclassOf(skinStyle.TargetType))
				{
					wpfSkinStyle = skinStyle;
				}
			}
			value = wpfSkinStyle;
		}
		return value;
	}
}
