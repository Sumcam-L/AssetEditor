using System;

namespace Sce.Atf.Wpf.Models;

public class CloseDialogEventArgs : EventArgs
{
	public bool? DialogResult { get; set; }

	public CloseDialogEventArgs(bool? result)
	{
		DialogResult = result;
	}
}
