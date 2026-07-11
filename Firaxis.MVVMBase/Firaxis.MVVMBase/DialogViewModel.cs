using System;
using System.Windows;
using System.Windows.Input;

namespace Firaxis.MVVMBase;

public class DialogViewModel : Notifier
{
	private DelegateCommand _okCommand;

	private DelegateCommand _cancelCommand;

	public ICommand OKCommand
	{
		get
		{
			if (_okCommand == null)
			{
				_okCommand = new DelegateCommand(ExecuteOKCommand, CanExecuteOKCommand);
			}
			return _okCommand;
		}
	}

	public ICommand CancelCommand
	{
		get
		{
			if (_cancelCommand == null)
			{
				_cancelCommand = new DelegateCommand(ExecuteCancelCommand, CanExecuteCancelCommand);
			}
			return _cancelCommand;
		}
	}

	public bool UserPressedOK { get; private set; }

	public bool CloseRequested { get; private set; }

	protected Window RegisteredWindow { get; set; }

	public event EventHandler RequestCloseEvent;

	public event EventHandler ClosingEvent;

	protected virtual void OnRequestCloseEvent()
	{
		this.RequestCloseEvent?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnClosingEvent()
	{
		this.ClosingEvent?.Invoke(this, EventArgs.Empty);
	}

	public DialogViewModel()
		: this(null)
	{
	}

	public DialogViewModel(Window window)
	{
		RegisteredWindow = window;
		if (RegisteredWindow != null && RegisteredWindow.DataContext == null)
		{
			RegisteredWindow.DataContext = this;
		}
		UserPressedOK = false;
		CloseRequested = false;
	}

	public void RegisterWindow(Window window)
	{
		RegisteredWindow = window;
		if (RegisteredWindow != null && RegisteredWindow.DataContext == null)
		{
			RegisteredWindow.DataContext = this;
		}
	}

	protected virtual void ExecuteOKCommand(object context)
	{
		UserPressedOK = true;
		Close();
	}

	protected virtual bool CanExecuteOKCommand(object context)
	{
		return true;
	}

	protected virtual void ExecuteCancelCommand(object context)
	{
		UserPressedOK = false;
		Close();
	}

	protected virtual bool CanExecuteCancelCommand(object context)
	{
		return true;
	}

	protected virtual void Close()
	{
		CloseRequested = true;
		if (RegisteredWindow != null)
		{
			RegisteredWindow.Dispatcher.Invoke(delegate
			{
				RegisteredWindow.Close();
			});
		}
		else
		{
			OnRequestCloseEvent();
		}
	}

	protected virtual void RefreshCommands()
	{
		if (_okCommand != null)
		{
			_okCommand.RaiseCanExecuteChanged();
		}
		if (_cancelCommand != null)
		{
			_cancelCommand.RaiseCanExecuteChanged();
		}
	}

	public void TriggerClosingEvent()
	{
		OnClosingEvent();
	}

	protected void ResetUserPressedOK()
	{
		UserPressedOK = false;
		CloseRequested = false;
	}
}
