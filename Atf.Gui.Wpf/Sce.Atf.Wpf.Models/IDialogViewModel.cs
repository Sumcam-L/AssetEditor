using System;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models;

public interface IDialogViewModel
{
	string Title { get; }

	ICommand OkCommand { get; }

	ICommand CancelCommand { get; }

	event EventHandler<CloseDialogEventArgs> CloseDialog;
}
