using System.Drawing;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LightTriggerAdapter : TriggerAdapter, IDurableTriggerAdapter
{
	public override Image TriggerImage => ResourceUtil.GetImage16(Resources.AnalyticLightFileIcon);

	public string LightAsset
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LightTriggerType.LightAssetAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightTriggerType.LightAssetAttribute, value);
		}
	}

	public string AttachmentPoint
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LightTriggerType.AttachmentPointNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightTriggerType.AttachmentPointNameAttribute, value);
		}
	}

	public float Duration
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightTriggerType.DurationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightTriggerType.DurationAttribute, value);
		}
	}

	public override void UpdateNativeDataOnlyFromAdapter()
	{
		base.UpdateNativeDataOnlyFromAdapter();
		base.Trigger.FXName = LightAsset;
		base.Trigger.AttachmentPointName = AttachmentPoint;
		base.Trigger.Duration = Duration;
	}

	public override void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
		base.UpdateAdapterDataOnlyFromNative(trigger);
		BugSubmitter.SilentAssert(trigger.Type == TriggerType.TT_LIGHT, "@summary LightTriggerAdapter.Update called with wrong type of ITrigger @assign bwhitman");
		LightAsset = trigger.FXName;
		AttachmentPoint = trigger.AttachmentPointName;
		Duration = trigger.Duration;
	}

	protected override void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		base.OnDomNodeAttributeChanged(attr);
		if (attr.AttributeInfo == EntitySchema.LightTriggerType.LightAssetAttribute)
		{
			base.Trigger.FXName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.LightTriggerType.AttachmentPointNameAttribute)
		{
			base.Trigger.AttachmentPointName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.LightTriggerType.DurationAttribute)
		{
			base.Trigger.Duration = (float)attr.NewValue;
		}
	}
}
