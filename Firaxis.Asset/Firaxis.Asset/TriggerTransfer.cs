using System.ComponentModel;
using System.Xml;
using Firaxis.Asset.Trigger;
using Firaxis.Reflection;
using Firaxis.Utility;

namespace Firaxis.Asset;

[DisplayName("Transfer")]
[ColorProvider(255, 0, 0)]
public class TriggerTransfer : TriggerBase, ITransferTrigger
{
	[Category("Behavior")]
	[DisplayName("Effect")]
	[Description("Effect to transfer")]
	[TypeConverter(typeof(TriggerEffectRefTypeConverter))]
	public int RefID { get; set; }

	public TriggerTransfer(ITriggerSystem owner)
		: base(owner)
	{
	}

	public override void Load(XmlDoc doc, XmlNode node)
	{
		base.Load(doc, node);
		RefID = doc.GetAttrib<int>(node, "RefID");
	}

	public override void Save(XmlDoc doc, XmlNode node)
	{
		base.Save(doc, node);
		doc.SetAttrib(node, "RefID", RefID);
	}
}
