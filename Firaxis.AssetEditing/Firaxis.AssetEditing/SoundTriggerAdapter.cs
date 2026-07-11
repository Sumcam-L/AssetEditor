using System.ComponentModel;
using System.Drawing;
using Firaxis.Asset.Properties;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Reflection;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class SoundTriggerAdapter : TriggerAdapter, IColorProvider
{
	public override Image TriggerImage => Firaxis.Asset.Properties.Resources.trig_slist;

	public string AudioEvent
	{
		get
		{
			return GetAttribute<string>(EntitySchema.SoundTriggerType.AudioEventAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SoundTriggerType.AudioEventAttribute, value);
		}
	}

	public string AttachmentPoint
	{
		get
		{
			return GetAttribute<string>(EntitySchema.SoundTriggerType.AttachmentPointNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SoundTriggerType.AttachmentPointNameAttribute, value);
		}
	}

	[Browsable(false)]
	public Color Color
	{
		get
		{
			if (string.IsNullOrEmpty(AttachmentPoint))
			{
				return Color.FromArgb(32, 255, 0);
			}
			return Color.FromArgb(224, 0, 255);
		}
	}

	public override void UpdateNativeDataOnlyFromAdapter()
	{
		base.UpdateNativeDataOnlyFromAdapter();
		base.Trigger.FXName = AudioEvent;
		base.Trigger.AttachmentPointName = AttachmentPoint;
	}

	public override void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
		base.UpdateAdapterDataOnlyFromNative(trigger);
		BugSubmitter.SilentAssert(trigger.Type == TriggerType.TT_SOUND, "@summary SoundTriggerAdapter.Update called with wrong type of ITrigger @assign bwhitman");
		AudioEvent = trigger.FXName;
		AttachmentPoint = trigger.AttachmentPointName;
	}

	protected override void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		base.OnDomNodeAttributeChanged(attr);
		if (attr.AttributeInfo == EntitySchema.SoundTriggerType.AudioEventAttribute)
		{
			base.Trigger.FXName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.SoundTriggerType.AttachmentPointNameAttribute)
		{
			base.Trigger.AttachmentPointName = (string)attr.NewValue;
		}
	}
}
