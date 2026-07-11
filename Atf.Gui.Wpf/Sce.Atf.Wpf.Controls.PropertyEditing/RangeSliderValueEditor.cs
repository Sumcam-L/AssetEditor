using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class RangeSliderValueEditor : ValueEditor
{
	public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(RangeSliderValueEditor), "RangeSliderValueEditorTemplate");

	private static RangeSliderValueEditor s_instance = new RangeSliderValueEditor();

	public static RangeSliderValueEditor Instance => s_instance;

	public override bool UsesCustomContext => true;

	public override object GetCustomContext(PropertyNode node)
	{
		return (node != null) ? new RangeSliderValueEditorContext(node) : null;
	}

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(TemplateKey, container);
	}
}
