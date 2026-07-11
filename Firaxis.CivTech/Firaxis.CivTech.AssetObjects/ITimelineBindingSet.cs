using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ITimelineBindingSet
{
	IEnumerable<ITimelineBinding> Bindings { get; }

	ITimelineBinding FindBinding(string slotName);

	ITimelineBinding Bind(string slotName, string timelineName);

	void Unbind(string slotName);
}
