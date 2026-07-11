using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IModelInstance
{
	string Name { get; }

	string GeoName { get; }

	uint GroupCount { get; }

	IEnumerable<IPrimGroupState> PrimGroups { get; }

	IPrimGroupState FindPrimGroupState(string meshName, string groupName, string stateName);

	IPrimGroupState AddPrimGroupState(string meshName, string groupName, string stateName);

	void RemoveGroupState(IPrimGroupState primGroupState);

	void ClearPrimGroups();
}
