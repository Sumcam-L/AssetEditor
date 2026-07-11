using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Controls;

public class TreeListViewItem : TreeViewItem
{
	private int m_level = -1;

	public int Level
	{
		get
		{
			if (m_level == -1)
			{
				TreeListViewItem treeListViewItem = ItemsControl.ItemsControlFromItemContainer(this) as TreeListViewItem;
				m_level = ((treeListViewItem != null) ? (treeListViewItem.Level + 1) : 0);
			}
			return m_level;
		}
	}

	static TreeListViewItem()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
	}

	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		base.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
	}

	protected override DependencyObject GetContainerForItemOverride()
	{
		return new TreeListViewItem();
	}

	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is TreeListViewItem;
	}

	private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
	{
		if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			BindingOperations.SetBinding(this, Control.HorizontalContentAlignmentProperty, new AncestorTypeBinding<ItemsControl>("HorizontalContentAlignment"));
			BindingOperations.SetBinding(this, Control.VerticalContentAlignmentProperty, new AncestorTypeBinding<ItemsControl>("VerticalContentAlignment"));
		}
	}
}
