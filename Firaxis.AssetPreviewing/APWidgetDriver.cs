using System.Collections.Generic;
using Firaxis.AssetEditing;
using Firaxis.CivTech.AssetObjects;

public abstract class APWidgetDriver
{
	public List<AttachmentPointAdapter> AttachmentList { get; set; }

	public WidgetFlags Flags { get; }

	public APWidgetDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
	{
		AttachmentList = new List<AttachmentPointAdapter>();
		AttachmentList.AddRange(aplist);
		Flags = flags;
	}

	public abstract string GetNativeWidgetName();

	public abstract void GetCustomArguments(IValueSet args);

	public abstract void OnWidgetEdit(IEntityChangeList changelist);

	public abstract void OnWidgetFinish();
}
