using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Behaviors;

public class CS4Options
{
	public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.RegisterAttached("DisplayMode", typeof(bool), typeof(CS4Options), new PropertyMetadata(false, DisplayModePropertyChanged));

	public static readonly DependencyProperty UseLayoutRoundingProperty = DependencyProperty.RegisterAttached("UseLayoutRounding", typeof(bool), typeof(CS4Options), new PropertyMetadata(false, UseLayoutRoundingPropertyChanged));

	public static void SetDisplayMode(UIElement element, bool value)
	{
		element.SetValue(DisplayModeProperty, value);
	}

	public static bool GetDisplayMode(UIElement element)
	{
		return (bool)element.GetValue(DisplayModeProperty);
	}

	private static void DisplayModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TextOptions.SetTextFormattingMode(d, TextFormattingMode.Display);
	}

	public static void SetUseLayoutRounding(UIElement element, bool value)
	{
		element.SetValue(UseLayoutRoundingProperty, value);
	}

	public static bool GetUseLayoutRounding(UIElement element)
	{
		return (bool)element.GetValue(UseLayoutRoundingProperty);
	}

	private static void UseLayoutRoundingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.SetValue(FrameworkElement.UseLayoutRoundingProperty, e.NewValue);
	}
}
