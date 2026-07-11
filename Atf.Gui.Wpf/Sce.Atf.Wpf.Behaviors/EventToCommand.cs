using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class EventToCommand : TriggerAction<FrameworkElement>
{
	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is EventToCommand { AssociatedObject: not null } eventToCommand)
		{
			eventToCommand.EnableDisableElement();
		}
	}));

	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		OnCommandChanged(s as EventToCommand, e);
	}));

	public static readonly DependencyProperty MustToggleIsEnabledProperty = DependencyProperty.Register("MustToggleIsEnabled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is EventToCommand { AssociatedObject: not null } eventToCommand)
		{
			eventToCommand.EnableDisableElement();
		}
	}));

	private object m_commandParameterValue;

	private bool? m_mustToggleValue;

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

	public object CommandParameterValue
	{
		get
		{
			return m_commandParameterValue ?? CommandParameter;
		}
		set
		{
			m_commandParameterValue = value;
			EnableDisableElement();
		}
	}

	public bool MustToggleIsEnabled
	{
		get
		{
			return (bool)GetValue(MustToggleIsEnabledProperty);
		}
		set
		{
			SetValue(MustToggleIsEnabledProperty, value);
		}
	}

	public bool MustToggleIsEnabledValue
	{
		get
		{
			return (!m_mustToggleValue.HasValue) ? MustToggleIsEnabled : m_mustToggleValue.Value;
		}
		set
		{
			m_mustToggleValue = value;
			EnableDisableElement();
		}
	}

	public bool PassEventArgsToCommand { get; set; }

	public void Invoke()
	{
		Invoke(null);
	}

	protected override void OnAttached()
	{
		base.OnAttached();
		EnableDisableElement();
	}

	protected override void Invoke(object parameter)
	{
		if (!AssociatedElementIsDisabled())
		{
			ICommand command = GetCommand();
			object obj = CommandParameterValue;
			if (obj == null && PassEventArgsToCommand)
			{
				obj = parameter;
			}
			if (command != null && command.CanExecute(obj))
			{
				command.Execute(obj);
			}
		}
	}

	private FrameworkElement GetAssociatedObject()
	{
		return base.AssociatedObject;
	}

	private ICommand GetCommand()
	{
		return Command;
	}

	private static void OnCommandChanged(EventToCommand element, DependencyPropertyChangedEventArgs e)
	{
		if (element != null)
		{
			if (e.OldValue != null)
			{
				((ICommand)e.OldValue).CanExecuteChanged -= element.OnCommandCanExecuteChanged;
			}
			ICommand command = (ICommand)e.NewValue;
			if (command != null)
			{
				command.CanExecuteChanged += element.OnCommandCanExecuteChanged;
			}
			element.EnableDisableElement();
		}
	}

	private bool AssociatedElementIsDisabled()
	{
		FrameworkElement frameworkElement = GetAssociatedObject();
		return frameworkElement != null && !frameworkElement.IsEnabled;
	}

	private void EnableDisableElement()
	{
		FrameworkElement frameworkElement = GetAssociatedObject();
		if (frameworkElement != null)
		{
			ICommand command = GetCommand();
			if (MustToggleIsEnabledValue && command != null)
			{
				frameworkElement.IsEnabled = command.CanExecute(CommandParameterValue);
			}
		}
	}

	private void OnCommandCanExecuteChanged(object sender, EventArgs e)
	{
		EnableDisableElement();
	}
}
