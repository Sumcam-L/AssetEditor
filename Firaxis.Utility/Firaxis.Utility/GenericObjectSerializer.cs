using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Firaxis.Utility;

public class GenericObjectSerializer : IXmlSerializable
{
	private object m_Data;

	public object Data => m_Data;

	public GenericObjectSerializer()
	{
		m_Data = null;
	}

	public GenericObjectSerializer(object data)
	{
		m_Data = data;
	}

	public void ReadXml(XmlReader reader)
	{
		reader.Read();
		Type type = Type.GetType(reader.GetAttribute("TypeName"));
		if (type != null)
		{
			reader.Read();
			StringReader textReader = new StringReader(reader.Value);
			XmlSerializer xmlSerializer = new XmlSerializer(type);
			m_Data = xmlSerializer.Deserialize(textReader);
		}
		else
		{
			m_Data = null;
		}
	}

	public void WriteXml(XmlWriter writer)
	{
		if (m_Data != null)
		{
			Type type = m_Data.GetType();
			string text = type.ToString();
			writer.WriteStartElement("Data");
			writer.WriteStartAttribute("TypeName");
			writer.WriteString(type.AssemblyQualifiedName);
			writer.WriteEndAttribute();
			StringWriter stringWriter = new StringWriter();
			XmlSerializer xmlSerializer = new XmlSerializer(type);
			xmlSerializer.Serialize(stringWriter, m_Data);
			writer.WriteCData(stringWriter.ToString());
			writer.WriteEndElement();
		}
		else
		{
			Type type2 = m_Data.GetType();
			writer.WriteStartElement("Data");
			writer.WriteStartAttribute("TypeName");
			writer.WriteString(string.Empty);
			writer.WriteEndAttribute();
		}
	}

	public XmlSchema GetSchema()
	{
		return null;
	}
}
