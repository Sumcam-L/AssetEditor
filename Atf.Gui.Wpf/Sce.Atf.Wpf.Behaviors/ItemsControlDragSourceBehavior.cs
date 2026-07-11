using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Behaviors;

public class ItemsControlDragSourceBehavior : DragSourceBehavior<ItemsControl>
{
	protected override void BeginDrag(MouseEventArgs e)
	{
		Point position = e.GetPosition(base.AssociatedObject);
		object itemAtPoint = base.AssociatedObject.GetItemAtPoint(position);
		if (itemAtPoint != null)
		{
			object[] array = new object[1] { itemAtPoint };
			DragDropEffects dragDropEffects = DragDropEffects.Move;
			IInstancingContext instancingContext = base.AssociatedObject.DataContext.As<IInstancingContext>();
			if (instancingContext != null && instancingContext.CanCopy())
			{
				dragDropEffects |= DragDropEffects.Copy;
			}
			IDragDropConverter dragDropConverter = base.AssociatedObject.DataContext.As<IDragDropConverter>();
			if (dragDropConverter != null)
			{
				array = dragDropConverter.Convert(array).ToArray();
			}
			DragDrop.DoDragDrop(base.AssociatedObject, array, dragDropEffects);
		}
	}
}
