using System;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class SourceObjectUITypeEditor : EnumUITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		ISourceObjectAdapter sourceObjectAdapter = context.Instance.As<ISourceObjectAdapter>();
		DefineEnum(sourceObjectAdapter.GetCurrentSourceObjects());
		return base.EditValue(context, provider, value);
	}
}
