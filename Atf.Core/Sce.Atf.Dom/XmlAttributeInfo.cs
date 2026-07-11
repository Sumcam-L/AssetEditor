namespace Sce.Atf.Dom;

public class XmlAttributeInfo : AttributeInfo
{
	private bool m_isElement;

	public bool IsElement
	{
		get
		{
			return m_isElement;
		}
		set
		{
			m_isElement = value;
		}
	}

	public XmlAttributeInfo(string name, AttributeType type)
		: base(name, type)
	{
	}
}
