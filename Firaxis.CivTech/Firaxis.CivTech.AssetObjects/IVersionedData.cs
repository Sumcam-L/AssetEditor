using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IVersionedData
{
	Version Version { get; }

	void SetVersion(string versionString);

	void SetVersion(int major, int minor, int build, int revision);
}
