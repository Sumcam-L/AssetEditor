using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetPreviewing;

public interface IEntityFilterProvider
{
	IEnumerable<InstanceType> AllowedTypes { get; }

	IEnumerable<string> AllowedClasses { get; }
}
