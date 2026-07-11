using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Interop;

public static class ContextMenuServiceExtensions
{
	public static void RunContextMenu(this IContextMenuService service, IEnumerable<object> commandTags)
	{
		ContextMenu contextMenu = service.GetContextMenu(commandTags);
		contextMenu.Placement = PlacementMode.MousePoint;
		OpenContextMenuIfNotEmpty(contextMenu);
	}

	public static void RunContextMenu(this IContextMenuService service, IEnumerable<object> commandTags, Point screenOffset)
	{
		ContextMenu contextMenu = service.GetContextMenu(commandTags);
		contextMenu.Placement = PlacementMode.AbsolutePoint;
		contextMenu.HorizontalOffset = screenOffset.X;
		contextMenu.VerticalOffset = screenOffset.Y;
		OpenContextMenuIfNotEmpty(contextMenu);
	}

	public static void RunContextMenu(this IContextMenuService service, IEnumerable<object> commandTags, UIElement element, Point offset)
	{
		ContextMenu contextMenu = service.GetContextMenu(commandTags);
		contextMenu.Placement = PlacementMode.Relative;
		contextMenu.PlacementTarget = element;
		contextMenu.HorizontalOffset = offset.X;
		contextMenu.VerticalOffset = offset.Y;
		OpenContextMenuIfNotEmpty(contextMenu);
	}

	private static void OpenContextMenuIfNotEmpty(ContextMenu menu)
	{
		IEnumerator enumerator = menu.ItemsSource.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				menu.IsOpen = true;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
