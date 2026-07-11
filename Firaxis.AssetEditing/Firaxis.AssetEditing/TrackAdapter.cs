using System;
using System.Collections.Generic;
using System.ComponentModel;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class TrackAdapter : AnimatableComponentAdapterBase, IPropertyEditingContext, IObservableContext
{
	private ITrack m_track;

	private TriggerType m_type = TriggerType.TT_COUNT;

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.TrackType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TrackType.NameAttribute, value);
		}
	}

	public int Index
	{
		get
		{
			return GetAttribute<int>(EntitySchema.TrackType.IndexAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.TrackType.IndexAttribute, value);
		}
	}

	public ITrack Track
	{
		get
		{
			return m_track;
		}
		set
		{
			bool flag = m_track != value;
			bool flag2 = string.IsNullOrEmpty(m_track?.Name) && !string.IsNullOrEmpty(Name);
			if (!flag && !flag2)
			{
				return;
			}
			BugSubmitter.SilentAssert(!flag || m_track == null || value == null, "What is about to cross link our track> @assign bwhitman");
			BugSubmitter.SilentAssert(value == null || value.Type == m_type, "What is about to cross link our track? @assign bwhitman");
			if (flag)
			{
				m_track = value;
			}
			if (m_track != null)
			{
				if (flag2)
				{
					m_track.Name = Name;
				}
				BugSubmitter.SilentAssert(m_track.Type == m_type, "What is cross linking our track? @assign bwhitman");
			}
		}
	}

	public TriggerType TriggerType
	{
		get
		{
			return m_type;
		}
		set
		{
			if (m_type != value)
			{
				BugSubmitter.SilentAssert(m_type == TriggerType.TT_COUNT, "This should only ever be set once and it defaults to TriggerType.TT_COUNT @assign bwhitman");
				m_type = value;
			}
		}
	}

	public IList<TriggerAdapter> Triggers { get; } = new List<TriggerAdapter>();

	public TimelineAdapter Timeline { get; private set; }

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

	public TriggerAdapter AddTrigger(float time)
	{
		TriggerAdapter triggerAdapter = Timeline.AddTrigger(time, TriggerType, Index);
		triggerAdapter.TrackAdapter = this;
		Triggers.Add(triggerAdapter);
		return triggerAdapter;
	}

	public TriggerAdapter AddTriggerCopyWithOffset(float offset, ITrigger trig)
	{
		TriggerAdapter triggerAdapter = AddTrigger(trig.StartTime + offset);
		triggerAdapter.UpdateAdapterDataOnlyFromNative(trig);
		return triggerAdapter;
	}

	public virtual void Update(ITrack track)
	{
		Track = track;
		Name = track.Name;
		TriggerType = track.Type;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		Timeline = GetParentAs<TimelineAdapter>();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (Track != null)
		{
			if (e.AttributeInfo == EntitySchema.TrackType.NameAttribute)
			{
				Track.Name = (string)e.NewValue;
			}
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}
	}

	public TrackAdapter()
	{
		if (this.ItemInserted != null && this.ItemRemoved != null && this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}
}
