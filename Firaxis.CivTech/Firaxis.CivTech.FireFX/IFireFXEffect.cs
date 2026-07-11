using System.Collections.Generic;

namespace Firaxis.CivTech.FireFX;

public interface IFireFXEffect
{
	string Name { get; }

	IEnumerable<IFireFXEmitter> Emitters { get; }
}
