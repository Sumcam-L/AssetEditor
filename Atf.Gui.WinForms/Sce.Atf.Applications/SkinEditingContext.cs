using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

internal class SkinEditingContext : EditingContext, ITreeView, IItemView
{
	public object Root => base.DomNode;

	public IEnumerable<object> GetChildren(object parent)
	{
		DomNode parentNode = parent.Cast<DomNode>();
		if (parentNode != DomNode)
		{
			yield break;
		}
		foreach (DomNode child in parentNode.Children)
		{
			yield return child;
		}
	}

	public void GetInfo(object item, ItemInfo info)
	{
		DomNode domNode = item.Cast<DomNode>();
		info.IsLeaf = true;
		info.AllowLabelEdit = false;
		if (domNode.Type.Equals(SkinSchema.styleType.Type))
		{
			info.Label = (string)domNode.GetAttribute(SkinSchema.styleType.nameAttribute);
		}
		else
		{
			info.Label = domNode.Type.Name;
		}
	}
}
