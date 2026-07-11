using System;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public class SelectedAssetChangedEventArgs : EventArgs
{
	public IAssetInstance Asset { get; private set; }

	public SelectedAssetChangedEventArgs(IAssetInstance asset)
	{
		Asset = asset;
	}
}
