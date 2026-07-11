using System;

namespace Firaxis.ATF;

public interface IVersionService
{
	Version ApplicationVersion { get; }

	Version LocalToolVersion { get; }

	bool IsLocalBuild();
}
