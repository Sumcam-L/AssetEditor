using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

public static class CommandBehavior
{
	public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(CommandBehavior), new FrameworkPropertyMetadata((object)null));

	public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(CommandBehavior));

	public static readonly DependencyProperty ActionProperty = DependencyProperty.RegisterAttached("Action", typeof(Action), typeof(CommandBehavior));

	public static readonly DependencyProperty RoutedEventNameProperty = DependencyProperty.RegisterAttached("RoutedEventName", typeof(string), typeof(CommandBehavior), new FrameworkPropertyMetadata(string.Empty, OnRoutedEventNameChanged));

	public static ICommand GetCommand(DependencyObject d)
	{
		return (ICommand)d.GetValue(CommandProperty);
	}

	public static void SetCommand(DependencyObject d, ICommand value)
	{
		d.SetValue(CommandProperty, value);
	}

	public static object GetCommandParameter(DependencyObject obj)
	{
		return obj.GetValue(CommandParameterProperty);
	}

	public static void SetCommandParameter(DependencyObject obj, object value)
	{
		obj.SetValue(CommandParameterProperty, value);
	}

	public static Action GetAction(DependencyObject obj)
	{
		return (Action)obj.GetValue(ActionProperty);
	}

	public static void SetAction(DependencyObject obj, Action value)
	{
		obj.SetValue(ActionProperty, value);
	}

	public static string GetRoutedEventName(DependencyObject d)
	{
		return (string)d.GetValue(RoutedEventNameProperty);
	}

	public static void SetRoutedEventName(DependencyObject d, string value)
	{
		d.SetValue(RoutedEventNameProperty, value);
	}

	private static void OnRoutedEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		string text = (string)e.NewValue;
		if (!string.IsNullOrEmpty(text))
		{
			EventHooker eventHooker = new EventHooker();
			eventHooker.ObjectWithAttachedCommand = d;
			EventInfo eventInfo = d.GetType().GetEvent(text, BindingFlags.Instance | BindingFlags.Public);
			if (eventInfo != null)
			{
				eventInfo.AddEventHandler(d, eventHooker.GetNewEventHandlerToRunCommand(eventInfo));
			}
		}
	}
}
