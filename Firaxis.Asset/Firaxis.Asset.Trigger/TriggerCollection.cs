using System.Collections.Generic;
using Firaxis.Collections;

namespace Firaxis.Asset.Trigger;

public class TriggerCollection : ListEventID<ITrigger>
{
	public TriggerCollection()
	{
	}

	public TriggerCollection(IEnumerable<ITrigger> collection)
		: base(collection)
	{
	}

	public TriggerCollection(int capacity)
		: base(capacity)
	{
	}
}
