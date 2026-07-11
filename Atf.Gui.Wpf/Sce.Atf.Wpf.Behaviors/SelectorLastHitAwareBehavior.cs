using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class SelectorLastHitAwareBehavior : Behavior<Selector>
{
	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.SelectionChanged += SetLastSelected;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.SelectionChanged -= SetLastSelected;
	}

	private void SetLastSelected(object sender, SelectionChangedEventArgs args)
	{
		ILastHitAware lastHitAware = base.AssociatedObject.DataContext.As<ILastHitAware>();
		if (lastHitAware != null)
		{
			lastHitAware.LastHit = ((args.AddedItems.Count > 0) ? args.AddedItems[args.AddedItems.Count - 1] : null);
		}
	}
}
