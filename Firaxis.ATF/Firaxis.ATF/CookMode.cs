using System;

namespace Firaxis.ATF;

[Flags]
public enum CookMode
{
	XLP = 1,
	ArtDef = 2,
	Dependency = 4,
	ChangedOnly = 8
}
