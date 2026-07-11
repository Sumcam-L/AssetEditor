using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TimelineBindingAdapter : BehaviorComponentAdapterBase, IPropertyEditingContext, IObservableContext, ITimelineBindingAdapter
{
	private TimelineAdapter TimelineAdapter => base.BehaviorProvider?.TimelineSet.FindTimeline(SlotName);

	public TimelineBindingType BindingType => TimelineBindingType.Timeline;

	public string SlotName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TimelineBindingType.SlotNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineBindingType.SlotNameAttribute, value);
		}
	}

	public string TimelineName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TimelineBindingType.TimelineNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineBindingType.TimelineNameAttribute, value);
		}
	}

	public float Duration
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TimelineBindingType.DurationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TimelineBindingType.DurationAttribute, value);
		}
	}

	public IEnumerable<object> Items
	{
		get
		{
			yield return base.DomNode;
		}
	}

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public void ClearBinding()
	{
		TimelineName = string.Empty;
	}

	public void Update(ITimelineBinding tlBinding)
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		SlotName = tlBinding.SlotName;
		TimelineName = tlBinding.TimelineName;
		Duration = TimelineAdapter?.Duration ?? 0f;
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public TimelineBindingAdapter()
	{
		if (this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.TimelineBindingType.DurationAttribute && TimelineAdapter != null)
		{
			TimelineAdapter.Duration = (float)e.NewValue;
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}
}
