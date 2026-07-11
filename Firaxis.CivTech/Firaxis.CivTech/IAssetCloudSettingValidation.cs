namespace Firaxis.CivTech;

public interface IAssetCloudSettingValidation
{
	bool ValidateUserLocalProjectConfig(string mainWorkspaceRoot);

	bool ValidateUserLocalToolHost();
}
