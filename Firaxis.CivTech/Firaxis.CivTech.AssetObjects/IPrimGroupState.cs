namespace Firaxis.CivTech.AssetObjects;

public interface IPrimGroupState
{
	string MeshName { get; }

	string GroupName { get; }

	string StateName { get; }

	IValueSet Values { get; }
}
