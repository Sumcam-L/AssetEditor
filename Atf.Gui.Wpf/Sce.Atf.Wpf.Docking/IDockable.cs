namespace Sce.Atf.Wpf.Docking;

internal interface IDockable
{
	DockTo? DockPreview { get; }

	void DockDragEnter(object sender, DockDragDropEventArgs e);

	void DockDragOver(object sender, DockDragDropEventArgs e);

	void DockDragLeave(object sender, DockDragDropEventArgs e);

	void DockDrop(object sender, DockDragDropEventArgs e);
}
