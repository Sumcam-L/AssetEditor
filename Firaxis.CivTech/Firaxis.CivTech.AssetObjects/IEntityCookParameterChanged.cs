namespace Firaxis.CivTech.AssetObjects;

public interface IEntityCookParameterChanged : IEntityChangedEvent
{
	string ParameterName { get; set; }

	IValue ChangedValue { get; set; }
}
