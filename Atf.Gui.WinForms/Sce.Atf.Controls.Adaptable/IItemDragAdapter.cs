namespace Sce.Atf.Controls.Adaptable;

public interface IItemDragAdapter
{
	void BeginDrag(ControlAdapter initiator);

	void EndingDrag();

	void EndDrag();
}
