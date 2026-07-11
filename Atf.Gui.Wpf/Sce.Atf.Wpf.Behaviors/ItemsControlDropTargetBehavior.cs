using System.Windows;
using System.Windows.Controls;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class ItemsControlDropTargetBehavior : DropTargetBehavior<ItemsControl>
{
	public static readonly DependencyProperty RootTargetProperty = DependencyProperty.Register("RootTarget", typeof(object), typeof(ItemsControlDropTargetBehavior), new PropertyMetadata((object)null));

	public object RootTarget
	{
		get
		{
			return GetValue(RootTargetProperty);
		}
		set
		{
			SetValue(RootTargetProperty, value);
		}
	}

	protected virtual object GetDropTarget(DragEventArgs e)
	{
		Point position = e.GetPosition(base.AssociatedObject);
		object obj = base.AssociatedObject.GetItemAtPoint(position);
		if (obj == null)
		{
			obj = RootTarget ?? base.AssociatedObject.DataContext;
		}
		return obj;
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		object dropTarget = GetDropTarget(e);
		if (ApplicationUtil.CanInsert(base.AssociatedObject.DataContext, dropTarget, e.Data))
		{
			if ((e.AllowedEffects & DragDropEffects.Copy) > DragDropEffects.None && (e.KeyStates & DragDropKeyStates.ControlKey) > DragDropKeyStates.None)
			{
				e.Effects = DragDropEffects.Copy;
			}
			else
			{
				e.Effects = DragDropEffects.Move;
			}
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
		e.Handled = true;
	}

	protected override void OnDrop(DragEventArgs e)
	{
		object dropTarget = GetDropTarget(e);
		if (!ApplicationUtil.CanInsert(base.AssociatedObject.DataContext, dropTarget, e.Data))
		{
			return;
		}
		IDataObject child = e.Data;
		if ((e.AllowedEffects & DragDropEffects.Copy) > DragDropEffects.None && (e.KeyStates & DragDropKeyStates.ControlKey) > DragDropKeyStates.None)
		{
			IContextRegistry exportedValueOrDefault = Composer.Current.Container.GetExportedValueOrDefault<IContextRegistry>();
			if (exportedValueOrDefault != null)
			{
				IInstancingContext activeContext = exportedValueOrDefault.GetActiveContext<IInstancingContext>();
				if (activeContext != null && activeContext.CanCopy())
				{
					object obj = activeContext.Copy();
					child = (obj as IDataObject) ?? new DataObject(obj);
				}
			}
		}
		IStatusService exportedValueOrDefault2 = Composer.Current.Container.GetExportedValueOrDefault<IStatusService>();
		ApplicationUtil.Insert(base.AssociatedObject.DataContext, dropTarget, child, "Drag Drop".Localize(), exportedValueOrDefault2);
		e.Effects = DragDropEffects.None;
		e.Handled = true;
	}
}
