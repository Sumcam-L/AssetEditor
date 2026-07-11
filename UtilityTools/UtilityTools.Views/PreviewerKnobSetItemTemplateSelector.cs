using System.Windows;
using System.Windows.Controls;

namespace UtilityTools.Views;

internal class PreviewerKnobSetItemTemplateSelector : DataTemplateSelector
{
	public DataTemplate ButtonKnobContentTemplate { get; set; }

	public DataTemplate ParameterKnobContentTemplate { get; set; }

	public DataTemplate KnobGroupContentTemplate { get; set; }

	public DataTemplate StringListPreviewerKnobContentTemplate { get; set; }

	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		return ButtonKnobContentTemplate;
	}
}
