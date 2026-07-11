using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class LastHitAwareBehavior : Behavior<ItemsControl>
{
	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
		base.AssociatedObject.MouseUp += AssociatedObject_MouseUp;
		base.AssociatedObject.DragOver += AssociatedObject_DragOver;
		base.AssociatedObject.PreviewQueryContinueDrag += AssociatedObject_PreviewQueryContinueDrag;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
		base.AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
		base.AssociatedObject.DragOver -= AssociatedObject_DragOver;
		base.AssociatedObject.PreviewQueryContinueDrag -= AssociatedObject_PreviewQueryContinueDrag;
	}

	private void AssociatedObject_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		SetLastHit();
	}

	private void AssociatedObject_DragOver(object sender, DragEventArgs e)
	{
		SetLastHit();
	}

	private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
	{
		SetLastHit();
	}

	private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
	{
		SetLastHit();
	}

	private void SetLastHit()
	{
		ILastHitAware lastHitAware = base.AssociatedObject.DataContext.As<ILastHitAware>();
		if (lastHitAware != null)
		{
			lastHitAware.LastHit = base.AssociatedObject.GetItemAtMousePoint();
		}
	}
}
