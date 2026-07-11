using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface IAssetBrowserTypeProvider
{
	IEnumerable<string> ValidClassNames { get; }

	IEnumerable<InstanceType> ValidTypes { get; }

	IEntityFilteringContext EntityFilteringContext { get; }
}
