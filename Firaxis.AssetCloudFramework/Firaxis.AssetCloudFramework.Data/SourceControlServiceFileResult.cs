using System.Collections.Generic;
using Firaxis.CivTech;

namespace Firaxis.AssetCloudFramework.Data;

public class SourceControlServiceFileResult : IServiceNameProvider
{
	public string Description { get; set; }

	public IList<string> Files { get; set; }

	public SourceControlServiceFileResult()
	{
		Description = string.Empty;
		Files = new List<string>();
	}

	public SourceControlServiceFileResult(IList<string> files)
	{
		Description = string.Empty;
		Files = files;
	}

	public string GetServiceName()
	{
		return "SourceControlService";
	}
}
