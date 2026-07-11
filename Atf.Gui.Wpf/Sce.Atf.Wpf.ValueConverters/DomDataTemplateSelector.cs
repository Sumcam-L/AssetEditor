using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Wpf.ValueConverters;

public class DomDataTemplateSelector : DataTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, DependencyObject container)
	{
		DomNode domNode = item.As<DomNode>();
		if (domNode != null)
		{
			DataTemplate tag = domNode.Type.GetTag<DataTemplate>();
			if (tag != null)
			{
				return tag;
			}
		}
		return base.SelectTemplate(item, container);
	}
}
