using System;
using Firaxis.CivTech.TextureExport;

namespace Firaxis.CivTech.AssetObjects;

public interface ITextureInstance : IImportedEntity, IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	IExportSettingsParams ExportSettings { get; set; }

	uint Height { get; set; }

	uint Width { get; set; }

	uint Depth { get; set; }

	uint NumMipMaps { get; set; }

	bool IsGradientMapTexture();

	string GetExportSettingsString();
}
