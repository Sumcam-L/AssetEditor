using System;
using System.ComponentModel;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class ArtDefFXTriggerElementTypeEditor : EnumUITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		ArtDefFXTriggerAdapter trigger = context.Instance.As<ArtDefFXTriggerAdapter>();
		if (trigger != null && !string.IsNullOrEmpty(trigger.VFXCollection))
		{
			string text = (from selObj in trigger.EntityAdapter.As<AssetAdapter>().AssetClass.ArtDefReferences
				where selObj.Tags.Contains("VFX", StringComparer.InvariantCultureIgnoreCase) && selObj.CollectionName == trigger.VFXCollection
				select selObj.TemplateName).FirstOrDefault();
			if (!string.IsNullOrEmpty(text))
			{
				string[] suitableElements = CivTechRegistry.ArtDefRegistry.GetSuitableElements(text, trigger.VFXCollection);
				DefineEnum(suitableElements);
			}
		}
		return base.EditValue(context, provider, value);
	}
}
