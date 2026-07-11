using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IClassFilterDefinition : IEntityFilterDefinition
{
	ICollection<string> ValidClassNames { get; }
}
