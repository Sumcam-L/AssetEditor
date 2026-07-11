using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Prototype : DomNodeAdapter
{
	protected abstract AttributeInfo NameAttribute { get; }

	protected abstract ChildInfo ElementChildInfo { get; }

	protected abstract ChildInfo WireChildInfo { get; }

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

	public IList<Element> Modules => GetChildList<Element>(ElementChildInfo);

	public IList<Wire> Connections => GetChildList<Wire>(WireChildInfo);
}
