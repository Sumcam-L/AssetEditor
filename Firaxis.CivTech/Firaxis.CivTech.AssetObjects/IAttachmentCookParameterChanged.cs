namespace Firaxis.CivTech.AssetObjects;

public interface IAttachmentCookParameterChanged : IEntityChangedEvent
{
	string AttachmentName { get; set; }

	string ParameterName { get; set; }

	IValue ChangedValue { get; set; }
}
