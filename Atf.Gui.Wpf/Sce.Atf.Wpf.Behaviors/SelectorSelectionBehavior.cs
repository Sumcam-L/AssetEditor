using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class SelectorSelectionBehavior : Behavior<Selector>
{
	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(SelectorSelectionBehavior), new PropertyMetadata(null, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (!(s is SelectorSelectionBehavior { AssociatedObject: null }))
		{
		}
	}));

	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(SelectorSelectionBehavior), new PropertyMetadata(null, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		OnCommandChanged(s as SelectorSelectionBehavior, e);
	}));

	public object CommandParameter
	{
		get
		{
			return GetValue(CommandParameterProperty);
		}
		set
		{
			SetValue(CommandParameterProperty, value);
		}
	}

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

	protected override void OnAttached()
	{
		base.OnAttached();
		base.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
	}

	private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		Invoke(sender, e);
	}

	private void Invoke(object clickedItem, SelectionChangedEventArgs parameter)
	{
		ICommand command = Command;
		if (command != null && command.CanExecute(CommandParameter))
		{
			command.Execute(new EventToCommandArgs(clickedItem, command, CommandParameter, parameter));
		}
	}

	private static void OnCommandChanged(SelectorSelectionBehavior thisBehaviour, DependencyPropertyChangedEventArgs e)
	{
		if (thisBehaviour != null)
		{
			ICommand command = (ICommand)e.OldValue;
			if (command != null)
			{
				command.CanExecuteChanged -= thisBehaviour.OnCommandCanExecuteChanged;
			}
			ICommand command2 = (ICommand)e.NewValue;
			if (command2 != null)
			{
				command2.CanExecuteChanged += thisBehaviour.OnCommandCanExecuteChanged;
			}
		}
	}

	private void OnCommandCanExecuteChanged(object sender, EventArgs e)
	{
	}
}
