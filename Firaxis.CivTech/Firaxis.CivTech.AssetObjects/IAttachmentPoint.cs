namespace Firaxis.CivTech.AssetObjects;

public interface IAttachmentPoint
{
	string Name { get; set; }

	string BoneName { get; set; }

	string ModelInstanceName { get; set; }

	IValueSet CookParameters { get; }

	IFloatVector3 Position { get; set; }

	IFloatVector3 Orientation { get; set; }

	float Scale { get; set; }
}
