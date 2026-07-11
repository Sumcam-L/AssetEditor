using System;

namespace Sce.Atf.Wpf.Models;

public class ShowDialogEventArgs : EventArgs
{
	public bool? DialogResult { get; set; }

	public object ViewModel { get; private set; }

	public ShowDialogEventArgs()
	{
	}

	public ShowDialogEventArgs(object viewModel)
	{
		ViewModel = viewModel;
	}
}
