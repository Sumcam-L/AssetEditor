using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public interface ISplitterDragSource : IDragSource
{
	bool IsVertical { get; }

	Rectangle DragLimitBounds { get; }

	void BeginDrag(Rectangle rectSplitter);

	void EndDrag();

	void MoveSplitter(int offset);
}
