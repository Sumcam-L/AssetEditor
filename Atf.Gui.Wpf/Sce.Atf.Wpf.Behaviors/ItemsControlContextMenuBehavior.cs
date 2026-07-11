using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public class ItemsControlContextMenuBehavior : ContextMenuBehavior
{
	protected override void OnAttached()
	{
		if (!(base.AssociatedObject is ItemsControl))
		{
			throw new InvalidOperationException("ItemsControlContextMenuBehavior can only be used on ItemsControl");
		}
		base.OnAttached();
	}

	protected override object GetCommandTarget(object sender, MouseButtonEventArgs e)
	{
		ItemsControl itemsControl = (ItemsControl)base.AssociatedObject;
		return itemsControl.GetItemAtPoint(e.GetPosition(itemsControl));
	}
}
