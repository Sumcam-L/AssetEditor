using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.Asset;

public class AttachmentPointNameEditor : UITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		IAttachmentPointNameProvider attachmentPointNameProvider = context.GetService<IAttachmentPointNameProvider>();
		if (attachmentPointNameProvider == null)
		{
			attachmentPointNameProvider = (context.Instance as DomNode)?.GetRoot().As<IAttachmentPointNameProvider>();
		}
		if (attachmentPointNameProvider == null)
		{
			attachmentPointNameProvider = (context.Instance as DomNodeAdapter)?.DomNode?.GetRoot().As<IAttachmentPointNameProvider>();
		}
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			using AttachmentEditorControl attachmentEditorControl = new AttachmentEditorControl(attachmentPointNameProvider, context);
			windowsFormsEditorService.DropDownControl(attachmentEditorControl);
			if (attachmentEditorControl.UserPressedOK)
			{
				value = attachmentEditorControl.SelectedAttachmentPoint;
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}
}
