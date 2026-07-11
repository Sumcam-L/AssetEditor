using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IInstanceTypeFilterDefinition : IEntityFilterDefinition
{
	ICollection<InstanceType> ValidInstanceTypes { get; }

	ICollection<InstanceType> SelectedInstanceTypes { get; }
}
