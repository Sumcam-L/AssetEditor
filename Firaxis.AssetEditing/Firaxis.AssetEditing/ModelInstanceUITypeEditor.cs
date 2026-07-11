using System;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ModelInstanceUITypeEditor : EnumUITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		AttachmentPointAdapter attachmentPointAdapter = context.Instance as AttachmentPointAdapter;
		DefineEnum(attachmentPointAdapter.ModelInstanceNames.ToArray());
		return base.EditValue(context, provider, value);
	}
}
