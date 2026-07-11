using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class PrototypeFolder : DomNodeAdapter
{
	protected abstract AttributeInfo NameAttribute { get; }

	protected abstract ChildInfo PrototypeChildInfo { get; }

	protected abstract ChildInfo PrototypeFolderChildInfo { get; }

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

	public IList<PrototypeFolder> Folders => GetChildList<PrototypeFolder>(PrototypeFolderChildInfo);

	public IList<Prototype> Prototypes => GetChildList<Prototype>(PrototypeChildInfo);
}
