using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class TriggerViewModel : SelectableItemViewModel
{
	public readonly ITrigger Trigger;

	public string TriggerName => Trigger.Name;

	public TriggerViewModel(ITrigger trigger)
	{
		Trigger = trigger;
	}
}
