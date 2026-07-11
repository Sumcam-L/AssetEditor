namespace Firaxis.CivTech.AssetObjects;

public interface ISplineClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	bool Is3D { get; set; }
}
