using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class TransferTargetTypeEditor : EnumUITypeEditor
{
	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		TransferTriggerAdapter transferTriggerAdapter = context.Instance.As<TransferTriggerAdapter>();
		if (transferTriggerAdapter != null)
		{
			List<string> list = new List<string>();
			list.Add("None (0)");
			IEnumerable<string> enumerable = transferTriggerAdapter.TimelineAdapter.Triggers.Where((TriggerAdapter wObj) => wObj.TriggerType == TriggerType.TT_ASSET_VFX || wObj.TriggerType == TriggerType.TT_ARTDEF_VFX).Select(delegate(TriggerAdapter sObj)
			{
				string text;
				if ((text = sObj.As<AssetFXTriggerAdapter>()?.VFXAsset) == null)
				{
					text = sObj.As<ArtDefFXTriggerAdapter>()?.VFXElement;
				}
				return text + " (" + sObj.Name + ")";
			}).ToArray();
			if (enumerable.Any())
			{
				list.AddRange(enumerable);
			}
			DefineEnum(list.ToArray());
		}
		return base.EditValue(context, provider, value);
	}
}
