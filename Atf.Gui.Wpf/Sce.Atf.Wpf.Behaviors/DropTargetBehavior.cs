using System.Windows;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public abstract class DropTargetBehavior<T> : Behavior<T> where T : FrameworkElement
{
	protected virtual void OnDragEnter(DragEventArgs e)
	{
	}

	protected virtual void OnDragOver(DragEventArgs e)
	{
	}

	protected virtual void OnDrop(DragEventArgs e)
	{
	}

	protected virtual void OnDragLeave(DragEventArgs e)
	{
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.AllowDrop = true;
		base.AssociatedObject.Drop += OnDrop;
		base.AssociatedObject.DragEnter += OnDragEnter;
		base.AssociatedObject.DragLeave += OnDragLeave;
		base.AssociatedObject.DragOver += OnDragOver;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.Drop -= OnDrop;
		base.AssociatedObject.DragEnter -= OnDragEnter;
		base.AssociatedObject.DragLeave -= OnDragLeave;
		base.AssociatedObject.DragOver -= OnDragOver;
	}

	private void OnDragEnter(object sender, DragEventArgs e)
	{
		OnDragEnter(e);
	}

	private void OnDragLeave(object sender, DragEventArgs e)
	{
		e.Effects = DragDropEffects.None;
		OnDragLeave(e);
	}

	private void OnDragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void OnDrop(object sender, DragEventArgs e)
	{
		OnDrop(e);
	}
}
