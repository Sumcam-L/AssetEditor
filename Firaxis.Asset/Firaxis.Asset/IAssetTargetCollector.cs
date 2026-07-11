using System.Collections.Generic;

namespace Firaxis.Asset;

public interface IAssetTargetCollector
{
	IEnumerable<IAssetTarget> AssetTargets { get; }
}
