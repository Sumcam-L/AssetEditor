using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Firaxis.Reflection;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TimelineAdapter : AnimatableComponentAdapterBase, IObservableContext
{
	private ITimeline m_timeline;

	private static IDictionary<TriggerType, DomNodeType> s_triggerTypeToDomNodeType;

	public string AnimationName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TimelineType.AnimationNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineType.AnimationNameAttribute, value);
		}
	}

	public string Description
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TimelineType.DescriptionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineType.DescriptionAttribute, value);
		}
	}

	public float Duration
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TimelineType.DurationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineType.DurationAttribute, value);
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TimelineType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineType.NameAttribute, value);
		}
	}

	public ITimeline Timeline
	{
		get
		{
			return m_timeline;
		}
		set
		{
			m_timeline = value;
			if (string.IsNullOrEmpty(m_timeline.Name) && !string.IsNullOrEmpty(Name))
			{
				m_timeline.Name = Name;
			}
		}
	}

	public IList<TriggerAdapter> Triggers { get; private set; }

	public IList<TrackAdapter> Tracks { get; private set; }

	private new IBehaviorProviderAdapter BehaviorProvider { get; set; }

	public IList<LegacyTriggerAdapter> LegacyTriggers { get; private set; }

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	static TimelineAdapter()
	{
		s_triggerTypeToDomNodeType = new Dictionary<TriggerType, DomNodeType>();
		s_triggerTypeToDomNodeType[TriggerType.TT_SOUND] = EntitySchema.SoundTriggerType.Type;
		s_triggerTypeToDomNodeType[TriggerType.TT_ASSET_VFX] = EntitySchema.AssetFXTriggerType.Type;
		s_triggerTypeToDomNodeType[TriggerType.TT_TRANSFER] = EntitySchema.TransferTriggerType.Type;
		s_triggerTypeToDomNodeType[TriggerType.TT_ACTION] = EntitySchema.ActionTriggerType.Type;
		s_triggerTypeToDomNodeType[TriggerType.TT_ARTDEF_VFX] = EntitySchema.ArtDefFXTriggerType.Type;
		s_triggerTypeToDomNodeType[TriggerType.TT_LIGHT] = EntitySchema.LightTriggerType.Type;
	}

	public TimelineAdapter()
	{
		_ = this.Reloaded;
	}

	public void AddDefaultTracks()
	{
		for (TriggerType triggerType = TriggerType.TT_SOUND; triggerType < TriggerType.TT_COUNT; triggerType++)
		{
			AddTrack(triggerType, GetDefaultTrackTypeName(triggerType));
		}
	}

	public TriggerAdapter AddTrigger(float trigTime, TriggerType trigType, int trackIndex)
	{
		string nextTriggerID = GetNextTriggerID();
		TriggerAdapter triggerAdapter = CreateTrigger(trigType);
		LegacyTriggerAdapter legacyTriggerAdapter = CreateLegacyTrigger(trigType);
		legacyTriggerAdapter.Adapter = triggerAdapter;
		triggerAdapter.LegacyAdapter = legacyTriggerAdapter;
		triggerAdapter.Name = nextTriggerID;
		Triggers.Add(triggerAdapter);
		triggerAdapter.TrackIndex = trackIndex;
		triggerAdapter.StartTime = trigTime;
		legacyTriggerAdapter.Name = nextTriggerID;
		LegacyTriggers.Add(legacyTriggerAdapter);
		legacyTriggerAdapter.TrackIndex = trackIndex;
		legacyTriggerAdapter.StartTime = trigTime;
		triggerAdapter.LegacyAdapter = legacyTriggerAdapter;
		legacyTriggerAdapter.Adapter = triggerAdapter;
		return triggerAdapter;
	}

	public void RemoveTrigger(TriggerAdapter trigger)
	{
		LegacyTriggers.RemoveIf((LegacyTriggerAdapter leg) => leg.Name == trigger.Name && leg.TriggerType == trigger.TriggerType && leg.TrackIndex == trigger.TrackIndex);
		Triggers.RemoveIf((TriggerAdapter trig) => trig.Name == trigger.Name && trig.TriggerType == trigger.TriggerType && trig.TrackIndex == trigger.TrackIndex);
	}

	public void RemoveTrack(TrackAdapter track)
	{
		TriggerType triggerType = track.TriggerType;
		int index = track.Index;
		TriggerAdapter[] array = track.Triggers.ToArray();
		foreach (TriggerAdapter trigger in array)
		{
			RemoveTrigger(trigger);
		}
		Tracks.Remove(track);
		foreach (TrackAdapter track2 in Tracks)
		{
			if (track2.TriggerType != triggerType || track2.Index < index)
			{
				continue;
			}
			int index2 = track2.Index - 1;
			track2.Index = index2;
			foreach (TriggerAdapter trigger2 in track2.Triggers)
			{
				trigger2.TrackIndex = track2.Index;
			}
		}
	}

	public TrackAdapter AddTrack(TriggerType trigType)
	{
		return AddTrack(trigType, GetDefaultTrackTypeName(trigType));
	}

	public TrackAdapter AddTrack(TriggerType trigType, string trackName)
	{
		int num = Tracks.Count((TrackAdapter trk) => trk.TriggerType == trigType);
		int num2 = -1;
		if (num > 0)
		{
			num2 = Tracks.Where((TrackAdapter tt) => tt.TriggerType == trigType).Max((TrackAdapter tm) => tm.Index);
		}
		BugSubmitter.SilentAssert(num - 1 == num2, "Non-contiguous track Indexes found on timeline \"{0}\" for track type \"{1}\" @summary Non-contiguous track Indexes found on timeline @assign bwhitman", Name, ReflectionHelper.GetDisplayName(trigType));
		DomNode domNode = new DomNode(EntitySchema.TrackType.Type);
		domNode.InitializeExtensions();
		TrackAdapter trackAdapter = domNode.As<TrackAdapter>();
		trackAdapter.Name = trackName;
		trackAdapter.Index = num2 + 1;
		trackAdapter.TriggerType = trigType;
		Tracks.Add(trackAdapter);
		return trackAdapter;
	}

	private int ComputeTrackIndexFromType(TriggerType triggerType, int trkIdx)
	{
		TrackAdapter trackAdapter = Tracks.FirstOrDefault((TrackAdapter ta) => ta.TriggerType == triggerType && ta.Index == trkIdx);
		if (trackAdapter == null)
		{
			trackAdapter = AddTrack(triggerType, GetDefaultTrackTypeName(triggerType));
		}
		return Tracks.IndexOf(trackAdapter);
	}

	public LegacyTriggerAdapter FindOrCreateLegacyTrigger(string name, TriggerType triggerType, float triggerTime, int trkIdx)
	{
		LegacyTriggerAdapter legacyTriggerAdapter = LegacyTriggers.FirstOrDefault((LegacyTriggerAdapter trig) => trig.Name == name && trig.TriggerType == triggerType);
		if (legacyTriggerAdapter == null)
		{
			int index = ComputeTrackIndexFromType(triggerType, trkIdx);
			TriggerAdapter triggerAdapter = Tracks[index].AddTrigger(triggerTime);
			triggerAdapter.Name = name;
			legacyTriggerAdapter = triggerAdapter.LegacyAdapter;
			legacyTriggerAdapter.Name = name;
		}
		return legacyTriggerAdapter;
	}

	public void Update(ITimeline timeline)
	{
		UnregisterDomNodeHandler();
		Name = timeline.Name;
		Timeline = timeline;
		Description = timeline.Description;
		AnimationName = timeline.AnimationName;
		if (!float.IsNaN(timeline.Duration))
		{
			Duration = timeline.Duration;
		}
		else
		{
			Duration = 1f;
		}
		Tracks.Clear();
		Triggers.Clear();
		LegacyTriggers.Clear();
		if (!timeline.Tracks.Any())
		{
			for (TriggerType triggerType = TriggerType.TT_SOUND; triggerType < TriggerType.TT_COUNT; triggerType++)
			{
				TrackAdapter trackAdapter = AddTrack(triggerType, GetDefaultTrackTypeName(triggerType));
				trackAdapter.Track = Timeline.AddTrack(trackAdapter.Name, trackAdapter.TriggerType);
			}
		}
		else
		{
			foreach (ITrack track in timeline.Tracks)
			{
				AddTrack(track.Type, track.Name).Update(track);
			}
		}
		foreach (ITrigger trigger in timeline.Triggers)
		{
			TrackAdapter trackAdapter2 = AddTrackIfNeeded(trigger);
			TriggerAdapter triggerAdapter = CreateTrigger(trigger.Type);
			LegacyTriggerAdapter legacyTriggerAdapter = CreateLegacyTrigger(trigger.Type);
			legacyTriggerAdapter.Adapter = triggerAdapter;
			triggerAdapter.LegacyAdapter = legacyTriggerAdapter;
			triggerAdapter.TrackAdapter = trackAdapter2;
			triggerAdapter.UpdateAdapterFromNative(trigger);
			Triggers.Add(triggerAdapter);
			AddTriggerToTrack(triggerAdapter);
			legacyTriggerAdapter.Update(trigger);
			LegacyTriggers.Add(legacyTriggerAdapter);
			triggerAdapter.LegacyAdapter = legacyTriggerAdapter;
			legacyTriggerAdapter.Adapter = triggerAdapter;
		}
		ValidateDuration();
		RegisterDomNodeHandlers();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		Triggers = new DomNodeListAdapter<TriggerAdapter>(base.DomNode, EntitySchema.TimelineType.TriggersChild);
		Tracks = new DomNodeListAdapter<TrackAdapter>(base.DomNode, EntitySchema.TimelineType.TracksChild);
		LegacyTriggers = new DomNodeListAdapter<LegacyTriggerAdapter>(base.DomNode, EntitySchema.TimelineType.LegacyTriggersChild);
		RegisterDomNodeHandlers();
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		BehaviorProvider = base.DomNode.GetRoot().As<IBehaviorProviderAdapter>();
	}

	private string GetNextTriggerID()
	{
		TimelineSetAdapter timelineSetAdapter = base.DomNode.Parent.As<TimelineSetAdapter>();
		int num = 0;
		foreach (TimelineAdapter timeline in timelineSetAdapter.Timelines)
		{
			foreach (TriggerAdapter trigger in timeline.Triggers)
			{
				int result = 0;
				if (!int.TryParse(trigger.Name, out result))
				{
					BugSubmitter.SilentReport($"Trigger with invalid name {trigger.Name} in timeline {timeline.Name} in entity {base.EntityAdapter.Name} of type {base.EntityAdapter.InstanceType} @assign bwhitman @summary Trigger with invalid name!");
				}
				else if (result > num)
				{
					num = result;
				}
			}
		}
		return (num + 1).ToString();
	}

	private void RegisterDomNodeHandlers()
	{
		UnregisterDomNodeHandler();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterDomNodeHandler()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	private void ValidateDuration()
	{
		IInstanceSet instances = base.EntityAdapter.Instances;
		string name = Name;
		string animationName = AnimationName;
		if (!string.IsNullOrEmpty(animationName))
		{
			IAnimationInstance animationInstance = instances.LoadEntityIfUnique<IAnimationInstance>(animationName);
			if (animationInstance != null)
			{
				if (animationInstance.Duration < 0f || animationInstance.Duration > 1000f)
				{
					Outputs.WriteLine(OutputMessageType.Error, "Animation {0} has a duration of {1}.  Its bound timeline ({2}) won't be displayed on the track.  Reimport the animation if this value is incorrect.", animationName, animationInstance.Duration, name);
				}
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Unable to load Animation {0} bound to timeline {1}.", animationName, name);
			}
		}
		else if (Duration < 0f || Duration > 1000f)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Timeline {0} has a duration of {1}.  It won't be displayed on the track.", name, Duration);
		}
	}

	private string GetDefaultTrackTypeName(TriggerType trigType)
	{
		return ReflectionHelper.GetDisplayName(trigType);
	}

	private TrackAdapter AddTrackIfNeeded(ITrigger trigger)
	{
		TrackAdapter trackAdapter = Tracks.FirstOrDefault((TrackAdapter ti) => ti.TriggerType == trigger.Type && ti.TriggerType == trigger.Type && ti.Index == trigger.TrackIndex);
		while (trackAdapter == null || trackAdapter.Index != trigger.TrackIndex)
		{
			trackAdapter = AddTrack(trigger.Type, GetDefaultTrackTypeName(trigger.Type));
			trackAdapter.Track = Timeline.AddTrack(trackAdapter.Name, trackAdapter.TriggerType);
		}
		return trackAdapter;
	}

	private void AddTriggerToTrack(TriggerAdapter triggerAdapter)
	{
		Tracks.Find((TrackAdapter trk) => trk.TriggerType == triggerAdapter.TriggerType && trk.Index == triggerAdapter.TrackIndex).Triggers.Add(triggerAdapter);
	}

	private LegacyTriggerAdapter CreateLegacyTrigger(TriggerType triggerType)
	{
		DomNode domNode = new DomNode(EntitySchema.LegacyTriggerType.Type);
		domNode.InitializeExtensions();
		LegacyTriggerAdapter legacyTriggerAdapter = domNode.As<LegacyTriggerAdapter>();
		legacyTriggerAdapter.TriggerType = triggerType;
		return legacyTriggerAdapter;
	}

	private TriggerAdapter CreateTrigger(TriggerType triggerType)
	{
		BugSubmitter.SilentAssert(s_triggerTypeToDomNodeType.ContainsKey(triggerType), "Trigger is a type we don't support DomNode editing on!");
		DomNode domNode = new DomNode(s_triggerTypeToDomNodeType[triggerType]);
		domNode.InitializeExtensions();
		TriggerAdapter triggerAdapter = domNode.As<TriggerAdapter>();
		triggerAdapter.TriggerType = triggerType;
		return triggerAdapter;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.TimelineType.AnimationNameAttribute)
		{
			Timeline.AnimationName = (string)e.NewValue;
			this.Reloaded.Raise(sender, new TimelineReloadEvent(this));
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			return;
		}
		if (e.AttributeInfo == EntitySchema.TimelineType.NameAttribute)
		{
			if (Timeline != null)
			{
				Timeline.Name = (string)e.NewValue;
			}
		}
		else if (e.AttributeInfo == EntitySchema.TimelineType.DescriptionAttribute)
		{
			Timeline.Description = (string)e.NewValue;
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
		}
		else if (e.AttributeInfo == EntitySchema.TimelineType.DurationAttribute)
		{
			if (string.IsNullOrEmpty(Timeline.AnimationName))
			{
				Timeline.Duration = (float)e.NewValue;
			}
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.Child.Parent != base.DomNode)
		{
			return;
		}
		TriggerAdapter triggerAdapter = e.Child.As<TriggerAdapter>();
		if (triggerAdapter != null)
		{
			ITrigger trigger = Timeline.FindTrigger(triggerAdapter.Name);
			if (trigger != null)
			{
				triggerAdapter.Trigger = trigger;
				triggerAdapter.UpdateAdapterFromNative(trigger);
			}
			else
			{
				trigger = Timeline.GetTrigger(triggerAdapter.Name, triggerAdapter.TriggerType);
				triggerAdapter.Trigger = trigger;
				triggerAdapter.UpdateNativeFromAdapter();
			}
		}
		else
		{
			LegacyTriggerAdapter legacyTriggerAdapter = e.Child.As<LegacyTriggerAdapter>();
			if (legacyTriggerAdapter != null)
			{
				ITrigger trigger2 = Timeline.GetTrigger(legacyTriggerAdapter.Name, legacyTriggerAdapter.TriggerType);
				legacyTriggerAdapter.Trigger = trigger2;
			}
			else
			{
				TrackAdapter trackAdapter = e.Child.As<TrackAdapter>();
				if (trackAdapter != null)
				{
					ITrack track = Timeline.AddTrack(trackAdapter.Name, trackAdapter.TriggerType);
					trackAdapter.Track = track;
				}
			}
		}
		if (e.Child.Is<LegacyTriggerAdapter>() || e.Child.Is<TriggerAdapter>() || e.Child.Is<TrackAdapter>())
		{
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child));
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		TriggerAdapter triggerAdapter = e.Child.As<TriggerAdapter>();
		if (triggerAdapter != null)
		{
			Timeline.RemoveTrigger(triggerAdapter.Name);
		}
		else
		{
			LegacyTriggerAdapter legacyTriggerAdapter = e.Child.As<LegacyTriggerAdapter>();
			if (legacyTriggerAdapter != null)
			{
				Timeline.RemoveTrigger(legacyTriggerAdapter.Name);
			}
			else
			{
				TrackAdapter trackAdapter = e.Child.As<TrackAdapter>();
				if (trackAdapter != null)
				{
					Timeline.RemoveTrack(trackAdapter.Track);
					trackAdapter.Track = null;
				}
			}
		}
		if (e.Child.Is<LegacyTriggerAdapter>() || e.Child.Is<TriggerAdapter>() || e.Child.Is<TrackAdapter>())
		{
			this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child));
			base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
		}
	}
}
