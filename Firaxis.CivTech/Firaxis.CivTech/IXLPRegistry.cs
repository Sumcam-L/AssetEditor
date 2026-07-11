using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public interface IXLPRegistry
{
	IXLPCacheData FindXLPData(string xlpEntryName);

	IXLPCacheData FindXLPEntryData(string relativeXLPPath, string xlpEntryName);

	EntityID GetEntityID(IXLPCacheData xlpEntryData);

	EntityID GetEntityID(string relativeXLPPath, string xlpEntryName);

	IEnumerable<IXLPCacheData> FindXLPEntryData(EntityID entity, string entityClassName);

	IDictionary<string, IEnumerable<IXLPCacheData>> GetXLPCacheData();

	IDictionary<string, IEnumerable<IXLPCacheData>> GetXLPCacheData(IEnumerable<string> packageNames);

	void HandleProjectChange();
}
