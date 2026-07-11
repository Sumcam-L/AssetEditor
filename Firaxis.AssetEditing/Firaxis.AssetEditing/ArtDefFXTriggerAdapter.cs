using System.Drawing;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefFXTriggerAdapter : TriggerAdapter, IDurableTriggerAdapter
{
	public override Image TriggerImage => ResourceUtil.GetImage16(Resources.ArtDefFileIcon);

	public string VFXElement
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ArtDefFXTriggerType.VFXElementAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ArtDefFXTriggerType.VFXElementAttribute, value);
		}
	}

	public string VFXCollection
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ArtDefFXTriggerType.VFXCollectionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ArtDefFXTriggerType.VFXCollectionAttribute, value);
		}
	}

	public string AttachmentPoint
	{
		get
		{
			return GetAttribute<string>(EntitySchema.ArtDefFXTriggerType.AttachmentPointNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ArtDefFXTriggerType.AttachmentPointNameAttribute, value);
		}
	}

	public float Duration
	{
		get
		{
			return GetAttribute<float>(EntitySchema.ArtDefFXTriggerType.DurationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.ArtDefFXTriggerType.DurationAttribute, value);
		}
	}

	public override void UpdateNativeDataOnlyFromAdapter()
	{
		base.UpdateNativeDataOnlyFromAdapter();
		base.Trigger.FXName = VFXElement;
		base.Trigger.CollectionName = VFXCollection;
		base.Trigger.AttachmentPointName = AttachmentPoint;
		base.Trigger.Duration = Duration;
	}

	public override void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
		base.UpdateAdapterDataOnlyFromNative(trigger);
		BugSubmitter.SilentAssert(trigger.Type == TriggerType.TT_ARTDEF_VFX, "@summary ArtDefFXTriggerAdapter.Update called with wrong type of ITrigger @assign bwhitman");
		VFXElement = trigger.FXName;
		VFXCollection = trigger.CollectionName;
		AttachmentPoint = trigger.AttachmentPointName;
		Duration = trigger.Duration;
	}

	protected override void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		base.OnDomNodeAttributeChanged(attr);
		if (attr.AttributeInfo == EntitySchema.ArtDefFXTriggerType.VFXElementAttribute)
		{
			base.Trigger.FXName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.ArtDefFXTriggerType.VFXCollectionAttribute)
		{
			base.Trigger.CollectionName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.ArtDefFXTriggerType.AttachmentPointNameAttribute)
		{
			base.Trigger.AttachmentPointName = (string)attr.NewValue;
		}
		else if (attr.AttributeInfo == EntitySchema.ArtDefFXTriggerType.DurationAttribute)
		{
			base.Trigger.Duration = (float)attr.NewValue;
		}
	}
}
