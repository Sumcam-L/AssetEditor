using System.Collections.Generic;
using Firaxis.AssetEditing;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetPreviewing;

internal class AttachmentScaleDriver : APWidgetDriver
{
	public float Scale { get; set; }

	public AttachmentScaleDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		: base(flags, aplist)
	{
		Scale = 0f;
	}

	public override string GetNativeWidgetName()
	{
		return "Scale";
	}

	public override void GetCustomArguments(IValueSet args)
	{
		args.Push<IFloatValue>("SnapPrecision").ParameterValue = base.Flags.GridSnappingPrecision;
	}

	public override void OnWidgetEdit(IEntityChangeList changelist)
	{
		foreach (AttachmentPointAdapter attachment in base.AttachmentList)
		{
			changelist.CreateAttachmentChangedEvent(attachment.EntityAdapter.InstanceEntity, attachment.Name, attachment.Name, attachment.ModelInstanceName, attachment.BoneName, attachment.Position, attachment.Orientation, attachment.Scale * Scale);
		}
	}

	public override void OnWidgetFinish()
	{
		foreach (AttachmentPointAdapter attachment in base.AttachmentList)
		{
			attachment.Scale *= Scale;
		}
		Scale = 0f;
	}
}
