using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class ElementRef : DomNodeAdapter
{
	protected abstract AttributeInfo RefAttribute { get; }

	public Element Element
	{
		get
		{
			return GetReference<Element>(RefAttribute);
		}
		set
		{
			SetReference(RefAttribute, value);
		}
	}
}
