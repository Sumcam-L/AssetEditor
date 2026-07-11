using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Behaviors;

public class RoutedDataTemplateSelector : DataTemplateSelector
{
	public static readonly RoutedEvent TemplateRequestedEvent = EventManager.RegisterRoutedEvent("TemplateRequested", RoutingStrategy.Bubble, typeof(TemplateRequestedEventHandler), typeof(RoutedDataTemplateSelector));

	[EditorBrowsable(EditorBrowsableState.Never)]
	public event TemplateRequestedEventHandler TemplateRequested
	{
		add
		{
			throw new InvalidOperationException("Do not directly hook the TemplateRequested event.");
		}
		remove
		{
			throw new InvalidOperationException("Do not directly unhook the TemplateRequested event.");
		}
	}

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		if (!(container is UIElement uIElement))
		{
			throw new ArgumentException("RoutedDataTemplateSelector only works with UIElements.");
		}
		TemplateRequestedEventArgs e = new TemplateRequestedEventArgs(TemplateRequestedEvent, uIElement, item);
		uIElement.RaiseEvent(e);
		return e.TemplateToUse;
	}
}
