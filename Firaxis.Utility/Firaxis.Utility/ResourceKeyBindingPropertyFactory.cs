using System;
using System.Windows;

namespace Firaxis.Utility;

public static class ResourceKeyBindingPropertyFactory
{
	public static DependencyProperty CreateResourceKeyBindingProperty(DependencyProperty boundProperty, Type ownerClass)
	{
		return DependencyProperty.RegisterAttached(boundProperty.Name + "ResourceKeyBinding", typeof(object), ownerClass, new PropertyMetadata(null, delegate(DependencyObject dp, DependencyPropertyChangedEventArgs e)
		{
			if (dp is FrameworkElement frameworkElement)
			{
				frameworkElement.SetResourceReference(boundProperty, e.NewValue);
			}
		}));
	}
}
