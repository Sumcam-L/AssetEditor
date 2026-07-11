using System.Collections.Generic;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ContentExporters;

public static class ImportOperationResultsExtensions
{
	public static IEnumerable<ImportOperationResult> GetFailedResults(this IEnumerable<ImportOperationResult> allResults)
	{
		foreach (ImportOperationResult result in allResults)
		{
			if (!result.Result)
			{
				yield return result;
			}
		}
	}

	public static IEnumerable<ImportOperationResult> GetValidResults(this IEnumerable<ImportOperationResult> allResults)
	{
		foreach (ImportOperationResult result in allResults)
		{
			if ((bool)result.Result)
			{
				yield return result;
			}
		}
	}

	public static IEnumerable<IImportedEntity> GetValidEntities(this IDictionary<IImportedEntity, ImportOperationResult> allResults)
	{
		foreach (KeyValuePair<IImportedEntity, ImportOperationResult> pair in allResults)
		{
			if ((bool)pair.Value.Result)
			{
				yield return pair.Key;
			}
		}
	}

	public static string GetCombinedFailureMessages(this IEnumerable<ImportOperationResult> failedResults)
	{
		StringBuilder stringBuilder = new StringBuilder("The following entities failed the export process.\n\n");
		foreach (ImportOperationResult failedResult in failedResults)
		{
			stringBuilder.AppendFormat($"{failedResult.Entity.Name} : {failedResult.Result.Message}\n");
		}
		BugSubmitter.SilentReport(stringBuilder.ToString() + " @assign agould @summary Exporting entity from source file failed");
		return stringBuilder.ToString();
	}
}
