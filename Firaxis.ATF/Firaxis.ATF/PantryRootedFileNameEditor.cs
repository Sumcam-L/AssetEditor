using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class PantryRootedFileNameEditor : UITypeEditor
{
	private readonly IEnumerable<IDocumentClient> m_documentClients;

	private readonly IDocumentService m_fileCommands;

	private readonly IFileDialogService m_fileDialogService;

	public string Filter { get; set; }

	public string Root { get; set; }

	public PantryRootedFileNameEditor(IFileDialogService fileDialogService, IDocumentService fileCommands, IEnumerable<IDocumentClient> documentClients, string root)
	{
		m_fileDialogService = fileDialogService;
		m_fileCommands = fileCommands;
		m_documentClients = documentClients;
		Root = root;
		if (!Root.EndsWith("\\"))
		{
			Root += "\\";
		}
	}

	public PantryRootedFileNameEditor(IFileDialogService fileDialogService, IDocumentService fileCommands, IEnumerable<IDocumentClient> documentClients, string root, string filter)
		: this(fileDialogService, fileCommands, documentClients, root)
	{
		Filter = filter;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			PantryRootedFileNameLauncher pantryRootedFileNameLauncher = new PantryRootedFileNameLauncher(windowsFormsEditorService, context, value, m_fileDialogService, m_fileCommands, m_documentClients, Root, Filter);
			windowsFormsEditorService.DropDownControl(pantryRootedFileNameLauncher);
			if (pantryRootedFileNameLauncher.UserPressedOK)
			{
				value = pantryRootedFileNameLauncher.SelectedData.Replace(Root, "");
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}
}
