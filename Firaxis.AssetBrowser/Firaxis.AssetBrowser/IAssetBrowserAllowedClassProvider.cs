using System.Collections.Generic;

namespace Firaxis.AssetBrowser;

public interface IAssetBrowserAllowedClassProvider
{
	IEnumerable<string> AllowedClasses { get; }
}
