using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public interface IPlatformSelectorContext
{
	IEnumerable<Platforms> AllowedPlatforms { get; }

	void AllowPlatform(Platforms platform);

	void ClearAllowedPlatforms();

	bool IsPlatformAllowed(Platforms platform);

	void RemovePlatform(Platforms platform);
}
