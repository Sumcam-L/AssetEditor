using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class DSGAdapter : InstanceEntityAdapter
{
	private IList<TextElementAdapter> m_animationSlots;

	private IList<TextElementAdapter> m_timelineSlots;

	public IList<TextElementAdapter> AnimationSlots => m_animationSlots;

	public IDSGInstance DSG => InstanceEntity as IDSGInstance;

	public IList<TextElementAdapter> TimelineSlots => m_timelineSlots;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.DSGEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.DSGEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.DSGEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.DSGEntityType.DescriptionAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.DSGEntityType.NameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.DSGEntityType.TagsChild;

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		UpdateTextList(DSG, DSG.GetAnimationSlots(), AnimationSlots, DSG.AddAnimationSlot, DSG.RemoveAnimationSlot);
		UpdateTextList(DSG, DSG.GetTimelineSlots(), TimelineSlots, DSG.AddTimelineSlot, DSG.RemoveTimelineSlot);
	}

	protected override void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		base.DomNode.As<EditingContext>();
		if (e.ChildInfo.Type == BaseSchema.TextElementType.Type)
		{
			TextElementAdapter textElementAdapter = e.Child.As<TextElementAdapter>();
			if (e.ChildInfo.Name == "AnimationSlots")
			{
				DSG.RemoveAnimationSlot(textElementAdapter.Text);
			}
			else if (e.ChildInfo.Name == "TimelineSlots")
			{
				DSG.RemoveTimelineSlot(textElementAdapter.Text);
			}
			else
			{
				base.HandleDomNodeChildRemoved(sender, e);
			}
		}
		else
		{
			base.HandleDomNodeChildRemoved(sender, e);
		}
	}

	protected override void OnNodeSet()
	{
		m_animationSlots = new DomNodeListAdapter<TextElementAdapter>(base.DomNode, EntitySchema.DSGEntityType.AnimationSlotsChild);
		m_timelineSlots = new DomNodeListAdapter<TextElementAdapter>(base.DomNode, EntitySchema.DSGEntityType.TimelineSlotsChild);
		base.OnNodeSet();
	}
}
