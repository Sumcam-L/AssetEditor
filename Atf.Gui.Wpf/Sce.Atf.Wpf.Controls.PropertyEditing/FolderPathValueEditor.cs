using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class FolderPathValueEditor : ValueEditor
{
	public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(FilePathValueEditor), "FolderPathValueEditorTemplate");

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(TemplateKey, container);
	}
}
