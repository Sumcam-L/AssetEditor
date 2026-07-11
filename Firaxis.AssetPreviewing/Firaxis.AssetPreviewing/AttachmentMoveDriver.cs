using System.Collections.Generic;
using Firaxis.AssetEditing;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetPreviewing;

public class AttachmentMoveDriver : APWidgetDriver
{
	public Point3F Position { get; set; }

	public AttachmentMoveDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		: base(flags, aplist)
	{
		Position = new Point3F(0f, 0f, 0f);
	}

	public override string GetNativeWidgetName()
	{
		return "Move";
	}

	public override void GetCustomArguments(IValueSet args)
	{
		args.Push<IBoolValue>("SnapToAPs").ParameterValue = base.Flags.AttachmentSnapping;
	}

	public override void OnWidgetEdit(IEntityChangeList changelist)
	{
		foreach (AttachmentPointAdapter attachment in base.AttachmentList)
		{
			changelist.CreateAttachmentChangedEvent(attachment.EntityAdapter.InstanceEntity, attachment.Name, attachment.Name, attachment.ModelInstanceName, attachment.BoneName, new float[3]
			{
				attachment.Position[0] + Position.x,
				attachment.Position[1] + Position.y,
				attachment.Position[2] + Position.z
			}, attachment.Orientation, attachment.Scale);
		}
	}

	public override void OnWidgetFinish()
	{
		foreach (AttachmentPointAdapter attachment in base.AttachmentList)
		{
			attachment.Position = new float[3]
			{
				attachment.Position[0] + Position.x,
				attachment.Position[1] + Position.y,
				attachment.Position[2] + Position.z
			};
		}
		Position.x = (Position.y = (Position.z = 0f));
	}
}
