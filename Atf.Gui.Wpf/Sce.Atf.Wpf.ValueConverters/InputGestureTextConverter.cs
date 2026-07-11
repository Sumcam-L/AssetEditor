using System;
using System.Collections;
using System.Globalization;
using System.Windows.Input;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Interop;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class InputGestureTextConverter : ConverterMarkupExtension<InputGestureTextConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is KeyGesture keyGesture)
		{
			return keyGesture.GetDisplayStringForCulture(culture);
		}
		if (value is Keys)
		{
			return GetDisplayStringForKeys((Keys)value, culture);
		}
		if (!(value is IList { Count: >=1 } list))
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is KeyGesture keyGesture2)
			{
				return keyGesture2.GetDisplayStringForCulture(culture);
			}
			if (list[i] is Keys)
			{
				return GetDisplayStringForKeys((Keys)list[i], culture);
			}
		}
		return null;
	}

	private static string GetDisplayStringForKeys(Keys value, CultureInfo culture)
	{
		return ToWpfKeyGesture(value).GetDisplayStringForCulture(culture);
	}

	private static KeyGesture ToWpfKeyGesture(Keys atfKeys)
	{
		ModifierKeys modifiers = Sce.Atf.Wpf.Interop.KeysInterop.ToWpfModifiers(atfKeys);
		Key key = Sce.Atf.Wpf.Interop.KeysInterop.ToWpf(atfKeys);
		return new KeyGesture(key, modifiers);
	}
}
