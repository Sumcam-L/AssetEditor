using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;

namespace Firaxis.CivTech;

public interface IQueryService
{
	SerializableDictionary<InstanceType, List<string>> InstanceItems { get; set; }
}
