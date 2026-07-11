using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class SliderValueEditor : ValueEditor
{
	public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(SliderValueEditor), "SliderValueEditorTemplate");

	private static SliderValueEditor s_instance = new SliderValueEditor();

	public static SliderValueEditor Instance => s_instance;

	public override bool UsesCustomContext => true;

	public override object GetCustomContext(PropertyNode node)
	{
		return (node != null) ? new SliderValueEditorContext(node) : null;
	}

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(TemplateKey, container);
	}
}
