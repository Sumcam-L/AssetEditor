using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class TimelineBindingViewModel : SelectableItemViewModel
{
	public readonly ITimelineBinding TimelineBinding;

	public string BoundName => TimelineBinding.TimelineName;

	public override string DisplayName => $"{SlotName}: {BoundName}";

	public string SlotName => TimelineBinding.SlotName;

	public TimelineBindingViewModel(ITimelineBinding timelineBinding)
	{
		TimelineBinding = timelineBinding;
	}
}
