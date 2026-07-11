using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public class FileServiceNameEditor : UITypeEditor
{
	private IFileDialogService m_fileDialogService;

	public string Filter { get; set; }

	[ImportingConstructor]
	public FileServiceNameEditor(IFileDialogService fileDialogService)
	{
		m_fileDialogService = fileDialogService;
		Filter = "";
	}

	public FileServiceNameEditor(IFileDialogService fileDialogService, string filter)
	{
		m_fileDialogService = fileDialogService;
		Filter = filter;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		string pathName = "";
		if (value is string)
		{
			pathName = (string)value;
		}
		m_fileDialogService.ForcedInitialDirectory = "";
		if (m_fileDialogService.OpenFileName(ref pathName, Filter) == FileDialogResult.OK)
		{
			value = pathName;
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}
}
