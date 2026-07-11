using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TimelineSetAdapter : BehaviorComponentAdapterBase
{
	public IList<TimelineAdapter> Timelines { get; private set; }

	public void RemoveTimeline(string timelineName)
	{
		TimelineAdapter timelineAdapter = FindTimeline(timelineName);
		BugSubmitter.Assert(timelineAdapter != null, "Timeline must exist to be removed");
		TrackAdapter[] array = timelineAdapter.Tracks.ToArray();
		foreach (TrackAdapter track in array)
		{
			timelineAdapter.RemoveTrack(track);
		}
		Timelines.Remove(timelineAdapter);
	}

	public TimelineAdapter FindOrCreateTimeline(string name)
	{
		TimelineAdapter timelineAdapter = FindTimeline(name);
		if (timelineAdapter == null)
		{
			timelineAdapter = AddTimeline(name);
		}
		return timelineAdapter;
	}

	public TimelineAdapter AddTimeline(string name)
	{
		TimelineAdapter timelineAdapter = AddTimeline();
		timelineAdapter.Name = name;
		Timelines.Add(timelineAdapter);
		timelineAdapter.AddDefaultTracks();
		base.BehaviorProvider.TimelineBindingSet.FindOrCreateBinding(timelineAdapter.Name, timelineAdapter.Name);
		return timelineAdapter;
	}

	public TimelineAdapter FindTimeline(string name)
	{
		return Timelines.FirstOrDefault((TimelineAdapter tl) => tl.Name == name);
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		IList<TimelineAdapter> list = new List<TimelineAdapter>();
		foreach (ITimeline timeline in base.BehaviorProvider.BehaviorData.Timelines.Timelines)
		{
			string timelineName = timeline.Name;
			TimelineAdapter timelineAdapter = Timelines.FirstOrDefault((TimelineAdapter tl) => tl.Name == timelineName);
			if (timelineAdapter == null)
			{
				timelineAdapter = AddTimeline();
				timelineAdapter.Timeline = timeline;
				timelineAdapter.Name = timelineName;
				Timelines.Add(timelineAdapter);
			}
			ITimelineBinding timelineBinding = base.BehaviorProvider.BehaviorData.TimelineBindings.Bindings.FirstOrDefault((ITimelineBinding binding) => binding.TimelineName == timelineName);
			if (timelineBinding != null)
			{
				string animationName = base.BehaviorProvider.GetAnimationName(timelineBinding.SlotName);
				if (!string.IsNullOrEmpty(animationName))
				{
					timeline.AnimationName = animationName;
				}
			}
			timelineAdapter.Update(timeline);
			list.Add(timelineAdapter);
		}
		foreach (var entryAdapter in Timelines.Except(list).ToArray())
		{
			Timelines.Remove(entryAdapter);
		}
		RegisterForDomChanges();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		Timelines = new DomNodeListAdapter<TimelineAdapter>(base.DomNode, EntitySchema.TimelineSetType.TimelineChild);
		RegisterForDomChanges();
	}

	private TimelineAdapter AddTimeline()
	{
		DomNode domNode = new DomNode(EntitySchema.TimelineType.Type);
		domNode.InitializeExtensions();
		return domNode.As<TimelineAdapter>();
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.TimelineSetType.TimelineChild)
		{
			TimelineAdapter tla = e.Child.As<TimelineAdapter>();
			tla.Timeline = base.BehaviorProvider.BehaviorData.Timelines.AddTimeline();
			ITimelineBinding timelineBinding = base.BehaviorProvider.BehaviorData.TimelineBindings.Bindings.FirstOrDefault((ITimelineBinding binding) => binding.TimelineName == tla.Name);
			if (timelineBinding != null && !string.IsNullOrEmpty(base.BehaviorProvider.GetAnimationName(timelineBinding.SlotName)))
			{
				base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			}
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.TimelineSetType.TimelineChild)
		{
			string name = e.Child.As<TimelineAdapter>()?.Name;
			base.BehaviorProvider.BehaviorData.Timelines.RemoveTimeline(name);
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
		}
	}

	private void RegisterForDomChanges()
	{
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterFromDomChanges()
	{
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
	}
}
