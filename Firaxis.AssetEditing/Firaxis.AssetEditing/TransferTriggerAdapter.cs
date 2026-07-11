using System.Drawing;
using Firaxis.Asset.Properties;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TransferTriggerAdapter : TriggerAdapter
{
	public override Image TriggerImage => Firaxis.Asset.Properties.Resources.file_refresh;

	public string TargetTrigger
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TransferTriggerType.TargetTriggerAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TransferTriggerType.TargetTriggerAttribute, value);
		}
	}

	public override void UpdateNativeDataOnlyFromAdapter()
	{
		base.UpdateNativeDataOnlyFromAdapter();
		base.Trigger.FXName = TargetTrigger;
	}

	public override void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
		base.UpdateAdapterDataOnlyFromNative(trigger);
		BugSubmitter.SilentAssert(trigger.Type == TriggerType.TT_TRANSFER, "@summary TransferTriggerAdapter.Update called with wrong type of ITrigger @assign bwhitman");
		TargetTrigger = trigger.FXName;
	}

	protected override void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		base.OnDomNodeAttributeChanged(attr);
		if (attr.AttributeInfo == EntitySchema.TransferTriggerType.TargetTriggerAttribute)
		{
			base.Trigger.FXName = attr.NewValue.ToString();
		}
	}
}
