using System;

namespace Sce.Atf.Dom;

public class AttributeEventArgs : EventArgs
{
	public readonly DomNode DomNode;

	public readonly AttributeInfo AttributeInfo;

	public readonly object OldValue;

	public readonly object NewValue;

	public AttributeEventArgs(DomNode node, AttributeInfo attributeInfo, object oldValue, object newValue)
	{
		DomNode = node;
		AttributeInfo = attributeInfo;
		OldValue = oldValue;
		NewValue = newValue;
	}
}
