namespace Firaxis.Asset;

public interface IAssetTarget
{
	IAssetProvider ParentAsset { get; set; }

	string TargetName { get; }

	void ShowIt();

	void HideIt();

	void CloseIt();
}
