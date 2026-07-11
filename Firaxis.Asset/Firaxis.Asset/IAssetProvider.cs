using System;

namespace Firaxis.Asset;

public interface IAssetProvider : IAssetTarget
{
	SourceControlFile FileAsset { get; }

	object AssetContents { get; }

	string Asset { get; }

	bool Modified { get; set; }

	bool EnableEdit { get; set; }

	event EventHandler ModifiedChanged;

	event EventHandler AssetChanged;

	event EventHandler AssetLoadSucceeded;

	event EventHandler AssetLoadFailed;

	event EventHandler AssetValidationFinished;

	event EventHandler EnableEditChanged;

	event EventHandler ParentAssetChanged;

	void SetAsset(string asset, IAssetProvider parentAsset);

	void ClearAsset();

	void Reload();

	void Save();

	void SaveAs(string file);

	void ValidateAsset();
}
