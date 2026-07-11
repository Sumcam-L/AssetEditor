using System.Collections.Generic;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class AssetTargetCollector : IAssetTargetCollector
{
	public IEnumerable<IAssetTarget> AssetTargets
	{
		get
		{
			if (!Context.TryGet<IAssetManager>(out var am))
			{
				yield break;
			}
			foreach (IAssetProvider asset in am.Assets)
			{
				yield return asset;
			}
		}
	}
}
