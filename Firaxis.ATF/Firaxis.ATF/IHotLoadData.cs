using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface IHotLoadData
{
	IEnumerable<Uri> DependencyFileUris { get; }

	IEnumerable<string> RelativeArtDefPaths { get; }

	IEnumerable<string> RelativePackagePaths { get; }
}
