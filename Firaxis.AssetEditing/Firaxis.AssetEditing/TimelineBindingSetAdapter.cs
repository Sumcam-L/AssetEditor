using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TimelineBindingSetAdapter : BehaviorComponentAdapterBase
{
	public IList<TimelineBindingAdapter> TimelineBindings { get; private set; }

	public TimelineBindingAdapter AddBinding(string slotName, string timelineName)
	{
		BugSubmitter.Assert(!TimelineBindings.Any((TimelineBindingAdapter tba) => tba.SlotName == slotName), "Attempted to bind slot {0} to {1} but it is already bound! @summary Attempted to rebind slot @assign bwhitman", slotName, timelineName);
		DomNode domNode = new DomNode(EntitySchema.TimelineBindingType.Type);
		domNode.InitializeExtensions();
		TimelineBindingAdapter timelineBindingAdapter = domNode.As<TimelineBindingAdapter>();
		timelineBindingAdapter.SlotName = slotName;
		timelineBindingAdapter.TimelineName = timelineName;
		TimelineBindings.Add(timelineBindingAdapter);
		return timelineBindingAdapter;
	}

	public void RemoveBinding(string slotName)
	{
		BugSubmitter.Assert(TimelineBindings.Any((TimelineBindingAdapter tba) => tba.SlotName == slotName), "Attempted to remove binding in  slot {0} but it isnt bound! @summary Attempted to unbind slot that isnt bound @assign bwhitman", slotName);
		TimelineBindings.Remove(TimelineBindings.First((TimelineBindingAdapter b) => b.SlotName == slotName));
	}

	public bool HasTimelineSlot(string slotName)
	{
		return TimelineBindings.Any((TimelineBindingAdapter tba) => tba.SlotName == slotName);
	}

	public TimelineBindingAdapter FindBinding(string slotName)
	{
		return TimelineBindings.FirstOrDefault((TimelineBindingAdapter tba) => tba.SlotName == slotName);
	}

	public TimelineBindingAdapter FindOrCreateBinding(string slotName, string timelineName)
	{
		TimelineBindingAdapter timelineBindingAdapter = FindBinding(slotName);
		if (timelineBindingAdapter == null)
		{
			timelineBindingAdapter = AddBinding(slotName, timelineName);
		}
		return timelineBindingAdapter;
	}

	public void Update()
	{
		UnregisterFromDomChanges();
		IList<TimelineBindingAdapter> list = new List<TimelineBindingAdapter>();
		foreach (ITimelineBinding binding in base.BehaviorProvider.BehaviorData.TimelineBindings.Bindings)
		{
			TimelineBindingAdapter timelineBindingAdapter = FindOrCreateBinding(binding.SlotName, binding.TimelineName);
			timelineBindingAdapter.Update(binding);
			list.Add(timelineBindingAdapter);
		}
		foreach (var entryAdapter in TimelineBindings.Except(list).ToArray())
		{
			TimelineBindings.Remove(entryAdapter);
		}
		RegisterForDomChanges();
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		TimelineBindings = new DomNodeListAdapter<TimelineBindingAdapter>(base.DomNode, EntitySchema.TimelineBindingSetType.TimelineBindingChild);
		RegisterForDomChanges();
	}

	protected virtual void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.TimelineBindingType.TimelineNameAttribute)
		{
			string text = (string)e.NewValue;
			TimelineBindingAdapter timelineBindingAdapter = sender.As<DomNode>().As<TimelineBindingAdapter>() ?? e.DomNode.As<TimelineBindingAdapter>();
			if (!string.IsNullOrEmpty(text))
			{
				base.BehaviorProvider.BehaviorData.TimelineBindings.Bind(timelineBindingAdapter.SlotName, text);
				base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			}
			else
			{
				base.BehaviorProvider.BehaviorData.TimelineBindings.Unbind(timelineBindingAdapter.SlotName);
				base.BatchChangelist?.CreateEntityChangedEvent(base.EntityAdapter.InstanceEntity.Type, base.EntityAdapter.InstanceEntity.Name);
			}
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.TimelineBindingSetType.TimelineBindingChild)
		{
			TimelineBindingAdapter timelineBindingAdapter = e.Child.As<TimelineBindingAdapter>();
			base.BehaviorProvider.BehaviorData.TimelineBindings.Bind(timelineBindingAdapter.SlotName, timelineBindingAdapter.TimelineName);
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.TimelineBindingSetType.TimelineBindingChild)
		{
			TimelineBindingAdapter timelineBindingAdapter = e.Child.As<TimelineBindingAdapter>();
			base.BehaviorProvider.BehaviorData.TimelineBindings.Unbind(timelineBindingAdapter.SlotName);
		}
	}
}
