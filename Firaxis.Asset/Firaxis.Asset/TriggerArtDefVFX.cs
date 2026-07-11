using System.ComponentModel;
using System.Drawing.Design;
using System.Xml;
using Firaxis.Asset.Trigger;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

[DisplayName("ArtDef VFX")]
[ColorProvider(64, 128, 128)]
public class TriggerArtDefVFX : TriggerBase, ITriggerEffect, ITriggerBase
{
	private string m_Event;

	private string m_CollectionName;

	[Category("Behavior")]
	[Description("Name of the attachment point associated with this trigger")]
	[Editor(typeof(AttachmentPointNameEditor), typeof(UITypeEditor))]
	public string AttachmentPoint { get; set; }

	[Category("Behavior")]
	[DisplayName("Element")]
	[Description("Name of the art definition element to associate with this trigger")]
	[TypeConverter(typeof(ArtDefinitionElementNameTypeConverter))]
	public string Event
	{
		get
		{
			return m_Event;
		}
		set
		{
			m_Event = value;
		}
	}

	[Category("Behavior")]
	[DisplayName("Collection")]
	[Description("Name of collection where the art definition element is defined")]
	[TypeConverter(typeof(ArtDefinitionCollectionNameTypeConverter))]
	public string CollectionName
	{
		get
		{
			return m_CollectionName;
		}
		set
		{
			m_CollectionName = value;
		}
	}

	public TriggerArtDefVFX(ITriggerSystem owner)
		: base(owner)
	{
		AttachmentPoint = string.Empty;
		Event = string.Empty;
		CollectionName = string.Empty;
	}

	public override void Load(XmlDoc doc, XmlNode node)
	{
		base.Load(doc, node);
		AttachmentPoint = doc.GetAttrib(node, "AttachmentPoint");
		Event = doc.GetAttrib(node, "Event");
		CollectionName = doc.GetAttrib(node, "CollectionName");
	}

	public override void Save(XmlDoc doc, XmlNode node)
	{
		base.Save(doc, node);
		doc.SetAttrib(node, "AttachmentPoint", AttachmentPoint);
		doc.SetAttrib(node, "Event", Event);
		doc.SetAttrib(node, "CollectionName", CollectionName);
	}
}
