using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml;
using Firaxis.Asset.Trigger;
using Firaxis.AssetBrowser;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

[DisplayName("Asset VFX")]
[ColorProvider(0, 0, 255)]
public class TriggerAssetVFX : TriggerBase, ITriggerEffect, ITriggerBase, IAssetBrowserAllowedClassProvider
{
	private string m_Event;

	[Category("Behavior")]
	[Description("Name of the attachment point associated with this trigger")]
	[Editor(typeof(AttachmentPointNameEditor), typeof(UITypeEditor))]
	public string AttachmentPoint { get; set; }

	[Category("Behavior")]
	[DisplayName("Asset")]
	[Description("Name of the asset to associate with this trigger")]
	[Editor(typeof(AssetBrowserAssetNameEditor), typeof(UITypeEditor))]
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
	public IEnumerable<string> AllowedClasses { get; set; }

	public TriggerAssetVFX(ITriggerSystem owner)
		: base(owner)
	{
		AttachmentPoint = string.Empty;
		Event = string.Empty;
		IEnumerable<string> allowedClasses = new List<string>();
		if (owner.TriggerTypeToValidClassesMapping != null && owner.TriggerTypeToValidClassesMapping.ContainsKey(TriggerType.TT_ASSET_VFX))
		{
			allowedClasses = owner.TriggerTypeToValidClassesMapping[TriggerType.TT_ASSET_VFX];
		}
		AllowedClasses = allowedClasses;
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
