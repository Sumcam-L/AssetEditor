using System;
using System.ComponentModel;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ArtDefFXTriggerCollectionTypeEditor : EnumUITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		ArtDefFXTriggerAdapter artDefFXTriggerAdapter = context.Instance.As<ArtDefFXTriggerAdapter>();
		if (artDefFXTriggerAdapter != null)
		{
			string[] names = (from selObj in artDefFXTriggerAdapter.EntityAdapter.As<AssetAdapter>().AssetClass.ArtDefReferences
				where selObj.Tags.Contains("VFX", StringComparer.InvariantCultureIgnoreCase)
				select selObj.CollectionName).ToArray();
			DefineEnum(names);
		}
		return base.EditValue(context, provider, value);
	}
}
