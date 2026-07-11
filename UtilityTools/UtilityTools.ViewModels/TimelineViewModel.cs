using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class TimelineViewModel : SelectableItemViewModel
{
	public readonly ITimeline Timeline;

	private ICollection<TriggerViewModel> m_triggers = new ObservableCollection<TriggerViewModel>();

	public bool NoChildren => !Triggers.Any();

	public string TimelineName => Timeline.Name;

	public ICollection<TriggerViewModel> Triggers => m_triggers;

	public TimelineViewModel(ITimeline timeline)
	{
		Timeline = timeline;
		foreach (ITrigger trigger in timeline.Triggers)
		{
			m_triggers.Add(new TriggerViewModel(trigger));
		}
	}
}
