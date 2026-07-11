using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LegacyTriggerAdapter : DomNodeAdapter
{
	private ITrigger m_trigger;

	public string AttachmentPointName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LegacyTriggerType.AttachmentPointNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.AttachmentPointNameAttribute, value);
		}
	}

	public string CollectionName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LegacyTriggerType.CollectionNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.CollectionNameAttribute, value);
		}
	}

	public float Duration
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LegacyTriggerType.DurationAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.DurationAttribute, value);
		}
	}

	public string FXName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LegacyTriggerType.FXNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.FXNameAttribute, value);
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LegacyTriggerType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.NameAttribute, value);
		}
	}

	public float StartTime
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LegacyTriggerType.StartTimeAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.StartTimeAttribute, value);
		}
	}

	public int TrackIndex
	{
		get
		{
			return GetAttribute<int>(EntitySchema.LegacyTriggerType.TrackIndexAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LegacyTriggerType.TrackIndexAttribute, value);
		}
	}

	public TriggerType TriggerType { get; set; }

	public TriggerAdapter Adapter { get; set; }

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

	public void Update(ITrigger trigger)
	{
		Trigger = trigger;
		if (Name != trigger.Name)
		{
			Name = trigger.Name;
		}
		if (FXName != trigger.FXName)
		{
			FXName = trigger.FXName;
		}
		if (AttachmentPointName != trigger.AttachmentPointName)
		{
			AttachmentPointName = trigger.AttachmentPointName;
		}
		if (StartTime != trigger.StartTime)
		{
			StartTime = trigger.StartTime;
		}
		if (Duration != trigger.Duration)
		{
			Duration = trigger.Duration;
		}
		if (CollectionName != trigger.CollectionName)
		{
			CollectionName = trigger.CollectionName;
		}
		if (TriggerType != trigger.Type)
		{
			TriggerType = trigger.Type;
		}
		if (TrackIndex != trigger.TrackIndex)
		{
			TrackIndex = trigger.TrackIndex;
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (Trigger != null)
		{
			if (e.AttributeInfo == EntitySchema.LegacyTriggerType.NameAttribute)
			{
				Trigger.Name = (string)e.NewValue;
			}
			else if (e.AttributeInfo == EntitySchema.LegacyTriggerType.FXNameAttribute)
			{
				Trigger.FXName = (string)e.NewValue;
			}
			else if (e.AttributeInfo == EntitySchema.LegacyTriggerType.AttachmentPointNameAttribute)
			{
				Trigger.AttachmentPointName = (string)e.NewValue;
			}
			else if (e.AttributeInfo == EntitySchema.LegacyTriggerType.StartTimeAttribute)
			{
				Trigger.StartTime = (float)e.NewValue;
			}
			else if (e.AttributeInfo == EntitySchema.LegacyTriggerType.DurationAttribute)
			{
				Trigger.Duration = (float)e.NewValue;
			}
			else if (e.AttributeInfo == EntitySchema.LegacyTriggerType.CollectionNameAttribute)
			{
				Trigger.CollectionName = (string)e.NewValue;
			}
			else if (e.AttributeInfo == EntitySchema.LegacyTriggerType.TrackIndexAttribute)
			{
				Trigger.TrackIndex = (int)e.NewValue;
			}
			Adapter.UpdateAdapterFromNative(Trigger);
		}
	}
}
