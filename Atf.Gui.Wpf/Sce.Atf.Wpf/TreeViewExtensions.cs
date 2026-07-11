using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Sce.Atf.Wpf;

public static class TreeViewExtensions
{
	public static void ApplyActionToAllTreeViewItems(Action<TreeViewItem> itemAction, ItemsControl itemsControl)
	{
		Stack<ItemsControl> stack = new Stack<ItemsControl>();
		stack.Push(itemsControl);
		while (stack.Count != 0)
		{
			ItemsControl itemsControl2 = stack.Pop();
			if (itemsControl2 is TreeViewItem obj)
			{
				itemAction(obj);
			}
			if (itemsControl2 == null)
			{
				continue;
			}
			foreach (object item2 in (IEnumerable)itemsControl2.Items)
			{
				ItemsControl item = (ItemsControl)itemsControl2.ItemContainerGenerator.ContainerFromItem(item2);
				stack.Push(item);
			}
		}
	}

	public static IEnumerable<TreeViewItem> AllTreeViewItems(ItemsControl itemsControl)
	{
		Stack<ItemsControl> itemsControlStack = new Stack<ItemsControl>();
		itemsControlStack.Push(itemsControl);
		while (itemsControlStack.Count != 0)
		{
			ItemsControl currentItem = itemsControlStack.Pop();
			if (currentItem is TreeViewItem currentTreeViewItem)
			{
				yield return currentTreeViewItem;
			}
			if (currentItem == null)
			{
				continue;
			}
			foreach (object dataItem in (IEnumerable)currentItem.Items)
			{
				ItemsControl childElement = (ItemsControl)currentItem.ItemContainerGenerator.ContainerFromItem(dataItem);
				itemsControlStack.Push(childElement);
			}
		}
	}

	public static TreeViewItem GetTreeViewItem(ItemsControl container, object item)
	{
		return AllTreeViewItems(container).FirstOrDefault((TreeViewItem x) => x.DataContext == item);
	}
}
