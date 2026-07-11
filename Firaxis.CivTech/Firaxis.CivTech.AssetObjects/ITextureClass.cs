namespace Firaxis.CivTech.AssetObjects;

public interface ITextureClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	ITextureExportOptions ExportOptions { get; }
}
