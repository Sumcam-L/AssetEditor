using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class LayerFolder : DomNodeAdapter
{
	protected abstract AttributeInfo NameAttribute { get; }

	protected abstract ChildInfo LayerFolderChild { get; }

	protected abstract ChildInfo ElementRefChildInfo { get; }

	public string Name
	{
		get
		{
			return (string)base.DomNode.GetAttribute(NameAttribute);
		}
		set
		{
			base.DomNode.SetAttribute(NameAttribute, value);
		}
	}

	public IList<LayerFolder> Folders => GetChildList<LayerFolder>(LayerFolderChild);

	public IList<ElementRef> ElementRefs => GetChildList<ElementRef>(ElementRefChildInfo);

	public IEnumerable<Element> GetElements()
	{
		foreach (ElementRef reference in ElementRefs)
		{
			yield return reference.Element;
		}
	}

	public bool Contains(Element element)
	{
		foreach (ElementRef elementRef in ElementRefs)
		{
			if (elementRef.Element == element)
			{
				return true;
			}
		}
		return false;
	}
}
