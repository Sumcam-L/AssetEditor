namespace Sce.Atf.Dom;

public struct XmlNodeReference
{
	public readonly DomNode Node;

	public readonly AttributeInfo AttributeInfo;

	public readonly string Value;

	public XmlNodeReference(DomNode node, AttributeInfo attributeInfo, string value)
	{
		Node = node;
		AttributeInfo = attributeInfo;
		Value = value;
	}
}
