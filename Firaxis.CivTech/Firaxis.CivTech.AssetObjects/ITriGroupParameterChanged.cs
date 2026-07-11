namespace Firaxis.CivTech.AssetObjects;

public interface ITriGroupParameterChanged : IEntityChangedEvent
{
	string ModelName { get; set; }

	string MeshName { get; set; }

	string GroupName { get; set; }

	string StateName { get; set; }

	string ParameterName { get; set; }

	IValue ChangedValue { get; set; }
}
