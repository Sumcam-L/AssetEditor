using System;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class BoneUITypeEditor : EnumUITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		AttachmentPointAdapter attachmentPointAdapter = context.Instance as AttachmentPointAdapter;
		DefineEnum(attachmentPointAdapter.CurrentBoneNames.ToArray());
		return base.EditValue(context, provider, value);
	}
}
