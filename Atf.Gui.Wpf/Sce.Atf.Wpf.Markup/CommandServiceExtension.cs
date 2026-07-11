using System;
using System.Windows.Input;
using System.Windows.Markup;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Markup;

[ContentProperty("Command")]
public class CommandServiceExtension : MarkupExtension
{
	public object Command { get; set; }

	public CommandServiceExtension()
	{
	}

	public CommandServiceExtension(object command)
	{
		Command = command;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		ICommand result = null;
		IComposer current = Composer.Current;
		if (current != null && Command != null)
		{
			CommandService exportedValue = current.Container.GetExportedValue<CommandService>();
			if (exportedValue != null)
			{
				result = exportedValue.GetCommand(Command);
			}
		}
		return result;
	}
}
