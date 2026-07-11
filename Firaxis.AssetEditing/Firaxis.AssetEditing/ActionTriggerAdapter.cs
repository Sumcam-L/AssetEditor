using System.ComponentModel;
using System.Drawing;
using Firaxis.Asset.Properties;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Reflection;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ActionTriggerAdapter : TriggerAdapter, IColorProvider
{
	public override Image TriggerImage => Firaxis.Asset.Properties.Resources.flagged;

	public string Action
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ActionTriggerType.ActionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ActionTriggerType.ActionAttribute, value);
		}
	}

	public string AttachmentPoint
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ActionTriggerType.AttachmentPointNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ActionTriggerType.AttachmentPointNameAttribute, value);
		}
	}

	[Browsable(false)]
	public Color Color
	{
		get
		{
			if (string.IsNullOrEmpty(AttachmentPoint))
			{
				return Color.FromArgb(255, 128, 0);
			}
			return Color.FromArgb(0, 128, 255);
		}
	}

	public override void UpdateNativeDataOnlyFromAdapter()
	{
		base.UpdateNativeDataOnlyFromAdapter();
		base.Trigger.FXName = Action;
		base.Trigger.AttachmentPointName = AttachmentPoint;
	}

	public override void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
		base.UpdateAdapterDataOnlyFromNative(trigger);
		BugSubmitter.SilentAssert(trigger.Type == TriggerType.TT_ACTION, "@summary ActionTriggerAdapter.Update called with wrong type of ITrigger @assign bwhitman");
		Action = trigger.FXName;
		AttachmentPoint = trigger.AttachmentPointName;
	}

	protected override void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		base.OnDomNodeAttributeChanged(attr);
		if (attr.AttributeInfo == EntitySchema.ActionTriggerType.ActionAttribute)
		{
			base.Trigger.FXName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.ActionTriggerType.AttachmentPointNameAttribute)
		{
			base.Trigger.AttachmentPointName = (string)attr.NewValue;
		}
	}
}
