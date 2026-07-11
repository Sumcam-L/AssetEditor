using System;
using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;

namespace Firaxis.ContentExporters;

public interface IContentExporter
{
	IEnumerable<string> SupportedFileTypes { get; }

	IEnumerable<InstanceType> SupportedInstanceTypes { get; }

	IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType);

	void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity entityClass);

	void Export(ICivTechService civTechSvc, IEnumerable<Tuple<ImportOperationResult, IClassEntity>> entities);

	ResultCode ValidateClass(IImportedEntity entity, IClassEntity entityClass);

	ResultCode Validate(IImportedEntity entity, IClassEntity entityClass, string localPantry);

	ResultCode RebuildExportedEntity(ICivTechService civTechSvc, IImportedEntity entity, IClassEntity entityClass);
}
