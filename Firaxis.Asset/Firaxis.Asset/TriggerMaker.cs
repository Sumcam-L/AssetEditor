using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.Asset;

public class TriggerMaker<T> : Maker<T>, ITriggerMaker
{
	public TriggerType TriggerType { get; private set; }

	public TriggerMaker(TriggerType triggerType, params object[] args)
		: base(args)
	{
		TriggerType = triggerType;
	}
}
