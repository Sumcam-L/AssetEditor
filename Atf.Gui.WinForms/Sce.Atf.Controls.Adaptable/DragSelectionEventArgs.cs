using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class DragSelectionEventArgs : EventArgs
{
	public readonly Rectangle Bounds;

	public readonly Keys Modifiers;

	public DragSelectionEventArgs(Rectangle bounds, Keys modifiers)
	{
		Bounds = bounds;
		Modifiers = modifiers;
	}
}
