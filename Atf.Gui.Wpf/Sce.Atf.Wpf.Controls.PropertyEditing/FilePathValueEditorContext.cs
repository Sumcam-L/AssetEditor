using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class FilePathValueEditorContext : NotifyPropertyChangedBase
{
	private readonly PropertyNode m_node;

	public string Filter { get; set; }

	public string DefaultExtension { get; set; }

	public object Value
	{
		get
		{
			return m_node.Value;
		}
		set
		{
			m_node.Value = value;
		}
	}

	public FilePathValueEditorContext(PropertyNode node, FilePathValueEditor editor)
	{
		m_node = node;
		Filter = editor.Filter;
		DefaultExtension = editor.DefaultExtension;
	}
}
