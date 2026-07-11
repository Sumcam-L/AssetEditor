using System;
using System.Windows;

namespace Firaxis.MVVMBase.Helpers;

public class WindowFactoryRegistration : DependencyObject
{
	public static readonly DependencyProperty ViewTypeProperty = DependencyProperty.Register("ViewType", typeof(Type), typeof(WindowFactoryRegistration), new FrameworkPropertyMetadata(null, null, CoerceViewType));

	public Type ViewType
	{
		get
		{
			return (Type)GetValue(ViewTypeProperty);
		}
		set
		{
			SetValue(ViewTypeProperty, value);
		}
	}

	private static object CoerceViewType(DependencyObject d, object basevalue)
	{
		if (!(d is WindowFactoryRegistration) || basevalue == null)
		{
			return basevalue;
		}
		Type type = basevalue as Type;
		return (type != null && typeof(Window).IsAssignableFrom(type)) ? basevalue : null;
	}
}
