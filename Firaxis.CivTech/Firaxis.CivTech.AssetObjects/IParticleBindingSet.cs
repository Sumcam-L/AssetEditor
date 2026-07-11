using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IParticleBindingSet
{
	IEnumerable<IParticleBinding> Bindings { get; }

	IParticleBinding FindBinding(string slotName);

	IParticleBinding Bind(string slotName, string particleName);

	void Unbind(string slotName);
}
