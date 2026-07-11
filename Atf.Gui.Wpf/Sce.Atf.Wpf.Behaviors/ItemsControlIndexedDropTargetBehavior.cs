using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class ItemsControlIndexedDropTargetBehavior : DropTargetBehavior<ItemsControl>
{
	protected override void OnDragEnter(DragEventArgs e)
	{
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		DependencyObject dependencyObject = (DependencyObject)e.OriginalSource;
		FrameworkElement frameworkElement = null;
		frameworkElement = ((!(base.AssociatedObject is TreeView)) ? (base.AssociatedObject.ContainerFromElement(dependencyObject) as FrameworkElement) : dependencyObject.FindAncestor<TreeViewItem>());
		object parent = ((frameworkElement != null) ? frameworkElement.DataContext : base.AssociatedObject.DataContext);
		if (ApplicationUtil.CanInsert(base.AssociatedObject.DataContext, parent, e.Data))
		{
			if ((e.AllowedEffects & DragDropEffects.Copy) > DragDropEffects.None && (e.KeyStates & DragDropKeyStates.ControlKey) > DragDropKeyStates.None)
			{
				e.Effects = DragDropEffects.Copy;
			}
			else
			{
				e.Effects = DragDropEffects.Move;
			}
			e.Handled = true;
		}
	}

	protected override void OnDrop(DragEventArgs e)
	{
		Point position = e.GetPosition(base.AssociatedObject);
		object obj = base.AssociatedObject.GetItemAtPoint(position);
		if (obj == null)
		{
			obj = base.AssociatedObject.DataContext;
		}
		if (ApplicationUtil.CanInsert(base.AssociatedObject.DataContext, obj, e.Data))
		{
			IStatusService exportedValueOrDefault = Composer.Current.Container.GetExportedValueOrDefault<IStatusService>();
			ApplicationUtil.Insert(base.AssociatedObject.DataContext, obj, e.Data, "Drag Drop".Localize(), exportedValueOrDefault);
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}
	}
}
