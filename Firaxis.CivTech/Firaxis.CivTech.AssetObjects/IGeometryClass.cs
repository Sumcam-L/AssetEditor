namespace Firaxis.CivTech.AssetObjects;

public interface IGeometryClass : IClassEntity, ICloudEntity, INameProvider, IVersionedData
{
	IParameterSet GroupParameters { get; }

	DCCExportType ExportType { get; set; }

	uint MaxSkinnedBones { get; set; }

	float MinimumTriangleArea { get; set; }

	bool EnableDegenerateCheck { get; set; }
}
