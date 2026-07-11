using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IImportedEntity : IInstanceEntity, ICloudEntity, INameProvider, IVersionedData, ISerializable, IEquatable<IInstanceEntity>, IEquatable<EntityID>
{
	string SourceFilePath { get; set; }

	string SourceObjectName { get; set; }

	DateTime ImportedTime { get; set; }

	DateTime ExportedTime { get; set; }

	bool NewEntity { get; set; }

	bool ReadyForExport();

	void UpdateImportedTime();

	void UpdateExportedTime();

	void ResetImportedTime();

	void ResetExportedTime();
}
