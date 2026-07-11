using System;

namespace Sce.Atf.Dom;

public abstract class Template : DomNodeAdapter, IReference<DomNode>
{
	public abstract string Name { get; set; }

	public abstract Guid Guid { get; set; }

	public abstract DomNode Target { get; set; }

	public abstract bool CanReference(DomNode item);
}
