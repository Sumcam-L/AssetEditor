using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class MultiLineTextValueEditor : ValueEditor
{
	public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(MultiLineTextValueEditor), "MultiLineTextValueEditorTemplate");

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(TemplateKey, container);
	}
}
