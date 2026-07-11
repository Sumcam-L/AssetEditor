using System.ComponentModel;
using System.Drawing.Design;
using System.Xml;
using Firaxis.Asset.Trigger;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

[DisplayName("Sound")]
[ColorProvider(32, 255, 0)]
public class TriggerSound : TriggerBase, ITriggerSound, ITriggerBase
{
	private string m_Event;

	[Category("Behavior")]
	[Description("Name of the attachment point on the model to attach the effect")]
	[Editor(typeof(AttachmentPointNameEditor), typeof(UITypeEditor))]
	public string AttachmentPoint { get; set; }

	[Category("Behavior")]
	[DisplayName("WwiseEvent")]
	[Description("Name of the Wwise event to associate with this trigger")]
	[Editor(typeof(SoundEventTypeEditor), typeof(UITypeEditor))]
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

	public TriggerSound(ITriggerSystem owner)
		: base(owner)
	{
		AttachmentPoint = string.Empty;
		Event = string.Empty;
	}

	public override void Load(XmlDoc doc, XmlNode node)
	{
		base.Load(doc, node);
		AttachmentPoint = doc.GetAttrib(node, "AttachmentPoint");
		Event = doc.GetAttrib(node, "Event");
	}

	public override void Save(XmlDoc doc, XmlNode node)
	{
		base.Save(doc, node);
		doc.SetAttrib(node, "AttachmentPoint", AttachmentPoint);
		doc.SetAttrib(node, "Event", Event);
	}
}
