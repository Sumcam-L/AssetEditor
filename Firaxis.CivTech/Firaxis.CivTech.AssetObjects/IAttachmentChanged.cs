namespace Firaxis.CivTech.AssetObjects;

public interface IAttachmentChanged : IEntityChangedEvent
{
	string OldAttachmentName { get; set; }

	string NewAttachmentName { get; set; }

	string ModelInstanceName { get; set; }

	string BoneName { get; set; }

	float[] Position { get; set; }

	float[] Orientation { get; set; }

	float Scale { get; set; }
}
