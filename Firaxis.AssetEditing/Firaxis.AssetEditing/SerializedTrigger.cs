using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public class SerializedTrigger : ITrigger
{
	public string AttachmentPointName { get; set; }

	public string CollectionName { get; set; }

	public string Description { get; set; }

	public float Duration { get; set; }

	public string FXName { get; set; }

	public string Name { get; set; }

	public float StartTime { get; set; }

	public int TrackIndex { get; set; }

	public TriggerType Type { get; set; }

	public SerializedTrigger()
	{
	}

	public SerializedTrigger(ITrigger trigger)
	{
		AttachmentPointName = trigger.AttachmentPointName;
		CollectionName = trigger.CollectionName;
		Description = trigger.Description;
		Duration = trigger.Duration;
		FXName = trigger.FXName;
		Name = trigger.Name;
		StartTime = trigger.StartTime;
		TrackIndex = trigger.TrackIndex;
		Type = trigger.Type;
	}
}
