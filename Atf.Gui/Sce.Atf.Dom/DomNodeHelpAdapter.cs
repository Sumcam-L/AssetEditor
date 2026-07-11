using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class DomNodeHelpAdapter : DomNodeAdapter, IHelpContext
{
	public string[] GetHelpKeys()
	{
		foreach (DomNode item in base.DomNode.Lineage)
		{
			IHelpContext tag = item.Type.GetTag<IHelpContext>();
			if (tag != null)
			{
				return tag.GetHelpKeys();
			}
		}
		return null;
	}
}
