using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Firaxis.Asset.Properties;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TriggerAdapter : AnimatableComponentAdapterBase, IPropertyEditingContext, IObservableContext, INamedAdapter
{
	private ITrigger m_trigger;

	public TriggerType TriggerType { get; set; }

	public virtual Image TriggerImage => Firaxis.Asset.Properties.Resources.trigger;

	public TimelineAdapter TimelineAdapter { get; private set; }

	public TrackAdapter TrackAdapter { get; set; }

	public LegacyTriggerAdapter LegacyAdapter { get; set; }

	public ITrigger Trigger
	{
		get
		{
			return m_trigger;
		}
		internal set
		{
			m_trigger = value;
			if (string.IsNullOrEmpty(m_trigger.Name) && !string.IsNullOrEmpty(Name))
			{
				m_trigger.Name = Name;
			}
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TriggerType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TriggerType.NameAttribute, value);
		}
	}

	public float StartTime
	{
		get
		{
			return GetAttribute<float>(EntitySchema.TriggerType.StartTimeAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TriggerType.StartTimeAttribute, value);
		}
	}

	public int TrackIndex
	{
		get
		{
			return GetAttribute<int>(EntitySchema.TriggerType.TrackIndexAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TriggerType.TrackIndexAttribute, value);
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

	public virtual void UpdateNativeDataOnlyFromAdapter()
	{
	}

	public virtual void UpdateNativeFromAdapter()
	{
		BugSubmitter.Assert(Trigger != null, "Trigger native object is null!");
		Trigger.Name = Name;
		Trigger.StartTime = StartTime;
		Trigger.TrackIndex = TrackIndex;
		UpdateNativeDataOnlyFromAdapter();
	}

	public virtual void UpdateAdapterDataOnlyFromNative(ITrigger trigger)
	{
	}

	public virtual void UpdateAdapterFromNative(ITrigger trigger)
	{
		UnregisterDomHandlers();
		Trigger = trigger;
		TriggerType = trigger.Type;
		Name = trigger.Name;
		StartTime = trigger.StartTime;
		TrackIndex = trigger.TrackIndex;
		UpdateAdapterDataOnlyFromNative(trigger);
		RegisterDomHandlers();
	}

	public TriggerAdapter()
	{
		if (this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	private void RegisterDomHandlers()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void UnregisterDomHandlers()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RegisterDomHandlers();
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		TimelineAdapter = GetParentAs<TimelineAdapter>();
	}

	protected virtual void OnDomNodeAttributeChanged(AttributeEventArgs attr)
	{
		if (Trigger != null)
		{
			if (attr.AttributeInfo == EntitySchema.TriggerType.NameAttribute)
			{
				Trigger.Name = (string)attr.NewValue;
			}
			else if (attr.AttributeInfo == EntitySchema.TriggerType.StartTimeAttribute)
			{
				Trigger.StartTime = (float)attr.NewValue;
			}
			else if (attr.AttributeInfo == EntitySchema.TriggerType.TrackIndexAttribute)
			{
				Trigger.TrackIndex = (int)attr.NewValue;
			}
			base.BatchChangelist?.CreateAssetTimelineChangedEvent(base.EntityAdapter.InstanceEntity, TimelineAdapter.Timeline);
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		OnDomNodeAttributeChanged(e);
		if (LegacyAdapter != null && Trigger != null)
		{
			LegacyAdapter.Update(Trigger);
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		if (e.OldValue != e.NewValue)
		{
			_ = e.NewValue;
		}
	}
}
