using Firaxis.ATF;

namespace Firaxis.AssetEditing;

public interface IAssetBrowserServiceProvider
{
	IAssetBrowserDialogService AssetBrowserService { get; }
}
