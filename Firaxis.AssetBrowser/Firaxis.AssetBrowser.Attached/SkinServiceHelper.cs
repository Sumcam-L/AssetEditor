using System.Windows;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.Attached;

public class SkinServiceHelper
{
	public static readonly DependencyProperty ApplySkinProperty = DependencyProperty.RegisterAttached("ApplySkin", typeof(bool), typeof(SkinServiceHelper), new PropertyMetadata(false, ApplySkinChanged));

	public static bool GetApplySkin(FrameworkElement target)
	{
		return (bool)target.GetValue(ApplySkinProperty);
	}

	public static void SetApplySkin(FrameworkElement target, bool value)
	{
		target.SetValue(ApplySkinProperty, value);
	}

	private static void ApplySkinChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is FrameworkElement target)
		{
			if (GetApplySkin(target))
			{
				Context.GetService<IWpfSkinService>()?.ApplySkin(target);
			}
			else
			{
				Context.GetService<IWpfSkinService>()?.RemoveSkin(target);
			}
		}
	}
}
