using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Xml;
using Firaxis.Asset.Trigger;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

[DisplayName("Action")]
[ColorProvider(255, 128, 0)]
public class TriggerAction : TriggerBase, ITriggerAction, ITriggerBase, IColorProvider
{
	private string m_Event;

	[Category("Behavior")]
	[Description("Name of the attachment point on the model to attach the effect")]
	[Editor(typeof(AttachmentPointNameEditor), typeof(UITypeEditor))]
	public string AttachmentPoint { get; set; }

	[Category("Behavior")]
	[DisplayName("Action")]
	[Description("Name of the action to associate with this trigger")]
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

	[Browsable(false)]
	public Color Color
	{
		get
		{
			if (string.IsNullOrEmpty(AttachmentPoint))
			{
				return Color.FromArgb(255, 128, 0);
			}
			return Color.FromArgb(0, 128, 255);
		}
	}

	public TriggerAction(ITriggerSystem owner)
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
