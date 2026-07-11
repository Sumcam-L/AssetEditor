using System.Collections.Generic;
using Firaxis.CivTech;

namespace Firaxis.AssetCloudFramework.Data;

public class SourceControlServicePathRequest : ISourceControlServiceRequest, ISourceControlServiceRequestDescriptor, IServiceNameProvider
{
	public string Description { get; set; }

	public IList<string> FullPaths { get; set; }

	public SourceControlServicePathRequest(IList<string> fullPaths)
	{
		Description = string.Empty;
		FullPaths = fullPaths;
	}

	public virtual IEnumerable<string> GetFullyQualifiedFilePaths(string gamePantry)
	{
		return FullPaths;
	}

	public string GetServiceName()
	{
		return "SourceControlService";
	}
}
