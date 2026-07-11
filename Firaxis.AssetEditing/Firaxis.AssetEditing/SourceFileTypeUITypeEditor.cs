using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using Firaxis.ContentExporters;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class SourceFileTypeUITypeEditor : UITypeEditor
{
	private IFileDialogService m_fileDialogService;

	public SourceFileTypeUITypeEditor(IFileDialogService fileDialogService)
	{
		m_fileDialogService = fileDialogService;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		string pathName = "";
		if (value is string)
		{
			pathName = (string)value;
		}
		DomNode domNode = context.Instance.As<DomNode>();
		ImportedEntityAdapter importedEntityAdapter = ((domNode.Parent == null) ? domNode.As<ImportedEntityAdapter>() : domNode.Parent.As<ImportedEntityAdapter>());
		List<string> list = null;
		if (importedEntityAdapter != null)
		{
			list = new List<string>(ExporterService.GetSupportedSourceFileExtensions(importedEntityAdapter.InstanceType));
		}
		IEnumerable<string> enumerable = ((list != null && list.Any()) ? list : new List<string>(new string[1] { ".*" }));
		StringBuilder stringBuilder = new StringBuilder("Source Files (");
		foreach (string item in enumerable)
		{
			stringBuilder.Append("*" + item + ";");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		stringBuilder.Append(")|");
		foreach (string item2 in enumerable)
		{
			stringBuilder.Append("*" + item2 + ";");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		m_fileDialogService.ForcedInitialDirectory = null;
		if (m_fileDialogService.OpenFileName(ref pathName, stringBuilder.ToString()) == FileDialogResult.OK)
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
