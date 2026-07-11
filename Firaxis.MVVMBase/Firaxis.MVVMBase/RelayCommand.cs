using System;
using System.Threading;
using System.Windows.Input;

namespace Firaxis.MVVMBase;

public class RelayCommand : ICommand
{
	private readonly WeakAction _execute;

	private readonly WeakFunc<bool> _canExecute;

	private EventHandler _requerySuggestedLocal;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (_canExecute != null)
			{
				EventHandler eventHandler = _requerySuggestedLocal;
				EventHandler eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
					eventHandler = Interlocked.CompareExchange(ref _requerySuggestedLocal, value2, eventHandler2);
				}
				while (eventHandler != eventHandler2);
				CommandManager.RequerySuggested += value;
			}
		}
		remove
		{
			if (_canExecute != null)
			{
				EventHandler eventHandler = _requerySuggestedLocal;
				EventHandler eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
					eventHandler = Interlocked.CompareExchange(ref _requerySuggestedLocal, value2, eventHandler2);
				}
				while (eventHandler != eventHandler2);
				CommandManager.RequerySuggested -= value;
			}
		}
	}

	public RelayCommand(Action execute)
		: this(execute, null)
	{
	}

	public RelayCommand(Action execute, Func<bool> canExecute)
	{
		if (execute == null)
		{
			throw new ArgumentNullException("execute");
		}
		_execute = new WeakAction(execute);
		if (canExecute != null)
		{
			_canExecute = new WeakFunc<bool>(canExecute);
		}
	}

	public void RaiseCanExecuteChanged()
	{
		CommandManager.InvalidateRequerySuggested();
	}

	public bool CanExecute(object parameter)
	{
		bool result;
		return _canExecute == null || (_canExecute.Execute(parameter, out result) && result);
	}

	public void Execute(object parameter)
	{
		if (CanExecute(parameter) && _execute != null)
		{
			_execute.Execute(parameter);
		}
	}
}
public class RelayCommand<T> : ICommand
{
	private readonly WeakAction<T> _execute;

	private readonly WeakFunc<T, bool> _canExecute;

	private EventHandler _requerySuggestedLocal;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (_canExecute != null)
			{
				EventHandler eventHandler = _requerySuggestedLocal;
				EventHandler eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
					eventHandler = Interlocked.CompareExchange(ref _requerySuggestedLocal, value2, eventHandler2);
				}
				while (eventHandler != eventHandler2);
				CommandManager.RequerySuggested += value;
			}
		}
		remove
		{
			if (_canExecute != null)
			{
				EventHandler eventHandler = _requerySuggestedLocal;
				EventHandler eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
					eventHandler = Interlocked.CompareExchange(ref _requerySuggestedLocal, value2, eventHandler2);
				}
				while (eventHandler != eventHandler2);
				CommandManager.RequerySuggested -= value;
			}
		}
	}

	public RelayCommand(Action<T> execute)
		: this(execute, (Func<T, bool>)null)
	{
	}

	public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
	{
		if (execute == null)
		{
			throw new ArgumentNullException("execute");
		}
		_execute = new WeakAction<T>(execute);
		if (canExecute != null)
		{
			_canExecute = new WeakFunc<T, bool>(canExecute);
		}
	}

	public void RaiseCanExecuteChanged()
	{
		CommandManager.InvalidateRequerySuggested();
	}

	public bool CanExecute(T parameter)
	{
		bool result;
		return _canExecute == null || (_canExecute.Execute(parameter, out result) && result);
	}

	bool ICommand.CanExecute(object parameter)
	{
		if (!(parameter is T))
		{
			return false;
		}
		bool result;
		return _canExecute == null || (_canExecute.Execute((T)parameter, out result) && result);
	}

	public void Execute(T parameter)
	{
		if (CanExecute(parameter) && _execute != null)
		{
			_execute.Execute(parameter);
		}
	}

	void ICommand.Execute(object parameter)
	{
		if (parameter is T parameter2 && CanExecute(parameter2) && _execute != null)
		{
			_execute.Execute(parameter2);
		}
	}
}
