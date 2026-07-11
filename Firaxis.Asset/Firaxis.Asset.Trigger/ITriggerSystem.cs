using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.Asset.Trigger;

public interface ITriggerSystem : ITagProvider, IServiceProviderProvider
{
	TriggerCollection Triggers { get; }

	TriggerTrackCollection Tracks { get; }

	Factory<ITrigger> TriggerFactory { get; }

	IDictionary<TriggerType, IEnumerable<string>> TriggerTypeToValidClassesMapping { get; }

	void InitializeTriggerFactory(IDictionary<TriggerType, IEnumerable<string>> triggerTypeToValidClassesMapping);
}
