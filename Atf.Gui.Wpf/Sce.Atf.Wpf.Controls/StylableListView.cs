using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public class StylableListView : ListView
{
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		if (element is ListViewItem && base.View != null && base.View is GridView)
		{
			element.SetValue(FrameworkElement.StyleProperty, TryFindResource(GridView.GridViewItemContainerStyleKey));
		}
	}
}
