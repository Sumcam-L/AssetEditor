using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class FolderBrowserDialogUITypeEditor : UITypeEditor
{
	private readonly FolderBrowserDialog m_dialog;

	public string Description
	{
		get
		{
			return m_dialog.Description;
		}
		set
		{
			m_dialog.Description = value;
		}
	}

	public FolderBrowserDialogUITypeEditor()
		: this(string.Empty)
	{
	}

	public FolderBrowserDialogUITypeEditor(string description)
	{
		m_dialog = new FolderBrowserDialog();
		m_dialog.Description = description;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider sp, object value)
	{
		IWindowsFormsEditorService windowsFormsEditorService = (IWindowsFormsEditorService)sp.GetService(typeof(IWindowsFormsEditorService));
		if (windowsFormsEditorService != null)
		{
			if (value != null)
			{
				m_dialog.SelectedPath = value.ToString();
			}
			if (m_dialog.ShowDialog() == DialogResult.OK)
			{
				value = m_dialog.SelectedPath;
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}
}
