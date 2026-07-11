namespace Firaxis.CivTech.AssetObjects;

public interface IAttachmentRemoved : IEntityChangedEvent
{
	string AttachmentName { get; set; }
}
