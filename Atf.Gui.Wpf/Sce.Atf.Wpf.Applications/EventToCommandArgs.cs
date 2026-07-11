using System;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Applications;

public class EventToCommandArgs
{
	public object Sender { get; private set; }

	public ICommand CommandRan { get; private set; }

	public object CommandParameter { get; private set; }

	public EventArgs EventArgs { get; private set; }

	public EventToCommandArgs(object sender, ICommand commandRan, object commandParameter, EventArgs eventArgs)
	{
		Sender = sender;
		CommandRan = commandRan;
		CommandParameter = commandParameter;
		EventArgs = eventArgs;
	}
}
