using System;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class FolderUriEditor : FolderNameEditor, IAnnotatedParams
{
	public void Initialize(string[] parameters)
	{
	}

	protected override void InitializeDialog(FolderBrowser dialog)
	{
		base.InitializeDialog(dialog);
		dialog.StartLocation = FolderBrowserFolder.MyComputer;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		object obj = base.EditValue(context, provider, value);
		if (value is Uri)
		{
			return new Uri(obj.ToString());
		}
		return obj?.ToString();
	}
}
