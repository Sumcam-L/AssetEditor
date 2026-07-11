using System.Windows;

namespace Sce.Atf.Wpf.Docking;

public interface IDockLayout
{
	DockPanel Root { get; }

	DockContent HitTest(Point position);

	bool HasChild(IDockContent content);

	bool HasDescendant(IDockContent content);

	void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo);

	void Undock(IDockContent content);

	void Undock(IDockLayout child);

	void Replace(IDockLayout oldLayout, IDockLayout newLayout);

	void Close();

	IDockLayout FindParentLayout(IDockContent content);
}
