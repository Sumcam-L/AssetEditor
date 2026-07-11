namespace Firaxis.CivTech.AssetObjects;

public interface IMaterialClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	IMaterialValidationOptions ValidationOptions { get; }
}
