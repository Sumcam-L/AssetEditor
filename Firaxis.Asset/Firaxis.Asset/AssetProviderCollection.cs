using System.IO;
using Firaxis.Collections;

namespace Firaxis.Asset;

public class AssetProviderCollection : ListEvent<IAssetProvider>
{
	public IAssetProvider FindByAsset(string file)
	{
		string name = Path.GetFileName(file);
		return Find((IAssetProvider a) => string.Compare(name, Path.GetFileName(a.Asset), ignoreCase: true) == 0);
	}
}
