using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class AssetCopierViewModel : DialogViewModel
{
	private DelegateCommand m_checkAnimationBindings;

	private DelegateCommand m_checkParticleEffects;

	private DelegateCommand m_checkTimelines;

	private DelegateCommand m_checkTriggers;

	private readonly ICollection<AnimationBindingViewModel> m_animationBindings = new ObservableCollection<AnimationBindingViewModel>();

	private readonly ICollection<VFXViewModel> m_particleEffects = new ObservableCollection<VFXViewModel>();

	private readonly IAssetInstance m_sourceAsset;

	private readonly IEnumerable<IAssetInstance> m_targetAssets;

	private readonly ICollection<TimelineViewModel> m_timelines = new ObservableCollection<TimelineViewModel>();

	private readonly ICollection<TimelineBindingViewModel> m_timelineBindings = new ObservableCollection<TimelineBindingViewModel>();

	public ICollection<AnimationBindingViewModel> AnimationBindings => m_animationBindings;

	public ICollection<VFXViewModel> ParticleEffects => m_particleEffects;

	public string SourceAssetName => m_sourceAsset.Name;

	public IEnumerable<string> TargetAssetNames
	{
		get
		{
			foreach (IAssetInstance asset in m_targetAssets)
			{
				yield return asset.Name;
			}
		}
	}

	public ICollection<TimelineBindingViewModel> TimelineBindings => m_timelineBindings;

	public ICollection<TimelineViewModel> Timelines => m_timelines;

	public ICommand CheckAnimationBindings
	{
		get
		{
			if (m_checkAnimationBindings == null)
			{
				m_checkAnimationBindings = new DelegateCommand(ExecuteCheckAnimationsCommand);
			}
			return m_checkAnimationBindings;
		}
	}

	public ICommand CheckParticleEffects
	{
		get
		{
			if (m_checkParticleEffects == null)
			{
				m_checkParticleEffects = new DelegateCommand(ExecuteCheckParticleEffectsCommand);
			}
			return m_checkParticleEffects;
		}
	}

	public ICommand CheckTimelines
	{
		get
		{
			if (m_checkTimelines == null)
			{
				m_checkTimelines = new DelegateCommand(ExecuteCheckTimelinesCommand);
			}
			return m_checkTimelines;
		}
	}

	public ICommand CheckTriggers
	{
		get
		{
			if (m_checkTriggers == null)
			{
				m_checkTriggers = new DelegateCommand(ExecuteCheckTriggersCommand);
			}
			return m_checkTriggers;
		}
	}

	public AssetCopierViewModel(IAssetInstance sourceAsset, IEnumerable<IAssetInstance> targetAssets)
	{
		m_sourceAsset = sourceAsset;
		m_targetAssets = targetAssets;
		m_sourceAsset.AnimationBindings.Bindings.ForEach(delegate(IAnimationBinding binding)
		{
			m_animationBindings.Add(new AnimationBindingViewModel(binding));
		});
		m_sourceAsset.GetParticleEffects().ForEach(delegate(string vfx)
		{
			m_particleEffects.Add(new VFXViewModel(vfx));
		});
		m_sourceAsset.TimelineBindings.Bindings.ForEach(delegate(ITimelineBinding binding)
		{
			m_timelineBindings.Add(new TimelineBindingViewModel(binding));
		});
		m_sourceAsset.Timelines.Timelines.ForEach(delegate(ITimeline timeline)
		{
			m_timelines.Add(new TimelineViewModel(timeline));
		});
	}

	protected override void ExecuteOKCommand(object context)
	{
		foreach (IAssetInstance targetAsset in m_targetAssets)
		{
			m_animationBindings.Where((AnimationBindingViewModel binding) => binding.IsSelected).ForEach(delegate(AnimationBindingViewModel binding)
			{
				targetAsset.AnimationBindings.Bind(binding.SlotName, binding.BoundName);
			});
			m_particleEffects.Where((VFXViewModel vfx) => vfx.IsSelected).ForEach(delegate(VFXViewModel vfx)
			{
				targetAsset.AddParticleEffect(vfx.VFXName);
			});
			m_timelineBindings.Where((TimelineBindingViewModel binding) => binding.IsSelected).ForEach(delegate(TimelineBindingViewModel binding)
			{
				targetAsset.TimelineBindings.Bind(binding.SlotName, binding.BoundName);
			});
			foreach (TimelineViewModel timeline3 in m_timelines)
			{
				if (timeline3.IsSelected)
				{
					ITimeline timeline = targetAsset.Timelines.GetTimeline(timeline3.TimelineName);
					timeline.ClearTriggers();
					timeline.AnimationName = timeline3.Timeline.AnimationName;
					timeline.Description = timeline3.Timeline.Description;
					timeline.Duration = timeline3.Timeline.Duration;
					foreach (ITrigger trigger3 in timeline3.Timeline.Triggers)
					{
						ITrigger trigger = timeline.GetTrigger(trigger3.Name, trigger3.Type);
						trigger.AttachmentPointName = trigger3.AttachmentPointName;
						trigger.CollectionName = trigger3.CollectionName;
						trigger.Description = trigger3.Description;
						trigger.Duration = trigger3.Duration;
						trigger.FXName = trigger3.FXName;
						trigger.StartTime = trigger3.StartTime;
					}
					continue;
				}
				IEnumerable<TriggerViewModel> enumerable = timeline3.Triggers.Where((TriggerViewModel trig) => trig.IsSelected);
				if (!enumerable.Any())
				{
					continue;
				}
				ITimeline timeline2 = targetAsset.Timelines.GetTimeline(timeline3.TimelineName);
				foreach (TriggerViewModel item in enumerable)
				{
					ITrigger trigger2 = timeline2.GetTrigger(item.Trigger.Name, item.Trigger.Type);
					trigger2.AttachmentPointName = item.Trigger.AttachmentPointName;
					trigger2.CollectionName = item.Trigger.CollectionName;
					trigger2.Description = item.Trigger.Description;
					trigger2.Duration = item.Trigger.Duration;
					trigger2.FXName = item.Trigger.FXName;
					trigger2.StartTime = item.Trigger.StartTime;
				}
			}
		}
		base.ExecuteOKCommand(context);
	}

	private void ExecuteCheckAnimationsCommand(object context)
	{
		bool checkedState = (bool)context;
		SetCollectionChecked(AnimationBindings, checkedState);
	}

	private void ExecuteCheckParticleEffectsCommand(object context)
	{
		bool checkedState = (bool)context;
		SetCollectionChecked(ParticleEffects, checkedState);
	}

	private void ExecuteCheckTimelinesCommand(object context)
	{
		bool checkedState = (bool)context;
		SetCollectionChecked(TimelineBindings, checkedState);
	}

	private void ExecuteCheckTriggersCommand(object context)
	{
		bool checkedState = (bool)context;
		SetCollectionChecked(Timelines, checkedState);
	}

	private void SetCollectionChecked(IEnumerable<SelectableItemViewModel> collection, bool checkedState)
	{
		collection.ForEach(delegate(SelectableItemViewModel item)
		{
			item.IsSelected = checkedState;
		});
	}
}
