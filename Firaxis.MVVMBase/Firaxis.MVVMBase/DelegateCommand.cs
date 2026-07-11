using System;
using System.Windows.Input;

namespace Firaxis.MVVMBase;

public class DelegateCommand : ICommand
{
	private readonly Predicate<object> _canExecute;

	private readonly Action<object> _execute;

	public event EventHandler CanExecuteChanged;

	public DelegateCommand(Action<object> execute)
		: this(execute, null)
	{
	}

	public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
	{
		_execute = execute;
		_canExecute = canExecute;
	}

	public DelegateCommand(ICommand command)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command cannot be null", "command");
		}
		_execute = command.Execute;
		_canExecute = command.CanExecute;
	}

	public bool CanExecute(object parameter)
	{
		if (_canExecute == null)
		{
			return true;
		}
		return _canExecute(parameter);
	}

	public void Execute(object parameter)
	{
		_execute(parameter);
	}

	public void RaiseCanExecuteChanged()
	{
		if (this.CanExecuteChanged != null)
		{
			this.CanExecuteChanged(this, EventArgs.Empty);
		}
	}
}
