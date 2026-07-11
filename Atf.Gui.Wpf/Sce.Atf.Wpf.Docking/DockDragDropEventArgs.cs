using System;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Docking;

public class DockDragDropEventArgs : EventArgs
{
	internal IDockContent Content { get; private set; }

	internal MouseEventArgs MouseEventArgs { get; private set; }

	internal DockDragDropEventArgs(IDockContent content, MouseEventArgs mouseArgs)
	{
		Content = content;
		MouseEventArgs = mouseArgs;
	}
}
