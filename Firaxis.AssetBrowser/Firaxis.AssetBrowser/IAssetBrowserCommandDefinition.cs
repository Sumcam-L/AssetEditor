using System.Windows.Input;
using System.Windows.Media;

namespace Firaxis.AssetBrowser;

public interface IAssetBrowserCommandDefinition
{
	string Name { get; }

	ImageSource Content { get; }

	ICommand Command { get; }
}
