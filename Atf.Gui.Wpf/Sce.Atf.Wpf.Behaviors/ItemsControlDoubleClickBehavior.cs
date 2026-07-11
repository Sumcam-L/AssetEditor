using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class ItemsControlDoubleClickBehavior : Behavior<ItemsControl>
{
	public static readonly DependencyProperty AutoEnableProperty = DependencyProperty.Register("AutoEnable", typeof(bool), typeof(ItemsControlDoubleClickBehavior), new PropertyMetadata(false, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		OnCommandChanged(s as ItemsControlDoubleClickBehavior, e);
	}));

	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ItemsControlDoubleClickBehavior), new PropertyMetadata(null, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is ItemsControlDoubleClickBehavior { AssociatedObject: not null } itemsControlDoubleClickBehavior)
		{
			itemsControlDoubleClickBehavior.EnableDisableElement();
		}
	}));

	public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ItemsControlDoubleClickBehavior), new PropertyMetadata(null, delegate(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		OnCommandChanged(s as ItemsControlDoubleClickBehavior, e);
	}));

	public bool AutoEnable
	{
		get
		{
			return (bool)GetValue(AutoEnableProperty);
		}
		set
		{
			SetValue(AutoEnableProperty, value);
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
		base.AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
		EnableDisableElement();
	}

	protected override void OnDetaching()
	{
		base.OnDetaching();
		base.AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;
	}

	private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		ItemsControl itemsControl = sender as ItemsControl;
		DependencyObject dependencyObject = e.OriginalSource as DependencyObject;
		if (itemsControl == null || dependencyObject == null)
		{
			return;
		}
		DependencyObject dependencyObject2 = ItemsControl.ContainerFromElement(sender as ItemsControl, e.OriginalSource as DependencyObject);
		if (dependencyObject2 != null && dependencyObject2 != DependencyProperty.UnsetValue)
		{
			object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(dependencyObject2);
			if (obj != null)
			{
				Invoke(obj, e);
			}
		}
	}

	private static void OnCommandChanged(ItemsControlDoubleClickBehavior thisBehaviour, DependencyPropertyChangedEventArgs e)
	{
		if (thisBehaviour != null)
		{
			if (e.OldValue != null)
			{
				((ICommand)e.OldValue).CanExecuteChanged -= thisBehaviour.OnCommandCanExecuteChanged;
			}
			ICommand command = (ICommand)e.NewValue;
			if (command != null)
			{
				command.CanExecuteChanged += thisBehaviour.OnCommandCanExecuteChanged;
			}
			thisBehaviour.EnableDisableElement();
		}
	}

	private bool IsAssociatedElementDisabled()
	{
		return base.AssociatedObject != null && !base.AssociatedObject.IsEnabled;
	}

	private void EnableDisableElement()
	{
		if (base.AssociatedObject != null && Command != null && AutoEnable)
		{
			base.AssociatedObject.IsEnabled = Command.CanExecute(CommandParameter);
		}
	}

	private void OnCommandCanExecuteChanged(object sender, EventArgs e)
	{
		EnableDisableElement();
	}

	protected void Invoke(object clickedItem, MouseButtonEventArgs parameter)
	{
		if (!IsAssociatedElementDisabled())
		{
			ICommand command = Command;
			if (command != null && command.CanExecute(CommandParameter))
			{
				command.Execute(new EventToCommandArgs(clickedItem, command, CommandParameter, parameter));
			}
		}
	}
}
