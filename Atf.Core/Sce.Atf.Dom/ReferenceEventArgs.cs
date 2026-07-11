using System;

namespace Sce.Atf.Dom;

public class ReferenceEventArgs : EventArgs
{
	public readonly DomNode Owner;

	public readonly AttributeInfo AttributeInfo;

	public readonly DomNode Target;

	public ReferenceEventArgs(DomNode owner, AttributeInfo attributeInfo, DomNode target)
	{
		Owner = owner;
		AttributeInfo = attributeInfo;
		Target = target;
	}
}
