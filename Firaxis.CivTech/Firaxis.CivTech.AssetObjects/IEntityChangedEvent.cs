namespace Firaxis.CivTech.AssetObjects;

public interface IEntityChangedEvent
{
	EntityChangeType Type { get; }

	string EntityName { get; }

	InstanceType InstanceType { get; }
}
