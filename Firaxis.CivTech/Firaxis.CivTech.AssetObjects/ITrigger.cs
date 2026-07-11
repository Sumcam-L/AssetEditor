namespace Firaxis.CivTech.AssetObjects;

public interface ITrigger
{
	string Name { get; set; }

	TriggerType Type { get; }

	string FXName { get; set; }

	string CollectionName { get; set; }

	string AttachmentPointName { get; set; }

	string Description { get; set; }

	float StartTime { get; set; }

	float Duration { get; set; }

	int TrackIndex { get; set; }
}
