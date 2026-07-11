using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class SelectionContextDragSourceBehavior : DragSourceBehavior<FrameworkElement>
{
	public static readonly DependencyProperty SelectionContextProperty = DependencyProperty.Register("SelectionContext", typeof(ISelectionContext), typeof(SelectionContextDragSourceBehavior), new PropertyMetadata((object)null));

	public ISelectionContext SelectionContext
	{
		get
		{
			return (ISelectionContext)GetValue(SelectionContextProperty);
		}
		set
		{
			SetValue(SelectionContextProperty, value);
		}
	}

	protected override void BeginDrag(MouseEventArgs e)
	{
		ISelectionContext selectionContext = SelectionContext;
		if (selectionContext == null)
		{
			selectionContext = base.AssociatedObject.DataContext.As<ISelectionContext>();
		}
		if (selectionContext == null)
		{
			return;
		}
		object[] array = selectionContext.Selection.ToArray();
		if (array.Length != 0)
		{
			DragDropEffects dragDropEffects = DragDropEffects.Move;
			IInstancingContext instancingContext = base.AssociatedObject.DataContext.As<IInstancingContext>();
			if (instancingContext != null && instancingContext.CanCopy())
			{
				dragDropEffects |= DragDropEffects.Copy;
			}
			DragDrop.DoDragDrop(base.AssociatedObject, array, dragDropEffects);
		}
	}
}
