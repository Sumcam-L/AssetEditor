using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IVersionControlSelectionService
{
	IDictionary<string, VersionControlInfo> VersionControlInfoMap { get; }

	IVersionControlService this[string name] { get; }
}
