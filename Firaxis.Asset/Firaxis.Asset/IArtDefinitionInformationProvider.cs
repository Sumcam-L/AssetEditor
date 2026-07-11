using System.Collections.Generic;

namespace Firaxis.Asset;

public interface IArtDefinitionInformationProvider
{
	Dictionary<string, List<string>> ArtDefinitionInformation { get; }
}
