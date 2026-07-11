using System;

namespace Firaxis.ATF;

public interface IVersionProvider
{
	Version Version { get; }
}
