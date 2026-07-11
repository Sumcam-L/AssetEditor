using System.IO;
using System.Windows.Forms;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class AssetManager : IAssetManager
{
	public AssetProviderCollection Assets { get; private set; }

	public ProviderFactory Factory { get; private set; }

	public AssetManager()
	{
		Assets = new AssetProviderCollection();
		Factory = new ProviderFactory();
	}

	public IAssetProvider LaunchAsset(string file)
	{
		return LaunchAsset(file, null);
	}

	public IAssetProvider LaunchAsset(string file, IAssetProvider parent)
	{
		if (string.IsNullOrEmpty(file))
		{
			ExceptionLogger.Log("Missing file", "Attempt to launch a missing file: <br>" + file, OperationResultLevel.Error);
			MessageBox.Show($"Unable to launch asset '{Path.GetFileName(file)}'. Check path location", "Launch Asset", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return null;
		}
		string fileName = Path.GetFileName(file);
		IAssetProvider assetProvider = Assets.FindByAsset(fileName);
		if (assetProvider != null)
		{
			assetProvider.ParentAsset = parent;
			assetProvider.ShowIt();
			return assetProvider;
		}
		IMaker maker = Factory.FindByExt(fileName);
		if (maker != null)
		{
			assetProvider = (IAssetProvider)maker.Make();
			assetProvider.ShowIt();
			assetProvider.SetAsset(file, parent);
			return assetProvider;
		}
		return null;
	}
}
