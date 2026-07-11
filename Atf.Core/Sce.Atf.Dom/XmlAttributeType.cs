using System;
using System.Xml.Schema;

namespace Sce.Atf.Dom;

public class XmlAttributeType : AttributeType
{
	private readonly XmlTypeCode m_xmlTypeCode;

	public XmlTypeCode XmlTypeCode => m_xmlTypeCode;

	public XmlAttributeType(string name, Type type, int length, XmlTypeCode xmlTypeCode)
		: base(name, type, length)
	{
		m_xmlTypeCode = xmlTypeCode;
	}

	public override string Convert(object value)
	{
		if (value != null && m_xmlTypeCode == XmlTypeCode.Base64Binary)
		{
			return System.Convert.ToBase64String((byte[])value);
		}
		return base.Convert(value);
	}

	public override object Convert(string s)
	{
		if (m_xmlTypeCode == XmlTypeCode.Base64Binary)
		{
			return System.Convert.FromBase64String(s);
		}
		return base.Convert(s);
	}
}
