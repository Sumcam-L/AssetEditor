using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models;

public class DialogViewModelBase : NotifyPropertyChangedBase, IDialogViewModel
{
	private string m_title;

	private static readonly PropertyChangedEventArgs s_titleArgs = ObservableUtil.CreateArgs((DialogViewModelBase x) => x.Title);

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
			OnPropertyChanged(s_titleArgs);
		}
	}

	public ICommand OkCommand { get; private set; }

	public ICommand CancelCommand { get; private set; }

	public event EventHandler<CloseDialogEventArgs> CloseDialog;

	public DialogViewModelBase()
	{
		OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk, isAutomaticRequeryDisabled: false);
		CancelCommand = new DelegateCommand(Cancel, CanCancel, isAutomaticRequeryDisabled: false);
	}

	protected virtual void OnCloseDialog(CloseDialogEventArgs args)
	{
		RaiseCloseDialog(args);
	}

	protected void RaiseCloseDialog(CloseDialogEventArgs args)
	{
		this.CloseDialog.Raise(this, args);
	}

	protected virtual bool CanExecuteOk()
	{
		return true;
	}

	protected virtual bool CanCancel()
	{
		return true;
	}

	private void ExecuteOk()
	{
		OnCloseDialog(new CloseDialogEventArgs(true));
	}

	private void Cancel()
	{
		OnCloseDialog(new CloseDialogEventArgs(false));
	}
}
public class DialogViewModelBase<TDialog> : DialogViewModelBase where TDialog : Window, new()
{
	public bool? DialogResult { get; protected set; }

	public bool? ShowDialog()
	{
		OnShowDialog();
		DialogResult = DialogUtils.ShowDialogWithViewModel<TDialog>(this);
		return DialogResult;
	}

	public void Show()
	{
		OnShow();
		DialogUtils.ShowWithViewModel<TDialog>(this);
	}

	public void Close()
	{
		CloseDialogEventArgs e = new CloseDialogEventArgs(false);
		OnCloseDialog(e);
		DialogResult = e.DialogResult;
	}

	protected virtual void OnShowDialog()
	{
	}

	protected virtual void OnShow()
	{
	}
}
