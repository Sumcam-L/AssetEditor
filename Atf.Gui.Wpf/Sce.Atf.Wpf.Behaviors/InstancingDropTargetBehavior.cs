using System.Windows;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class InstancingDropTargetBehavior : DropTargetBehavior<FrameworkElement>
{
	protected override void OnDragOver(DragEventArgs e)
	{
		if (ApplicationUtil.CanInsert(base.AssociatedObject.DataContext, null, e.Data))
		{
			e.Effects = DragDropEffects.Move;
			e.Handled = true;
		}
		else
		{
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}
	}

	protected override void OnDrop(DragEventArgs e)
	{
		if (ApplicationUtil.CanInsert(base.AssociatedObject.DataContext, null, e.Data))
		{
			IStatusService exportedValueOrDefault = Composer.Current.Container.GetExportedValueOrDefault<IStatusService>();
			ApplicationUtil.Insert(base.AssociatedObject.DataContext, null, e.Data, "Drag Drop".Localize(), exportedValueOrDefault);
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}
	}
}
