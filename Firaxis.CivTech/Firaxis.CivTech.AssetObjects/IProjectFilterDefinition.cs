using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IProjectFilterDefinition : IEntityFilterDefinition
{
	ICollection<string> ValidProjectNames { get; }
}
