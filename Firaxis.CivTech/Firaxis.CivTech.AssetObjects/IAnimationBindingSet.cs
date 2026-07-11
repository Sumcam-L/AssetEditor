using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IAnimationBindingSet
{
	IEnumerable<IAnimationBinding> Bindings { get; }

	IAnimationBinding FindBinding(string slotName);

	IAnimationBinding Bind(string slotName, string animationName);

	void Unbind(string slotName);
}
