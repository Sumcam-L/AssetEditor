using System.Windows;

namespace Sce.Atf.Wpf.Behaviors;

public class TemplateRequestedEventArgs : RoutedEventArgs
{
	private readonly object m_dataObject;

	public object DataObject => m_dataObject;

	public DataTemplate TemplateToUse { get; set; }

	public UIElement TemplatedElement => base.OriginalSource as UIElement;

	public TemplateRequestedEventArgs(RoutedEvent routedEvent, UIElement templatedElement, object dataObject)
		: base(routedEvent, templatedElement)
	{
		m_dataObject = dataObject;
	}
}
