using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors;

internal sealed class EventHooker
{
	public DependencyObject ObjectWithAttachedCommand { get; set; }

	public Delegate GetNewEventHandlerToRunCommand(EventInfo eventInfo)
	{
		Delegate obj = null;
		if (eventInfo == null)
		{
			throw new ArgumentNullException("eventInfo");
		}
		if (eventInfo.EventHandlerType == null)
		{
			throw new ArgumentException("EventHandlerType is null");
		}
		if ((object)obj == null)
		{
			obj = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, GetType().GetMethod("OnEventRaised", BindingFlags.Instance | BindingFlags.NonPublic));
		}
		return obj;
	}

	private void OnEventRaised(object sender, EventArgs e)
	{
		DependencyObject dependencyObject = (DependencyObject)sender;
		ICommand command = (ICommand)dependencyObject.GetValue(CommandBehavior.CommandProperty);
		if (command != null)
		{
			object value = dependencyObject.GetValue(CommandBehavior.CommandParameterProperty);
			if (command.CanExecute(value))
			{
				command.Execute(value);
			}
		}
		else
		{
			((Action)dependencyObject.GetValue(CommandBehavior.ActionProperty))?.Invoke();
		}
	}
}
