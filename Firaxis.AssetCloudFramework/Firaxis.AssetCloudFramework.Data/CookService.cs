using Firaxis.CivTech;

namespace Firaxis.AssetCloudFramework.Data;

public class CookService : IValidateService, IServiceNameProvider
{
	public string XLPPath { get; set; }

	public CookService()
	{
		XLPPath = string.Empty;
	}

	public bool Validate()
	{
		return !string.IsNullOrEmpty(XLPPath);
	}

	public string GetServiceName()
	{
		return GetType().Name;
	}
}
