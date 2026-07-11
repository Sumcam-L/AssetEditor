namespace Firaxis.CivTech.AssetObjects;

public interface IAnimationClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	DCCExportType ExportType { get; set; }
}
