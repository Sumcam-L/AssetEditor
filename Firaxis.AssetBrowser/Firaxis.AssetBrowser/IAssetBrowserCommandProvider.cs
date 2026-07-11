using System.Collections.Generic;

namespace Firaxis.AssetBrowser;

public interface IAssetBrowserCommandProvider
{
	IEnumerable<IAssetBrowserCommandDefinition> Commands { get; }
}
