using System;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public class DragSelectionEventArgs : EventArgs
{
	public readonly Rect Region;

	public readonly ModifierKeys Modifiers;

	public DragSelectionEventArgs(Rect region, ModifierKeys modifiers)
	{
		Region = region;
		Modifiers = modifiers;
	}
}
