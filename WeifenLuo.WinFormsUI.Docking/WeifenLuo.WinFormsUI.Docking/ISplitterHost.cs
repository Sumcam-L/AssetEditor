namespace WeifenLuo.WinFormsUI.Docking;

public interface ISplitterHost : ISplitterDragSource, IDragSource
{
	DockPanel DockPanel { get; }

	DockState DockState { get; }

	bool IsDockWindow { get; }
}
