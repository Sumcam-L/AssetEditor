namespace Firaxis.CivTech.AssetObjects;

public interface IEnvironmentLightClass : ILightClass, IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	IEnvironmentMapImportOptions ImportOptions { get; }
}
