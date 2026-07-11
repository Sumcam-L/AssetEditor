using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class StandardValuesEditor : ValueEditor
{
	private static readonly StandardValuesEditor s_instance = new StandardValuesEditor();

	public static StandardValuesEditor Instance => s_instance;

	public override bool UsesCustomContext => true;

	public override object GetCustomContext(PropertyNode node)
	{
		return (node == null) ? null : new StandardValuesEditorContext(node);
	}

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(PropertyGrid.StandardValuesEditorTemplateKey, container);
	}
}
