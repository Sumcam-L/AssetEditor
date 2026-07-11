using System;

namespace Firaxis.CivTech.FireFX;

[Flags]
public enum EmitterFlags
{
	BaseEmitter = 1,
	SpawnOnly = 2,
	SimOnly = 4
}
