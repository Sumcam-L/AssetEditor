using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sce.Atf.Wpf;

public class DelegateCommand : ICommand
{
	private readonly Action _executeMethod = null;

	private readonly Func<bool> _canExecuteMethod = null;

	private bool _isAutomaticRequeryDisabled = false;

	private List<WeakReference> _canExecuteChangedHandlers;

	public bool IsAutomaticRequeryDisabled
	{
		get
		{
			return _isAutomaticRequeryDisabled;
		}
		set
		{
			if (_isAutomaticRequeryDisabled != value)
			{
				if (value)
				{
					CommandManagerHelper.RemoveHandlersFromRequerySuggested(_canExecuteChangedHandlers);
				}
				else
				{
					CommandManagerHelper.AddHandlersToRequerySuggested(_canExecuteChangedHandlers);
				}
				_isAutomaticRequeryDisabled = value;
			}
		}
	}

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (!_isAutomaticRequeryDisabled)
			{
				CommandManager.RequerySuggested += value;
			}
			CommandManagerHelper.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
		}
		remove
		{
			if (!_isAutomaticRequeryDisabled)
			{
				CommandManager.RequerySuggested -= value;
			}
			CommandManagerHelper.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
		}
	}

	public DelegateCommand(Action executeMethod)
		: this(executeMethod, null, isAutomaticRequeryDisabled: false)
	{
	}

	public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
		: this(executeMethod, canExecuteMethod, isAutomaticRequeryDisabled: false)
	{
	}

	public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
	{
		Requires.NotNull(executeMethod, "executeMethod");
		_executeMethod = executeMethod;
		_canExecuteMethod = canExecuteMethod;
		_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
	}

	public bool CanExecute()
	{
		if (_canExecuteMethod != null)
		{
			return _canExecuteMethod();
		}
		return true;
	}

	public void Execute()
	{
		if (_executeMethod != null)
		{
			_executeMethod();
		}
	}

	public void RaiseCanExecuteChanged()
	{
		OnCanExecuteChanged();
	}

	protected virtual void OnCanExecuteChanged()
	{
		CommandManagerHelper.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
	}

	bool ICommand.CanExecute(object parameter)
	{
		return CanExecute();
	}

	void ICommand.Execute(object parameter)
	{
		Execute();
	}
}
public class DelegateCommand<T> : ICommand
{
	private readonly Action<T> m_executeMethod = null;

	private readonly Func<T, bool> m_canExecuteMethod = null;

	private bool m_isAutomaticRequeryDisabled = false;

	private List<WeakReference> m_canExecuteChangedHandlers;

	public bool IsAutomaticRequeryDisabled
	{
		get
		{
			return m_isAutomaticRequeryDisabled;
		}
		set
		{
			if (m_isAutomaticRequeryDisabled != value)
			{
				if (value)
				{
					CommandManagerHelper.RemoveHandlersFromRequerySuggested(m_canExecuteChangedHandlers);
				}
				else
				{
					CommandManagerHelper.AddHandlersToRequerySuggested(m_canExecuteChangedHandlers);
				}
				m_isAutomaticRequeryDisabled = value;
			}
		}
	}

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (!m_isAutomaticRequeryDisabled)
			{
				CommandManager.RequerySuggested += value;
			}
			CommandManagerHelper.AddWeakReferenceHandler(ref m_canExecuteChangedHandlers, value, 2);
		}
		remove
		{
			if (!m_isAutomaticRequeryDisabled)
			{
				CommandManager.RequerySuggested -= value;
			}
			CommandManagerHelper.RemoveWeakReferenceHandler(m_canExecuteChangedHandlers, value);
		}
	}

	public DelegateCommand(Action<T> executeMethod)
		: this(executeMethod, (Func<T, bool>)null, false)
	{
	}

	public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
		: this(executeMethod, canExecuteMethod, false)
	{
	}

	public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
	{
		Requires.NotNull(executeMethod, "executeMethod");
		m_executeMethod = executeMethod;
		m_canExecuteMethod = canExecuteMethod;
		m_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
	}

	public bool CanExecute(T parameter)
	{
		if (m_canExecuteMethod != null)
		{
			return m_canExecuteMethod(parameter);
		}
		return true;
	}

	public void Execute(T parameter)
	{
		if (m_executeMethod != null)
		{
			m_executeMethod(parameter);
		}
	}

	public void RaiseCanExecuteChanged()
	{
		OnCanExecuteChanged();
	}

	protected virtual void OnCanExecuteChanged()
	{
		CommandManagerHelper.CallWeakReferenceHandlers(m_canExecuteChangedHandlers);
	}

	bool ICommand.CanExecute(object parameter)
	{
		if (parameter == null && typeof(T).IsValueType)
		{
			return m_canExecuteMethod == null;
		}
		return CanExecute((T)parameter);
	}

	void ICommand.Execute(object parameter)
	{
		Execute((T)parameter);
	}
}
public class DelegateCommand<T1, T2> : ICommand
{
	private readonly Action<T2> m_executeMethod = null;

