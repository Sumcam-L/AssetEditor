using System.Windows;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public abstract class AdaptableBehavior<T> : Behavior<T> where T : DependencyObject
{
	protected override void OnAttached()
	{
		base.OnAttached();
		if (base.AssociatedObject is FrameworkElement control)
		{
			control.AsAdaptableControl().Attach(this);
		}
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		if (base.AssociatedObject is FrameworkElement control)
		{
			control.AsAdaptableControl().Detach(this);
		}
	}
}
