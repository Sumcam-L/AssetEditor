using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class TriggerListFilteringContext : IPropertyEditingContext, IObservableContext, IAdaptable, IValidationContext
{
	private ISelectionContext SelectionContext { get; set; }

	private IValidationContext ValidationContext { get; set; }

	private IObservableContext ObservableContext { get; set; }

	private IBehaviorProviderAdapter TargetAdapter { get; set; }

	public virtual IEnumerable<object> Items
	{
		get
		{
			if (SelectionContext == null || SelectionContext.SelectionCount == 0)
			{
				return TargetAdapter.Items;
			}
			if (SelectionContext.SelectionCount == 1)
			{
				AnimationBindingAdapter aba = SelectionContext.LastSelected.As<AnimationBindingAdapter>();
				if (aba != null)
				{
					TimelineAdapter timelineAdapter = TargetAdapter.TimelineSet.Timelines.FirstOrDefault((TimelineAdapter itr) => itr.Name == aba.SlotName);
					if (timelineAdapter != null)
					{
						return timelineAdapter.Triggers;
					}
				}
				TimelineBindingAdapter tba = SelectionContext.LastSelected.As<TimelineBindingAdapter>();
				if (tba != null)
				{
					TimelineAdapter timelineAdapter2 = TargetAdapter.TimelineSet.Timelines.FirstOrDefault((TimelineAdapter itr) => itr.Name == tba.SlotName);
					if (timelineAdapter2 != null)
					{
						return timelineAdapter2.Triggers;
					}
				}
				TrackAdapter trackAdapter = SelectionContext.LastSelected.As<TrackAdapter>();
				if (trackAdapter != null)
				{
					return trackAdapter.Triggers;
				}
				TriggerAdapter triggerAdapter = SelectionContext.LastSelected.As<TriggerAdapter>();
				IEnumerable<TriggerAdapter> result;
				if (triggerAdapter != null)
				{
					IEnumerable<TriggerAdapter> enumerable = new TriggerAdapter[1] { triggerAdapter };
					result = enumerable;
				}
				else
				{
					result = Enumerable.Empty<TriggerAdapter>();
				}
				return result;
			}
			return from selObj in SelectionContext.Selection
				where selObj.Is<TriggerAdapter>()
				select selObj.As<TriggerAdapter>();
		}
	}

	public virtual IEnumerable<PropertyDescriptor> PropertyDescriptors => PropertyUtils.GetSharedProperties(Items);

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public event EventHandler Beginning;

	public event EventHandler Cancelled;

	public event EventHandler Ending;

	public event EventHandler Ended;

	public TriggerListFilteringContext(IBehaviorProviderAdapter adapter)
	{
		TargetAdapter = adapter;
		SelectionContext = TargetAdapter.As<ISelectionContext>();
		if (SelectionContext != null)
		{
			SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
		}
		ValidationContext = TargetAdapter.As<IValidationContext>();
		if (ValidationContext != null)
		{
			ValidationContext.Beginning += ValidationContext_Beginning;
			ValidationContext.Ending += ValidationContext_Ending;
			ValidationContext.Ended += ValidationContext_Ended;
			ValidationContext.Cancelled += ValidationContext_Cancelled;
		}
		ObservableContext = TargetAdapter.As<IObservableContext>();
		if (ObservableContext != null)
		{
			ObservableContext.ItemInserted += ObservableContext_ItemInserted;
			ObservableContext.ItemRemoved += ObservableContext_ItemRemoved;
			ObservableContext.ItemChanged += ObservableContext_ItemChanged;
		}
	}

	private void ObservableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		this.ItemChanged.Raise(sender, e);
	}

	private void ObservableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		this.ItemRemoved.Raise(sender, e);
	}

	private void ObservableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		this.ItemInserted.Raise(sender, e);
	}

	private void ValidationContext_Cancelled(object sender, EventArgs e)
	{
		this.Cancelled.Raise(sender, e);
	}

	private void ValidationContext_Ending(object sender, EventArgs e)
	{
		this.Ending.Raise(sender, e);
	}

	private void ValidationContext_Ended(object sender, EventArgs e)
	{
		this.Ended.Raise(sender, e);
	}

	private void ValidationContext_Beginning(object sender, EventArgs e)
	{
		this.Beginning.Raise(sender, e);
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		this.Reloaded.Raise(this, EventArgs.Empty);
	}

	public virtual object GetAdapter(Type type)
	{
		return TargetAdapter.As(type);
	}
}
