using System;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf;

public class CommandReference : Freezable, ICommand, IDisposable
{
	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandReference), new PropertyMetadata(OnCommandChanged));

	public ICommand Command
	{
		get
		{
			return (ICommand)GetValue(CommandProperty);
		}
		set
		{
			SetValue(CommandProperty, value);
		}
	}

	public event EventHandler CanExecuteChanged
	{
		add
		{
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			CommandManager.RequerySuggested -= value;
		}
	}

	public bool CanExecute(object parameter)
	{
		return Command != null && Command.CanExecute(parameter);
	}

	public void Execute(object parameter)
	{
		Command.Execute(parameter);
	}

	private void RaiseCanExecuteChanged(object sender, EventArgs e)
	{
	}

	private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CommandReference commandReference = d as CommandReference;
		ICommand command = e.OldValue as ICommand;
		ICommand command2 = e.NewValue as ICommand;
		if (command != null)
		{
			command.CanExecuteChanged -= delegate(object s, EventArgs args)
			{
				commandReference.RaiseCanExecuteChanged(s, args);
			};
		}
		if (command2 != null)
		{
			command2.CanExecuteChanged += delegate(object s, EventArgs args)
			{
				commandReference.RaiseCanExecuteChanged(s, args);
			};
		}
	}

	protected override Freezable CreateInstanceCore()
	{
		throw new NotSupportedException();
	}

	public void Dispose()
	{
	}
}