	private readonly Func<T1, bool> m_canExecuteMethod = null;

	private bool m_isAutomaticRequeryDisabled = false;

	private List<WeakReference> m_canExecuteChangedHandlers;

	public bool IsAutomaticRequeryDisabled
	{
		get
		{
			return m_isAutomaticRequeryDisabled;
		}
		set
		{
			if (m_isAutomaticRequeryDisabled != value)
			{
				if (value)
				{
					CommandManagerHelper.RemoveHandlersFromRequerySuggested(m_canExecuteChangedHandlers);
				}
				else
				{
					CommandManagerHelper.AddHandlersToRequerySuggested(m_canExecuteChangedHandlers);
				}
				m_isAutomaticRequeryDisabled = value;
			}
		}
	}

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (!m_isAutomaticRequeryDisabled)
			{
				CommandManager.RequerySuggested += value;
			}
			CommandManagerHelper.AddWeakReferenceHandler(ref m_canExecuteChangedHandlers, value, 2);
		}
		remove
		{
			if (!m_isAutomaticRequeryDisabled)
			{
				CommandManager.RequerySuggested -= value;
			}
			CommandManagerHelper.RemoveWeakReferenceHandler(m_canExecuteChangedHandlers, value);
		}
	}

	public DelegateCommand(Action<T2> m_executeMethod)
		: this(m_executeMethod, (Func<T1, bool>)null, false)
	{
	}

	public DelegateCommand(Action<T2> m_executeMethod, Func<T1, bool> m_canExecuteMethod)
		: this(m_executeMethod, m_canExecuteMethod, false)
	{
	}

	public DelegateCommand(Action<T2> executeMethod, Func<T1, bool> canExecuteMethod, bool isAutomaticRequeryDisabled)
	{
		Requires.NotNull(executeMethod, "executeMethod");
		m_executeMethod = executeMethod;
		m_canExecuteMethod = canExecuteMethod;
		m_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
	}

	public bool CanExecute(T1 parameter)
	{
		if (m_canExecuteMethod != null)
		{
			return m_canExecuteMethod(parameter);
		}
		return true;
	}

	public void Execute(T2 parameter)
	{
		if (m_executeMethod != null)
		{
			m_executeMethod(parameter);
		}
	}

	public void RaiseCanExecuteChanged()
	{
		OnCanExecuteChanged();
	}

	protected virtual void OnCanExecuteChanged()
	{
		CommandManagerHelper.CallWeakReferenceHandlers(m_canExecuteChangedHandlers);
	}

	bool ICommand.CanExecute(object parameter)
	{
		if (parameter == null && typeof(T1).IsValueType)
		{
			return m_canExecuteMethod == null;
		}
		return CanExecute((T1)parameter);
	}

	void ICommand.Execute(object parameter)
	{
		Execute((T2)parameter);
	}
}
