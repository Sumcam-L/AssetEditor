using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class DomNodeAdapterExtensions
{
	public static AT CreateComponentAdapter<AT>(this DomNodeAdapter dn, DomNodeType dt, ChildInfo info) where AT : DomNodeAdapter
	{
		DomNode domNode = new DomNode(dt);
		domNode.InitializeExtensions();
		dn.DomNode.SetChild(info, domNode);
		return domNode.As<AT>();
	}
}
