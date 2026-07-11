using System.Collections.Generic;

namespace Firaxis.AssetCloudFramework.Data;

public interface ISourceControlServiceRequest
{
	IEnumerable<string> GetFullyQualifiedFilePaths(string gamePantry);
}
