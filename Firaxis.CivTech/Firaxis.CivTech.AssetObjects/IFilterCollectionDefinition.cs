using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IFilterCollectionDefinition : IEntityFilterDefinition
{
	ICollection<IEntityFilterDefinition> FilterDefinitions { get; }
}
