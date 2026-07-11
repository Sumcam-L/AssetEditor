using System.ComponentModel;
using System.Xml;
using Firaxis.Asset.Trigger;
using Firaxis.Collections;
using Firaxis.Reflection;
using Firaxis.Utility;
using Firaxis.Utility.Converters;

namespace Firaxis.Asset;

public class TriggerBase : ITrigger, IUniqueID, ITriggerTrackInfo, ISerializableXml
{
	public const string EffectNamePrefix = "ART_DEF_VEFFECT_";

	[Browsable(false)]
	public ITriggerSystem Owner { get; private set; }

	[Category("Behavior")]
	[TypeConverter(typeof(TimeCodeConverter))]
	[Description("Time trigger will activate. Enter in seconds or frame notation")]
	public float Time { get; set; }

	[Category("Behavior")]
	[Description("Time in seconds for the trigger to continually fire")]
	public float Duration { get; set; }

	[Browsable(false)]
	public bool Repeat { get; set; }

	[Browsable(false)]
	public string TrackID { get; set; }

	[Browsable(false)]
	public int SubTrackID { get; set; }

	[Browsable(false)]
	public int ID { get; set; }

	[Category("Behavior")]
	[Description("Time in seconds notation")]
	public float TimeSeconds => Time;

	[Category("Appearance")]
	[DisplayName("ID")]
	public int IdEditor => ID;

	[Category("Appearance")]
	[DisplayName("Type")]
	public string TypeName => ReflectionHelper.GetDisplayName(this);

	public TriggerBase(ITriggerSystem owner)
	{
		Owner = owner;
	}

	public override string ToString()
	{
		return $"ID:{ID} {ReflectionHelper.GetDisplayName(this)}";
	}

	public virtual void Load(XmlDoc doc, XmlNode node)
	{
		Time = doc.GetAttrib<float>(node, "Time");
		Duration = doc.GetAttrib<float>(node, "Duration");
		TrackID = doc.GetAttrib<string>(node, "TrackID");
		SubTrackID = doc.GetAttrib<int>(node, "SubTrackID");
	}

	public virtual void Save(XmlDoc doc, XmlNode node)
	{
		doc.SetAttrib(node, "Time", Time);
		doc.SetAttrib(node, "Duration", Duration);
		doc.SetAttrib<string>(node, "TrackID", TrackID);
		doc.SetAttrib(node, "SubTrackID", SubTrackID);
	}
}
