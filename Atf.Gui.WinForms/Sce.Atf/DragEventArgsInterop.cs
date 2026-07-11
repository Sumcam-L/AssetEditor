using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

public static class DragEventArgsInterop
{
	public static Sce.Atf.Input.DragEventArgs ToAtf(System.Windows.Forms.DragEventArgs arg)
	{
		return new Sce.Atf.Input.DragEventArgs(arg.Data, arg.KeyState, arg.X, arg.Y, (Sce.Atf.Input.DragDropEffects)arg.AllowedEffect, (Sce.Atf.Input.DragDropEffects)arg.Effect);
	}

	public static System.Windows.Forms.DragEventArgs ToWf(Sce.Atf.Input.DragEventArgs arg)
	{
		return new System.Windows.Forms.DragEventArgs((IDataObject)arg.Data, arg.KeyState, arg.X, arg.Y, (System.Windows.Forms.DragDropEffects)arg.AllowedEffect, (System.Windows.Forms.DragDropEffects)arg.Effect);
	}
}
