using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public class TreeListView : TreeView
{
	private GridViewColumnCollection m_columns;

	public GridViewColumnCollection Columns
	{
		get
		{
			if (m_columns == null)
			{
				m_columns = new GridViewColumnCollection();
			}
			return m_columns;
		}
	}

	static TreeListView()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
	}

	public bool SetSelectedItem(object item)
	{
		if (item == null)
		{
			return false;
		}
		TreeListViewItem treeListViewItem = (TreeListViewItem)base.ItemContainerGenerator.ContainerFromItem(item);
		if (treeListViewItem != null)
		{
			treeListViewItem.IsSelected = true;
			return true;
		}
		return false;
	}

	protected override DependencyObject GetContainerForItemOverride()
	{
		return new TreeListViewItem();
	}

	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is TreeListViewItem;
	}
}
