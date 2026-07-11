using System.Collections.Generic;
using Firaxis.Collections;

namespace Firaxis.Asset.Trigger;

public class TriggerTrackCollection : ListEvent<TriggerTrack>
{
	public TriggerTrackCollection()
	{
	}

	public TriggerTrackCollection(IEnumerable<TriggerTrack> collection)
		: base(collection)
	{
	}

	public TriggerTrackCollection(int capacity)
		: base(capacity)
	{
	}

	public TriggerTrack Find(string ec)
	{
		return Find((TriggerTrack a) => a.ID == ec);
	}
}
