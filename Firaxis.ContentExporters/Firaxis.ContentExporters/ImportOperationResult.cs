using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;

namespace Firaxis.ContentExporters;

public class ImportOperationResult
{
	public readonly IImportedEntity Entity;

	public ResultCode Result { get; set; }

	public ImportOperationResult(IImportedEntity entity)
	{
		Entity = entity;
		Result = ResultCode.Success;
	}
}
