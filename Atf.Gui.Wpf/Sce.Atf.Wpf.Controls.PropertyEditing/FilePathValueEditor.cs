using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class FilePathValueEditor : ValueEditor
{
	public static readonly ResourceKey TemplateKey = new ComponentResourceKey(typeof(FilePathValueEditor), "FilePathValueEditorTemplate");

	public override bool UsesCustomContext => true;

	public string Filter { get; set; }

	public string DefaultExtension { get; set; }

	public override object GetCustomContext(PropertyNode node)
	{
		return (node != null) ? new FilePathValueEditorContext(node, this) : null;
	}

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(TemplateKey, container);
	}
}
