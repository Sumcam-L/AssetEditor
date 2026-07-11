using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class ButtonPopupBehavior : Behavior<Button>
{
	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.Click += OnButtonClick;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.Click -= OnButtonClick;
	}

	private void OnButtonClick(object sender, RoutedEventArgs e)
	{
		if (sender is Button { ContextMenu: not null } button)
		{
			button.ContextMenu.PlacementTarget = button;
			button.ContextMenu.Placement = PlacementMode.Bottom;
			ContextMenuService.SetPlacement(button, PlacementMode.Bottom);
			button.ContextMenu.IsOpen = true;
		}
	}
}
