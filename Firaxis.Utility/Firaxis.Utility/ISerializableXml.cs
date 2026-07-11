using System.Xml;

namespace Firaxis.Utility;

public interface ISerializableXml
{
	void Load(XmlDoc doc, XmlNode node);

	void Save(XmlDoc doc, XmlNode node);
}
