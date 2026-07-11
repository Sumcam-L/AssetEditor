using System.Windows;
using System.Windows.Input;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Models;

public class ConfirmationDialogViewModel : DialogViewModelBase<ConfirmationDialog>
{
	public ICommand NoCommand { get; private set; }

	public bool HideCancelButton { get; set; }

	public string Message { get; set; }

	public string YesButtonText { get; set; }

	public string NoButtonText { get; set; }

	public string CancelButtonText { get; set; }

	public MessageBoxResult Result { get; private set; }

	public ConfirmationDialogViewModel(string title, string message)
	{
		base.Title = title;
		Message = message;
		YesButtonText = "Yes".Localize();
		NoButtonText = "No".Localize();
		CancelButtonText = "Cancel".Localize();
		NoCommand = new DelegateCommand(ExecuteNo, CanExecuteNo, isAutomaticRequeryDisabled: false);
	}

	protected override void OnCloseDialog(CloseDialogEventArgs args)
	{
		base.OnCloseDialog(args);
		if (Result != MessageBoxResult.No)
		{
			Result = ((args.DialogResult == true) ? MessageBoxResult.Yes : MessageBoxResult.Cancel);
		}
	}

	private bool CanExecuteNo()
	{
		return true;
	}

	private void ExecuteNo()
	{
		Result = MessageBoxResult.No;
		OnCloseDialog(new CloseDialogEventArgs(true));
	}
}
